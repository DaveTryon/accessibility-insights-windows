// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using AccessibilityInsights.SharedUx.ColorBlindness;
using System.Drawing;

using Condition = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace AccessibilityInsights.SharedUx.Utilities
{
    public class VisionSimulator
    {
        private readonly static Condition Protonopia;
        private readonly static Condition Deuteranopia;
        private readonly static Condition Tritanopia;
        private readonly static Condition Achromatopsia;
        private readonly static Condition TypicalVision;

        static VisionSimulator()
        {
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

            Protonopia = Condition.Build.DenseOfArray(protonopia);
            Deuteranopia = Condition.Build.DenseOfArray(deuteranopia);
            Tritanopia = Condition.Build.DenseOfArray(tritanopia);
            Achromatopsia = Condition.Build.DenseOfArray(achromatopsia);
            TypicalVision = Condition.Build.DenseOfArray(typicalVision);
        }

        internal static Bitmap SimulateProtonopia(Bitmap image) => SimulateCondition(image, Protonopia);
        internal static Bitmap SimulateDeuteranopia(Bitmap image) => SimulateCondition(image, Deuteranopia);
        internal static Bitmap SimulateTritanopia(Bitmap image) => SimulateCondition(image, Tritanopia);
        internal static Bitmap SimulateAchromatopsia(Bitmap image) => SimulateCondition(image, Achromatopsia);
        internal static Bitmap SimulateTypicalVision(Bitmap image) => SimulateCondition(image, TypicalVision);

        private static Bitmap SimulateCondition(Bitmap image, Condition condition)
        {
            Bitmap simulatedImage = new Bitmap(image);
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color input = image.GetPixel(x, y);
                    simulatedImage.SetPixel(x, y, SimulateColor(input, condition));
                }
            }

            return simulatedImage;
        }

        private static Color SimulateColor(Color inputColor, Condition condition)
        {
            LMSColor lms = new LMSColor(inputColor);
            lms.ApplyTransform(condition);
            return lms.RgbColor;
        }
    }
}
