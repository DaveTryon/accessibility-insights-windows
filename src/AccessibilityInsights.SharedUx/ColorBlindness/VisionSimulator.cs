﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using AccessibilityInsights.SharedUx.Utilities;
using System;
using System.Drawing;

using ConditionMatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace AccessibilityInsights.SharedUx.ColorBlindness
{
    public static class VisionSimulator
    {
        private static readonly ConditionMatrix Protonopia;
        private static readonly ConditionMatrix Deuteranopia;
        private static readonly ConditionMatrix Tritanopia;
        private static readonly ConditionMatrix Achromatopsia;

#pragma warning disable CA1810 // Initialize reference type static fields inline
        static VisionSimulator()
#pragma warning restore CA1810 // Initialize reference type static fields inline
        {
#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
            // Matrices are based on https://ixora.io/projects/colorblindness/color-blindness-simulation-research/
            double[,] protonopia =
            {
                { 0.0, 1.05118294, -0.05116099 },
                { 0.0, 1.0, 0.0 },
                { 0.0, 0.0, -1.0 },
            };
            double[,] deuteranopia =
            {
                { 1.0, 0.0, 0.0 },
                { 0.9513092, 0.0, 0.04866992 },
                { 0.0, 0.0, -1.0 },
            };
            double[,] tritanopia =
            {
                { 1.0, 0.0, 0.0 },
                { 0.0, 1.0, 0.0 },
                { -0.86744736, 1.86727089, 0 },
            };
            double[,] achromatopsia =
            {
                { 0.212656, 0.715158, 0.072186 },
                { 0.212656, 0.715158, 0.072186 },
                { 0.212656, 0.715158, 0.072186 },
            };
#pragma warning restore CA1814 // Prefer jagged arrays over multidimensional

            Protonopia = ConditionMatrix.Build.DenseOfArray(protonopia);
            Deuteranopia = ConditionMatrix.Build.DenseOfArray(deuteranopia);
            Tritanopia = ConditionMatrix.Build.DenseOfArray(tritanopia);
            Achromatopsia = ConditionMatrix.Build.DenseOfArray(achromatopsia);
        }

        private static ConditionMatrix GetConditionMatrix(VisionCondition visionCondition)
        {
            switch (visionCondition)
            {
                case VisionCondition.Achromatopsia: return Achromatopsia;
                case VisionCondition.Deuteranopia: return Deuteranopia;
                case VisionCondition.Protonopia: return Protonopia;
                case VisionCondition.Tritanopia: return Tritanopia;
                case VisionCondition.TypicalVision: return null;
            }

            throw new ArgumentException("Unknown condition type", nameof(visionCondition));
        }

        internal static void SimulateCondition(Bitmap image, VisionCondition visionCondition)
        {
            ConditionMatrix conditionMatrix = GetConditionMatrix(visionCondition);

            if (conditionMatrix == null) // Perf optimization for typical vision
                return;

            image.UpdateBitmap(color => SimulateCondition(color, conditionMatrix));
        }

        internal static System.Windows.Media.Color SimulateCondition(System.Windows.Media.Color color, VisionCondition visionCondition)
        {
            return SimulateCondition(color, GetConditionMatrix(visionCondition));
        }

        internal static Color SimulateCondition(Color color, VisionCondition visionCondition)
        {
            return SimulateCondition(color, GetConditionMatrix(visionCondition));
        }

        private static System.Windows.Media.Color SimulateCondition(System.Windows.Media.Color color, ConditionMatrix conditionMatrix)
        {
            if (conditionMatrix == null) // Perf optimization for typical vision
                return color;

            Color convertedColor = Color.FromArgb(color.A, color.R, color.G, color.B);
            Color simulatedColor = SimulateCondition(convertedColor, conditionMatrix);
            return System.Windows.Media.Color.FromArgb(simulatedColor.A, simulatedColor.R, simulatedColor.G, simulatedColor.B);
        }

        private static Color SimulateCondition(Color inputColor, ConditionMatrix conditionMatrix)
        {
            if (conditionMatrix == null) // Perf optimization for typical vision
                return inputColor;

            LMSColor lms = new LMSColor(inputColor);
            lms.ApplyTransform(conditionMatrix);
            return lms.RgbColor;
        }
    }
}
