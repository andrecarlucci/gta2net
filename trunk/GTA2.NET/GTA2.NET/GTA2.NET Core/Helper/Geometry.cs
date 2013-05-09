// GTA2.NET
// 
// File: Geometry.cs
// Created: 09.05.2013
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Helper
{
    public static class Geometry
    {
        public static bool PointInPolygon(Vector2 p, Vector2[] poly)
        {
            var inside = false;
            if (poly.Length < 3)
                return false;

            var oldPoint = new Vector2(poly[poly.Length - 1].X, poly[poly.Length - 1].Y);
            for (var i = 0; i < poly.Length; i++)
            {
                var newPoint = new Vector2(poly[i].X, poly[i].Y);
                Vector2 p1;
                Vector2 p2;
                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X) && ((long) p.Y - (long) p1.Y)*(long) (p2.X - p1.X) < ((long) p2.Y - (long) p1.Y)*(long) (p.X - p1.X))
                    inside = !inside;
                oldPoint = newPoint;
            }
            return inside;
        }
    }
}
