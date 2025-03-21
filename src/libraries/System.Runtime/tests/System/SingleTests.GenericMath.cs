// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Xunit;

namespace System.Tests
{
    public class SingleTests_GenericMath
    {
        private const float MinNormal = 1.17549435E-38f;

        private const float MaxSubnormal = 1.17549421E-38f;

        private static void AssertBitwiseEqual(float expected, float actual)
        {
            uint expectedBits = BitConverter.SingleToUInt32Bits(expected);
            uint actualBits = BitConverter.SingleToUInt32Bits(actual);

            if (expectedBits == actualBits)
            {
                return;
            }

            if (float.IsNaN(expected) && float.IsNaN(actual))
            {
                return;
            }

            throw new Xunit.Sdk.EqualException(expected, actual);
        }

        //
        // IAdditionOperators
        //

        [Fact]
        public static void op_AdditionTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, AdditionOperatorsHelper<float, float, float>.op_Addition(float.NegativeInfinity, 1.0f));
            AssertBitwiseEqual(float.MinValue, AdditionOperatorsHelper<float, float, float>.op_Addition(float.MinValue, 1.0f));
            AssertBitwiseEqual(0.0f, AdditionOperatorsHelper<float, float, float>.op_Addition(-1.0f, 1.0f));
            AssertBitwiseEqual(1.0f, AdditionOperatorsHelper<float, float, float>.op_Addition(-MinNormal, 1.0f));
            AssertBitwiseEqual(1.0f, AdditionOperatorsHelper<float, float, float>.op_Addition(-MaxSubnormal, 1.0f));
            AssertBitwiseEqual(1.0f, AdditionOperatorsHelper<float, float, float>.op_Addition(-float.Epsilon, 1.0f));
            AssertBitwiseEqual(1.0f, AdditionOperatorsHelper<float, float, float>.op_Addition(-0.0f, 1.0f));
            AssertBitwiseEqual(float.NaN, AdditionOperatorsHelper<float, float, float>.op_Addition(float.NaN, 1.0f));
            AssertBitwiseEqual(1.0f, AdditionOperatorsHelper<float, float, float>.op_Addition(0.0f, 1.0f));
            AssertBitwiseEqual(1.0f, AdditionOperatorsHelper<float, float, float>.op_Addition(float.Epsilon, 1.0f));
            AssertBitwiseEqual(1.0f, AdditionOperatorsHelper<float, float, float>.op_Addition(MaxSubnormal, 1.0f));
            AssertBitwiseEqual(1.0f, AdditionOperatorsHelper<float, float, float>.op_Addition(MinNormal, 1.0f));
            AssertBitwiseEqual(2.0f, AdditionOperatorsHelper<float, float, float>.op_Addition(1.0f, 1.0f));
            AssertBitwiseEqual(float.MaxValue, AdditionOperatorsHelper<float, float, float>.op_Addition(float.MaxValue, 1.0f));
            AssertBitwiseEqual(float.PositiveInfinity, AdditionOperatorsHelper<float, float, float>.op_Addition(float.PositiveInfinity, 1.0f));
        }

        [Fact]
        public static void op_CheckedAdditionTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, AdditionOperatorsHelper<float, float, float>.op_CheckedAddition(float.NegativeInfinity, 1.0f));
            AssertBitwiseEqual(float.MinValue, AdditionOperatorsHelper<float, float, float>.op_CheckedAddition(float.MinValue, 1.0f));
            AssertBitwiseEqual(0.0f, AdditionOperatorsHelper<float, float, float>.op_CheckedAddition(-1.0f, 1.0f));
            AssertBitwiseEqual(1.0f, AdditionOperatorsHelper<float, float, float>.op_CheckedAddition(-MinNormal, 1.0f));
            AssertBitwiseEqual(1.0f, AdditionOperatorsHelper<float, float, float>.op_CheckedAddition(-MaxSubnormal, 1.0f));
            AssertBitwiseEqual(1.0f, AdditionOperatorsHelper<float, float, float>.op_CheckedAddition(-float.Epsilon, 1.0f));
            AssertBitwiseEqual(1.0f, AdditionOperatorsHelper<float, float, float>.op_CheckedAddition(-0.0f, 1.0f));
            AssertBitwiseEqual(float.NaN, AdditionOperatorsHelper<float, float, float>.op_CheckedAddition(float.NaN, 1.0f));
            AssertBitwiseEqual(1.0f, AdditionOperatorsHelper<float, float, float>.op_CheckedAddition(0.0f, 1.0f));
            AssertBitwiseEqual(1.0f, AdditionOperatorsHelper<float, float, float>.op_CheckedAddition(float.Epsilon, 1.0f));
            AssertBitwiseEqual(1.0f, AdditionOperatorsHelper<float, float, float>.op_CheckedAddition(MaxSubnormal, 1.0f));
            AssertBitwiseEqual(1.0f, AdditionOperatorsHelper<float, float, float>.op_CheckedAddition(MinNormal, 1.0f));
            AssertBitwiseEqual(2.0f, AdditionOperatorsHelper<float, float, float>.op_CheckedAddition(1.0f, 1.0f));
            AssertBitwiseEqual(float.MaxValue, AdditionOperatorsHelper<float, float, float>.op_CheckedAddition(float.MaxValue, 1.0f));
            AssertBitwiseEqual(float.PositiveInfinity, AdditionOperatorsHelper<float, float, float>.op_CheckedAddition(float.PositiveInfinity, 1.0f));
        }

        //
        // IAdditiveIdentity
        //

        [Fact]
        public static void AdditiveIdentityTest()
        {
            AssertBitwiseEqual(0.0f, AdditiveIdentityHelper<float, float>.AdditiveIdentity);
        }

        //
        // IBinaryNumber
        //

        [Fact]
        public static void IsPow2Test()
        {
            Assert.False(BinaryNumberHelper<float>.IsPow2(float.NegativeInfinity));
            Assert.False(BinaryNumberHelper<float>.IsPow2(float.MinValue));
            Assert.False(BinaryNumberHelper<float>.IsPow2(-1.0f));
            Assert.False(BinaryNumberHelper<float>.IsPow2(-MinNormal));
            Assert.False(BinaryNumberHelper<float>.IsPow2(-MaxSubnormal));
            Assert.False(BinaryNumberHelper<float>.IsPow2(-float.Epsilon));
            Assert.False(BinaryNumberHelper<float>.IsPow2(-0.0f));
            Assert.False(BinaryNumberHelper<float>.IsPow2(float.NaN));
            Assert.False(BinaryNumberHelper<float>.IsPow2(0.0f));
            Assert.False(BinaryNumberHelper<float>.IsPow2(float.Epsilon));
            Assert.False(BinaryNumberHelper<float>.IsPow2(MaxSubnormal));
            Assert.True(BinaryNumberHelper<float>.IsPow2(MinNormal));
            Assert.True(BinaryNumberHelper<float>.IsPow2(1.0f));
            Assert.False(BinaryNumberHelper<float>.IsPow2(float.MaxValue));
            Assert.False(BinaryNumberHelper<float>.IsPow2(float.PositiveInfinity));
        }

        [Fact]
        public static void Log2Test()
        {
            AssertBitwiseEqual(float.NaN, BinaryNumberHelper<float>.Log2(float.NegativeInfinity));
            AssertBitwiseEqual(float.NaN, BinaryNumberHelper<float>.Log2(float.MinValue));
            AssertBitwiseEqual(float.NaN, BinaryNumberHelper<float>.Log2(-1.0f));
            AssertBitwiseEqual(float.NaN, BinaryNumberHelper<float>.Log2(-MinNormal));
            AssertBitwiseEqual(float.NaN, BinaryNumberHelper<float>.Log2(-MaxSubnormal));
            AssertBitwiseEqual(float.NaN, BinaryNumberHelper<float>.Log2(-float.Epsilon));
            AssertBitwiseEqual(float.NegativeInfinity, BinaryNumberHelper<float>.Log2(-0.0f));
            AssertBitwiseEqual(float.NaN, BinaryNumberHelper<float>.Log2(float.NaN));
            AssertBitwiseEqual(float.NegativeInfinity, BinaryNumberHelper<float>.Log2(0.0f));
            AssertBitwiseEqual(-149.0f, BinaryNumberHelper<float>.Log2(float.Epsilon));
            AssertBitwiseEqual(-126.0f, BinaryNumberHelper<float>.Log2(MaxSubnormal));
            AssertBitwiseEqual(-126.0f, BinaryNumberHelper<float>.Log2(MinNormal));
            AssertBitwiseEqual(0.0f, BinaryNumberHelper<float>.Log2(1.0f));
            AssertBitwiseEqual(128.0f, BinaryNumberHelper<float>.Log2(float.MaxValue));
            AssertBitwiseEqual(float.PositiveInfinity, BinaryNumberHelper<float>.Log2(float.PositiveInfinity));
        }

        //
        // IComparisonOperators
        //

        [Fact]
        public static void op_GreaterThanTest()
        {
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThan(float.NegativeInfinity, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThan(float.MinValue, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThan(-1.0f, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThan(-MinNormal, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThan(-MaxSubnormal, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThan(-float.Epsilon, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThan(-0.0f, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThan(float.NaN, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThan(0.0f, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThan(float.Epsilon, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThan(MaxSubnormal, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThan(MinNormal, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThan(1.0f, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_GreaterThan(float.MaxValue, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_GreaterThan(float.PositiveInfinity, 1.0f));
        }

        [Fact]
        public static void op_GreaterThanOrEqualTest()
        {
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThanOrEqual(float.NegativeInfinity, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThanOrEqual(float.MinValue, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThanOrEqual(-1.0f, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThanOrEqual(-MinNormal, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThanOrEqual(-MaxSubnormal, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThanOrEqual(-float.Epsilon, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThanOrEqual(-0.0f, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThanOrEqual(float.NaN, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThanOrEqual(0.0f, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThanOrEqual(float.Epsilon, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThanOrEqual(MaxSubnormal, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_GreaterThanOrEqual(MinNormal, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_GreaterThanOrEqual(1.0f, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_GreaterThanOrEqual(float.MaxValue, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_GreaterThanOrEqual(float.PositiveInfinity, 1.0f));
        }

        [Fact]
        public static void op_LessThanTest()
        {
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThan(float.NegativeInfinity, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThan(float.MinValue, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThan(-1.0f, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThan(-MinNormal, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThan(-MaxSubnormal, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThan(-float.Epsilon, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThan(-0.0f, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_LessThan(float.NaN, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThan(0.0f, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThan(float.Epsilon, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThan(MaxSubnormal, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThan(MinNormal, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_LessThan(1.0f, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_LessThan(float.MaxValue, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_LessThan(float.PositiveInfinity, 1.0f));
        }

        [Fact]
        public static void op_LessThanOrEqualTest()
        {
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThanOrEqual(float.NegativeInfinity, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThanOrEqual(float.MinValue, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThanOrEqual(-1.0f, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThanOrEqual(-MinNormal, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThanOrEqual(-MaxSubnormal, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThanOrEqual(-float.Epsilon, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThanOrEqual(-0.0f, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_LessThanOrEqual(float.NaN, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThanOrEqual(0.0f, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThanOrEqual(float.Epsilon, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThanOrEqual(MaxSubnormal, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThanOrEqual(MinNormal, 1.0f));
            Assert.True(ComparisonOperatorsHelper<float, float>.op_LessThanOrEqual(1.0f, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_LessThanOrEqual(float.MaxValue, 1.0f));
            Assert.False(ComparisonOperatorsHelper<float, float>.op_LessThanOrEqual(float.PositiveInfinity, 1.0f));
        }

        //
        // IDecrementOperators
        //

        [Fact]
        public static void op_DecrementTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, DecrementOperatorsHelper<float>.op_Decrement(float.NegativeInfinity));
            AssertBitwiseEqual(float.MinValue, DecrementOperatorsHelper<float>.op_Decrement(float.MinValue));
            AssertBitwiseEqual(-2.0f, DecrementOperatorsHelper<float>.op_Decrement(-1.0f));
            AssertBitwiseEqual(-1.0f, DecrementOperatorsHelper<float>.op_Decrement(-MinNormal));
            AssertBitwiseEqual(-1.0f, DecrementOperatorsHelper<float>.op_Decrement(-MaxSubnormal));
            AssertBitwiseEqual(-1.0f, DecrementOperatorsHelper<float>.op_Decrement(-float.Epsilon));
            AssertBitwiseEqual(-1.0f, DecrementOperatorsHelper<float>.op_Decrement(-0.0f));
            AssertBitwiseEqual(float.NaN, DecrementOperatorsHelper<float>.op_Decrement(float.NaN));
            AssertBitwiseEqual(-1.0f, DecrementOperatorsHelper<float>.op_Decrement(0.0f));
            AssertBitwiseEqual(-1.0f, DecrementOperatorsHelper<float>.op_Decrement(float.Epsilon));
            AssertBitwiseEqual(-1.0f, DecrementOperatorsHelper<float>.op_Decrement(MaxSubnormal));
            AssertBitwiseEqual(-1.0f, DecrementOperatorsHelper<float>.op_Decrement(MinNormal));
            AssertBitwiseEqual(0.0f, DecrementOperatorsHelper<float>.op_Decrement(1.0f));
            AssertBitwiseEqual(float.MaxValue, DecrementOperatorsHelper<float>.op_Decrement(float.MaxValue));
            AssertBitwiseEqual(float.PositiveInfinity, DecrementOperatorsHelper<float>.op_Decrement(float.PositiveInfinity));
        }

        [Fact]
        public static void op_CheckedDecrementTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, DecrementOperatorsHelper<float>.op_CheckedDecrement(float.NegativeInfinity));
            AssertBitwiseEqual(float.MinValue, DecrementOperatorsHelper<float>.op_CheckedDecrement(float.MinValue));
            AssertBitwiseEqual(-2.0f, DecrementOperatorsHelper<float>.op_CheckedDecrement(-1.0f));
            AssertBitwiseEqual(-1.0f, DecrementOperatorsHelper<float>.op_CheckedDecrement(-MinNormal));
            AssertBitwiseEqual(-1.0f, DecrementOperatorsHelper<float>.op_CheckedDecrement(-MaxSubnormal));
            AssertBitwiseEqual(-1.0f, DecrementOperatorsHelper<float>.op_CheckedDecrement(-float.Epsilon));
            AssertBitwiseEqual(-1.0f, DecrementOperatorsHelper<float>.op_CheckedDecrement(-0.0f));
            AssertBitwiseEqual(float.NaN, DecrementOperatorsHelper<float>.op_CheckedDecrement(float.NaN));
            AssertBitwiseEqual(-1.0f, DecrementOperatorsHelper<float>.op_CheckedDecrement(0.0f));
            AssertBitwiseEqual(-1.0f, DecrementOperatorsHelper<float>.op_CheckedDecrement(float.Epsilon));
            AssertBitwiseEqual(-1.0f, DecrementOperatorsHelper<float>.op_CheckedDecrement(MaxSubnormal));
            AssertBitwiseEqual(-1.0f, DecrementOperatorsHelper<float>.op_CheckedDecrement(MinNormal));
            AssertBitwiseEqual(0.0f, DecrementOperatorsHelper<float>.op_CheckedDecrement(1.0f));
            AssertBitwiseEqual(float.MaxValue, DecrementOperatorsHelper<float>.op_CheckedDecrement(float.MaxValue));
            AssertBitwiseEqual(float.PositiveInfinity, DecrementOperatorsHelper<float>.op_CheckedDecrement(float.PositiveInfinity));
        }

        //
        // IDivisionOperators
        //

        [Fact]
        public static void op_DivisionTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, DivisionOperatorsHelper<float, float, float>.op_Division(float.NegativeInfinity, 2.0f));
            AssertBitwiseEqual(-1.70141173E+38f, DivisionOperatorsHelper<float, float, float>.op_Division(float.MinValue, 2.0f));
            AssertBitwiseEqual(-0.5f, DivisionOperatorsHelper<float, float, float>.op_Division(-1.0f, 2.0f));
            AssertBitwiseEqual(-5.87747175E-39f, DivisionOperatorsHelper<float, float, float>.op_Division(-MinNormal, 2.0f));
            AssertBitwiseEqual(-5.87747175E-39f, DivisionOperatorsHelper<float, float, float>.op_Division(-MaxSubnormal, 2.0f));
            AssertBitwiseEqual(-0.0f, DivisionOperatorsHelper<float, float, float>.op_Division(-float.Epsilon, 2.0f));
            AssertBitwiseEqual(-0.0f, DivisionOperatorsHelper<float, float, float>.op_Division(-0.0f, 2.0f));
            AssertBitwiseEqual(float.NaN, DivisionOperatorsHelper<float, float, float>.op_Division(float.NaN, 2.0f));
            AssertBitwiseEqual(0.0f, DivisionOperatorsHelper<float, float, float>.op_Division(0.0f, 2.0f));
            AssertBitwiseEqual(0.0f, DivisionOperatorsHelper<float, float, float>.op_Division(float.Epsilon, 2.0f));
            AssertBitwiseEqual(5.87747175E-39f, DivisionOperatorsHelper<float, float, float>.op_Division(MaxSubnormal, 2.0f));
            AssertBitwiseEqual(5.87747175E-39f, DivisionOperatorsHelper<float, float, float>.op_Division(MinNormal, 2.0f));
            AssertBitwiseEqual(0.5f, DivisionOperatorsHelper<float, float, float>.op_Division(1.0f, 2.0f));
            AssertBitwiseEqual(1.70141173E+38f, DivisionOperatorsHelper<float, float, float>.op_Division(float.MaxValue, 2.0f));
            AssertBitwiseEqual(float.PositiveInfinity, DivisionOperatorsHelper<float, float, float>.op_Division(float.PositiveInfinity, 2.0f));
        }

        [Fact]
        public static void op_CheckedDivisionTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, DivisionOperatorsHelper<float, float, float>.op_CheckedDivision(float.NegativeInfinity, 2.0f));
            AssertBitwiseEqual(-1.70141173E+38f, DivisionOperatorsHelper<float, float, float>.op_CheckedDivision(float.MinValue, 2.0f));
            AssertBitwiseEqual(-0.5f, DivisionOperatorsHelper<float, float, float>.op_CheckedDivision(-1.0f, 2.0f));
            AssertBitwiseEqual(-5.87747175E-39f, DivisionOperatorsHelper<float, float, float>.op_CheckedDivision(-MinNormal, 2.0f));
            AssertBitwiseEqual(-5.87747175E-39f, DivisionOperatorsHelper<float, float, float>.op_CheckedDivision(-MaxSubnormal, 2.0f));
            AssertBitwiseEqual(-0.0f, DivisionOperatorsHelper<float, float, float>.op_CheckedDivision(-float.Epsilon, 2.0f));
            AssertBitwiseEqual(-0.0f, DivisionOperatorsHelper<float, float, float>.op_CheckedDivision(-0.0f, 2.0f));
            AssertBitwiseEqual(float.NaN, DivisionOperatorsHelper<float, float, float>.op_CheckedDivision(float.NaN, 2.0f));
            AssertBitwiseEqual(0.0f, DivisionOperatorsHelper<float, float, float>.op_CheckedDivision(0.0f, 2.0f));
            AssertBitwiseEqual(0.0f, DivisionOperatorsHelper<float, float, float>.op_CheckedDivision(float.Epsilon, 2.0f));
            AssertBitwiseEqual(5.87747175E-39f, DivisionOperatorsHelper<float, float, float>.op_CheckedDivision(MaxSubnormal, 2.0f));
            AssertBitwiseEqual(5.87747175E-39f, DivisionOperatorsHelper<float, float, float>.op_CheckedDivision(MinNormal, 2.0f));
            AssertBitwiseEqual(0.5f, DivisionOperatorsHelper<float, float, float>.op_CheckedDivision(1.0f, 2.0f));
            AssertBitwiseEqual(1.70141173E+38f, DivisionOperatorsHelper<float, float, float>.op_CheckedDivision(float.MaxValue, 2.0f));
            AssertBitwiseEqual(float.PositiveInfinity, DivisionOperatorsHelper<float, float, float>.op_CheckedDivision(float.PositiveInfinity, 2.0f));
        }

        //
        // IEqualityOperators
        //

        [Fact]
        public static void op_EqualityTest()
        {
            Assert.False(EqualityOperatorsHelper<float, float>.op_Equality(float.NegativeInfinity, 1.0f));
            Assert.False(EqualityOperatorsHelper<float, float>.op_Equality(float.MinValue, 1.0f));
            Assert.False(EqualityOperatorsHelper<float, float>.op_Equality(-1.0f, 1.0f));
            Assert.False(EqualityOperatorsHelper<float, float>.op_Equality(-MinNormal, 1.0f));
            Assert.False(EqualityOperatorsHelper<float, float>.op_Equality(-MaxSubnormal, 1.0f));
            Assert.False(EqualityOperatorsHelper<float, float>.op_Equality(-float.Epsilon, 1.0f));
            Assert.False(EqualityOperatorsHelper<float, float>.op_Equality(-0.0f, 1.0f));
            Assert.False(EqualityOperatorsHelper<float, float>.op_Equality(float.NaN, 1.0f));
            Assert.False(EqualityOperatorsHelper<float, float>.op_Equality(0.0f, 1.0f));
            Assert.False(EqualityOperatorsHelper<float, float>.op_Equality(float.Epsilon, 1.0f));
            Assert.False(EqualityOperatorsHelper<float, float>.op_Equality(MaxSubnormal, 1.0f));
            Assert.False(EqualityOperatorsHelper<float, float>.op_Equality(MinNormal, 1.0f));
            Assert.True(EqualityOperatorsHelper<float, float>.op_Equality(1.0f, 1.0f));
            Assert.False(EqualityOperatorsHelper<float, float>.op_Equality(float.MaxValue, 1.0f));
            Assert.False(EqualityOperatorsHelper<float, float>.op_Equality(float.PositiveInfinity, 1.0f));
        }

        [Fact]
        public static void op_InequalityTest()
        {
            Assert.True(EqualityOperatorsHelper<float, float>.op_Inequality(float.NegativeInfinity, 1.0f));
            Assert.True(EqualityOperatorsHelper<float, float>.op_Inequality(float.MinValue, 1.0f));
            Assert.True(EqualityOperatorsHelper<float, float>.op_Inequality(-1.0f, 1.0f));
            Assert.True(EqualityOperatorsHelper<float, float>.op_Inequality(-MinNormal, 1.0f));
            Assert.True(EqualityOperatorsHelper<float, float>.op_Inequality(-MaxSubnormal, 1.0f));
            Assert.True(EqualityOperatorsHelper<float, float>.op_Inequality(-float.Epsilon, 1.0f));
            Assert.True(EqualityOperatorsHelper<float, float>.op_Inequality(-0.0f, 1.0f));
            Assert.True(EqualityOperatorsHelper<float, float>.op_Inequality(float.NaN, 1.0f));
            Assert.True(EqualityOperatorsHelper<float, float>.op_Inequality(0.0f, 1.0f));
            Assert.True(EqualityOperatorsHelper<float, float>.op_Inequality(float.Epsilon, 1.0f));
            Assert.True(EqualityOperatorsHelper<float, float>.op_Inequality(MaxSubnormal, 1.0f));
            Assert.True(EqualityOperatorsHelper<float, float>.op_Inequality(MinNormal, 1.0f));
            Assert.False(EqualityOperatorsHelper<float, float>.op_Inequality(1.0f, 1.0f));
            Assert.True(EqualityOperatorsHelper<float, float>.op_Inequality(float.MaxValue, 1.0f));
            Assert.True(EqualityOperatorsHelper<float, float>.op_Inequality(float.PositiveInfinity, 1.0f));
        }

        //
        // IFloatingPoint
        //

        [Fact]
        public static void GetExponentByteCountTest()
        {
            Assert.Equal(1, FloatingPointHelper<float>.GetExponentByteCount(float.NegativeInfinity));
            Assert.Equal(1, FloatingPointHelper<float>.GetExponentByteCount(float.MinValue));
            Assert.Equal(1, FloatingPointHelper<float>.GetExponentByteCount(-1.0f));
            Assert.Equal(1, FloatingPointHelper<float>.GetExponentByteCount(-MinNormal));
            Assert.Equal(1, FloatingPointHelper<float>.GetExponentByteCount(-MaxSubnormal));
            Assert.Equal(1, FloatingPointHelper<float>.GetExponentByteCount(-float.Epsilon));
            Assert.Equal(1, FloatingPointHelper<float>.GetExponentByteCount(-0.0f));
            Assert.Equal(1, FloatingPointHelper<float>.GetExponentByteCount(float.NaN));
            Assert.Equal(1, FloatingPointHelper<float>.GetExponentByteCount(0.0f));
            Assert.Equal(1, FloatingPointHelper<float>.GetExponentByteCount(float.Epsilon));
            Assert.Equal(1, FloatingPointHelper<float>.GetExponentByteCount(MaxSubnormal));
            Assert.Equal(1, FloatingPointHelper<float>.GetExponentByteCount(MinNormal));
            Assert.Equal(1, FloatingPointHelper<float>.GetExponentByteCount(1.0f));
            Assert.Equal(1, FloatingPointHelper<float>.GetExponentByteCount(float.MaxValue));
            Assert.Equal(1, FloatingPointHelper<float>.GetExponentByteCount(float.PositiveInfinity));
        }

        [Fact]
        public static void GetExponentShortestBitLengthTest()
        {
            Assert.Equal(8, FloatingPointHelper<float>.GetExponentShortestBitLength(float.NegativeInfinity));
            Assert.Equal(7, FloatingPointHelper<float>.GetExponentShortestBitLength(float.MinValue));
            Assert.Equal(0, FloatingPointHelper<float>.GetExponentShortestBitLength(-1.0f));
            Assert.Equal(8, FloatingPointHelper<float>.GetExponentShortestBitLength(-MinNormal));
            Assert.Equal(8, FloatingPointHelper<float>.GetExponentShortestBitLength(-MaxSubnormal));
            Assert.Equal(8, FloatingPointHelper<float>.GetExponentShortestBitLength(-float.Epsilon));
            Assert.Equal(8, FloatingPointHelper<float>.GetExponentShortestBitLength(-0.0f));
            Assert.Equal(8, FloatingPointHelper<float>.GetExponentShortestBitLength(float.NaN));
            Assert.Equal(8, FloatingPointHelper<float>.GetExponentShortestBitLength(0.0f));
            Assert.Equal(8, FloatingPointHelper<float>.GetExponentShortestBitLength(float.Epsilon));
            Assert.Equal(8, FloatingPointHelper<float>.GetExponentShortestBitLength(MaxSubnormal));
            Assert.Equal(8, FloatingPointHelper<float>.GetExponentShortestBitLength(MinNormal));
            Assert.Equal(0, FloatingPointHelper<float>.GetExponentShortestBitLength(1.0f));
            Assert.Equal(7, FloatingPointHelper<float>.GetExponentShortestBitLength(float.MaxValue));
            Assert.Equal(8, FloatingPointHelper<float>.GetExponentShortestBitLength(float.PositiveInfinity));
        }

        [Fact]
        public static void GetSignificandByteCountTest()
        {
            Assert.Equal(4, FloatingPointHelper<float>.GetSignificandByteCount(float.NegativeInfinity));
            Assert.Equal(4, FloatingPointHelper<float>.GetSignificandByteCount(float.MinValue));
            Assert.Equal(4, FloatingPointHelper<float>.GetSignificandByteCount(-1.0f));
            Assert.Equal(4, FloatingPointHelper<float>.GetSignificandByteCount(-MinNormal));
            Assert.Equal(4, FloatingPointHelper<float>.GetSignificandByteCount(-MaxSubnormal));
            Assert.Equal(4, FloatingPointHelper<float>.GetSignificandByteCount(-float.Epsilon));
            Assert.Equal(4, FloatingPointHelper<float>.GetSignificandByteCount(-0.0f));
            Assert.Equal(4, FloatingPointHelper<float>.GetSignificandByteCount(float.NaN));
            Assert.Equal(4, FloatingPointHelper<float>.GetSignificandByteCount(0.0f));
            Assert.Equal(4, FloatingPointHelper<float>.GetSignificandByteCount(float.Epsilon));
            Assert.Equal(4, FloatingPointHelper<float>.GetSignificandByteCount(MaxSubnormal));
            Assert.Equal(4, FloatingPointHelper<float>.GetSignificandByteCount(MinNormal));
            Assert.Equal(4, FloatingPointHelper<float>.GetSignificandByteCount(1.0f));
            Assert.Equal(4, FloatingPointHelper<float>.GetSignificandByteCount(float.MaxValue));
            Assert.Equal(4, FloatingPointHelper<float>.GetSignificandByteCount(float.PositiveInfinity));
        }

        [Fact]
        public static void GetSignificandBitLengthTest()
        {
            Assert.Equal(24, FloatingPointHelper<float>.GetSignificandBitLength(float.NegativeInfinity));
            Assert.Equal(24, FloatingPointHelper<float>.GetSignificandBitLength(float.MinValue));
            Assert.Equal(24, FloatingPointHelper<float>.GetSignificandBitLength(-1.0f));
            Assert.Equal(24, FloatingPointHelper<float>.GetSignificandBitLength(-MinNormal));
            Assert.Equal(24, FloatingPointHelper<float>.GetSignificandBitLength(-MaxSubnormal));
            Assert.Equal(24, FloatingPointHelper<float>.GetSignificandBitLength(-float.Epsilon));
            Assert.Equal(24, FloatingPointHelper<float>.GetSignificandBitLength(-0.0f));
            Assert.Equal(24, FloatingPointHelper<float>.GetSignificandBitLength(float.NaN));
            Assert.Equal(24, FloatingPointHelper<float>.GetSignificandBitLength(0.0f));
            Assert.Equal(24, FloatingPointHelper<float>.GetSignificandBitLength(float.Epsilon));
            Assert.Equal(24, FloatingPointHelper<float>.GetSignificandBitLength(MaxSubnormal));
            Assert.Equal(24, FloatingPointHelper<float>.GetSignificandBitLength(MinNormal));
            Assert.Equal(24, FloatingPointHelper<float>.GetSignificandBitLength(1.0f));
            Assert.Equal(24, FloatingPointHelper<float>.GetSignificandBitLength(float.MaxValue));
            Assert.Equal(24, FloatingPointHelper<float>.GetSignificandBitLength(float.PositiveInfinity));
        }

        [Fact]
        public static void TryWriteExponentBigEndianTest()
        {
            Span<byte> destination = stackalloc byte[1];
            int bytesWritten = 0;

            Assert.True(FloatingPointHelper<float>.TryWriteExponentBigEndian(float.NegativeInfinity, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x80 }, destination.ToArray()); // -128

            Assert.True(FloatingPointHelper<float>.TryWriteExponentBigEndian(float.MinValue, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x7F }, destination.ToArray()); // +127

            Assert.True(FloatingPointHelper<float>.TryWriteExponentBigEndian(-1.0f, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x00 }, destination.ToArray()); // +0

            Assert.True(FloatingPointHelper<float>.TryWriteExponentBigEndian(-MinNormal, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x82 }, destination.ToArray()); // -126

            Assert.True(FloatingPointHelper<float>.TryWriteExponentBigEndian(-MaxSubnormal, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x81 }, destination.ToArray()); // -127

            Assert.True(FloatingPointHelper<float>.TryWriteExponentBigEndian(-float.Epsilon, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x81 }, destination.ToArray()); // -127

            Assert.True(FloatingPointHelper<float>.TryWriteExponentBigEndian(-0.0f, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x81 }, destination.ToArray()); // -127

            Assert.True(FloatingPointHelper<float>.TryWriteExponentBigEndian(float.NaN, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x80 }, destination.ToArray()); // -128

            Assert.True(FloatingPointHelper<float>.TryWriteExponentBigEndian(0.0f, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x81 }, destination.ToArray()); // -127

            Assert.True(FloatingPointHelper<float>.TryWriteExponentBigEndian(float.Epsilon, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x81 }, destination.ToArray()); // -127

            Assert.True(FloatingPointHelper<float>.TryWriteExponentBigEndian(MaxSubnormal, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x81 }, destination.ToArray()); // -127

            Assert.True(FloatingPointHelper<float>.TryWriteExponentBigEndian(MinNormal, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x82 }, destination.ToArray()); // -126

            Assert.True(FloatingPointHelper<float>.TryWriteExponentBigEndian(1.0f, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x00 }, destination.ToArray()); // +0

            Assert.True(FloatingPointHelper<float>.TryWriteExponentBigEndian(float.MaxValue, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x7F }, destination.ToArray()); // +127

            Assert.True(FloatingPointHelper<float>.TryWriteExponentBigEndian(float.PositiveInfinity, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x80 }, destination.ToArray()); // -128

            Assert.False(FloatingPointHelper<float>.TryWriteExponentBigEndian(default, Span<byte>.Empty, out bytesWritten));
            Assert.Equal(0, bytesWritten);
            Assert.Equal(new byte[] { 0x80 }, destination.ToArray());
        }

        [Fact]
        public static void TryWriteExponentLittleEndianTest()
        {
            Span<byte> destination = stackalloc byte[1];
            int bytesWritten = 0;

            Assert.True(FloatingPointHelper<float>.TryWriteExponentLittleEndian(float.NegativeInfinity, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x80 }, destination.ToArray()); // -128

            Assert.True(FloatingPointHelper<float>.TryWriteExponentLittleEndian(float.MinValue, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x7F }, destination.ToArray()); // +127

            Assert.True(FloatingPointHelper<float>.TryWriteExponentLittleEndian(-1.0f, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x00 }, destination.ToArray()); // +0

            Assert.True(FloatingPointHelper<float>.TryWriteExponentLittleEndian(-MinNormal, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x82 }, destination.ToArray()); // -126

            Assert.True(FloatingPointHelper<float>.TryWriteExponentLittleEndian(-MaxSubnormal, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x81 }, destination.ToArray()); // -127

            Assert.True(FloatingPointHelper<float>.TryWriteExponentLittleEndian(-float.Epsilon, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x81 }, destination.ToArray()); // -127

            Assert.True(FloatingPointHelper<float>.TryWriteExponentLittleEndian(-0.0f, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x81 }, destination.ToArray()); // -127

            Assert.True(FloatingPointHelper<float>.TryWriteExponentLittleEndian(float.NaN, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x80 }, destination.ToArray()); // -128

            Assert.True(FloatingPointHelper<float>.TryWriteExponentLittleEndian(0.0f, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x81 }, destination.ToArray()); // -127

            Assert.True(FloatingPointHelper<float>.TryWriteExponentLittleEndian(float.Epsilon, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x81 }, destination.ToArray()); // -127

            Assert.True(FloatingPointHelper<float>.TryWriteExponentLittleEndian(MaxSubnormal, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x81 }, destination.ToArray()); // -127

            Assert.True(FloatingPointHelper<float>.TryWriteExponentLittleEndian(MinNormal, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x82 }, destination.ToArray()); // -126

            Assert.True(FloatingPointHelper<float>.TryWriteExponentLittleEndian(1.0f, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x00 }, destination.ToArray()); // +0

            Assert.True(FloatingPointHelper<float>.TryWriteExponentLittleEndian(float.MaxValue, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x7F }, destination.ToArray()); // +127

            Assert.True(FloatingPointHelper<float>.TryWriteExponentLittleEndian(float.PositiveInfinity, destination, out bytesWritten));
            Assert.Equal(1, bytesWritten);
            Assert.Equal(new byte[] { 0x80 }, destination.ToArray()); // -128

            Assert.False(FloatingPointHelper<float>.TryWriteExponentLittleEndian(default, Span<byte>.Empty, out bytesWritten));
            Assert.Equal(0, bytesWritten);
            Assert.Equal(new byte[] { 0x80 }, destination.ToArray());
        }

        [Fact]
        public static void TryWriteSignificandBigEndianTest()
        {
            Span<byte> destination = stackalloc byte[4];
            int bytesWritten = 0;

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandBigEndian(float.NegativeInfinity, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x80, 0x00, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandBigEndian(float.MinValue, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0xFF, 0xFF, 0xff }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandBigEndian(-1.0f, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x80, 0x00, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandBigEndian(-MinNormal, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x80, 0x00, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandBigEndian(-MaxSubnormal, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x7F, 0xFF, 0xFF }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandBigEndian(-float.Epsilon, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x00, 0x00, 0x01 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandBigEndian(-0.0f, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x00, 0x00, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandBigEndian(float.NaN, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0xC0, 0x00, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandBigEndian(0.0f, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x00, 0x00, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandBigEndian(float.Epsilon, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x00, 0x00, 0x01 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandBigEndian(MaxSubnormal, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x7F, 0xFF, 0xFF }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandBigEndian(MinNormal, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x80, 0x00, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandBigEndian(1.0f, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x80, 0x00, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandBigEndian(float.MaxValue, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0xFF, 0xFF, 0xFF }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandBigEndian(float.PositiveInfinity, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x80, 0x00, 0x00 }, destination.ToArray());

            Assert.False(FloatingPointHelper<float>.TryWriteSignificandBigEndian(default, Span<byte>.Empty, out bytesWritten));
            Assert.Equal(0, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x80, 0x00, 0x00 }, destination.ToArray());
        }

        [Fact]
        public static void TryWriteSignificandLittleEndianTest()
        {
            Span<byte> destination = stackalloc byte[4];
            int bytesWritten = 0;

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandLittleEndian(float.NegativeInfinity, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x00, 0x80, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandLittleEndian(float.MinValue, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0xFF, 0xFF, 0xFF, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandLittleEndian(-1.0f, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x00, 0x80, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandLittleEndian(-MinNormal, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x00, 0x80, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandLittleEndian(-MaxSubnormal, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0xFF, 0xFF, 0x7F, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandLittleEndian(-float.Epsilon, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x01, 0x00, 0x00, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandLittleEndian(-0.0f, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x00, 0x00, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandLittleEndian(float.NaN, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x00, 0xC0, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandLittleEndian(0.0f, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x00, 0x00, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandLittleEndian(float.Epsilon, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x01, 0x00, 0x00, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandLittleEndian(MaxSubnormal, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0xFF, 0xFF, 0x7F, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandLittleEndian(MinNormal, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x00, 0x80, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandLittleEndian(1.0f, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x00, 0x80, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandLittleEndian(float.MaxValue, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0xFF, 0xFF, 0xFF, 0x00 }, destination.ToArray());

            Assert.True(FloatingPointHelper<float>.TryWriteSignificandLittleEndian(float.PositiveInfinity, destination, out bytesWritten));
            Assert.Equal(4, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x00, 0x80, 0x00 }, destination.ToArray());

            Assert.False(FloatingPointHelper<float>.TryWriteSignificandLittleEndian(default, Span<byte>.Empty, out bytesWritten));
            Assert.Equal(0, bytesWritten);
            Assert.Equal(new byte[] { 0x00, 0x00, 0x80, 0x00 }, destination.ToArray());
        }

        //
        // IIncrementOperators
        //

        [Fact]
        public static void op_IncrementTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, IncrementOperatorsHelper<float>.op_Increment(float.NegativeInfinity));
            AssertBitwiseEqual(float.MinValue, IncrementOperatorsHelper<float>.op_Increment(float.MinValue));
            AssertBitwiseEqual(0.0f, IncrementOperatorsHelper<float>.op_Increment(-1.0f));
            AssertBitwiseEqual(1.0f, IncrementOperatorsHelper<float>.op_Increment(-MinNormal));
            AssertBitwiseEqual(1.0f, IncrementOperatorsHelper<float>.op_Increment(-MaxSubnormal));
            AssertBitwiseEqual(1.0f, IncrementOperatorsHelper<float>.op_Increment(-float.Epsilon));
            AssertBitwiseEqual(1.0f, IncrementOperatorsHelper<float>.op_Increment(-0.0f));
            AssertBitwiseEqual(float.NaN, IncrementOperatorsHelper<float>.op_Increment(float.NaN));
            AssertBitwiseEqual(1.0f, IncrementOperatorsHelper<float>.op_Increment(0.0f));
            AssertBitwiseEqual(1.0f, IncrementOperatorsHelper<float>.op_Increment(float.Epsilon));
            AssertBitwiseEqual(1.0f, IncrementOperatorsHelper<float>.op_Increment(MaxSubnormal));
            AssertBitwiseEqual(1.0f, IncrementOperatorsHelper<float>.op_Increment(MinNormal));
            AssertBitwiseEqual(2.0f, IncrementOperatorsHelper<float>.op_Increment(1.0f));
            AssertBitwiseEqual(float.MaxValue, IncrementOperatorsHelper<float>.op_Increment(float.MaxValue));
            AssertBitwiseEqual(float.PositiveInfinity, IncrementOperatorsHelper<float>.op_Increment(float.PositiveInfinity));
        }

        [Fact]
        public static void op_CheckedIncrementTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, IncrementOperatorsHelper<float>.op_CheckedIncrement(float.NegativeInfinity));
            AssertBitwiseEqual(float.MinValue, IncrementOperatorsHelper<float>.op_CheckedIncrement(float.MinValue));
            AssertBitwiseEqual(0.0f, IncrementOperatorsHelper<float>.op_CheckedIncrement(-1.0f));
            AssertBitwiseEqual(1.0f, IncrementOperatorsHelper<float>.op_CheckedIncrement(-MinNormal));
            AssertBitwiseEqual(1.0f, IncrementOperatorsHelper<float>.op_CheckedIncrement(-MaxSubnormal));
            AssertBitwiseEqual(1.0f, IncrementOperatorsHelper<float>.op_CheckedIncrement(-float.Epsilon));
            AssertBitwiseEqual(1.0f, IncrementOperatorsHelper<float>.op_CheckedIncrement(-0.0f));
            AssertBitwiseEqual(float.NaN, IncrementOperatorsHelper<float>.op_CheckedIncrement(float.NaN));
            AssertBitwiseEqual(1.0f, IncrementOperatorsHelper<float>.op_CheckedIncrement(0.0f));
            AssertBitwiseEqual(1.0f, IncrementOperatorsHelper<float>.op_CheckedIncrement(float.Epsilon));
            AssertBitwiseEqual(1.0f, IncrementOperatorsHelper<float>.op_CheckedIncrement(MaxSubnormal));
            AssertBitwiseEqual(1.0f, IncrementOperatorsHelper<float>.op_CheckedIncrement(MinNormal));
            AssertBitwiseEqual(2.0f, IncrementOperatorsHelper<float>.op_CheckedIncrement(1.0f));
            AssertBitwiseEqual(float.MaxValue, IncrementOperatorsHelper<float>.op_CheckedIncrement(float.MaxValue));
            AssertBitwiseEqual(float.PositiveInfinity, IncrementOperatorsHelper<float>.op_CheckedIncrement(float.PositiveInfinity));
        }

        //
        // IMinMaxValue
        //

        [Fact]
        public static void MaxValueTest()
        {
            AssertBitwiseEqual(float.MaxValue, MinMaxValueHelper<float>.MaxValue);
        }

        [Fact]
        public static void MinValueTest()
        {
            AssertBitwiseEqual(float.MinValue, MinMaxValueHelper<float>.MinValue);
        }

        //
        // IModulusOperators
        //

        [Fact]
        public static void op_ModulusTest()
        {
            AssertBitwiseEqual(float.NaN, ModulusOperatorsHelper<float, float, float>.op_Modulus(float.NegativeInfinity, 2.0f));
            AssertBitwiseEqual(-0.0f, ModulusOperatorsHelper<float, float, float>.op_Modulus(float.MinValue, 2.0f));
            AssertBitwiseEqual(-1.0f, ModulusOperatorsHelper<float, float, float>.op_Modulus(-1.0f, 2.0f));
            AssertBitwiseEqual(-MinNormal, ModulusOperatorsHelper<float, float, float>.op_Modulus(-MinNormal, 2.0f));
            AssertBitwiseEqual(-MaxSubnormal, ModulusOperatorsHelper<float, float, float>.op_Modulus(-MaxSubnormal, 2.0f));
            AssertBitwiseEqual(-float.Epsilon, ModulusOperatorsHelper<float, float, float>.op_Modulus(-float.Epsilon, 2.0f)); ;
            AssertBitwiseEqual(-0.0f, ModulusOperatorsHelper<float, float, float>.op_Modulus(-0.0f, 2.0f));
            AssertBitwiseEqual(float.NaN, ModulusOperatorsHelper<float, float, float>.op_Modulus(float.NaN, 2.0f));
            AssertBitwiseEqual(0.0f, ModulusOperatorsHelper<float, float, float>.op_Modulus(0.0f, 2.0f));
            AssertBitwiseEqual(float.Epsilon, ModulusOperatorsHelper<float, float, float>.op_Modulus(float.Epsilon, 2.0f));
            AssertBitwiseEqual(MaxSubnormal, ModulusOperatorsHelper<float, float, float>.op_Modulus(MaxSubnormal, 2.0f));
            AssertBitwiseEqual(MinNormal, ModulusOperatorsHelper<float, float, float>.op_Modulus(MinNormal, 2.0f));
            AssertBitwiseEqual(1.0f, ModulusOperatorsHelper<float, float, float>.op_Modulus(1.0f, 2.0f));
            AssertBitwiseEqual(0.0f, ModulusOperatorsHelper<float, float, float>.op_Modulus(float.MaxValue, 2.0f));
            AssertBitwiseEqual(float.NaN, ModulusOperatorsHelper<float, float, float>.op_Modulus(float.PositiveInfinity, 2.0f));
        }

        //
        // IMultiplicativeIdentity
        //

        [Fact]
        public static void MultiplicativeIdentityTest()
        {
            AssertBitwiseEqual(1.0f, MultiplicativeIdentityHelper<float, float>.MultiplicativeIdentity);
        }

        //
        // IMultiplyOperators
        //

        [Fact]
        public static void op_MultiplyTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, MultiplyOperatorsHelper<float, float, float>.op_Multiply(float.NegativeInfinity, 2.0f));
            AssertBitwiseEqual(float.NegativeInfinity, MultiplyOperatorsHelper<float, float, float>.op_Multiply(float.MinValue, 2.0f));
            AssertBitwiseEqual(-2.0f, MultiplyOperatorsHelper<float, float, float>.op_Multiply(-1.0f, 2.0f));
            AssertBitwiseEqual(-2.3509887E-38f, MultiplyOperatorsHelper<float, float, float>.op_Multiply(-MinNormal, 2.0f));
            AssertBitwiseEqual(-2.35098842E-38f, MultiplyOperatorsHelper<float, float, float>.op_Multiply(-MaxSubnormal, 2.0f));
            AssertBitwiseEqual(-2.80259693E-45f, MultiplyOperatorsHelper<float, float, float>.op_Multiply(-float.Epsilon, 2.0f));
            AssertBitwiseEqual(-0.0f, MultiplyOperatorsHelper<float, float, float>.op_Multiply(-0.0f, 2.0f));
            AssertBitwiseEqual(float.NaN, MultiplyOperatorsHelper<float, float, float>.op_Multiply(float.NaN, 2.0f));
            AssertBitwiseEqual(0.0f, MultiplyOperatorsHelper<float, float, float>.op_Multiply(0.0f, 2.0f));
            AssertBitwiseEqual(2.80259693E-45f, MultiplyOperatorsHelper<float, float, float>.op_Multiply(float.Epsilon, 2.0f));
            AssertBitwiseEqual(2.35098842E-38f, MultiplyOperatorsHelper<float, float, float>.op_Multiply(MaxSubnormal, 2.0f));
            AssertBitwiseEqual(2.3509887E-38f, MultiplyOperatorsHelper<float, float, float>.op_Multiply(MinNormal, 2.0f));
            AssertBitwiseEqual(2.0f, MultiplyOperatorsHelper<float, float, float>.op_Multiply(1.0f, 2.0f));
            AssertBitwiseEqual(float.PositiveInfinity, MultiplyOperatorsHelper<float, float, float>.op_Multiply(float.MaxValue, 2.0f));
            AssertBitwiseEqual(float.PositiveInfinity, MultiplyOperatorsHelper<float, float, float>.op_Multiply(float.PositiveInfinity, 2.0f));
        }

        [Fact]
        public static void op_CheckedMultiplyTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, MultiplyOperatorsHelper<float, float, float>.op_CheckedMultiply(float.NegativeInfinity, 2.0f));
            AssertBitwiseEqual(float.NegativeInfinity, MultiplyOperatorsHelper<float, float, float>.op_CheckedMultiply(float.MinValue, 2.0f));
            AssertBitwiseEqual(-2.0f, MultiplyOperatorsHelper<float, float, float>.op_CheckedMultiply(-1.0f, 2.0f));
            AssertBitwiseEqual(-2.3509887E-38f, MultiplyOperatorsHelper<float, float, float>.op_CheckedMultiply(-MinNormal, 2.0f));
            AssertBitwiseEqual(-2.35098842E-38f, MultiplyOperatorsHelper<float, float, float>.op_CheckedMultiply(-MaxSubnormal, 2.0f));
            AssertBitwiseEqual(-2.80259693E-45f, MultiplyOperatorsHelper<float, float, float>.op_CheckedMultiply(-float.Epsilon, 2.0f));
            AssertBitwiseEqual(-0.0f, MultiplyOperatorsHelper<float, float, float>.op_CheckedMultiply(-0.0f, 2.0f));
            AssertBitwiseEqual(float.NaN, MultiplyOperatorsHelper<float, float, float>.op_CheckedMultiply(float.NaN, 2.0f));
            AssertBitwiseEqual(0.0f, MultiplyOperatorsHelper<float, float, float>.op_CheckedMultiply(0.0f, 2.0f));
            AssertBitwiseEqual(2.80259693E-45f, MultiplyOperatorsHelper<float, float, float>.op_CheckedMultiply(float.Epsilon, 2.0f));
            AssertBitwiseEqual(2.35098842E-38f, MultiplyOperatorsHelper<float, float, float>.op_CheckedMultiply(MaxSubnormal, 2.0f));
            AssertBitwiseEqual(2.3509887E-38f, MultiplyOperatorsHelper<float, float, float>.op_CheckedMultiply(MinNormal, 2.0f));
            AssertBitwiseEqual(2.0f, MultiplyOperatorsHelper<float, float, float>.op_CheckedMultiply(1.0f, 2.0f));
            AssertBitwiseEqual(float.PositiveInfinity, MultiplyOperatorsHelper<float, float, float>.op_CheckedMultiply(float.MaxValue, 2.0f));
            AssertBitwiseEqual(float.PositiveInfinity, MultiplyOperatorsHelper<float, float, float>.op_CheckedMultiply(float.PositiveInfinity, 2.0f));
        }

        //
        // INumber
        //

        [Fact]
        public static void ClampTest()
        {
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Clamp(float.NegativeInfinity, 1.0f, 63.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Clamp(float.MinValue, 1.0f, 63.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Clamp(-1.0f, 1.0f, 63.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Clamp(-MinNormal, 1.0f, 63.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Clamp(-MaxSubnormal, 1.0f, 63.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Clamp(-float.Epsilon, 1.0f, 63.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Clamp(-0.0f, 1.0f, 63.0f));
            AssertBitwiseEqual(float.NaN, NumberHelper<float>.Clamp(float.NaN, 1.0f, 63.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Clamp(0.0f, 1.0f, 63.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Clamp(float.Epsilon, 1.0f, 63.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Clamp(MaxSubnormal, 1.0f, 63.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Clamp(MinNormal, 1.0f, 63.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Clamp(1.0f, 1.0f, 63.0f));
            AssertBitwiseEqual(63.0f, NumberHelper<float>.Clamp(float.MaxValue, 1.0f, 63.0f));
            AssertBitwiseEqual(63.0f, NumberHelper<float>.Clamp(float.PositiveInfinity, 1.0f, 63.0f));
        }

        [Fact]
        public static void MaxTest()
        {
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Max(float.NegativeInfinity, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Max(float.MinValue, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Max(-1.0f, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Max(-MinNormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Max(-MaxSubnormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Max(-float.Epsilon, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Max(-0.0f, 1.0f));
            AssertBitwiseEqual(float.NaN, NumberHelper<float>.Max(float.NaN, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Max(0.0f, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Max(float.Epsilon, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Max(MaxSubnormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Max(MinNormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Max(1.0f, 1.0f));
            AssertBitwiseEqual(float.MaxValue, NumberHelper<float>.Max(float.MaxValue, 1.0f));
            AssertBitwiseEqual(float.PositiveInfinity, NumberHelper<float>.Max(float.PositiveInfinity, 1.0f));
        }

        [Fact]
        public static void MaxNumberTest()
        {
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MaxNumber(float.NegativeInfinity, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MaxNumber(float.MinValue, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MaxNumber(-1.0f, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MaxNumber(-MinNormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MaxNumber(-MaxSubnormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MaxNumber(-float.Epsilon, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MaxNumber(-0.0f, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MaxNumber(float.NaN, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MaxNumber(0.0f, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MaxNumber(float.Epsilon, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MaxNumber(MaxSubnormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MaxNumber(MinNormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MaxNumber(1.0f, 1.0f));
            AssertBitwiseEqual(float.MaxValue, NumberHelper<float>.MaxNumber(float.MaxValue, 1.0f));
            AssertBitwiseEqual(float.PositiveInfinity, NumberHelper<float>.MaxNumber(float.PositiveInfinity, 1.0f));
        }

        [Fact]
        public static void MinTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, NumberHelper<float>.Min(float.NegativeInfinity, 1.0f));
            AssertBitwiseEqual(float.MinValue, NumberHelper<float>.Min(float.MinValue, 1.0f));
            AssertBitwiseEqual(-1.0f, NumberHelper<float>.Min(-1.0f, 1.0f));
            AssertBitwiseEqual(-MinNormal, NumberHelper<float>.Min(-MinNormal, 1.0f));
            AssertBitwiseEqual(-MaxSubnormal, NumberHelper<float>.Min(-MaxSubnormal, 1.0f));
            AssertBitwiseEqual(-float.Epsilon, NumberHelper<float>.Min(-float.Epsilon, 1.0f));
            AssertBitwiseEqual(-0.0f, NumberHelper<float>.Min(-0.0f, 1.0f));
            AssertBitwiseEqual(float.NaN, NumberHelper<float>.Min(float.NaN, 1.0f));
            AssertBitwiseEqual(0.0f, NumberHelper<float>.Min(0.0f, 1.0f));
            AssertBitwiseEqual(float.Epsilon, NumberHelper<float>.Min(float.Epsilon, 1.0f));
            AssertBitwiseEqual(MaxSubnormal, NumberHelper<float>.Min(MaxSubnormal, 1.0f));
            AssertBitwiseEqual(MinNormal, NumberHelper<float>.Min(MinNormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Min(1.0f, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Min(float.MaxValue, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.Min(float.PositiveInfinity, 1.0f));
        }

        [Fact]
        public static void MinNumberTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, NumberHelper<float>.MinNumber(float.NegativeInfinity, 1.0f));
            AssertBitwiseEqual(float.MinValue, NumberHelper<float>.MinNumber(float.MinValue, 1.0f));
            AssertBitwiseEqual(-1.0f, NumberHelper<float>.MinNumber(-1.0f, 1.0f));
            AssertBitwiseEqual(-MinNormal, NumberHelper<float>.MinNumber(-MinNormal, 1.0f));
            AssertBitwiseEqual(-MaxSubnormal, NumberHelper<float>.MinNumber(-MaxSubnormal, 1.0f));
            AssertBitwiseEqual(-float.Epsilon, NumberHelper<float>.MinNumber(-float.Epsilon, 1.0f));
            AssertBitwiseEqual(-0.0f, NumberHelper<float>.MinNumber(-0.0f, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MinNumber(float.NaN, 1.0f));
            AssertBitwiseEqual(0.0f, NumberHelper<float>.MinNumber(0.0f, 1.0f));
            AssertBitwiseEqual(float.Epsilon, NumberHelper<float>.MinNumber(float.Epsilon, 1.0f));
            AssertBitwiseEqual(MaxSubnormal, NumberHelper<float>.MinNumber(MaxSubnormal, 1.0f));
            AssertBitwiseEqual(MinNormal, NumberHelper<float>.MinNumber(MinNormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MinNumber(1.0f, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MinNumber(float.MaxValue, 1.0f));
            AssertBitwiseEqual(1.0f, NumberHelper<float>.MinNumber(float.PositiveInfinity, 1.0f));
        }

        [Fact]
        public static void SignTest()
        {
            Assert.Equal(-1, NumberHelper<float>.Sign(float.NegativeInfinity));
            Assert.Equal(-1, NumberHelper<float>.Sign(float.MinValue));
            Assert.Equal(-1, NumberHelper<float>.Sign(-1.0f));
            Assert.Equal(-1, NumberHelper<float>.Sign(-MinNormal));
            Assert.Equal(-1, NumberHelper<float>.Sign(-MaxSubnormal));
            Assert.Equal(-1, NumberHelper<float>.Sign(-float.Epsilon));

            Assert.Equal(0, NumberHelper<float>.Sign(-0.0f));
            Assert.Equal(0, NumberHelper<float>.Sign(0.0f));

            Assert.Equal(1, NumberHelper<float>.Sign(float.Epsilon));
            Assert.Equal(1, NumberHelper<float>.Sign(MaxSubnormal));
            Assert.Equal(1, NumberHelper<float>.Sign(MinNormal));
            Assert.Equal(1, NumberHelper<float>.Sign(1.0f));
            Assert.Equal(1, NumberHelper<float>.Sign(float.MaxValue));
            Assert.Equal(1, NumberHelper<float>.Sign(float.PositiveInfinity));

            Assert.Throws<ArithmeticException>(() => NumberHelper<float>.Sign(float.NaN));
        }

        //
        // INumberBase
        //

        [Fact]
        public static void OneTest()
        {
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.One);
        }

        [Fact]
        public static void ZeroTest()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.Zero);
        }

        [Fact]
        public static void AbsTest()
        {
            AssertBitwiseEqual(float.PositiveInfinity, NumberBaseHelper<float>.Abs(float.NegativeInfinity));
            AssertBitwiseEqual(float.MaxValue, NumberBaseHelper<float>.Abs(float.MinValue));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.Abs(-1.0f));
            AssertBitwiseEqual(MinNormal, NumberBaseHelper<float>.Abs(-MinNormal));
            AssertBitwiseEqual(MaxSubnormal, NumberBaseHelper<float>.Abs(-MaxSubnormal));
            AssertBitwiseEqual(float.Epsilon, NumberBaseHelper<float>.Abs(-float.Epsilon));
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.Abs(-0.0f));
            AssertBitwiseEqual(float.NaN, NumberBaseHelper<float>.Abs(float.NaN));
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.Abs(0.0f));
            AssertBitwiseEqual(float.Epsilon, NumberBaseHelper<float>.Abs(float.Epsilon));
            AssertBitwiseEqual(MaxSubnormal, NumberBaseHelper<float>.Abs(MaxSubnormal));
            AssertBitwiseEqual(MinNormal, NumberBaseHelper<float>.Abs(MinNormal));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.Abs(1.0f));
            AssertBitwiseEqual(float.MaxValue, NumberBaseHelper<float>.Abs(float.MaxValue));
            AssertBitwiseEqual(float.PositiveInfinity, NumberBaseHelper<float>.Abs(float.PositiveInfinity));
        }

        [Fact]
        public static void CreateCheckedFromByteTest()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateChecked<byte>(0x00));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateChecked<byte>(0x01));
            AssertBitwiseEqual(127.0f, NumberBaseHelper<float>.CreateChecked<byte>(0x7F));
            AssertBitwiseEqual(128.0f, NumberBaseHelper<float>.CreateChecked<byte>(0x80));
            AssertBitwiseEqual(255.0f, NumberBaseHelper<float>.CreateChecked<byte>(0xFF));
        }

        [Fact]
        public static void CreateCheckedFromCharTest()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateChecked<char>((char)0x0000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateChecked<char>((char)0x0001));
            AssertBitwiseEqual(32767.0f, NumberBaseHelper<float>.CreateChecked<char>((char)0x7FFF));
            AssertBitwiseEqual(32768.0f, NumberBaseHelper<float>.CreateChecked<char>((char)0x8000));
            AssertBitwiseEqual(65535.0f, NumberBaseHelper<float>.CreateChecked<char>((char)0xFFFF));
        }

        [Fact]
        public static void CreateCheckedFromInt16Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateChecked<short>(0x0000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateChecked<short>(0x0001));
            AssertBitwiseEqual(32767.0f, NumberBaseHelper<float>.CreateChecked<short>(0x7FFF));
            AssertBitwiseEqual(-32768.0f, NumberBaseHelper<float>.CreateChecked<short>(unchecked((short)0x8000)));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateChecked<short>(unchecked((short)0xFFFF)));
        }

        [Fact]
        public static void CreateCheckedFromInt32Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateChecked<int>(0x00000000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateChecked<int>(0x00000001));
            AssertBitwiseEqual(2147483647.0f, NumberBaseHelper<float>.CreateChecked<int>(0x7FFFFFFF));
            AssertBitwiseEqual(-2147483648.0f, NumberBaseHelper<float>.CreateChecked<int>(unchecked((int)0x80000000)));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateChecked<int>(unchecked((int)0xFFFFFFFF)));
        }

        [Fact]
        public static void CreateCheckedFromInt64Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateChecked<long>(0x0000000000000000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateChecked<long>(0x0000000000000001));
            AssertBitwiseEqual(9223372036854775807.0f, NumberBaseHelper<float>.CreateChecked<long>(0x7FFFFFFFFFFFFFFF));
            AssertBitwiseEqual(-9223372036854775808.0f, NumberBaseHelper<float>.CreateChecked<long>(unchecked(unchecked((long)0x8000000000000000))));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateChecked<long>(unchecked(unchecked((long)0xFFFFFFFFFFFFFFFF))));
        }

        [Fact]
        public static void CreateCheckedFromInt128Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateChecked<Int128>(new Int128(0x0000_0000_0000_0000, 0x0000_0000_0000_0000)));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateChecked<Int128>(new Int128(0x0000_0000_0000_0000, 0x0000_0000_0000_0001)));
            AssertBitwiseEqual(170141183460469231731687303715884105727.0f, NumberBaseHelper<float>.CreateChecked<Int128>(new Int128(0x7FFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF)));
            AssertBitwiseEqual(-170141183460469231731687303715884105728.0f, NumberBaseHelper<float>.CreateChecked<Int128>(new Int128(0x8000_0000_0000_0000, 0x0000_0000_0000_0000)));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateChecked<Int128>(new Int128(0xFFFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF)));
        }

        [Fact]
        public static void CreateCheckedFromIntPtrTest()
        {
            if (Environment.Is64BitProcess)
            {
                AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateChecked<nint>(unchecked((nint)0x0000000000000000)));
                AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateChecked<nint>(unchecked((nint)0x0000000000000001)));
                AssertBitwiseEqual(9223372036854775807.0f, NumberBaseHelper<float>.CreateChecked<nint>(unchecked((nint)0x7FFFFFFFFFFFFFFF)));
                AssertBitwiseEqual(-9223372036854775808.0f, NumberBaseHelper<float>.CreateChecked<nint>(unchecked((nint)0x8000000000000000)));
                AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateChecked<nint>(unchecked((nint)0xFFFFFFFFFFFFFFFF)));
            }
            else
            {
                AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateChecked<nint>((nint)0x00000000));
                AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateChecked<nint>((nint)0x00000001));
                AssertBitwiseEqual(2147483647.0f, NumberBaseHelper<float>.CreateChecked<nint>((nint)0x7FFFFFFF));
                AssertBitwiseEqual(-2147483648.0f, NumberBaseHelper<float>.CreateChecked<nint>(unchecked((nint)0x80000000)));
                AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateChecked<nint>(unchecked((nint)0xFFFFFFFF)));
            }
        }

        [Fact]
        public static void CreateCheckedFromSByteTest()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateChecked<sbyte>(0x00));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateChecked<sbyte>(0x01));
            AssertBitwiseEqual(127.0f, NumberBaseHelper<float>.CreateChecked<sbyte>(0x7F));
            AssertBitwiseEqual(-128.0f, NumberBaseHelper<float>.CreateChecked<sbyte>(unchecked((sbyte)0x80)));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateChecked<sbyte>(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void CreateCheckedFromUInt16Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateChecked<ushort>(0x0000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateChecked<ushort>(0x0001));
            AssertBitwiseEqual(32767.0f, NumberBaseHelper<float>.CreateChecked<ushort>(0x7FFF));
            AssertBitwiseEqual(32768.0f, NumberBaseHelper<float>.CreateChecked<ushort>(0x8000));
            AssertBitwiseEqual(65535.0f, NumberBaseHelper<float>.CreateChecked<ushort>(0xFFFF));
        }

        [Fact]
        public static void CreateCheckedFromUInt32Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateChecked<uint>(0x00000000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateChecked<uint>(0x00000001));
            AssertBitwiseEqual(2147483647.0f, NumberBaseHelper<float>.CreateChecked<uint>(0x7FFFFFFF));
            AssertBitwiseEqual(2147483648.0f, NumberBaseHelper<float>.CreateChecked<uint>(0x80000000));
            AssertBitwiseEqual(4294967295.0f, NumberBaseHelper<float>.CreateChecked<uint>(0xFFFFFFFF));
        }

        [Fact]
        public static void CreateCheckedFromUInt64Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateChecked<ulong>(0x0000000000000000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateChecked<ulong>(0x0000000000000001));
            AssertBitwiseEqual(9223372036854775807.0f, NumberBaseHelper<float>.CreateChecked<ulong>(0x7FFFFFFFFFFFFFFF));
            AssertBitwiseEqual(9223372036854775808.0f, NumberBaseHelper<float>.CreateChecked<ulong>(0x8000000000000000));
            AssertBitwiseEqual(18446744073709551615.0f, NumberBaseHelper<float>.CreateChecked<ulong>(0xFFFFFFFFFFFFFFFF));
        }

        [Fact]
        public static void CreateCheckedFromUInt128Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateChecked<UInt128>(new UInt128(0x0000_0000_0000_0000, 0x0000_0000_0000_0000)));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateChecked<UInt128>(new UInt128(0x0000_0000_0000_0000, 0x0000_0000_0000_0001)));
            AssertBitwiseEqual(170141183460469231731687303715884105727.0f, NumberBaseHelper<float>.CreateChecked<UInt128>(new UInt128(0x7FFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF)));
            AssertBitwiseEqual(170141183460469231731687303715884105728.0f, NumberBaseHelper<float>.CreateChecked<UInt128>(new UInt128(0x8000_0000_0000_0000, 0x0000_0000_0000_0000)));
            AssertBitwiseEqual(float.PositiveInfinity, NumberBaseHelper<float>.CreateChecked<UInt128>(new UInt128(0xFFFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF)));
        }

        [Fact]
        public static void CreateCheckedFromUIntPtrTest()
        {
            if (Environment.Is64BitProcess)
            {
                AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateChecked<nuint>(unchecked((nuint)0x0000000000000000)));
                AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateChecked<nuint>(unchecked((nuint)0x0000000000000001)));
                AssertBitwiseEqual(9223372036854775807.0f, NumberBaseHelper<float>.CreateChecked<nuint>(unchecked((nuint)0x7FFFFFFFFFFFFFFF)));

                // https://github.com/dotnet/roslyn/issues/60714
                // AssertBitwiseEqual(9223372036854775808.0f, NumberBaseHelper<float>.CreateChecked<nuint>(unchecked((nuint)0x8000000000000000)));
                // AssertBitwiseEqual(18446744073709551615.0f,NumberBaseHelper<float>.CreateChecked<nuint>(unchecked((nuint)0xFFFFFFFFFFFFFFFF)));
            }
            else
            {
                AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateChecked<nuint>((nuint)0x00000000));
                AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateChecked<nuint>((nuint)0x00000001));
                AssertBitwiseEqual(2147483647.0f, NumberBaseHelper<float>.CreateChecked<nuint>((nuint)0x7FFFFFFF));

                // https://github.com/dotnet/roslyn/issues/60714
                // AssertBitwiseEqual(2147483648.0f, NumberBaseHelper<float>.CreateChecked<nuint>((nuint)0x80000000));
                // AssertBitwiseEqual(4294967295.0f, NumberBaseHelper<float>.CreateChecked<nuint>((nuint)0xFFFFFFFF));
            }
        }

        [Fact]
        public static void CreateSaturatingFromByteTest()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateSaturating<byte>(0x00));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateSaturating<byte>(0x01));
            AssertBitwiseEqual(127.0f, NumberBaseHelper<float>.CreateSaturating<byte>(0x7F));
            AssertBitwiseEqual(128.0f, NumberBaseHelper<float>.CreateSaturating<byte>(0x80));
            AssertBitwiseEqual(255.0f, NumberBaseHelper<float>.CreateSaturating<byte>(0xFF));
        }

        [Fact]
        public static void CreateSaturatingFromCharTest()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateSaturating<char>((char)0x0000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateSaturating<char>((char)0x0001));
            AssertBitwiseEqual(32767.0f, NumberBaseHelper<float>.CreateSaturating<char>((char)0x7FFF));
            AssertBitwiseEqual(32768.0f, NumberBaseHelper<float>.CreateSaturating<char>((char)0x8000));
            AssertBitwiseEqual(65535.0f, NumberBaseHelper<float>.CreateSaturating<char>((char)0xFFFF));
        }

        [Fact]
        public static void CreateSaturatingFromInt16Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateSaturating<short>(0x0000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateSaturating<short>(0x0001));
            AssertBitwiseEqual(32767.0f, NumberBaseHelper<float>.CreateSaturating<short>(0x7FFF));
            AssertBitwiseEqual(-32768.0f, NumberBaseHelper<float>.CreateSaturating<short>(unchecked((short)0x8000)));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateSaturating<short>(unchecked((short)0xFFFF)));
        }

        [Fact]
        public static void CreateSaturatingFromInt32Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateSaturating<int>(0x00000000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateSaturating<int>(0x00000001));
            AssertBitwiseEqual(2147483647.0f, NumberBaseHelper<float>.CreateSaturating<int>(0x7FFFFFFF));
            AssertBitwiseEqual(-2147483648.0f, NumberBaseHelper<float>.CreateSaturating<int>(unchecked((int)0x80000000)));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateSaturating<int>(unchecked((int)0xFFFFFFFF)));
        }

        [Fact]
        public static void CreateSaturatingFromInt64Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateSaturating<long>(0x0000000000000000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateSaturating<long>(0x0000000000000001));
            AssertBitwiseEqual(9223372036854775807.0f, NumberBaseHelper<float>.CreateSaturating<long>(0x7FFFFFFFFFFFFFFF));
            AssertBitwiseEqual(-9223372036854775808.0f, NumberBaseHelper<float>.CreateSaturating<long>(unchecked(unchecked((long)0x8000000000000000))));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateSaturating<long>(unchecked(unchecked((long)0xFFFFFFFFFFFFFFFF))));
        }

        [Fact]
        public static void CreateSaturatingFromInt128Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateSaturating<Int128>(new Int128(0x0000_0000_0000_0000, 0x0000_0000_0000_0000)));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateSaturating<Int128>(new Int128(0x0000_0000_0000_0000, 0x0000_0000_0000_0001)));
            AssertBitwiseEqual(170141183460469231731687303715884105727.0f, NumberBaseHelper<float>.CreateSaturating<Int128>(new Int128(0x7FFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF)));
            AssertBitwiseEqual(-170141183460469231731687303715884105728.0f, NumberBaseHelper<float>.CreateSaturating<Int128>(new Int128(0x8000_0000_0000_0000, 0x0000_0000_0000_0000)));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateSaturating<Int128>(new Int128(0xFFFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF)));
        }

        [Fact]
        public static void CreateSaturatingFromIntPtrTest()
        {
            if (Environment.Is64BitProcess)
            {
                AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateSaturating<nint>(unchecked((nint)0x0000000000000000)));
                AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateSaturating<nint>(unchecked((nint)0x0000000000000001)));
                AssertBitwiseEqual(9223372036854775807.0f, NumberBaseHelper<float>.CreateSaturating<nint>(unchecked((nint)0x7FFFFFFFFFFFFFFF)));
                AssertBitwiseEqual(-9223372036854775808.0f, NumberBaseHelper<float>.CreateSaturating<nint>(unchecked((nint)0x8000000000000000)));
                AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateSaturating<nint>(unchecked((nint)0xFFFFFFFFFFFFFFFF)));
            }
            else
            {
                AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateSaturating<nint>((nint)0x00000000));
                AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateSaturating<nint>((nint)0x00000001));
                AssertBitwiseEqual(2147483647.0f, NumberBaseHelper<float>.CreateSaturating<nint>((nint)0x7FFFFFFF));
                AssertBitwiseEqual(-2147483648.0f, NumberBaseHelper<float>.CreateSaturating<nint>(unchecked((nint)0x80000000)));
                AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateSaturating<nint>(unchecked((nint)0xFFFFFFFF)));
            }
        }

        [Fact]
        public static void CreateSaturatingFromSByteTest()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateSaturating<sbyte>(0x00));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateSaturating<sbyte>(0x01));
            AssertBitwiseEqual(127.0f, NumberBaseHelper<float>.CreateSaturating<sbyte>(0x7F));
            AssertBitwiseEqual(-128.0f, NumberBaseHelper<float>.CreateSaturating<sbyte>(unchecked((sbyte)0x80)));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateSaturating<sbyte>(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void CreateSaturatingFromUInt16Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateSaturating<ushort>(0x0000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateSaturating<ushort>(0x0001));
            AssertBitwiseEqual(32767.0f, NumberBaseHelper<float>.CreateSaturating<ushort>(0x7FFF));
            AssertBitwiseEqual(32768.0f, NumberBaseHelper<float>.CreateSaturating<ushort>(0x8000));
            AssertBitwiseEqual(65535.0f, NumberBaseHelper<float>.CreateSaturating<ushort>(0xFFFF));
        }

        [Fact]
        public static void CreateSaturatingFromUInt32Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateSaturating<uint>(0x00000000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateSaturating<uint>(0x00000001));
            AssertBitwiseEqual(2147483647.0f, NumberBaseHelper<float>.CreateSaturating<uint>(0x7FFFFFFF));
            AssertBitwiseEqual(2147483648.0f, NumberBaseHelper<float>.CreateSaturating<uint>(0x80000000));
            AssertBitwiseEqual(4294967295.0f, NumberBaseHelper<float>.CreateSaturating<uint>(0xFFFFFFFF));
        }

        [Fact]
        public static void CreateSaturatingFromUInt64Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateSaturating<ulong>(0x0000000000000000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateSaturating<ulong>(0x0000000000000001));
            AssertBitwiseEqual(9223372036854775807.0f, NumberBaseHelper<float>.CreateSaturating<ulong>(0x7FFFFFFFFFFFFFFF));
            AssertBitwiseEqual(9223372036854775808.0f, NumberBaseHelper<float>.CreateSaturating<ulong>(0x8000000000000000));
            AssertBitwiseEqual(18446744073709551615.0f, NumberBaseHelper<float>.CreateSaturating<ulong>(0xFFFFFFFFFFFFFFFF));
        }

        [Fact]
        public static void CreateSaturatingFromUInt128Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateSaturating<UInt128>(new UInt128(0x0000_0000_0000_0000, 0x0000_0000_0000_0000)));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateSaturating<UInt128>(new UInt128(0x0000_0000_0000_0000, 0x0000_0000_0000_0001)));
            AssertBitwiseEqual(170141183460469231731687303715884105727.0f, NumberBaseHelper<float>.CreateSaturating<UInt128>(new UInt128(0x7FFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF)));
            AssertBitwiseEqual(170141183460469231731687303715884105728.0f, NumberBaseHelper<float>.CreateSaturating<UInt128>(new UInt128(0x8000_0000_0000_0000, 0x0000_0000_0000_0000)));
            AssertBitwiseEqual(float.PositiveInfinity, NumberBaseHelper<float>.CreateSaturating<UInt128>(new UInt128(0xFFFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF)));
        }

        [Fact]
        public static void CreateSaturatingFromUIntPtrTest()
        {
            if (Environment.Is64BitProcess)
            {
                AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateSaturating<nuint>(unchecked((nuint)0x0000000000000000)));
                AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateSaturating<nuint>(unchecked((nuint)0x0000000000000001)));
                AssertBitwiseEqual(9223372036854775807.0f, NumberBaseHelper<float>.CreateSaturating<nuint>(unchecked((nuint)0x7FFFFFFFFFFFFFFF)));

                // https://github.com/dotnet/roslyn/issues/60714
                // AssertBitwiseEqual(9223372036854775808.0f, NumberBaseHelper<float>.CreateSaturating<nuint>(unchecked((nuint)0x8000000000000000)));
                // AssertBitwiseEqual(18446744073709551615.0f, NumberBaseHelper<float>.CreateSaturating<nuint>(unchecked((nuint)0xFFFFFFFFFFFFFFFF)));
            }
            else
            {
                AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateSaturating<nuint>((nuint)0x00000000));
                AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateSaturating<nuint>((nuint)0x00000001));
                AssertBitwiseEqual(2147483647.0f, NumberBaseHelper<float>.CreateSaturating<nuint>((nuint)0x7FFFFFFF));

                // https://github.com/dotnet/roslyn/issues/60714
                // AssertBitwiseEqual(2147483648.0f, NumberBaseHelper<float>.CreateSaturating<nuint>((nuint)0x80000000));
                // AssertBitwiseEqual(4294967295.0f, NumberBaseHelper<float>.CreateSaturating<nuint>((nuint)0xFFFFFFFF));
            }
        }

        [Fact]
        public static void CreateTruncatingFromByteTest()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateTruncating<byte>(0x00));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateTruncating<byte>(0x01));
            AssertBitwiseEqual(127.0f, NumberBaseHelper<float>.CreateTruncating<byte>(0x7F));
            AssertBitwiseEqual(128.0f, NumberBaseHelper<float>.CreateTruncating<byte>(0x80));
            AssertBitwiseEqual(255.0f, NumberBaseHelper<float>.CreateTruncating<byte>(0xFF));
        }

        [Fact]
        public static void CreateTruncatingFromCharTest()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateTruncating<char>((char)0x0000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateTruncating<char>((char)0x0001));
            AssertBitwiseEqual(32767.0f, NumberBaseHelper<float>.CreateTruncating<char>((char)0x7FFF));
            AssertBitwiseEqual(32768.0f, NumberBaseHelper<float>.CreateTruncating<char>((char)0x8000));
            AssertBitwiseEqual(65535.0f, NumberBaseHelper<float>.CreateTruncating<char>((char)0xFFFF));
        }

        [Fact]
        public static void CreateTruncatingFromInt16Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateTruncating<short>(0x0000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateTruncating<short>(0x0001));
            AssertBitwiseEqual(32767.0f, NumberBaseHelper<float>.CreateTruncating<short>(0x7FFF));
            AssertBitwiseEqual(-32768.0f, NumberBaseHelper<float>.CreateTruncating<short>(unchecked((short)0x8000)));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateTruncating<short>(unchecked((short)0xFFFF)));
        }

        [Fact]
        public static void CreateTruncatingFromInt32Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateTruncating<int>(0x00000000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateTruncating<int>(0x00000001));
            AssertBitwiseEqual(2147483647.0f, NumberBaseHelper<float>.CreateTruncating<int>(0x7FFFFFFF));
            AssertBitwiseEqual(-2147483648.0f, NumberBaseHelper<float>.CreateTruncating<int>(unchecked((int)0x80000000)));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateTruncating<int>(unchecked((int)0xFFFFFFFF)));
        }

        [Fact]
        public static void CreateTruncatingFromInt64Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateTruncating<long>(0x0000000000000000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateTruncating<long>(0x0000000000000001));
            AssertBitwiseEqual(9223372036854775807.0f, NumberBaseHelper<float>.CreateTruncating<long>(0x7FFFFFFFFFFFFFFF));
            AssertBitwiseEqual(-9223372036854775808.0f, NumberBaseHelper<float>.CreateTruncating<long>(unchecked(unchecked((long)0x8000000000000000))));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateTruncating<long>(unchecked(unchecked((long)0xFFFFFFFFFFFFFFFF))));
        }

        [Fact]
        public static void CreateTruncatingFromInt128Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateTruncating<Int128>(new Int128(0x0000_0000_0000_0000, 0x0000_0000_0000_0000)));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateTruncating<Int128>(new Int128(0x0000_0000_0000_0000, 0x0000_0000_0000_0001)));
            AssertBitwiseEqual(170141183460469231731687303715884105727.0f, NumberBaseHelper<float>.CreateTruncating<Int128>(new Int128(0x7FFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF)));
            AssertBitwiseEqual(-170141183460469231731687303715884105728.0f, NumberBaseHelper<float>.CreateTruncating<Int128>(new Int128(0x8000_0000_0000_0000, 0x0000_0000_0000_0000)));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateTruncating<Int128>(new Int128(0xFFFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF)));
        }

        [Fact]
        public static void CreateTruncatingFromIntPtrTest()
        {
            if (Environment.Is64BitProcess)
            {
                AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateTruncating<nint>(unchecked((nint)0x0000000000000000)));
                AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateTruncating<nint>(unchecked((nint)0x0000000000000001)));
                AssertBitwiseEqual(9223372036854775807.0f, NumberBaseHelper<float>.CreateTruncating<nint>(unchecked((nint)0x7FFFFFFFFFFFFFFF)));
                AssertBitwiseEqual(-9223372036854775808.0f, NumberBaseHelper<float>.CreateTruncating<nint>(unchecked((nint)0x8000000000000000)));
                AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateTruncating<nint>(unchecked((nint)0xFFFFFFFFFFFFFFFF)));
            }
            else
            {
                AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateTruncating<nint>((nint)0x00000000));
                AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateTruncating<nint>((nint)0x00000001));
                AssertBitwiseEqual(2147483647.0f, NumberBaseHelper<float>.CreateTruncating<nint>((nint)0x7FFFFFFF));
                AssertBitwiseEqual(-2147483648.0f, NumberBaseHelper<float>.CreateTruncating<nint>(unchecked((nint)0x80000000)));
                AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateTruncating<nint>(unchecked((nint)0xFFFFFFFF)));
            }
        }

        [Fact]
        public static void CreateTruncatingFromSByteTest()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateTruncating<sbyte>(0x00));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateTruncating<sbyte>(0x01));
            AssertBitwiseEqual(127.0f, NumberBaseHelper<float>.CreateTruncating<sbyte>(0x7F));
            AssertBitwiseEqual(-128.0f, NumberBaseHelper<float>.CreateTruncating<sbyte>(unchecked((sbyte)0x80)));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.CreateTruncating<sbyte>(unchecked((sbyte)0xFF)));
        }

        [Fact]
        public static void CreateTruncatingFromUInt16Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateTruncating<ushort>(0x0000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateTruncating<ushort>(0x0001));
            AssertBitwiseEqual(32767.0f, NumberBaseHelper<float>.CreateTruncating<ushort>(0x7FFF));
            AssertBitwiseEqual(32768.0f, NumberBaseHelper<float>.CreateTruncating<ushort>(0x8000));
            AssertBitwiseEqual(65535.0f, NumberBaseHelper<float>.CreateTruncating<ushort>(0xFFFF));
        }

        [Fact]
        public static void CreateTruncatingFromUInt32Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateTruncating<uint>(0x00000000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateTruncating<uint>(0x00000001));
            AssertBitwiseEqual(2147483647.0f, NumberBaseHelper<float>.CreateTruncating<uint>(0x7FFFFFFF));
            AssertBitwiseEqual(2147483648.0f, NumberBaseHelper<float>.CreateTruncating<uint>(0x80000000));
            AssertBitwiseEqual(4294967295.0f, NumberBaseHelper<float>.CreateTruncating<uint>(0xFFFFFFFF));
        }

        [Fact]
        public static void CreateTruncatingFromUInt64Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateTruncating<ulong>(0x0000000000000000));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateTruncating<ulong>(0x0000000000000001));
            AssertBitwiseEqual(9223372036854775807.0f, NumberBaseHelper<float>.CreateTruncating<ulong>(0x7FFFFFFFFFFFFFFF));
            AssertBitwiseEqual(9223372036854775808.0f, NumberBaseHelper<float>.CreateTruncating<ulong>(0x8000000000000000));
            AssertBitwiseEqual(18446744073709551615.0f, NumberBaseHelper<float>.CreateTruncating<ulong>(0xFFFFFFFFFFFFFFFF));
        }

        [Fact]
        public static void CreateTruncatingFromUInt128Test()
        {
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateTruncating<UInt128>(new UInt128(0x0000_0000_0000_0000, 0x0000_0000_0000_0000)));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateTruncating<UInt128>(new UInt128(0x0000_0000_0000_0000, 0x0000_0000_0000_0001)));
            AssertBitwiseEqual(170141183460469231731687303715884105727.0f, NumberBaseHelper<float>.CreateTruncating<UInt128>(new UInt128(0x7FFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF)));
            AssertBitwiseEqual(170141183460469231731687303715884105728.0f, NumberBaseHelper<float>.CreateTruncating<UInt128>(new UInt128(0x8000_0000_0000_0000, 0x0000_0000_0000_0000)));
            AssertBitwiseEqual(float.PositiveInfinity, NumberBaseHelper<float>.CreateTruncating<UInt128>(new UInt128(0xFFFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF)));
        }

        [Fact]
        public static void CreateTruncatingFromUIntPtrTest()
        {
            if (Environment.Is64BitProcess)
            {
                AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateTruncating<nuint>(unchecked((nuint)0x0000000000000000)));
                AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateTruncating<nuint>(unchecked((nuint)0x0000000000000001)));
                AssertBitwiseEqual(9223372036854775807.0f, NumberBaseHelper<float>.CreateTruncating<nuint>(unchecked((nuint)0x7FFFFFFFFFFFFFFF)));

                // https://github.com/dotnet/roslyn/issues/60714
                // AssertBitwiseEqual(9223372036854775808.0f, NumberBaseHelper<float>.CreateTruncating<nuint>(unchecked((nuint)0x8000000000000000)));
                // AssertBitwiseEqual(18446744073709551615.0f, NumberBaseHelper<float>.CreateTruncating<nuint>(unchecked((nuint)0xFFFFFFFFFFFFFFFF)));
            }
            else
            {
                AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.CreateTruncating<nuint>((nuint)0x00000000));
                AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.CreateTruncating<nuint>((nuint)0x00000001));
                AssertBitwiseEqual(2147483647.0f, NumberBaseHelper<float>.CreateTruncating<nuint>((nuint)0x7FFFFFFF));

                // https://github.com/dotnet/roslyn/issues/60714
                // AssertBitwiseEqual(2147483648.0f, NumberBaseHelper<float>.CreateTruncating<nuint>((nuint)0x80000000));
                // AssertBitwiseEqual(4294967295.0f, NumberBaseHelper<float>.CreateTruncating<nuint>((nuint)0xFFFFFFFF));
            }
        }

        [Fact]
        public static void IsFiniteTest()
        {
            Assert.False(NumberBaseHelper<float>.IsFinite(float.NegativeInfinity));
            Assert.True(NumberBaseHelper<float>.IsFinite(float.MinValue));
            Assert.True(NumberBaseHelper<float>.IsFinite(-1.0f));
            Assert.True(NumberBaseHelper<float>.IsFinite(-MinNormal));
            Assert.True(NumberBaseHelper<float>.IsFinite(-MaxSubnormal));
            Assert.True(NumberBaseHelper<float>.IsFinite(-float.Epsilon));
            Assert.True(NumberBaseHelper<float>.IsFinite(-0.0f));
            Assert.False(NumberBaseHelper<float>.IsFinite(float.NaN));
            Assert.True(NumberBaseHelper<float>.IsFinite(0.0f));
            Assert.True(NumberBaseHelper<float>.IsFinite(float.Epsilon));
            Assert.True(NumberBaseHelper<float>.IsFinite(MaxSubnormal));
            Assert.True(NumberBaseHelper<float>.IsFinite(MinNormal));
            Assert.True(NumberBaseHelper<float>.IsFinite(1.0f));
            Assert.True(NumberBaseHelper<float>.IsFinite(float.MaxValue));
            Assert.False(NumberBaseHelper<float>.IsFinite(float.PositiveInfinity));
        }

        [Fact]
        public static void IsInfinityTest()
        {
            Assert.True(NumberBaseHelper<float>.IsInfinity(float.NegativeInfinity));
            Assert.False(NumberBaseHelper<float>.IsInfinity(float.MinValue));
            Assert.False(NumberBaseHelper<float>.IsInfinity(-1.0f));
            Assert.False(NumberBaseHelper<float>.IsInfinity(-MinNormal));
            Assert.False(NumberBaseHelper<float>.IsInfinity(-MaxSubnormal));
            Assert.False(NumberBaseHelper<float>.IsInfinity(-float.Epsilon));
            Assert.False(NumberBaseHelper<float>.IsInfinity(-0.0f));
            Assert.False(NumberBaseHelper<float>.IsInfinity(float.NaN));
            Assert.False(NumberBaseHelper<float>.IsInfinity(0.0f));
            Assert.False(NumberBaseHelper<float>.IsInfinity(float.Epsilon));
            Assert.False(NumberBaseHelper<float>.IsInfinity(MaxSubnormal));
            Assert.False(NumberBaseHelper<float>.IsInfinity(MinNormal));
            Assert.False(NumberBaseHelper<float>.IsInfinity(1.0f));
            Assert.False(NumberBaseHelper<float>.IsInfinity(float.MaxValue));
            Assert.True(NumberBaseHelper<float>.IsInfinity(float.PositiveInfinity));
        }

        [Fact]
        public static void IsNaNTest()
        {
            Assert.False(NumberBaseHelper<float>.IsNaN(float.NegativeInfinity));
            Assert.False(NumberBaseHelper<float>.IsNaN(float.MinValue));
            Assert.False(NumberBaseHelper<float>.IsNaN(-1.0f));
            Assert.False(NumberBaseHelper<float>.IsNaN(-MinNormal));
            Assert.False(NumberBaseHelper<float>.IsNaN(-MaxSubnormal));
            Assert.False(NumberBaseHelper<float>.IsNaN(-float.Epsilon));
            Assert.False(NumberBaseHelper<float>.IsNaN(-0.0f));
            Assert.True(NumberBaseHelper<float>.IsNaN(float.NaN));
            Assert.False(NumberBaseHelper<float>.IsNaN(0.0f));
            Assert.False(NumberBaseHelper<float>.IsNaN(float.Epsilon));
            Assert.False(NumberBaseHelper<float>.IsNaN(MaxSubnormal));
            Assert.False(NumberBaseHelper<float>.IsNaN(MinNormal));
            Assert.False(NumberBaseHelper<float>.IsNaN(1.0f));
            Assert.False(NumberBaseHelper<float>.IsNaN(float.MaxValue));
            Assert.False(NumberBaseHelper<float>.IsNaN(float.PositiveInfinity));
        }

        [Fact]
        public static void IsNegativeTest()
        {
            Assert.True(NumberBaseHelper<float>.IsNegative(float.NegativeInfinity));
            Assert.True(NumberBaseHelper<float>.IsNegative(float.MinValue));
            Assert.True(NumberBaseHelper<float>.IsNegative(-1.0f));
            Assert.True(NumberBaseHelper<float>.IsNegative(-MinNormal));
            Assert.True(NumberBaseHelper<float>.IsNegative(-MaxSubnormal));
            Assert.True(NumberBaseHelper<float>.IsNegative(-float.Epsilon));
            Assert.True(NumberBaseHelper<float>.IsNegative(-0.0f));
            Assert.True(NumberBaseHelper<float>.IsNegative(float.NaN));
            Assert.False(NumberBaseHelper<float>.IsNegative(0.0f));
            Assert.False(NumberBaseHelper<float>.IsNegative(float.Epsilon));
            Assert.False(NumberBaseHelper<float>.IsNegative(MaxSubnormal));
            Assert.False(NumberBaseHelper<float>.IsNegative(MinNormal));
            Assert.False(NumberBaseHelper<float>.IsNegative(1.0f));
            Assert.False(NumberBaseHelper<float>.IsNegative(float.MaxValue));
            Assert.False(NumberBaseHelper<float>.IsNegative(float.PositiveInfinity));
        }

        [Fact]
        public static void IsNegativeInfinityTest()
        {
            Assert.True(NumberBaseHelper<float>.IsNegativeInfinity(float.NegativeInfinity));
            Assert.False(NumberBaseHelper<float>.IsNegativeInfinity(float.MinValue));
            Assert.False(NumberBaseHelper<float>.IsNegativeInfinity(-1.0f));
            Assert.False(NumberBaseHelper<float>.IsNegativeInfinity(-MinNormal));
            Assert.False(NumberBaseHelper<float>.IsNegativeInfinity(-MaxSubnormal));
            Assert.False(NumberBaseHelper<float>.IsNegativeInfinity(-float.Epsilon));
            Assert.False(NumberBaseHelper<float>.IsNegativeInfinity(-0.0f));
            Assert.False(NumberBaseHelper<float>.IsNegativeInfinity(float.NaN));
            Assert.False(NumberBaseHelper<float>.IsNegativeInfinity(0.0f));
            Assert.False(NumberBaseHelper<float>.IsNegativeInfinity(float.Epsilon));
            Assert.False(NumberBaseHelper<float>.IsNegativeInfinity(MaxSubnormal));
            Assert.False(NumberBaseHelper<float>.IsNegativeInfinity(MinNormal));
            Assert.False(NumberBaseHelper<float>.IsNegativeInfinity(1.0f));
            Assert.False(NumberBaseHelper<float>.IsNegativeInfinity(float.MaxValue));
            Assert.False(NumberBaseHelper<float>.IsNegativeInfinity(float.PositiveInfinity));
        }

        [Fact]
        public static void IsNormalTest()
        {
            Assert.False(NumberBaseHelper<float>.IsNormal(float.NegativeInfinity));
            Assert.True(NumberBaseHelper<float>.IsNormal(float.MinValue));
            Assert.True(NumberBaseHelper<float>.IsNormal(-1.0f));
            Assert.True(NumberBaseHelper<float>.IsNormal(-MinNormal));
            Assert.False(NumberBaseHelper<float>.IsNormal(-MaxSubnormal));
            Assert.False(NumberBaseHelper<float>.IsNormal(-float.Epsilon));
            Assert.False(NumberBaseHelper<float>.IsNormal(-0.0f));
            Assert.False(NumberBaseHelper<float>.IsNormal(float.NaN));
            Assert.False(NumberBaseHelper<float>.IsNormal(0.0f));
            Assert.False(NumberBaseHelper<float>.IsNormal(float.Epsilon));
            Assert.False(NumberBaseHelper<float>.IsNormal(MaxSubnormal));
            Assert.True(NumberBaseHelper<float>.IsNormal(MinNormal));
            Assert.True(NumberBaseHelper<float>.IsNormal(1.0f));
            Assert.True(NumberBaseHelper<float>.IsNormal(float.MaxValue));
            Assert.False(NumberBaseHelper<float>.IsNormal(float.PositiveInfinity));
        }

        [Fact]
        public static void IsPositiveInfinityTest()
        {
            Assert.False(NumberBaseHelper<float>.IsPositiveInfinity(float.NegativeInfinity));
            Assert.False(NumberBaseHelper<float>.IsPositiveInfinity(float.MinValue));
            Assert.False(NumberBaseHelper<float>.IsPositiveInfinity(-1.0f));
            Assert.False(NumberBaseHelper<float>.IsPositiveInfinity(-MinNormal));
            Assert.False(NumberBaseHelper<float>.IsPositiveInfinity(-MaxSubnormal));
            Assert.False(NumberBaseHelper<float>.IsPositiveInfinity(-float.Epsilon));
            Assert.False(NumberBaseHelper<float>.IsPositiveInfinity(-0.0f));
            Assert.False(NumberBaseHelper<float>.IsPositiveInfinity(float.NaN));
            Assert.False(NumberBaseHelper<float>.IsPositiveInfinity(0.0f));
            Assert.False(NumberBaseHelper<float>.IsPositiveInfinity(float.Epsilon));
            Assert.False(NumberBaseHelper<float>.IsPositiveInfinity(MaxSubnormal));
            Assert.False(NumberBaseHelper<float>.IsPositiveInfinity(MinNormal));
            Assert.False(NumberBaseHelper<float>.IsPositiveInfinity(1.0f));
            Assert.False(NumberBaseHelper<float>.IsPositiveInfinity(float.MaxValue));
            Assert.True(NumberBaseHelper<float>.IsPositiveInfinity(float.PositiveInfinity));
        }

        [Fact]
        public static void IsSubnormalTest()
        {
            Assert.False(NumberBaseHelper<float>.IsSubnormal(float.NegativeInfinity));
            Assert.False(NumberBaseHelper<float>.IsSubnormal(float.MinValue));
            Assert.False(NumberBaseHelper<float>.IsSubnormal(-1.0f));
            Assert.False(NumberBaseHelper<float>.IsSubnormal(-MinNormal));
            Assert.True(NumberBaseHelper<float>.IsSubnormal(-MaxSubnormal));
            Assert.True(NumberBaseHelper<float>.IsSubnormal(-float.Epsilon));
            Assert.False(NumberBaseHelper<float>.IsSubnormal(-0.0f));
            Assert.False(NumberBaseHelper<float>.IsSubnormal(float.NaN));
            Assert.False(NumberBaseHelper<float>.IsSubnormal(0.0f));
            Assert.True(NumberBaseHelper<float>.IsSubnormal(float.Epsilon));
            Assert.True(NumberBaseHelper<float>.IsSubnormal(MaxSubnormal));
            Assert.False(NumberBaseHelper<float>.IsSubnormal(MinNormal));
            Assert.False(NumberBaseHelper<float>.IsSubnormal(1.0f));
            Assert.False(NumberBaseHelper<float>.IsSubnormal(float.MaxValue));
            Assert.False(NumberBaseHelper<float>.IsSubnormal(float.PositiveInfinity));
        }

        [Fact]
        public static void MaxMagnitudeTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, NumberBaseHelper<float>.MaxMagnitude(float.NegativeInfinity, 1.0f));
            AssertBitwiseEqual(float.MinValue, NumberBaseHelper<float>.MaxMagnitude(float.MinValue, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitude(-1.0f, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitude(-MinNormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitude(-MaxSubnormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitude(-float.Epsilon, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitude(-0.0f, 1.0f));
            AssertBitwiseEqual(float.NaN, NumberBaseHelper<float>.MaxMagnitude(float.NaN, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitude(0.0f, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitude(float.Epsilon, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitude(MaxSubnormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitude(MinNormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitude(1.0f, 1.0f));
            AssertBitwiseEqual(float.MaxValue, NumberBaseHelper<float>.MaxMagnitude(float.MaxValue, 1.0f));
            AssertBitwiseEqual(float.PositiveInfinity, NumberBaseHelper<float>.MaxMagnitude(float.PositiveInfinity, 1.0f));
        }

        [Fact]
        public static void MaxMagnitudeNumberTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, NumberBaseHelper<float>.MaxMagnitudeNumber(float.NegativeInfinity, 1.0f));
            AssertBitwiseEqual(float.MinValue, NumberBaseHelper<float>.MaxMagnitudeNumber(float.MinValue, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitudeNumber(-1.0f, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitudeNumber(-MinNormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitudeNumber(-MaxSubnormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitudeNumber(-float.Epsilon, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitudeNumber(-0.0f, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitudeNumber(float.NaN, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitudeNumber(0.0f, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitudeNumber(float.Epsilon, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitudeNumber(MaxSubnormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitudeNumber(MinNormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MaxMagnitudeNumber(1.0f, 1.0f));
            AssertBitwiseEqual(float.MaxValue, NumberBaseHelper<float>.MaxMagnitudeNumber(float.MaxValue, 1.0f));
            AssertBitwiseEqual(float.PositiveInfinity, NumberBaseHelper<float>.MaxMagnitudeNumber(float.PositiveInfinity, 1.0f));
        }

        [Fact]
        public static void MinMagnitudeTest()
        {
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MinMagnitude(float.NegativeInfinity, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MinMagnitude(float.MinValue, 1.0f));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.MinMagnitude(-1.0f, 1.0f));
            AssertBitwiseEqual(-MinNormal, NumberBaseHelper<float>.MinMagnitude(-MinNormal, 1.0f));
            AssertBitwiseEqual(-MaxSubnormal, NumberBaseHelper<float>.MinMagnitude(-MaxSubnormal, 1.0f));
            AssertBitwiseEqual(-float.Epsilon, NumberBaseHelper<float>.MinMagnitude(-float.Epsilon, 1.0f));
            AssertBitwiseEqual(-0.0f, NumberBaseHelper<float>.MinMagnitude(-0.0f, 1.0f));
            AssertBitwiseEqual(float.NaN, NumberBaseHelper<float>.MinMagnitude(float.NaN, 1.0f));
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.MinMagnitude(0.0f, 1.0f));
            AssertBitwiseEqual(float.Epsilon, NumberBaseHelper<float>.MinMagnitude(float.Epsilon, 1.0f));
            AssertBitwiseEqual(MaxSubnormal, NumberBaseHelper<float>.MinMagnitude(MaxSubnormal, 1.0f));
            AssertBitwiseEqual(MinNormal, NumberBaseHelper<float>.MinMagnitude(MinNormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MinMagnitude(1.0f, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MinMagnitude(float.MaxValue, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MinMagnitude(float.PositiveInfinity, 1.0f));
        }

        [Fact]
        public static void MinMagnitudeNumberTest()
        {
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MinMagnitudeNumber(float.NegativeInfinity, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MinMagnitudeNumber(float.MinValue, 1.0f));
            AssertBitwiseEqual(-1.0f, NumberBaseHelper<float>.MinMagnitudeNumber(-1.0f, 1.0f));
            AssertBitwiseEqual(-MinNormal, NumberBaseHelper<float>.MinMagnitudeNumber(-MinNormal, 1.0f));
            AssertBitwiseEqual(-MaxSubnormal, NumberBaseHelper<float>.MinMagnitudeNumber(-MaxSubnormal, 1.0f));
            AssertBitwiseEqual(-float.Epsilon, NumberBaseHelper<float>.MinMagnitudeNumber(-float.Epsilon, 1.0f));
            AssertBitwiseEqual(-0.0f, NumberBaseHelper<float>.MinMagnitudeNumber(-0.0f, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MinMagnitudeNumber(float.NaN, 1.0f));
            AssertBitwiseEqual(0.0f, NumberBaseHelper<float>.MinMagnitudeNumber(0.0f, 1.0f));
            AssertBitwiseEqual(float.Epsilon, NumberBaseHelper<float>.MinMagnitudeNumber(float.Epsilon, 1.0f));
            AssertBitwiseEqual(MaxSubnormal, NumberBaseHelper<float>.MinMagnitudeNumber(MaxSubnormal, 1.0f));
            AssertBitwiseEqual(MinNormal, NumberBaseHelper<float>.MinMagnitudeNumber(MinNormal, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MinMagnitudeNumber(1.0f, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MinMagnitudeNumber(float.MaxValue, 1.0f));
            AssertBitwiseEqual(1.0f, NumberBaseHelper<float>.MinMagnitudeNumber(float.PositiveInfinity, 1.0f));
        }

        [Fact]
        public static void TryCreateFromByteTest()
        {
            float result;

            Assert.True(NumberBaseHelper<float>.TryCreate<byte>(0x00, out result));
            Assert.Equal(0.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<byte>(0x01, out result));
            Assert.Equal(1.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<byte>(0x7F, out result));
            Assert.Equal(127.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<byte>(0x80, out result));
            Assert.Equal(128.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<byte>(0xFF, out result));
            Assert.Equal(255.0f, result);
        }

        [Fact]
        public static void TryCreateFromCharTest()
        {
            float result;

            Assert.True(NumberBaseHelper<float>.TryCreate<char>((char)0x0000, out result));
            Assert.Equal(0.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<char>((char)0x0001, out result));
            Assert.Equal(1.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<char>((char)0x7FFF, out result));
            Assert.Equal(32767.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<char>((char)0x8000, out result));
            Assert.Equal(32768.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<char>((char)0xFFFF, out result));
            Assert.Equal(65535.0f, result);
        }

        [Fact]
        public static void TryCreateFromInt16Test()
        {
            float result;

            Assert.True(NumberBaseHelper<float>.TryCreate<short>(0x0000, out result));
            Assert.Equal(0.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<short>(0x0001, out result));
            Assert.Equal(1.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<short>(0x7FFF, out result));
            Assert.Equal(32767.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<short>(unchecked((short)0x8000), out result));
            Assert.Equal(-32768.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<short>(unchecked((short)0xFFFF), out result));
            Assert.Equal(-1.0f, result);
        }

        [Fact]
        public static void TryCreateFromInt32Test()
        {
            float result;

            Assert.True(NumberBaseHelper<float>.TryCreate<int>(0x00000000, out result));
            Assert.Equal(0.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<int>(0x00000001, out result));
            Assert.Equal(1.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<int>(0x7FFFFFFF, out result));
            Assert.Equal(2147483647.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<int>(unchecked((int)0x80000000), out result));
            Assert.Equal(-2147483648.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<int>(unchecked((int)0xFFFFFFFF), out result));
            Assert.Equal(-1.0f, result);
        }

        [Fact]
        public static void TryCreateFromInt64Test()
        {
            float result;

            Assert.True(NumberBaseHelper<float>.TryCreate<long>(0x0000000000000000, out result));
            Assert.Equal(0.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<long>(0x0000000000000001, out result));
            Assert.Equal(1.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<long>(0x7FFFFFFFFFFFFFFF, out result));
            Assert.Equal(9223372036854775807.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<long>(unchecked(unchecked((long)0x8000000000000000)), out result));
            Assert.Equal(-9223372036854775808.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<long>(unchecked(unchecked((long)0xFFFFFFFFFFFFFFFF)), out result));
            Assert.Equal(-1.0f, result);
        }

        [Fact]
        public static void TryCreateFromInt128Test()
        {
            float result;

            Assert.True(NumberBaseHelper<float>.TryCreate<Int128>(new Int128(0x0000_0000_0000_0000, 0x0000_0000_0000_0000), out result));
            Assert.Equal(0.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<Int128>(new Int128(0x0000_0000_0000_0000, 0x0000_0000_0000_0001), out result));
            Assert.Equal(1.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<Int128>(new Int128(0x7FFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF), out result));
            Assert.Equal(170141183460469231731687303715884105727.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<Int128>(new Int128(0x8000_0000_0000_0000, 0x0000_0000_0000_0000), out result));
            Assert.Equal(-170141183460469231731687303715884105728.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<Int128>(new Int128(0xFFFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF), out result));
            Assert.Equal(-1.0f, result);
        }

        [Fact]
        public static void TryCreateFromIntPtrTest()
        {
            float result;

            if (Environment.Is64BitProcess)
            {
                Assert.True(NumberBaseHelper<float>.TryCreate<nint>(unchecked((nint)0x0000000000000000), out result));
                Assert.Equal(0.0f, result);

                Assert.True(NumberBaseHelper<float>.TryCreate<nint>(unchecked((nint)0x0000000000000001), out result));
                Assert.Equal(1.0f, result);

                Assert.True(NumberBaseHelper<float>.TryCreate<nint>(unchecked((nint)0x7FFFFFFFFFFFFFFF), out result));
                Assert.Equal(9223372036854775807.0f, result);

                Assert.True(NumberBaseHelper<float>.TryCreate<nint>(unchecked((nint)0x8000000000000000), out result));
                Assert.Equal(-9223372036854775808.0f, result);

                Assert.True(NumberBaseHelper<float>.TryCreate<nint>(unchecked((nint)0xFFFFFFFFFFFFFFFF), out result));
                Assert.Equal(-1.0f, result);
            }
            else
            {
                Assert.True(NumberBaseHelper<float>.TryCreate<nint>((nint)0x00000000, out result));
                Assert.Equal(0.0f, result);

                Assert.True(NumberBaseHelper<float>.TryCreate<nint>((nint)0x00000001, out result));
                Assert.Equal(1.0f, result);

                Assert.True(NumberBaseHelper<float>.TryCreate<nint>((nint)0x7FFFFFFF, out result));
                Assert.Equal(2147483647.0f, result);

                Assert.True(NumberBaseHelper<float>.TryCreate<nint>(unchecked((nint)0x80000000), out result));
                Assert.Equal(-2147483648.0f, result);

                Assert.True(NumberBaseHelper<float>.TryCreate<nint>(unchecked((nint)0xFFFFFFFF), out result));
                Assert.Equal(-1.0f, result);
            }
        }

        [Fact]
        public static void TryCreateFromSByteTest()
        {
            float result;

            Assert.True(NumberBaseHelper<float>.TryCreate<sbyte>(0x00, out result));
            Assert.Equal(0.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<sbyte>(0x01, out result));
            Assert.Equal(1.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<sbyte>(0x7F, out result));
            Assert.Equal(127.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<sbyte>(unchecked((sbyte)0x80), out result));
            Assert.Equal(-128.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<sbyte>(unchecked((sbyte)0xFF), out result));
            Assert.Equal(-1.0f, result);
        }

        [Fact]
        public static void TryCreateFromUInt16Test()
        {
            float result;

            Assert.True(NumberBaseHelper<float>.TryCreate<ushort>(0x0000, out result));
            Assert.Equal(0.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<ushort>(0x0001, out result));
            Assert.Equal(1.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<ushort>(0x7FFF, out result));
            Assert.Equal(32767.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<ushort>(0x8000, out result));
            Assert.Equal(32768.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<ushort>(0xFFFF, out result));
            Assert.Equal(65535.0f, result);
        }

        [Fact]
        public static void TryCreateFromUInt32Test()
        {
            float result;

            Assert.True(NumberBaseHelper<float>.TryCreate<uint>(0x00000000, out result));
            Assert.Equal(0.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<uint>(0x00000001, out result));
            Assert.Equal(1.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<uint>(0x7FFFFFFF, out result));
            Assert.Equal(2147483647.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<uint>(0x80000000, out result));
            Assert.Equal(2147483648.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<uint>(0xFFFFFFFF, out result));
            Assert.Equal(4294967295.0f, result);
        }

        [Fact]
        public static void TryCreateFromUInt64Test()
        {
            float result;

            Assert.True(NumberBaseHelper<float>.TryCreate<ulong>(0x0000000000000000, out result));
            Assert.Equal(0.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<ulong>(0x0000000000000001, out result));
            Assert.Equal(1.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<ulong>(0x7FFFFFFFFFFFFFFF, out result));
            Assert.Equal(9223372036854775807.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<ulong>(0x8000000000000000, out result));
            Assert.Equal(9223372036854775808.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<ulong>(0xFFFFFFFFFFFFFFFF, out result));
            Assert.Equal(18446744073709551615.0f, result);
        }

        [Fact]
        public static void TryCreateFromUInt128Test()
        {
            float result;

            Assert.True(NumberBaseHelper<float>.TryCreate<UInt128>(new UInt128(0x0000_0000_0000_0000, 0x0000_0000_0000_0000), out result));
            Assert.Equal(0.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<UInt128>(new UInt128(0x0000_0000_0000_0000, 0x0000_0000_0000_0001), out result));
            Assert.Equal(1.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<UInt128>(new UInt128(0x7FFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF), out result));
            Assert.Equal(170141183460469231731687303715884105727.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<UInt128>(new UInt128(0x8000_0000_0000_0000, 0x0000_0000_0000_0000), out result));
            Assert.Equal(170141183460469231731687303715884105728.0f, result);

            Assert.True(NumberBaseHelper<float>.TryCreate<UInt128>(new UInt128(0xFFFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF), out result));
            Assert.Equal(float.PositiveInfinity, result);
        }

        [Fact]
        public static void TryCreateFromUIntPtrTest()
        {
            float result;

            if (Environment.Is64BitProcess)
            {
                Assert.True(NumberBaseHelper<float>.TryCreate<nuint>(unchecked((nuint)0x0000000000000000), out result));
                Assert.Equal(0.0f, result);

                Assert.True(NumberBaseHelper<float>.TryCreate<nuint>(unchecked((nuint)0x0000000000000001), out result));
                Assert.Equal(1.0f, result);

                Assert.True(NumberBaseHelper<float>.TryCreate<nuint>(unchecked((nuint)0x7FFFFFFFFFFFFFFF), out result));
                Assert.Equal(9223372036854775807.0f, result);

                // https://github.com/dotnet/roslyn/issues/60714
                // Assert.True(NumberBaseHelper<float>.TryCreate<nuint>(unchecked((nuint)0x8000000000000000), out result));
                // Assert.Equal(9223372036854775808.0f, result);
                //
                // Assert.True(NumberBaseHelper<float>.TryCreate<nuint>(unchecked((nuint)0xFFFFFFFFFFFFFFFF), out result));
                // Assert.Equal(18446744073709551615.0f, result);
            }
            else
            {
                Assert.True(NumberBaseHelper<float>.TryCreate<nuint>((nuint)0x00000000, out result));
                Assert.Equal(0.0f, result);

                Assert.True(NumberBaseHelper<float>.TryCreate<nuint>((nuint)0x00000001, out result));
                Assert.Equal(1.0f, result);

                Assert.True(NumberBaseHelper<float>.TryCreate<nuint>((nuint)0x7FFFFFFF, out result));
                Assert.Equal(2147483647.0f, result);

                // https://github.com/dotnet/roslyn/issues/60714
                // Assert.True(NumberBaseHelper<float>.TryCreate<nuint>(unchecked((nuint)0x80000000), out result));
                // Assert.Equal(2147483648.0f, result);
                //
                // Assert.True(NumberBaseHelper<float>.TryCreate<nuint>(unchecked((nuint)0xFFFFFFFF), out result));
                // Assert.Equal(4294967295.0f, result);
            }
        }

        //
        // ISignedNumber
        //

        [Fact]
        public static void NegativeOneTest()
        {
            Assert.Equal(-1.0f, SignedNumberHelper<float>.NegativeOne);
        }

        //
        // ISubtractionOperators
        //

        [Fact]
        public static void op_SubtractionTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, SubtractionOperatorsHelper<float, float, float>.op_Subtraction(float.NegativeInfinity, 1.0f));
            AssertBitwiseEqual(float.MinValue, SubtractionOperatorsHelper<float, float, float>.op_Subtraction(float.MinValue, 1.0f));
            AssertBitwiseEqual(-2.0f, SubtractionOperatorsHelper<float, float, float>.op_Subtraction(-1.0f, 1.0f));
            AssertBitwiseEqual(-1.0f, SubtractionOperatorsHelper<float, float, float>.op_Subtraction(-MinNormal, 1.0f));
            AssertBitwiseEqual(-1.0f, SubtractionOperatorsHelper<float, float, float>.op_Subtraction(-MaxSubnormal, 1.0f));
            AssertBitwiseEqual(-1.0f, SubtractionOperatorsHelper<float, float, float>.op_Subtraction(-float.Epsilon, 1.0f));
            AssertBitwiseEqual(-1.0f, SubtractionOperatorsHelper<float, float, float>.op_Subtraction(-0.0f, 1.0f));
            AssertBitwiseEqual(float.NaN, SubtractionOperatorsHelper<float, float, float>.op_Subtraction(float.NaN, 1.0f));
            AssertBitwiseEqual(-1.0f, SubtractionOperatorsHelper<float, float, float>.op_Subtraction(0.0f, 1.0f));
            AssertBitwiseEqual(-1.0f, SubtractionOperatorsHelper<float, float, float>.op_Subtraction(float.Epsilon, 1.0f));
            AssertBitwiseEqual(-1.0f, SubtractionOperatorsHelper<float, float, float>.op_Subtraction(MaxSubnormal, 1.0f));
            AssertBitwiseEqual(-1.0f, SubtractionOperatorsHelper<float, float, float>.op_Subtraction(MinNormal, 1.0f));
            AssertBitwiseEqual(0.0f, SubtractionOperatorsHelper<float, float, float>.op_Subtraction(1.0f, 1.0f));
            AssertBitwiseEqual(float.MaxValue, SubtractionOperatorsHelper<float, float, float>.op_Subtraction(float.MaxValue, 1.0f));
            AssertBitwiseEqual(float.PositiveInfinity, SubtractionOperatorsHelper<float, float, float>.op_Subtraction(float.PositiveInfinity, 1.0f));
        }

        [Fact]
        public static void op_CheckedSubtractionTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, SubtractionOperatorsHelper<float, float, float>.op_CheckedSubtraction(float.NegativeInfinity, 1.0f));
            AssertBitwiseEqual(float.MinValue, SubtractionOperatorsHelper<float, float, float>.op_CheckedSubtraction(float.MinValue, 1.0f));
            AssertBitwiseEqual(-2.0f, SubtractionOperatorsHelper<float, float, float>.op_CheckedSubtraction(-1.0f, 1.0f));
            AssertBitwiseEqual(-1.0f, SubtractionOperatorsHelper<float, float, float>.op_CheckedSubtraction(-MinNormal, 1.0f));
            AssertBitwiseEqual(-1.0f, SubtractionOperatorsHelper<float, float, float>.op_CheckedSubtraction(-MaxSubnormal, 1.0f));
            AssertBitwiseEqual(-1.0f, SubtractionOperatorsHelper<float, float, float>.op_CheckedSubtraction(-float.Epsilon, 1.0f));
            AssertBitwiseEqual(-1.0f, SubtractionOperatorsHelper<float, float, float>.op_CheckedSubtraction(-0.0f, 1.0f));
            AssertBitwiseEqual(float.NaN, SubtractionOperatorsHelper<float, float, float>.op_CheckedSubtraction(float.NaN, 1.0f));
            AssertBitwiseEqual(-1.0f, SubtractionOperatorsHelper<float, float, float>.op_CheckedSubtraction(0.0f, 1.0f));
            AssertBitwiseEqual(-1.0f, SubtractionOperatorsHelper<float, float, float>.op_CheckedSubtraction(float.Epsilon, 1.0f));
            AssertBitwiseEqual(-1.0f, SubtractionOperatorsHelper<float, float, float>.op_CheckedSubtraction(MaxSubnormal, 1.0f));
            AssertBitwiseEqual(-1.0f, SubtractionOperatorsHelper<float, float, float>.op_CheckedSubtraction(MinNormal, 1.0f));
            AssertBitwiseEqual(0.0f, SubtractionOperatorsHelper<float, float, float>.op_CheckedSubtraction(1.0f, 1.0f));
            AssertBitwiseEqual(float.MaxValue, SubtractionOperatorsHelper<float, float, float>.op_CheckedSubtraction(float.MaxValue, 1.0f));
            AssertBitwiseEqual(float.PositiveInfinity, SubtractionOperatorsHelper<float, float, float>.op_CheckedSubtraction(float.PositiveInfinity, 1.0f));
        }

        //
        // IUnaryNegationOperators
        //

        [Fact]
        public static void op_UnaryNegationTest()
        {
            AssertBitwiseEqual(float.PositiveInfinity, UnaryNegationOperatorsHelper<float, float>.op_UnaryNegation(float.NegativeInfinity));
            AssertBitwiseEqual(float.MaxValue, UnaryNegationOperatorsHelper<float, float>.op_UnaryNegation(float.MinValue));
            AssertBitwiseEqual(1.0f, UnaryNegationOperatorsHelper<float, float>.op_UnaryNegation(-1.0f));
            AssertBitwiseEqual(MinNormal, UnaryNegationOperatorsHelper<float, float>.op_UnaryNegation(-MinNormal));
            AssertBitwiseEqual(MaxSubnormal, UnaryNegationOperatorsHelper<float, float>.op_UnaryNegation(-MaxSubnormal));
            AssertBitwiseEqual(float.Epsilon, UnaryNegationOperatorsHelper<float, float>.op_UnaryNegation(-float.Epsilon));
            AssertBitwiseEqual(0.0f, UnaryNegationOperatorsHelper<float, float>.op_UnaryNegation(-0.0f));
            AssertBitwiseEqual(float.NaN, UnaryNegationOperatorsHelper<float, float>.op_UnaryNegation(float.NaN));
            AssertBitwiseEqual(-0.0f, UnaryNegationOperatorsHelper<float, float>.op_UnaryNegation(0.0f));
            AssertBitwiseEqual(-float.Epsilon, UnaryNegationOperatorsHelper<float, float>.op_UnaryNegation(float.Epsilon));
            AssertBitwiseEqual(-MaxSubnormal, UnaryNegationOperatorsHelper<float, float>.op_UnaryNegation(MaxSubnormal));
            AssertBitwiseEqual(-MinNormal, UnaryNegationOperatorsHelper<float, float>.op_UnaryNegation(MinNormal));
            AssertBitwiseEqual(-1.0f, UnaryNegationOperatorsHelper<float, float>.op_UnaryNegation(1.0f));
            AssertBitwiseEqual(float.MinValue, UnaryNegationOperatorsHelper<float, float>.op_UnaryNegation(float.MaxValue));
            AssertBitwiseEqual(float.NegativeInfinity, UnaryNegationOperatorsHelper<float, float>.op_UnaryNegation(float.PositiveInfinity));
        }

        [Fact]
        public static void op_CheckedUnaryNegationTest()
        {
            AssertBitwiseEqual(float.PositiveInfinity, UnaryNegationOperatorsHelper<float, float>.op_CheckedUnaryNegation(float.NegativeInfinity));
            AssertBitwiseEqual(float.MaxValue, UnaryNegationOperatorsHelper<float, float>.op_CheckedUnaryNegation(float.MinValue));
            AssertBitwiseEqual(1.0f, UnaryNegationOperatorsHelper<float, float>.op_CheckedUnaryNegation(-1.0f));
            AssertBitwiseEqual(MinNormal, UnaryNegationOperatorsHelper<float, float>.op_CheckedUnaryNegation(-MinNormal));
            AssertBitwiseEqual(MaxSubnormal, UnaryNegationOperatorsHelper<float, float>.op_CheckedUnaryNegation(-MaxSubnormal));
            AssertBitwiseEqual(float.Epsilon, UnaryNegationOperatorsHelper<float, float>.op_CheckedUnaryNegation(-float.Epsilon));
            AssertBitwiseEqual(0.0f, UnaryNegationOperatorsHelper<float, float>.op_CheckedUnaryNegation(-0.0f));
            AssertBitwiseEqual(float.NaN, UnaryNegationOperatorsHelper<float, float>.op_CheckedUnaryNegation(float.NaN));
            AssertBitwiseEqual(-0.0f, UnaryNegationOperatorsHelper<float, float>.op_CheckedUnaryNegation(0.0f));
            AssertBitwiseEqual(-float.Epsilon, UnaryNegationOperatorsHelper<float, float>.op_CheckedUnaryNegation(float.Epsilon));
            AssertBitwiseEqual(-MaxSubnormal, UnaryNegationOperatorsHelper<float, float>.op_CheckedUnaryNegation(MaxSubnormal));
            AssertBitwiseEqual(-MinNormal, UnaryNegationOperatorsHelper<float, float>.op_CheckedUnaryNegation(MinNormal));
            AssertBitwiseEqual(-1.0f, UnaryNegationOperatorsHelper<float, float>.op_CheckedUnaryNegation(1.0f));
            AssertBitwiseEqual(float.MinValue, UnaryNegationOperatorsHelper<float, float>.op_CheckedUnaryNegation(float.MaxValue));
            AssertBitwiseEqual(float.NegativeInfinity, UnaryNegationOperatorsHelper<float, float>.op_CheckedUnaryNegation(float.PositiveInfinity));
        }

        //
        // IUnaryPlusOperators
        //

        [Fact]
        public static void op_UnaryPlusTest()
        {
            AssertBitwiseEqual(float.NegativeInfinity, UnaryPlusOperatorsHelper<float, float>.op_UnaryPlus(float.NegativeInfinity));
            AssertBitwiseEqual(float.MinValue, UnaryPlusOperatorsHelper<float, float>.op_UnaryPlus(float.MinValue));
            AssertBitwiseEqual(-1.0f, UnaryPlusOperatorsHelper<float, float>.op_UnaryPlus(-1.0f));
            AssertBitwiseEqual(-MinNormal, UnaryPlusOperatorsHelper<float, float>.op_UnaryPlus(-MinNormal));
            AssertBitwiseEqual(-MaxSubnormal, UnaryPlusOperatorsHelper<float, float>.op_UnaryPlus(-MaxSubnormal));
            AssertBitwiseEqual(-float.Epsilon, UnaryPlusOperatorsHelper<float, float>.op_UnaryPlus(-float.Epsilon));
            AssertBitwiseEqual(-0.0f, UnaryPlusOperatorsHelper<float, float>.op_UnaryPlus(-0.0f));
            AssertBitwiseEqual(float.NaN, UnaryPlusOperatorsHelper<float, float>.op_UnaryPlus(float.NaN));
            AssertBitwiseEqual(0.0f, UnaryPlusOperatorsHelper<float, float>.op_UnaryPlus(0.0f));
            AssertBitwiseEqual(float.Epsilon, UnaryPlusOperatorsHelper<float, float>.op_UnaryPlus(float.Epsilon));
            AssertBitwiseEqual(MaxSubnormal, UnaryPlusOperatorsHelper<float, float>.op_UnaryPlus(MaxSubnormal));
            AssertBitwiseEqual(MinNormal, UnaryPlusOperatorsHelper<float, float>.op_UnaryPlus(MinNormal));
            AssertBitwiseEqual(1.0f, UnaryPlusOperatorsHelper<float, float>.op_UnaryPlus(1.0f));
            AssertBitwiseEqual(float.MaxValue, UnaryPlusOperatorsHelper<float, float>.op_UnaryPlus(float.MaxValue));
            AssertBitwiseEqual(float.PositiveInfinity, UnaryPlusOperatorsHelper<float, float>.op_UnaryPlus(float.PositiveInfinity));
        }

        //
        // IParsable and ISpanParsable
        //

        [Theory]
        [MemberData(nameof(SingleTests.Parse_Valid_TestData), MemberType = typeof(SingleTests))]
        public static void ParseValidStringTest(string value, NumberStyles style, IFormatProvider provider, float expected)
        {
            bool isDefaultProvider = provider == null || provider == NumberFormatInfo.CurrentInfo;
            float result;
            if ((style & ~(NumberStyles.Float | NumberStyles.AllowThousands)) == 0 && style != NumberStyles.None)
            {
                // Use Parse(string) or Parse(string, IFormatProvider)
                if (isDefaultProvider)
                {
                    Assert.True(ParsableHelper<float>.TryParse(value, null, out result));
                    Assert.Equal(expected, result);

                    Assert.Equal(expected, ParsableHelper<float>.Parse(value, null));
                }

                Assert.Equal(expected, ParsableHelper<float>.Parse(value, provider));
            }

            // Use Parse(string, NumberStyles, IFormatProvider)
            Assert.True(NumberBaseHelper<float>.TryParse(value, style, provider, out result));
            Assert.Equal(expected, result);

            Assert.Equal(expected, NumberBaseHelper<float>.Parse(value, style, provider));

            if (isDefaultProvider)
            {
                // Use Parse(string, NumberStyles) or Parse(string, NumberStyles, IFormatProvider)
                Assert.True(NumberBaseHelper<float>.TryParse(value, style, NumberFormatInfo.CurrentInfo, out result));
                Assert.Equal(expected, result);

                Assert.Equal(expected, NumberBaseHelper<float>.Parse(value, style, null));
                Assert.Equal(expected, NumberBaseHelper<float>.Parse(value, style, NumberFormatInfo.CurrentInfo));
            }
        }

        [Theory]
        [MemberData(nameof(SingleTests.Parse_Invalid_TestData), MemberType = typeof(SingleTests))]
        public static void ParseInvalidStringTest(string value, NumberStyles style, IFormatProvider provider, Type exceptionType)
        {
            bool isDefaultProvider = provider == null || provider == NumberFormatInfo.CurrentInfo;
            float result;
            if ((style & ~(NumberStyles.Float | NumberStyles.AllowThousands)) == 0 && style != NumberStyles.None && (style & NumberStyles.AllowLeadingWhite) == (style & NumberStyles.AllowTrailingWhite))
            {
                // Use Parse(string) or Parse(string, IFormatProvider)
                if (isDefaultProvider)
                {
                    Assert.False(ParsableHelper<float>.TryParse(value, null, out result));
                    Assert.Equal(default(float), result);

                    Assert.Throws(exceptionType, () => ParsableHelper<float>.Parse(value, null));
                }

                Assert.Throws(exceptionType, () => ParsableHelper<float>.Parse(value, provider));
            }

            // Use Parse(string, NumberStyles, IFormatProvider)
            Assert.False(NumberBaseHelper<float>.TryParse(value, style, provider, out result));
            Assert.Equal(default(float), result);

            Assert.Throws(exceptionType, () => NumberBaseHelper<float>.Parse(value, style, provider));

            if (isDefaultProvider)
            {
                // Use Parse(string, NumberStyles) or Parse(string, NumberStyles, IFormatProvider)
                Assert.False(NumberBaseHelper<float>.TryParse(value, style, NumberFormatInfo.CurrentInfo, out result));
                Assert.Equal(default(float), result);

                Assert.Throws(exceptionType, () => NumberBaseHelper<float>.Parse(value, style, null));
                Assert.Throws(exceptionType, () => NumberBaseHelper<float>.Parse(value, style, NumberFormatInfo.CurrentInfo));
            }
        }

        [Theory]
        [MemberData(nameof(SingleTests.Parse_ValidWithOffsetCount_TestData), MemberType = typeof(SingleTests))]
        public static void ParseValidSpanTest(string value, int offset, int count, NumberStyles style, IFormatProvider provider, float expected)
        {
            bool isDefaultProvider = provider == null || provider == NumberFormatInfo.CurrentInfo;
            float result;
            if ((style & ~(NumberStyles.Float | NumberStyles.AllowThousands)) == 0 && style != NumberStyles.None)
            {
                // Use Parse(string) or Parse(string, IFormatProvider)
                if (isDefaultProvider)
                {
                    Assert.True(SpanParsableHelper<float>.TryParse(value.AsSpan(offset, count), null, out result));
                    Assert.Equal(expected, result);

                    Assert.Equal(expected, SpanParsableHelper<float>.Parse(value.AsSpan(offset, count), null));
                }

                Assert.Equal(expected, SpanParsableHelper<float>.Parse(value.AsSpan(offset, count), provider: provider));
            }

            Assert.Equal(expected, NumberBaseHelper<float>.Parse(value.AsSpan(offset, count), style, provider));

            Assert.True(NumberBaseHelper<float>.TryParse(value.AsSpan(offset, count), style, provider, out result));
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(SingleTests.Parse_Invalid_TestData), MemberType = typeof(SingleTests))]
        public static void ParseInvalidSpanTest(string value, NumberStyles style, IFormatProvider provider, Type exceptionType)
        {
            if (value != null)
            {
                Assert.Throws(exceptionType, () => NumberBaseHelper<float>.Parse(value.AsSpan(), style, provider));

                Assert.False(NumberBaseHelper<float>.TryParse(value.AsSpan(), style, provider, out float result));
                Assert.Equal(0, result);
            }
        }
    }
}
