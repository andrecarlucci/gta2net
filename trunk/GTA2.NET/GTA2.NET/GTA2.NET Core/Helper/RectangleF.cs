// GTA2.NET
// 
// File: RectangleF.cs
// Created: 23.06.2013
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
    public struct RectangleF
    {
        private static readonly RectangleF EmptyRectangle = new RectangleF();

        public float Height;
        public float Width;
        public float X;
        public float Y;

        static RectangleF()
        {
        }

        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public static RectangleF Empty
        {
            get { return EmptyRectangle; }
        }

        public float Left
        {
            get { return X; }
        }

        public float Right
        {
            get { return X + Width; }
        }

        public float Top
        {
            get { return Y; }
        }

        public float Bottom
        {
            get { return Y + Height; }
        }

        public Vector2 Location
        {
            get
            {
                return new Vector2(X, Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Vector2 Center
        {
            get
            {
                return new Vector2(X + Width / 2, Y + Height / 2);
            }
        }

        // ReSharper disable CompareOfFloatsByEqualityOperator
        public bool IsEmpty
        {
            get
            {
                if (Width == 0 && Height == 0 && X == 0)

                    return Y == 0;
                return false;
            }
        }

        public static bool operator ==(RectangleF a, RectangleF b)
        {
            if (a.X == b.X && a.Y == b.Y && a.Width == b.Width)
                return a.Height == b.Height;
            return false;
        }
        // ReSharper restore CompareOfFloatsByEqualityOperator

        public static bool operator !=(RectangleF a, RectangleF b)
        {
            return !(a == b);
        }

        public bool Contains(float x, float y)
        {
            if (X <= x && x < X + Width && Y <= y)
                return y < Y + Height;
            return false;
        }

        public bool Contains(Vector2 value)
        {
            //the original Version of this code excludes the boarder.
            //if (X <= value.X && value.X < X + Width && Y <= value.Y)
            //    return value.Y < Y + Height;
            //return false;
            if (X <= value.X && value.X <= X + Width && Y <= value.Y)
                return value.Y <= Y + Height;
            return false;
        }

        public bool Contains(RectangleF value)
        {
            if (X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y)
                return value.Y + value.Height <= Y + Height;
            return false;
        }

        public void Offset(Vector2 offset)
        {
            X += offset.X;
            Y += offset.Y;
        }

        public void Offset(float offsetX, float offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        public void Inflate(float horizontalValue, float verticalValue)
        {
            X -= horizontalValue;
            Y -= verticalValue;
            Width += horizontalValue*2;
            Height += verticalValue*2;
        }

        public bool Equals(RectangleF other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RectangleF))
                return false;
            return this == (RectangleF) obj;
        }

        public override string ToString()
        {
            return string.Format("{{X:{0} Y:{1} Width:{2} Height:{3}}}", (object) X, (object) Y, (object) Width, (object) Height);
        }

        public override int GetHashCode()
        {
            //return this.X ^ this.Y ^ this.Width ^ this.Height;
            return 0; //ToDo
        }

        public bool Intersects(RectangleF value)
        {
            if (value.Left < Right && Left < value.Right && value.Top < Bottom)
                return Top < value.Bottom;
            return false;
        }

        public void Intersects(ref RectangleF value, out bool result)
        {
            result = value.Left < Right && Left < value.Right && value.Top < Bottom && Top < value.Bottom;
        }

        public static RectangleF Intersect(RectangleF value1, RectangleF value2)
        {
            RectangleF result;
            Intersect(ref value1, ref value2, out result);
            return result;
        }

        public static void Intersect(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
        {
            if (value1.Intersects(value2))
            {
                var num1 = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
                var x = Math.Max(value1.X, value2.X);
                var y = Math.Max(value1.Y, value2.Y);
                var num2 = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
                result = new RectangleF(x, y, num1 - x, num2 - y);
            }
            else
                result = new RectangleF(0, 0, 0, 0);
        }

        public static RectangleF Union(RectangleF value1, RectangleF value2)
        {
            var x = Math.Min(value1.X, value2.X);
            var y = Math.Min(value1.Y, value2.Y);
            return new RectangleF(x, y, Math.Max(value1.Right, value2.Right) - x, Math.Max(value1.Bottom, value2.Bottom) - y);
        }

        public static void Union(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
        {
            result.X = Math.Min(value1.X, value2.X);
            result.Y = Math.Min(value1.Y, value2.Y);
            result.Width = Math.Max(value1.Right, value2.Right) - result.X;
            result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
        }
    }
}