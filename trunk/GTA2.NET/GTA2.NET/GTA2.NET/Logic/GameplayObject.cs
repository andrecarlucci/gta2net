// GTA2.NET
// 
// File: GameplayObject.cs
// Created: 21.02.2013
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
using System.Text;
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

        public event EventHandler PositionChanged;
        public event EventHandler RotationChanged;

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
            //set { _origin = value; }
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
                Vector2 topLeft = new Vector2(Position3.X - WidthHalf, Position3.Y - HeightHalf);
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
                Vector2 topLeft = TopLeft2;
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
                Vector2 topRight = new Vector2(Position3.X + WidthHalf, Position3.Y - HeightHalf);
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
                Vector2 topRight = TopRight2;
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
                Vector2 bottomRight = new Vector2(Position3.X + WidthHalf, Position3.Y + HeightHalf);
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
                Vector2 bottomRight = BottomRight2;
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
                Vector2 bottomLeft = new Vector2(Position3.X - WidthHalf, Position3.Y + HeightHalf);
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
                Vector2 bottomLeft = BottomLeft2;
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

            LineCollisionResult lineCollision;
            lineCollision.Collision = false;
            List<PolygonLine> blockLines = CreateBlockLines(ref topLeft, ref topRight, ref bottomRight, ref bottomLeft, ref newTopLeft, ref newTopRight, ref newBottomRight, ref newBottomLeft);
            if (blockLines.Count > 0)
            {
                lineCollision = LineCollide(ref direction, ref topLeft, ref topRight, ref bottomRight, ref bottomLeft, ref newTopLeft, ref newTopRight, ref newBottomRight, ref newBottomLeft, ref blockLines);

                if (lineCollision.Collision)
                {
                    //forward = 0;
                    //collision = true;
                    //System.Diagnostics.Debug.WriteLine("OK");
                }
                bool satCollision = false;
                satCollision  = SatCollision(ref direction, ref topLeft, ref topRight, ref bottomRight, ref bottomLeft, ref blockLines);
                if (lineCollision.Collision || satCollision)
                {
                    collision = true;
                    forward = 0;
                }
            }

            //Culculate height, check all 4 points of the object and take the maximum value of those.
            //We have to calculate the new value of each point again because it may have changed in the collision detection algorithm.
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

            if (PositionChanged != null)
                PositionChanged(this, EventArgs.Empty);
        }

        private List<PolygonLine> CreateBlockLines(ref Vector2 topLeft, ref Vector2 topRight, ref Vector2 bottomRight, ref Vector2 bottomLeft, ref Vector2 newTopLeft, ref Vector2 newTopRight, ref Vector2 newBottomRight, ref Vector2 newBottomLeft)
        {
            List<PolygonLine> blockLines = new List<PolygonLine>();
            //find all correspondig block coordinates (x & y) which the object is laying on.
            int minBlockX = (int)newTopLeft.X;
            int maxBlockX = minBlockX;
            int minBlockY = (int)newTopLeft.Y;
            int maxBlockY = minBlockY;

            //check all x, y values ranging from the current position to the new position (to check them against collisions)
            //current position
            SetMinMax(ref minBlockX, ref maxBlockX, topLeft.X);
            SetMinMax(ref minBlockY, ref maxBlockY, topLeft.Y);
            SetMinMax(ref minBlockX, ref maxBlockX, topRight.X);
            SetMinMax(ref minBlockY, ref maxBlockY, topRight.Y);
            SetMinMax(ref minBlockX, ref maxBlockX, bottomRight.X);
            SetMinMax(ref minBlockY, ref maxBlockY, bottomRight.Y);
            SetMinMax(ref minBlockX, ref maxBlockX, bottomLeft.X);
            SetMinMax(ref minBlockY, ref maxBlockY, bottomLeft.Y);

            //new position
            SetMinMax(ref minBlockX, ref maxBlockX, newTopLeft.X);
            SetMinMax(ref minBlockY, ref maxBlockY, newTopLeft.Y);
            SetMinMax(ref minBlockX, ref maxBlockX, newTopRight.X);
            SetMinMax(ref minBlockY, ref maxBlockY, newTopRight.Y);
            SetMinMax(ref minBlockX, ref maxBlockX, newBottomRight.X);
            SetMinMax(ref minBlockY, ref maxBlockY, newBottomRight.Y);
            SetMinMax(ref minBlockX, ref maxBlockX, newBottomLeft.X);
            SetMinMax(ref minBlockY, ref maxBlockY, newBottomLeft.Y);

            //check z + 1 blocks because they might block us
            int z = (int)Position3.Z + 1;

            if (maxBlockX > MainGame.Map.CityBlocks.GetLength(0) - 1)
                maxBlockX = MainGame.Map.CityBlocks.GetLength(0) - 1;

            if (maxBlockY > MainGame.Map.CityBlocks.GetLength(1) - 1)
                maxBlockY = MainGame.Map.CityBlocks.GetLength(1) - 1;


            //get a list with all blocking faces of the blocks.
            for (int x = minBlockX; x < maxBlockX + 1; x++)
            {
                for (int y = minBlockY; y < maxBlockY + 1; y++)
                {
                    BlockInfo block = MainGame.Map.CityBlocks[x, y, z];
                    if (block.IsEmpty)
                        continue;
                    GetLines(ref x, ref y, ref block, blockLines);
                }
            }
            return blockLines;
        }

        private static LineCollisionResult LineCollide(ref Vector2 direction, ref Vector2 topLeft, ref Vector2 topRight, ref Vector2 bottomRight, ref Vector2 bottomLeft,ref Vector2 newTopLeft, ref Vector2 newTopRight, ref Vector2 newBottomRight, ref Vector2 newBottomLeft, ref List<PolygonLine> blockLines)
        {
            LineCollisionResult result = new LineCollisionResult();

            //calculate four lies (one of each corner of the object) from the current position to the new position
            PolygonLine lineTopLeft = new PolygonLine(topLeft, newTopLeft);
            PolygonLine lineTopRight = new PolygonLine(topRight, newTopRight);
            PolygonLine lineBottomRight = new PolygonLine(bottomRight, newBottomRight);
            PolygonLine lineBottomLeft = new PolygonLine(bottomLeft, newBottomLeft);

            bool collision = false;
            float minValue = float.MaxValue;
            Vector2 minVector = direction;
            //check if the lines collide with any block
            for (int i = 0; i < blockLines.Count; i++)
            {
                Direction blockFaceType = blockLines[i].BlockFaceType;
                //if (blockFaceType == Direction.Down || blockFaceType == Direction.Left || blockFaceType == Direction.None|| blockFaceType == Direction.Right || blockFaceType == Direction.Up)
                //    continue;

                Vector2? topLeftIntersection = lineTopLeft.IntersectionWith(blockLines[i]);
                if (topLeftIntersection != null)
                {
                    MinVector(topLeftIntersection.Value, ref topLeft, ref minVector, ref minValue, ref blockFaceType, ref collision);
                    result.TopLeftCollision = true;
                }
                Vector2? topRightIntersection = lineTopRight.IntersectionWith(blockLines[i]);
                if (topRightIntersection != null)
                {
                    MinVector(topRightIntersection.Value, ref topRight, ref minVector, ref minValue, ref blockFaceType, ref collision);
                    result.TopRightCollision = true;
                }
                Vector2? bottomRightIntersection = lineBottomRight.IntersectionWith(blockLines[i]);
                if (bottomRightIntersection != null)
                {
                        MinVector(bottomRightIntersection.Value, ref bottomRight, ref minVector, ref minValue, ref blockFaceType, ref collision);
                        result.BottomRightCollision = true;
                }
                Vector2? bottomLeftIntersection = lineBottomLeft.IntersectionWith(blockLines[i]);
                if (bottomLeftIntersection != null)
                {
                        MinVector(bottomLeftIntersection.Value, ref bottomLeft, ref minVector, ref minValue, ref blockFaceType, ref collision);
                        result.BottomLeftCollision = true;
                }
                if (minVector == Vector2.Zero)
                    break;
            }
            direction = minVector;
            result.Collision = collision;
            return result;
        }

        private static void MinVector(Vector2 intersectionVector, ref Vector2 lineVector, ref Vector2 minVector, ref float minValue,ref Direction blockFaceType, ref bool collision)
        {
            collision = true;

            Vector2 currentVector = intersectionVector - lineVector;
            float distance = currentVector.Length();

            if (distance < minValue)
            {
                minVector = currentVector;
                minValue = distance;
            }
        }

        private static bool SatCollision(ref Vector2 direction, ref Vector2 topLeft, ref Vector2 topRight, ref Vector2 bottomRight, ref Vector2 bottomLeft, ref List<PolygonLine> blockLines)
        {
            Polygon polyonObject = new Polygon();
            polyonObject.Points.Add(topLeft + direction);
            polyonObject.Points.Add(topRight + direction);
            polyonObject.Points.Add(bottomRight + direction);
            polyonObject.Points.Add(bottomLeft + direction);

            bool collision = false;
            float minInterval = float.MaxValue;
            float maxInternal = float.MinValue;
            Vector2 minVector = direction;
            for (int i = 0; i < blockLines.Count; i++)
            {
                Polygon polygonBlockLine = new Polygon();
                polygonBlockLine.Points.Add(blockLines[i].Start);
                polygonBlockLine.Points.Add(blockLines[i].End);
                PolygonCollisionResult result = SeparatingAxisTheorem.PolygonCollision(ref polyonObject, ref polygonBlockLine, ref direction);
                if (result.Intersect)
                {
                    if (result.MinIntervalDistance > maxInternal)
                    {
                        collision = true;
                        maxInternal = result.MinIntervalDistance;
                        minVector = result.MinimumTranslationVector;
                    }

                    //if (result.MinIntervalDistance < minInterval)
                    //{
                    //    collision = true;
                    //    minInterval = result.MinIntervalDistance;
                    //    minVector = result.MinimumTranslationVector;
                    //}


                    //if (result.MinIntervalDistance == 0)
                    //    result.MinIntervalDistance = 0.0001f;
                    //direction = result.TranslationAxis * result.MinIntervalDistance;
                    //direction *= result.MinIntervalDistance;
                }
            }
            if (collision)
                direction = minVector;
                //direction *= minInterval;
            return collision;
        }

        protected float Adjust(ref float rotationAngleNew)
        {
            if (rotationAngleNew < MathHelper.Pi) // 0° - 180°
            {
                if (rotationAngleNew < MathHelper.PiOver2) // 0° - 90°
                {
                    if (rotationAngleNew < MathHelper.PiOver4) // 0° - 45°
                    {
                        // --> 0°
                        return 0;

                    }
                    else // 45° - 90°
                    {
                        // --> 90° (π / 2)
                        return MathHelper.PiOver2;
                    }
                }
                else // 90° - 180°
                {
                    if (rotationAngleNew < (3 * MathHelper.Pi / 4)) // 90° - 135°
                    {
                        // --> 90° (π / 2)
                        return MathHelper.PiOver2;
                    }
                    else // 135° - 180°
                    {
                        // --> 180° (π)
                        return MathHelper.Pi;
                    }
                }
            }
            else // 180° - 360°
            {
                if (rotationAngleNew < (3 * MathHelper.Pi / 2)) // 180° - 270°
                {
                    if (rotationAngleNew < 5 * MathHelper.Pi / 4) // 180° - 225°
                    {
                        // --> 180° (π)
                        return MathHelper.Pi;
                    }
                    else // 225° - 270°
                    {
                        // --> 270° (3π / 2)
                        return 3 * MathHelper.PiOver2;
                    }
                }
                else // 270° - 360°
                {
                    if (rotationAngleNew < (7 * MathHelper.Pi / 4)) // 270° - 315°
                    {
                        // --> 270° (3π / 2)
                        return 3 * MathHelper.PiOver2;
                    }
                    else // 315° - 360°
                    {
                        // --> 0°
                        return 0;
                    }
                }
            }
        }

        private static PolygonLine CreatePolygon(ref int x, ref int y, ref BlockInfo block, Direction direction)
        {
            PolygonLine polygon = new PolygonLine();
            switch (direction)
            {
                case Direction.Up:
                        polygon.Points.Add(new Vector2(x, y));
                        polygon.Points.Add(new Vector2(x + 1, y));
                        polygon.BlockFaceType = direction;
                    //}
                    break;
                case Direction.Down:
                        polygon.Points.Add(new Vector2(x + 1, y + 1));
                        polygon.Points.Add(new Vector2(x, y + 1));
                        polygon.BlockFaceType = direction;
                    //}
                    break;
                case Direction.Left:
                    if (block.SlopeType == SlopeType.DiagonalFacingUpRight || block.SlopeType == SlopeType.DiagonalFacingDownRight || !block.IsDiagonalSlope)
                    {
                        polygon.Points.Add(new Vector2(x, y + 1));
                        polygon.Points.Add(new Vector2(x, y));
                        polygon.BlockFaceType = direction;
                    }
                    else if (block.SlopeType == SlopeType.DiagonalFacingUpLeft)
                    {
                        polygon.Points.Add(new Vector2(x, y + 1));
                        polygon.Points.Add(new Vector2(x + 1, y));
                        polygon.BlockFaceType = Direction.DiagonalUpLeft;
                    }
                    else if (block.SlopeType == SlopeType.DiagonalFacingDownLeft)
                    {
                        polygon.Points.Add(new Vector2(x + 1, y + 1));
                        polygon.Points.Add(new Vector2(x, y));
                        polygon.BlockFaceType = Direction.DiagonalDownLeft;
                    }
                    break;
                case Direction.Right:
                    if (block.SlopeType == SlopeType.DiagonalFacingUpLeft || block.SlopeType == SlopeType.DiagonalFacingDownLeft || !block.IsDiagonalSlope)
                    {
                        polygon.Points.Add(new Vector2(x + 1, y));
                        polygon.Points.Add(new Vector2(x + 1, y + 1));
                        polygon.BlockFaceType = direction;
                    }
                    else if (block.SlopeType == SlopeType.DiagonalFacingUpRight)
                    {
                        polygon.Points.Add(new Vector2(x + 1, y + 1));
                        polygon.Points.Add(new Vector2(x, y));
                        polygon.BlockFaceType = Direction.DiagonalUpRight;
                    }
                    else if (block.SlopeType == SlopeType.DiagonalFacingDownRight)
                    {
                        polygon.Points.Add(new Vector2(x + 1, y));
                        polygon.Points.Add(new Vector2(x, y + 1));
                        polygon.BlockFaceType = Direction.DiagonalDownRight;
                    }
                    break;
            }

            return polygon;
        }

        private static void GetLines(ref int x, ref int y, ref BlockInfo block, ICollection<PolygonLine> blockLines)
        {
            if (block.Top)
            {
                PolygonLine polygon = CreatePolygon(ref x, ref y, ref block, Direction.Up);
                if (polygon.Points.Count > 0)
                {
                    blockLines.Add(polygon);
                }
            }
            if (block.Bottom)
            {
                PolygonLine polygon = CreatePolygon(ref x, ref y, ref block, Direction.Down);
                if (polygon.Points.Count > 0)
                {
                    blockLines.Add(polygon);
                }
            }
            if (block.Left)
            {
                PolygonLine polygon = CreatePolygon(ref x, ref y, ref block, Direction.Left);
                if (polygon.Points.Count > 0)
                {
                    blockLines.Add(polygon);
                }
            }
            if (block.Right)
            {
                PolygonLine polygon = CreatePolygon(ref x, ref y, ref block, Direction.Right);
                if (polygon.Points.Count > 0)
                {
                    blockLines.Add(polygon);
                }
            }
        }

        private void ApplyGravity(ref float x, ref float y, ref float z)
        {
            //if (z == 0)
            //    return;
            //BlockInfo block = MainGame.Map.CityBlocks[(int)x, (int)y, (int)z];
            //if (block.Lid != null && block.Lid)
            //    return;
            //if (z % 1 == 0)
            //{
            //    BlockInfo blockBelow = MainGame.Map.CityBlocks[(int)x, (int)y, (int)(z - 1)];
            //    if (blockBelow.IsEmpty)
            //    {
            //        z -= 0.1f;
            //    }
            //}
        }

        private static void SetMinMax(ref int minBlock, ref int maxBlock, float currentValue)
        {
            if (currentValue < minBlock)
                minBlock = (int)currentValue;
            if (currentValue > maxBlock)
                maxBlock = (int)currentValue;
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