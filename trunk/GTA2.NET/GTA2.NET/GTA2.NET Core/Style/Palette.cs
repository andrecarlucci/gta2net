// GTA2.NET
// 
// File: Palette.cs
// Created: 14.04.2013
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
using Hiale.GTA2NET.Core.Helper;
using Color = Microsoft.Xna.Framework.Color;

namespace Hiale.GTA2NET.Core.Style
{
    [NamedVersion("Palette", 0, 1)]
    public class Palette
    {
        public Color[] Colors { get; set; }

        public Palette()
        {
            Colors = new Color[256];
        }

        public void Parse(byte[] data, int position)
        {
            if (data.Length != 4 || data[3] != 0)
                throw new ArgumentException();
            //Format is Blue, Green, Red, [Reserved Byte]
            Colors[position] = new Color(data[2], data[1], data[0], 0);
        }

        public static void SavePalettes(Palette[] palettes, string fileName)
        {
            using (var bmp = new Bitmap(palettes.Length, 256))
            {
                var bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var stride = bmData.Stride;
                var scan0 = bmData.Scan0;
                unsafe
                {
                    var p = (byte*)(void*)scan0;
                    var nOffset = stride - bmp.Width * 4;
                    for (var y = 0; y < bmp.Height; ++y)
                    {
                        for (var x = 0; x < bmp.Width; ++x)
                        {
                            p[0] = palettes[x].Colors[y].B;
                            p[1] = palettes[x].Colors[y].G;
                            p[2] = palettes[x].Colors[y].R;
                            p[3] = 255;
                            p += 4;
                        }
                        p += nOffset;
                    }
                }
                bmp.UnlockBits(bmData);
                bmp.Save(fileName, ImageFormat.Png);
            }
        }

        public static Palette[] LoadPalettes(string fileName)
        {
            Palette[] ps;
            using (var bmp = (Bitmap)Image.FromFile(fileName))
            {
                ps = new Palette[bmp.Width];
                for (var i = 0; i < ps.Length; i++)
                    ps[i] = new Palette();

                var bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var stride = bmData.Stride;
                var scan0 = bmData.Scan0;

                unsafe
                {
                    var p = (byte*)(void*)scan0;
                    var nOffset = stride - bmp.Width * 4;

                    for (var y = 0; y < bmp.Height; ++y)
                    {
                        for (var x = 0; x < bmp.Width; ++x)
                        {
                            ps[x].Colors[y] = new Color(p[2], p[1], p[0]);
                            p += 4;
                        }
                        p += nOffset;
                    }

                }
                bmp.UnlockBits(bmData);
            }
            return ps;
        }
    }
}
