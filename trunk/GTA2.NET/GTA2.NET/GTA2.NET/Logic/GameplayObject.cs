// GTA2.NET
// 
// File: GameplayObject.cs
// Created: 16.02.2010
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
using Hiale.GTA2NET.Core;
using Hiale.GTA2NET.Core.Map;
using Microsoft.Xna.Framework;
using Hiale.GTA2NET.Helper;

namespace Hiale.GTA2NET.Logic
{
    public class GameplayObject
    {
        protected struct LineCollisionResult
        {
            public bool Collision;
            public bool TopLeftCollision;
            public bool TopRightCollision;
            public bool BottomRightCollision;
            public bool BottomLeftCollision;
        }

        public bool PlayerControlled { get; set; }

        /// <summary>
        /// Current position of this object. It represents the centre of the object.
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


        private Vector2 _origin;
        /// <summary>
        /// Origin of the object in 2D space.
        /// </summary>
        public Vector2 Origin
        {
            get
            {
                if (_origin == Vector2.Zero)
                    _origin = new Vector2(WidthHalf, HeightHalf);
                return _origin;
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

        /// <summary>
        /// 2D top left point of the object.
        /// </summary>
        public Vector2 TopLeft2
        {
            get
            {
                var topLeft = new Vector2(Position3.X - WidthHalf, Position3.Y - HeightHalf);
                return MainGame.RotatePoint(topLeft, Position2, RotationAngle);
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
                var topRight = new Vector2(Position3.X + WidthHalf, Position3.Y - HeightHalf);
                return MainGame.RotatePoint(topRight, Position2, RotationAngle);
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
                var bottomRight = new Vector2(Position3.X + WidthHalf, Position3.Y + HeightHalf);
                return MainGame.RotatePoint(bottomRight, Position2, RotationAngle);
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
                var bottomLeft = new Vector2(Position3.X - WidthHalf, Position3.Y + HeightHalf);
                return MainGame.RotatePoint(bottomLeft, Position2, RotationAngle);
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
        /// Width of the object. Used by collision detection.
        /// </summary>
        public float Width { get; protected set; }

        protected float WidthHalf;

        /// <summary>
        /// Height of the object. Used by collision detection.
        /// </summary>
        public float Height { get; protected set; }

        protected float HeightHalf;



        /// <summary>
        /// Helper variable to calculate the distance moved.
        /// </summary>
        protected static readonly Vector2 OriginZero = Vector2.Zero;

        //Added 23.03.2010
        public Vector2 Velocity;
        public bool Dead;

        protected GameplayObject(Vector3 startUpPosition, float width, float height) //add startup rotation
        {
            Position3 = startUpPosition;
            Width = width / 64;
            WidthHalf = Width / 2;
            Height = height / 64;
            HeightHalf = Height / 2;
        }

        protected void CreateSprite()
        {
            //if (ObjectCreated != null)
            //    ObjectCreated(this, new GenericEventArgs<GameplayObject>(this));
        }



        private float forward;
        private float rotation;
        private bool autoRotation;
        private float autoRotationAmount;

        public virtual void SetPlayerInput(PlayerInput playerInput, float elapsedTime)
        {
            if (playerInput.Forward > 0) //Accelerate
            {
                if (forward < 1)
                {
                    forward += playerInput.Forward * elapsedTime * 0.5f;
                    if (forward > 1)
                        forward = 1;
                }
            }
            else if (playerInput.Forward < 0) //Break
            {
                if (forward > -1)
                {
                    forward += playerInput.Forward * elapsedTime * 0.5f;
                    if (forward < -1)
                        forward = -1;
                }

            }
            else if (playerInput.Forward == 0) //Neither accelerate nor break
            {
                if (forward > 0)
                {
                    forward -= elapsedTime * 0.5f;
                    if (forward < 0)
                        forward = 0;
                }
                else if (forward < 0)
                {
                    forward += elapsedTime * 0.5f;
                    if (forward > 0)
                        forward = 0;
                }
            }

            rotation = playerInput.Rotation;
        }

        /// <summary>
        /// Update the gameplay object.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public virtual void Update(float elapsedTime)
        {
            if (forward == 0) //do nothing if we don't go forward or backward
                return;

            float forwardDelta = forward * elapsedTime * MainGame.ForwardScalar;
            float rotationDelta = rotation * elapsedTime * MainGame.RotationScalar;

            //if (forwardDelta == 0) //do nothing if we don't go forward or backward
            //    return;

            if (forwardDelta < 0) //Backwards
                rotationDelta *= -1;

            if (rotationDelta < 0)
                rotationDelta = 360 - Math.Abs(rotationDelta);


            //Calculate the direction based on the current angle.
            float rotationAngleNew = RotationAngle;
            rotationAngleNew += MathHelper.ToRadians(rotationDelta);

            if (autoRotation)
            {
                rotationAngleNew = autoRotationAmount;
                autoRotation = false;
            }

            Vector2 direction = MainGame.RotatePoint(new Vector2(0, forwardDelta), OriginZero, rotationAngleNew);

            bool collision = false;


            Vector2 topLeft = TopLeft2; //Create these for cache
            Vector2 topRight = TopRight2;
            Vector2 bottomRight = BottomRight2;
            Vector2 bottomLeft = BottomLeft2;

            //calculate the tentative new position of each point
            Vector2 newTopLeft = topLeft + direction;
            Vector2 newTopRight = topRight + direction;
            Vector2 newBottomRight = bottomRight + direction;
            Vector2 newBottomLeft = bottomLeft + direction;

            float maxZ = float.MinValue;
            float currentHeight = Position3.Z;
            SetCorrectHeight(ref currentHeight, topLeft + direction);
            SetMaxF(ref maxZ, currentHeight);
            currentHeight = Position3.Z;
            SetCorrectHeight(ref currentHeight, topRight + direction);
            SetMaxF(ref maxZ, currentHeight);
            currentHeight = Position3.Z;
            SetCorrectHeight(ref currentHeight, bottomRight + direction);
            SetMaxF(ref maxZ, currentHeight);
            currentHeight = Position3.Z;
            SetCorrectHeight(ref currentHeight, bottomLeft + direction);
            SetMaxF(ref maxZ, currentHeight);

            if (maxZ < 0)
                maxZ = Position3.Z; //still bug here...

            if (!collision)
                RotationAngle = rotationAngleNew;

            float newPositionX = Position3.X + direction.X;
            float newPositionY = Position3.Y + direction.Y;
            //ApplyGravity(ref newPositionX, ref newPositionY, ref weightedHeight);
            Position3 = new Vector3(newPositionX, newPositionY, maxZ);
            MainGame.WindowTitle = Position3.ToString();

        }

        private static void SetMaxF(ref float maxBlock, float currentValue)
        {
            if (currentValue > maxBlock)
                maxBlock = currentValue;
        }

        private static void SetCorrectHeight(ref float value, Vector2 point)
        {
            float x = point.X;
            float y = point.Y;
            int z = (int)Math.Round(value, 0);
            BlockInfo blockAbove = MainGame.Map.CityBlocks[(int)point.X, (int)point.Y, z + 1];
            if (blockAbove.IsMovableSlope)
                z++;
            BlockInfo blockBelow = MainGame.Map.CityBlocks[(int)point.X, (int)point.Y, z - 1];
            if (blockBelow.IsMovableSlope)
                z--;
            float newValue = MainGame.GetHeightF(ref x, ref y, ref z);
            if (newValue != value)
                value = newValue;
        }
    }
}