// GTA2.NET
// 
// File: Obstacle.cs
// Created: 09.03.2013
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
    public interface IObstacle
    {
        int Z { get; set; }
    }

    [Serializable]
    public class RectangleObstacle : IObstacle
    {
        public int Z { get; set; }

        public Vector2 Position;

        public float Width;

        public float Length;

        public RectangleObstacle(Vector2 position, int z, float width, float length)
        {
            Z = z;
            Position = position;
            Width = width;
            Length = length;
        }
    }

    [Serializable]
    public class PolygonObstacle : IObstacle
    {
        public int Z { get; set; }

        public List<Vector2> Vertices { get; set; }
 
        public PolygonObstacle(int z)
        {
            Z = z;
            Vertices = new List<Vector2>();
        }

        public bool IsPointInPolygon(Vector2 point)
        {
            var isInside = false;
            for (int i = 0, j = Vertices.Count - 1; i < Vertices.Count; j = i++)
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if ((point.X == Vertices[i].X && point.Y == Vertices[i].Y) || (point.X == Vertices[j].X && point.Y == Vertices[j].Y)) //point IS one of the edges
                // ReSharper restore CompareOfFloatsByEqualityOperator
                    return true;
                if (((Vertices[i].Y > point.Y) != (Vertices[j].Y > point.Y)) && (point.X < (Vertices[j].X - Vertices[i].X) * (point.Y - Vertices[i].Y) / (Vertices[j].Y - Vertices[i].Y) + Vertices[i].X))
                    isInside = !isInside;
            }
            return isInside;
        }
    }
}
