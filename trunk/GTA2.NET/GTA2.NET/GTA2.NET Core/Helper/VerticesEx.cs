// GTA2.NET
// 
// File: VerticesEx.cs
// Created: 28.06.2013
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
    public class VerticesEx : Vertices, IEquatable<VerticesEx>
    {
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyFieldInGetHashCode
                return this.Aggregate(397, (current, vector) => current*29 + vector.X.GetHashCode()*vector.Y.GetHashCode());
                // ReSharper restore NonReadonlyFieldInGetHashCode
            }
        }

        public bool Equals(VerticesEx other)
        {
            if (other == null)
                return false;
            return Count == other.Count && this.All(other.Contains);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var vertices = obj as VerticesEx;
            return vertices != null && Equals(vertices);
        }

        public static bool operator ==(VerticesEx a, VerticesEx b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || ((object)b == null))
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(VerticesEx a, VerticesEx b)
        {
            return !(a == b);
        }

        public bool Contains(List<Vector2> polygon)
        {
            return polygon.All(point => IsPointInPolygonOrEdge(this, point));
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

        public static bool IsPolygonSubsetOf(Vertices polygonA, Vertices polygonB, out Vertices biggerPolygon)
        {
            var area = polygonA.GetArea();
            var area2 = polygonB.GetArea();
            Vertices smallerPolygon;
            if (area > area2)
            {
                biggerPolygon = polygonA;
                smallerPolygon = polygonB;
            }
            else
            {
                biggerPolygon = polygonB;
                smallerPolygon = polygonA;
            }
            var containAll = true;
            foreach (var vertex in smallerPolygon)
            {
                if (IsPointInPolygonOrEdge(biggerPolygon, vertex))
                    continue;
                containAll = false;
                break;
            }
            return containAll;
        }

        public static bool IsRectangle(IList<Vector2> polygon)
        {
            if (polygon.Count != 4)
                return false;
            // ReSharper disable CompareOfFloatsByEqualityOperator
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (polygon[i].X != polygon[j].X && polygon[i].Y != polygon[j].Y)
                    return false;
            }
            return true;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

    }
}