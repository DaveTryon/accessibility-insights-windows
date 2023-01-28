// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace AccessibilityInsights.Win32
{
    internal class LockBitsHolder : IDisposable
    {
        private bool disposedValue;

        private Bitmap _bitmap;
        internal BitmapData BitmapData { get; private set; }

        internal LockBitsHolder(Bitmap bitmap)
        {
            _bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
            BitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && _bitmap != null && BitmapData != null)
                {
                    _bitmap.UnlockBits(BitmapData);
                    BitmapData = null;
                    _bitmap = null;
                }

                disposedValue = true;
            }
        }

        ~LockBitsHolder()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
