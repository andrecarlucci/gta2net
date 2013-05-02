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
using System.Collections.Generic;
using Hiale.GTA2NET.Core.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Hiale.GTA2NET.Core.Collision;

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

        public Dictionary<int, CompactRectangle> tileAtlas;

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
        public bool IsMe(SlopeType slopeType)
        {
            return (SlopeType == slopeType);
        }

        /// <summary>
        /// Calculate the coordinates of the verticies.
        /// </summary>
        public abstract void SetUpCube();

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
        
        public override string ToString()
        {
            if (IsEmpty)
                return "[empty block]";
            return "Lid: " + Lid + " Left: " + Left + " Top: " + Top + " Right: " + Right + " Bottom: " + Bottom + " Ground: " + GroundType.ToString();
        }

        #region Coordinates
        /// <summary>
        /// Based in the position of the BlockInfo calculate the positons of the verticies of that cube.
        /// </summary>
        /// <param name="frontCoords">The coordinates of the top face of the cube</param>
        /// <param name="backCoords">The coordinates of the botton face of the cube</param>
        protected void PrepareCoordinates(out FaceCoordinates frontCoords, out FaceCoordinates backCoords)
        {
            var position = Position;
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

        /// <summary>
        /// Gets the coordinates for the texture acording to its rotation and flip.
        /// </summary>
        /// <param name="sourceRectangle">Represents the position and size of the texture.</param>
        /// <param name="rotation">The rotation to aply to the texture.</param>
        /// <param name="flip">If the Texture must be fliped.</param>
        /// <returns>A array with 4 positions where each position represent one of the vertices od the texture.</returns>
        protected Vector2[] GetTexturePositions(CompactRectangle sourceRectangle, RotationType rotation, bool flip)
        {
            double pixelPerWidth = 1f / 2048;   //TODO: this values must be equals to the tiles size.
            double pixelPerHeight = 1f / 4096;
            Vector2[] texturePosition = new Vector2[4];

            Vector2 texTopLeft = new Vector2((float)((sourceRectangle.X + 1) * pixelPerWidth), (float)((sourceRectangle.Y + 1) * pixelPerHeight));
            Vector2 texTopRight = new Vector2((float)((sourceRectangle.X + sourceRectangle.Width - 1) * pixelPerWidth), (float)((sourceRectangle.Y + 1) * pixelPerHeight));            
            Vector2 texBottomLeft = new Vector2((float)((sourceRectangle.X + 1) * pixelPerWidth), (float)((sourceRectangle.Y + sourceRectangle.Height - 1) * pixelPerHeight));
            Vector2 texBottomRight = new Vector2((float)((sourceRectangle.X + sourceRectangle.Width - 1) * pixelPerWidth), (float)((sourceRectangle.Y + sourceRectangle.Height - 1) * pixelPerHeight));

            if (flip)
            {
                Vector2 helper = texTopLeft;
                texTopLeft = texTopRight;
                texTopRight = helper;

                helper = texBottomLeft;
                texBottomLeft = texBottomRight;
                texBottomRight = helper;

                if (rotation == RotationType.Rotate90) //special cases.
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

        #region Cube
        /// <summary>
        /// Creates the Lid vertices Coordinates.
        /// </summary>
        /// <param name="lidCoords">The lid coordinates.</param>
        protected void CreateFrontVertices(FaceCoordinates lidCoords)
        {
            if (!Lid)
                return;

            var texPos = GetTexturePositions(tileAtlas[Lid.TileNumber], Lid.Rotation, Lid.Flip);
            this.Coors.Add(new VertexPositionNormalTexture(lidCoords.TopRight, Vector3.Zero, texPos[2]));
            this.Coors.Add(new VertexPositionNormalTexture(lidCoords.BottomRight, Vector3.Zero, texPos[1]));
            this.Coors.Add(new VertexPositionNormalTexture(lidCoords.TopLeft, Vector3.Zero, texPos[3]));
            this.Coors.Add(new VertexPositionNormalTexture(lidCoords.BottomLeft, Vector3.Zero, texPos[0]));

            var startIndex = Coors.Count - 4;
            this.IndexBufferCollection.Add(startIndex);
            this.IndexBufferCollection.Add(startIndex + 1);
            this.IndexBufferCollection.Add(startIndex + 2);
            this.IndexBufferCollection.Add(startIndex + 1);
            this.IndexBufferCollection.Add(startIndex + 3);
            this.IndexBufferCollection.Add(startIndex + 2);
        }

        /// <summary>
        /// Creates the Top vertices Coordinates.
        /// </summary>
        /// <param name="frontCoords"></param>
        /// <param name="backCoords"></param>
        protected void CreateTopVertices(FaceCoordinates frontCoords, FaceCoordinates backCoords)
        {
            if (!Top)
                return;

            var texPos = GetTexturePositions(tileAtlas[this.Top.TileNumber], this.Top.Rotation, this.Top.Flip);
            Coors.Add(new VertexPositionNormalTexture(frontCoords.TopRight, Vector3.Zero, texPos[2]));
            Coors.Add(new VertexPositionNormalTexture(backCoords.TopLeft, Vector3.Zero, texPos[0]));
            Coors.Add(new VertexPositionNormalTexture(backCoords.TopRight, Vector3.Zero, texPos[1]));
            Coors.Add(new VertexPositionNormalTexture(frontCoords.TopLeft, Vector3.Zero, texPos[3]));

            var startIndex = Coors.Count - 4;
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex + 1);
            IndexBufferCollection.Add(startIndex + 2);
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex + 3);
            IndexBufferCollection.Add(startIndex + 1);
        }

        protected void CreateBottomVertices(FaceCoordinates frontCoords, FaceCoordinates backCoords)
        {
            if (!Bottom)
                return;

            var texPos = GetTexturePositions(tileAtlas[this.Bottom.TileNumber], this.Bottom.Rotation, this.Bottom.Flip);
            Coors.Add(new VertexPositionNormalTexture(frontCoords.BottomRight, Vector3.Zero, texPos[2]));
            Coors.Add(new VertexPositionNormalTexture(backCoords.BottomRight, Vector3.Zero, texPos[1]));
            Coors.Add(new VertexPositionNormalTexture(backCoords.BottomLeft, Vector3.Zero, texPos[0]));
            Coors.Add(new VertexPositionNormalTexture(frontCoords.BottomLeft, Vector3.Zero, texPos[3]));

            var startIndex = Coors.Count - 4;
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex + 1);
            IndexBufferCollection.Add(startIndex + 2);
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex + 2);
            IndexBufferCollection.Add(startIndex + 3);
        }

        protected void CreateLeftVertices(FaceCoordinates frontCoords, FaceCoordinates backCoords, Byte rotation)
        {
            if (!Left)
                return;

            var newFront = new FaceCoordinates();
            var newBack = new FaceCoordinates();
            if (rotation == 0)
            {
                newFront = CorrectLeftRightVertices(frontCoords, true);
                newBack = CorrectLeftRightVertices(backCoords, true);
            }
            else if (rotation == 2)
            {
                newFront = CorrectLeftRightVertices(frontCoords, false);
                newBack = CorrectLeftRightVertices(backCoords, false);
            }
            var texPos = GetTexturePositions(tileAtlas[this.Left.TileNumber], this.Left.Rotation, this.Left.Flip);
            Coors.Add(new VertexPositionNormalTexture(newFront.TopRight, Vector3.Zero, texPos[3]));
            Coors.Add(new VertexPositionNormalTexture(newBack.BottomRight, Vector3.Zero, texPos[1]));
            Coors.Add(new VertexPositionNormalTexture(newFront.BottomRight, Vector3.Zero, texPos[2]));
            Coors.Add(new VertexPositionNormalTexture(newBack.TopRight, Vector3.Zero, texPos[0]));

            //Left also has a strange index buffer order...
            var startIndex = Coors.Count - 4;
            IndexBufferCollection.Add(startIndex + 2);
            IndexBufferCollection.Add(startIndex + 1);
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex + 1);
            IndexBufferCollection.Add(startIndex + 3);
            IndexBufferCollection.Add(startIndex);
        }

        protected void CreateRightVertices(FaceCoordinates frontCoords, FaceCoordinates backCoords, Byte rotation)
        {
            if (this.Right.TileNumber > 0)
            {
                FaceCoordinates newFront = new FaceCoordinates();
                FaceCoordinates newBack = new FaceCoordinates();
                if (rotation == 0)
                {
                    newFront = CorrectLeftRightVertices(frontCoords, false);
                    newBack = CorrectLeftRightVertices(backCoords, false);
                }
                else if (rotation == 2)
                {
                    newFront = CorrectLeftRightVertices(frontCoords, true);
                    newBack = CorrectLeftRightVertices(backCoords, true);
                }
                //ToDo: Add more rotation codes...
                Vector2[] texPos = GetTexturePositions(tileAtlas[this.Right.TileNumber], this.Right.Rotation, this.Right.Flip);
                Coors.Add(new VertexPositionNormalTexture(newFront.TopLeft, Vector3.Zero, texPos[2]));
                Coors.Add(new VertexPositionNormalTexture(newFront.BottomLeft, Vector3.Zero, texPos[3]));
                Coors.Add(new VertexPositionNormalTexture(newBack.BottomLeft, Vector3.Zero, texPos[0]));
                Coors.Add(new VertexPositionNormalTexture(newBack.TopLeft, Vector3.Zero, texPos[1]));

                //...
                int startIndex = Coors.Count - 4;
                IndexBufferCollection.Add(startIndex + 2);
                IndexBufferCollection.Add(startIndex + 1);
                IndexBufferCollection.Add(startIndex);
                IndexBufferCollection.Add(startIndex);
                IndexBufferCollection.Add(startIndex + 3);
                IndexBufferCollection.Add(startIndex + 2);
            }
        }
        #endregion

        #region Low Slopes
        /// <summary>
        /// Creates a Low Slope.
        /// </summary>
        /// <param name="subType"></param>
        /// <param name="rotation"></param>
        protected void SetLowSlope(byte subType, byte rotation)
        {
            //Sample is a right slope, use it for orientation, it gets rotated to fit all other directions
            FaceCoordinates frontCoordinates;
            FaceCoordinates backCoordinates;
            PrepareCoordinates(out frontCoordinates, out backCoordinates);

            float slopeScalar = 1f;

            switch (subType)
            {
                case 26:
                    slopeScalar = 0.5f;
                    break;
                case 45:
                    slopeScalar = 0;
                    break;
                case 7:
                    slopeScalar = 0.875f;
                    break;
            }

            Vector3 middleTopLeft = frontCoordinates.TopLeft;
            Vector3 middleTopRight = frontCoordinates.TopRight;
            Vector3 middleBottomRight = frontCoordinates.BottomRight;
            Vector3 middleBottomLeft = frontCoordinates.BottomLeft;
            FaceCoordinates middleCoordinates = new FaceCoordinates(middleTopLeft, middleTopRight, middleBottomRight, middleBottomLeft);

            if (rotation > 0)
            {
                frontCoordinates = RotateSlope(frontCoordinates, rotation);
                backCoordinates = RotateSlope(backCoordinates, rotation);
                middleCoordinates = RotateSlope(middleCoordinates, rotation);
            }

            middleCoordinates.TopLeft.Z -= slopeScalar * GlobalScalar.Z;
            middleCoordinates.TopRight.Z -= slopeScalar * GlobalScalar.Z;
            middleCoordinates.BottomRight.Z -= slopeScalar * GlobalScalar.Z;
            middleCoordinates.BottomLeft.Z -= slopeScalar * GlobalScalar.Z;

            //Lid face
            if (this.Lid.TileNumber > 0)
            {
                RotationType lidRotation = this.Lid.Rotation;
                lidRotation = RotateEnum(lidRotation, rotation);
                Vector2[] texPos = GetTexturePositions(tileAtlas[this.Lid.TileNumber], lidRotation, this.Lid.Flip);
                this.Coors.Add(new VertexPositionNormalTexture(middleCoordinates.TopRight, Vector3.Zero, texPos[2]));
                this.Coors.Add(new VertexPositionNormalTexture(middleCoordinates.BottomRight, Vector3.Zero, texPos[1]));
                this.Coors.Add(new VertexPositionNormalTexture(backCoordinates.BottomLeft, Vector3.Zero, texPos[0]));
                this.Coors.Add(new VertexPositionNormalTexture(backCoordinates.TopLeft, Vector3.Zero, texPos[3]));

                int startIndex = this.Coors.Count - 4;
                this.IndexBufferCollection.Add(startIndex);
                this.IndexBufferCollection.Add(startIndex + 1);
                this.IndexBufferCollection.Add(startIndex + 2);
                this.IndexBufferCollection.Add(startIndex);
                this.IndexBufferCollection.Add(startIndex + 2);
                this.IndexBufferCollection.Add(startIndex + 3);
            }

            BlockFace topFace = null;
            BlockFace bottomFace = null;
            BlockFace rightFace = null;
            switch (rotation)
            {
                case 0: //No rotation
                    topFace = this.Top;
                    bottomFace = this.Bottom;
                    rightFace = this.Right;
                    break;
                case 1: //
                    topFace = this.Left;
                    bottomFace = this.Right;
                    rightFace = this.Top;
                    break;
                case 2:
                    topFace = this.Bottom;
                    bottomFace = this.Top;
                    rightFace = this.Left;
                    break;
                case 3:
                    topFace = this.Right;
                    bottomFace = this.Left;
                    rightFace = this.Bottom;
                    break;

            }

            //Top face
            if (topFace.TileNumber > 0)
            {
                Vector2[] texPos = GetTexturePositions(tileAtlas[topFace.TileNumber], topFace.Rotation, topFace.Flip);
                Vector2 center = GetCenterPosition(ref texPos[3], ref texPos[0], slopeScalar);
                this.Coors.Add(new VertexPositionNormalTexture(middleCoordinates.TopRight, Vector3.Zero, center));
                this.Coors.Add(new VertexPositionNormalTexture(backCoordinates.TopRight, Vector3.Zero, texPos[0]));
                this.Coors.Add(new VertexPositionNormalTexture(backCoordinates.TopLeft, Vector3.Zero, texPos[1]));

                int startIndex = this.Coors.Count - 3;
                this.IndexBufferCollection.Add(startIndex + 2);
                this.IndexBufferCollection.Add(startIndex + 1);
                this.IndexBufferCollection.Add(startIndex + 0);
            }
            //Bottom face
            if (bottomFace.TileNumber > 0)
            {
                Vector2[] texPos = GetTexturePositions(tileAtlas[bottomFace.TileNumber], bottomFace.Rotation, bottomFace.Flip);
                Vector2 center = GetCenterPosition(ref texPos[2], ref texPos[1], slopeScalar);
                this.Coors.Add(new VertexPositionNormalTexture(middleCoordinates.BottomRight, Vector3.Zero, center));
                this.Coors.Add(new VertexPositionNormalTexture(backCoordinates.BottomRight, Vector3.Zero, texPos[1]));
                this.Coors.Add(new VertexPositionNormalTexture(backCoordinates.BottomLeft, Vector3.Zero, texPos[0]));

                int startIndex = this.Coors.Count - 3;
                this.IndexBufferCollection.Add(startIndex);
                this.IndexBufferCollection.Add(startIndex + 1);
                this.IndexBufferCollection.Add(startIndex + 2);
            }

            //Right face
            if (rightFace.TileNumber > 0) //this face is not supported by GTA2, the editor removes this face.
            {
                Vector2[] texPos = GetTexturePositions(tileAtlas[rightFace.TileNumber], rightFace.Rotation, rightFace.Flip);
                Vector2 center = GetCenterPosition(ref texPos[1], ref texPos[2], slopeScalar);
                this.Coors.Add(new VertexPositionNormalTexture(middleCoordinates.TopRight, Vector3.Zero, center));
                center = GetCenterPosition(ref texPos[0], ref texPos[3], slopeScalar);
                this.Coors.Add(new VertexPositionNormalTexture(middleCoordinates.BottomRight, Vector3.Zero, center));
                this.Coors.Add(new VertexPositionNormalTexture(backCoordinates.BottomRight, Vector3.Zero, texPos[3]));
                this.Coors.Add(new VertexPositionNormalTexture(backCoordinates.TopRight, Vector3.Zero, texPos[2]));

                int startIndex = this.Coors.Count - 4;
                this.IndexBufferCollection.Add(startIndex + 2);
                this.IndexBufferCollection.Add(startIndex + 1);
                this.IndexBufferCollection.Add(startIndex + 0);
                this.IndexBufferCollection.Add(startIndex);
                this.IndexBufferCollection.Add(startIndex + 3);
                this.IndexBufferCollection.Add(startIndex + 2);
            }
        }

        protected static Vector2 GetCenterPosition(ref Vector2 lowerEnd, ref Vector2 higherEnd, float amount) //ToDo: method name
        {
            Vector2 center;
            center.X = MathHelper.Lerp(lowerEnd.X, higherEnd.X, amount);
            center.Y = MathHelper.Lerp(lowerEnd.Y, higherEnd.Y, amount);
            return center;
        }

        /// <summary>
        /// Roteate a slope by 90° (rotation = 1), 180° (rotation = 2) or 270° (rotation = 3)
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
        #endregion

        #region High Slopes
        protected void SetUpSlope_High(byte subType, byte rotation)
        {
            //Sample is a right slope, use it for orientation, it gets rotated to fit all other directions
            FaceCoordinates frontCoordinates;
            FaceCoordinates backCoordinates;
            PrepareCoordinates(out frontCoordinates, out backCoordinates);

            float middleSlopeScalar = 1f;
            float frontSlopeScalar = 0f;
            switch (subType)
            {
                case 26:
                    middleSlopeScalar = 0.5f;
                    frontSlopeScalar = 0;
                    break;
                case 7:
                    middleSlopeScalar = 0.875f;
                    frontSlopeScalar = 0.75f;
                    break;
                case 8:
                    middleSlopeScalar = 0.75f;
                    frontSlopeScalar = 0.625f;
                    break;
                case 9:
                    middleSlopeScalar = 0.625f;
                    frontSlopeScalar = 0.5f;
                    break;
                case 10:
                    middleSlopeScalar = 0.5f;
                    frontSlopeScalar = 0.375f;
                    break;
                case 11:
                    middleSlopeScalar = 0.375f;
                    frontSlopeScalar = 0.25f;
                    break;
                case 12:
                    middleSlopeScalar = 0.25f;
                    frontSlopeScalar = 0.125f;
                    break;
                case 13:
                    middleSlopeScalar = 0.125f;
                    frontSlopeScalar = 0;
                    break;
            }

            Vector3 middleTopLeft = frontCoordinates.TopLeft;
            Vector3 middleTopRight = frontCoordinates.TopRight;
            Vector3 middleBottomRight = frontCoordinates.BottomRight;
            Vector3 middleBottomLeft = frontCoordinates.BottomLeft;
            FaceCoordinates middleCoordinates = new FaceCoordinates(middleTopLeft, middleTopRight, middleBottomRight, middleBottomLeft);

            if (rotation > 0)
            {
                frontCoordinates = RotateSlope(frontCoordinates, rotation);
                backCoordinates = RotateSlope(backCoordinates, rotation);
                middleCoordinates = RotateSlope(middleCoordinates, rotation);
            }

            frontCoordinates.TopLeft.Z -= frontSlopeScalar * GlobalScalar.Z;
            frontCoordinates.TopRight.Z -= frontSlopeScalar * GlobalScalar.Z;
            frontCoordinates.BottomRight.Z -= frontSlopeScalar * GlobalScalar.Z;
            frontCoordinates.BottomLeft.Z -= frontSlopeScalar * GlobalScalar.Z;

            middleCoordinates.TopLeft.Z -= middleSlopeScalar * GlobalScalar.Z;
            middleCoordinates.TopRight.Z -= middleSlopeScalar * GlobalScalar.Z;
            middleCoordinates.BottomRight.Z -= middleSlopeScalar * GlobalScalar.Z;
            middleCoordinates.BottomLeft.Z -= middleSlopeScalar * GlobalScalar.Z;

            //Front face (diagonal)
            if (this.Lid.TileNumber > 0)
            {
                RotationType lidRotation = this.Lid.Rotation;
                lidRotation = RotateEnum(lidRotation, rotation);
                Vector2[] texPos = GetTexturePositions(tileAtlas[this.Lid.TileNumber], lidRotation, this.Lid.Flip);
                this.Coors.Add(new VertexPositionNormalTexture(frontCoordinates.TopRight, Vector3.Zero, texPos[2]));
                this.Coors.Add(new VertexPositionNormalTexture(frontCoordinates.BottomRight, Vector3.Zero, texPos[1]));
                this.Coors.Add(new VertexPositionNormalTexture(middleCoordinates.BottomLeft, Vector3.Zero, texPos[0]));
                this.Coors.Add(new VertexPositionNormalTexture(middleCoordinates.TopLeft, Vector3.Zero, texPos[3]));

                int startIndex = this.Coors.Count - 4;
                this.IndexBufferCollection.Add(startIndex + 0);
                this.IndexBufferCollection.Add(startIndex + 1);
                this.IndexBufferCollection.Add(startIndex + 2);
                this.IndexBufferCollection.Add(startIndex + 0);
                this.IndexBufferCollection.Add(startIndex + 2);
                this.IndexBufferCollection.Add(startIndex + 3);
            }

            BlockFace topFace = null;
            BlockFace bottomFace = null;
            BlockFace leftFace = null;
            BlockFace rightFace = null;
            switch (rotation)
            {
                case 0:
                    topFace = this.Top;
                    bottomFace = this.Bottom;
                    leftFace = this.Left;
                    rightFace = this.Right;
                    break;
                case 1:
                    topFace = this.Left;
                    bottomFace = this.Right;
                    leftFace = this.Bottom;
                    rightFace = this.Top;
                    break;
                case 2:
                    topFace = this.Bottom;
                    bottomFace = this.Top;
                    leftFace = this.Right;
                    rightFace = this.Left;
                    break;
                case 3:
                    topFace = this.Right;
                    bottomFace = this.Left;
                    leftFace = this.Top;
                    rightFace = this.Bottom;
                    break;
            }

            //Top face
            if (topFace.TileNumber > 0)
            {
                Vector2[] texPos = GetTexturePositions(tileAtlas[topFace.TileNumber], topFace.Rotation, topFace.Flip);
                Vector2 center = GetCenterPosition(ref texPos[0], ref texPos[3], frontSlopeScalar);
                this.Coors.Add(new VertexPositionNormalTexture(frontCoordinates.TopRight, Vector3.Zero, center)); //was 3
                this.Coors.Add(new VertexPositionNormalTexture(backCoordinates.TopRight, Vector3.Zero, texPos[0]));
                this.Coors.Add(new VertexPositionNormalTexture(backCoordinates.TopLeft, Vector3.Zero, texPos[1]));
                center = GetCenterPosition(ref texPos[1], ref texPos[2], middleSlopeScalar);
                this.Coors.Add(new VertexPositionNormalTexture(middleCoordinates.TopLeft, Vector3.Zero, center));

                int startIndex = this.Coors.Count - 4;
                this.IndexBufferCollection.Add(startIndex + 2);
                this.IndexBufferCollection.Add(startIndex + 1);
                this.IndexBufferCollection.Add(startIndex + 0);

                this.IndexBufferCollection.Add(startIndex + 3);
                this.IndexBufferCollection.Add(startIndex + 2);
                this.IndexBufferCollection.Add(startIndex + 0);
            }
            //Bottom face
            if (bottomFace.TileNumber > 0)
            {
                Vector2[] texPos = GetTexturePositions(tileAtlas[bottomFace.TileNumber], bottomFace.Rotation, bottomFace.Flip);
                Vector2 center = GetCenterPosition(ref texPos[2], ref texPos[1], frontSlopeScalar);
                this.Coors.Add(new VertexPositionNormalTexture(frontCoordinates.BottomRight, Vector3.Zero, center)); //was texPos[2]
                this.Coors.Add(new VertexPositionNormalTexture(backCoordinates.BottomRight, Vector3.Zero, texPos[1]));
                this.Coors.Add(new VertexPositionNormalTexture(backCoordinates.BottomLeft, Vector3.Zero, texPos[0]));
                center = GetCenterPosition(ref texPos[3], ref texPos[0], middleSlopeScalar);
                this.Coors.Add(new VertexPositionNormalTexture(middleCoordinates.BottomLeft, Vector3.Zero, center));

                int startIndex = this.Coors.Count - 4;
                this.IndexBufferCollection.Add(startIndex);
                this.IndexBufferCollection.Add(startIndex + 1);
                this.IndexBufferCollection.Add(startIndex + 2);

                this.IndexBufferCollection.Add(startIndex + 0);
                this.IndexBufferCollection.Add(startIndex + 2);
                this.IndexBufferCollection.Add(startIndex + 3);
            }

            //ToDo Left face (but probably not supported in GTA2 anyway)

            //Right face
            CreateRightVertices(frontCoordinates, backCoordinates, rotation);
        }        

        protected FaceCoordinates CorrectLeftRightVertices(FaceCoordinates coordinates, Boolean left)
        {
            FaceCoordinates newCoords = new FaceCoordinates();

            float value;
            if (left)
                value = -GlobalScalar.X;
            else
                value = GlobalScalar.X;

            newCoords.TopLeft.X = coordinates.TopLeft.X + value;
            newCoords.TopRight.X = coordinates.TopRight.X + value;
            newCoords.BottomRight.X = coordinates.BottomRight.X + value;
            newCoords.BottomLeft.X = coordinates.BottomLeft.X + value;

            newCoords.TopLeft.Y = coordinates.TopLeft.Y;
            newCoords.TopRight.Y = coordinates.TopRight.Y;
            newCoords.BottomRight.Y = coordinates.BottomRight.Y;
            newCoords.BottomLeft.Y = coordinates.BottomLeft.Y;

            newCoords.TopLeft.Z = coordinates.TopLeft.Z;
            newCoords.TopRight.Z = coordinates.TopRight.Z;
            newCoords.BottomRight.Z = coordinates.BottomRight.Z;
            newCoords.BottomLeft.Z = coordinates.BottomLeft.Z;

            return newCoords;
        }
        #endregion

        #region Diagonal Slopes

        protected void SetUpSlopeDiagonal(byte rotation)
        {
            FaceCoordinates frontCoordinates;
            FaceCoordinates backCoordinates;
            PrepareCoordinates(out frontCoordinates, out backCoordinates);

            if (rotation > 0)
            {
                frontCoordinates = RotateSlope(frontCoordinates, rotation);
                backCoordinates = RotateSlope(backCoordinates, rotation);
            }

            if (Position.X == 79 && Position.Y == 175 && Position.Z == 4)
                Console.Read();
            //
            //LID
            //
            if (this.Lid)
            {
                RotationType lidRotation = this.Lid.Rotation;
                lidRotation = RotateEnum(lidRotation, rotation);

                if (this.Lid.Flip) //ToDo: This is just a dirty way! Problem: rotation Bug if flipped
                {
                    if (rotation == 1 || rotation == 3)
                    {
                        switch (lidRotation)
                        {
                            case RotationType.Rotate90:
                                lidRotation = RotationType.Rotate270;
                                break;
                            case RotationType.Rotate270:
                                lidRotation = RotationType.Rotate90;
                                break;
                        }
                    }
                }

                var texPos = GetTexturePositions(tileAtlas[this.Lid.TileNumber], lidRotation, this.Lid.Flip);
                Coors.Add(new VertexPositionNormalTexture(frontCoordinates.TopLeft, Vector3.Zero, texPos[3]));
                Coors.Add(new VertexPositionNormalTexture(frontCoordinates.BottomRight, Vector3.Zero, texPos[1]));
                Coors.Add(new VertexPositionNormalTexture(frontCoordinates.BottomLeft, Vector3.Zero, texPos[0]));

                var startIndex = Coors.Count - 3;
                IndexBufferCollection.Add(startIndex);
                IndexBufferCollection.Add(startIndex + 1);
                IndexBufferCollection.Add(startIndex + 2);
            }

            //int TileNumber = 0;
            BlockFace diagonalFace = null;
            switch (rotation)
            {
                case 0:
                    diagonalFace = this.Right;
                    break;
                case 1:
                    diagonalFace = this.Left;
                    break;
                case 2:
                    diagonalFace = this.Left;
                    break;
                case 3:
                    diagonalFace = this.Right;
                    break;
            }

            //Diagonal face
            if (diagonalFace)
            {
                var texPos = GetTexturePositions(tileAtlas[diagonalFace.TileNumber], diagonalFace.Rotation, diagonalFace.Flip);
                Coors.Add(new VertexPositionNormalTexture(frontCoordinates.TopLeft, Vector3.Zero, texPos[3]));
                Coors.Add(new VertexPositionNormalTexture(frontCoordinates.BottomRight, Vector3.Zero, texPos[2]));
                Coors.Add(new VertexPositionNormalTexture(backCoordinates.BottomRight, Vector3.Zero, texPos[1]));
                Coors.Add(new VertexPositionNormalTexture(backCoordinates.TopLeft, Vector3.Zero, texPos[0]));

                var startIndex = Coors.Count - 4;
                IndexBufferCollection.Add(startIndex + 2);
                IndexBufferCollection.Add(startIndex + 1);
                IndexBufferCollection.Add(startIndex);
                IndexBufferCollection.Add(startIndex + 3);
                IndexBufferCollection.Add(startIndex + 2);
                IndexBufferCollection.Add(startIndex);
            }

            PrepareCoordinates(out frontCoordinates, out backCoordinates);
            switch (rotation)
            {
                case 0: //Facing up right
                    CreateBottomVertices(frontCoordinates, backCoordinates);
                    CreateLeftVertices(frontCoordinates, backCoordinates, 0);
                    break;
                case 1: //Facing up left
                    CreateBottomVertices(frontCoordinates, backCoordinates);
                    CreateRightVertices(frontCoordinates, backCoordinates, 0);
                    break;
                case 2: //Facing down left --> BUG
                    CreateTopVertices(frontCoordinates, backCoordinates);
                    CreateRightVertices(frontCoordinates, backCoordinates, 0);
                    break;
                case 3: //Facing down right --> BUG
                    CreateTopVertices(frontCoordinates, backCoordinates);
                    CreateLeftVertices(frontCoordinates, backCoordinates, 0);
                    break;
            }
        }
        #endregion

        #region Abstract Methods
        public abstract List<IObstacle> GetColision();

        #endregion

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

        #region Not Used
        public static float PartialBlockScalar = 0.375f;

        //Created but never used.
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

        public bool IsDiagonalSlope
        {
            get
            {
                byte slope = (byte)SlopeType;
                return slope >= 45 && slope <= 48;
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


        #endregion
    }
}
