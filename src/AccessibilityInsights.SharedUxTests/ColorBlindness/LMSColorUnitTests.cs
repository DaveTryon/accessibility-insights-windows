// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using AccessibilityInsights.SharedUx.ColorBlindness;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace AccessibilityInsights.SharedUxTests.ColorBlindness
{
    [TestClass]
    public class LMSColorUnitTests
    {
        private static IEnumerable<Color> KnownNonAlphaColors = BuildKnownNonAlphaColors();

        private static IEnumerable<Color> BuildKnownNonAlphaColors()
        {
            List<Color> knownNonAlphaColors = new List<Color>();
            foreach (KnownColor knownColor in Enum.GetValues(typeof(KnownColor)))
            {
                Color color = Color.FromKnownColor(knownColor);
                if (color.A != 255)
                    continue;

                knownNonAlphaColors.Add(color);
            }

            return knownNonAlphaColors;
        }

        private static Matrix<double> BuildScalingMatrix(double scale)
        {
            double[,] scaleArray =
            {
                { scale, 0.0, 0.0 },
                { 0.0, scale, 0.0 },
                { 0.0, 0.0, scale },
            };
            return Matrix<double>.Build.DenseOfArray(scaleArray);
        }

        [TestMethod]
        [Timeout(1000)]
        public void Ctor_HasAlpha_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(() => new LMSColor(Color.FromArgb(128, Color.Aqua)));
        }

        [TestMethod]
        [Timeout(1000)]
        public void Ctor_NoAlpha_DoesNotThrow()
        {
            new LMSColor(Color.Aqua);
        }

        [TestMethod]
        [Timeout(1000)]
        public void RgbColor_NoTransform_RoundTripValuesMatchOriginalValues()
        {
            foreach (Color input in KnownNonAlphaColors)
            {
                LMSColor lms = new LMSColor(input);
                Color rgb = lms.RgbColor;
                Assert.AreEqual(input.A, rgb.A, input.ToString());
                Assert.AreEqual(input.R, rgb.R, input.ToString());
                Assert.AreEqual(input.G, rgb.G, input.ToString());
                Assert.AreEqual(input.B, rgb.B, input.ToString());
            }
        }

        [TestMethod]
        [Timeout(1000)]
        public void ApplyTransform_TransformIsNull_ThrowsArgumentNullException()
        {
            LMSColor lms = new LMSColor(Color.Gray);
            Assert.ThrowsException<ArgumentNullException>(() => lms.ApplyTransform(null));
        }

        [TestMethod]
        [Timeout(1000)]
        public void ApplyTransform_FiftyPercent_TransformedValuesAreReduced()
        {
            Matrix<double> fiftyPercentTransform = BuildScalingMatrix(0.5);

            foreach (Color input in KnownNonAlphaColors)
            {
                LMSColor lms = new LMSColor(input);

                lms.ApplyTransform(fiftyPercentTransform);

                Color rgb = lms.RgbColor;
                Assert.IsTrue(input.A >= rgb.A, input.ToString());
                Assert.IsTrue(input.R >= rgb.R, input.ToString());
                Assert.IsTrue(input.G >= rgb.G, input.ToString());
                Assert.IsTrue(input.B >= rgb.B, input.ToString());
            }
        }

        [TestMethod]
        [Timeout(1000)]
        public void ApplyTransform_FiftyPercentThenTwoHundredPercent_TransformedValuesMatchOriginalValues()
        {
            Matrix<double> fiftyPercentTransform = BuildScalingMatrix(0.5);
            Matrix<double> twoHundredPercentTransform = BuildScalingMatrix(2.0);

            foreach (Color input in KnownNonAlphaColors)
            {
                LMSColor lms = new LMSColor(input);

                lms.ApplyTransform(fiftyPercentTransform);
                lms.ApplyTransform(twoHundredPercentTransform);

                Color rgb = lms.RgbColor;
                Assert.AreEqual(input.A, rgb.A, input.ToString());
                Assert.AreEqual(input.R, rgb.R, input.ToString());
                Assert.AreEqual(input.G, rgb.G, input.ToString());
                Assert.AreEqual(input.B, rgb.B, input.ToString());
            }
        }
    }
}
