// GTA2.NET
// 
// File: FallEdge.cs
// Created: 13.03.2013
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
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public struct FallEdge : ILineObstacle
    {
        public int Z { get; set; }

        public Vector2 Start { get; set; }
        public Vector2 End { get; set; }
        public LineObstacleType Type { get; set; }

        public FallEdge(Vector2 start, Vector2 end, int z, LineObstacleType type) : this()
        {
            Z = z;
            Start = start;
            End = end;
            Type = type;
        }

        public static FallEdge DefaultLeft(int x, int y, int z)
        {
            return new FallEdge(new Vector2(x, y), new Vector2(x, y + 1), z, LineObstacleType.Vertical);
        }

        public static FallEdge DefaultTop(int x, int y, int z)
        {
            return new FallEdge(new Vector2(x, y), new Vector2(x + 1, y), z, LineObstacleType.Horizontal);
        }

        public static FallEdge DefaultRight(int x, int y, int z)
        {
            return new FallEdge(new Vector2(x + 1, y), new Vector2(x + 1, y + 1), z, LineObstacleType.Vertical);
        }

        public static FallEdge DefaultBottom(int x, int y, int z)
        {
            return new FallEdge(new Vector2(x, y + 1), new Vector2(x + 1, y + 1), z, LineObstacleType.Horizontal);
        }

        public override string ToString()
        {
            return Start + " - " + End;
        }

    }
}
