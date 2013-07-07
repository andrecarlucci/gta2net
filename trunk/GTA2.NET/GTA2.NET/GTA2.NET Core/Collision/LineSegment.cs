// GTA2.NET
// 
// File: LineSegment.cs
// Created: 23.05.2013
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
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public class LineSegment : LineObstacle
    {
        public new Vector2 Start
        {
            get
            {
                return IsInverted ? base.End : base.Start;
            }
            set
            {
                if (IsInverted)
                    base.End = value;
                else
                    base.Start = value;
                UpdateDirection();
            }
        }

        public new Vector2 End
        {
            get
            {
                return IsInverted ? base.Start : base.End;
            }
            set
            {
                if (IsInverted)
                    base.Start = value;
                else
                    base.End = value;
                UpdateDirection();
            }
        }

        public bool IsInverted
        {
            get { return _isInverted; }
            set
            {
                _isInverted = value;
                UpdateDirection();
            }
        }

        [XmlIgnore]
        public Direction Direction;

        private bool _isInverted;

        private LineSegment() : this(Vector2.Zero, Vector2.Zero)
        {
            //Xml Serializer
        }

        public LineSegment(Vector2 start, Vector2 end) : base(start, end)
        {
            UpdateDirection();
        }

        public LineSegment(Vector2 start, Vector2 end, bool isInverted) : base(start, end)
        {
            IsInverted = isInverted;
            UpdateDirection();
        }

        public LineSegment Invert()
        {
            IsInverted = !IsInverted;
            return this;
        }

        private void UpdateDirection()
        {
            Direction = CalculateDirection(Start, End);
        }

        private static Direction CalculateDirection(Vector2 startPoint, Vector2 endPoint)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (startPoint.X == endPoint.X)
            {
                if (startPoint.Y == endPoint.Y)
                    return Direction.None;
                if (startPoint.Y < endPoint.Y)
                    return Direction.Down;
                if (startPoint.Y > endPoint.Y)
                    return Direction.Up;
            }
            if (startPoint.X < endPoint.X)
            {
                if (startPoint.Y == endPoint.Y)
                    return Direction.Right;
                if (startPoint.Y < endPoint.Y)
                    return Direction.DownRight;
                if (startPoint.Y > endPoint.Y)
                    return Direction.UpRight;
            }
            if (startPoint.X > endPoint.X)
            {
                if (startPoint.Y == endPoint.Y)
                    return Direction.Left;
                if (startPoint.Y < endPoint.Y)
                    return Direction.DownLeft;
                if (startPoint.Y > endPoint.Y)
                    return Direction.UpLeft;
            }
            return Direction.None;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public override string ToString()
        {
            return Start + " - " + End;
        }
    }
}
