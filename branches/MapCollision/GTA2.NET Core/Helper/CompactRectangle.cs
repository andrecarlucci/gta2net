// GTA2.NET
// 
// File: CompactRectangle.cs
// Created: 11.04.2013
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
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Helper
{
    /// <summary>
    /// This struct is used to serialize Rectangles to XML, as in the XNA rectangle the position is saved twice.
    /// </summary>
    [Serializable]
    public struct CompactRectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public CompactRectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public static CompactRectangle FromRectangle(Rectangle rect)
        {
            return new CompactRectangle {X = rect.X, Y = rect.X, Width = rect.Width, Height = rect.Height};
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        public static CompactRectangle FromDrawingRectangle(System.Drawing.Rectangle rect)
        {
            return new CompactRectangle {X = rect.X, Y = rect.X, Width = rect.Width, Height = rect.Height};
        }

        public System.Drawing.Rectangle ToDrawingRectangle()
        {
            return new System.Drawing.Rectangle(X, Y, Width, Height);
        }

    }
}
