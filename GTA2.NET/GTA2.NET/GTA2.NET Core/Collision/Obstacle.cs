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
using Hiale.GTA2NET.Core.Helper;
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
    public class PolygonObstacle : IObstacle //ToDo: Use Farsser Vertices instead of List<Vector2>
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
            return Geometry.IsPointInPolygon(Vertices, point);
        }

        public bool IsPointInPolygonOrEdge(Vector2 point)
        {
            return Geometry.IsPointInPolygonOrEdge(Vertices, point);
        }
    }
}
