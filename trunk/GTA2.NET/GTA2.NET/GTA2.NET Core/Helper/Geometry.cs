// GTA2.NET
// 
// File: Geometry.cs
// Created: 16.06.2013
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

namespace Hiale.GTA2NET.Core.Helper
{
    public class Geometry
    {
        public static float CalculatePolygonArea(List<Vector2> polygon)
        {
            int i;
            float area = 0;
            for (i = 0; i < polygon.Count; i++)
            {
                var j = (i + 1) % polygon.Count;
                area += polygon[i].X * polygon[j].Y;
                area -= polygon[i].Y * polygon[j].X;
            }
            area /= 2;
            return (area < 0 ? -area : area);
        }

        public static bool IsPointInPolygon(List<Vector2> vertices, Vector2 point)
        {
            var isInside = false;
            for (int i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++)
            {
                if (((vertices[i].Y > point.Y) != (vertices[j].Y > point.Y)) && (point.X < (vertices[j].X - vertices[i].X) * (point.Y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y) + vertices[i].X))
                    isInside = !isInside;
            }
            return isInside;
        }

        public static bool IsPointInPolygonOrEdge(List<Vector2> vertices, Vector2 point)
        {
            var isInside = false;
            for (int i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++)
            {
                if (IsPointOnLineSegment(vertices[i], vertices[j], point, 0))
                    return true;
                if (((vertices[i].Y) >= point.Y != (vertices[j].Y >= point.Y)) && (point.X <= (vertices[j].X - vertices[i].X) * (point.Y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y) + vertices[i].X))
                    isInside = !isInside;
            }

            return isInside;
        }

        public static bool IsPointOnLineSegment(Vector2 endPoint1, Vector2 endPoint2, Vector2 point, float epsilon)
        {
            var segmentLengthSqr = (endPoint2.X - endPoint1.X) * (endPoint2.X - endPoint1.X) + (endPoint2.Y - endPoint1.Y) * (endPoint2.Y - endPoint1.Y);
            var r = ((point.X - endPoint1.X) * (endPoint2.X - endPoint1.X) + (point.Y - endPoint1.Y) * (endPoint2.Y - endPoint1.Y)) / segmentLengthSqr;
            if (r < 0 || r > 1)
                return false;
            var sl = (float)(((endPoint1.Y - point.Y) * (endPoint2.X - endPoint1.X) - (endPoint1.X - point.X) * (endPoint2.Y - endPoint1.Y)) / Math.Sqrt(segmentLengthSqr));
            return -epsilon <= sl && sl <= epsilon;
        }
    }
}
