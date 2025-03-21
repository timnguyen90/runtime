// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Xunit;

namespace System.Tests
{
    public class SByteTests_GenericMath
    {
        //
        // IAdditionOperators
        //

        [Fact]
        public static void op_AdditionTest()
        {
            Assert.Equal((sbyte)0x01, AdditionOperatorsHelper<sbyte, sbyte, sbyte>.op_Addition((sbyte)0x00, (sbyte)1));
            Assert.Equal((sbyte)0x02, AdditionOperatorsHelper<sbyte, sbyte, sbyte>.op_Addition((sbyte)0x01, (sbyte)1));
            Assert.Equal(unchecked((sbyte)0x80), AdditionOperatorsHelper<sbyte, sbyte, sbyte>.op_Addition((sbyte)0x7F, (sbyte)1));
            Assert.Equal(unchecked((sbyte)0x81), AdditionOperatorsHelper<sbyte, sbyte, sbyte>.op_Addition(unchecked((sbyte)0x80), (sbyte)1));
            Assert.Equal((sbyte)0x00, AdditionOperatorsHelper<sbyte, sbyte, sbyte>.op_Addition(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void op_CheckedAdditionTest()
        {
            Assert.Equal((sbyte)0x01, AdditionOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedAddition((sbyte)0x00, (sbyte)1));
            Assert.Equal((sbyte)0x02, AdditionOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedAddition((sbyte)0x01, (sbyte)1));
            Assert.Equal(unchecked((sbyte)0x81), AdditionOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedAddition(unchecked((sbyte)0x80), (sbyte)1));
            Assert.Equal((sbyte)0x00, AdditionOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedAddition(unchecked((sbyte)0xFF), (sbyte)1));

            Assert.Throws<OverflowException>(() => AdditionOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedAddition((sbyte)0x7F, (sbyte)1));
        }

        //
        // IAdditiveIdentity
        //

        [Fact]
        public static void AdditiveIdentityTest()
        {
            Assert.Equal((sbyte)0x00, AdditiveIdentityHelper<sbyte, sbyte>.AdditiveIdentity);
        }

        //
        // IBinaryInteger
        //

        [Fact]
        public static void DivRemTest()
        {
            Assert.Equal(((sbyte)0x00, (sbyte)0x00), BinaryIntegerHelper<sbyte>.DivRem((sbyte)0x00, (sbyte)2));
            Assert.Equal(((sbyte)0x00, (sbyte)0x01), BinaryIntegerHelper<sbyte>.DivRem((sbyte)0x01, (sbyte)2));
            Assert.Equal(((sbyte)0x3F, (sbyte)0x01), BinaryIntegerHelper<sbyte>.DivRem((sbyte)0x7F, (sbyte)2));
            Assert.Equal((unchecked((sbyte)0xC0), (sbyte)0x00), BinaryIntegerHelper<sbyte>.DivRem(unchecked((sbyte)0x80), (sbyte)2));
            Assert.Equal(((sbyte)0x00, unchecked((sbyte)0xFF)), BinaryIntegerHelper<sbyte>.DivRem(unchecked((sbyte)0xFF), (sbyte)2));
        }

        [Fact]
        public static void LeadingZeroCountTest()
        {
            Assert.Equal((sbyte)0x08, BinaryIntegerHelper<sbyte>.LeadingZeroCount((sbyte)0x00));
            Assert.Equal((sbyte)0x07, BinaryIntegerHelper<sbyte>.LeadingZeroCount((sbyte)0x01));
            Assert.Equal((sbyte)0x01, BinaryIntegerHelper<sbyte>.LeadingZeroCount((sbyte)0x7F));
            Assert.Equal((sbyte)0x00, BinaryIntegerHelper<sbyte>.LeadingZeroCount(unchecked((sbyte)0x80)));
            Assert.Equal((sbyte)0x00, BinaryIntegerHelper<sbyte>.LeadingZeroCount(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void PopCountTest()
        {
            Assert.Equal((sbyte)0x00, BinaryIntegerHelper<sbyte>.PopCount((sbyte)0x00));
            Assert.Equal((sbyte)0x01, BinaryIntegerHelper<sbyte>.PopCount((sbyte)0x01));
            Assert.Equal((sbyte)0x07, BinaryIntegerHelper<sbyte>.PopCount((sbyte)0x7F));
            Assert.Equal((sbyte)0x01, BinaryIntegerHelper<sbyte>.PopCount(unchecked((sbyte)0x80)));
            Assert.Equal((sbyte)0x08, BinaryIntegerHelper<sbyte>.PopCount(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void RotateLeftTest()
        {
            Assert.Equal((sbyte)0x00, BinaryIntegerHelper<sbyte>.RotateLeft((sbyte)0x00, 1));
            Assert.Equal((sbyte)0x02, BinaryIntegerHelper<sbyte>.RotateLeft((sbyte)0x01, 1));
            Assert.Equal(unchecked((sbyte)0xFE), BinaryIntegerHelper<sbyte>.RotateLeft((sbyte)0x7F, 1));
            Assert.Equal((sbyte)0x01, BinaryIntegerHelper<sbyte>.RotateLeft(unchecked((sbyte)0x80), 1));
            Assert.Equal(unchecked((sbyte)0xFF), BinaryIntegerHelper<sbyte>.RotateLeft(unchecked((sbyte)0xFF), 1));
        }

        [Fact]
        public static void RotateRightTest()
        {
            Assert.Equal((sbyte)0x00, BinaryIntegerHelper<sbyte>.RotateRight((sbyte)0x00, 1));
            Assert.Equal(unchecked((sbyte)0x80), BinaryIntegerHelper<sbyte>.RotateRight((sbyte)0x01, 1));
            Assert.Equal(unchecked((sbyte)0xBF), BinaryIntegerHelper<sbyte>.RotateRight((sbyte)0x7F, 1));
            Assert.Equal((sbyte)0x40, BinaryIntegerHelper<sbyte>.RotateRight(unchecked((sbyte)0x80), 1));
            Assert.Equal(unchecked((sbyte)0xFF), BinaryIntegerHelper<sbyte>.RotateRight(unchecked((sbyte)0xFF), 1));
        }

        [Fact]
        public static void TrailingZeroCountTest()
        {
            Assert.Equal((sbyte)0x08, BinaryIntegerHelper<sbyte>.TrailingZeroCount((sbyte)0x00));
            Assert.Equal((sbyte)0x00, BinaryIntegerHelper<sbyte>.TrailingZeroCount((sbyte)0x01));
            Assert.Equal((sbyte)0x00, BinaryIntegerHelper<sbyte>.TrailingZeroCount((sbyte)0x7F));
            Assert.Equal((sbyte)0x07, BinaryIntegerHelper<sbyte>.TrailingZeroCount(unchecked((sbyte)0x80)));
            Assert.Equal((sbyte)0x00, BinaryIntegerHelper<sbyte>.TrailingZeroCount(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void GetByteCountTest()
        {
            Assert.Equal(1, BinaryIntegerHelper<sbyte>.GetByteCount((sbyte)0x00));
            Assert.Equal(1, BinaryIntegerHelper<sbyte>.GetByteCount((sbyte)0x01));
            Assert.Equal(1, BinaryIntegerHelper<sbyte>.GetByteCount((sbyte)0x7F));
            Assert.Equal(1, BinaryIntegerHelper<sbyte>.GetByteCount(unchecked((sbyte)0x80)));
            Assert.Equal(1, BinaryIntegerHelper<sbyte>.GetByteCount(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void GetShortestBitLengthTest()
        {
            Assert.Equal(0x00, BinaryIntegerHelper<sbyte>.GetShortestBitLength((sbyte)0x00));
            Assert.Equal(0x01, BinaryIntegerHelper<sbyte>.GetShortestBitLength((sbyte)0x01));
            Assert.Equal(0x07, BinaryIntegerHelper<sbyte>.GetShortestBitLength((sbyte)0x7F));
            Assert.Equal(0x08, BinaryIntegerHelper<sbyte>.GetShortestBitLength(unchecked((sbyte)0x80)));
            Assert.Equal(0x01, BinaryIntegerHelper<sbyte>.GetShortestBitLength(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void TryWriteBigEndianTest()
        {
            Span<byte> destination = stackalloc byte[1];
            int bytesWritten = 0;

            Assert.True(BinaryIntegerHelper<sbyte>.TryWriteBigEndian((sbyte)0x00, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x00 }, destination.ToArray());

            Assert.True(BinaryIntegerHelper<sbyte>.TryWriteBigEndian((sbyte)0x01, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x01 }, destination.ToArray());

            Assert.True(BinaryIntegerHelper<sbyte>.TryWriteBigEndian((sbyte)0x7F, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x7F }, destination.ToArray());

            Assert.True(BinaryIntegerHelper<sbyte>.TryWriteBigEndian(unchecked((sbyte)0x80), destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x80 }, destination.ToArray());

            Assert.True(BinaryIntegerHelper<sbyte>.TryWriteBigEndian(unchecked((sbyte)0xFF), destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0xFF }, destination.ToArray());

            Assert.False(BinaryIntegerHelper<sbyte>.TryWriteBigEndian(default, Span<byte>.Empty, out bytesWritten));
            Assert.Equal(0, bytesWritten);
            Assert.Equal(new byte[] { 0xFF }, destination.ToArray());
        }

        [Fact]
        public static void TryWriteLittleEndianTest()
        {
            Span<byte> destination = stackalloc byte[1];
            int bytesWritten = 0;

            Assert.True(BinaryIntegerHelper<sbyte>.TryWriteLittleEndian((sbyte)0x00, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x00 }, destination.ToArray());

            Assert.True(BinaryIntegerHelper<sbyte>.TryWriteLittleEndian((sbyte)0x01, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x01 }, destination.ToArray());

            Assert.True(BinaryIntegerHelper<sbyte>.TryWriteLittleEndian((sbyte)0x7F, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x7F }, destination.ToArray());

            Assert.True(BinaryIntegerHelper<sbyte>.TryWriteLittleEndian(unchecked((sbyte)0x80), destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x80 }, destination.ToArray());

            Assert.True(BinaryIntegerHelper<sbyte>.TryWriteLittleEndian(unchecked((sbyte)0xFF), destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0xFF }, destination.ToArray());

            Assert.False(BinaryIntegerHelper<sbyte>.TryWriteLittleEndian(default, Span<byte>.Empty, out bytesWritten));
            Assert.Equal(0, bytesWritten);
            Assert.Equal(new byte[] { 0xFF }, destination.ToArray());
        }

        //
        // IBinaryNumber
        //

        [Fact]
        public static void IsPow2Test()
        {
            Assert.False(BinaryNumberHelper<sbyte>.IsPow2((sbyte)0x00));
            Assert.True(BinaryNumberHelper<sbyte>.IsPow2((sbyte)0x01));
            Assert.False(BinaryNumberHelper<sbyte>.IsPow2((sbyte)0x7F));
            Assert.False(BinaryNumberHelper<sbyte>.IsPow2(unchecked((sbyte)0x80)));
            Assert.False(BinaryNumberHelper<sbyte>.IsPow2(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void Log2Test()
        {
            Assert.Equal((sbyte)0x00, BinaryNumberHelper<sbyte>.Log2((sbyte)0x00));
            Assert.Equal((sbyte)0x00, BinaryNumberHelper<sbyte>.Log2((sbyte)0x01));
            Assert.Equal((sbyte)0x06, BinaryNumberHelper<sbyte>.Log2((sbyte)0x7F));
            Assert.Throws<ArgumentOutOfRangeException>(() => BinaryNumberHelper<sbyte>.Log2(unchecked((sbyte)0x80)));
            Assert.Throws<ArgumentOutOfRangeException>(() => BinaryNumberHelper<sbyte>.Log2(unchecked((sbyte)0xFF)));
        }

        //
        // IBitwiseOperators
        //

        [Fact]
        public static void op_BitwiseAndTest()
        {
            Assert.Equal((sbyte)0x00, BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_BitwiseAnd((sbyte)0x00, (sbyte)1));
            Assert.Equal((sbyte)0x01, BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_BitwiseAnd((sbyte)0x01, (sbyte)1));
            Assert.Equal((sbyte)0x01, BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_BitwiseAnd((sbyte)0x7F, (sbyte)1));
            Assert.Equal((sbyte)0x00, BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_BitwiseAnd(unchecked((sbyte)0x80), (sbyte)1));
            Assert.Equal((sbyte)0x01, BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_BitwiseAnd(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void op_BitwiseOrTest()
        {
            Assert.Equal((sbyte)0x01, BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_BitwiseOr((sbyte)0x00, (sbyte)1));
            Assert.Equal((sbyte)0x01, BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_BitwiseOr((sbyte)0x01, (sbyte)1));
            Assert.Equal((sbyte)0x7F, BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_BitwiseOr((sbyte)0x7F, (sbyte)1));
            Assert.Equal(unchecked((sbyte)0x81), BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_BitwiseOr(unchecked((sbyte)0x80), (sbyte)1));
            Assert.Equal(unchecked((sbyte)0xFF), BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_BitwiseOr(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void op_ExclusiveOrTest()
        {
            Assert.Equal((sbyte)0x01, BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_ExclusiveOr((sbyte)0x00, (sbyte)1));
            Assert.Equal((sbyte)0x00, BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_ExclusiveOr((sbyte)0x01, (sbyte)1));
            Assert.Equal((sbyte)0x7E, BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_ExclusiveOr((sbyte)0x7F, (sbyte)1));
            Assert.Equal(unchecked((sbyte)0x81), BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_ExclusiveOr(unchecked((sbyte)0x80), (sbyte)1));
            Assert.Equal(unchecked((sbyte)0xFE), BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_ExclusiveOr(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void op_OnesComplementTest()
        {
            Assert.Equal(unchecked((sbyte)0xFF), BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_OnesComplement((sbyte)0x00));
            Assert.Equal(unchecked((sbyte)0xFE), BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_OnesComplement((sbyte)0x01));
            Assert.Equal(unchecked((sbyte)0x80), BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_OnesComplement((sbyte)0x7F));
            Assert.Equal((sbyte)0x7F, BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_OnesComplement(unchecked((sbyte)0x80)));
            Assert.Equal((sbyte)0x00, BitwiseOperatorsHelper<sbyte, sbyte, sbyte>.op_OnesComplement(unchecked((sbyte)0xFF)));
        }

        //
        // IComparisonOperators
        //

        [Fact]
        public static void op_GreaterThanTest()
        {
            Assert.False(ComparisonOperatorsHelper<sbyte, sbyte>.op_GreaterThan((sbyte)0x00, (sbyte)1));
            Assert.False(ComparisonOperatorsHelper<sbyte, sbyte>.op_GreaterThan((sbyte)0x01, (sbyte)1));
            Assert.True(ComparisonOperatorsHelper<sbyte, sbyte>.op_GreaterThan((sbyte)0x7F, (sbyte)1));
            Assert.False(ComparisonOperatorsHelper<sbyte, sbyte>.op_GreaterThan(unchecked((sbyte)0x80), (sbyte)1));
            Assert.False(ComparisonOperatorsHelper<sbyte, sbyte>.op_GreaterThan(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void op_GreaterThanOrEqualTest()
        {
            Assert.False(ComparisonOperatorsHelper<sbyte, sbyte>.op_GreaterThanOrEqual((sbyte)0x00, (sbyte)1));
            Assert.True(ComparisonOperatorsHelper<sbyte, sbyte>.op_GreaterThanOrEqual((sbyte)0x01, (sbyte)1));
            Assert.True(ComparisonOperatorsHelper<sbyte, sbyte>.op_GreaterThanOrEqual((sbyte)0x7F, (sbyte)1));
            Assert.False(ComparisonOperatorsHelper<sbyte, sbyte>.op_GreaterThanOrEqual(unchecked((sbyte)0x80), (sbyte)1));
            Assert.False(ComparisonOperatorsHelper<sbyte, sbyte>.op_GreaterThanOrEqual(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void op_LessThanTest()
        {
            Assert.True(ComparisonOperatorsHelper<sbyte, sbyte>.op_LessThan((sbyte)0x00, (sbyte)1));
            Assert.False(ComparisonOperatorsHelper<sbyte, sbyte>.op_LessThan((sbyte)0x01, (sbyte)1));
            Assert.False(ComparisonOperatorsHelper<sbyte, sbyte>.op_LessThan((sbyte)0x7F, (sbyte)1));
            Assert.True(ComparisonOperatorsHelper<sbyte, sbyte>.op_LessThan(unchecked((sbyte)0x80), (sbyte)1));
            Assert.True(ComparisonOperatorsHelper<sbyte, sbyte>.op_LessThan(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void op_LessThanOrEqualTest()
        {
            Assert.True(ComparisonOperatorsHelper<sbyte, sbyte>.op_LessThanOrEqual((sbyte)0x00, (sbyte)1));
            Assert.True(ComparisonOperatorsHelper<sbyte, sbyte>.op_LessThanOrEqual((sbyte)0x01, (sbyte)1));
            Assert.False(ComparisonOperatorsHelper<sbyte, sbyte>.op_LessThanOrEqual((sbyte)0x7F, (sbyte)1));
            Assert.True(ComparisonOperatorsHelper<sbyte, sbyte>.op_LessThanOrEqual(unchecked((sbyte)0x80), (sbyte)1));
            Assert.True(ComparisonOperatorsHelper<sbyte, sbyte>.op_LessThanOrEqual(unchecked((sbyte)0xFF), (sbyte)1));
        }

        //
        // IDecrementOperators
        //

        [Fact]
        public static void op_DecrementTest()
        {
            Assert.Equal(unchecked((sbyte)0xFF), DecrementOperatorsHelper<sbyte>.op_Decrement((sbyte)0x00));
            Assert.Equal((sbyte)0x00, DecrementOperatorsHelper<sbyte>.op_Decrement((sbyte)0x01));
            Assert.Equal((sbyte)0x7E, DecrementOperatorsHelper<sbyte>.op_Decrement((sbyte)0x7F));
            Assert.Equal((sbyte)0x7F, DecrementOperatorsHelper<sbyte>.op_Decrement(unchecked((sbyte)0x80)));
            Assert.Equal(unchecked((sbyte)0xFE), DecrementOperatorsHelper<sbyte>.op_Decrement(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void op_CheckedDecrementTest()
        {
            Assert.Equal(unchecked((sbyte)0xFF), DecrementOperatorsHelper<sbyte>.op_CheckedDecrement((sbyte)0x00));
            Assert.Equal((sbyte)0x00, DecrementOperatorsHelper<sbyte>.op_CheckedDecrement((sbyte)0x01));
            Assert.Equal((sbyte)0x7E, DecrementOperatorsHelper<sbyte>.op_CheckedDecrement((sbyte)0x7F));
            Assert.Equal(unchecked((sbyte)0xFE), DecrementOperatorsHelper<sbyte>.op_CheckedDecrement(unchecked((sbyte)0xFF)));

            Assert.Throws<OverflowException>(() => DecrementOperatorsHelper<sbyte>.op_CheckedDecrement(unchecked((sbyte)0x80)));
        }

        //
        // IDivisionOperators
        //

        [Fact]
        public static void op_DivisionTest()
        {
            Assert.Equal((sbyte)0x00, DivisionOperatorsHelper<sbyte, sbyte, sbyte>.op_Division((sbyte)0x00, (sbyte)2));
            Assert.Equal((sbyte)0x00, DivisionOperatorsHelper<sbyte, sbyte, sbyte>.op_Division((sbyte)0x01, (sbyte)2));
            Assert.Equal((sbyte)0x3F, DivisionOperatorsHelper<sbyte, sbyte, sbyte>.op_Division((sbyte)0x7F, (sbyte)2));
            Assert.Equal(unchecked((sbyte)0xC0), DivisionOperatorsHelper<sbyte, sbyte, sbyte>.op_Division(unchecked((sbyte)0x80), (sbyte)2));
            Assert.Equal((sbyte)0x00, DivisionOperatorsHelper<sbyte, sbyte, sbyte>.op_Division(unchecked((sbyte)0xFF), (sbyte)2));

            Assert.Throws<DivideByZeroException>(() => DivisionOperatorsHelper<sbyte, sbyte, sbyte>.op_Division((sbyte)0x01, (sbyte)0));
        }

        [Fact]
        public static void op_CheckedDivisionTest()
        {
            Assert.Equal((sbyte)0x00, DivisionOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedDivision((sbyte)0x00, (sbyte)2));
            Assert.Equal((sbyte)0x00, DivisionOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedDivision((sbyte)0x01, (sbyte)2));
            Assert.Equal((sbyte)0x3F, DivisionOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedDivision((sbyte)0x7F, (sbyte)2));
            Assert.Equal(unchecked((sbyte)0xC0), DivisionOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedDivision(unchecked((sbyte)0x80), (sbyte)2));
            Assert.Equal((sbyte)0x00, DivisionOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedDivision(unchecked((sbyte)0xFF), (sbyte)2));

            Assert.Throws<DivideByZeroException>(() => DivisionOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedDivision((sbyte)0x01, (sbyte)0));
        }

        //
        // IEqualityOperators
        //

        [Fact]
        public static void op_EqualityTest()
        {
            Assert.False(EqualityOperatorsHelper<sbyte, sbyte>.op_Equality((sbyte)0x00, (sbyte)1));
            Assert.True(EqualityOperatorsHelper<sbyte, sbyte>.op_Equality((sbyte)0x01, (sbyte)1));
            Assert.False(EqualityOperatorsHelper<sbyte, sbyte>.op_Equality((sbyte)0x7F, (sbyte)1));
            Assert.False(EqualityOperatorsHelper<sbyte, sbyte>.op_Equality(unchecked((sbyte)0x80), (sbyte)1));
            Assert.False(EqualityOperatorsHelper<sbyte, sbyte>.op_Equality(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void op_InequalityTest()
        {
            Assert.True(EqualityOperatorsHelper<sbyte, sbyte>.op_Inequality((sbyte)0x00, (sbyte)1));
            Assert.False(EqualityOperatorsHelper<sbyte, sbyte>.op_Inequality((sbyte)0x01, (sbyte)1));
            Assert.True(EqualityOperatorsHelper<sbyte, sbyte>.op_Inequality((sbyte)0x7F, (sbyte)1));
            Assert.True(EqualityOperatorsHelper<sbyte, sbyte>.op_Inequality(unchecked((sbyte)0x80), (sbyte)1));
            Assert.True(EqualityOperatorsHelper<sbyte, sbyte>.op_Inequality(unchecked((sbyte)0xFF), (sbyte)1));
        }

        //
        // IIncrementOperators
        //

        [Fact]
        public static void op_IncrementTest()
        {
            Assert.Equal((sbyte)0x01, IncrementOperatorsHelper<sbyte>.op_Increment((sbyte)0x00));
            Assert.Equal((sbyte)0x02, IncrementOperatorsHelper<sbyte>.op_Increment((sbyte)0x01));
            Assert.Equal(unchecked((sbyte)0x80), IncrementOperatorsHelper<sbyte>.op_Increment((sbyte)0x7F));
            Assert.Equal(unchecked((sbyte)0x81), IncrementOperatorsHelper<sbyte>.op_Increment(unchecked((sbyte)0x80)));
            Assert.Equal((sbyte)0x00, IncrementOperatorsHelper<sbyte>.op_Increment(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void op_CheckedIncrementTest()
        {
            Assert.Equal((sbyte)0x01, IncrementOperatorsHelper<sbyte>.op_CheckedIncrement((sbyte)0x00));
            Assert.Equal((sbyte)0x02, IncrementOperatorsHelper<sbyte>.op_CheckedIncrement((sbyte)0x01));
            Assert.Equal(unchecked((sbyte)0x81), IncrementOperatorsHelper<sbyte>.op_CheckedIncrement(unchecked((sbyte)0x80)));
            Assert.Equal((sbyte)0x00, IncrementOperatorsHelper<sbyte>.op_CheckedIncrement(unchecked((sbyte)0xFF)));

            Assert.Throws<OverflowException>(() => IncrementOperatorsHelper<sbyte>.op_CheckedIncrement((sbyte)0x7F));
        }

        //
        // IMinMaxValue
        //

        [Fact]
        public static void MaxValueTest()
        {
            Assert.Equal((sbyte)0x7F, MinMaxValueHelper<sbyte>.MaxValue);
        }

        [Fact]
        public static void MinValueTest()
        {
            Assert.Equal(unchecked((sbyte)0x80), MinMaxValueHelper<sbyte>.MinValue);
        }

        //
        // IModulusOperators
        //

        [Fact]
        public static void op_ModulusTest()
        {
            Assert.Equal((sbyte)0x00, ModulusOperatorsHelper<sbyte, sbyte, sbyte>.op_Modulus((sbyte)0x00, (sbyte)2));
            Assert.Equal((sbyte)0x01, ModulusOperatorsHelper<sbyte, sbyte, sbyte>.op_Modulus((sbyte)0x01, (sbyte)2));
            Assert.Equal((sbyte)0x01, ModulusOperatorsHelper<sbyte, sbyte, sbyte>.op_Modulus((sbyte)0x7F, (sbyte)2));
            Assert.Equal((sbyte)0x00, ModulusOperatorsHelper<sbyte, sbyte, sbyte>.op_Modulus(unchecked((sbyte)0x80), (sbyte)2));
            Assert.Equal(unchecked((sbyte)0xFF), ModulusOperatorsHelper<sbyte, sbyte, sbyte>.op_Modulus(unchecked((sbyte)0xFF), (sbyte)2));

            Assert.Throws<DivideByZeroException>(() => ModulusOperatorsHelper<sbyte, sbyte, sbyte>.op_Modulus((sbyte)0x01, (sbyte)0));
        }

        //
        // IMultiplicativeIdentity
        //

        [Fact]
        public static void MultiplicativeIdentityTest()
        {
            Assert.Equal((sbyte)0x01, MultiplicativeIdentityHelper<sbyte, sbyte>.MultiplicativeIdentity);
        }

        //
        // IMultiplyOperators
        //

        [Fact]
        public static void op_MultiplyTest()
        {
            Assert.Equal((sbyte)0x00, MultiplyOperatorsHelper<sbyte, sbyte, sbyte>.op_Multiply((sbyte)0x00, (sbyte)2));
            Assert.Equal((sbyte)0x02, MultiplyOperatorsHelper<sbyte, sbyte, sbyte>.op_Multiply((sbyte)0x01, (sbyte)2));
            Assert.Equal(unchecked((sbyte)0xFE), MultiplyOperatorsHelper<sbyte, sbyte, sbyte>.op_Multiply((sbyte)0x7F, (sbyte)2));
            Assert.Equal((sbyte)0x00, MultiplyOperatorsHelper<sbyte, sbyte, sbyte>.op_Multiply(unchecked((sbyte)0x80), (sbyte)2));
            Assert.Equal(unchecked((sbyte)0xFE), MultiplyOperatorsHelper<sbyte, sbyte, sbyte>.op_Multiply(unchecked((sbyte)0xFF), (sbyte)2));
        }

        [Fact]
        public static void op_CheckedMultiplyTest()
        {
            Assert.Equal((sbyte)0x00, MultiplyOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedMultiply((sbyte)0x00, (sbyte)2));
            Assert.Equal((sbyte)0x02, MultiplyOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedMultiply((sbyte)0x01, (sbyte)2));
            Assert.Equal(unchecked((sbyte)0xFE), MultiplyOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedMultiply(unchecked((sbyte)0xFF), (sbyte)2));

            Assert.Throws<OverflowException>(() => MultiplyOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedMultiply((sbyte)0x7F, (sbyte)2));
            Assert.Throws<OverflowException>(() => MultiplyOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedMultiply(unchecked((sbyte)0x80), (sbyte)2));
        }

        //
        // INumber
        //

        [Fact]
        public static void ClampTest()
        {
            Assert.Equal((sbyte)0x00, NumberHelper<sbyte>.Clamp((sbyte)0x00, unchecked((sbyte)0xC0), (sbyte)0x3F));
            Assert.Equal((sbyte)0x01, NumberHelper<sbyte>.Clamp((sbyte)0x01, unchecked((sbyte)0xC0), (sbyte)0x3F));
            Assert.Equal((sbyte)0x3F, NumberHelper<sbyte>.Clamp((sbyte)0x7F, unchecked((sbyte)0xC0), (sbyte)0x3F));
            Assert.Equal(unchecked((sbyte)0xC0), NumberHelper<sbyte>.Clamp(unchecked((sbyte)0x80), unchecked((sbyte)0xC0), (sbyte)0x3F));
            Assert.Equal(unchecked((sbyte)0xFF), NumberHelper<sbyte>.Clamp(unchecked((sbyte)0xFF), unchecked((sbyte)0xC0), (sbyte)0x3F));
        }

        [Fact]
        public static void MaxTest()
        {
            Assert.Equal((sbyte)0x01, NumberHelper<sbyte>.Max((sbyte)0x00, (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberHelper<sbyte>.Max((sbyte)0x01, (sbyte)1));
            Assert.Equal((sbyte)0x7F, NumberHelper<sbyte>.Max((sbyte)0x7F, (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberHelper<sbyte>.Max(unchecked((sbyte)0x80), (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberHelper<sbyte>.Max(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void MaxNumberTest()
        {
            Assert.Equal((sbyte)0x01, NumberHelper<sbyte>.MaxNumber((sbyte)0x00, (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberHelper<sbyte>.MaxNumber((sbyte)0x01, (sbyte)1));
            Assert.Equal((sbyte)0x7F, NumberHelper<sbyte>.MaxNumber((sbyte)0x7F, (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberHelper<sbyte>.MaxNumber(unchecked((sbyte)0x80), (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberHelper<sbyte>.MaxNumber(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void MinTest()
        {
            Assert.Equal((sbyte)0x00, NumberHelper<sbyte>.Min((sbyte)0x00, (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberHelper<sbyte>.Min((sbyte)0x01, (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberHelper<sbyte>.Min((sbyte)0x7F, (sbyte)1));
            Assert.Equal(unchecked((sbyte)0x80), NumberHelper<sbyte>.Min(unchecked((sbyte)0x80), (sbyte)1));
            Assert.Equal(unchecked((sbyte)0xFF), NumberHelper<sbyte>.Min(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void MinNumberTest()
        {
            Assert.Equal((sbyte)0x00, NumberHelper<sbyte>.MinNumber((sbyte)0x00, (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberHelper<sbyte>.MinNumber((sbyte)0x01, (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberHelper<sbyte>.MinNumber((sbyte)0x7F, (sbyte)1));
            Assert.Equal(unchecked((sbyte)0x80), NumberHelper<sbyte>.MinNumber(unchecked((sbyte)0x80), (sbyte)1));
            Assert.Equal(unchecked((sbyte)0xFF), NumberHelper<sbyte>.MinNumber(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void SignTest()
        {
            Assert.Equal(0, NumberHelper<sbyte>.Sign((sbyte)0x00));
            Assert.Equal(1, NumberHelper<sbyte>.Sign((sbyte)0x01));
            Assert.Equal(1, NumberHelper<sbyte>.Sign((sbyte)0x7F));
            Assert.Equal(-1, NumberHelper<sbyte>.Sign(unchecked((sbyte)0x80)));
            Assert.Equal(-1, NumberHelper<sbyte>.Sign(unchecked((sbyte)0xFF)));
        }

        //
        // INumberBase
        //

        [Fact]
        public static void OneTest()
        {
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.One);
        }

        [Fact]
        public static void ZeroTest()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.Zero);
        }

        [Fact]
        public static void AbsTest()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.Abs((sbyte)0x00));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.Abs((sbyte)0x01));
            Assert.Equal((sbyte)0x7F, NumberBaseHelper<sbyte>.Abs((sbyte)0x7F));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.Abs(unchecked((sbyte)0x80)));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.Abs(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void CreateCheckedFromByteTest()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateChecked<byte>(0x00));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateChecked<byte>(0x01));
            Assert.Equal((sbyte)0x7F, NumberBaseHelper<sbyte>.CreateChecked<byte>(0x7F));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<byte>(0x80));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<byte>(0xFF));
        }

        [Fact]
        public static void CreateCheckedFromCharTest()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateChecked<char>((char)0x0000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateChecked<char>((char)0x0001));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<char>((char)0x7FFF));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<char>((char)0x8000));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<char>((char)0xFFFF));
        }

        [Fact]
        public static void CreateCheckedFromInt16Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateChecked<short>(0x0000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateChecked<short>(0x0001));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<short>(0x7FFF));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<short>(unchecked((short)0x8000)));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateChecked<short>(unchecked((short)0xFFFF)));
        }

        [Fact]
        public static void CreateCheckedFromInt32Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateChecked<int>(0x00000000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateChecked<int>(0x00000001));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<int>(0x7FFFFFFF));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<int>(unchecked((int)0x80000000)));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateChecked<int>(unchecked((int)0xFFFFFFFF)));
        }

        [Fact]
        public static void CreateCheckedFromInt64Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateChecked<long>(0x0000000000000000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateChecked<long>(0x0000000000000001));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<long>(0x7FFFFFFFFFFFFFFF));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<long>(unchecked((long)0x8000000000000000)));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateChecked<long>(unchecked((long)0xFFFFFFFFFFFFFFFF)));
        }

        [Fact]
        public static void CreateCheckedFromIntPtrTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateChecked<nint>(unchecked((nint)0x0000000000000000)));
                Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateChecked<nint>(unchecked((nint)0x0000000000000001)));
                Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<nint>(unchecked((nint)0x7FFFFFFFFFFFFFFF)));
                Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<nint>(unchecked((nint)0x8000000000000000)));
                Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateChecked<nint>(unchecked((nint)0xFFFFFFFFFFFFFFFF)));
            }
            else
            {
                Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateChecked<nint>((nint)0x00000000));
                Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateChecked<nint>((nint)0x00000001));
                Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<nint>((nint)0x7FFFFFFF));
                Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<nint>(unchecked((nint)0x80000000)));
                Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateChecked<nint>(unchecked((nint)0xFFFFFFFF)));
            }
        }

        [Fact]
        public static void CreateCheckedFromSByteTest()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateChecked<sbyte>(0x00));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateChecked<sbyte>(0x01));
            Assert.Equal((sbyte)0x7F, NumberBaseHelper<sbyte>.CreateChecked<sbyte>(0x7F));
            Assert.Equal(unchecked((sbyte)0x80), NumberBaseHelper<sbyte>.CreateChecked<sbyte>(unchecked((sbyte)0x80)));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateChecked<sbyte>(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void CreateCheckedFromUInt16Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateChecked<ushort>(0x0000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateChecked<ushort>(0x0001));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<ushort>(0x7FFF));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<ushort>(0x8000));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<ushort>(0xFFFF));
        }

        [Fact]
        public static void CreateCheckedFromUInt32Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateChecked<uint>(0x00000000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateChecked<uint>(0x00000001));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<uint>(0x7FFFFFFF));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<uint>(0x80000000));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<uint>(0xFFFFFFFF));
        }

        [Fact]
        public static void CreateCheckedFromUInt64Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateChecked<ulong>(0x0000000000000000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateChecked<ulong>(0x0000000000000001));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<ulong>(0x7FFFFFFFFFFFFFFF));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<ulong>(0x8000000000000000));
            Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<ulong>(0xFFFFFFFFFFFFFFFF));
        }

        [Fact]
        public static void CreateCheckedFromUIntPtrTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateChecked<nuint>(unchecked((nuint)0x0000000000000000)));
                Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateChecked<nuint>(unchecked((nuint)0x0000000000000001)));
                Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<nuint>(unchecked((nuint)0x7FFFFFFFFFFFFFFF)));
                Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<nuint>(unchecked((nuint)0x8000000000000000)));
                Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<nuint>(unchecked((nuint)0xFFFFFFFFFFFFFFFF)));
            }
            else
            {
                Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateChecked<nuint>((nuint)0x00000000));
                Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateChecked<nuint>((nuint)0x00000001));
                Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<nuint>((nuint)0x7FFFFFFF));
                Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<nuint>((nuint)0x80000000));
                Assert.Throws<OverflowException>(() => NumberBaseHelper<sbyte>.CreateChecked<nuint>((nuint)0xFFFFFFFF));
            }
        }

        [Fact]
        public static void CreateSaturatingFromByteTest()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateSaturating<byte>(0x00));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateSaturating<byte>(0x01));
            Assert.Equal((sbyte)0x7F, NumberBaseHelper<sbyte>.CreateSaturating<byte>(0x7F));
            Assert.Equal((sbyte)0x7F, NumberBaseHelper<sbyte>.CreateSaturating<byte>(0x80));
            Assert.Equal((sbyte)0x7F, NumberBaseHelper<sbyte>.CreateSaturating<byte>(0xFF));
        }

        [Fact]
        public static void CreateSaturatingFromCharTest()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateSaturating<char>((char)0x0000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateSaturating<char>((char)0x0001));
            Assert.Equal((sbyte)0x7F, NumberBaseHelper<sbyte>.CreateSaturating<char>((char)0x7FFF));
            Assert.Equal((sbyte)0x7F, NumberBaseHelper<sbyte>.CreateSaturating<char>((char)0x8000));
            Assert.Equal((sbyte)0x7F, NumberBaseHelper<sbyte>.CreateSaturating<char>((char)0xFFFF));
        }

        [Fact]
        public static void CreateSaturatingFromInt16Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateSaturating<short>(0x0000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateSaturating<short>(0x0001));
            Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<short>(0x7FFF));
            Assert.Equal(unchecked((sbyte)0x80), NumberBaseHelper<sbyte>.CreateSaturating<short>(unchecked((short)0x8000)));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateSaturating<short>(unchecked((short)0xFFFF)));
        }

        [Fact]
        public static void CreateSaturatingFromInt32Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateSaturating<int>(0x00000000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateSaturating<int>(0x00000001));
            Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<int>(0x7FFFFFFF));
            Assert.Equal(unchecked((sbyte)0x80), NumberBaseHelper<sbyte>.CreateSaturating<int>(unchecked((int)0x80000000)));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateSaturating<int>(unchecked((int)0xFFFFFFFF)));
        }

        [Fact]
        public static void CreateSaturatingFromInt64Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateSaturating<long>(0x0000000000000000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateSaturating<long>(0x0000000000000001));
            Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<long>(0x7FFFFFFFFFFFFFFF));
            Assert.Equal(unchecked((sbyte)0x80), NumberBaseHelper<sbyte>.CreateSaturating<long>(unchecked((long)0x8000000000000000)));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateSaturating<long>(unchecked((long)0xFFFFFFFFFFFFFFFF)));
        }

        [Fact]
        public static void CreateSaturatingFromIntPtrTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateSaturating<nint>(unchecked((nint)0x0000000000000000)));
                Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateSaturating<nint>(unchecked((nint)0x0000000000000001)));
                Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<nint>(unchecked((nint)0x7FFFFFFFFFFFFFFF)));
                Assert.Equal(unchecked((sbyte)0x80), NumberBaseHelper<sbyte>.CreateSaturating<nint>(unchecked((nint)0x8000000000000000)));
                Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateSaturating<nint>(unchecked((nint)0xFFFFFFFFFFFFFFFF)));
            }
            else
            {
                Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateSaturating<nint>((nint)0x00000000));
                Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateSaturating<nint>((nint)0x00000001));
                Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<nint>((nint)0x7FFFFFFF));
                Assert.Equal(unchecked((sbyte)0x80), NumberBaseHelper<sbyte>.CreateSaturating<nint>(unchecked((nint)0x80000000)));
                Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateSaturating<nint>(unchecked((nint)0xFFFFFFFF)));
            }
        }

        [Fact]
        public static void CreateSaturatingFromSByteTest()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateSaturating<sbyte>(0x00));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateSaturating<sbyte>(0x01));
            Assert.Equal((sbyte)0x7F, NumberBaseHelper<sbyte>.CreateSaturating<sbyte>(0x7F));
            Assert.Equal(unchecked((sbyte)0x80), NumberBaseHelper<sbyte>.CreateSaturating<sbyte>(unchecked((sbyte)0x80)));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateSaturating<sbyte>(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void CreateSaturatingFromUInt16Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateSaturating<ushort>(0x0000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateSaturating<ushort>(0x0001));
            Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<ushort>(0x7FFF));
            Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<ushort>(0x8000));
            Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<ushort>(0xFFFF));
        }

        [Fact]
        public static void CreateSaturatingFromUInt32Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateSaturating<uint>(0x00000000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateSaturating<uint>(0x00000001));
            Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<uint>(0x7FFFFFFF));
            Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<uint>(0x80000000));
            Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<uint>(0xFFFFFFFF));
        }

        [Fact]
        public static void CreateSaturatingFromUInt64Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateSaturating<ulong>(0x0000000000000000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateSaturating<ulong>(0x0000000000000001));
            Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<ulong>(0x7FFFFFFFFFFFFFFF));
            Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<ulong>(0x8000000000000000));
            Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<ulong>(0xFFFFFFFFFFFFFFFF));
        }

        [Fact]
        public static void CreateSaturatingFromUIntPtrTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateSaturating<nuint>(unchecked((nuint)0x0000000000000000)));
                Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateSaturating<nuint>(unchecked((nuint)0x0000000000000001)));
                Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<nuint>(unchecked((nuint)0x7FFFFFFFFFFFFFFF)));
                Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<nuint>(unchecked((nuint)0x8000000000000000)));
                Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<nuint>(unchecked((nuint)0xFFFFFFFFFFFFFFFF)));
            }
            else
            {
                Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateSaturating<nuint>((nuint)0x00000000));
                Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateSaturating<nuint>((nuint)0x00000001));
                Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<nuint>((nuint)0x7FFFFFFF));
                Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<nuint>((nuint)0x80000000));
                Assert.Equal(unchecked((sbyte)0x7F), NumberBaseHelper<sbyte>.CreateSaturating<nuint>((nuint)0xFFFFFFFF));
            }
        }

        [Fact]
        public static void CreateTruncatingFromByteTest()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<byte>(0x00));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateTruncating<byte>(0x01));
            Assert.Equal((sbyte)0x7F, NumberBaseHelper<sbyte>.CreateTruncating<byte>(0x7F));
            Assert.Equal(unchecked((sbyte)0x80), NumberBaseHelper<sbyte>.CreateTruncating<byte>(0x80));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<byte>(0xFF));
        }

        [Fact]
        public static void CreateTruncatingFromCharTest()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<char>((char)0x0000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateTruncating<char>((char)0x0001));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<char>((char)0x7FFF));
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<char>((char)0x8000));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<char>((char)0xFFFF));
        }

        [Fact]
        public static void CreateTruncatingFromInt16Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<short>(0x0000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateTruncating<short>(0x0001));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<short>(0x7FFF));
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<short>(unchecked((short)0x8000)));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<short>(unchecked((short)0xFFFF)));
        }

        [Fact]
        public static void CreateTruncatingFromInt32Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<int>(0x00000000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateTruncating<int>(0x00000001));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<int>(0x7FFFFFFF));
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<int>(unchecked((int)0x80000000)));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<int>(unchecked((int)0xFFFFFFFF)));
        }

        [Fact]
        public static void CreateTruncatingFromInt64Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<long>(0x0000000000000000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateTruncating<long>(0x0000000000000001));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<long>(0x7FFFFFFFFFFFFFFF));
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<long>(unchecked((long)0x8000000000000000)));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<long>(unchecked((long)0xFFFFFFFFFFFFFFFF)));
        }

        [Fact]
        public static void CreateTruncatingFromIntPtrTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<nint>(unchecked((nint)0x0000000000000000)));
                Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateTruncating<nint>(unchecked((nint)0x0000000000000001)));
                Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<nint>(unchecked((nint)0x7FFFFFFFFFFFFFFF)));
                Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<nint>(unchecked((nint)0x8000000000000000)));
                Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<nint>(unchecked((nint)0xFFFFFFFFFFFFFFFF)));
            }
            else
            {
                Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<nint>((nint)0x00000000));
                Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateTruncating<nint>((nint)0x00000001));
                Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<nint>((nint)0x7FFFFFFF));
                Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<nint>(unchecked((nint)0x80000000)));
                Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<nint>(unchecked((nint)0xFFFFFFFF)));
            }
        }

        [Fact]
        public static void CreateTruncatingFromSByteTest()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<sbyte>(0x00));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateTruncating<sbyte>(0x01));
            Assert.Equal((sbyte)0x7F, NumberBaseHelper<sbyte>.CreateTruncating<sbyte>(0x7F));
            Assert.Equal(unchecked((sbyte)0x80), NumberBaseHelper<sbyte>.CreateTruncating<sbyte>(unchecked((sbyte)0x80)));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<sbyte>(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void CreateTruncatingFromUInt16Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<ushort>(0x0000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateTruncating<ushort>(0x0001));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<ushort>(0x7FFF));
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<ushort>(0x8000));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<ushort>(0xFFFF));
        }

        [Fact]
        public static void CreateTruncatingFromUInt32Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<uint>(0x00000000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateTruncating<uint>(0x00000001));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<uint>(0x7FFFFFFF));
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<uint>(0x80000000));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<uint>(0xFFFFFFFF));
        }

        [Fact]
        public static void CreateTruncatingFromUInt64Test()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<ulong>(0x0000000000000000));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateTruncating<ulong>(0x0000000000000001));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<ulong>(0x7FFFFFFFFFFFFFFF));
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<ulong>(0x8000000000000000));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<ulong>(0xFFFFFFFFFFFFFFFF));
        }

        [Fact]
        public static void CreateTruncatingFromUIntPtrTest()
        {
            if (Environment.Is64BitProcess)
            {
                Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<nuint>(unchecked((nuint)0x0000000000000000)));
                Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateTruncating<nuint>(unchecked((nuint)0x0000000000000001)));
                Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<nuint>(unchecked((nuint)0x7FFFFFFFFFFFFFFF)));
                Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<nuint>(unchecked((nuint)0x8000000000000000)));
                Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<nuint>(unchecked((nuint)0xFFFFFFFFFFFFFFFF)));
            }
            else
            {
                Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<nuint>((nuint)0x00000000));
                Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.CreateTruncating<nuint>((nuint)0x00000001));
                Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<nuint>((nuint)0x7FFFFFFF));
                Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.CreateTruncating<nuint>((nuint)0x80000000));
                Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.CreateTruncating<nuint>((nuint)0xFFFFFFFF));
            }
        }

        [Fact]
        public static void IsFiniteTest()
        {
            Assert.True(NumberBaseHelper<sbyte>.IsFinite((sbyte)0x00));
            Assert.True(NumberBaseHelper<sbyte>.IsFinite((sbyte)0x01));
            Assert.True(NumberBaseHelper<sbyte>.IsFinite((sbyte)0x7F));
            Assert.True(NumberBaseHelper<sbyte>.IsFinite(unchecked((sbyte)0x80)));
            Assert.True(NumberBaseHelper<sbyte>.IsFinite(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void IsInfinityTest()
        {
            Assert.False(NumberBaseHelper<sbyte>.IsInfinity((sbyte)0x00));
            Assert.False(NumberBaseHelper<sbyte>.IsInfinity((sbyte)0x01));
            Assert.False(NumberBaseHelper<sbyte>.IsInfinity((sbyte)0x7F));
            Assert.False(NumberBaseHelper<sbyte>.IsInfinity(unchecked((sbyte)0x80)));
            Assert.False(NumberBaseHelper<sbyte>.IsInfinity(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void IsNaNTest()
        {
            Assert.False(NumberBaseHelper<sbyte>.IsNaN((sbyte)0x00));
            Assert.False(NumberBaseHelper<sbyte>.IsNaN((sbyte)0x01));
            Assert.False(NumberBaseHelper<sbyte>.IsNaN((sbyte)0x7F));
            Assert.False(NumberBaseHelper<sbyte>.IsNaN(unchecked((sbyte)0x80)));
            Assert.False(NumberBaseHelper<sbyte>.IsNaN(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void IsNegativeTest()
        {
            Assert.False(NumberBaseHelper<sbyte>.IsNegative((sbyte)0x00));
            Assert.False(NumberBaseHelper<sbyte>.IsNegative((sbyte)0x01));
            Assert.False(NumberBaseHelper<sbyte>.IsNegative((sbyte)0x7F));
            Assert.True(NumberBaseHelper<sbyte>.IsNegative(unchecked((sbyte)0x80)));
            Assert.True(NumberBaseHelper<sbyte>.IsNegative(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void IsNegativeInfinityTest()
        {
            Assert.False(NumberBaseHelper<sbyte>.IsNegativeInfinity((sbyte)0x00));
            Assert.False(NumberBaseHelper<sbyte>.IsNegativeInfinity((sbyte)0x01));
            Assert.False(NumberBaseHelper<sbyte>.IsNegativeInfinity((sbyte)0x7F));
            Assert.False(NumberBaseHelper<sbyte>.IsNegativeInfinity(unchecked((sbyte)0x80)));
            Assert.False(NumberBaseHelper<sbyte>.IsNegativeInfinity(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void IsNormalTest()
        {
            Assert.False(NumberBaseHelper<sbyte>.IsNormal((sbyte)0x00));
            Assert.True(NumberBaseHelper<sbyte>.IsNormal((sbyte)0x01));
            Assert.True(NumberBaseHelper<sbyte>.IsNormal((sbyte)0x7F));
            Assert.True(NumberBaseHelper<sbyte>.IsNormal(unchecked((sbyte)0x80)));
            Assert.True(NumberBaseHelper<sbyte>.IsNormal(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void IsPositiveInfinityTest()
        {
            Assert.False(NumberBaseHelper<sbyte>.IsPositiveInfinity((sbyte)0x00));
            Assert.False(NumberBaseHelper<sbyte>.IsPositiveInfinity((sbyte)0x01));
            Assert.False(NumberBaseHelper<sbyte>.IsPositiveInfinity((sbyte)0x7F));
            Assert.False(NumberBaseHelper<sbyte>.IsPositiveInfinity(unchecked((sbyte)0x80)));
            Assert.False(NumberBaseHelper<sbyte>.IsPositiveInfinity(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void IsSubnormalTest()
        {
            Assert.False(NumberBaseHelper<sbyte>.IsSubnormal((sbyte)0x00));
            Assert.False(NumberBaseHelper<sbyte>.IsSubnormal((sbyte)0x01));
            Assert.False(NumberBaseHelper<sbyte>.IsSubnormal((sbyte)0x7F));
            Assert.False(NumberBaseHelper<sbyte>.IsSubnormal(unchecked((sbyte)0x80)));
            Assert.False(NumberBaseHelper<sbyte>.IsSubnormal(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void MaxMagnitudeTest()
        {
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.MaxMagnitude((sbyte)0x00, (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.MaxMagnitude((sbyte)0x01, (sbyte)1));
            Assert.Equal((sbyte)0x7F, NumberBaseHelper<sbyte>.MaxMagnitude((sbyte)0x7F, (sbyte)1));
            Assert.Equal(unchecked((sbyte)0x80), NumberBaseHelper<sbyte>.MaxMagnitude(unchecked((sbyte)0x80), (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.MaxMagnitude(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void MaxMagnitudeNumberTest()
        {
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.MaxMagnitudeNumber((sbyte)0x00, (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.MaxMagnitudeNumber((sbyte)0x01, (sbyte)1));
            Assert.Equal((sbyte)0x7F, NumberBaseHelper<sbyte>.MaxMagnitudeNumber((sbyte)0x7F, (sbyte)1));
            Assert.Equal(unchecked((sbyte)0x80), NumberBaseHelper<sbyte>.MaxMagnitudeNumber(unchecked((sbyte)0x80), (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.MaxMagnitudeNumber(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void MinMagnitudeTest()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.MinMagnitude((sbyte)0x00, (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.MinMagnitude((sbyte)0x01, (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.MinMagnitude((sbyte)0x7F, (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.MinMagnitude(unchecked((sbyte)0x80), (sbyte)1));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.MinMagnitude(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void MinMagnitudeNumberTest()
        {
            Assert.Equal((sbyte)0x00, NumberBaseHelper<sbyte>.MinMagnitudeNumber((sbyte)0x00, (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.MinMagnitudeNumber((sbyte)0x01, (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.MinMagnitudeNumber((sbyte)0x7F, (sbyte)1));
            Assert.Equal((sbyte)0x01, NumberBaseHelper<sbyte>.MinMagnitudeNumber(unchecked((sbyte)0x80), (sbyte)1));
            Assert.Equal(unchecked((sbyte)0xFF), NumberBaseHelper<sbyte>.MinMagnitudeNumber(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void TryCreateFromByteTest()
        {
            sbyte result;

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<byte>(0x00, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<byte>(0x01, out result));
            Assert.Equal((sbyte)0x01, result);

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<byte>(0x7F, out result));
            Assert.Equal((sbyte)0x7F, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<byte>(0x80, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<byte>(0xFF, out result));
            Assert.Equal((sbyte)0x00, result);
        }

        [Fact]
        public static void TryCreateFromCharTest()
        {
            sbyte result;

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<char>((char)0x0000, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<char>((char)0x0001, out result));
            Assert.Equal((sbyte)0x01, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<char>((char)0x7FFF, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<char>((char)0x8000, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<char>((char)0xFFFF, out result));
            Assert.Equal((sbyte)0x00, result);
        }

        [Fact]
        public static void TryCreateFromInt16Test()
        {
            sbyte result;

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<short>(0x0000, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<short>(0x0001, out result));
            Assert.Equal((sbyte)0x01, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<short>(0x7FFF, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<short>(unchecked((short)0x8000), out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<short>(unchecked((short)0xFFFF), out result));
            Assert.Equal(unchecked((sbyte)0xFF), result);
        }

        [Fact]
        public static void TryCreateFromInt32Test()
        {
            sbyte result;

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<int>(0x00000000, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<int>(0x00000001, out result));
            Assert.Equal((sbyte)0x01, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<int>(0x7FFFFFFF, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<int>(unchecked((int)0x80000000), out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<int>(unchecked((int)0xFFFFFFFF), out result));
            Assert.Equal(unchecked((sbyte)0xFF), result);
        }

        [Fact]
        public static void TryCreateFromInt64Test()
        {
            sbyte result;

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<long>(0x0000000000000000, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<long>(0x0000000000000001, out result));
            Assert.Equal((sbyte)0x01, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<long>(0x7FFFFFFFFFFFFFFF, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<long>(unchecked((long)0x8000000000000000), out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<long>(unchecked((long)0xFFFFFFFFFFFFFFFF), out result));
            Assert.Equal(unchecked((sbyte)0xFF), result);
        }

        [Fact]
        public static void TryCreateFromIntPtrTest()
        {
            sbyte result;

            if (Environment.Is64BitProcess)
            {
                Assert.True(NumberBaseHelper<sbyte>.TryCreate<nint>(unchecked((nint)0x0000000000000000), out result));
                Assert.Equal((sbyte)0x00, result);

                Assert.True(NumberBaseHelper<sbyte>.TryCreate<nint>(unchecked((nint)0x0000000000000001), out result));
                Assert.Equal((sbyte)0x01, result);

                Assert.False(NumberBaseHelper<sbyte>.TryCreate<nint>(unchecked((nint)0x7FFFFFFFFFFFFFFF), out result));
                Assert.Equal((sbyte)0x00, result);

                Assert.False(NumberBaseHelper<sbyte>.TryCreate<nint>(unchecked((nint)0x8000000000000000), out result));
                Assert.Equal((sbyte)0x00, result);

                Assert.True(NumberBaseHelper<sbyte>.TryCreate<nint>(unchecked((nint)0xFFFFFFFFFFFFFFFF), out result));
                Assert.Equal(unchecked((sbyte)0xFF), result);
            }
            else
            {
                Assert.True(NumberBaseHelper<sbyte>.TryCreate<nint>((nint)0x00000000, out result));
                Assert.Equal((sbyte)0x00, result);

                Assert.True(NumberBaseHelper<sbyte>.TryCreate<nint>((nint)0x00000001, out result));
                Assert.Equal((sbyte)0x01, result);

                Assert.False(NumberBaseHelper<sbyte>.TryCreate<nint>((nint)0x7FFFFFFF, out result));
                Assert.Equal((sbyte)0x00, result);

                Assert.False(NumberBaseHelper<sbyte>.TryCreate<nint>(unchecked((nint)0x80000000), out result));
                Assert.Equal((sbyte)0x00, result);

                Assert.True(NumberBaseHelper<sbyte>.TryCreate<nint>(unchecked((nint)0xFFFFFFFF), out result));
                Assert.Equal(unchecked((sbyte)0xFF), result);
            }
        }

        [Fact]
        public static void TryCreateFromSByteTest()
        {
            sbyte result;

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<sbyte>(0x00, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<sbyte>(0x01, out result));
            Assert.Equal((sbyte)0x01, result);

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<sbyte>(0x7F, out result));
            Assert.Equal((sbyte)0x7F, result);

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<sbyte>(unchecked((sbyte)0x80), out result));
            Assert.Equal(unchecked((sbyte)0x80), result);

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<sbyte>(unchecked((sbyte)0xFF), out result));
            Assert.Equal(unchecked((sbyte)0xFF), result);
        }

        [Fact]
        public static void TryCreateFromUInt16Test()
        {
            sbyte result;

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<ushort>(0x0000, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<ushort>(0x0001, out result));
            Assert.Equal((sbyte)0x01, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<ushort>(0x7FFF, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<ushort>(0x8000, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<ushort>(0xFFFF, out result));
            Assert.Equal((sbyte)0x00, result);
        }

        [Fact]
        public static void TryCreateFromUInt32Test()
        {
            sbyte result;

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<uint>(0x00000000, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<uint>(0x00000001, out result));
            Assert.Equal((sbyte)0x01, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<uint>(0x7FFFFFFF, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<uint>(0x80000000, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<uint>(0xFFFFFFFF, out result));
            Assert.Equal((sbyte)0x00, result);
        }

        [Fact]
        public static void TryCreateFromUInt64Test()
        {
            sbyte result;

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<ulong>(0x0000000000000000, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.True(NumberBaseHelper<sbyte>.TryCreate<ulong>(0x0000000000000001, out result));
            Assert.Equal((sbyte)0x01, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<ulong>(0x7FFFFFFFFFFFFFFF, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<ulong>(0x8000000000000000, out result));
            Assert.Equal((sbyte)0x00, result);

            Assert.False(NumberBaseHelper<sbyte>.TryCreate<ulong>(0xFFFFFFFFFFFFFFFF, out result));
            Assert.Equal((sbyte)0x00, result);
        }

        [Fact]
        public static void TryCreateFromUIntPtrTest()
        {
            sbyte result;

            if (Environment.Is64BitProcess)
            {
                Assert.True(NumberBaseHelper<sbyte>.TryCreate<nuint>(unchecked((nuint)0x0000000000000000), out result));
                Assert.Equal((sbyte)0x00, result);

                Assert.True(NumberBaseHelper<sbyte>.TryCreate<nuint>(unchecked((nuint)0x0000000000000001), out result));
                Assert.Equal((sbyte)0x01, result);

                Assert.False(NumberBaseHelper<sbyte>.TryCreate<nuint>(unchecked((nuint)0x7FFFFFFFFFFFFFFF), out result));
                Assert.Equal((sbyte)0x00, result);

                Assert.False(NumberBaseHelper<sbyte>.TryCreate<nuint>(unchecked((nuint)0x8000000000000000), out result));
                Assert.Equal((sbyte)0x00, result);

                Assert.False(NumberBaseHelper<sbyte>.TryCreate<nuint>(unchecked((nuint)0xFFFFFFFFFFFFFFFF), out result));
                Assert.Equal((sbyte)0x00, result);
            }
            else
            {
                Assert.True(NumberBaseHelper<sbyte>.TryCreate<nuint>((nuint)0x00000000, out result));
                Assert.Equal((sbyte)0x00, result);

                Assert.True(NumberBaseHelper<sbyte>.TryCreate<nuint>((nuint)0x00000001, out result));
                Assert.Equal((sbyte)0x01, result);

                Assert.False(NumberBaseHelper<sbyte>.TryCreate<nuint>((nuint)0x7FFFFFFF, out result));
                Assert.Equal((sbyte)0x00, result);

                Assert.False(NumberBaseHelper<sbyte>.TryCreate<nuint>(unchecked((nuint)0x80000000), out result));
                Assert.Equal((sbyte)0x00, result);

                Assert.False(NumberBaseHelper<sbyte>.TryCreate<nuint>(unchecked((nuint)0xFFFFFFFF), out result));
                Assert.Equal((sbyte)0x00, result);
            }
        }

        //
        // IShiftOperators
        //

        [Fact]
        public static void op_LeftShiftTest()
        {
            Assert.Equal((sbyte)0x00, ShiftOperatorsHelper<sbyte, sbyte>.op_LeftShift((sbyte)0x00, 1));
            Assert.Equal((sbyte)0x02, ShiftOperatorsHelper<sbyte, sbyte>.op_LeftShift((sbyte)0x01, 1));
            Assert.Equal(unchecked((sbyte)0xFE), ShiftOperatorsHelper<sbyte, sbyte>.op_LeftShift((sbyte)0x7F, 1));
            Assert.Equal((sbyte)0x00, ShiftOperatorsHelper<sbyte, sbyte>.op_LeftShift(unchecked((sbyte)0x80), 1));
            Assert.Equal(unchecked((sbyte)0xFE), ShiftOperatorsHelper<sbyte, sbyte>.op_LeftShift(unchecked((sbyte)0xFF), 1));
        }

        [Fact]
        public static void op_RightShiftTest()
        {
            Assert.Equal((sbyte)0x00, ShiftOperatorsHelper<sbyte, sbyte>.op_RightShift((sbyte)0x00, 1));
            Assert.Equal((sbyte)0x00, ShiftOperatorsHelper<sbyte, sbyte>.op_RightShift((sbyte)0x01, 1));
            Assert.Equal((sbyte)0x3F, ShiftOperatorsHelper<sbyte, sbyte>.op_RightShift((sbyte)0x7F, 1));
            Assert.Equal(unchecked((sbyte)0xC0), ShiftOperatorsHelper<sbyte, sbyte>.op_RightShift(unchecked((sbyte)0x80), 1));
            Assert.Equal(unchecked((sbyte)0xFF), ShiftOperatorsHelper<sbyte, sbyte>.op_RightShift(unchecked((sbyte)0xFF), 1));
        }

        [Fact]
        public static void op_UnsignedRightShiftTest()
        {
            Assert.Equal((sbyte)0x00, ShiftOperatorsHelper<sbyte, sbyte>.op_UnsignedRightShift((sbyte)0x00, 1));
            Assert.Equal((sbyte)0x00, ShiftOperatorsHelper<sbyte, sbyte>.op_UnsignedRightShift((sbyte)0x01, 1));
            Assert.Equal((sbyte)0x3F, ShiftOperatorsHelper<sbyte, sbyte>.op_UnsignedRightShift((sbyte)0x7F, 1));
            Assert.Equal((sbyte)0x40, ShiftOperatorsHelper<sbyte, sbyte>.op_UnsignedRightShift(unchecked((sbyte)0x80), 1));
            Assert.Equal((sbyte)0x7F, ShiftOperatorsHelper<sbyte, sbyte>.op_UnsignedRightShift(unchecked((sbyte)0xFF), 1));
        }

        //
        // ISignedNumber
        //

        [Fact]
        public static void NegativeOneTest()
        {
            Assert.Equal(unchecked((sbyte)0xFF), SignedNumberHelper<sbyte>.NegativeOne);
        }

        //
        // ISubtractionOperators
        //

        [Fact]
        public static void op_SubtractionTest()
        {
            Assert.Equal(unchecked((sbyte)0xFF), SubtractionOperatorsHelper<sbyte, sbyte, sbyte>.op_Subtraction((sbyte)0x00, (sbyte)1));
            Assert.Equal((sbyte)0x00, SubtractionOperatorsHelper<sbyte, sbyte, sbyte>.op_Subtraction((sbyte)0x01, (sbyte)1));
            Assert.Equal((sbyte)0x7E, SubtractionOperatorsHelper<sbyte, sbyte, sbyte>.op_Subtraction((sbyte)0x7F, (sbyte)1));
            Assert.Equal((sbyte)0x7F, SubtractionOperatorsHelper<sbyte, sbyte, sbyte>.op_Subtraction(unchecked((sbyte)0x80), (sbyte)1));
            Assert.Equal(unchecked((sbyte)0xFE), SubtractionOperatorsHelper<sbyte, sbyte, sbyte>.op_Subtraction(unchecked((sbyte)0xFF), (sbyte)1));
        }

        [Fact]
        public static void op_CheckedSubtractionTest()
        {
            Assert.Equal(unchecked((sbyte)0xFF), SubtractionOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedSubtraction((sbyte)0x00, (sbyte)1));
            Assert.Equal((sbyte)0x00, SubtractionOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedSubtraction((sbyte)0x01, (sbyte)1));
            Assert.Equal((sbyte)0x7E, SubtractionOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedSubtraction((sbyte)0x7F, (sbyte)1));
            Assert.Equal(unchecked((sbyte)0xFE), SubtractionOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedSubtraction(unchecked((sbyte)0xFF), (sbyte)1));

            Assert.Throws<OverflowException>(() => SubtractionOperatorsHelper<sbyte, sbyte, sbyte>.op_CheckedSubtraction(unchecked((sbyte)0x80), (sbyte)1));
        }

        //
        // IUnaryNegationOperators
        //

        [Fact]
        public static void op_UnaryNegationTest()
        {
            Assert.Equal((sbyte)0x00, UnaryNegationOperatorsHelper<sbyte, sbyte>.op_UnaryNegation((sbyte)0x00));
            Assert.Equal(unchecked((sbyte)0xFF), UnaryNegationOperatorsHelper<sbyte, sbyte>.op_UnaryNegation((sbyte)0x01));
            Assert.Equal(unchecked((sbyte)0x81), UnaryNegationOperatorsHelper<sbyte, sbyte>.op_UnaryNegation((sbyte)0x7F));
            Assert.Equal(unchecked((sbyte)0x80), UnaryNegationOperatorsHelper<sbyte, sbyte>.op_UnaryNegation(unchecked((sbyte)0x80)));
            Assert.Equal((sbyte)0x01, UnaryNegationOperatorsHelper<sbyte, sbyte>.op_UnaryNegation(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void op_CheckedUnaryNegationTest()
        {
            Assert.Equal((sbyte)0x00, UnaryNegationOperatorsHelper<sbyte, sbyte>.op_CheckedUnaryNegation((sbyte)0x00));
            Assert.Equal(unchecked((sbyte)0xFF), UnaryNegationOperatorsHelper<sbyte, sbyte>.op_CheckedUnaryNegation((sbyte)0x01));
            Assert.Equal(unchecked((sbyte)0x81), UnaryNegationOperatorsHelper<sbyte, sbyte>.op_CheckedUnaryNegation((sbyte)0x7F));
            Assert.Equal((sbyte)0x01, UnaryNegationOperatorsHelper<sbyte, sbyte>.op_CheckedUnaryNegation(unchecked((sbyte)0xFF)));

            Assert.Throws<OverflowException>(() => UnaryNegationOperatorsHelper<sbyte, sbyte>.op_CheckedUnaryNegation(unchecked((sbyte)0x80)));
        }

        //
        // IUnaryPlusOperators
        //

        [Fact]
        public static void op_UnaryPlusTest()
        {
            Assert.Equal((sbyte)0x00, UnaryPlusOperatorsHelper<sbyte, sbyte>.op_UnaryPlus((sbyte)0x00));
            Assert.Equal((sbyte)0x01, UnaryPlusOperatorsHelper<sbyte, sbyte>.op_UnaryPlus((sbyte)0x01));
            Assert.Equal((sbyte)0x7F, UnaryPlusOperatorsHelper<sbyte, sbyte>.op_UnaryPlus((sbyte)0x7F));
            Assert.Equal(unchecked((sbyte)0x80), UnaryPlusOperatorsHelper<sbyte, sbyte>.op_UnaryPlus(unchecked((sbyte)0x80)));
            Assert.Equal(unchecked((sbyte)0xFF), UnaryPlusOperatorsHelper<sbyte, sbyte>.op_UnaryPlus(unchecked((sbyte)0xFF)));
        }

        //
        // IParsable and ISpanParsable
        //

        [Theory]
        [MemberData(nameof(SByteTests.Parse_Valid_TestData), MemberType = typeof(SByteTests))]
        public static void ParseValidStringTest(string value, NumberStyles style, IFormatProvider provider, sbyte expected)
        {
            sbyte result;

            // Default style and provider
            if ((style == NumberStyles.Integer) && (provider is null))
            {
                Assert.True(ParsableHelper<sbyte>.TryParse(value, provider, out result));
                Assert.Equal(expected, result);
                Assert.Equal(expected, ParsableHelper<sbyte>.Parse(value, provider));
            }

            // Default provider
            if (provider is null)
            {
                Assert.Equal(expected, NumberBaseHelper<sbyte>.Parse(value, style, provider));

                // Substitute default NumberFormatInfo
                Assert.True(NumberBaseHelper<sbyte>.TryParse(value, style, new NumberFormatInfo(), out result));
                Assert.Equal(expected, result);
                Assert.Equal(expected, NumberBaseHelper<sbyte>.Parse(value, style, new NumberFormatInfo()));
            }

            // Default style
            if (style == NumberStyles.Integer)
            {
                Assert.Equal(expected, ParsableHelper<sbyte>.Parse(value, provider));
            }

            // Full overloads
            Assert.True(NumberBaseHelper<sbyte>.TryParse(value, style, provider, out result));
            Assert.Equal(expected, result);
            Assert.Equal(expected, NumberBaseHelper<sbyte>.Parse(value, style, provider));
        }

        [Theory]
        [MemberData(nameof(SByteTests.Parse_Invalid_TestData), MemberType = typeof(SByteTests))]
        public static void ParseInvalidStringTest(string value, NumberStyles style, IFormatProvider provider, Type exceptionType)
        {
            sbyte result;

            // Default style and provider
            if ((style == NumberStyles.Integer) && (provider is null))
            {
                Assert.False(ParsableHelper<sbyte>.TryParse(value, provider, out result));
                Assert.Equal(default(sbyte), result);
                Assert.Throws(exceptionType, () => ParsableHelper<sbyte>.Parse(value, provider));
            }

            // Default provider
            if (provider is null)
            {
                Assert.Throws(exceptionType, () => NumberBaseHelper<sbyte>.Parse(value, style, provider));

                // Substitute default NumberFormatInfo
                Assert.False(NumberBaseHelper<sbyte>.TryParse(value, style, new NumberFormatInfo(), out result));
                Assert.Equal(default(sbyte), result);
                Assert.Throws(exceptionType, () => NumberBaseHelper<sbyte>.Parse(value, style, new NumberFormatInfo()));
            }

            // Default style
            if (style == NumberStyles.Integer)
            {
                Assert.Throws(exceptionType, () => ParsableHelper<sbyte>.Parse(value, provider));
            }

            // Full overloads
            Assert.False(NumberBaseHelper<sbyte>.TryParse(value, style, provider, out result));
            Assert.Equal(default(sbyte), result);
            Assert.Throws(exceptionType, () => NumberBaseHelper<sbyte>.Parse(value, style, provider));
        }

        [Theory]
        [MemberData(nameof(SByteTests.Parse_ValidWithOffsetCount_TestData), MemberType = typeof(SByteTests))]
        public static void ParseValidSpanTest(string value, int offset, int count, NumberStyles style, IFormatProvider provider, sbyte expected)
        {
            sbyte result;

            // Default style and provider
            if ((style == NumberStyles.Integer) && (provider is null))
            {
                Assert.True(SpanParsableHelper<sbyte>.TryParse(value.AsSpan(offset, count), provider, out result));
                Assert.Equal(expected, result);
            }

            Assert.Equal(expected, NumberBaseHelper<sbyte>.Parse(value.AsSpan(offset, count), style, provider));

            Assert.True(NumberBaseHelper<sbyte>.TryParse(value.AsSpan(offset, count), style, provider, out result));
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(SByteTests.Parse_Invalid_TestData), MemberType = typeof(SByteTests))]
        public static void ParseInvalidSpanTest(string value, NumberStyles style, IFormatProvider provider, Type exceptionType)
        {
            if (value is null)
            {
                return;
            }

            sbyte result;

            // Default style and provider
            if ((style == NumberStyles.Integer) && (provider is null))
            {
                Assert.False(SpanParsableHelper<sbyte>.TryParse(value.AsSpan(), provider, out result));
                Assert.Equal(default(sbyte), result);
            }

            Assert.Throws(exceptionType, () => NumberBaseHelper<sbyte>.Parse(value.AsSpan(), style, provider));

            Assert.False(NumberBaseHelper<sbyte>.TryParse(value.AsSpan(), style, provider, out result));
            Assert.Equal(default(sbyte), result);
        }
    }
}
