// GTA2.NET
// 
// File: ConvertUnits.cs
// Created: 01.08.2013
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
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Helper
{
    public static class ConvertUnits
    {
        public const float MetersPerBlock = 3.6f;
        public const int PixelsPerBlock = 64;

        public static float ToMeters(float blockUnits)
        {
            return blockUnits * MetersPerBlock;
        }

        public static Vector2 ToMeters(this Vector2 blockUnits)
        {
            return Vector2.Multiply(blockUnits, MetersPerBlock);
        }

        public static Vertices ToMeters(this List<Vector2> blockUnits)
        {
            return new Vertices(blockUnits.Select(blockUnit => blockUnit.ToMeters()).ToList());
        }

        public static float ToBlockUnits(int pixels)
        {
            return (float) pixels/PixelsPerBlock;
        }

        public static float ToBlockUnits(float meters)
        {
            return meters/MetersPerBlock;
        }

        public static Vector2 ToBlockUnits(this Vector2 meters)
        {
            return Vector2.Divide(meters, MetersPerBlock);
        }
    }
}
