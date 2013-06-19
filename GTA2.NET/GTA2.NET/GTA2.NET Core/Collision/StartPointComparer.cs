// GTA2.NET
// 
// File: StartPointComparer.cs
// Created: 20.06.2013
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
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public class StartPointComparer : Comparer<Vector2>
    {
        public override int Compare(Vector2 x, Vector2 y)
        {
            //if (x == null && y == null)
            //    return 0;
            //if (x == null)
            //    return -1;
            //if (y == null)
            //    return 1;

            //var xMinX = float.MaxValue;
            //var xMinY = float.MaxValue;
            //if (x.Start.X < xMinX)
            //    xMinX = x.Start.X;
            //if (x.End.X < xMinX)
            //    xMinX = x.End.X;

            //if (x.Start.Y < xMinY)
            //    xMinY = x.Start.Y;
            //if (x.End.Y < xMinY)
            //    xMinY = x.End.Y;

            //var yMinX = float.MaxValue;
            //var yMinY = float.MaxValue;
            //if (y.Start.X < yMinX)
            //    yMinX = y.Start.X;
            //if (y.End.X < yMinX)
            //    yMinX = y.End.X;

            //if (y.Start.Y < yMinY)
            //    yMinY = y.Start.Y;
            //if (y.End.Y < yMinY)
            //    yMinY = y.End.Y;

            //if (xMinX < yMinX)
            //    return 1;
            //if (yMinX < xMinX)
            //    return -1;
            //if (xMinX == yMinX)
            //{
            //    if (xMinY < yMinY)
            //        return 1;
            //    if (yMinY < xMinY)
            //        return -1;
            //    if (xMinY == yMinY)
            //        return 0;
            //}

            if (x.X < y.X)
                return -1;
            if (x.X > y.X)
                return 1;
            if (x.X == y.X)
            {
                if (x.Y < y.Y)
                    return -1;
                if (x.Y > y.Y)
                    return 1;
                if (x.Y == y.Y)
                    return 0;
            }
            return 0;

        }
    }
}
