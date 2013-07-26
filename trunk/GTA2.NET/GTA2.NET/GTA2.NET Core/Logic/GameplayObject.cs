// GTA2.NET
// 
// File: GameplayObject.cs
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
using Hiale.GTA2NET.Core.Helper;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Logic
{
    public class GameplayObject
    {
        /// <summary>
        /// Current position of this object. It represents the center of the object.
        /// </summary>
        public Vector3 Position3 { get; protected set; }

        /// <summary>
        /// 2D position of the object.
        /// </summary>
        public Vector2 Position2
        {
            get
            {
                return new Vector2(Position3.X, Position3.Y);
            }
        }

        private const float Circumference = 2 * (float)Math.PI;

        private float _rotationAngle;
        /// <summary>
        /// Current rotation angle in radians.
        /// </summary>
        public float RotationAngle
        {
            get { return _rotationAngle; }
            set
            {
                if (value < -Circumference)
                    value += Circumference;
                else if (value > Circumference)
                    value -= Circumference;
                _rotationAngle = value;
            }
        }

        public bool PlayerControlled { get; set; }

        /// <summary>
        /// 2D top left point of the object.
        /// </summary>
        public Vector2 TopLeft2
        {
            get
            {
                var topLeft = new Vector2(Position3.X - (Width / 2), Position3.Y - (Height / 2));
                return Geometry.RotatePoint(topLeft, Position2, RotationAngle);
            }
        }

        /// <summary>
        /// 3D top left point of the object.
        /// </summary>
        public Vector3 TopLeft3
        {
            get
            {
                var topLeft = TopLeft2;
                return new Vector3(topLeft.X, topLeft.Y, Position3.Z);
            }
        }


        /// <summary>
        /// 2D top right point of the object.
        /// </summary>
        public Vector2 TopRight2
        {
            get
            {
                var topRight = new Vector2(Position3.X + (Width / 2), Position3.Y - (Height / 2));
                return Geometry.RotatePoint(topRight, Position2, RotationAngle);
            }
        }

        /// <summary>
        /// 3D top right point of the object.
        /// </summary>
        public Vector3 TopRight3
        {
            get
            {
                var topRight = TopRight2;
                return new Vector3(topRight.X, topRight.Y, Position3.Z);
            }
        }

        /// <summary>
        /// 2D bottom right point of the object.
        /// </summary>
        public Vector2 BottomRight2
        {
            get
            {
                var bottomRight = new Vector2(Position3.X + (Width / 2), Position3.Y + (Height / 2));
                return Geometry.RotatePoint(bottomRight, Position2, RotationAngle);
            }
        }

        /// <summary>
        /// 3D bottom right point of the object.
        /// </summary>
        public Vector3 BottomRight3
        {
            get
            {
                var bottomRight = BottomRight2;
                return new Vector3(bottomRight.X, bottomRight.Y, Position3.Z);
            }
        }

        /// <summary>
        /// 2D bottom left point of the object.
        /// </summary>
        public Vector2 BottomLeft2
        {
            get
            {
                var bottomLeft = new Vector2(Position3.X - (Width / 2), Position3.Y + (Height / 2));
                return Geometry.RotatePoint(bottomLeft, Position2, RotationAngle);
            }
        }

        /// <summary>
        /// 3D bottom left point of the object.
        /// </summary>
        public Vector3 BottomLeft3
        {
            get
            {
                var bottomLeft = BottomLeft2;
                return new Vector3(bottomLeft.X, bottomLeft.Y, Position3.Z);
            }
        }

        /// <summary>
        /// Width of the object in "block units".
        /// </summary>
        public float Width { get; protected set; }

        /// <summary>
        /// Height of the object in "block units.
        /// </summary>
        public float Height { get; protected set; }

        protected GameplayObject(Vector3 startUpPosition, float startUpRotation, float width, float height)
        {
            Position3 = startUpPosition;
            RotationAngle = startUpRotation;
            Width = width / 64;
            Height = height / 64;
        }

        public virtual void Update(ParticipantInput input, float elapsedTime)
        {
            
        }
    }
}
