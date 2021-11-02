// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using AccessibilityInsights.SharedUx.Utilities;
using System.Drawing;

using Condition = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace AccessibilityInsights.SharedUx.ColorBlindness
{
    public static class VisionSimulator
    {
        private readonly static Condition Protonopia;
        private readonly static Condition Deuteranopia;
        private readonly static Condition Tritanopia;
        private readonly static Condition Achromatopsia;
        private readonly static Condition TypicalVision;

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
            double[,] typicalVision =
            {
                { 1.0, 0.0, 0.0 },
                { 0.0, 1.0, 0.0 },
                { 0.0, 0.0, 1.0 },
            };
#pragma warning restore CA1814 // Prefer jagged arrays over multidimensional

            Protonopia = Condition.Build.DenseOfArray(protonopia);
            Deuteranopia = Condition.Build.DenseOfArray(deuteranopia);
            Tritanopia = Condition.Build.DenseOfArray(tritanopia);
            Achromatopsia = Condition.Build.DenseOfArray(achromatopsia);
            TypicalVision = Condition.Build.DenseOfArray(typicalVision);
        }

        private static Condition GetCondition(VisionCondition visionCondition)
        {
            switch (visionCondition)
            {
                case VisionCondition.Achromatopsia: return Achromatopsia;
                case VisionCondition.Deuteranopia: return Deuteranopia;
                case VisionCondition.Protonopia: return Protonopia;
                case VisionCondition.Tritanopia: return Tritanopia;
                default: return TypicalVision;
            }
        }

        internal static void SimulateCondition(Bitmap image, VisionCondition visionCondition)
        {
            image.UpdateBitmap(color => SimulateCondition(color, GetCondition(visionCondition)));
        }

        internal static void SimulateCondition(Bitmap image, Condition condition)
        {
            image.UpdateBitmap(color => SimulateCondition(color, condition));
        }

        internal static System.Windows.Media.Color SimulateCondition(System.Windows.Media.Color color, VisionCondition visionCondition)
        {
            return SimulateCondition(color, GetCondition(visionCondition));
        }

        internal static Color SimulateCondition(Color color, VisionCondition visionCondition)
        {
            return SimulateCondition(color, GetCondition(visionCondition));
        }

        internal static System.Windows.Media.Color SimulateCondition(System.Windows.Media.Color color, Condition condition)
        {
            Color convertedColor = Color.FromArgb(color.A, color.R, color.G, color.B);
            Color simulatedColor = SimulateCondition(convertedColor, condition);
            return System.Windows.Media.Color.FromArgb(simulatedColor.A, simulatedColor.R, simulatedColor.G, simulatedColor.B);
        }

        internal static Color SimulateCondition(Color inputColor, Condition condition)
        {
            LMSColor lms = new LMSColor(inputColor);
            lms.ApplyTransform(condition);
            return lms.RgbColor;
        }
    }
}
