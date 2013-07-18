// GTA2.NET
// 
// File: Geometry.cs
// Created: 14.07.2013
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
    public class Geometry
    {
        /// <summary>
        /// Normalizes the vector and returns its length. Also checks for zero to save some time and avoid NaN or Infinity results.
        /// </summary>
        /// <param name="v">Vector to normalize</param>
        /// <returns>Length of vector</returns>
        public static float SafeNormalize(ref Vector2 v)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (v.X == 0 && v.Y == 0)
                return 0;
            // ReSharper restore CompareOfFloatsByEqualityOperator

            var length = (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);

            var num = 1f / length;
            v.X *= num;
            v.Y *= num;
            return length;
        }

        /// <summary>
        /// Rotate a point from a given location and adjust using the Origin we are rotating around
        /// </summary>
        /// <param name="point">The point to rotate.</param>
        /// <param name="origin">Used origin for the rotation.</param>
        /// <param name="rotation">Angle in radians.</param>
        /// <returns></returns>
        public static Vector2 RotatePoint(Vector2 point, Vector2 origin, float rotation)
        {
            float outX = 0;
            float outY = 0;
            RotatePointInternal(point.X, point.Y, origin.X, origin.Y, rotation, ref outX, ref outY);
            return new Vector2(outX, outY);
        }

        /// <summary>
        /// Rotate a point from a given location and adjust using the Origin we are rotating around
        /// </summary>
        /// <param name="point">The point to rotate.</param>
        /// <param name="origin">Used origin for the rotation.</param>
        /// <param name="rotation">Angle in radians.</param>
        /// <returns></returns>
        public static Vector3 RotatePoint3(Vector3 point, Vector3 origin, float rotation)
        {
            float outX = 0;
            float outY = 0;
            RotatePointInternal(point.X, point.Y, origin.X, origin.Y, rotation, ref outX, ref outY);
            return new Vector3(outX, outY, point.Z);
        }

        private static void RotatePointInternal(float inX, float inY, float originX, float originY, float rotation, ref float outX, ref float outY)
        {
            outX = (float)(originX + (inX - originX) * Math.Cos(rotation) - (inY - originY) * Math.Sin(rotation));
            outY = (float)(originY + (inY - originY) * Math.Cos(rotation) + (inX - originX) * Math.Sin(rotation));
        }
    }
}
