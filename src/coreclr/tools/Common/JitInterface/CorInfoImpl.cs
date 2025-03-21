// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

#if SUPPORT_JIT
using Internal.Runtime.CompilerServices;
#endif

using Internal.IL;
using Internal.TypeSystem;
using Internal.TypeSystem.Ecma;
using Internal.TypeSystem.Interop;
using Internal.CorConstants;
using Internal.Pgo;

using ILCompiler;
using ILCompiler.DependencyAnalysis;
using Internal.IL.Stubs;

#if READYTORUN
using System.Reflection.Metadata.Ecma335;
using ILCompiler.DependencyAnalysis.ReadyToRun;
#endif

namespace Internal.JitInterface
{
    internal sealed unsafe partial class CorInfoImpl
    {
        //
        // Global initialization and state
        //
        private enum ImageFileMachine
        {
            I386 = 0x014c,
            IA64 = 0x0200,
            AMD64 = 0x8664,
            ARM = 0x01c4,
            ARM64 = 0xaa64,
        }

        internal const string JitLibrary = "clrjitilc";

#if SUPPORT_JIT
        private const string JitSupportLibrary = "*";
#else
        internal const string JitSupportLibrary = "jitinterface";
#endif

        private IntPtr _jit;

        private IntPtr _unmanagedCallbacks; // array of pointers to JIT-EE interface callbacks

        private ExceptionDispatchInfo _lastException;

        private struct PgoInstrumentationResults
        {
            public PgoInstrumentationSchema* pSchema;
            public uint countSchemaItems;
            public byte* pInstrumentationData;
            public HRESULT hr;
        }
        Dictionary<MethodDesc, PgoInstrumentationResults> _pgoResults = new Dictionary<MethodDesc, PgoInstrumentationResults>();

        [DllImport(JitLibrary)]
        private extern static IntPtr jitStartup(IntPtr host);

        private static class JitPointerAccessor
        {
            [DllImport(JitLibrary)]
            private extern static IntPtr getJit();

            [DllImport(JitSupportLibrary)]
            private extern static CorJitResult JitProcessShutdownWork(IntPtr jit);

            static JitPointerAccessor()
            {
                s_jit = getJit();

                if (s_jit != IntPtr.Zero)
                {
                    AppDomain.CurrentDomain.ProcessExit += (_, _) => JitProcessShutdownWork(s_jit);
                    AppDomain.CurrentDomain.UnhandledException += (_, _) => JitProcessShutdownWork(s_jit);
                }
            }

            public static IntPtr Get()
            {
                return s_jit;
            }

            private static readonly IntPtr s_jit;
        }

        private struct LikelyClassRecord
        {
            public IntPtr clsHandle;
            public uint likelihood;

            public LikelyClassRecord(IntPtr clsHandle, uint likelihood)
            {
                this.clsHandle = clsHandle;
                this.likelihood = likelihood;
            }
        }

        [DllImport(JitLibrary)]
        private extern static uint getLikelyClasses(LikelyClassRecord* pLikelyClasses, uint maxLikelyClasses, PgoInstrumentationSchema* schema, uint countSchemaItems, byte*pInstrumentationData, int ilOffset);

        [DllImport(JitSupportLibrary)]
        private extern static IntPtr GetJitHost(IntPtr configProvider);

        //
        // Per-method initialization and state
        //
        private static CorInfoImpl GetThis(IntPtr thisHandle)
        {
            CorInfoImpl _this = Unsafe.Read<CorInfoImpl>((void*)thisHandle);
            Debug.Assert(_this is CorInfoImpl);
            return _this;
        }

        [DllImport(JitSupportLibrary)]
        private extern static CorJitResult JitCompileMethod(out IntPtr exception,
            IntPtr jit, IntPtr thisHandle, IntPtr callbacks,
            ref CORINFO_METHOD_INFO info, uint flags, out IntPtr nativeEntry, out uint codeSize);

        [DllImport(JitSupportLibrary)]
        private extern static uint GetMaxIntrinsicSIMDVectorLength(IntPtr jit, CORJIT_FLAGS* flags);

        [DllImport(JitSupportLibrary)]
        private extern static IntPtr AllocException([MarshalAs(UnmanagedType.LPWStr)]string message, int messageLength);

        [DllImport(JitSupportLibrary)]
        private extern static void JitSetOs(IntPtr jit, CORINFO_OS os);

        private IntPtr AllocException(Exception ex)
        {
            _lastException = ExceptionDispatchInfo.Capture(ex);

            string exString = ex.ToString();
            IntPtr nativeException = AllocException(exString, exString.Length);
            if (_nativeExceptions == null)
            {
                _nativeExceptions = new List<IntPtr>();
            }
            _nativeExceptions.Add(nativeException);
            return nativeException;
        }

        [DllImport(JitSupportLibrary)]
        private extern static void FreeException(IntPtr obj);

        [DllImport(JitSupportLibrary)]
        private extern static char* GetExceptionMessage(IntPtr obj);

        public static void Startup(CORINFO_OS os)
        {
            jitStartup(GetJitHost(JitConfigProvider.Instance.UnmanagedInstance));
            JitSetOs(JitPointerAccessor.Get(), os);
        }

        public CorInfoImpl()
        {
            _jit = JitPointerAccessor.Get();
            if (_jit == IntPtr.Zero)
            {
                throw new IOException("Failed to initialize JIT");
            }

            _unmanagedCallbacks = GetUnmanagedCallbacks();
        }

        public TextWriter Log
        {
            get
            {
                return _compilation.Logger.Writer;
            }
        }

        private CORINFO_MODULE_STRUCT_* _methodScope; // Needed to resolve CORINFO_EH_CLAUSE tokens

        public static IEnumerable<PgoSchemaElem> ConvertTypeHandleHistogramsToCompactTypeHistogramFormat(PgoSchemaElem[] pgoData, CompilationModuleGroup compilationModuleGroup)
        {
            bool hasTypeHistogram = false;
            foreach (var elem in pgoData)
            {
                if (elem.InstrumentationKind == PgoInstrumentationKind.HandleHistogramTypes)
                {
                    // found histogram
                    hasTypeHistogram = true;
                    break;
                }
            }
            if (!hasTypeHistogram)
            {
                foreach (var elem in pgoData)
                {
                    yield return elem;
                }
            }
            else
            {
                int currentObjectIndex = 0x1000000; // This needs to be a somewhat large non-zero number, so that the jit does not confuse it with NULL, or any other special value.
                Dictionary<object, IntPtr> objectToHandle = new Dictionary<object, IntPtr>();
                Dictionary<IntPtr, object> handleToObject = new Dictionary<IntPtr, object>();

                ComputeJitPgoInstrumentationSchema(LocalObjectToHandle, pgoData, out var nativeSchema, out var instrumentationData);

                for (int i = 0; i < pgoData.Length; i++)
                {
                    if ((i + 1 < pgoData.Length) &&
                        (pgoData[i].InstrumentationKind == PgoInstrumentationKind.HandleHistogramIntCount ||
                         pgoData[i].InstrumentationKind == PgoInstrumentationKind.HandleHistogramLongCount) &&
                        (pgoData[i + 1].InstrumentationKind == PgoInstrumentationKind.HandleHistogramTypes))
                    {
                        PgoSchemaElem? newElem = ComputeLikelyClass(i, handleToObject, nativeSchema, instrumentationData, compilationModuleGroup);
                        if (newElem.HasValue)
                        {
                            yield return newElem.Value;
                        }
                        i++; // The histogram is two entries long, so skip an extra entry
                        continue;
                    }
                    yield return pgoData[i];
                }

                IntPtr LocalObjectToHandle(object input)
                {
                    if (objectToHandle.TryGetValue(input, out var result))
                    {
                        return result;
                    }
                    result = new IntPtr(currentObjectIndex++);
                    objectToHandle.Add(input, result);
                    handleToObject.Add(result, input);
                    return result;
                }
            }
        }

        private static PgoSchemaElem? ComputeLikelyClass(int index, Dictionary<IntPtr, object> handleToObject, PgoInstrumentationSchema[] nativeSchema, byte[] instrumentationData, CompilationModuleGroup compilationModuleGroup)
        {
            // getLikelyClasses will use two entries from the native schema table. There must be at least two present to avoid overruning the buffer
            if (index > (nativeSchema.Length - 2))
                return null;

            fixed(PgoInstrumentationSchema* pSchema = &nativeSchema[index])
            {
                fixed(byte* pInstrumentationData = &instrumentationData[0])
                {
                    // We're going to store only the most popular type to reduce size of the profile
                    LikelyClassRecord* likelyClasses = stackalloc LikelyClassRecord[1];
                    uint numberOfClasses = getLikelyClasses(likelyClasses, 1, pSchema, 2, pInstrumentationData, nativeSchema[index].ILOffset);

                    if (numberOfClasses > 0)
                    {
                        TypeDesc type = (TypeDesc)handleToObject[likelyClasses->clsHandle];
#if READYTORUN
                        if (compilationModuleGroup.VersionsWithType(type))
#endif
                        {
                            PgoSchemaElem likelyClassElem = new PgoSchemaElem();
                            likelyClassElem.InstrumentationKind = PgoInstrumentationKind.GetLikelyClass;
                            likelyClassElem.ILOffset = nativeSchema[index].ILOffset;
                            likelyClassElem.Count = 1;
                            likelyClassElem.Other = (int)(likelyClasses->likelihood | (numberOfClasses << 8));
                            likelyClassElem.DataObject = new TypeSystemEntityOrUnknown[] { new TypeSystemEntityOrUnknown(type) };
                            return likelyClassElem;
                        }
                    }
                }
            }

            return null;
        }

        private void CompileMethodInternal(IMethodNode methodCodeNodeNeedingCode, MethodIL methodIL)
        {
            // methodIL must not be null
            if (methodIL == null)
            {
                ThrowHelper.ThrowInvalidProgramException(ExceptionStringID.InvalidProgramSpecific, MethodBeingCompiled);
            }

            CORINFO_METHOD_INFO methodInfo;
            Get_CORINFO_METHOD_INFO(MethodBeingCompiled, methodIL, &methodInfo);

            _methodScope = methodInfo.scope;

#if !READYTORUN
            SetDebugInformation(methodCodeNodeNeedingCode, methodIL);
#endif

            CorInfoImpl _this = this;

            IntPtr exception;
            IntPtr nativeEntry;
            uint codeSize;
            var result = JitCompileMethod(out exception,
                    _jit, (IntPtr)Unsafe.AsPointer(ref _this), _unmanagedCallbacks,
                    ref methodInfo, (uint)CorJitFlag.CORJIT_FLAG_CALL_GETJITFLAGS, out nativeEntry, out codeSize);
            if (exception != IntPtr.Zero)
            {
                if (_lastException != null)
                {
                    // If we captured a managed exception, rethrow that.
                    // TODO: might not actually be the real reason. It could be e.g. a JIT failure/bad IL that followed
                    // an inlining attempt with a type system problem in it...
#if SUPPORT_JIT
                    _lastException.Throw();
#else
                    if (_lastException.SourceException is TypeSystemException)
                    {
                        // Type system exceptions can be turned into code that throws the exception at runtime.
                        _lastException.Throw();
                    }
#if READYTORUN
                    else if (_lastException.SourceException is RequiresRuntimeJitException)
                    {
                        // Runtime JIT requirement is not a cause for failure, we just mustn't JIT a particular method
                        _lastException.Throw();
                    }
#endif
                    else
                    {
                        // This is just a bug somewhere.
                        throw new CodeGenerationFailedException(_methodCodeNode.Method, _lastException.SourceException);
                    }
#endif
                }

                // This is a failure we don't know much about.
                char* szMessage = GetExceptionMessage(exception);
                string message = szMessage != null ? new string(szMessage) : "JIT Exception";
                throw new Exception(message);
            }
            if (result == CorJitResult.CORJIT_BADCODE)
            {
                ThrowHelper.ThrowInvalidProgramException();
            }
            if (result == CorJitResult.CORJIT_IMPLLIMITATION)
            {
#if READYTORUN
                throw new RequiresRuntimeJitException("JIT implementation limitation");
#else
                ThrowHelper.ThrowInvalidProgramException();
#endif
            }
            if (result != CorJitResult.CORJIT_OK)
            {
#if SUPPORT_JIT
                // FailFast?
                throw new Exception("JIT failed");
#else
                throw new CodeGenerationFailedException(_methodCodeNode.Method);
#endif
            }

            if (codeSize < _code.Length)
            {
                if (_compilation.TypeSystemContext.Target.Architecture != TargetArchitecture.ARM64)
                {
                    // For xarch/arm32, the generated code is sometimes smaller than the memory allocated.
                    // In that case, trim the codeBlock to the actual value.
                    //
                    // For arm64, the allocation request of `hotCodeSize` also includes the roData size
                    // while the `codeSize` returned just contains the size of the native code. As such,
                    // there is guarantee that for armarch, (codeSize == _code.Length) is always true.
                    //
                    // Currently, hot/cold splitting is not done and hence `codeSize` just includes the size of
                    // hotCode. Once hot/cold splitting is done, need to trim respective `_code` or `_coldCode`
                    // accordingly.
                    Debug.Assert(codeSize != 0);
                    Array.Resize(ref _code, (int)codeSize);
                }
            }
            PublishCode();
            PublishROData();
        }

        private void PublishCode()
        {
            var relocs = _codeRelocs.ToArray();
            Array.Sort(relocs, (x, y) => (x.Offset - y.Offset));

            int alignment = JitConfigProvider.Instance.HasFlag(CorJitFlag.CORJIT_FLAG_SIZE_OPT) ?
                _compilation.NodeFactory.Target.MinimumFunctionAlignment :
                _compilation.NodeFactory.Target.OptimumFunctionAlignment;

            alignment = Math.Max(alignment, _codeAlignment);

            var objectData = new ObjectNode.ObjectData(_code,
                                                       relocs,
                                                       alignment,
                                                       new ISymbolDefinitionNode[] { _methodCodeNode });
            ObjectNode.ObjectData ehInfo = _ehClauses != null ? EncodeEHInfo() : null;
            DebugEHClauseInfo[] debugEHClauseInfos = null;
            if (_ehClauses != null)
            {
                debugEHClauseInfos = new DebugEHClauseInfo[_ehClauses.Length];
                for (int i = 0; i < _ehClauses.Length; i++)
                {
                    var clause = _ehClauses[i];
                    debugEHClauseInfos[i] = new DebugEHClauseInfo(clause.TryOffset, clause.TryLength,
                                                        clause.HandlerOffset, clause.HandlerLength);
                }
            }

            _methodCodeNode.SetCode(objectData
#if !SUPPORT_JIT && !READYTORUN
                , isFoldable: (_compilation._compilationOptions & RyuJitCompilationOptions.MethodBodyFolding) != 0
#endif
                );

            _methodCodeNode.InitializeFrameInfos(_frameInfos);
            _methodCodeNode.InitializeDebugEHClauseInfos(debugEHClauseInfos);
            _methodCodeNode.InitializeGCInfo(_gcInfo);
            _methodCodeNode.InitializeEHInfo(ehInfo);

            _methodCodeNode.InitializeDebugLocInfos(_debugLocInfos);
            _methodCodeNode.InitializeDebugVarInfos(_debugVarInfos);
#if READYTORUN
            _methodCodeNode.InitializeInliningInfo(_inlinedMethods.ToArray(), _compilation.NodeFactory);

            // Detect cases where the instruction set support used is a superset of the baseline instruction set specification
            var baselineSupport = _compilation.InstructionSetSupport;
            bool needPerMethodInstructionSetFixup = false;
            foreach (var instructionSet in _actualInstructionSetSupported)
            {
                if (!baselineSupport.IsInstructionSetSupported(instructionSet))
                {
                    needPerMethodInstructionSetFixup = true;
                }
            }
            foreach (var instructionSet in _actualInstructionSetUnsupported)
            {
                if (!baselineSupport.IsInstructionSetExplicitlyUnsupported(instructionSet))
                {
                    needPerMethodInstructionSetFixup = true;
                }
            }

            if (needPerMethodInstructionSetFixup)
            {
                TargetArchitecture architecture = _compilation.TypeSystemContext.Target.Architecture;
                _actualInstructionSetSupported.ExpandInstructionSetByImplication(architecture);
                _actualInstructionSetUnsupported.ExpandInstructionSetByReverseImplication(architecture);
                _actualInstructionSetUnsupported.Set64BitInstructionSetVariants(architecture);

                InstructionSetSupport actualSupport = new InstructionSetSupport(_actualInstructionSetSupported, _actualInstructionSetUnsupported, architecture);
                var node = _compilation.SymbolNodeFactory.PerMethodInstructionSetSupportFixup(actualSupport);
                _methodCodeNode.Fixups.Add(node);
            }
#else
            var methodIL = (MethodIL)HandleToObject((IntPtr)_methodScope);
            CodeBasedDependencyAlgorithm.AddDependenciesDueToMethodCodePresence(ref _additionalDependencies, _compilation.NodeFactory, MethodBeingCompiled, methodIL);
            _methodCodeNode.InitializeNonRelocationDependencies(_additionalDependencies);
            _methodCodeNode.InitializeDebugInfo(_debugInfo);

            LocalVariableDefinition[] locals = methodIL.GetLocals();
            TypeDesc[] localTypes = new TypeDesc[locals.Length];
            for (int i = 0; i < localTypes.Length; i++)
                localTypes[i] = locals[i].Type;

            _methodCodeNode.InitializeLocalTypes(localTypes);
#endif
        }

        private void PublishROData()
        {
            if (_roDataBlob == null)
            {
                return;
            }

            var relocs = _roDataRelocs.ToArray();
            Array.Sort(relocs, (x, y) => (x.Offset - y.Offset));
            var objectData = new ObjectNode.ObjectData(_roData,
                                                       relocs,
                                                       _roDataAlignment,
                                                       new ISymbolDefinitionNode[] { _roDataBlob });

            _roDataBlob.InitializeData(objectData);
        }

        private MethodDesc MethodBeingCompiled
        {
            get
            {
                return _methodCodeNode.Method;
            }
        }

        private int PointerSize
        {
            get
            {
                return _compilation.TypeSystemContext.Target.PointerSize;
            }
        }

        private Dictionary<Object, GCHandle> _pins = new Dictionary<object, GCHandle>();

        private IntPtr GetPin(Object obj)
        {
            GCHandle handle;
            if (!_pins.TryGetValue(obj, out handle))
            {
                handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
                _pins.Add(obj, handle);
            }
            return handle.AddrOfPinnedObject();
        }

        private List<IntPtr> _nativeExceptions;

        private void CompileMethodCleanup()
        {
            foreach (var pin in _pins)
                pin.Value.Free();
            _pins.Clear();

            if (_nativeExceptions != null)
            {
                foreach (IntPtr ex in _nativeExceptions)
                    FreeException(ex);
                _nativeExceptions = null;
            }

            _methodCodeNode = null;

            _code = null;
            _coldCode = null;

            _roData = null;
            _roDataBlob = null;

            _codeRelocs = new ArrayBuilder<Relocation>();
            _roDataRelocs = new ArrayBuilder<Relocation>();

            _numFrameInfos = 0;
            _usedFrameInfos = 0;
            _frameInfos = null;

            _gcInfo = null;
            _ehClauses = null;

#if !READYTORUN
            _debugInfo = null;

            _additionalDependencies = null;
#endif
            _debugLocInfos = null;
            _debugVarInfos = null;
            _lastException = null;

#if READYTORUN
            _inlinedMethods = new ArrayBuilder<MethodDesc>();
            _actualInstructionSetSupported = default(InstructionSetFlags);
            _actualInstructionSetUnsupported = default(InstructionSetFlags);
#endif

            _instantiationToJitVisibleInstantiation = null;

            _pgoResults.Clear();
        }

        private Dictionary<Object, IntPtr> _objectToHandle = new Dictionary<Object, IntPtr>();
        private List<Object> _handleToObject = new List<Object>();

        private const int handleMultipler = 8;
        private const int handleBase = 0x420000;

#if DEBUG
        private static readonly IntPtr s_handleHighBitSet = (sizeof(IntPtr) == 4) ? new IntPtr(0x40000000) : new IntPtr(0x4000000000000000);
#endif

        private IntPtr ObjectToHandle(Object obj)
        {
            // SuperPMI relies on the handle returned from this function being stable for the lifetime of the crossgen2 process
            // If handle deletion is implemented, please update SuperPMI
            IntPtr handle;
            if (!_objectToHandle.TryGetValue(obj, out handle))
            {
                handle = (IntPtr)(handleMultipler * _handleToObject.Count + handleBase);
#if DEBUG
                handle = new IntPtr((long)s_handleHighBitSet | (long)handle);
#endif
                _handleToObject.Add(obj);
                _objectToHandle.Add(obj, handle);
            }
            return handle;
        }

        private Object HandleToObject(IntPtr handle)
        {
#if DEBUG
            handle = new IntPtr(~(long)s_handleHighBitSet & (long) handle);
#endif
            int index = ((int)handle - handleBase) / handleMultipler;
            return _handleToObject[index];
        }

        private MethodDesc HandleToObject(CORINFO_METHOD_STRUCT_* method) => (MethodDesc)HandleToObject((IntPtr)method);
        private CORINFO_METHOD_STRUCT_* ObjectToHandle(MethodDesc method) => (CORINFO_METHOD_STRUCT_*)ObjectToHandle((Object)method);
        private TypeDesc HandleToObject(CORINFO_CLASS_STRUCT_* type) => (TypeDesc)HandleToObject((IntPtr)type);
        private CORINFO_CLASS_STRUCT_* ObjectToHandle(TypeDesc type) => (CORINFO_CLASS_STRUCT_*)ObjectToHandle((Object)type);
        private FieldDesc HandleToObject(CORINFO_FIELD_STRUCT_* field) => (FieldDesc)HandleToObject((IntPtr)field);
        private CORINFO_FIELD_STRUCT_* ObjectToHandle(FieldDesc field) => (CORINFO_FIELD_STRUCT_*)ObjectToHandle((object)field);
        private MethodILScope HandleToObject(CORINFO_MODULE_STRUCT_* module) => (MethodIL)HandleToObject((IntPtr)module);
        private CORINFO_MODULE_STRUCT_* ObjectToHandle(MethodILScope methodIL) => (CORINFO_MODULE_STRUCT_*)ObjectToHandle((object)methodIL);
        private MethodSignature HandleToObject(MethodSignatureInfo* method) => (MethodSignature)HandleToObject((IntPtr)method);
        private MethodSignatureInfo* ObjectToHandle(MethodSignature method) => (MethodSignatureInfo*)ObjectToHandle((object)method);

        private bool Get_CORINFO_METHOD_INFO(MethodDesc method, MethodIL methodIL, CORINFO_METHOD_INFO* methodInfo)
        {
            if (methodIL == null)
            {
                *methodInfo = default(CORINFO_METHOD_INFO);
                return false;
            }

            methodInfo->ftn = ObjectToHandle(method);
            methodInfo->scope = ObjectToHandle(methodIL);
            var ilCode = methodIL.GetILBytes();
            methodInfo->ILCode = (byte*)GetPin(ilCode);
            methodInfo->ILCodeSize = (uint)ilCode.Length;
            methodInfo->maxStack = (uint)methodIL.MaxStack;
            var exceptionRegions = methodIL.GetExceptionRegions();
            methodInfo->EHcount = (uint)exceptionRegions.Length;
            methodInfo->options = methodIL.IsInitLocals ? CorInfoOptions.CORINFO_OPT_INIT_LOCALS : (CorInfoOptions)0;

            if (method.AcquiresInstMethodTableFromThis())
            {
                methodInfo->options |= CorInfoOptions.CORINFO_GENERICS_CTXT_FROM_THIS;
            }
            else if (method.RequiresInstMethodDescArg())
            {
                methodInfo->options |= CorInfoOptions.CORINFO_GENERICS_CTXT_FROM_METHODDESC;
            }
            else if (method.RequiresInstMethodTableArg())
            {
                methodInfo->options |= CorInfoOptions.CORINFO_GENERICS_CTXT_FROM_METHODTABLE;
            }
            methodInfo->regionKind = CorInfoRegionKind.CORINFO_REGION_NONE;
            Get_CORINFO_SIG_INFO(method, sig: &methodInfo->args, methodIL);
            Get_CORINFO_SIG_INFO(methodIL.GetLocals(), &methodInfo->locals);

#if READYTORUN
            if ((methodInfo->options & CorInfoOptions.CORINFO_GENERICS_CTXT_MASK) != 0)
            {
                foreach (var region in exceptionRegions)
                {
                    if (region.Kind == ILExceptionRegionKind.Catch)
                    {
                        TypeDesc catchType = (TypeDesc)methodIL.GetObject(region.ClassToken);
                        if (catchType.IsCanonicalSubtype(CanonicalFormKind.Any))
                        {
                            methodInfo->options |= CorInfoOptions.CORINFO_GENERICS_CTXT_KEEP_ALIVE;
                            break;
                        }
                    }
                }
            }
#endif

            return true;
        }

        private Dictionary<Instantiation, IntPtr[]> _instantiationToJitVisibleInstantiation = null;
        private CORINFO_CLASS_STRUCT_** GetJitInstantiation(Instantiation inst)
        {
            IntPtr [] jitVisibleInstantiation;
            if (_instantiationToJitVisibleInstantiation == null)
            {
                _instantiationToJitVisibleInstantiation = new Dictionary<Instantiation, IntPtr[]>();
            }

            if (!_instantiationToJitVisibleInstantiation.TryGetValue(inst, out jitVisibleInstantiation))
            {
                jitVisibleInstantiation =  new IntPtr[inst.Length];
                for (int i = 0; i < inst.Length; i++)
                    jitVisibleInstantiation[i] = (IntPtr)ObjectToHandle(inst[i]);
                _instantiationToJitVisibleInstantiation.Add(inst, jitVisibleInstantiation);
            }
            return (CORINFO_CLASS_STRUCT_**)GetPin(jitVisibleInstantiation);
        }

        private void Get_CORINFO_SIG_INFO(MethodDesc method, CORINFO_SIG_INFO* sig, MethodILScope scope, bool suppressHiddenArgument = false)
        {
            Get_CORINFO_SIG_INFO(method.Signature, sig, scope);

            // Does the method have a hidden parameter?
            bool hasHiddenParameter = !suppressHiddenArgument && method.RequiresInstArg();

            if (method.IsIntrinsic)
            {
                // Some intrinsics will beg to differ about the hasHiddenParameter decision
#if !READYTORUN
                if (_compilation.TypeSystemContext.IsSpecialUnboxingThunkTargetMethod(method))
                    hasHiddenParameter = false;
#endif

                if (method.IsArrayAddressMethod())
                    hasHiddenParameter = true;

                // We only populate sigInst for intrinsic methods because most of the time,
                // JIT doesn't care what the instantiation is and this is expensive.
                Instantiation owningTypeInst = method.OwningType.Instantiation;
                sig->sigInst.classInstCount = (uint)owningTypeInst.Length;
                if (owningTypeInst.Length != 0)
                {
                    sig->sigInst.classInst = GetJitInstantiation(owningTypeInst);
                }

                sig->sigInst.methInstCount = (uint)method.Instantiation.Length;
                if (method.Instantiation.Length != 0)
                {
                    sig->sigInst.methInst = GetJitInstantiation(method.Instantiation);
                }
            }

            if (hasHiddenParameter)
            {
                sig->callConv |= CorInfoCallConv.CORINFO_CALLCONV_PARAMTYPE;
            }
        }

        private CorInfoCallConvExtension GetUnmanagedCallingConventionFromAttribute(CustomAttributeValue<TypeDesc> attributeWithCallConvsArray, out bool suppressGCTransition)
        {
            suppressGCTransition = false;
            CorInfoCallConvExtension callConv = (CorInfoCallConvExtension)PlatformDefaultUnmanagedCallingConvention();

            bool found = false;
            bool memberFunctionVariant = false;
            foreach (DefType defType in attributeWithCallConvsArray.EnumerateCallConvsFromAttribute())
            {
                if (defType.Name == "CallConvMemberFunction")
                {
                    memberFunctionVariant = true;
                    continue;
                }

                if (defType.Name == "CallConvSuppressGCTransition")
                {
                    suppressGCTransition = true;
                    continue;
                }

                CorInfoCallConvExtension? callConvLocal = GetCallingConventionForCallConvType(defType);

                if (callConvLocal.HasValue)
                {
                    // Error if there are multiple recognized calling conventions
                    if (found)
                        ThrowHelper.ThrowInvalidProgramException(ExceptionStringID.InvalidProgramMultipleCallConv, MethodBeingCompiled);

                    callConv = callConvLocal.Value;
                    found = true;
                }
            }

            if (memberFunctionVariant)
            {
                callConv = GetMemberFunctionCallingConventionVariant(callConv);
            }

            return callConv;
        }

        private bool TryGetUnmanagedCallingConventionFromModOpt(MethodSignature signature, out CorInfoCallConvExtension callConv, out bool suppressGCTransition)
        {
            suppressGCTransition = false;
            // Default to managed since in the modopt case we need to differentiate explicitly using a calling convention that matches the default
            // and not specifying a calling convention at all and using the implicit default case in P/Invoke stub inlining.
            callConv = CorInfoCallConvExtension.Managed;
            if (!signature.HasEmbeddedSignatureData)
                return false;

            bool found = false;
            bool memberFunctionVariant = false;
            foreach (EmbeddedSignatureData data in signature.GetEmbeddedSignatureData())
            {
                if (data.kind != EmbeddedSignatureDataKind.OptionalCustomModifier)
                    continue;

                // We only care about the modifiers for the return type. These will be at the start of
                // the signature, so will be first in the array of embedded signature data.
                if (data.index != MethodSignature.IndexOfCustomModifiersOnReturnType)
                    break;

                if (!(data.type is DefType defType))
                    continue;

                if (defType.Namespace != "System.Runtime.CompilerServices")
                    continue;

                if (defType.Name == "CallConvSuppressGCTransition")
                {
                    suppressGCTransition = true;
                    continue;
                }
                else if (defType.Name == "CallConvMemberFunction")
                {
                    memberFunctionVariant = true;
                    continue;
                }

                CorInfoCallConvExtension? callConvLocal = GetCallingConventionForCallConvType(defType);

                if (callConvLocal.HasValue)
                {
                    // Error if there are multiple recognized calling conventions
                    if (found)
                        ThrowHelper.ThrowInvalidProgramException(ExceptionStringID.InvalidProgramMultipleCallConv, MethodBeingCompiled);

                    callConv = callConvLocal.Value;
                    found = true;
                }
            }

            if (memberFunctionVariant)
            {
                callConv = GetMemberFunctionCallingConventionVariant(found ? callConv : (CorInfoCallConvExtension)PlatformDefaultUnmanagedCallingConvention());
                found = true;
            }

            return found;
        }

        private static CorInfoCallConvExtension? GetCallingConventionForCallConvType(DefType defType) =>
            // Look for a recognized calling convention in metadata.
            defType.Name switch
            {
                "CallConvCdecl" => CorInfoCallConvExtension.C,
                "CallConvStdcall" => CorInfoCallConvExtension.Stdcall,
                "CallConvFastcall" => CorInfoCallConvExtension.Fastcall,
                "CallConvThiscall" => CorInfoCallConvExtension.Thiscall,
                _ => null
            };

        private static CorInfoCallConvExtension GetMemberFunctionCallingConventionVariant(CorInfoCallConvExtension baseCallConv) =>
            baseCallConv switch
            {
                CorInfoCallConvExtension.C => CorInfoCallConvExtension.CMemberFunction,
                CorInfoCallConvExtension.Stdcall => CorInfoCallConvExtension.StdcallMemberFunction,
                CorInfoCallConvExtension.Fastcall => CorInfoCallConvExtension.FastcallMemberFunction,
                var c => c
            };

        private void Get_CORINFO_SIG_INFO(MethodSignature signature, CORINFO_SIG_INFO* sig, MethodILScope scope)
        {
            sig->callConv = (CorInfoCallConv)(signature.Flags & MethodSignatureFlags.UnmanagedCallingConventionMask);

            // Varargs are not supported in .NET Core
            if (sig->callConv == CorInfoCallConv.CORINFO_CALLCONV_VARARG)
                ThrowHelper.ThrowBadImageFormatException();

            if (!signature.IsStatic) sig->callConv |= CorInfoCallConv.CORINFO_CALLCONV_HASTHIS;

            TypeDesc returnType = signature.ReturnType;

            CorInfoType corInfoRetType = asCorInfoType(signature.ReturnType, &sig->retTypeClass);
            sig->_retType = (byte)corInfoRetType;
            sig->retTypeSigClass = ObjectToHandle(signature.ReturnType);

            sig->flags = 0;    // used by IL stubs code

            sig->numArgs = (ushort)signature.Length;

            sig->args = (CORINFO_ARG_LIST_STRUCT_*)0; // CORINFO_ARG_LIST_STRUCT_ is argument index

            sig->sigInst.classInst = null; // Not used by the JIT
            sig->sigInst.classInstCount = 0; // Not used by the JIT
            sig->sigInst.methInst = null; // Not used by the JIT
            sig->sigInst.methInstCount = (uint)signature.GenericParameterCount;

            sig->pSig = null;
            sig->cbSig = 0; // Not used by the JIT
            sig->methodSignature = ObjectToHandle(signature);
            sig->scope = scope is not null ? ObjectToHandle(scope) : null; // scope can be null for internal calls and COM methods.
            sig->token = 0; // Not used by the JIT
        }

        private void Get_CORINFO_SIG_INFO(LocalVariableDefinition[] locals, CORINFO_SIG_INFO* sig)
        {
            sig->callConv = CorInfoCallConv.CORINFO_CALLCONV_DEFAULT;
            sig->_retType = (byte)CorInfoType.CORINFO_TYPE_VOID;
            sig->retTypeClass = null;
            sig->retTypeSigClass = null;
            sig->flags = CorInfoSigInfoFlags.CORINFO_SIGFLAG_IS_LOCAL_SIG;

            sig->numArgs = (ushort)locals.Length;

            sig->sigInst.classInst = null;
            sig->sigInst.classInstCount = 0;
            sig->sigInst.methInst = null;
            sig->sigInst.methInstCount = 0;

            sig->args = (CORINFO_ARG_LIST_STRUCT_*)0; // CORINFO_ARG_LIST_STRUCT_ is argument index


            sig->pSig = null;
            sig->cbSig = 0; // Not used by the JIT
            sig->methodSignature = (MethodSignatureInfo*)ObjectToHandle(locals);
            sig->scope = null; // Not used by the JIT
            sig->token = 0; // Not used by the JIT
        }

        private CorInfoType asCorInfoType(TypeDesc type)
        {
            return asCorInfoType(type, out _);
        }

        private CorInfoType asCorInfoType(TypeDesc type, out TypeDesc typeIfNotPrimitive)
        {
            if (type.IsEnum)
            {
                type = type.UnderlyingType;
            }

            if (type.IsPrimitive)
            {
                typeIfNotPrimitive = null;
                Debug.Assert((CorInfoType)TypeFlags.Void == CorInfoType.CORINFO_TYPE_VOID);
                Debug.Assert((CorInfoType)TypeFlags.Double == CorInfoType.CORINFO_TYPE_DOUBLE);

                return (CorInfoType)type.Category;
            }

            if (type.IsPointer || type.IsFunctionPointer)
            {
                typeIfNotPrimitive = null;
                return CorInfoType.CORINFO_TYPE_PTR;
            }

            typeIfNotPrimitive = type;

            if (type.IsByRef)
            {
                return CorInfoType.CORINFO_TYPE_BYREF;
            }

            if (type.IsValueType)
            {
                if (_compilation.TypeSystemContext.Target.Architecture == TargetArchitecture.X86)
                {
                    LayoutInt elementSize = type.GetElementSize();

#if READYTORUN
                    if (elementSize.IsIndeterminate)
                    {
                        throw new RequiresRuntimeJitException(type);
                    }
#endif
                }
                return CorInfoType.CORINFO_TYPE_VALUECLASS;
            }

            return CorInfoType.CORINFO_TYPE_CLASS;
        }

        private CorInfoType asCorInfoType(TypeDesc type, CORINFO_CLASS_STRUCT_** structType)
        {
            var corInfoType = asCorInfoType(type, out TypeDesc typeIfNotPrimitive);
            *structType = (typeIfNotPrimitive != null) ? ObjectToHandle(typeIfNotPrimitive) : null;
            return corInfoType;
        }

        private CORINFO_CONTEXT_STRUCT* contextFromMethod(MethodDesc method)
        {
            return (CORINFO_CONTEXT_STRUCT*)(((ulong)ObjectToHandle(method)) | (ulong)CorInfoContextFlags.CORINFO_CONTEXTFLAGS_METHOD);
        }

        private CORINFO_CONTEXT_STRUCT* contextFromType(TypeDesc type)
        {
            return (CORINFO_CONTEXT_STRUCT*)(((ulong)ObjectToHandle(type)) | (ulong)CorInfoContextFlags.CORINFO_CONTEXTFLAGS_CLASS);
        }

        private static CORINFO_CONTEXT_STRUCT* contextFromMethodBeingCompiled()
        {
            return (CORINFO_CONTEXT_STRUCT*)1;
        }

        private MethodDesc methodFromContext(CORINFO_CONTEXT_STRUCT* contextStruct)
        {
            if (contextStruct == contextFromMethodBeingCompiled())
            {
                return MethodBeingCompiled;
            }

            if (((ulong)contextStruct & (ulong)CorInfoContextFlags.CORINFO_CONTEXTFLAGS_MASK) == (ulong)CorInfoContextFlags.CORINFO_CONTEXTFLAGS_CLASS)
            {
                return null;
            }
            else
            {
                return HandleToObject((CORINFO_METHOD_STRUCT_*)((ulong)contextStruct & ~(ulong)CorInfoContextFlags.CORINFO_CONTEXTFLAGS_MASK));
            }
        }

        private TypeDesc typeFromContext(CORINFO_CONTEXT_STRUCT* contextStruct)
        {
            if (contextStruct == contextFromMethodBeingCompiled())
            {
                return MethodBeingCompiled.OwningType;
            }

            if (((ulong)contextStruct & (ulong)CorInfoContextFlags.CORINFO_CONTEXTFLAGS_MASK) == (ulong)CorInfoContextFlags.CORINFO_CONTEXTFLAGS_CLASS)
            {
                return HandleToObject((CORINFO_CLASS_STRUCT_*)((ulong)contextStruct & ~(ulong)CorInfoContextFlags.CORINFO_CONTEXTFLAGS_MASK));
            }
            else
            {
                return HandleToObject((CORINFO_METHOD_STRUCT_*)((ulong)contextStruct & ~(ulong)CorInfoContextFlags.CORINFO_CONTEXTFLAGS_MASK)).OwningType;
            }
        }

        private TypeSystemEntity entityFromContext(CORINFO_CONTEXT_STRUCT* contextStruct)
        {
            if (contextStruct == contextFromMethodBeingCompiled())
            {
                return MethodBeingCompiled.HasInstantiation ? (TypeSystemEntity)MethodBeingCompiled: (TypeSystemEntity)MethodBeingCompiled.OwningType;
            }

            return (TypeSystemEntity)HandleToObject((IntPtr)((ulong)contextStruct & ~(ulong)CorInfoContextFlags.CORINFO_CONTEXTFLAGS_MASK));
        }

        private bool isIntrinsic(CORINFO_METHOD_STRUCT_* ftn)
        {
            MethodDesc method = HandleToObject(ftn);
            return method.IsIntrinsic || HardwareIntrinsicHelpers.IsHardwareIntrinsic(method);
        }

        private uint getMethodAttribsInternal(MethodDesc method)
        {
            CorInfoFlag result = 0;

            // CORINFO_FLG_PROTECTED - verification only

            if (method.Signature.IsStatic)
                result |= CorInfoFlag.CORINFO_FLG_STATIC;

            if (method.IsSynchronized)
                result |= CorInfoFlag.CORINFO_FLG_SYNCH;
            if (method.IsIntrinsic)
                result |= CorInfoFlag.CORINFO_FLG_INTRINSIC;
            if (method.IsVirtual)
                result |= CorInfoFlag.CORINFO_FLG_VIRTUAL;
            if (method.IsAbstract)
                result |= CorInfoFlag.CORINFO_FLG_ABSTRACT;
            if (method.IsConstructor || method.IsStaticConstructor)
                result |= CorInfoFlag.CORINFO_FLG_CONSTRUCTOR;

            //
            // See if we need to embed a .cctor call at the head of the
            // method body.
            //

            // method or class might have the final bit
            if (_compilation.IsEffectivelySealed(method))
                result |= CorInfoFlag.CORINFO_FLG_FINAL;

            if (method.IsSharedByGenericInstantiations)
                result |= CorInfoFlag.CORINFO_FLG_SHAREDINST;

            if (method.IsPInvoke)
                result |= CorInfoFlag.CORINFO_FLG_PINVOKE;

#if READYTORUN
            if (method.RequireSecObject)
            {
                result |= CorInfoFlag.CORINFO_FLG_DONT_INLINE_CALLER;
            }
#endif

            if (method.IsAggressiveOptimization)
            {
                result |= CorInfoFlag.CORINFO_FLG_AGGRESSIVE_OPT;
            }

            // TODO: Cache inlining hits
            // Check for an inlining directive.

            if (method.IsNoInlining)
            {
                /* Function marked as not inlineable */
                result |= CorInfoFlag.CORINFO_FLG_DONT_INLINE;
            }
            else if (method.IsAggressiveInlining)
            {
                result |= CorInfoFlag.CORINFO_FLG_FORCEINLINE;
            }

            if (method.OwningType.IsDelegate && method.Name == "Invoke")
            {
                // This is now used to emit efficient invoke code for any delegate invoke,
                // including multicast.
                result |= CorInfoFlag.CORINFO_FLG_DELEGATE_INVOKE;

                // RyuJIT special cases this method; it would assert if it's not final
                // and we might not have set the bit in the code above.
                result |= CorInfoFlag.CORINFO_FLG_FINAL;
            }

#if READYTORUN
            // Check for SIMD intrinsics
            if (method.Context.Target.MaximumSimdVectorLength == SimdVectorLength.None)
            {
                DefType owningDefType = method.OwningType as DefType;
                if (owningDefType != null && VectorOfTFieldLayoutAlgorithm.IsVectorOfTType(owningDefType))
                {
                    throw new RequiresRuntimeJitException("This function is using SIMD intrinsics, their size is machine specific");
                }
            }
#endif

            // Check for hardware intrinsics
            if (HardwareIntrinsicHelpers.IsHardwareIntrinsic(method))
            {
                result |= CorInfoFlag.CORINFO_FLG_INTRINSIC;
            }

            // Internal calls typically turn into fcalls that do not always
            // probe for GC. Be conservative here and always let JIT know that
            // this method may not do GC checks so the JIT might need to make
            // callers fully interruptible.
            if (method.IsInternalCall)
            {
                result |= CorInfoFlag.CORINFO_FLG_NOGCCHECK;
            }

            return (uint)result;
        }

        private void setMethodAttribs(CORINFO_METHOD_STRUCT_* ftn, CorInfoMethodRuntimeFlags attribs)
        {
            // TODO: Inlining
        }

        private void getMethodSig(CORINFO_METHOD_STRUCT_* ftn, CORINFO_SIG_INFO* sig, CORINFO_CLASS_STRUCT_* memberParent)
        {
            MethodDesc method = HandleToObject(ftn);

            // There might be a more concrete parent type specified - this can happen when inlining.
            if (memberParent != null)
            {
                TypeDesc type = HandleToObject(memberParent);

                // Typically, the owning type of the method is a canonical type and the member parent
                // supplied by RyuJIT is a concrete instantiation.
                if (type != method.OwningType)
                {
                    Debug.Assert(type.HasSameTypeDefinition(method.OwningType));
                    Instantiation methodInst = method.Instantiation;
                    method = _compilation.TypeSystemContext.GetMethodForInstantiatedType(method.GetTypicalMethodDefinition(), (InstantiatedType)type);
                    if (methodInst.Length > 0)
                    {
                        method = method.MakeInstantiatedMethod(methodInst);
                    }
                }
            }

            Get_CORINFO_SIG_INFO(method, sig: sig, scope: null);
        }

        private bool getMethodInfo(CORINFO_METHOD_STRUCT_* ftn, CORINFO_METHOD_INFO* info)
        {
            MethodDesc method = HandleToObject(ftn);
            MethodIL methodIL = _compilation.GetMethodIL(method);
            return Get_CORINFO_METHOD_INFO(method, methodIL, info);
        }

        private CorInfoInline canInline(CORINFO_METHOD_STRUCT_* callerHnd, CORINFO_METHOD_STRUCT_* calleeHnd)
        {
            MethodDesc callerMethod = HandleToObject(callerHnd);
            MethodDesc calleeMethod = HandleToObject(calleeHnd);

            if (_compilation.CanInline(callerMethod, calleeMethod))
            {
                // No restrictions on inlining
                return CorInfoInline.INLINE_PASS;
            }
            else
            {
                // Call may not be inlined
                return CorInfoInline.INLINE_NEVER;
            }
        }

        private void reportTailCallDecision(CORINFO_METHOD_STRUCT_* callerHnd, CORINFO_METHOD_STRUCT_* calleeHnd, bool fIsTailPrefix, CorInfoTailCall tailCallResult, byte* reason)
        {
        }

        private void getEHinfo(CORINFO_METHOD_STRUCT_* ftn, uint EHnumber, ref CORINFO_EH_CLAUSE clause)
        {
            var methodIL = _compilation.GetMethodIL(HandleToObject(ftn));

            var ehRegion = methodIL.GetExceptionRegions()[EHnumber];

            clause.Flags = (CORINFO_EH_CLAUSE_FLAGS)ehRegion.Kind;
            clause.TryOffset = (uint)ehRegion.TryOffset;
            clause.TryLength = (uint)ehRegion.TryLength;
            clause.HandlerOffset = (uint)ehRegion.HandlerOffset;
            clause.HandlerLength = (uint)ehRegion.HandlerLength;
            clause.ClassTokenOrOffset = (uint)((ehRegion.Kind == ILExceptionRegionKind.Filter) ? ehRegion.FilterOffset : ehRegion.ClassToken);
        }

        private CORINFO_CLASS_STRUCT_* getMethodClass(CORINFO_METHOD_STRUCT_* method)
        {
            var m = HandleToObject(method);
            return ObjectToHandle(m.OwningType);
        }

        private CORINFO_MODULE_STRUCT_* getMethodModule(CORINFO_METHOD_STRUCT_* method)
        {
            MethodDesc m = HandleToObject(method);
            if (m is UnboxingMethodDesc unboxingMethodDesc)
            {
                m = unboxingMethodDesc.Target;
            }

            MethodIL methodIL = _compilation.GetMethodIL(m);
            if (methodIL == null)
            {
                return null;
            }
            return ObjectToHandle(methodIL);
        }

        private bool resolveVirtualMethod(CORINFO_DEVIRTUALIZATION_INFO* info)
        {
            // Initialize OUT fields
            info->devirtualizedMethod = null;
            info->requiresInstMethodTableArg = false;
            info->exactContext = null;
            info->detail = CORINFO_DEVIRTUALIZATION_DETAIL.CORINFO_DEVIRTUALIZATION_UNKNOWN;

            TypeDesc objType = HandleToObject(info->objClass);

            // __Canon cannot be devirtualized
            if (objType.IsCanonicalDefinitionType(CanonicalFormKind.Any))
            {
                info->detail = CORINFO_DEVIRTUALIZATION_DETAIL.CORINFO_DEVIRTUALIZATION_FAILED_CANON;
                return false;
            }

            MethodDesc decl = HandleToObject(info->virtualMethod);
            Debug.Assert(!decl.HasInstantiation);

            if ((info->context != null) && decl.OwningType.IsInterface)
            {
                TypeDesc ownerTypeDesc = typeFromContext(info->context);
                if (decl.OwningType != ownerTypeDesc)
                {
                    Debug.Assert(ownerTypeDesc is InstantiatedType);
                    decl = _compilation.TypeSystemContext.GetMethodForInstantiatedType(decl.GetTypicalMethodDefinition(), (InstantiatedType)ownerTypeDesc);
                }
            }

            MethodDesc originalImpl = _compilation.ResolveVirtualMethod(decl, objType, out info->detail);

            if (originalImpl == null)
            {
                // If this assert fires, we failed to devirtualize, probably due to a failure to resolve the
                // virtual to an exact target. This should never happen in practice if the input IL is valid,
                // and the algorithm for virtual function resolution is correct; however, if it does, this is
                // a safe condition, and we could delete this assert. This assert exists in order to help identify
                // cases where the virtual function resolution algorithm either does not function, or is not used
                // correctly.
#if DEBUG
                if (info->detail == CORINFO_DEVIRTUALIZATION_DETAIL.CORINFO_DEVIRTUALIZATION_UNKNOWN)
                {
                    Console.Error.WriteLine($"Failed devirtualization with unexpected unknown failure while compiling {MethodBeingCompiled} with decl {decl} targetting type {objType}");
                    Debug.Assert(info->detail != CORINFO_DEVIRTUALIZATION_DETAIL.CORINFO_DEVIRTUALIZATION_UNKNOWN);
                }
#endif
                return false;
            }

            TypeDesc owningType = originalImpl.OwningType;

            // RyuJIT expects to get the canonical form back
            MethodDesc impl = originalImpl.GetCanonMethodTarget(CanonicalFormKind.Specific);

            bool unboxingStub = impl.OwningType.IsValueType;

            MethodDesc nonUnboxingImpl = impl;
            if (unboxingStub)
            {
                impl = getUnboxingThunk(impl);
            }

#if READYTORUN
            // As there are a variety of situations where the resolved virtual method may be different at compile and runtime (primarily due to subtle differences
            // in the virtual resolution algorithm between the runtime and the compiler, although details such as whether or not type equivalence is enabled
            // can also have an effect), record any decisions made, and if there are differences, simply skip use of the compiled method.
            var resolver = _compilation.NodeFactory.Resolver;

            MethodWithToken methodWithTokenDecl;

            if (info->pResolvedTokenVirtualMethod != null)
            {
                methodWithTokenDecl = ComputeMethodWithToken(decl, ref *info->pResolvedTokenVirtualMethod, null, false);
            }
            else
            {
                ModuleToken declToken = resolver.GetModuleTokenForMethod(decl.GetTypicalMethodDefinition(), throwIfNotFound: false);
                if (declToken.IsNull)
                {
                    info->detail = CORINFO_DEVIRTUALIZATION_DETAIL.CORINFO_DEVIRTUALIZATION_FAILED_DECL_NOT_REPRESENTABLE;
                    return false;
                }
                if (!_compilation.CompilationModuleGroup.VersionsWithTypeReference(decl.OwningType))
                {
                    info->detail = CORINFO_DEVIRTUALIZATION_DETAIL.CORINFO_DEVIRTUALIZATION_FAILED_DECL_NOT_REPRESENTABLE;
                    return false;
                }
                methodWithTokenDecl = new MethodWithToken(decl, declToken, null, false, null, devirtualizedMethodOwner: decl.OwningType);
            }
            MethodWithToken methodWithTokenImpl;
#endif

            if (decl == originalImpl)
            {
#if READYTORUN
                methodWithTokenImpl = methodWithTokenDecl;
#endif
                if (info->pResolvedTokenVirtualMethod != null)
                {
                    info->resolvedTokenDevirtualizedMethod = *info->pResolvedTokenVirtualMethod;
                }
                else
                {
                    info->resolvedTokenDevirtualizedMethod = CreateResolvedTokenFromMethod(this, decl
#if READYTORUN
                        , methodWithTokenDecl
#endif
                        );
                }
                info->resolvedTokenDevirtualizedUnboxedMethod = default(CORINFO_RESOLVED_TOKEN);
            }
            else
            {
#if READYTORUN
                methodWithTokenImpl = new MethodWithToken(nonUnboxingImpl, resolver.GetModuleTokenForMethod(nonUnboxingImpl.GetTypicalMethodDefinition()), null, unboxingStub, null, devirtualizedMethodOwner: impl.OwningType);
#endif

                info->resolvedTokenDevirtualizedMethod = CreateResolvedTokenFromMethod(this, impl
#if READYTORUN
                    , methodWithTokenImpl
#endif
                    );

                if (unboxingStub)
                {
                    info->resolvedTokenDevirtualizedUnboxedMethod = info->resolvedTokenDevirtualizedMethod;
                    info->resolvedTokenDevirtualizedUnboxedMethod.tokenContext = contextFromMethod(nonUnboxingImpl);
                    info->resolvedTokenDevirtualizedUnboxedMethod.hMethod = ObjectToHandle(nonUnboxingImpl);
                }
                else
                {
                    info->resolvedTokenDevirtualizedUnboxedMethod = default(CORINFO_RESOLVED_TOKEN);
                }
            }

#if READYTORUN
            // Testing has not shown that concerns about virtual matching are significant
            // Only generate verification for builds with the stress mode enabled
            if (_compilation.SymbolNodeFactory.VerifyTypeAndFieldLayout)
            {
                ISymbolNode virtualResolutionNode = _compilation.SymbolNodeFactory.CheckVirtualFunctionOverride(methodWithTokenDecl, objType, methodWithTokenImpl);
                _methodCodeNode.Fixups.Add(virtualResolutionNode);
            }
#endif
            info->detail = CORINFO_DEVIRTUALIZATION_DETAIL.CORINFO_DEVIRTUALIZATION_SUCCESS;
            info->devirtualizedMethod = ObjectToHandle(impl);
            info->requiresInstMethodTableArg = false;
            info->exactContext = contextFromType(owningType);

            return true;

            static CORINFO_RESOLVED_TOKEN CreateResolvedTokenFromMethod(CorInfoImpl jitInterface, MethodDesc method
#if READYTORUN
                , MethodWithToken methodWithToken
#endif
                )
            {
#if !READYTORUN
                MethodDesc unboxedMethodDesc = method.IsUnboxingThunk() ? method.GetUnboxedMethod() : method;
                var methodWithToken = new
                {
                    Method = unboxedMethodDesc,
                    OwningType = unboxedMethodDesc.OwningType,
                };
#endif

                CORINFO_RESOLVED_TOKEN result = default(CORINFO_RESOLVED_TOKEN);
                MethodILScope scope = jitInterface._compilation.GetMethodIL(methodWithToken.Method);
                if (scope == null)
                {
                    scope = Internal.IL.EcmaMethodILScope.Create((EcmaMethod)methodWithToken.Method.GetTypicalMethodDefinition());
                }
                result.tokenScope = jitInterface.ObjectToHandle(scope);
                result.tokenContext = jitInterface.contextFromMethod(method);
#if READYTORUN
                result.token = methodWithToken.Token.Token;
                if (methodWithToken.Token.TokenType != CorTokenType.mdtMethodDef)
                {
                    Debug.Assert(false); // This should never happen, but we protect against total failure with the throw below.
                    throw new RequiresRuntimeJitException("Attempt to devirtualize and unable to create token for devirtualized method");
                }
#else
                result.token = (mdToken)0x06BAAAAD;
#endif
                result.tokenType = CorInfoTokenKind.CORINFO_TOKENKIND_DevirtualizedMethod;
                result.hClass = jitInterface.ObjectToHandle(methodWithToken.OwningType);
                result.hMethod = jitInterface.ObjectToHandle(method);

                return result;
            }
        }

        private CORINFO_METHOD_STRUCT_* getUnboxedEntry(CORINFO_METHOD_STRUCT_* ftn, ref bool requiresInstMethodTableArg)
        {
            MethodDesc result = null;
            requiresInstMethodTableArg = false;

            MethodDesc method = HandleToObject(ftn);
            if (method.IsUnboxingThunk())
            {
                result = method.GetUnboxedMethod();
                requiresInstMethodTableArg = method.RequiresInstMethodTableArg();
            }

            return result != null ? ObjectToHandle(result) : null;
        }

        private CORINFO_CLASS_STRUCT_* getDefaultComparerClass(CORINFO_CLASS_STRUCT_* elemType)
        {
            TypeDesc comparand = HandleToObject(elemType);
            TypeDesc comparer = IL.Stubs.ComparerIntrinsics.GetComparerForType(comparand);
            return comparer != null ? ObjectToHandle(comparer) : null;
        }

        private CORINFO_CLASS_STRUCT_* getDefaultEqualityComparerClass(CORINFO_CLASS_STRUCT_* elemType)
        {
            TypeDesc comparand = HandleToObject(elemType);
            TypeDesc comparer = IL.Stubs.ComparerIntrinsics.GetEqualityComparerForType(comparand);
            return comparer != null ? ObjectToHandle(comparer) : null;
        }

        private bool isIntrinsicType(CORINFO_CLASS_STRUCT_* classHnd)
        {
            TypeDesc type = HandleToObject(classHnd);
            return type.IsIntrinsic;
        }

        private MethodSignatureFlags PlatformDefaultUnmanagedCallingConvention()
        {
            return _compilation.TypeSystemContext.Target.IsWindows ?
                MethodSignatureFlags.UnmanagedCallingConventionStdCall : MethodSignatureFlags.UnmanagedCallingConventionCdecl;
        }

        private CorInfoCallConvExtension getUnmanagedCallConv(CORINFO_METHOD_STRUCT_* method, CORINFO_SIG_INFO* sig, ref bool pSuppressGCTransition)
        {
            pSuppressGCTransition = false;

            if (method != null)
            {
                MethodDesc methodDesc = HandleToObject(method);
                CorInfoCallConvExtension callConv = GetUnmanagedCallConv(HandleToObject(method), out pSuppressGCTransition);
                return callConv;
            }
            else
            {
                Debug.Assert(sig != null);

                CorInfoCallConvExtension callConv = GetUnmanagedCallConv(HandleToObject(sig->methodSignature), out pSuppressGCTransition);
                return callConv;
            }
        }
        private CorInfoCallConvExtension GetUnmanagedCallConv(MethodDesc methodDesc, out bool suppressGCTransition)
        {
            suppressGCTransition = false;
            MethodSignatureFlags callConv = methodDesc.Signature.Flags & MethodSignatureFlags.UnmanagedCallingConventionMask;
            if (callConv == MethodSignatureFlags.None)
            {
                if (methodDesc.IsPInvoke)
                {
                    suppressGCTransition = methodDesc.HasSuppressGCTransitionAttribute();
                    MethodSignatureFlags unmanagedCallConv = methodDesc.GetPInvokeMethodMetadata().Flags.UnmanagedCallingConvention;

                    if (unmanagedCallConv == MethodSignatureFlags.None)
                    {
                        MethodDesc methodDescLocal = methodDesc;
                        if (methodDesc is IL.Stubs.PInvokeTargetNativeMethod rawPInvoke)
                        {
                            methodDescLocal = rawPInvoke.Target;
                        }

                        CustomAttributeValue<TypeDesc>? unmanagedCallConvAttribute = ((EcmaMethod)methodDescLocal).GetDecodedCustomAttribute("System.Runtime.InteropServices", "UnmanagedCallConvAttribute");
                        if (unmanagedCallConvAttribute != null)
                        {
                            bool suppressGCTransitionLocal;
                            CorInfoCallConvExtension callConvFromAttribute = GetUnmanagedCallingConventionFromAttribute(unmanagedCallConvAttribute.Value, out suppressGCTransitionLocal);
                            suppressGCTransition |= suppressGCTransitionLocal;
                            return callConvFromAttribute;
                        }

                        unmanagedCallConv = PlatformDefaultUnmanagedCallingConvention();
                    }

                    // Verify that it is safe to convert MethodSignatureFlags.UnmanagedCallingConvention to CorInfoCallConvExtension via a simple cast
                    Debug.Assert((int)CorInfoCallConvExtension.C == (int)MethodSignatureFlags.UnmanagedCallingConventionCdecl);
                    Debug.Assert((int)CorInfoCallConvExtension.Stdcall == (int)MethodSignatureFlags.UnmanagedCallingConventionStdCall);
                    Debug.Assert((int)CorInfoCallConvExtension.Thiscall == (int)MethodSignatureFlags.UnmanagedCallingConventionThisCall);

                    return (CorInfoCallConvExtension)unmanagedCallConv;
                }
                else
                {
                    Debug.Assert(methodDesc.IsUnmanagedCallersOnly);
                    CustomAttributeValue<TypeDesc> unmanagedCallersOnlyAttribute = ((EcmaMethod)methodDesc).GetDecodedCustomAttribute("System.Runtime.InteropServices", "UnmanagedCallersOnlyAttribute").Value;
                    return GetUnmanagedCallingConventionFromAttribute(unmanagedCallersOnlyAttribute, out _);
                }
            }
            return GetUnmanagedCallConv(methodDesc.Signature, out suppressGCTransition);
        }

        private CorInfoCallConvExtension GetUnmanagedCallConv(MethodSignature signature, out bool suppressGCTransition)
        {
            suppressGCTransition = false;
            switch (signature.Flags & MethodSignatureFlags.UnmanagedCallingConventionMask)
            {
                case MethodSignatureFlags.None:
                    ThrowHelper.ThrowInvalidProgramException();
                    return CorInfoCallConvExtension.Managed;
                case MethodSignatureFlags.UnmanagedCallingConventionCdecl:
                    return CorInfoCallConvExtension.C;
                case MethodSignatureFlags.UnmanagedCallingConventionStdCall:
                    return CorInfoCallConvExtension.Stdcall;
                case MethodSignatureFlags.UnmanagedCallingConventionThisCall:
                    return CorInfoCallConvExtension.Thiscall;
                case MethodSignatureFlags.UnmanagedCallingConvention:
                    if (TryGetUnmanagedCallingConventionFromModOpt(signature, out CorInfoCallConvExtension callConvMaybe, out suppressGCTransition))
                    {
                        return callConvMaybe;
                    }
                    else
                    {
                        return (CorInfoCallConvExtension)PlatformDefaultUnmanagedCallingConvention();
                    }
                default:
                    ThrowHelper.ThrowInvalidProgramException();
                    return CorInfoCallConvExtension.Managed;
            }
        }

        private bool satisfiesMethodConstraints(CORINFO_CLASS_STRUCT_* parent, CORINFO_METHOD_STRUCT_* method)
        { throw new NotImplementedException("satisfiesMethodConstraints"); }
        private bool isCompatibleDelegate(CORINFO_CLASS_STRUCT_* objCls, CORINFO_CLASS_STRUCT_* methodParentCls, CORINFO_METHOD_STRUCT_* method, CORINFO_CLASS_STRUCT_* delegateCls, ref bool pfIsOpenDelegate)
        { throw new NotImplementedException("isCompatibleDelegate"); }
        private void setPatchpointInfo(PatchpointInfo* patchpointInfo)
        { throw new NotImplementedException("setPatchpointInfo"); }
        private PatchpointInfo* getOSRInfo(ref uint ilOffset)
        { throw new NotImplementedException("getOSRInfo"); }

        private void methodMustBeLoadedBeforeCodeIsRun(CORINFO_METHOD_STRUCT_* method)
        {
        }

        private CORINFO_METHOD_STRUCT_* mapMethodDeclToMethodImpl(CORINFO_METHOD_STRUCT_* method)
        { throw new NotImplementedException("mapMethodDeclToMethodImpl"); }

        private static object ResolveTokenWithSubstitution(MethodILScope methodIL, mdToken token, Instantiation typeInst, Instantiation methodInst)
        {
            // Grab the generic definition of the method IL, resolve the token within the definition,
            // and instantiate it with the given context.
            object result = methodIL.GetMethodILScopeDefinition().GetObject((int)token);

            if (result is MethodDesc methodResult)
            {
                result = methodResult.InstantiateSignature(typeInst, methodInst);
            }
            else if (result is FieldDesc fieldResult)
            {
                result = fieldResult.InstantiateSignature(typeInst, methodInst);
            }
            else
            {
                result = ((TypeDesc)result).InstantiateSignature(typeInst, methodInst);
            }

            return result;
        }

        private static object ResolveTokenInScope(MethodILScope methodIL, object typeOrMethodContext, mdToken token)
        {
            MethodDesc owningMethod = methodIL.OwningMethod;

            // If token context differs from the scope, it means we're inlining.
            // If we're inlining a shared method body, we might be able to un-share
            // the referenced token and avoid runtime lookups.
            // Resolve the token in the inlining context.

            object result;
            if (owningMethod != typeOrMethodContext &&
                owningMethod.IsCanonicalMethod(CanonicalFormKind.Any))
            {
                Instantiation typeInst = default;
                Instantiation methodInst = default;

                if (typeOrMethodContext is TypeDesc typeContext)
                {
                    Debug.Assert(typeContext.HasSameTypeDefinition(owningMethod.OwningType) || typeContext.IsArray);
                    typeInst = typeContext.Instantiation;
                }
                else
                {
                    var methodContext = (MethodDesc)typeOrMethodContext;
                    // Allow cases where the method's do not have instantiations themselves, if
                    // 1. The method defining the context is generic, but the target method is not
                    // 2. Both methods are not generic
                    // 3. The methods are the same generic
                    // AND
                    // The methods are on the same type
                    Debug.Assert((methodContext.HasInstantiation && !owningMethod.HasInstantiation) ||
                        (!methodContext.HasInstantiation && !owningMethod.HasInstantiation) ||
                        methodContext.GetTypicalMethodDefinition() == owningMethod.GetTypicalMethodDefinition() ||
                        (owningMethod.Name == "CreateDefaultInstance" && methodContext.Name == "CreateInstance"));
                    Debug.Assert(methodContext.OwningType.HasSameTypeDefinition(owningMethod.OwningType));
                    typeInst = methodContext.OwningType.Instantiation;
                    methodInst = methodContext.Instantiation;
                }

                result = ResolveTokenWithSubstitution(methodIL, token, typeInst, methodInst);
            }
            else
            {
                // Not inlining - just resolve the token within the methodIL
                result = methodIL.GetObject((int)token);
            }

            return result;
        }

        private object GetRuntimeDeterminedObjectForToken(ref CORINFO_RESOLVED_TOKEN pResolvedToken)
        {
            // Since RyuJIT operates on canonical types (as opposed to runtime determined ones), but the
            // dependency analysis operates on runtime determined ones, we convert the resolved token
            // to the runtime determined form (e.g. Foo<__Canon> becomes Foo<T__Canon>).

            var methodIL = HandleToObject(pResolvedToken.tokenScope);

            var typeOrMethodContext = (pResolvedToken.tokenContext == contextFromMethodBeingCompiled()) ?
                MethodBeingCompiled : HandleToObject((IntPtr)pResolvedToken.tokenContext);

            object result = GetRuntimeDeterminedObjectForToken(methodIL, typeOrMethodContext, pResolvedToken.token);
            if (pResolvedToken.tokenType == CorInfoTokenKind.CORINFO_TOKENKIND_Newarr)
                result = ((TypeDesc)result).MakeArrayType();

            return result;
        }

        private object GetRuntimeDeterminedObjectForToken(MethodILScope methodIL, object typeOrMethodContext, mdToken token)
        {
            object result = ResolveTokenInScope(methodIL, typeOrMethodContext, token);

            if (result is MethodDesc method)
            {
                if (method.IsSharedByGenericInstantiations)
                {
                    MethodDesc sharedMethod = methodIL.OwningMethod.GetSharedRuntimeFormMethodTarget();
                    result = ResolveTokenWithSubstitution(methodIL, token, sharedMethod.OwningType.Instantiation, sharedMethod.Instantiation);
                    Debug.Assert(((MethodDesc)result).IsRuntimeDeterminedExactMethod);
                }
            }
            else if (result is FieldDesc field)
            {
                if (field.OwningType.IsCanonicalSubtype(CanonicalFormKind.Any))
                {
                    MethodDesc sharedMethod = methodIL.OwningMethod.GetSharedRuntimeFormMethodTarget();
                    result = ResolveTokenWithSubstitution(methodIL, token, sharedMethod.OwningType.Instantiation, sharedMethod.Instantiation);
                    Debug.Assert(((FieldDesc)result).OwningType.IsRuntimeDeterminedSubtype);
                }
            }
            else
            {
                TypeDesc type = (TypeDesc)result;
                if (type.IsCanonicalSubtype(CanonicalFormKind.Any))
                {
                    MethodDesc sharedMethod = methodIL.OwningMethod.GetSharedRuntimeFormMethodTarget();
                    result = ResolveTokenWithSubstitution(methodIL, token, sharedMethod.OwningType.Instantiation, sharedMethod.Instantiation);
                    Debug.Assert(((TypeDesc)result).IsRuntimeDeterminedSubtype ||
                        /* If the resolved type is not runtime determined there's a chance we went down this path
                           because there was a literal typeof(__Canon) in the compiled IL - check for that
                           by resolving the token in the definition. */
                        ((TypeDesc)methodIL.GetMethodILScopeDefinition().GetObject((int)token)).IsCanonicalDefinitionType(CanonicalFormKind.Any));
                }
            }

            return result;
        }

        private void resolveToken(ref CORINFO_RESOLVED_TOKEN pResolvedToken)
        {
            var methodIL = HandleToObject(pResolvedToken.tokenScope);

            var typeOrMethodContext = (pResolvedToken.tokenContext == contextFromMethodBeingCompiled()) ?
                MethodBeingCompiled : HandleToObject((IntPtr)pResolvedToken.tokenContext);

            object result = ResolveTokenInScope(methodIL, typeOrMethodContext, pResolvedToken.token);

            pResolvedToken.hClass = null;
            pResolvedToken.hMethod = null;
            pResolvedToken.hField = null;

#if READYTORUN
            TypeDesc owningType = methodIL.OwningMethod.GetTypicalMethodDefinition().OwningType;
            bool recordToken = _compilation.CompilationModuleGroup.VersionsWithType(owningType) && owningType is EcmaType;
#endif

            if (result is MethodDesc method)
            {
                pResolvedToken.hMethod = ObjectToHandle(method);

                TypeDesc owningClass = method.OwningType;
                pResolvedToken.hClass = ObjectToHandle(owningClass);

#if !SUPPORT_JIT
                _compilation.TypeSystemContext.EnsureLoadableMethod(method);
#endif

#if READYTORUN
                if (recordToken)
                {
                    ModuleToken methodModuleToken = HandleToModuleToken(ref pResolvedToken);
                    var resolver = _compilation.NodeFactory.Resolver;
                    resolver.AddModuleTokenForMethod(method, methodModuleToken);
                }
#else
                _compilation.NodeFactory.MetadataManager.GetDependenciesDueToAccess(ref _additionalDependencies, _compilation.NodeFactory, (MethodIL)methodIL, method);
#endif
            }
            else
            if (result is FieldDesc)
            {
                FieldDesc field = result as FieldDesc;

                // References to literal fields from IL body should never resolve.
                // The CLR would throw a MissingFieldException while jitting and so should we.
                if (field.IsLiteral)
                    ThrowHelper.ThrowMissingFieldException(field.OwningType, field.Name);

                pResolvedToken.hField = ObjectToHandle(field);

                TypeDesc owningClass = field.OwningType;
                pResolvedToken.hClass = ObjectToHandle(owningClass);

#if !SUPPORT_JIT
                _compilation.TypeSystemContext.EnsureLoadableType(owningClass);
#endif

#if READYTORUN
                if (recordToken)
                {
                    _compilation.NodeFactory.Resolver.AddModuleTokenForField(field, HandleToModuleToken(ref pResolvedToken));
                }
#else
                _compilation.NodeFactory.MetadataManager.GetDependenciesDueToAccess(ref _additionalDependencies, _compilation.NodeFactory, (MethodIL)methodIL, field);
#endif
            }
            else
            {
                TypeDesc type = (TypeDesc)result;

#if READYTORUN
                if (recordToken)
                {
                    _compilation.NodeFactory.Resolver.AddModuleTokenForType(type, HandleToModuleToken(ref pResolvedToken));
                }
#endif

                if (pResolvedToken.tokenType == CorInfoTokenKind.CORINFO_TOKENKIND_Newarr)
                {
                    if (type.IsVoid)
                        ThrowHelper.ThrowInvalidProgramException(ExceptionStringID.InvalidProgramSpecific, methodIL.OwningMethod);

                    type = type.MakeArrayType();
                }
                pResolvedToken.hClass = ObjectToHandle(type);

#if !SUPPORT_JIT
                _compilation.TypeSystemContext.EnsureLoadableType(type);
#endif
            }

            pResolvedToken.pTypeSpec = null;
            pResolvedToken.cbTypeSpec = 0;
            pResolvedToken.pMethodSpec = null;
            pResolvedToken.cbMethodSpec = 0;
        }

        private bool tryResolveToken(ref CORINFO_RESOLVED_TOKEN pResolvedToken)
        {
            resolveToken(ref pResolvedToken);
            return true;
        }

        private void findSig(CORINFO_MODULE_STRUCT_* module, uint sigTOK, CORINFO_CONTEXT_STRUCT* context, CORINFO_SIG_INFO* sig)
        {
            var methodIL = HandleToObject(module);
            var methodSig = (MethodSignature)methodIL.GetObject((int)sigTOK);

            Get_CORINFO_SIG_INFO(methodSig, sig, methodIL);

#if !READYTORUN
            // Check whether we need to report this as a fat pointer call
            if (_compilation.IsFatPointerCandidate(methodIL.OwningMethod, methodSig))
            {
                sig->flags |= CorInfoSigInfoFlags.CORINFO_SIGFLAG_FAT_CALL;
            }
#else
            VerifyMethodSignatureIsStable(methodSig);
#endif
        }

        private void findCallSiteSig(CORINFO_MODULE_STRUCT_* module, uint methTOK, CORINFO_CONTEXT_STRUCT* context, CORINFO_SIG_INFO* sig)
        {
            var methodIL = HandleToObject(module);
            Get_CORINFO_SIG_INFO(((MethodDesc)methodIL.GetObject((int)methTOK)), sig: sig, methodIL);
        }

        private CORINFO_CLASS_STRUCT_* getTokenTypeAsHandle(ref CORINFO_RESOLVED_TOKEN pResolvedToken)
        {
            WellKnownType result = WellKnownType.RuntimeTypeHandle;

            if (pResolvedToken.hMethod != null)
            {
                result = WellKnownType.RuntimeMethodHandle;
            }
            else
            if (pResolvedToken.hField != null)
            {
                result = WellKnownType.RuntimeFieldHandle;
            }

            return ObjectToHandle(_compilation.TypeSystemContext.GetWellKnownType(result));
        }

        private CorInfoCanSkipVerificationResult canSkipVerification(CORINFO_MODULE_STRUCT_* module)
        {
            return CorInfoCanSkipVerificationResult.CORINFO_VERIFICATION_CAN_SKIP;
        }

        private bool isValidToken(CORINFO_MODULE_STRUCT_* module, uint metaTOK)
        { throw new NotImplementedException("isValidToken"); }
        private bool isValidStringRef(CORINFO_MODULE_STRUCT_* module, uint metaTOK)
        { throw new NotImplementedException("isValidStringRef"); }

        private int getStringLiteral(CORINFO_MODULE_STRUCT_* module, uint metaTOK, char* buffer, int size)
        {
            Debug.Assert(size >= 0);

            MethodILScope methodIL = HandleToObject(module);
            string str = (string)methodIL.GetObject((int)metaTOK);

            if (buffer != null)
            {
                // Copy str's content to buffer
                str.AsSpan(0, Math.Min(size, str.Length)).CopyTo(new Span<char>(buffer, size));
            }
            return str.Length;
        }

        private CorInfoType asCorInfoType(CORINFO_CLASS_STRUCT_* cls)
        {
            var type = HandleToObject(cls);
            return asCorInfoType(type);
        }

        private byte* getClassName(CORINFO_CLASS_STRUCT_* cls)
        {
            var type = HandleToObject(cls);
            StringBuilder nameBuilder = new StringBuilder();
            TypeString.Instance.AppendName(nameBuilder, type);
            return (byte*)GetPin(StringToUTF8(nameBuilder.ToString()));
        }

        private byte* getClassNameFromMetadata(CORINFO_CLASS_STRUCT_* cls, byte** namespaceName)
        {
            var type = HandleToObject(cls) as MetadataType;
            if (type != null)
            {
                if (namespaceName != null)
                    *namespaceName = (byte*)GetPin(StringToUTF8(type.Namespace));
                return (byte*)GetPin(StringToUTF8(type.Name));
            }

            if (namespaceName != null)
                *namespaceName = null;
            return null;
        }

        private CORINFO_CLASS_STRUCT_* getTypeInstantiationArgument(CORINFO_CLASS_STRUCT_* cls, uint index)
        {
            TypeDesc type = HandleToObject(cls);
            Instantiation inst = type.Instantiation;

            return index < (uint)inst.Length ? ObjectToHandle(inst[(int)index]) : null;
        }


        private int appendClassName(char** ppBuf, ref int pnBufLen, CORINFO_CLASS_STRUCT_* cls, bool fNamespace, bool fFullInst, bool fAssembly)
        {
            // We support enough of this to make SIMD work, but not much else.

            Debug.Assert(fNamespace && !fFullInst && !fAssembly);

            var type = HandleToObject(cls);
            string name = TypeString.Instance.FormatName(type);

            int length = name.Length;
            if (pnBufLen > 0)
            {
                char* buffer = *ppBuf;
                int lengthToCopy = Math.Min(name.Length, pnBufLen);
                for (int i = 0; i < lengthToCopy; i++)
                    buffer[i] = name[i];
                if (name.Length < pnBufLen)
                {
                    buffer[name.Length] = (char)0;
                }
                else
                {
                    buffer[pnBufLen - 1] = (char)0;
                    lengthToCopy -= 1;
                }

                pnBufLen -= lengthToCopy;
                *ppBuf = buffer + lengthToCopy;
            }

            return length;
        }

        private bool isValueClass(CORINFO_CLASS_STRUCT_* cls)
        {
            return HandleToObject(cls).IsValueType;
        }

        private CorInfoInlineTypeCheck canInlineTypeCheck(CORINFO_CLASS_STRUCT_* cls, CorInfoInlineTypeCheckSource source)
        {
            // TODO: when we support multiple modules at runtime, this will need to do more work
            // NOTE: cls can be null
            return CorInfoInlineTypeCheck.CORINFO_INLINE_TYPECHECK_PASS;
        }

        private uint getClassAttribs(CORINFO_CLASS_STRUCT_* cls)
        {
            TypeDesc type = HandleToObject(cls);
            return getClassAttribsInternal(type);
        }

        private uint getClassAttribsInternal(TypeDesc type)
        {
            // TODO: Support for verification (CORINFO_FLG_GENERIC_TYPE_VARIABLE)

            CorInfoFlag result = (CorInfoFlag)0;

            var metadataType = type as MetadataType;

            // The array flag is used to identify the faked-up methods on
            // array types, i.e. .ctor, Get, Set and Address
            if (type.IsArray)
                result |= CorInfoFlag.CORINFO_FLG_ARRAY;

            if (type.IsInterface)
                result |= CorInfoFlag.CORINFO_FLG_INTERFACE;

            if (type.IsArray || type.IsString)
                result |= CorInfoFlag.CORINFO_FLG_VAROBJSIZE;

            if (type.IsValueType)
            {
                result |= CorInfoFlag.CORINFO_FLG_VALUECLASS;

                if (metadataType.IsByRefLike)
                    result |= CorInfoFlag.CORINFO_FLG_BYREF_LIKE;

                // The CLR has more complicated rules around CUSTOMLAYOUT, but this will do.
                if (metadataType.IsExplicitLayout || (metadataType.IsSequentialLayout && metadataType.GetClassLayout().Size != 0) || metadataType.IsWellKnownType(WellKnownType.TypedReference))
                    result |= CorInfoFlag.CORINFO_FLG_CUSTOMLAYOUT;

                if (metadataType.IsUnsafeValueType)
                    result |= CorInfoFlag.CORINFO_FLG_UNSAFE_VALUECLASS;
            }

            if (type.IsCanonicalSubtype(CanonicalFormKind.Any))
                result |= CorInfoFlag.CORINFO_FLG_SHAREDINST;

            if (type.HasVariance)
                result |= CorInfoFlag.CORINFO_FLG_VARIANCE;

            if (type.IsDelegate)
                result |= CorInfoFlag.CORINFO_FLG_DELEGATE;

            if (_compilation.IsEffectivelySealed(type))
                result |= CorInfoFlag.CORINFO_FLG_FINAL;

            if (type.IsIntrinsic)
                result |= CorInfoFlag.CORINFO_FLG_INTRINSIC_TYPE;

            if (metadataType != null)
            {
                if (metadataType.ContainsGCPointers)
                    result |= CorInfoFlag.CORINFO_FLG_CONTAINS_GC_PTR;

                if (metadataType.IsBeforeFieldInit)
                    result |= CorInfoFlag.CORINFO_FLG_BEFOREFIELDINIT;

                // Assume overlapping fields for explicit layout.
                if (metadataType.IsExplicitLayout)
                    result |= CorInfoFlag.CORINFO_FLG_OVERLAPPING_FIELDS;

                if (metadataType.IsAbstract)
                    result |= CorInfoFlag.CORINFO_FLG_ABSTRACT;
            }

#if READYTORUN
            if (!_compilation.CompilationModuleGroup.VersionsWithType(type))
            {
                // Prevent the JIT from drilling into types outside of the current versioning bubble
                result |= CorInfoFlag.CORINFO_FLG_DONT_DIG_FIELDS;
                result &= ~CorInfoFlag.CORINFO_FLG_BEFOREFIELDINIT;
            }
#endif

            return (uint)result;
        }

        private CORINFO_MODULE_STRUCT_* getClassModule(CORINFO_CLASS_STRUCT_* cls)
        { throw new NotImplementedException("getClassModule"); }
        private CORINFO_ASSEMBLY_STRUCT_* getModuleAssembly(CORINFO_MODULE_STRUCT_* mod)
        { throw new NotImplementedException("getModuleAssembly"); }
        private byte* getAssemblyName(CORINFO_ASSEMBLY_STRUCT_* assem)
        { throw new NotImplementedException("getAssemblyName"); }

        private void* LongLifetimeMalloc(UIntPtr sz)
        {
            return (void*)Marshal.AllocCoTaskMem((int)sz);
        }

        private void LongLifetimeFree(void* obj)
        {
            Marshal.FreeCoTaskMem((IntPtr)obj);
        }

        private UIntPtr getClassModuleIdForStatics(CORINFO_CLASS_STRUCT_* cls, CORINFO_MODULE_STRUCT_** pModule, void** ppIndirection)
        { throw new NotImplementedException("getClassModuleIdForStatics"); }

        private uint getClassSize(CORINFO_CLASS_STRUCT_* cls)
        {
            TypeDesc type = HandleToObject(cls);
            LayoutInt classSize = type.GetElementSize();
#if READYTORUN
            if (classSize.IsIndeterminate)
            {
                throw new RequiresRuntimeJitException(type);
            }

            if (NeedsTypeLayoutCheck(type))
            {
                ISymbolNode node = _compilation.SymbolNodeFactory.CheckTypeLayout(type);
                _methodCodeNode.Fixups.Add(node);
            }
#endif
            return (uint)classSize.AsInt;
        }

        private uint getHeapClassSize(CORINFO_CLASS_STRUCT_* cls)
        {
            TypeDesc type = HandleToObject(cls);

            Debug.Assert(!type.IsValueType);
            Debug.Assert(type.IsDefType);
            Debug.Assert(!type.IsString);
#if READYTORUN
            Debug.Assert(_compilation.IsInheritanceChainLayoutFixedInCurrentVersionBubble(type));
#endif

            return (uint)((DefType)type).InstanceByteCount.AsInt;
        }

        private bool canAllocateOnStack(CORINFO_CLASS_STRUCT_* cls)
        {
            TypeDesc type = HandleToObject(cls);

            Debug.Assert(!type.IsValueType);
            Debug.Assert(type.IsDefType);

            bool result = !type.HasFinalizer;

#if READYTORUN
            if (!_compilation.IsInheritanceChainLayoutFixedInCurrentVersionBubble(type))
                result = false;
#endif

            return result;
        }

        /// <summary>
        /// Managed implementation of CEEInfo::getClassAlignmentRequirementStatic
        /// </summary>
        public static int GetClassAlignmentRequirementStatic(DefType type)
        {
            int alignment = type.Context.Target.PointerSize;

            if (type is MetadataType metadataType && metadataType.HasLayout())
            {
                if (metadataType.IsSequentialLayout || MarshalUtils.IsBlittableType(metadataType))
                {
                    alignment = metadataType.InstanceFieldAlignment.AsInt;
                }
            }

            if (type.Context.Target.Architecture == TargetArchitecture.ARM &&
                alignment < 8 && type.RequiresAlign8())
            {
                // If the structure contains 64-bit primitive fields and the platform requires 8-byte alignment for
                // such fields then make sure we return at least 8-byte alignment. Note that it's technically possible
                // to create unmanaged APIs that take unaligned structures containing such fields and this
                // unconditional alignment bump would cause us to get the calling convention wrong on platforms such
                // as ARM. If we see such cases in the future we'd need to add another control (such as an alignment
                // property for the StructLayout attribute or a marshaling directive attribute for p/invoke arguments)
                // that allows more precise control. For now we'll go with the likely scenario.
                alignment = 8;
            }

            return alignment;
        }

        private Dictionary<DefType, bool> _doubleAlignHeuristicCache = new Dictionary<DefType, bool>();

        //*******************************************************************************
        //
        // Heuristic to determine if we should have instances of this class 8 byte aligned
        //
        static bool ShouldAlign8(int dwR8Fields, int dwTotalFields)
        {
            return dwR8Fields*2>dwTotalFields && dwR8Fields>=2;
        }

        static bool ShouldAlign8(DefType type)
        {
            int instanceFields = 0;
            int doubleFields = 0;
            var doubleType = type.Context.GetWellKnownType(WellKnownType.Double);
            foreach (var field in type.GetFields())
            {
                if (field.IsStatic)
                    continue;

                instanceFields++;

                if (field.FieldType == doubleType)
                    doubleFields++;
            }

            return ShouldAlign8(doubleFields, instanceFields);
        }

        private uint getClassAlignmentRequirement(CORINFO_CLASS_STRUCT_* cls, bool fDoubleAlignHint)
        {
            DefType type = (DefType)HandleToObject(cls);


            var target = type.Context.Target;
            if (fDoubleAlignHint)
            {
                if (target.Architecture == TargetArchitecture.X86)
                {
                    if ((type.IsValueType) && (type.InstanceFieldAlignment.AsInt > 4))
                    {
                        // On X86, double aligning the stack is expensive. if fDoubleAlignHint is true
                        // only align the local variable if it has a large enough fraction of double fields
                        // in comparison to the total field count.
                        if (!_doubleAlignHeuristicCache.TryGetValue(type, out bool doDoubleAlign))
                        {
                            doDoubleAlign = ShouldAlign8(type);
                            _doubleAlignHeuristicCache.Add(type, doDoubleAlign);
                        }

                        // Return the size of the double align hint. Ignore the actual alignment info account
                        // so that structs with 64-bit integer fields do not trigger double aligned frames on x86.
                        if (doDoubleAlign)
                            return 8;
                    }

                    return (uint)target.PointerSize;
                }
            }

            return (uint)GetClassAlignmentRequirementStatic(type);
        }

        private int MarkGcField(byte* gcPtrs, CorInfoGCType gcType)
        {
            // Ensure that if we have multiple fields with the same offset,
            // that we don't double count the data in the gc layout.
            if (*gcPtrs == (byte)CorInfoGCType.TYPE_GC_NONE)
            {
                *gcPtrs = (byte)gcType;
                return 1;
            }
            else
            {
                Debug.Assert(*gcPtrs == (byte)gcType);
                return 0;
            }
        }

        private int GatherClassGCLayout(TypeDesc type, byte* gcPtrs)
        {
            int result = 0;

            if (type.IsByReferenceOfT)
            {
                return MarkGcField(gcPtrs, CorInfoGCType.TYPE_GC_BYREF);
            }

            foreach (var field in type.GetFields())
            {
                if (field.IsStatic)
                    continue;

                CorInfoGCType gcType = CorInfoGCType.TYPE_GC_NONE;

                var fieldType = field.FieldType;
                if (fieldType.IsValueType)
                {
                    var fieldDefType = (DefType)fieldType;
                    if (!fieldDefType.ContainsGCPointers && !fieldDefType.IsByRefLike)
                        continue;

                    gcType = CorInfoGCType.TYPE_GC_OTHER;
                }
                else if (fieldType.IsGCPointer)
                {
                    gcType = CorInfoGCType.TYPE_GC_REF;
                }
                else if (fieldType.IsByRef)
                {
                    gcType = CorInfoGCType.TYPE_GC_BYREF;
                }
                else
                {
                    continue;
                }

                Debug.Assert(field.Offset.AsInt % PointerSize == 0);
                byte* fieldGcPtrs = gcPtrs + field.Offset.AsInt / PointerSize;

                if (gcType == CorInfoGCType.TYPE_GC_OTHER)
                {
                    result += GatherClassGCLayout(fieldType, fieldGcPtrs);
                }
                else
                {
                    result += MarkGcField(fieldGcPtrs, gcType);
                }
            }
            return result;
        }

        private uint getClassGClayout(CORINFO_CLASS_STRUCT_* cls, byte* gcPtrs)
        {
            uint result = 0;

            DefType type = (DefType)HandleToObject(cls);

            int pointerSize = PointerSize;

            int ptrsCount = AlignmentHelper.AlignUp(type.InstanceFieldSize.AsInt, pointerSize) / pointerSize;

            // Assume no GC pointers at first
            for (int i = 0; i < ptrsCount; i++)
                gcPtrs[i] = (byte)CorInfoGCType.TYPE_GC_NONE;

            if (type.ContainsGCPointers || type.IsByRefLike)
            {
                result = (uint)GatherClassGCLayout(type, gcPtrs);
            }
            return result;
        }

        private uint getClassNumInstanceFields(CORINFO_CLASS_STRUCT_* cls)
        {
            TypeDesc type = HandleToObject(cls);

            uint result = 0;
            foreach (var field in type.GetFields())
            {
                if (!field.IsStatic)
                    result++;
            }

            return result;
        }

        private CORINFO_FIELD_STRUCT_* getFieldInClass(CORINFO_CLASS_STRUCT_* clsHnd, int num)
        {
            TypeDesc classWithFields = HandleToObject(clsHnd);

            int iCurrentFoundField = -1;
            foreach (var field in classWithFields.GetFields())
            {
                if (field.IsStatic)
                    continue;

                ++iCurrentFoundField;
                if (iCurrentFoundField == num)
                {
                    return ObjectToHandle(field);
                }
            }

            // We could not find the field that was searched for.
            throw new InvalidOperationException();
        }

        private bool checkMethodModifier(CORINFO_METHOD_STRUCT_* hMethod, byte* modifier, bool fOptional)
        { throw new NotImplementedException("checkMethodModifier"); }

        private CorInfoHelpFunc getSharedCCtorHelper(CORINFO_CLASS_STRUCT_* clsHnd)
        { throw new NotImplementedException("getSharedCCtorHelper"); }

        private CORINFO_CLASS_STRUCT_* getTypeForBox(CORINFO_CLASS_STRUCT_* cls)
        {
            var type = HandleToObject(cls);

            var typeForBox = type.IsNullable ? type.Instantiation[0] : type;

            return ObjectToHandle(typeForBox);
        }

        private CorInfoHelpFunc getBoxHelper(CORINFO_CLASS_STRUCT_* cls)
        {
            var type = HandleToObject(cls);

            if (type.IsByRefLike)
                ThrowHelper.ThrowInvalidProgramException(ExceptionStringID.InvalidProgramSpecific, MethodBeingCompiled);

            return type.IsNullable ? CorInfoHelpFunc.CORINFO_HELP_BOX_NULLABLE : CorInfoHelpFunc.CORINFO_HELP_BOX;
        }

        private CorInfoHelpFunc getUnBoxHelper(CORINFO_CLASS_STRUCT_* cls)
        {
            var type = HandleToObject(cls);

            return type.IsNullable ? CorInfoHelpFunc.CORINFO_HELP_UNBOX_NULLABLE : CorInfoHelpFunc.CORINFO_HELP_UNBOX;
        }

        private byte* getHelperName(CorInfoHelpFunc helpFunc)
        {
            return (byte*)GetPin(StringToUTF8(helpFunc.ToString()));
        }

        private CorInfoInitClassResult initClass(CORINFO_FIELD_STRUCT_* field, CORINFO_METHOD_STRUCT_* method, CORINFO_CONTEXT_STRUCT* context)
        {
            FieldDesc fd = field == null ? null : HandleToObject(field);
            Debug.Assert(fd == null || fd.IsStatic);

            MethodDesc md = method == null ? MethodBeingCompiled : HandleToObject(method);
            TypeDesc type = fd != null ? fd.OwningType : typeFromContext(context);

            if (
#if READYTORUN
                IsClassPreInited(type)
#else
                _isFallbackBodyCompilation ||
                !_compilation.HasLazyStaticConstructor(type)
#endif
                )
            {
                return CorInfoInitClassResult.CORINFO_INITCLASS_NOT_REQUIRED;
            }

            MetadataType typeToInit = (MetadataType)type;

            if (fd == null)
            {
                if (typeToInit.IsBeforeFieldInit)
                {
                    // We can wait for field accesses to run .cctor
                    return CorInfoInitClassResult.CORINFO_INITCLASS_NOT_REQUIRED;
                }

                // Run .cctor on statics & constructors
                if (md.Signature.IsStatic)
                {
                    // Except don't class construct on .cctor - it would be circular
                    if (md.IsStaticConstructor)
                    {
                        return CorInfoInitClassResult.CORINFO_INITCLASS_NOT_REQUIRED;
                    }
                }
                else if (!md.IsConstructor && !typeToInit.IsValueType && !typeToInit.IsInterface)
                {
                    // According to the spec, we should be able to do this optimization for both reference and valuetypes.
                    // To maintain backward compatibility, we are doing it for reference types only.
                    // We don't do this for interfaces though, as those don't have instance constructors.
                    // For instance methods of types with precise-initialization
                    // semantics, we can assume that the .ctor triggerred the
                    // type initialization.
                    // This does not hold for NULL "this" object. However, the spec does
                    // not require that case to work.
                    return CorInfoInitClassResult.CORINFO_INITCLASS_NOT_REQUIRED;
                }
            }

            if (typeToInit.IsCanonicalSubtype(CanonicalFormKind.Any))
            {
                if (fd == null && method != null && context == contextFromMethodBeingCompiled())
                {
                    // If we're inling a call to a method in our own type, then we should already
                    // have triggered the .cctor when caller was itself called.
                    return CorInfoInitClassResult.CORINFO_INITCLASS_NOT_REQUIRED;
                }

                // Shared generic code has to use helper. Moreover, tell JIT not to inline since
                // inlining of generic dictionary lookups is not supported.
                return CorInfoInitClassResult.CORINFO_INITCLASS_USE_HELPER | CorInfoInitClassResult.CORINFO_INITCLASS_DONT_INLINE;
            }

            //
            // Try to prove that the initialization is not necessary because of nesting
            //

            if (fd == null)
            {
                // Handled above
                Debug.Assert(!typeToInit.IsBeforeFieldInit);

                if (method != null && typeToInit == MethodBeingCompiled.OwningType)
                {
                    // If we're inling a call to a method in our own type, then we should already
                    // have triggered the .cctor when caller was itself called.
                    return CorInfoInitClassResult.CORINFO_INITCLASS_NOT_REQUIRED;
                }
            }
            else
            {
                // This optimization may cause static fields in reference types to be accessed without cctor being triggered
                // for NULL "this" object. It does not conform with what the spec says. However, we have been historically
                // doing it for perf reasons.
                if (!typeToInit.IsValueType && !typeToInit.IsInterface && !typeToInit.IsBeforeFieldInit)
                {
                    if (typeToInit == typeFromContext(context) || typeToInit == MethodBeingCompiled.OwningType)
                    {
                        // The class will be initialized by the time we access the field.
                        return CorInfoInitClassResult.CORINFO_INITCLASS_NOT_REQUIRED;
                    }
                }

                // If we are currently compiling the class constructor for this static field access then we can skip the initClass
                if (MethodBeingCompiled.OwningType == typeToInit && MethodBeingCompiled.IsStaticConstructor)
                {
                    // The class will be initialized by the time we access the field.
                    return CorInfoInitClassResult.CORINFO_INITCLASS_NOT_REQUIRED;
                }
            }

            return CorInfoInitClassResult.CORINFO_INITCLASS_USE_HELPER;
        }

        private CORINFO_CLASS_STRUCT_* getBuiltinClass(CorInfoClassId classId)
        {
            switch (classId)
            {
                case CorInfoClassId.CLASSID_SYSTEM_OBJECT:
                    return ObjectToHandle(_compilation.TypeSystemContext.GetWellKnownType(WellKnownType.Object));

                case CorInfoClassId.CLASSID_TYPED_BYREF:
                    return ObjectToHandle(_compilation.TypeSystemContext.GetWellKnownType(WellKnownType.TypedReference));

                case CorInfoClassId.CLASSID_TYPE_HANDLE:
                    return ObjectToHandle(_compilation.TypeSystemContext.GetWellKnownType(WellKnownType.RuntimeTypeHandle));

                case CorInfoClassId.CLASSID_FIELD_HANDLE:
                    return ObjectToHandle(_compilation.TypeSystemContext.GetWellKnownType(WellKnownType.RuntimeFieldHandle));

                case CorInfoClassId.CLASSID_METHOD_HANDLE:
                    return ObjectToHandle(_compilation.TypeSystemContext.GetWellKnownType(WellKnownType.RuntimeMethodHandle));

                case CorInfoClassId.CLASSID_ARGUMENT_HANDLE:
                    ThrowHelper.ThrowTypeLoadException("System", "RuntimeArgumentHandle", _compilation.TypeSystemContext.SystemModule);
                    return null;

                case CorInfoClassId.CLASSID_STRING:
                    return ObjectToHandle(_compilation.TypeSystemContext.GetWellKnownType(WellKnownType.String));

                case CorInfoClassId.CLASSID_RUNTIME_TYPE:
                    return ObjectToHandle(_compilation.TypeSystemContext.SystemModule.GetKnownType("System", "RuntimeType"));

                default:
                    throw new NotImplementedException();
            }
        }

        private CorInfoType getTypeForPrimitiveValueClass(CORINFO_CLASS_STRUCT_* cls)
        {
            var type = HandleToObject(cls);

            if (!type.IsPrimitive && !type.IsEnum)
                return CorInfoType.CORINFO_TYPE_UNDEF;

            return asCorInfoType(type);
        }

        private CorInfoType getTypeForPrimitiveNumericClass(CORINFO_CLASS_STRUCT_* cls)
        {
            var type = HandleToObject(cls);

            if (type.IsPrimitiveNumeric)
                return asCorInfoType(type);

            return CorInfoType.CORINFO_TYPE_UNDEF;
        }

        private bool canCast(CORINFO_CLASS_STRUCT_* child, CORINFO_CLASS_STRUCT_* parent)
        { throw new NotImplementedException("canCast"); }
        private bool areTypesEquivalent(CORINFO_CLASS_STRUCT_* cls1, CORINFO_CLASS_STRUCT_* cls2)
        { throw new NotImplementedException("areTypesEquivalent"); }

        private TypeCompareState compareTypesForCast(CORINFO_CLASS_STRUCT_* fromClass, CORINFO_CLASS_STRUCT_* toClass)
        {
            TypeDesc fromType = HandleToObject(fromClass);
            TypeDesc toType = HandleToObject(toClass);

            TypeCompareState result = TypeCompareState.May;

            if (fromType.IsIDynamicInterfaceCastable)
            {
                result = TypeCompareState.May;
            }
            else if (toType.IsNullable)
            {
                // If casting to Nullable<T>, don't try to optimize
                result = TypeCompareState.May;
            }
            else if (!fromType.IsCanonicalSubtype(CanonicalFormKind.Any) && !toType.IsCanonicalSubtype(CanonicalFormKind.Any))
            {
                // If the types are not shared, we can check directly.
                if (fromType.CanCastTo(toType))
                    result = TypeCompareState.Must;
                else
                    result = TypeCompareState.MustNot;
            }
            else if (fromType.IsCanonicalSubtype(CanonicalFormKind.Any) && !toType.IsCanonicalSubtype(CanonicalFormKind.Any))
            {
                // Casting from a shared type to an unshared type.
                // Only handle casts to interface types for now
                if (toType.IsInterface)
                {
                    // Do a preliminary check.
                    bool canCast = fromType.CanCastTo(toType);

                    // Pass back positive results unfiltered. The unknown type
                    // parameters in fromClass did not come into play.
                    if (canCast)
                    {
                        result = TypeCompareState.Must;
                    }
                    // We have __Canon parameter(s) in fromClass, somewhere.
                    //
                    // In CanCastTo, these __Canon(s) won't match the interface or
                    // instantiated types on the interface, so CanCastTo may
                    // return false negatives.
                    //
                    // Only report MustNot if the fromClass is not __Canon
                    // and the interface is not instantiated; then there is
                    // no way for the fromClass __Canon(s) to confuse things.
                    //
                    //    __Canon       -> IBar             May
                    //    IFoo<__Canon> -> IFoo<string>     May
                    //    IFoo<__Canon> -> IBar             MustNot
                    //
                    else if (fromType.IsCanonicalDefinitionType(CanonicalFormKind.Any))
                    {
                        result = TypeCompareState.May;
                    }
                    else if (toType.HasInstantiation)
                    {
                        result = TypeCompareState.May;
                    }
                    else
                    {
                        result = TypeCompareState.MustNot;
                    }
                }
            }

#if READYTORUN
            // In R2R it is a breaking change for a previously positive
            // cast to become negative, but not for a previously negative
            // cast to become positive. So in R2R a negative result is
            // always reported back as May.
            if (result == TypeCompareState.MustNot)
            {
                result = TypeCompareState.May;
            }
#endif

            return result;
        }

        private TypeCompareState compareTypesForEquality(CORINFO_CLASS_STRUCT_* cls1, CORINFO_CLASS_STRUCT_* cls2)
        {
            TypeCompareState result = TypeCompareState.May;

            TypeDesc type1 = HandleToObject(cls1);
            TypeDesc type2 = HandleToObject(cls2);

            // If neither type is a canonical subtype, type handle comparison suffices
            if (!type1.IsCanonicalSubtype(CanonicalFormKind.Any) && !type2.IsCanonicalSubtype(CanonicalFormKind.Any))
            {
                result = (type1 == type2 ? TypeCompareState.Must : TypeCompareState.MustNot);
            }
            // If either or both types are canonical subtypes, we can sometimes prove inequality.
            else
            {
                // If either is a value type then the types cannot
                // be equal unless the type defs are the same.
                if (type1.IsValueType || type2.IsValueType)
                {
                    if (!type1.IsCanonicalDefinitionType(CanonicalFormKind.Universal) && !type2.IsCanonicalDefinitionType(CanonicalFormKind.Universal))
                    {
                        if (!type1.HasSameTypeDefinition(type2))
                        {
                            result = TypeCompareState.MustNot;
                        }
                    }
                }
                // If we have two ref types that are not __Canon, then the
                // types cannot be equal unless the type defs are the same.
                else
                {
                    if (!type1.IsCanonicalDefinitionType(CanonicalFormKind.Any) && !type2.IsCanonicalDefinitionType(CanonicalFormKind.Any))
                    {
                        if (!type1.HasSameTypeDefinition(type2))
                        {
                            result = TypeCompareState.MustNot;
                        }
                    }
                }
            }

            return result;
        }

        private CORINFO_CLASS_STRUCT_* mergeClasses(CORINFO_CLASS_STRUCT_* cls1, CORINFO_CLASS_STRUCT_* cls2)
        {
            TypeDesc type1 = HandleToObject(cls1);
            TypeDesc type2 = HandleToObject(cls2);

            TypeDesc merged = TypeExtensions.MergeTypesToCommonParent(type1, type2);

#if DEBUG
            // Make sure the merge is reflexive in the cases we "support".
            TypeDesc reflexive = TypeExtensions.MergeTypesToCommonParent(type2, type1);

            // If both sides are classes than either they have a common non-interface parent (in which case it is
            // reflexive)
            // OR they share a common interface, and it can be order dependent (if they share multiple interfaces
            // in common)
            if (!type1.IsInterface && !type2.IsInterface)
            {
                if (merged.IsInterface)
                {
                    Debug.Assert(reflexive.IsInterface);
                }
                else
                {
                    Debug.Assert(merged == reflexive);
                }
            }
            // Both results must either be interfaces or classes.  They cannot be mixed.
            Debug.Assert(merged.IsInterface == reflexive.IsInterface);

            // If the result of the merge was a class, then the result of the reflexive merge was the same class.
            if (!merged.IsInterface)
            {
                Debug.Assert(merged == reflexive);
            }

            // If both sides are arrays, then the result is either an array or g_pArrayClass.  The above is
            // actually true about the element type for references types, but I think that that is a little
            // excessive for sanity.
            if (type1.IsArray && type2.IsArray)
            {
                TypeDesc arrayClass = _compilation.TypeSystemContext.GetWellKnownType(WellKnownType.Array);
                Debug.Assert((merged.IsArray && reflexive.IsArray)
                         || ((merged == arrayClass) && (reflexive == arrayClass)));
            }

            // The results must always be assignable
            Debug.Assert(type1.CanCastTo(merged) && type2.CanCastTo(merged) && type1.CanCastTo(reflexive)
                     && type2.CanCastTo(reflexive));
#endif

            return ObjectToHandle(merged);
        }

        private bool isMoreSpecificType(CORINFO_CLASS_STRUCT_* cls1, CORINFO_CLASS_STRUCT_* cls2)
        {
            TypeDesc type1 = HandleToObject(cls1);
            TypeDesc type2 = HandleToObject(cls2);

            // If we have a mixture of shared and unshared types,
            // consider the unshared type as more specific.
            bool isType1CanonSubtype = type1.IsCanonicalSubtype(CanonicalFormKind.Any);
            bool isType2CanonSubtype = type2.IsCanonicalSubtype(CanonicalFormKind.Any);
            if (isType1CanonSubtype != isType2CanonSubtype)
            {
                // Only one of type1 and type2 is shared.
                // type2 is more specific if type1 is the shared type.
                return isType1CanonSubtype;
            }

            // Otherwise both types are either shared or not shared.
            // Look for a common parent type.
            TypeDesc merged = TypeExtensions.MergeTypesToCommonParent(type1, type2);

            // If the common parent is type1, then type2 is more specific.
            return merged == type1;
        }

        private CORINFO_CLASS_STRUCT_* getParentType(CORINFO_CLASS_STRUCT_* cls)
        { throw new NotImplementedException("getParentType"); }

        private CorInfoType getChildType(CORINFO_CLASS_STRUCT_* clsHnd, CORINFO_CLASS_STRUCT_** clsRet)
        {
            CorInfoType result = CorInfoType.CORINFO_TYPE_UNDEF;

            var td = HandleToObject(clsHnd);
            if (td.IsArray || td.IsByRef || td.IsPointer)
            {
                TypeDesc returnType = ((ParameterizedType)td).ParameterType;
                result = asCorInfoType(returnType, clsRet);
            }
            else
                clsRet = null;

            return result;
        }

        private bool satisfiesClassConstraints(CORINFO_CLASS_STRUCT_* cls)
        { throw new NotImplementedException("satisfiesClassConstraints"); }

        private bool isSDArray(CORINFO_CLASS_STRUCT_* cls)
        {
            var td = HandleToObject(cls);
            return td.IsSzArray;
        }

        private uint getArrayRank(CORINFO_CLASS_STRUCT_* cls)
        {
            uint rank = 0;
            var td = HandleToObject(cls) as ArrayType;
            if (td != null)
            {
                rank = (uint)td.Rank;
            }
            return rank;
        }

        private CorInfoArrayIntrinsic getArrayIntrinsicID(CORINFO_METHOD_STRUCT_* ftn)
        {
            CorInfoArrayIntrinsic kind = CorInfoArrayIntrinsic.ILLEGAL;
            if (HandleToObject(ftn) is ArrayMethod am)
            {
                kind = am.Kind switch
                {
                    ArrayMethodKind.Get => CorInfoArrayIntrinsic.GET,
                    ArrayMethodKind.Set => CorInfoArrayIntrinsic.SET,
                    ArrayMethodKind.Address => CorInfoArrayIntrinsic.ADDRESS,
                    _ => CorInfoArrayIntrinsic.ILLEGAL
                };
            }
            return kind;
        }

        private void* getArrayInitializationData(CORINFO_FIELD_STRUCT_* field, uint size)
        {
            var fd = HandleToObject(field);

            // Check for invalid arguments passed to InitializeArray intrinsic
            if (!fd.HasRva ||
                size > fd.FieldType.GetElementSize().AsInt)
            {
                return null;
            }

            return (void*)ObjectToHandle(_compilation.GetFieldRvaData(fd));
        }

        private CorInfoIsAccessAllowedResult canAccessClass(ref CORINFO_RESOLVED_TOKEN pResolvedToken, CORINFO_METHOD_STRUCT_* callerHandle, ref CORINFO_HELPER_DESC pAccessHelper)
        {
            // TODO: Access check
            return CorInfoIsAccessAllowedResult.CORINFO_ACCESS_ALLOWED;
        }

        private byte* getFieldName(CORINFO_FIELD_STRUCT_* ftn, byte** moduleName)
        {
            var field = HandleToObject(ftn);
            if (moduleName != null)
            {
                MetadataType typeDef = field.OwningType.GetTypeDefinition() as MetadataType;
                if (typeDef != null)
                    *moduleName = (byte*)GetPin(StringToUTF8(typeDef.GetFullName()));
                else
                    *moduleName = (byte*)GetPin(StringToUTF8("unknown"));
            }

            return (byte*)GetPin(StringToUTF8(field.Name));
        }

        private CORINFO_CLASS_STRUCT_* getFieldClass(CORINFO_FIELD_STRUCT_* field)
        {
            var fieldDesc = HandleToObject(field);
            return ObjectToHandle(fieldDesc.OwningType);
        }

        private CorInfoType getFieldType(CORINFO_FIELD_STRUCT_* field, CORINFO_CLASS_STRUCT_** structType, CORINFO_CLASS_STRUCT_* memberParent)
        {
            FieldDesc fieldDesc = HandleToObject(field);
            TypeDesc fieldType = fieldDesc.FieldType;

            CorInfoType type;
            if (structType != null)
            {
                type = asCorInfoType(fieldType, structType);
            }
            else
            {
                type = asCorInfoType(fieldType);
            }

            Debug.Assert(!fieldDesc.OwningType.IsByReferenceOfT ||
                fieldDesc.OwningType.GetKnownField("_value").FieldType.Category == TypeFlags.IntPtr);
            if (type == CorInfoType.CORINFO_TYPE_NATIVEINT && fieldDesc.OwningType.IsByReferenceOfT)
            {
                Debug.Assert(structType == null || *structType == null);
                Debug.Assert(fieldDesc.Offset.AsInt == 0);
                type = CorInfoType.CORINFO_TYPE_BYREF;
            }

            return type;
        }

        private uint getFieldOffset(CORINFO_FIELD_STRUCT_* field)
        {
            var fieldDesc = HandleToObject(field);

            Debug.Assert(fieldDesc.Offset != FieldAndOffset.InvalidOffset);

            return (uint)fieldDesc.Offset.AsInt;
        }

        private CORINFO_FIELD_ACCESSOR getFieldIntrinsic(FieldDesc field)
        {
            Debug.Assert(field.IsIntrinsic);

            var owningType = field.OwningType;
            if ((owningType.IsWellKnownType(WellKnownType.IntPtr) ||
                    owningType.IsWellKnownType(WellKnownType.UIntPtr)) &&
                        field.Name == "Zero")
            {
                return CORINFO_FIELD_ACCESSOR.CORINFO_FIELD_INTRINSIC_ZERO;
            }
            else if (owningType.IsString && field.Name == "Empty")
            {
                return CORINFO_FIELD_ACCESSOR.CORINFO_FIELD_INTRINSIC_EMPTY_STRING;
            }
            else if (owningType.Name == "BitConverter" && owningType.Namespace == "System" &&
                field.Name == "IsLittleEndian")
            {
                return CORINFO_FIELD_ACCESSOR.CORINFO_FIELD_INTRINSIC_ISLITTLEENDIAN;
            }

            return (CORINFO_FIELD_ACCESSOR)(-1);
        }

        private bool isFieldStatic(CORINFO_FIELD_STRUCT_* fldHnd)
        {
            return HandleToObject(fldHnd).IsStatic;
        }

        private void getBoundaries(CORINFO_METHOD_STRUCT_* ftn, ref uint cILOffsets, ref uint* pILOffsets, BoundaryTypes* implicitBoundaries)
        {
            // TODO: Debugging
            cILOffsets = 0;
            pILOffsets = null;
            *implicitBoundaries = BoundaryTypes.DEFAULT_BOUNDARIES;
        }

        private void getVars(CORINFO_METHOD_STRUCT_* ftn, ref uint cVars, ILVarInfo** vars, ref bool extendOthers)
        {
            // TODO: Debugging

            cVars = 0;
            *vars = null;

            // Just tell the JIT to extend everything.
            extendOthers = true;
        }

        private void* allocateArray(UIntPtr cBytes)
        {
            return (void*)Marshal.AllocHGlobal((IntPtr)(void*)cBytes);
        }

        private void freeArray(void* array)
        {
            Marshal.FreeHGlobal((IntPtr)array);
        }

        private CORINFO_ARG_LIST_STRUCT_* getArgNext(CORINFO_ARG_LIST_STRUCT_* args)
        {
            return (CORINFO_ARG_LIST_STRUCT_*)((int)args + 1);
        }

        private CorInfoTypeWithMod getArgType(CORINFO_SIG_INFO* sig, CORINFO_ARG_LIST_STRUCT_* args, CORINFO_CLASS_STRUCT_** vcTypeRet)
        {
            int index = (int)args;
            Object sigObj = HandleToObject((IntPtr)sig->methodSignature);

            MethodSignature methodSig = sigObj as MethodSignature;

            if (methodSig != null)
            {
                TypeDesc type = methodSig[index];

                CorInfoType corInfoType = asCorInfoType(type, vcTypeRet);
                return (CorInfoTypeWithMod)corInfoType;
            }
            else
            {
                LocalVariableDefinition[] locals = (LocalVariableDefinition[])sigObj;
                TypeDesc type = locals[index].Type;

                CorInfoType corInfoType = asCorInfoType(type, vcTypeRet);

                return (CorInfoTypeWithMod)corInfoType | (locals[index].IsPinned ? CorInfoTypeWithMod.CORINFO_TYPE_MOD_PINNED : 0);
            }
        }

        private uint getLoongArch64PassStructInRegisterFlags(CORINFO_CLASS_STRUCT_* cls)
        {
            throw new NotImplementedException("For LoongArch64, would be implemented later");
        }

        private CORINFO_CLASS_STRUCT_* getArgClass(CORINFO_SIG_INFO* sig, CORINFO_ARG_LIST_STRUCT_* args)
        {
            int index = (int)args;
            Object sigObj = HandleToObject((IntPtr)sig->methodSignature);

            MethodSignature methodSig = sigObj as MethodSignature;
            if (methodSig != null)
            {
                TypeDesc type = methodSig[index];
                return ObjectToHandle(type);
            }
            else
            {
                LocalVariableDefinition[] locals = (LocalVariableDefinition[])sigObj;
                TypeDesc type = locals[index].Type;
                return ObjectToHandle(type);
            }
        }

        private CorInfoHFAElemType getHFAType(CORINFO_CLASS_STRUCT_* hClass)
        {
            var type = (DefType)HandleToObject(hClass);

            // See MethodTable::GetHFAType and Compiler::GetHfaType.
            return (type.ValueTypeShapeCharacteristics & ValueTypeShapeCharacteristics.AggregateMask) switch
            {
                ValueTypeShapeCharacteristics.Float32Aggregate => CorInfoHFAElemType.CORINFO_HFA_ELEM_FLOAT,
                ValueTypeShapeCharacteristics.Float64Aggregate => CorInfoHFAElemType.CORINFO_HFA_ELEM_DOUBLE,
                ValueTypeShapeCharacteristics.Vector64Aggregate => CorInfoHFAElemType.CORINFO_HFA_ELEM_VECTOR64,
                ValueTypeShapeCharacteristics.Vector128Aggregate => CorInfoHFAElemType.CORINFO_HFA_ELEM_VECTOR128,
                _ => CorInfoHFAElemType.CORINFO_HFA_ELEM_NONE
            };
        }

        private HRESULT GetErrorHRESULT(_EXCEPTION_POINTERS* pExceptionPointers)
        { throw new NotImplementedException("GetErrorHRESULT"); }
        private uint GetErrorMessage(char* buffer, uint bufferLength)
        { throw new NotImplementedException("GetErrorMessage"); }

        private int FilterException(_EXCEPTION_POINTERS* pExceptionPointers)
        {
            // This method is completely handled by the C++ wrapper to the JIT-EE interface,
            // and should never reach the managed implementation.
            Debug.Fail("CorInfoImpl.FilterException should not be called");
            throw new NotSupportedException("FilterException");
        }

        private bool runWithErrorTrap(void* function, void* parameter)
        {
            // This method is completely handled by the C++ wrapper to the JIT-EE interface,
            // and should never reach the managed implementation.
            Debug.Fail("CorInfoImpl.runWithErrorTrap should not be called");
            throw new NotSupportedException("runWithErrorTrap");
        }

        private bool runWithSPMIErrorTrap(void* function, void* parameter)
        {
            // This method is completely handled by the C++ wrapper to the JIT-EE interface,
            // and should never reach the managed implementation.
            Debug.Fail("CorInfoImpl.runWithSPMIErrorTrap should not be called");
            throw new NotSupportedException("runWithSPMIErrorTrap");
        }

        private void ThrowExceptionForJitResult(HRESULT result)
        { throw new NotImplementedException("ThrowExceptionForJitResult"); }
        private void ThrowExceptionForHelper(ref CORINFO_HELPER_DESC throwHelper)
        { throw new NotImplementedException("ThrowExceptionForHelper"); }

        public static CORINFO_OS TargetToOs(TargetDetails target)
        {
            return target.IsWindows ? CORINFO_OS.CORINFO_WINNT :
                   target.IsOSX ? CORINFO_OS.CORINFO_MACOS : CORINFO_OS.CORINFO_UNIX;
        }

        private void getEEInfo(ref CORINFO_EE_INFO pEEInfoOut)
        {
            pEEInfoOut = new CORINFO_EE_INFO();

#if DEBUG
            // In debug, write some bogus data to the struct to ensure we have filled everything
            // properly.
            fixed (CORINFO_EE_INFO* tmp = &pEEInfoOut)
                MemoryHelper.FillMemory((byte*)tmp, 0xcc, Marshal.SizeOf<CORINFO_EE_INFO>());
#endif

            int pointerSize = this.PointerSize;

            pEEInfoOut.inlinedCallFrameInfo.size = (uint)SizeOfPInvokeTransitionFrame;

            pEEInfoOut.offsetOfDelegateInstance = (uint)pointerSize;            // Delegate::m_firstParameter
            pEEInfoOut.offsetOfDelegateFirstTarget = OffsetOfDelegateFirstTarget;

            pEEInfoOut.sizeOfReversePInvokeFrame = (uint)SizeOfReversePInvokeTransitionFrame;

            pEEInfoOut.osPageSize = new UIntPtr(0x1000);

            pEEInfoOut.maxUncheckedOffsetForNullObject = (_compilation.NodeFactory.Target.IsWindows) ?
                new UIntPtr(32 * 1024 - 1) : new UIntPtr((uint)pEEInfoOut.osPageSize / 2 - 1);

            pEEInfoOut.targetAbi = TargetABI;
            pEEInfoOut.osType = TargetToOs(_compilation.NodeFactory.Target);
        }

        private char* getJitTimeLogFilename()
        {
            return null;
        }

        private mdToken getMethodDefFromMethod(CORINFO_METHOD_STRUCT_* hMethod)
        {
            MethodDesc method = HandleToObject(hMethod);
#if READYTORUN
            if (method is UnboxingMethodDesc unboxingMethodDesc)
            {
                method = unboxingMethodDesc.Target;
            }
#endif
            MethodDesc methodDefinition = method.GetTypicalMethodDefinition();

            // Need to cast down to EcmaMethod. Do not use this as a precedent that casting to Ecma*
            // within the JitInterface is fine. We might want to consider moving this to Compilation.
            TypeSystem.Ecma.EcmaMethod ecmaMethodDefinition = methodDefinition as TypeSystem.Ecma.EcmaMethod;
            if (ecmaMethodDefinition != null)
            {
                return (mdToken)System.Reflection.Metadata.Ecma335.MetadataTokens.GetToken(ecmaMethodDefinition.Handle);
            }

            return 0;
        }

        private static byte[] StringToUTF8(string s)
        {
            int byteCount = Encoding.UTF8.GetByteCount(s);
            byte[] bytes = new byte[byteCount + 1];
            Encoding.UTF8.GetBytes(s, 0, s.Length, bytes, 0);
            return bytes;
        }

        private byte* getMethodName(CORINFO_METHOD_STRUCT_* ftn, byte** moduleName)
        {
            MethodDesc method = HandleToObject(ftn);

            if (moduleName != null)
            {
                MetadataType typeDef = method.OwningType.GetTypeDefinition() as MetadataType;
                if (typeDef != null)
                    *moduleName = (byte*)GetPin(StringToUTF8(typeDef.GetFullName()));
                else
                    *moduleName = (byte*)GetPin(StringToUTF8("unknown"));
            }

            return (byte*)GetPin(StringToUTF8(method.Name));
        }

        private String getMethodNameFromMetadataImpl(MethodDesc method, out string className, out string namespaceName, out string enclosingClassName)
        {
            string result = null;
            className = null;
            namespaceName = null;
            enclosingClassName = null;

            result = method.Name;

            MetadataType owningType = method.OwningType as MetadataType;
            if (owningType != null)
            {
                className = owningType.Name;
                namespaceName = owningType.Namespace;

                // Query enclosingClassName when the method is in a nested class
                // and get the namespace of enclosing classes (nested class's namespace is empty)
                var containingType = owningType.ContainingType;
                if (containingType != null)
                {
                    enclosingClassName = containingType.Name;
                    namespaceName = containingType.Namespace;
                }
            }

            return result;
        }

        private byte* getMethodNameFromMetadata(CORINFO_METHOD_STRUCT_* ftn, byte** className, byte** namespaceName, byte** enclosingClassName)
        {
            MethodDesc method = HandleToObject(ftn);

            string result;
            string classResult;
            string namespaceResult;
            string enclosingResult;

            result = getMethodNameFromMetadataImpl(method, out classResult, out namespaceResult, out enclosingResult);

            if (className != null)
                *className = classResult != null ? (byte*)GetPin(StringToUTF8(classResult)) : null;
            if (namespaceName != null)
                *namespaceName = namespaceResult != null ? (byte*)GetPin(StringToUTF8(namespaceResult)) : null;
            if (enclosingClassName != null)
                *enclosingClassName = enclosingResult != null ? (byte*)GetPin(StringToUTF8(enclosingResult)) : null;

            return result != null ? (byte*)GetPin(StringToUTF8(result)) : null;
        }

        private uint getMethodHash(CORINFO_METHOD_STRUCT_* ftn)
        {
            return (uint)HandleToObject(ftn).GetHashCode();
        }

        private UIntPtr findNameOfToken(CORINFO_MODULE_STRUCT_* moduleHandle, mdToken token, byte* szFQName, UIntPtr FQNameCapacity)
        { throw new NotImplementedException("findNameOfToken"); }

        private bool getSystemVAmd64PassStructInRegisterDescriptor(CORINFO_CLASS_STRUCT_* structHnd, SYSTEMV_AMD64_CORINFO_STRUCT_REG_PASSING_DESCRIPTOR* structPassInRegDescPtr)
        {
            TypeDesc typeDesc = HandleToObject(structHnd);

            SystemVStructClassificator.GetSystemVAmd64PassStructInRegisterDescriptor(typeDesc, out *structPassInRegDescPtr);
            return true;
        }

        private uint getThreadTLSIndex(ref void* ppIndirection)
        { throw new NotImplementedException("getThreadTLSIndex"); }
        private void* getInlinedCallFrameVptr(ref void* ppIndirection)
        { throw new NotImplementedException("getInlinedCallFrameVptr"); }

        private Dictionary<CorInfoHelpFunc, ISymbolNode> _helperCache = new Dictionary<CorInfoHelpFunc, ISymbolNode>();
        private void* getHelperFtn(CorInfoHelpFunc ftnNum, ref void* ppIndirection)
        {
            ISymbolNode entryPoint;
            if (!_helperCache.TryGetValue(ftnNum, out entryPoint))
            {
                entryPoint = GetHelperFtnUncached(ftnNum);
                _helperCache.Add(ftnNum, entryPoint);
            }
            if (entryPoint.RepresentsIndirectionCell)
            {
                ppIndirection = (void*)ObjectToHandle(entryPoint);
                return null;
            }
            else
            {
                ppIndirection = null;
                return (void*)ObjectToHandle(entryPoint);
            }
        }

        private void getFunctionFixedEntryPoint(CORINFO_METHOD_STRUCT_* ftn, bool isUnsafeFunctionPointer, ref CORINFO_CONST_LOOKUP pResult)
        { throw new NotImplementedException("getFunctionFixedEntryPoint"); }

        private CorInfoHelpFunc getLazyStringLiteralHelper(CORINFO_MODULE_STRUCT_* handle)
        {
            // TODO: Lazy string literal helper
            return CorInfoHelpFunc.CORINFO_HELP_UNDEF;
        }

        private CORINFO_MODULE_STRUCT_* embedModuleHandle(CORINFO_MODULE_STRUCT_* handle, ref void* ppIndirection)
        { throw new NotImplementedException("embedModuleHandle"); }

        private CORINFO_FIELD_STRUCT_* embedFieldHandle(CORINFO_FIELD_STRUCT_* handle, ref void* ppIndirection)
        { throw new NotImplementedException("embedFieldHandle"); }

        private CORINFO_RUNTIME_LOOKUP_KIND GetGenericRuntimeLookupKind(MethodDesc method)
        {
            if (method.RequiresInstMethodDescArg())
                return CORINFO_RUNTIME_LOOKUP_KIND.CORINFO_LOOKUP_METHODPARAM;
            else if (method.RequiresInstMethodTableArg())
                return CORINFO_RUNTIME_LOOKUP_KIND.CORINFO_LOOKUP_CLASSPARAM;
            else
            {
                Debug.Assert(method.AcquiresInstMethodTableFromThis());
                return CORINFO_RUNTIME_LOOKUP_KIND.CORINFO_LOOKUP_THISOBJ;
            }
        }

        private void getLocationOfThisType(CORINFO_METHOD_STRUCT_* context, ref CORINFO_LOOKUP_KIND result)
        {
            MethodDesc method = HandleToObject(context);

            if (method.IsSharedByGenericInstantiations)
            {
                result.needsRuntimeLookup = true;
                result.runtimeLookupKind = GetGenericRuntimeLookupKind(method);
            }
            else
            {
                result.needsRuntimeLookup = false;
                result.runtimeLookupKind = CORINFO_RUNTIME_LOOKUP_KIND.CORINFO_LOOKUP_THISOBJ;
            }
        }

        private void* GetCookieForPInvokeCalliSig(CORINFO_SIG_INFO* szMetaSig, ref void* ppIndirection)
        { throw new NotImplementedException("GetCookieForPInvokeCalliSig"); }
        private CORINFO_JUST_MY_CODE_HANDLE_* getJustMyCodeHandle(CORINFO_METHOD_STRUCT_* method, ref CORINFO_JUST_MY_CODE_HANDLE_* ppIndirection)
        {
            ppIndirection = null;
            return null;
        }
        private void GetProfilingHandle(ref bool pbHookFunction, ref void* pProfilerHandle, ref bool pbIndirectedHandles)
        { throw new NotImplementedException("GetProfilingHandle"); }

        /// <summary>
        /// Create a CORINFO_CONST_LOOKUP to a symbol and put the address into the addr field
        /// </summary>
        private CORINFO_CONST_LOOKUP CreateConstLookupToSymbol(ISymbolNode symbol)
        {
            CORINFO_CONST_LOOKUP constLookup = new CORINFO_CONST_LOOKUP();
            constLookup.addr = (void*)ObjectToHandle(symbol);
            constLookup.accessType = symbol.RepresentsIndirectionCell ? InfoAccessType.IAT_PVALUE : InfoAccessType.IAT_VALUE;
            return constLookup;
        }

        private bool canAccessFamily(CORINFO_METHOD_STRUCT_* hCaller, CORINFO_CLASS_STRUCT_* hInstanceType)
        { throw new NotImplementedException("canAccessFamily"); }
        private bool isRIDClassDomainID(CORINFO_CLASS_STRUCT_* cls)
        { throw new NotImplementedException("isRIDClassDomainID"); }
        private uint getClassDomainID(CORINFO_CLASS_STRUCT_* cls, ref void* ppIndirection)
        { throw new NotImplementedException("getClassDomainID"); }

        private void* getFieldAddress(CORINFO_FIELD_STRUCT_* field, void** ppIndirection)
        {
            FieldDesc fieldDesc = HandleToObject(field);
            Debug.Assert(fieldDesc.HasRva);
            ISymbolNode node = _compilation.GetFieldRvaData(fieldDesc);
            void *handle = (void *)ObjectToHandle(node);
            if (node.RepresentsIndirectionCell)
            {
                *ppIndirection = handle;
                return null;
            }
            else
            {
                if (ppIndirection != null)
                    *ppIndirection = null;
                return handle;
            }
        }

        private CORINFO_CLASS_STRUCT_* getStaticFieldCurrentClass(CORINFO_FIELD_STRUCT_* field, byte* pIsSpeculative)
        {
            if (pIsSpeculative != null)
                *pIsSpeculative = 1;

            return null;
        }

        private IntPtr getVarArgsHandle(CORINFO_SIG_INFO* pSig, ref void* ppIndirection)
        { throw new NotImplementedException("getVarArgsHandle"); }
        private bool canGetVarArgsHandle(CORINFO_SIG_INFO* pSig)
        { throw new NotImplementedException("canGetVarArgsHandle"); }

        private InfoAccessType emptyStringLiteral(ref void* ppValue)
        {
            return constructStringLiteral(_methodScope, (mdToken)CorTokenType.mdtString, ref ppValue);
        }

        private uint getFieldThreadLocalStoreID(CORINFO_FIELD_STRUCT_* field, ref void* ppIndirection)
        { throw new NotImplementedException("getFieldThreadLocalStoreID"); }
        private void addActiveDependency(CORINFO_MODULE_STRUCT_* moduleFrom, CORINFO_MODULE_STRUCT_* moduleTo)
        { throw new NotImplementedException("addActiveDependency"); }
        private CORINFO_METHOD_STRUCT_* GetDelegateCtor(CORINFO_METHOD_STRUCT_* methHnd, CORINFO_CLASS_STRUCT_* clsHnd, CORINFO_METHOD_STRUCT_* targetMethodHnd, ref DelegateCtorArgs pCtorData)
        { throw new NotImplementedException("GetDelegateCtor"); }
        private void MethodCompileComplete(CORINFO_METHOD_STRUCT_* methHnd)
        { throw new NotImplementedException("MethodCompileComplete"); }

        private bool getTailCallHelpers(ref CORINFO_RESOLVED_TOKEN callToken, CORINFO_SIG_INFO* sig, CORINFO_GET_TAILCALL_HELPERS_FLAGS flags, ref CORINFO_TAILCALL_HELPERS pResult)
        {
            // Slow tailcalls are not supported yet
            // https://github.com/dotnet/runtime/issues/35423
#if READYTORUN
            throw new RequiresRuntimeJitException(nameof(getTailCallHelpers));
#else
            return false;
#endif
        }

        private byte[] _code;
        private byte[] _coldCode;
        private int _codeAlignment;

        private byte[] _roData;

        private MethodReadOnlyDataNode _roDataBlob;
        private int _roDataAlignment;

        private int _numFrameInfos;
        private int _usedFrameInfos;
        private FrameInfo[] _frameInfos;

        private byte[] _gcInfo;
        private CORINFO_EH_CLAUSE[] _ehClauses;

        private void allocMem(ref AllocMemArgs args)
        {
            args.hotCodeBlock = (void*)GetPin(_code = new byte[args.hotCodeSize]);
            args.hotCodeBlockRW = args.hotCodeBlock;

            if (args.coldCodeSize != 0)
            {
                args.coldCodeBlock = (void*)GetPin(_coldCode = new byte[args.coldCodeSize]);
                args.coldCodeBlockRW = args.coldCodeBlock;
            }

            _codeAlignment = -1;
            if ((args.flag & CorJitAllocMemFlag.CORJIT_ALLOCMEM_FLG_32BYTE_ALIGN) != 0)
            {
                _codeAlignment = 32;
            }
            else if ((args.flag & CorJitAllocMemFlag.CORJIT_ALLOCMEM_FLG_16BYTE_ALIGN) != 0)
            {
                _codeAlignment = 16;
            }

            if (args.roDataSize != 0)
            {
                _roDataAlignment = 8;

                if ((args.flag & CorJitAllocMemFlag.CORJIT_ALLOCMEM_FLG_RODATA_32BYTE_ALIGN) != 0)
                {
                    _roDataAlignment = 32;
                }
                else if ((args.flag & CorJitAllocMemFlag.CORJIT_ALLOCMEM_FLG_RODATA_16BYTE_ALIGN) != 0)
                {
                    _roDataAlignment = 16;
                }
                else if (args.roDataSize < 8)
                {
                    _roDataAlignment = PointerSize;
                }

                _roData = new byte[args.roDataSize];

                _roDataBlob = new MethodReadOnlyDataNode(MethodBeingCompiled);

                args.roDataBlock = (void*)GetPin(_roData);
                args.roDataBlockRW = args.roDataBlock;
            }

            if (_numFrameInfos > 0)
            {
                _frameInfos = new FrameInfo[_numFrameInfos];
            }
        }

        private void reserveUnwindInfo(bool isFunclet, bool isColdCode, uint unwindSize)
        {
            _numFrameInfos++;
        }

        private void allocUnwindInfo(byte* pHotCode, byte* pColdCode, uint startOffset, uint endOffset, uint unwindSize, byte* pUnwindBlock, CorJitFuncKind funcKind)
        {
            Debug.Assert(FrameInfoFlags.Filter == (FrameInfoFlags)CorJitFuncKind.CORJIT_FUNC_FILTER);
            Debug.Assert(FrameInfoFlags.Handler == (FrameInfoFlags)CorJitFuncKind.CORJIT_FUNC_HANDLER);

            FrameInfoFlags flags = (FrameInfoFlags)funcKind;

            if (funcKind == CorJitFuncKind.CORJIT_FUNC_ROOT)
            {
                if (this.MethodBeingCompiled.IsUnmanagedCallersOnly)
                    flags |= FrameInfoFlags.ReversePInvoke;
            }

            byte[] blobData = new byte[unwindSize];

            for (uint i = 0; i < unwindSize; i++)
            {
                blobData[i] = pUnwindBlock[i];
            }

#if !READYTORUN
            var target = _compilation.TypeSystemContext.Target;

            if (target.Architecture == TargetArchitecture.ARM64 && target.OperatingSystem == TargetOS.Linux)
            {
                blobData = CompressARM64CFI(blobData);
            }
#endif

            _frameInfos[_usedFrameInfos++] = new FrameInfo(flags, (int)startOffset, (int)endOffset, blobData);
        }

        private void* allocGCInfo(UIntPtr size)
        {
            _gcInfo = new byte[(int)size];
            return (void*)GetPin(_gcInfo);
        }

        private bool logMsg(uint level, byte* fmt, IntPtr args)
        {
            // Console.WriteLine(Marshal.PtrToStringAnsi((IntPtr)fmt));
            return false;
        }

        private int doAssert(byte* szFile, int iLine, byte* szExpr)
        {
            Log.WriteLine(Marshal.PtrToStringAnsi((IntPtr)szFile) + ":" + iLine);
            Log.WriteLine(Marshal.PtrToStringAnsi((IntPtr)szExpr));

            return 1;
        }

        private void reportFatalError(CorJitResult result)
        {
            // We could add some logging here, but for now it's unnecessary.
            // CompileMethod is going to fail with this CorJitResult anyway.
        }

        private void recordCallSite(uint instrOffset, CORINFO_SIG_INFO* callSig, CORINFO_METHOD_STRUCT_* methodHandle)
        {
        }

        private ArrayBuilder<Relocation> _codeRelocs;
        private ArrayBuilder<Relocation> _roDataRelocs;


        /// <summary>
        /// Various type of block.
        /// </summary>
        public enum BlockType : sbyte
        {
            /// <summary>Not a generated block.</summary>
            Unknown = -1,
            /// <summary>Represent code.</summary>
            Code = 0,
            /// <summary>Represent cold code (i.e. code not called frequently).</summary>
            ColdCode = 1,
            /// <summary>Read-only data.</summary>
            ROData = 2,
            /// <summary>Instrumented Block Count Data</summary>
            BBCounts = 3
        }

        private BlockType findKnownBlock(void* location, out int offset)
        {
            fixed (byte* pCode = _code)
            {
                if (pCode <= (byte*)location && (byte*)location < pCode + _code.Length)
                {
                    offset = (int)((byte*)location - pCode);
                    return BlockType.Code;
                }
            }

            if (_coldCode != null)
            {
                fixed (byte* pColdCode = _coldCode)
                {
                    if (pColdCode <= (byte*)location && (byte*)location < pColdCode + _coldCode.Length)
                    {
                        offset = (int)((byte*)location - pColdCode);
                        return BlockType.ColdCode;
                    }
                }
            }

            if (_roData != null)
            {
                fixed (byte* pROData = _roData)
                {
                    if (pROData <= (byte*)location && (byte*)location < pROData + _roData.Length)
                    {
                        offset = (int)((byte*)location - pROData);
                        return BlockType.ROData;
                    }
                }
            }

            {
                BlockType retBlockType = BlockType.Unknown;
                offset = 0;
                findKnownBBCountBlock(ref retBlockType, location, ref offset);
                if (retBlockType == BlockType.BBCounts)
                    return retBlockType;
            }

            offset = 0;
            return BlockType.Unknown;
        }

        partial void findKnownBBCountBlock(ref BlockType blockType, void* location, ref int offset);

        private ref ArrayBuilder<Relocation> findRelocBlock(BlockType blockType, out int length)
        {
            switch (blockType)
            {
                case BlockType.Code:
                    length = _code.Length;
                    return ref _codeRelocs;
                case BlockType.ROData:
                    length = _roData.Length;
                    return ref _roDataRelocs;
                default:
                    throw new NotImplementedException("Arbitrary relocs");
            }
        }

        // Translates relocation type constants used by JIT (defined in winnt.h) to RelocType enumeration
        private static RelocType GetRelocType(TargetArchitecture targetArchitecture, ushort fRelocType)
        {
            if (targetArchitecture != TargetArchitecture.ARM64)
                return (RelocType)fRelocType;

            const ushort IMAGE_REL_ARM64_BRANCH26 = 3;
            const ushort IMAGE_REL_ARM64_PAGEBASE_REL21 = 4;
            const ushort IMAGE_REL_ARM64_PAGEOFFSET_12A = 6;

            switch (fRelocType)
            {
                case IMAGE_REL_ARM64_BRANCH26:
                    return RelocType.IMAGE_REL_BASED_ARM64_BRANCH26;
                case IMAGE_REL_ARM64_PAGEBASE_REL21:
                    return RelocType.IMAGE_REL_BASED_ARM64_PAGEBASE_REL21;
                case IMAGE_REL_ARM64_PAGEOFFSET_12A:
                    return RelocType.IMAGE_REL_BASED_ARM64_PAGEOFFSET_12A;
                default:
                    Debug.Fail("Invalid RelocType: " + fRelocType);
                    return 0;
            };
        }

        private void recordRelocation(void* location, void* locationRW, void* target, ushort fRelocType, ushort slotNum, int addlDelta)
        {
            // slotNum is not used
            Debug.Assert(slotNum == 0);

            int relocOffset;
            BlockType locationBlock = findKnownBlock(location, out relocOffset);
            Debug.Assert(locationBlock != BlockType.Unknown, "BlockType.Unknown not expected");

            int length;
            ref ArrayBuilder<Relocation> sourceBlock = ref findRelocBlock(locationBlock, out length);

            int relocDelta;
            BlockType targetBlock = findKnownBlock(target, out relocDelta);

            ISymbolNode relocTarget;
            switch (targetBlock)
            {
                case BlockType.Code:
                    relocTarget = _methodCodeNode;
                    break;

                case BlockType.ColdCode:
                    // TODO: Arbitrary relocs
                    throw new NotImplementedException("ColdCode relocs");

                case BlockType.ROData:
                    relocTarget = _roDataBlob;
                    break;

#if READYTORUN
                case BlockType.BBCounts:
                    relocTarget = null;
                    break;
#endif

                default:
                    // Reloc points to something outside of the generated blocks
                    var targetObject = HandleToObject((IntPtr)target);

#if READYTORUN
                    if (targetObject is RequiresRuntimeJitIfUsedSymbol requiresRuntimeSymbol)
                    {
                        throw new RequiresRuntimeJitException(requiresRuntimeSymbol.Message);
                    }
#endif

                    relocTarget = (ISymbolNode)targetObject;
                    break;
            }

            relocDelta += addlDelta;

            TargetArchitecture targetArchitecture = _compilation.TypeSystemContext.Target.Architecture;
            RelocType relocType = GetRelocType(targetArchitecture, fRelocType);
            // relocDelta is stored as the value
            Relocation.WriteValue(relocType, location, relocDelta);

            if (sourceBlock.Count == 0)
                sourceBlock.EnsureCapacity(length / 32 + 1);
            sourceBlock.Add(new Relocation(relocType, relocOffset, relocTarget));
        }

        private ushort getRelocTypeHint(void* target)
        {
            switch (_compilation.TypeSystemContext.Target.Architecture)
            {
                case TargetArchitecture.X64:
                    return (ushort)ILCompiler.DependencyAnalysis.RelocType.IMAGE_REL_BASED_REL32;

                case TargetArchitecture.ARM:
                    return (ushort)ILCompiler.DependencyAnalysis.RelocType.IMAGE_REL_BASED_THUMB_BRANCH24;

                default:
                    return UInt16.MaxValue;
            }
        }

        private uint getExpectedTargetArchitecture()
        {
            TargetArchitecture arch = _compilation.TypeSystemContext.Target.Architecture;

            switch (arch)
            {
                case TargetArchitecture.X86:
                    return (uint)ImageFileMachine.I386;
                case TargetArchitecture.X64:
                    return (uint)ImageFileMachine.AMD64;
                case TargetArchitecture.ARM:
                    return (uint)ImageFileMachine.ARM;
                case TargetArchitecture.ARM64:
                    return (uint)ImageFileMachine.ARM64;
                default:
                    throw new NotImplementedException("Expected target architecture is not supported");
            }
        }

        private bool doesFieldBelongToClass(CORINFO_FIELD_STRUCT_* fld, CORINFO_CLASS_STRUCT_* cls)
        {
            var field = HandleToObject(fld);
            var queryType = HandleToObject(cls);

            Debug.Assert(!field.IsStatic);

            // doesFieldBelongToClass implements the predicate of...
            // if field is not associated with the class in any way, return false.
            // if field is the only FieldDesc that the JIT might see for a given class handle
            // and logical field pair then return true. This is needed as the field handle here
            // is used as a key into a hashtable mapping writes to fields to value numbers.
            //
            // In this implmentation this is made more complex as the JIT is exposed to CORINFO_FIELD_STRUCT
            // pointers which represent exact instantions, so performing exact matching is the necessary approach

            // BaseType._field, BaseType -> true
            // BaseType._field, DerivedType -> true
            // BaseType<__Canon>._field, BaseType<__Canon> -> true
            // BaseType<__Canon>._field, BaseType<string> -> false
            // BaseType<__Canon>._field, BaseType<object> -> false
            // BaseType<sbyte>._field, BaseType<sbyte> -> true
            // BaseType<sbyte>._field, BaseType<byte> -> false

            var fieldOwnerType = field.OwningType;

            while (queryType != null)
            {
                if (fieldOwnerType == queryType)
                    return true;
                queryType = queryType.BaseType;
            }

            return false;
        }

        private bool isMethodDefinedInCoreLib()
        {
            TypeDesc owningType = MethodBeingCompiled.OwningType;
            MetadataType owningMetadataType = owningType as MetadataType;
            if (owningMetadataType == null)
            {
                return false;
            }
            return owningMetadataType.Module == _compilation.TypeSystemContext.SystemModule;
        }

        private uint getJitFlags(ref CORJIT_FLAGS flags, uint sizeInBytes)
        {
            // Read the user-defined configuration options.
            foreach (var flag in JitConfigProvider.Instance.Flags)
                flags.Set(flag);

            flags.InstructionSetFlags.Add(_compilation.InstructionSetSupport.OptimisticFlags);

            // Set the rest of the flags that don't make sense to expose publically.
            flags.Set(CorJitFlag.CORJIT_FLAG_SKIP_VERIFICATION);
            flags.Set(CorJitFlag.CORJIT_FLAG_READYTORUN);
            flags.Set(CorJitFlag.CORJIT_FLAG_RELOC);
            flags.Set(CorJitFlag.CORJIT_FLAG_PREJIT);
            flags.Set(CorJitFlag.CORJIT_FLAG_USE_PINVOKE_HELPERS);

            TargetArchitecture targetArchitecture = _compilation.TypeSystemContext.Target.Architecture;

            switch (targetArchitecture)
            {
                case TargetArchitecture.X64:
                case TargetArchitecture.X86:
                    Debug.Assert(InstructionSet.X86_SSE2 == InstructionSet.X64_SSE2);
                    Debug.Assert(_compilation.InstructionSetSupport.IsInstructionSetSupported(InstructionSet.X86_SSE2));
                    break;

                case TargetArchitecture.ARM64:
                    Debug.Assert(_compilation.InstructionSetSupport.IsInstructionSetSupported(InstructionSet.ARM64_AdvSimd));
                    break;
            }

#if READYTORUN
            if (targetArchitecture == TargetArchitecture.ARM && !_compilation.TypeSystemContext.Target.IsWindows)
                flags.Set(CorJitFlag.CORJIT_FLAG_RELATIVE_CODE_RELOCS);
#endif

            if (this.MethodBeingCompiled.IsUnmanagedCallersOnly)
            {
                // Validate UnmanagedCallersOnlyAttribute usage
                if (!this.MethodBeingCompiled.Signature.IsStatic) // Must be a static method
                {
                    ThrowHelper.ThrowInvalidProgramException(ExceptionStringID.InvalidProgramNonStaticMethod, this.MethodBeingCompiled);
                }

                if (this.MethodBeingCompiled.HasInstantiation || this.MethodBeingCompiled.OwningType.HasInstantiation) // No generics involved
                {
                    ThrowHelper.ThrowInvalidProgramException(ExceptionStringID.InvalidProgramGenericMethod, this.MethodBeingCompiled);
                }

#if READYTORUN
                // TODO: enable this check in full AOT
                if (Marshaller.IsMarshallingRequired(this.MethodBeingCompiled.Signature, Array.Empty<ParameterMetadata>(), ((MetadataType)this.MethodBeingCompiled.OwningType).Module)) // Only blittable arguments
                {
                    ThrowHelper.ThrowInvalidProgramException(ExceptionStringID.InvalidProgramNonBlittableTypes, this.MethodBeingCompiled);
                }
#endif

                flags.Set(CorJitFlag.CORJIT_FLAG_REVERSE_PINVOKE);
            }

            if (this.MethodBeingCompiled.IsPInvoke)
            {
                flags.Set(CorJitFlag.CORJIT_FLAG_IL_STUB);
            }

            if (this.MethodBeingCompiled.IsNoOptimization)
            {
                flags.Set(CorJitFlag.CORJIT_FLAG_MIN_OPT);
            }

            if (this.MethodBeingCompiled.Context.Target.Abi == TargetAbi.NativeAotArmel)
            {
                flags.Set(CorJitFlag.CORJIT_FLAG_SOFTFP_ABI);
            }

            return (uint)sizeof(CORJIT_FLAGS);
        }

        private PgoSchemaElem[] getPgoInstrumentationResults(MethodDesc method)
        {
            return _compilation.ProfileData[method]?.SchemaData;
        }

        public static void ComputeJitPgoInstrumentationSchema(Func<object, IntPtr> objectToHandle, PgoSchemaElem[] pgoResultsSchemas, out PgoInstrumentationSchema[] nativeSchemas, out byte[] instrumentationData, Func<TypeDesc, bool> typeFilter = null)
        {
            nativeSchemas = new PgoInstrumentationSchema[pgoResultsSchemas.Length];
            MemoryStream msInstrumentationData = new MemoryStream();
            BinaryWriter bwInstrumentationData = new BinaryWriter(msInstrumentationData);
            for (int i = 0; i < nativeSchemas.Length; i++)
            {
                if ((bwInstrumentationData.BaseStream.Position % 8) == 4)
                {
                    bwInstrumentationData.Write(0);
                }

                Debug.Assert((bwInstrumentationData.BaseStream.Position % 8) == 0);
                nativeSchemas[i].Offset = new IntPtr(checked((int)bwInstrumentationData.BaseStream.Position));
                nativeSchemas[i].ILOffset = pgoResultsSchemas[i].ILOffset;
                nativeSchemas[i].Count = pgoResultsSchemas[i].Count;
                nativeSchemas[i].Other = pgoResultsSchemas[i].Other;
                nativeSchemas[i].InstrumentationKind = (PgoInstrumentationKind)pgoResultsSchemas[i].InstrumentationKind;

                if (pgoResultsSchemas[i].DataObject == null)
                {
                    bwInstrumentationData.Write(pgoResultsSchemas[i].DataLong);
                }
                else
                {
                    object dataObject = pgoResultsSchemas[i].DataObject;
                    if (dataObject is int[] intArray)
                    {
                        foreach (int intVal in intArray)
                            bwInstrumentationData.Write(intVal);
                    }
                    else if (dataObject is long[] longArray)
                    {
                        foreach (long longVal in longArray)
                            bwInstrumentationData.Write(longVal);
                    }
                    else if (dataObject is TypeSystemEntityOrUnknown[] typeArray)
                    {
                        foreach (TypeSystemEntityOrUnknown typeVal in typeArray)
                        {
                            IntPtr ptrVal;

                            if (typeVal.AsType != null && (typeFilter == null || typeFilter(typeVal.AsType)))
                            {
                                ptrVal = (IntPtr)objectToHandle(typeVal.AsType);
                            }
                            else if (typeVal.AsMethod != null)
                            {
                                ptrVal = (IntPtr)objectToHandle(typeVal.AsMethod);
                            }
                            else
                            {
                                // The "Unknown types are the values from 1-33
                                ptrVal = new IntPtr((typeVal.AsUnknown % 32) + 1);
                            }

                            if (IntPtr.Size == 4)
                                bwInstrumentationData.Write((int)ptrVal);
                            else
                                bwInstrumentationData.Write((long)ptrVal);
                        }
                    }
                }
            }

            bwInstrumentationData.Flush();

            instrumentationData = msInstrumentationData.ToArray();
        }

        private HRESULT getPgoInstrumentationResults(CORINFO_METHOD_STRUCT_* ftnHnd, ref PgoInstrumentationSchema* pSchema, ref uint countSchemaItems, byte** pInstrumentationData,
            ref PgoSource pPgoSource)
        {
            MethodDesc methodDesc = HandleToObject(ftnHnd);

            if (!_pgoResults.TryGetValue(methodDesc, out PgoInstrumentationResults pgoResults))
            {
                var pgoResultsSchemas = getPgoInstrumentationResults(methodDesc);
                if (pgoResultsSchemas == null)
                {
                    pgoResults.hr = HRESULT.E_NOTIMPL;
                }
                else
                {
                    ComputeJitPgoInstrumentationSchema(ObjectToHandle, pgoResultsSchemas, out var nativeSchemas, out var instrumentationData
#if !READYTORUN
                        , _compilation.CanConstructType
#endif
                        );

                    pgoResults.pInstrumentationData = (byte*)GetPin(instrumentationData);
                    pgoResults.countSchemaItems = (uint)nativeSchemas.Length;
                    pgoResults.pSchema = (PgoInstrumentationSchema*)GetPin(nativeSchemas);
                    pgoResults.hr = HRESULT.S_OK;
                }

                _pgoResults.Add(methodDesc, pgoResults);
            }

            pSchema = pgoResults.pSchema;
            countSchemaItems = pgoResults.countSchemaItems;
            *pInstrumentationData = pgoResults.pInstrumentationData;
            pPgoSource = PgoSource.Static;
            return pgoResults.hr;
        }

#if READYTORUN
        InstructionSetFlags _actualInstructionSetSupported;
        InstructionSetFlags _actualInstructionSetUnsupported;

        private bool notifyInstructionSetUsage(InstructionSet instructionSet, bool supportEnabled)
        {
            instructionSet = InstructionSetFlags.ConvertToImpliedInstructionSetForVectorInstructionSets(_compilation.TypeSystemContext.Target.Architecture, instructionSet);

            Debug.Assert(!_compilation.InstructionSetSupport.NonSpecifiableFlags.HasInstructionSet(instructionSet));

            if (supportEnabled)
            {
                _actualInstructionSetSupported.AddInstructionSet(instructionSet);
            }
            else
            {
                // By policy we code review all changes into corelib, such that failing to use an instruction
                // set is not a reason to not support usage of it.
                if (!isMethodDefinedInCoreLib())
                {
                    _actualInstructionSetUnsupported.AddInstructionSet(instructionSet);
                }
            }
            return supportEnabled;
        }
#else
        private bool notifyInstructionSetUsage(InstructionSet instructionSet, bool supportEnabled)
        {
            instructionSet = InstructionSetFlags.ConvertToImpliedInstructionSetForVectorInstructionSets(_compilation.TypeSystemContext.Target.Architecture, instructionSet);

            Debug.Assert(!_compilation.InstructionSetSupport.NonSpecifiableFlags.HasInstructionSet(instructionSet));

            return supportEnabled ? _compilation.InstructionSetSupport.IsInstructionSetSupported(instructionSet) : false;
        }
#endif
    }
}
