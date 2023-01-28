// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MathNet.Numerics.LinearAlgebra;
using System;
using System.Drawing;

using Transform = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace AccessibilityInsights.SharedUx.ColorBlindness
{
    internal class LMSColor
    {
        private static readonly Transform FromRgb;
        private static readonly Transform ToRgb;

        private Vector<double> _lms;

#pragma warning disable CA1810 // Initialize reference type static fields inline
        static LMSColor()
#pragma warning restore CA1810 // Initialize reference type static fields inline
        {
#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
            // Matrices are based on https://ixora.io/projects/colorblindness/color-blindness-simulation-research/
            double[,] fromRgbArray =
            {
                { 0.31399022, 0.63951294, 0.04649755 },
                { 0.15537241, 0.75789446, 0.08670142 },
                { 0.01775239, 0.10944209, 0.87256922 },
            };
            double[,] toRgbArray =
            {
                { 5.47221206, -4.6419601, 0.16963708 },
                { -1.1252419, 2.29317094, -0.1678952 },
                { 0.02980165, -0.19318073, 1.16364789 },
            };
#pragma warning restore CA1814 // Prefer jagged arrays over multidimensional

            FromRgb = Transform.Build.DenseOfArray(fromRgbArray);
            ToRgb = Transform.Build.DenseOfArray(toRgbArray);
        }

        internal LMSColor(Color input)
        {
            if (input.A != 255)
                throw new ArgumentException("Alpha colors are not supported", nameof(input));

            double[] inputVector = { input.R, input.G, input.B };
            _lms = FromRgb * Vector<double>.Build.DenseOfArray(inputVector);
        }

        internal void ApplyTransform(Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            _lms = transform * _lms;
        }

        internal Color RgbColor
        {
            get
            {
                Vector<double> rgbVector = ToRgb * _lms;
                return Color.FromArgb(NearestValidColorValue(rgbVector[0]), NearestValidColorValue(rgbVector[1]), NearestValidColorValue(rgbVector[2]));
            }
        }

        private static int NearestValidColorValue(double input)
        {
            // The LMS color space is unbounded, so we need to include bounds check when converting back
            return Math.Max(0, Math.Min(255, (int)Math.Round(input)));
        }
    }
}
