// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Functions
{
    public static partial class MathTests
    {
        // Tests MathF.Tanh(float) over 5000 iterations for the domain -1, +1

        private const float tanhSingleDelta = 0.0004f;
        private const float tanhSingleExpectedResult = 0.816701353f;

        public static void TanhSingleTest()
        {
            var result = 0.0f; var value = -1.0f;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                value += tanhSingleDelta;
                result += MathF.Tanh(value);
            }

            var diff = MathF.Abs(tanhSingleExpectedResult - result);

            if (diff > singleEpsilon)
            {
                throw new Exception($"Expected Result {tanhSingleExpectedResult,10:g9}; Actual Result {result,10:g9}");
            }
        }
    }
}
