// GTA2.NET
// 
// File: LockBitmap.cs
// Created: 28.04.2013
// 
// 
// Copyright (C) 2010-2013 Hiale
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR
// IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Grand Theft Auto (GTA) is a registred trademark of Rockstar Games.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Hiale.GTA2NET.WinUI
{
    public class LockBitmap : IDisposable
    {
        //private readonly Bitmap _source;
        private IntPtr _ptr = IntPtr.Zero;
        private BitmapData _bitmapData;

        public byte[] Pixels { get; set; }
        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Bitmap Source { get; private set; }

        public LockBitmap(Bitmap source)
        {
            Source = source;
        }

        /// <summary>
        /// Lock bitmap data
        /// </summary>
        public void LockBits()
        {
            // Get width and height of bitmap
            Width = Source.Width;
            Height = Source.Height;

            // get total locked pixels count
            var pixelCount = Width*Height;

            // Create rectangle to lock
            var rect = new Rectangle(0, 0, Width, Height);

            // get source bitmap pixel format size
            Depth = Image.GetPixelFormatSize(Source.PixelFormat);

            // Check if bpp (Bits Per Pixel) is 8, 24, or 32
            if (Depth != 8 && Depth != 24 && Depth != 32)
                throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");

            // Lock bitmap and return bitmap data
            _bitmapData = Source.LockBits(rect, ImageLockMode.ReadWrite, Source.PixelFormat);

            // create byte array to copy pixel values
            var step = Depth/8;
            Pixels = new byte[pixelCount*step];
            _ptr = _bitmapData.Scan0;

            // Copy data from pointer to array
            Marshal.Copy(_ptr, Pixels, 0, Pixels.Length);
        }

        /// <summary>
        /// Unlock bitmap data
        /// </summary>
        public void UnlockBits()
        {
            // Copy data from byte array to pointer
            Marshal.Copy(Pixels, 0, _ptr, Pixels.Length);

            // Unlock bitmap data
            Source.UnlockBits(_bitmapData);
            _bitmapData = null;
        }

        /// <summary>
        /// Get the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color GetPixel(int x, int y)
        {
            var clr = Color.Empty;

            // Get color components count
            var cCount = Depth/8;

            // Get start index of the specified pixel
            var i = ((y*Width) + x)*cCount;

            if (i > Pixels.Length - cCount)
                throw new IndexOutOfRangeException();

            if (Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                var b = Pixels[i];
                var g = Pixels[i + 1];
                var r = Pixels[i + 2];
                var a = Pixels[i + 3]; // a
                clr = Color.FromArgb(a, r, g, b);
            }
            if (Depth == 24) // For 24 bpp get Red, Green and Blue
            {
                var b = Pixels[i];
                var g = Pixels[i + 1];
                var r = Pixels[i + 2];
                clr = Color.FromArgb(r, g, b);
            }
            if (Depth == 8) // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                var c = Pixels[i];
                clr = Color.FromArgb(c, c, c);
            }
            return clr;
        }

        /// <summary>
        /// Set the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void SetPixel(int x, int y, Color color)
        {
            // Get color components count
            var cCount = Depth/8;

            // Get start index of the specified pixel
            var i = ((y*Width) + x)*cCount;

            if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
                Pixels[i + 3] = color.A;
            }
            if (Depth == 24) // For 24 bpp set Red, Green and Blue
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
            }
            if (Depth == 8) // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                Pixels[i] = color.B;
            }
        }

        public void Dispose()
        {
            if (Source == null)
                return;
            try
            {
                if (_bitmapData != null)
                    Source.UnlockBits(_bitmapData);
            }
            catch (Exception)
            {
                //ignore
            }
            Source.Dispose();
        }
    }
}