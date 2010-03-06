//Created: 16.02.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiale.GTA2NET.Core.Map;
using Microsoft.Xna.Framework;
using Hiale.GTA2NET.Helper;
using Hiale.GTA2.Core.Map;

namespace Hiale.GTA2NET.Logic
{
    public class MovableObject
    {
        public static event EventHandler<GenericEventArgs<MovableObject>> ObjectCreated;
        public event EventHandler PositionChanged;
        public event EventHandler RotationChanged;

        private Vector3 _position3;
        /// <summary>
        /// Current position of this object. It represents the centre of the object.
        /// </summary>
        public Vector3 Position3
        {
            get { return _position3; }
            set
            {
                _position3 = value;
                //if (PositionChanged != null)
                //    PositionChanged(this, EventArgs.Empty);
            }
        }

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
                    _origin = new Vector2(Width / 2, Height / 2);
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
                Vector2 topLeft = new Vector2(Position3.X - (Width / 2), Position3.Y - (Height / 2));
                return RotatePoint(topLeft, Position2, RotationAngle);
            }
        }

        private float _topLeftZ;

        /// <summary>
        /// 3D top left point of the object.
        /// </summary>
        public Vector3 TopLeft3
        {
            get
            {
                Vector2 topLeft = TopLeft2;
                return new Vector3(topLeft.X, topLeft.Y, _topLeftZ);
            }
        }
        

        /// <summary>
        /// 2D top right point of the object.
        /// </summary>
        public Vector2 TopRight2
        {
            get
            {
                Vector2 topRight = new Vector2(Position3.X + (Width / 2), Position3.Y - (Height / 2));
                return RotatePoint(topRight, Position2, RotationAngle);
            }
        }

        private float _topRightZ;

        /// <summary>
        /// 3D top right point of the object.
        /// </summary>
        public Vector3 TopRight3
        {
            get
            {
                Vector2 topRight = TopRight2;
                return new Vector3(topRight.X, topRight.Y, _topRightZ);
            }
        }

        /// <summary>
        /// 2D bottom right point of the object.
        /// </summary>
        public Vector2 BottomRight2
        {
            get
            {
                Vector2 bottomRight = new Vector2(Position3.X + (Width / 2), Position3.Y + (Height / 2));
                return RotatePoint(bottomRight, Position2, RotationAngle);
            }
        }

        private float _bottomRightZ;

        /// <summary>
        /// 3D bottom right point of the object.
        /// </summary>
        public Vector3 BottomRight3
        {
            get
            {
                Vector2 bottomRight = BottomRight2;
                return new Vector3(bottomRight.X, bottomRight.Y, _bottomRightZ);
            }
        }

        /// <summary>
        /// 2D bottom left point of the object.
        /// </summary>
        public Vector2 BottomLeft2
        {
            get
            {
                Vector2 bottomLeft = new Vector2(Position3.X - (Width / 2), Position3.Y + (Height / 2));
                return RotatePoint(bottomLeft, Position2, RotationAngle);
            }
        }

        private float _bottomLeftZ;

        /// <summary>
        /// 3D bottom left point of the object.
        /// </summary>
        public Vector3 BottomLeft3
        {
            get
            {
                Vector2 bottomLeft = BottomLeft2;
                return new Vector3(bottomLeft.X, bottomLeft.Y, _bottomLeftZ);
            }
        }

        private float _width;
        /// <summary>
        /// Width of the object.
        /// </summary>
        public float Width
        {
            get { return _width; }
        }

        private float _height;
        /// <summary>
        /// Height of the object.
        /// </summary>
        public float Height
        {
            get { return _height; }
        }

        /// <summary>
        /// Helper variable to calculate the distance moved.
        /// </summary>
        private static Vector2 OriginZero = Vector2.Zero;

        protected float Velocity;

        public MovableObject(Vector3 position) //ToDo: Add SpriteNumber(s), Width, Height, StartUpRotation
        {
            _position3 = position;

            //NEW 04.03.2010
            _topLeftZ = position.Z;
            _topRightZ = position.Z;
            _bottomRightZ = position.Z;
            _bottomLeftZ = position.Z;

            if (ObjectCreated != null)
                ObjectCreated(this, new GenericEventArgs<MovableObject>(this));
        }

        /// <summary>
        /// Temporary helper method to set the width and height. Will be moved to the constructor in the future.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetDimension(float width, float height) //ToDo: move to constructor
        {
            _width = width;
            _height = height;
        }


        /// <summary>
        /// Moves the object forward or backwards and changes the rotation angle.
        /// </summary>
        /// <param name="forwardChange">Positive values mean 'go forward', negative 'go backward'</param>
        /// <param name="rotationChange">ToDo</param>
        public void Move(float forwardChange, float rotationChange)
        {
            if (forwardChange < 0) //Backwards
                rotationChange *= -1;


            float rotationAngleNew = RotationAngle;
            rotationAngleNew += MathHelper.ToRadians(rotationChange);
            //RotationAngle += MathHelper.ToRadians(rotationChange);
            Vector2 direction = RotatePoint(new Vector2(0, forwardChange), OriginZero, rotationAngleNew);

            //Collisions check here

            if (forwardChange != 0)
            {
                //if (CheckCollision(ref direction))
                //    return;
                CheckCollision(ref direction);
                RotationAngle = rotationAngleNew;

                //direction.X = (float) Math.Round(direction.X, 4);
                //direction.Y = (float)Math.Round(direction.Y, 4);

                float axis1 = MathHelper.Lerp(_topLeftZ, _bottomRightZ, 0.5f);
                float axis2 = MathHelper.Lerp(_topRightZ, _bottomLeftZ, 0.5f);
                float weightedHeight = MathHelper.Lerp(axis1, axis2, 0.5f);

                Position3 = new Vector3(Position3.X + direction.X, Position3.Y + direction.Y, weightedHeight);
                //ChangePosition(direction.X, direction.Y);

                //NEW 04.03.2010
                //_topLeftZ = Position3.Z;
                //_topRightZ = Position3.Z;
                //_bottomRightZ = Position3.Z;
                //_bottomLeftZ = Position3.Z;


                if (PositionChanged != null)
                    PositionChanged(this, EventArgs.Empty);
            }
        }


        private void CheckCollision(ref Vector2 direction)
        {
            Vector2 newTopLeft = TopLeft2 + direction;
            Vector2 newTopRight = TopRight2 + direction;
            Vector2 newBottomRight = BottomRight2 + direction;
            Vector2 newBottomLeft = BottomLeft2 + direction;

            int minBlockX = (int)newTopLeft.X;
            int maxBlockX = (int)newTopLeft.X;
            int minBlockY = (int)newTopLeft.Y;
            int maxBlockY = (int)newTopLeft.Y;
            SetMinMax(ref minBlockX, ref maxBlockX, ref minBlockY, ref maxBlockY, newTopLeft);
            SetMinMax(ref minBlockX, ref maxBlockX, ref minBlockY, ref maxBlockY, newTopRight);
            SetMinMax(ref minBlockX, ref maxBlockX, ref minBlockY, ref maxBlockY, newBottomRight);
            SetMinMax(ref minBlockX, ref maxBlockX, ref minBlockY, ref maxBlockY, newBottomLeft);


            int minBlockZ = (int) Position3.Z - 1;
            int maxBlockZ = (int) Position3.Z + 1;
            if (minBlockZ < 0)
                minBlockZ = 0;
            if (maxBlockZ > 7)
                maxBlockZ = 7;

            //minBlockZ = 1;
            //maxBlockZ = 1;

            for (int x = minBlockX; x < maxBlockX + 1; x++)
            {
                for (int y = minBlockY; y < maxBlockY + 1; y++)
                {
                    for (int z = minBlockZ; z < maxBlockZ + 1; z++)
                    {
                        BlockInfo block = MainGame.Map.CityBlocks[x, y, z];
                        //if (x == 48 && y == 184)
                        //    System.Diagnostics.Debug.WriteLine("OK");
                        bool movableSlope = false;
                        if (!ProcessBlock(ref block, ref x, ref y, ref z, ref movableSlope))
                            continue;
                        if (!block.LidOnly || movableSlope)
                        {
                            Polygon polygonA = new Polygon(); //new Rectangle
                            polygonA.Points.Add(newTopLeft);
                            polygonA.Points.Add(newTopRight);
                            polygonA.Points.Add(newBottomRight);
                            polygonA.Points.Add(newBottomLeft);

                            Polygon polygonB = CreatePolygon(ref block, ref x, ref y);

                            SeparatingAxisTheorem.PolygonCollisionResult resNew =
                                SeparatingAxisTheorem.PolygonCollision(ref polygonA, ref polygonB, ref direction);
                            if (resNew.Intersect)
                            {
                                //if (Math.Abs(direction.X) > Math.Abs(direction.Y))
                                //{
                                //    if (direction.X > 0)
                                //        System.Diagnostics.Debug.WriteLine("Collision left");
                                //    else if (direction.X < 0)
                                //        System.Diagnostics.Debug.WriteLine("Collision right");
                                //}
                                //else
                                //{

                                //    if (direction.Y > 0)
                                //        System.Diagnostics.Debug.WriteLine("Collision top");
                                //    else if (direction.Y < 0)
                                //        System.Diagnostics.Debug.WriteLine("Collision bottom");
                                //}
                                if (block.IsLowSlope || block.IsHighSlope)
                                {
                                    _topLeftZ = MainGame.GetHighestPointF(newTopLeft.X, newTopLeft.Y);
                                    _topRightZ = MainGame.GetHighestPointF(newTopRight.X, newTopRight.Y);
                                    _bottomRightZ = MainGame.GetHighestPointF(newBottomRight.X, newBottomRight.Y);
                                    _bottomLeftZ = MainGame.GetHighestPointF(newBottomLeft.X, newBottomLeft.Y);
                                    //Position3.Z = Position3.Z++;
                                    return;
                                }

                                //direction.X = 0;
                                //direction.Y = 0;
                                direction.X = resNew.MinimumTranslationVector.X;
                                direction.Y = resNew.MinimumTranslationVector.Y;
                                return;
                            }
                        }
                        _topLeftZ = z;
                        _topRightZ = z;
                        _bottomRightZ = z;
                        _bottomLeftZ = z;
                    }
                }
            }
        }

        private static void SetMinMax(ref int minBlockX, ref int maxBlockX, ref int minBlockY, ref int maxBlockY, Vector2 currentVector)
        {
            if (currentVector.X < minBlockX)
                minBlockX = (int)currentVector.X;
            if (currentVector.X > maxBlockX)
                maxBlockX = (int)currentVector.X;
            if (currentVector.Y < minBlockY)
                minBlockY = (int)currentVector.Y;
            if (currentVector.Y > maxBlockY)
                maxBlockY = (int)currentVector.Y;
        }

        private bool ProcessBlock(ref BlockInfo block, ref int x, ref int y, ref int z, ref bool movableSlope)
        {
            if (x == 48 || x == 78)
            {
                System.Diagnostics.Debug.WriteLine("OK");
                //if (z == 2)
                //{
                //    movableSlope = true;
                //    return true;
                //}
                return false;
            }

            movableSlope = false;
            int currentZ = (int) Position3.Z;
            
            BlockInfo blockAbove = MainGame.Map.CityBlocks[x, y, currentZ + 1];

            //if above block is a low slope, ignore equal z
            if (z == currentZ && !blockAbove.IsLowSlope && !blockAbove.IsHighSlope)
                return true;

            //check 1 above only if it's a diagnoal or a low slope
            SlopeType slope = block.SlopeType;
            if (z == currentZ + 1)
            {
                if (slope == SlopeType.DiagonalFacingDownLeft || slope == SlopeType.DiagonalFacingDownRight ||
                    slope == SlopeType.DiagonalFacingUpLeft || slope == SlopeType.DiagonalFacingUpRight)
                    return true;
                if (block.IsLowSlope || block.IsHighSlope)
                {
                    movableSlope = true;
                    return true;
                }
            }

            return false;
        }

        //private static bool IsLowSlope(ref BlockInfo block)
        //{
        //    return block.SlopeType == SlopeType.Up26Low || block.SlopeType == SlopeType.Down26Low || block.SlopeType == SlopeType.Left26Low || block.SlopeType == SlopeType.Right26Low;
        //}

        private static Polygon CreatePolygon(ref BlockInfo block, ref int x, ref int y)
        {
            Polygon polygon = new Polygon();
            SlopeType slope = block.SlopeType;

            switch (slope)
            {
                case SlopeType.None:
                    polygon.Points.Add(new Vector2(x, y));
                    polygon.Points.Add(new Vector2(x + 1, y));
                    polygon.Points.Add(new Vector2(x + 1, y + 1));
                    polygon.Points.Add(new Vector2(x, y + 1));
                    break;
                case SlopeType.DiagonalFacingDownLeft:
                    polygon.Points.Add(new Vector2(x, y));
                    polygon.Points.Add(new Vector2(x + 1, y));
                    polygon.Points.Add(new Vector2(x + 1, y + 1));
                    break;
                case SlopeType.DiagonalFacingDownRight:
                    polygon.Points.Add(new Vector2(x, y));
                    polygon.Points.Add(new Vector2(x + 1, y));
                    polygon.Points.Add(new Vector2(x, y + 1));
                    break;
                case SlopeType.DiagonalFacingUpLeft:
                    polygon.Points.Add(new Vector2(x + 1, y));
                    polygon.Points.Add(new Vector2(x + 1, y + 1));
                    polygon.Points.Add(new Vector2(x, y + 1));
                    break;
                case SlopeType.DiagonalFacingUpRight:
                    polygon.Points.Add(new Vector2(x, y));
                    polygon.Points.Add(new Vector2(x + 1, y + 1));
                    polygon.Points.Add(new Vector2(x, y + 1));
                    break;
                default: //ToDo: implement all slopes!
                    polygon.Points.Add(new Vector2(x, y));
                    polygon.Points.Add(new Vector2(x + 1, y));
                    polygon.Points.Add(new Vector2(x + 1, y + 1));
                    polygon.Points.Add(new Vector2(x, y + 1));
                    break;
            }
            return polygon;
        }

        /// <summary>
        /// Rotate a point from a given location and adjust using the Origin we
        /// are rotating around
        /// </summary>
        /// <param name="point">The point to rotate.</param>
        /// <param name="origin">Used origin for the rotation.</param>
        /// <param name="rotation">Angle in radians.</param>
        /// <returns></returns>
        public Vector2 RotatePoint(Vector2 point, Vector2 origin, float rotation)
        {
            Vector2 aTranslatedPoint = new Vector2();
            aTranslatedPoint.X = (float)(origin.X + (point.X - origin.X) * Math.Cos(rotation)
                - (point.Y - origin.Y) * Math.Sin(rotation));
            aTranslatedPoint.Y = (float)(origin.Y + (point.Y - origin.Y) * Math.Cos(rotation)
                + (point.X - origin.X) * Math.Sin(rotation));
            return aTranslatedPoint;
        }
    }
}
