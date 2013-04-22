// GTA2.NET
// 
// File: BlockInfo.cs
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
using System;
using Microsoft.Xna.Framework;
using Hiale.GTA2NET.Core.Helper;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Hiale.GTA2NET.Core.Map
{
    /// <summary>
    /// Represents the block structur in the original GTA map files.
    /// For more information see GTA2 Map Format.
    /// </summary>
    public struct blockInfo
    {
        public ushort Left, Right, Top, Bottom, Lid;
        public byte Arrows;
        public byte SlopeType;
    }


    /// <summary>
    /// Represents a Block from the Map.
    /// </summary>
    public abstract class BlockInfo
    {
        #region FaceCoordinates
        /// <summary>
        /// Represents the coordinates of the 4 vertices of each face from the block.
        /// </summary>
        protected struct FaceCoordinates
        {
            public Vector3 TopLeft;
            public Vector3 TopRight;
            public Vector3 BottomRight;
            public Vector3 BottomLeft;

            public FaceCoordinates(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft)
            {
                TopLeft = topLeft;
                TopRight = topRight;
                BottomRight = bottomRight;
                BottomLeft = bottomLeft;
            }
        }
        #endregion

        public static float PartialBlockScalar = 0.375f;

        protected Vector3 GlobalScalar = new Vector3(1, 1, 0.5f);

        ///// <summary>
        ///// Position of this block in a map.
        ///// </summary>
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

        protected blockInfo blockInfo;

        public Dictionary<int, Rectangle> TileAtlas;

        protected BlockInfo(blockInfo blockInfo, Vector3 pos)
        {
            this.blockInfo = blockInfo;
            this.Left = new BlockFaceEdge(blockInfo.Left);
            this.Right = new BlockFaceEdge(blockInfo.Right);
            this.Top = new BlockFaceEdge(blockInfo.Top);
            this.Bottom = new BlockFaceEdge(blockInfo.Bottom);
            this.Lid = new BlockFaceLid(blockInfo.Lid);
            this.Arrows = (RoadTrafficType)blockInfo.Arrows; //ToDo: Check, don't know if this works...
            this.ParseSlope(blockInfo.SlopeType);
            this.Position = pos;

            this.Coors = new List<VertexPositionNormalTexture>();
            this.IndexBufferCollection = new List<int>();
        }

        public BlockInfo()
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
        /// Makes a new copy of this object.
        /// </summary>
        /// <returns>The new copy.</returns>
        public abstract BlockInfo DeepCopy();

        /// <summary>
        /// Makes a new copy of this object.
        /// </summary>
        /// <param name="blockInfo">The blockInfo</param>
        /// <param name="pos">The position</param>
        /// <returns>The new copy</returns>
        public abstract BlockInfo DeepCopy(blockInfo blockInfo, Vector3 pos);

        /// <summary>
        /// Test if this instance represents the slopeType
        /// </summary>
        /// <param name="slopeType">SlopeType to test</param>
        /// <returns>True </returns>
        public abstract Boolean IsMe(SlopeType slopeType);

        /// <summary>
        /// Calculate the coordinates of the verticies.
        /// </summary>
        protected abstract void SetUpCube();

        public void ParseSlope(byte type)
        {
            if (type == 0)
                return;

            var groundType = 0;
            groundType += (type & 1);
            groundType += (type & 2);
            GroundType = (GroundType)groundType;

            if (type < 4)
                return;

            var slopeType = 0;
            for (var i = 2; i < 8; i++)
            {
                if (BitHelper.CheckBit(type, i))
                    slopeType += (int)Math.Pow(2, i - 2);
            }
            SlopeType = (SlopeType)slopeType;

            //make some corrections to the collision bits.
            switch (SlopeType)
            {
                case SlopeType.DiagonalFacingUpLeft:
                case SlopeType.DiagonalFacingDownLeft:
                    Left.Wall = true;
                    Left.BulletWall = true;
                    break;
                case SlopeType.DiagonalFacingUpRight:
                case SlopeType.DiagonalFacingDownRight:
                    Right.Wall = true;
                    Right.BulletWall = true;
                    break;
            }
        }

        public virtual bool IsEmpty
        {
            get
            {
                if ((Left == null && Right == null && Top == null && Bottom == null && Lid == null) || ((!Left && !Right && !Top && !Bottom && !Lid)))
                    return true;
                return false;
            }
        }

        public bool IsLidOnly
        {
            get
            {
                if (IsEmpty)
                    return false;
                if (Lid != null && Lid && (Left == null || !Left) && (Right == null || !Right) && (Top == null || !Top) && (Bottom == null || !Bottom))
                    return true;
                return false;
            }
        }               

        //the player can walk on these
        public bool IsMovableSlope
        {
            get
            {
                var slope = (byte)SlopeType;
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

        public bool IsDiagonalSlope
        {
            get
            {
                byte slope = (byte)SlopeType;
                return slope >= 45 && slope <= 48;
            }
        }

        public byte SlopeSubTyp
        {
            get
            {
                byte slope = (byte)SlopeType;
                if (slope > 0 && slope < 9)
                    return 26;
                if (slope > 8 && slope < 41)
                    return 7;
                if (slope > 40 && slope < 45)
                    return 45;
                return 0;
            }
        }        

        public override string ToString()
        {
            if (IsEmpty)
                return "[empty block]";
            return "Lid: " + Lid + " Left: " + Left + " Top: " + Top + " Right: " + Right + " Bottom: " + Bottom + " Ground: " + GroundType.ToString();
        }

        #region Coordinates

        protected void PrepareCoordinates(Vector3 position, out FaceCoordinates frontCoords, out FaceCoordinates backCoords)
        {
            position.Y *= -1;

            //Coordinates of the cube
            Vector3 topLeftFront = (new Vector3(0.0f, 0.0f, 1f) + position) * GlobalScalar;
            Vector3 topRightFront = (new Vector3(1f, 0.0f, 1f) + position) * GlobalScalar;
            Vector3 bottomLeftFront = (new Vector3(0.0f, -1f, 1f) + position) * GlobalScalar;
            Vector3 bottomRightFront = (new Vector3(1f, -1f, 1f) + position) * GlobalScalar;
            frontCoords = new FaceCoordinates(topLeftFront, topRightFront, bottomRightFront, bottomLeftFront);

            Vector3 topLeftBack = (new Vector3(0.0f, 0.0f, 0.0f) + position) * GlobalScalar;
            Vector3 topRightBack = (new Vector3(1f, 0.0f, 0.0f) + position) * GlobalScalar;
            Vector3 bottomLeftBack = (new Vector3(0.0f, -1f, 0.0f) + position) * GlobalScalar;
            Vector3 bottomRightBack = (new Vector3(1f, -1f, 0.0f) + position) * GlobalScalar;
            backCoords = new FaceCoordinates(topLeftBack, topRightBack, bottomRightBack, bottomLeftBack);
        }
        #endregion

        protected Vector2[] GetTexturePositions(Rectangle sourceRectangle, RotationType rotation, bool flip)
        {
            double pixelPerWidth = 1f / 2046;
            double pixelPerHeight = 1f / 2112;
            Vector2[] texturePosition = new Vector2[4];

            Vector2 texTopLeft = new Vector2((float)((sourceRectangle.X + 1) * pixelPerWidth), (float)((sourceRectangle.Y + 1) * pixelPerHeight));
            Vector2 texTopRight = new Vector2((float)((sourceRectangle.X + sourceRectangle.Width - 1) * pixelPerWidth), (float)((sourceRectangle.Y + 1) * pixelPerHeight));
            Vector2 texBottomRight = new Vector2((float)((sourceRectangle.X + sourceRectangle.Width - 1) * pixelPerWidth), (float)((sourceRectangle.Y + sourceRectangle.Height - 1) * pixelPerHeight));
            Vector2 texBottomLeft = new Vector2((float)((sourceRectangle.X + 1) * pixelPerWidth), (float)((sourceRectangle.Y + sourceRectangle.Height - 1) * pixelPerHeight));

            if (flip)
            {
                Vector2 helper = texTopLeft;
                texTopLeft = texTopRight;
                texTopRight = helper;
                helper = texBottomLeft;
                texBottomLeft = texBottomRight;
                texBottomRight = helper;
                if (rotation == RotationType.Rotate90) //Hack
                {
                    rotation = RotationType.Rotate270;
                }
                else if (rotation == RotationType.Rotate270)
                {
                    rotation = RotationType.Rotate90;
                }
            }

            switch (rotation)
            {
                case RotationType.RotateNone:
                    texturePosition[0] = texBottomLeft;
                    texturePosition[1] = texBottomRight;
                    texturePosition[2] = texTopRight;
                    texturePosition[3] = texTopLeft;
                    break;
                case RotationType.Rotate90:
                    texturePosition[3] = texBottomLeft;
                    texturePosition[0] = texBottomRight;
                    texturePosition[1] = texTopRight;
                    texturePosition[2] = texTopLeft;
                    break;
                case RotationType.Rotate180:
                    texturePosition[2] = texBottomLeft;
                    texturePosition[3] = texBottomRight;
                    texturePosition[0] = texTopRight;
                    texturePosition[1] = texTopLeft;
                    break;
                case RotationType.Rotate270:
                    texturePosition[1] = texBottomLeft;
                    texturePosition[2] = texBottomRight;
                    texturePosition[3] = texTopRight;
                    texturePosition[0] = texTopLeft;
                    break;
            }
            return texturePosition;
        }

        #region Not Implemented

        /// <summary>
        /// ToDo
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
    }
}
