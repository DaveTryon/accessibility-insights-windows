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

        internal static VisionCondition CurrentCondition { get; set; }

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

            CurrentCondition = VisionCondition.TypicalVision;
        }

        internal static void SimulateCurrentCondition(Bitmap image) => SimulateCondition(image, GetCurrentCondition());
        internal static void SimulateProtonopia(Bitmap image) => SimulateCondition(image, Protonopia);
        internal static void SimulateDeuteranopia(Bitmap image) => SimulateCondition(image, Deuteranopia);
        internal static void SimulateTritanopia(Bitmap image) => SimulateCondition(image, Tritanopia);
        internal static void SimulateAchromatopsia(Bitmap image) => SimulateCondition(image, Achromatopsia);
        internal static void SimulateTypicalVision(Bitmap image) => SimulateCondition(image, TypicalVision);

        internal static Color SimulateCurrentCondition(Color color) => SimulateColor(color, GetCurrentCondition());
        internal static Color SimulateProtonopia(Color color) => SimulateColor(color, Protonopia);
        internal static Color SimulateDeuteranopia(Color color) => SimulateColor(color, Deuteranopia);
        internal static Color SimulateTritanopia(Color color) => SimulateColor(color, Tritanopia);
        internal static Color SimulateAchromatopsia(Color color) => SimulateColor(color, Achromatopsia);
        internal static Color SimulateTypicalVisionColor(Color color) => SimulateColor(color, TypicalVision);

        private static Condition GetCurrentCondition()
        {
            switch (CurrentCondition)
            {
                case VisionCondition.Achromatopsia: return Achromatopsia;
                case VisionCondition.Deuteranopia: return Deuteranopia;
                case VisionCondition.Protonopia: return Protonopia;
                case VisionCondition.Tritanopia: return Tritanopia;
                default: return TypicalVision;
            }
        }
        private static void SimulateCondition(Bitmap image, Condition condition)
        {
            image.UpdateBitmap(color => SimulateColor(color, condition));
        }

        private static Color SimulateColor(Color inputColor, Condition condition)
        {
            LMSColor lms = new LMSColor(inputColor);
            lms.ApplyTransform(condition);
            return lms.RgbColor;
        }
    }
}
