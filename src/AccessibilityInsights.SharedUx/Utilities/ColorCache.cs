// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Drawing;

namespace AccessibilityInsights.SharedUx.Utilities
{
    internal class ColorCache
    {
        private readonly Dictionary<Color, Color> _cachedColors = new Dictionary<Color, Color>();

        internal Color GetMappedColor(Color colorToMap, Func<Color, Color> colorMapper)
        {
            if (_cachedColors.TryGetValue(colorToMap, out Color mappedColor))
                return mappedColor;

            mappedColor = colorMapper(colorToMap);
            _cachedColors.Add(colorToMap, mappedColor);
            return mappedColor;
        }
    }
}
