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
using System.Collections.Generic;
using Hiale.GTA2NET.Core.Map;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public interface IObstacle
    {
        int Z { get; set; }
    }

    public interface ILineObstacle : IObstacle
    {
        Vector2 Start { get; set; }
        Vector2 End { get; set; }
        LineObstacleType Type { get; set; }
    }

    //public class SlopeObstacle : IObstacle
    //{
    //    public int Z { get; set; }

    //    public Vector2 Position;

    //    public SlopeType SlopeType;

    //    public SlopeObstacle(Vector2 position, int z, SlopeType slopeType) : this()
    //    {
    //        Z = z;
    //        Position = position;
    //        SlopeType = slopeType;
    //    }
    //}

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

    public enum LineObstacleType
    {
        Horizontal,
        Vertical,
        Other
    }

    public class LineObstacle : ILineObstacle
    {
        public int Z { get; set; }
        public Vector2 Start { get; set; }
        public Vector2 End { get; set; }
        public LineObstacleType Type { get; set; }

        public LineObstacle(Vector2 start, Vector2 end, int z, LineObstacleType type)
        {
            Z = z;
            Start = start;
            End = end;
            Type = type;
        }

        public static LineObstacle DefaultLeft(int x, int y, int z)
        {
            return new LineObstacle(new Vector2(x, y), new Vector2(x, y + 1), z, LineObstacleType.Vertical);
        }

        public static LineObstacle DefaultTop(int x, int y, int z)
        {
            return new LineObstacle(new Vector2(x, y), new Vector2(x + 1, y), z, LineObstacleType.Horizontal);
        }

        public static LineObstacle DefaultRight(int x, int y, int z)
        {
            return new LineObstacle(new Vector2(x + 1, y), new Vector2(x + 1, y + 1), z, LineObstacleType.Vertical);
        }

        public static LineObstacle DefaultBottom(int x, int y, int z)
        {
            return new LineObstacle(new Vector2(x + 1, y + 1), new Vector2(x, y + 1), z, LineObstacleType.Horizontal);
        }

        public override string ToString()
        {
            return Start + " - " + End;
        }
    }

    public class SlopeLineObstacle : LineObstacle
    {
        public SlopeLineObstacle(Vector2 start, Vector2 end, int z, LineObstacleType type) : base(start, end, z, type)
        {
        }
    }

}
