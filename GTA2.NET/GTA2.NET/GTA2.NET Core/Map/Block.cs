// GTA2.NET
// 
// File: Block.cs
// Created: 17.01.2010
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

using Hiale.GTA2NET.Core.Collision;
using Hiale.GTA2NET.Core.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Hiale.GTA2NET.Core.Map
{
    /// <summary>
    ///     Represents the block structure in the original GTA map files.
    ///     For more information see GTA2 Map Format.
    /// </summary>
    public struct BlockStructure
    {
        public byte Arrows;
        public ushort Bottom;
        public ushort Left;
        public ushort Lid;
        public ushort Right;
        public byte SlopeType;
        public ushort Top;
    }

    /// <summary>
    ///     Represents a Block from the Map.
    /// </summary>
    public abstract class Block
    {

        /// <summary>
        /// Size of a PartialBlocks
        /// </summary>
        static protected float PartialBlockScalar = 0.375f;
        /// <summary>
        /// Size of the blocks
        /// </summary>
        static protected Vector3 GlobalScalar = new Vector3(1, 1, 0.5f);
        /// <summary>
        /// Used to retrive the Texture to the faces of block.
        /// </summary>
        static public Textures textures;


        protected BlockStructure BlockStructure;

        protected Block(BlockStructure blockStructure, Vector3 pos)
        {
            BlockStructure = blockStructure;
            Left = new BlockFaceEdge(blockStructure.Left);
            Right = new BlockFaceEdge(blockStructure.Right);
            Top = new BlockFaceEdge(blockStructure.Top);
            Bottom = new BlockFaceEdge(blockStructure.Bottom);
            Lid = new BlockFaceLid(blockStructure.Lid);
            Arrows = (RoadTrafficType) blockStructure.Arrows;
            ParseSlope(blockStructure.SlopeType);
            Position = pos;
            Coors = new List<VertexPositionNormalTexture>();
            IndexBufferCollection = new List<int>();
            Left.FixTileWallCollision();
            Right.FixTileWallCollision();
            Top.FixTileWallCollision();
            Bottom.FixTileWallCollision();
        }

        protected Block()
        {
            Left = BlockFaceEdge.Empty;
            Right = BlockFaceEdge.Empty;
            Top = BlockFaceEdge.Empty;
            Bottom = BlockFaceEdge.Empty;
            Lid = BlockFaceLid.Empty;

            Coors = new List<VertexPositionNormalTexture>();
            IndexBufferCollection = new List<int>();
        }

        /// <summary>
        /// Position of this block in a map.
        /// </summary>
        public Vector3 Position { get; set; }

        public BlockFaceEdge Left { get; set; }

        public BlockFaceEdge Right { get; set; }

        public BlockFaceEdge Top { get; set; }

        public BlockFaceEdge Bottom { get; set; }

        public BlockFaceLid Lid { get; set; }

        public GroundType GroundType { get; private set; }

        public SlopeType SlopeType { get; protected set; }

        public List<VertexPositionNormalTexture> Coors { get; protected set; }

        public List<int> IndexBufferCollection { get; protected set; }

        public virtual bool IsEmpty
        {
            get { return (Left == null && Right == null && Top == null && Bottom == null && Lid == null) || ((!Left && !Right && !Top && !Bottom && !Lid)); }
        }

        //the player can walk on these
        public bool IsMovableSlope
        {
            get
            {
                var slope = (byte) SlopeType;
                return slope != 0 && slope < 41;
            }
        }

        public SlopeDirection SlopeDirection
        {
            get
            {
                switch (SlopeType)
                {
                    case SlopeType.Up26Low:
                    case SlopeType.Up26High:
                    case SlopeType.Up7Low:
                    case SlopeType.Up7High0:
                    case SlopeType.Up7High1:
                    case SlopeType.Up7High2:
                    case SlopeType.Up7High3:
                    case SlopeType.Up7High4:
                    case SlopeType.Up7High5:
                    case SlopeType.Up7High6:
                    case SlopeType.Up45:
                        return SlopeDirection.Up;
                    case SlopeType.Down26Low:
                    case SlopeType.Down26High:
                    case SlopeType.Down7Low:
                    case SlopeType.Down7High0:
                    case SlopeType.Down7High1:
                    case SlopeType.Down7High2:
                    case SlopeType.Down7High3:
                    case SlopeType.Down7High4:
                    case SlopeType.Down7High5:
                    case SlopeType.Down7High6:
                    case SlopeType.Down45:
                        return SlopeDirection.Down;
                    case SlopeType.Left26Low:
                    case SlopeType.Left26High:
                    case SlopeType.Left7Low:
                    case SlopeType.Left7High0:
                    case SlopeType.Left7High1:
                    case SlopeType.Left7High2:
                    case SlopeType.Left7High3:
                    case SlopeType.Left7High4:
                    case SlopeType.Left7High5:
                    case SlopeType.Left7High6:
                    case SlopeType.Left45:
                        return SlopeDirection.Left;
                    case SlopeType.Right26Low:
                    case SlopeType.Right26High:
                    case SlopeType.Right7Low:
                    case SlopeType.Right7High0:
                    case SlopeType.Right7High1:
                    case SlopeType.Right7High2:
                    case SlopeType.Right7High3:
                    case SlopeType.Right7High4:
                    case SlopeType.Right7High5:
                    case SlopeType.Right7High6:
                    case SlopeType.Right45:
                        return SlopeDirection.Right;
                }
                return SlopeDirection.None;
            }
        }

        /// <summary>
        ///     Makes a copy of this object.
        /// </summary>
        /// <returns>The new copy.</returns>
        public virtual Block DeepCopy()
        {
            return DeepCopy(BlockStructure, Position);
        }

        /// <summary>
        ///     Makes a copy of this object.
        /// </summary>
        /// <param name="blockStructure">The BlockStructure</param>
        /// <param name="pos">The position</param>
        /// <returns>The new copy</returns>
        public virtual Block DeepCopy(BlockStructure blockStructure, Vector3 pos)
        {
            var arguments = new object[2];
            arguments[0] = blockStructure;
            arguments[1] = pos;
            var block = (Block) Activator.CreateInstance(GetType(), arguments);
            block.SlopeType = SlopeType;
            return block;
        }

        /// <summary>
        ///     Test if this instance represents the slopeType
        /// </summary>
        /// <param name="slopeType">SlopeType to test</param>
        /// <returns>True </returns>
        public virtual bool IsSlopeOf(SlopeType slopeType)
        {
            return (SlopeType == slopeType);
        }

        /// <summary>
        ///     Calculate the coordinates of the verticies.
        /// </summary>
        public abstract void SetUpCube();

        public void ParseSlope(byte type)
        {
            if (type == 0)
                return;

            int groundType = 0;
            groundType += (type & 1);
            groundType += (type & 2);
            GroundType = (GroundType) groundType;

            if (type < 4)
                return;

            int slopeType = 0;
            for (int i = 2; i < 8; i++)
            {
                if (BitHelper.CheckBit(type, i))
                    slopeType += (int) Math.Pow(2, i - 2);
            }
            SlopeType = (SlopeType) slopeType;

            //make some corrections to the collision bits.
            switch (SlopeType)
            {
                case SlopeType.DiagonalFacingUpLeft:
                case SlopeType.DiagonalFacingDownLeft:
                case SlopeType.DiagonalSlopeFacingUpLeft:
                case SlopeType.DiagonalSlopeFacingDownLeft:
                case SlopeType.DiagonalFacingUpRight:
                case SlopeType.DiagonalFacingDownRight:
                case SlopeType.DiagonalSlopeFacingUpRight:
                case SlopeType.DiagonalSlopeFacingDownRight:
                case SlopeType.PartialBlockLeft:
                case SlopeType.PartialBlockRight:
                case SlopeType.PartialBlockTop:
                case SlopeType.PartialBlockBottom:
                case SlopeType.PartialBlockTopLeft:
                case SlopeType.PartialBlockTopRight:
                case SlopeType.PartialBlockBottomRight:
                case SlopeType.PartialBlockBottomLeft:
                case SlopeType.PartialCentreBlock:
                    Top.Wall = Top;
                    Top.BulletWall = Top;
                    Bottom.Wall = Bottom;
                    Bottom.BulletWall = Bottom;
                    Left.Wall = Left;
                    Left.BulletWall = Left;
                    Right.Wall = Right;
                    Right.BulletWall = Right;
                    break;
            }
        }

        public override string ToString()
        {
            var positionString = Position == Vector3.Zero ? String.Empty : " Position: " + Position;
            if (IsEmpty)
                return "[empty block]" + positionString;
            return "Type: " + SlopeType + positionString + " Lid: " + Lid + " Left: " + Left + " Top: " + Top + " Right: " + Right + " Bottom: " + Bottom + " Ground: " + GroundType;
        }

        

        protected virtual ILineObstacle GetDefaultLeftCollison()
        {
            return LineObstacle.DefaultLeft((int) Position.X, (int) Position.Y, (int) Position.Z);
        }

        protected virtual ILineObstacle GetDefaultTopCollison()
        {
            return LineObstacle.DefaultTop((int)Position.X, (int)Position.Y, (int)Position.Z);
        }

        protected virtual ILineObstacle GetDefaultRightCollison()
        {
            return LineObstacle.DefaultRight((int)Position.X, (int)Position.Y, (int)Position.Z);
        }

        protected virtual ILineObstacle GetDefaultBottomCollison()
        {
            return LineObstacle.DefaultBottom((int)Position.X, (int)Position.Y, (int)Position.Z);
        }

        #region Not Implemented

        /// <summary>
        ///     ToDo
        /// </summary>
        public RoadTrafficType Arrows { get; set; }

        ///// <summary>
        ///// ToDo - enum
        ///// </summary>
        //public byte BaseSlopeType { get; private set; }

        //public bool IsDiagonalSlope
        //{
        //    get
        //    {
        //        return SlopeType == SlopeType.DiagonalFacingDownLeft || SlopeType == SlopeType.DiagonalFacingDownRight || SlopeType == SlopeType.DiagonalFacingUpLeft || SlopeType == SlopeType.DiagonalFacingUpRight;
        //    }
        //}

        //public bool IsLowSlope
        //{
        //    get
        //    {
        //        return SlopeType == SlopeType.Up26Low || SlopeType == SlopeType.Down26Low || SlopeType == SlopeType.Left26Low || SlopeType == SlopeType.Right26Low;
        //    }
        //}

        //public bool IsHighSlope
        //{
        //    get
        //    {
        //        return SlopeType == SlopeType.Up26High || SlopeType == SlopeType.Down26High || SlopeType == SlopeType.Left26High || SlopeType == SlopeType.Right26High;
        //    }
        //}

        //public bool IsAir()
        //{
        //    //int Bit0 = SlopeType & 1;
        //    //int Bit1 = SlopeType & 2;
        //    return ((SlopeType & 1) != 1) && ((SlopeType & 2) != 2); //Bit 0 and Bit 1 are both 0
        //}

        //public bool IsRoad()
        //{
        //    return ((SlopeType & 1) == 1) && ((SlopeType & 2) != 2); //Bit 0 is 1 and Bit 1 is 0
        //}

        //public bool IsPavement()
        //{
        //    return ((SlopeType & 1) != 1) && ((SlopeType & 2) == 2); //Bit 0 is 0 and Bit 1 is 1
        //}

        //public bool IsField()
        //{
        //    return ((SlopeType & 1) == 1) && ((SlopeType & 2) == 2); //Bit 0 and Bit 1 are both 1
        //}

        #endregion

        #region Not Used



        //Created but never used.
        public byte SlopeSubTyp
        {
            get
            {
                var slope = (byte) SlopeType;
                if (slope > 0 && slope < 9)
                    return 26;
                if (slope > 8 && slope < 41)
                    return 7;
                if (slope > 40 && slope < 45)
                    return 45;
                return 0;
            }
        }

        public bool IsDiagonalSlope
        {
            get
            {
                var slope = (byte) SlopeType;
                return slope >= 45 && slope <= 48;
            }
        }

        public bool IsLidOnly
        {
            get
            {
                if (IsEmpty)
                    return false;
                return Lid != null && Lid && (Left == null || !Left) && (Right == null || !Right) && (Top == null || !Top) && (Bottom == null || !Bottom);
            }
        }

        #endregion

        #region Cube

        /// <summary>
        /// Creates the Lid vertices Coordinates.
        /// </summary>
        /// <param name="lidCoords">The lid coordinates.</param>
        /// <param name="texture">The lid texture</param>
        protected void CreateFrontVertices(FaceCoordinates lidCoords, Vector2[] texture)
        {
            if (!Lid)
                return;
            
            Coors.Add(new VertexPositionNormalTexture(lidCoords.TopRight, Vector3.Zero, texture[2]));
            Coors.Add(new VertexPositionNormalTexture(lidCoords.BottomRight, Vector3.Zero, texture[1]));
            Coors.Add(new VertexPositionNormalTexture(lidCoords.TopLeft, Vector3.Zero, texture[3]));
            Coors.Add(new VertexPositionNormalTexture(lidCoords.BottomLeft, Vector3.Zero, texture[0]));

            int startIndex = Coors.Count - 4;
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex + 1);
            IndexBufferCollection.Add(startIndex + 2);
            IndexBufferCollection.Add(startIndex + 1);
            IndexBufferCollection.Add(startIndex + 3);
            IndexBufferCollection.Add(startIndex + 2);
        }

        /// <summary>
        /// Creates the Top vertices Coordinates.
        /// </summary>
        /// <param name="frontCoords"></param>
        /// <param name="backCoords"></param>
        protected void CreateTopVertices(FaceCoordinates frontCoords, FaceCoordinates backCoords, Vector2[] texture)
        {
            if (!Top)
                return;

            Coors.Add(new VertexPositionNormalTexture(frontCoords.TopRight, Vector3.Zero, texture[2]));
            Coors.Add(new VertexPositionNormalTexture(backCoords.TopLeft, Vector3.Zero, texture[0]));
            Coors.Add(new VertexPositionNormalTexture(backCoords.TopRight, Vector3.Zero, texture[1]));
            Coors.Add(new VertexPositionNormalTexture(frontCoords.TopLeft, Vector3.Zero, texture[3]));

            int startIndex = Coors.Count - 4;
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex + 1);
            IndexBufferCollection.Add(startIndex + 2);
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex + 3);
            IndexBufferCollection.Add(startIndex + 1);
        }

        protected void CreateBottomVertices(FaceCoordinates frontCoords, FaceCoordinates backCoords, Vector2[] texture)
        {
            if (!Bottom)
                return;

            Coors.Add(new VertexPositionNormalTexture(frontCoords.BottomRight, Vector3.Zero, texture[2]));
            Coors.Add(new VertexPositionNormalTexture(backCoords.BottomRight, Vector3.Zero, texture[1]));
            Coors.Add(new VertexPositionNormalTexture(backCoords.BottomLeft, Vector3.Zero, texture[0]));
            Coors.Add(new VertexPositionNormalTexture(frontCoords.BottomLeft, Vector3.Zero, texture[3]));

            int startIndex = Coors.Count - 4;
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex + 1);
            IndexBufferCollection.Add(startIndex + 2);
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex + 2);
            IndexBufferCollection.Add(startIndex + 3);
        }

        protected void CreateLeftVertices(FaceCoordinates frontCoords, FaceCoordinates backCoords, Vector2[] texture)
        {
            if (!Left)
                return;

            Coors.Add(new VertexPositionNormalTexture(frontCoords.TopLeft, Vector3.Zero, texture[3]));
            Coors.Add(new VertexPositionNormalTexture(backCoords.BottomLeft, Vector3.Zero, texture[1]));
            Coors.Add(new VertexPositionNormalTexture(frontCoords.BottomLeft, Vector3.Zero, texture[2]));
            Coors.Add(new VertexPositionNormalTexture(backCoords.TopLeft, Vector3.Zero, texture[0]));

            //Left also has a strange index buffer order...
            int startIndex = Coors.Count - 4;
            IndexBufferCollection.Add(startIndex + 2);
            IndexBufferCollection.Add(startIndex + 1);
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex + 1);
            IndexBufferCollection.Add(startIndex + 3);
            IndexBufferCollection.Add(startIndex);
        }

        protected void CreateRightVertices(FaceCoordinates frontCoords, FaceCoordinates backCoords, Vector2[] texture)
        {
            if (!Right)
                return;

            Coors.Add(new VertexPositionNormalTexture(frontCoords.TopRight, Vector3.Zero, texture[2]));
            Coors.Add(new VertexPositionNormalTexture(frontCoords.BottomRight, Vector3.Zero, texture[3]));
            Coors.Add(new VertexPositionNormalTexture(backCoords.BottomRight, Vector3.Zero, texture[0]));
            Coors.Add(new VertexPositionNormalTexture(backCoords.TopRight, Vector3.Zero, texture[1]));

            //...
            int startIndex = Coors.Count - 4;
            IndexBufferCollection.Add(startIndex + 2);
            IndexBufferCollection.Add(startIndex + 1);
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex + 3);
            IndexBufferCollection.Add(startIndex + 2);
        }

        #endregion

        protected static Vector2 GetCenterPosition(ref Vector2 lowerEnd, ref Vector2 higherEnd, float amount) //ToDo: method name
        {
            Vector2 center;
            center.X = MathHelper.Lerp(lowerEnd.X, higherEnd.X, amount);
            center.Y = MathHelper.Lerp(lowerEnd.Y, higherEnd.Y, amount);
            return center;
        }

        /// <summary>
        ///     Roteate a slope by 90° (rotation = 1), 180° (rotation = 2) or 270° (rotation = 3)
        /// </summary>
        /// <param name="frontCoordinates"></param>
        /// <param name="rotation"></param>
        protected FaceCoordinates RotateSlope(FaceCoordinates frontCoordinates, byte rotation)
        {
            for (int i = 0; i < rotation; i++)
            {
                Vector3 topLeft = frontCoordinates.BottomLeft;
                Vector3 topRight = frontCoordinates.TopLeft;
                Vector3 bottomRight = frontCoordinates.TopRight;
                Vector3 bottomLeft = frontCoordinates.BottomRight;

                frontCoordinates.TopLeft = topLeft;
                frontCoordinates.TopRight = topRight;
                frontCoordinates.BottomRight = bottomRight;
                frontCoordinates.BottomLeft = bottomLeft;
            }
            return frontCoordinates;
        }

        protected static RotationType RotateEnum(RotationType type, byte rotation)
        {
            for (int i = 0; i < rotation; i++)
            {
                switch (type)
                {
                    case RotationType.RotateNone:
                        type = RotationType.Rotate90;
                        break;
                    case RotationType.Rotate90:
                        type = RotationType.Rotate180;
                        break;
                    case RotationType.Rotate180:
                        type = RotationType.Rotate270;
                        break;
                    case RotationType.Rotate270:
                        type = RotationType.RotateNone;
                        break;
                }
            }
            return type;
        }

        public virtual void GetCollision(List<ILineObstacle> obstacles, bool bulletWall)
        {
            if (DoesWallCollide(Left, bulletWall))
                obstacles.Add(GetDefaultLeftCollison());
            if (DoesWallCollide(Top, bulletWall))
                obstacles.Add(GetDefaultTopCollison());
            if (DoesWallCollide(Right, bulletWall))
                obstacles.Add(GetDefaultRightCollison());
            if (DoesWallCollide(Bottom, bulletWall))
                obstacles.Add(GetDefaultBottomCollison());

        }

        protected virtual bool DoesWallCollide(BlockFaceEdge blockFace, bool bulletWall)
        {
            return blockFace && (bulletWall ? blockFace.BulletWall : blockFace.Wall);
        }

        #region Coordinates

        /// <summary>
        ///     Based in the position of the Block calculate the positons of the verticies of the block.
        /// </summary>
        /// <param name="frontCoords">The coordinates of the top face of the cube</param>
        /// <param name="backCoords">The coordinates of the botton face of the cube</param>
        protected void PrepareCoordinates(out FaceCoordinates frontCoords, out FaceCoordinates backCoords)
        {
            float x = 0;
            float y = 0;
            float width = 1;
            float height = 1;
            PrepareCoordinates(x, y, width, height, out frontCoords, out backCoords);
        }

        /// <summary>
        ///     Based in the position of the Block calculate the positons of the verticies of the block.
        /// </summary>
        /// <param name="frontCoords">The coordinates of the top face of the cube</param>
        /// <param name="backCoords">The coordinates of the botton face of the cube</param>
        /// <param name="x">X position of the TopLeft corner.</param>
        /// <param name="y">Y position of the TopLeft corner.</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected void PrepareCoordinates(float x, float y, float width, float height, out FaceCoordinates frontCoords, out FaceCoordinates backCoords)
        {
            Vector3 position = Position;
            position.Y *= -1;

            //Coordinates of the cube
            Vector3 topLeftFront = (new Vector3(x, -y, 1f) + position) * GlobalScalar;
            Vector3 topRightFront = (new Vector3(x + width, -y, 1f) + position) * GlobalScalar;
            Vector3 bottomLeftFront = (new Vector3(x, -(y + height), 1f) + position) * GlobalScalar;
            Vector3 bottomRightFront = (new Vector3(x + width, -(y + height), 1f) + position) * GlobalScalar;
            frontCoords = new FaceCoordinates(topLeftFront, topRightFront, bottomRightFront, bottomLeftFront);

            Vector3 topLeftBack = (new Vector3(x, -y, 0.0f) + position) * GlobalScalar;
            Vector3 topRightBack = (new Vector3(x + width, -y, 0.0f) + position) * GlobalScalar;
            Vector3 bottomLeftBack = (new Vector3(x, -(y + height), 0.0f) + position) * GlobalScalar;
            Vector3 bottomRightBack = (new Vector3(x + width, -(y + height), 0.0f) + position) * GlobalScalar;
            backCoords = new FaceCoordinates(topLeftBack, topRightBack, bottomRightBack, bottomLeftBack);
        }

        #endregion
    }
}