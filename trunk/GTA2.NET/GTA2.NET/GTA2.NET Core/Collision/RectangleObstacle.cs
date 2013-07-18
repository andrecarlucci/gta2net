// GTA2.NET
// 
// File: RectangleObstacle.cs
// Created: 30.06.2013
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
using FarseerPhysics.Common;
using Hiale.GTA2NET.Core.Helper;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    [Serializable]
    public class RectangleObstacle : PolygonObstacle
    {
        public ObstacleType Type
        {
            get { return ObstacleType.Rectangle; }
        }

        public new int Z { get; set; }

        public new bool IsSlope { get; set; }

        public new bool CollideWithBullets
        {
            get { return true; }
        }

        public float X { get; set; }

        public float Y { get; set; }

        public float Width { get; set; }

        public float Length { get; set; }

        public new bool Contains(Vector2 point)
        {
            return new RectangleF(X, Y, Width, Length).Contains(point);
        }

        public RectangleObstacle()
        {
            
        }

        public RectangleObstacle(Vertices vertices, int z) : base(vertices, z)
        {
            Z = z;
            CalculateBounds();
        }

        private void CalculateBounds()
        {
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minY = float.MaxValue;
            var maxY = float.MinValue;
            foreach (var polygonVertex in Vertices)
            {
                if (polygonVertex.X < minX)
                    minX = polygonVertex.X;
                if (polygonVertex.X > maxX)
                    maxX = polygonVertex.X;
                if (polygonVertex.Y < minY)
                    minY = polygonVertex.Y;
                if (polygonVertex.Y > maxY)
                    maxY = polygonVertex.Y;
            }
            X = minX;
            Y = minY;
            Width = maxX - minX;
            Length = maxY - minY;
        }
    }
}
