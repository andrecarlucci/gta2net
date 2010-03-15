//Created: 16.02.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiale.GTA2NET.Core.Map;
using Microsoft.Xna.Framework;
using Hiale.GTA2NET.Helper;

namespace Hiale.GTA2NET.Logic
{
    public class MovableObject
    {
        public static event EventHandler<GenericEventArgs<MovableObject>> ObjectCreated;
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
            protected set
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

        protected float Velocity;

        protected MovableObject(Vector3 startUpPosition, float width, float height)
        {
            Position3 = startUpPosition;
            Width = width / 64;
            WidthHalf = Width / 2;
            Height = height / 64;
            HeightHalf = Height / 2;
        }

        protected void CreateSprite()
        {
            if (ObjectCreated != null)
                ObjectCreated(this, new GenericEventArgs<MovableObject>(this));
        }

        /// <summary>
        /// Moves the object forward or backwards and changes the rotation angle.
        /// </summary>
        /// <param name="forwardDelta">Positive values mean 'go forward', negative 'go backward'</param>
        /// <param name="rotationDelta">Change of the rotation angle</param>
        /// <param name="elapsedGameTime">The amount of elapsedGameTime since the last update</param>
        public void Move(ref float forwardDelta, ref float rotationDelta, ref float elapsedGameTime)
        {
            if (forwardDelta < 0) //Backwards
                rotationDelta *= -1;

            if (forwardDelta == 0)
                return;

            float rotationAngleNew = RotationAngle;
            rotationAngleNew += MathHelper.ToRadians(rotationDelta);
            Vector2 direction = MainGame.RotatePoint(new Vector2(0, forwardDelta), OriginZero, rotationAngleNew);

            //Bugs:
            //Low/High Slops only allow from proper edge!

            Vector2 topLeft = TopLeft2; //Create these for cache
            Vector2 topRight = TopRight2;
            Vector2 bottomRight = BottomRight2;
            Vector2 bottomLeft = BottomLeft2;
            Vector2 before = direction;
            if (Collide(ref direction, ref topLeft, ref topRight, ref bottomRight, ref bottomLeft))
                return;

            //Culculate height, check all 4 points of the object and take the maximum value of those.
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

            RotationAngle = rotationAngleNew;

            float newPositionX = Position3.X + direction.X;
            float newPositionY = Position3.Y + direction.Y;
            //ApplyGravity(ref newPositionX, ref newPositionY, ref weightedHeight);
            Position3 = new Vector3(newPositionX, newPositionY, maxZ);
            //MainGame.WindowTitle = maxZ.ToString();

            if (PositionChanged != null)
                PositionChanged(this, EventArgs.Empty);
        }

        private bool Collide(ref Vector2 direction, ref Vector2 topLeft, ref Vector2 topRight, ref Vector2 bottomRight, ref Vector2 bottomLeft)
        {
            //calculate the tentative new position of each point
            Vector2 newTopLeft = topLeft + direction;
            Vector2 newTopRight = topRight + direction;
            Vector2 newBottomRight = bottomRight + direction;
            Vector2 newBottomLeft = bottomLeft + direction;

            //find all correspondig block coordinates (x & y) which the object is laying on.
            int minBlockX = (int)newTopLeft.X;
            int maxBlockX = minBlockX;
            int minBlockY = (int)newTopLeft.Y;
            int maxBlockY = minBlockY;
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

            //if we go through a narrow passage, it can happen that the object collides on both sides and implact them mutual
            //for the moment we immediately stop the object then
            bool topCollision = false;
            bool bottomCollision = false;
            bool leftCollision = false;
            bool rightCollision = false;

            for (int x = minBlockX; x < maxBlockX + 1; x++)
            {
                for (int y = minBlockY; y < maxBlockY + 1; y++)
                {
                    BlockInfo block = MainGame.Map.CityBlocks[x, y, z];
                    if (block.IsEmpty)
                        continue;
                    Polygon polygonObject = new Polygon();
                    polygonObject.Points.Add(newTopLeft);
                    polygonObject.Points.Add(newTopRight);
                    polygonObject.Points.Add(newBottomRight);
                    polygonObject.Points.Add(newBottomLeft);
                    CheckBlock(ref x, ref y, ref block, ref polygonObject, ref direction, ref topCollision, ref bottomCollision, ref leftCollision, ref rightCollision);
                }
            }

            if ((topCollision && bottomCollision) || (leftCollision && rightCollision))
                return true;

            return false;
        }

        private void CheckBlock(ref int x, ref int y, ref BlockInfo block, ref Polygon polygonObject, ref Vector2 direction, ref bool topCollision, ref bool bottomCollision, ref bool leftCollision, ref bool rightCollision)
        {
            PolygonCollisionResult topCollisionResult;
            PolygonCollisionResult bottomCollisionResult;
            PolygonCollisionResult leftCollisionResult;
            PolygonCollisionResult rightCollisionResult;

            Vector2 directionCopy = direction;

            if (block.Top.TileNumber > 0)
            {
                Polygon polygonTop = CreatePolygon(ref x, ref y, ref block, SlopeDirection.Up);
                if (polygonTop.Points.Count > 0)
                {
                    topCollisionResult = SeparatingAxisTheorem.PolygonCollision(ref polygonObject, ref polygonTop, ref directionCopy);
                    if (topCollisionResult.Intersect)
                    {
                        direction += topCollisionResult.MinimumTranslationVector;
                        topCollision = true;
                    }
                }
            }

            if (block.Bottom.TileNumber > 0)
            {
                Polygon polygonBottom = CreatePolygon(ref x, ref y, ref block, SlopeDirection.Down);
                if (polygonBottom.Points.Count > 0)
                {
                    bottomCollisionResult = SeparatingAxisTheorem.PolygonCollision(ref polygonObject, ref polygonBottom, ref directionCopy);
                    if (bottomCollisionResult.Intersect)
                    {
                        direction += bottomCollisionResult.MinimumTranslationVector;
                        bottomCollision = true;
                    }
                }
            }

            if (block.Left.TileNumber > 0)
            {
                Polygon polygonLeft = CreatePolygon(ref x, ref y, ref block, SlopeDirection.Left);
                if (polygonLeft.Points.Count > 0)
                {
                    leftCollisionResult = SeparatingAxisTheorem.PolygonCollision(ref polygonObject, ref polygonLeft, ref directionCopy);
                    if (leftCollisionResult.Intersect)
                    {
                        direction += leftCollisionResult.MinimumTranslationVector;
                        leftCollision = true;
                    }
                }
            }

            if (block.Right.TileNumber > 0)
            {
                Polygon polygonRight = CreatePolygon(ref x, ref y, ref block, SlopeDirection.Right);
                if (polygonRight.Points.Count > 0)
                {
                    rightCollisionResult = SeparatingAxisTheorem.PolygonCollision(ref polygonObject, ref polygonRight, ref directionCopy);
                    if (rightCollisionResult.Intersect)
                    {
                        direction += rightCollisionResult.MinimumTranslationVector;
                        rightCollision = true;
                    }
                }
            }
        }

        private Polygon CreatePolygon(ref int x, ref int y, ref BlockInfo block, SlopeDirection direction)
        {
            Polygon polygon = new Polygon();
            switch (direction)
            {
                case SlopeDirection.Up:
                    if (!block.IsDiagonalSlope)
                    {
                        polygon.Points.Add(new Vector2(x, y));
                        polygon.Points.Add(new Vector2(x + 1, y));
                    }
                    break;
                case SlopeDirection.Down:
                    if (!block.IsDiagonalSlope)
                    {
                        polygon.Points.Add(new Vector2(x + 1, y + 1));
                        polygon.Points.Add(new Vector2(x, y + 1));
                    }
                    break;
                case SlopeDirection.Left:
                    if (!block.IsDiagonalSlope)
                    {
                        polygon.Points.Add(new Vector2(x, y + 1));
                        polygon.Points.Add(new Vector2(x, y));
                    }
                    else if (block.SlopeType == SlopeType.DiagonalFacingUpLeft)
                    {
                        polygon.Points.Add(new Vector2(x, y + 1));
                        polygon.Points.Add(new Vector2(x + 1, y));
                    }
                    else if (block.SlopeType == SlopeType.DiagonalFacingDownLeft)
                    {
                        polygon.Points.Add(new Vector2(x + 1, y + 1));
                        polygon.Points.Add(new Vector2(x, y));
                    }
                    break;
                case SlopeDirection.Right:
                    if (!block.IsDiagonalSlope)
                    {
                        polygon.Points.Add(new Vector2(x + 1, y));
                        polygon.Points.Add(new Vector2(x + 1, y + 1));
                    }
                    else if (block.SlopeType == SlopeType.DiagonalFacingUpRight)
                    {
                        polygon.Points.Add(new Vector2(x + 1, y + 1));
                        polygon.Points.Add(new Vector2(x, y));
                    }
                    else if (block.SlopeType == SlopeType.DiagonalFacingDownRight)
                    {
                        polygon.Points.Add(new Vector2(x + 1, y));
                        polygon.Points.Add(new Vector2(x, y + 1));
                    }
                    break;
            }

            return polygon;
        }

        private void ApplyGravity(ref float x, ref float y, ref float z)
        {
            //if (z == 0)
            //    return;
            //BlockInfo block = MainGame.Map.CityBlocks[(int)x, (int)y, (int)z];
            //if (block.Lid != null && block.Lid.TileNumber > 0)
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

        private static Polygon CreateBlockPolygon(ref BlockInfo block, ref int x, ref int y)
        {
            Polygon polygon = new Polygon();
            SlopeType slope = block.SlopeType;

            switch (slope)
            {
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
