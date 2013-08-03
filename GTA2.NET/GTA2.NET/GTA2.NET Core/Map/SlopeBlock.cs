// GTA2.NET
// 
// File: SlopeBlock.cs
// Created: 11.05.2013
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
using Hiale.GTA2NET.Core.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hiale.GTA2NET.Core.Map
{
    public abstract class SlopeBlock : Block
    {
        protected SlopeBlock()
        {
            //
        }

        protected SlopeBlock(BlockStructure blockStructure, Vector3 pos) : base(blockStructure, pos)
        {
            //
        }

        /// <summary>
        ///     Creates a Low Slope.
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
            var middleCoordinates = new FaceCoordinates(middleTopLeft, middleTopRight, middleBottomRight, middleBottomLeft);

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
            if (Lid)
            {
                RotationType lidRotation = Lid.Rotation;
                lidRotation = RotateEnum(lidRotation, rotation);
                Vector2[] texPos = textures.GetNormalTexture((UInt32)Lid.TileNumber, Lid.Rotation, Lid.Flip);
                Coors.Add(new VertexPositionNormalTexture(middleCoordinates.TopRight, Vector3.Zero, texPos[2]));
                Coors.Add(new VertexPositionNormalTexture(middleCoordinates.BottomRight, Vector3.Zero, texPos[1]));
                Coors.Add(new VertexPositionNormalTexture(backCoordinates.BottomLeft, Vector3.Zero, texPos[0]));
                Coors.Add(new VertexPositionNormalTexture(backCoordinates.TopLeft, Vector3.Zero, texPos[3]));

                int startIndex = Coors.Count - 4;
                IndexBufferCollection.Add(startIndex);
                IndexBufferCollection.Add(startIndex + 1);
                IndexBufferCollection.Add(startIndex + 2);
                IndexBufferCollection.Add(startIndex);
                IndexBufferCollection.Add(startIndex + 2);
                IndexBufferCollection.Add(startIndex + 3);
            }

            BlockFace topFace = null;
            BlockFace bottomFace = null;
            BlockFace rightFace = null;
            switch (rotation)
            {
                case 0: //No rotation
                    topFace = Top;
                    bottomFace = Bottom;
                    rightFace = Right;
                    break;
                case 1: //
                    topFace = Left;
                    bottomFace = Right;
                    rightFace = Top;
                    break;
                case 2:
                    topFace = Bottom;
                    bottomFace = Top;
                    rightFace = Left;
                    break;
                case 3:
                    topFace = Right;
                    bottomFace = Left;
                    rightFace = Bottom;
                    break;
            }

            //Top face
            if (topFace)
            {
                Vector2[] texPos = textures.GetNormalTexture((UInt32)topFace.TileNumber, topFace.Rotation, topFace.Flip);
                Vector2 center = GetCenterPosition(ref texPos[3], ref texPos[0], slopeScalar);
                Coors.Add(new VertexPositionNormalTexture(middleCoordinates.TopRight, Vector3.Zero, center));
                Coors.Add(new VertexPositionNormalTexture(backCoordinates.TopRight, Vector3.Zero, texPos[0]));
                Coors.Add(new VertexPositionNormalTexture(backCoordinates.TopLeft, Vector3.Zero, texPos[1]));

                int startIndex = Coors.Count - 3;
                IndexBufferCollection.Add(startIndex + 2);
                IndexBufferCollection.Add(startIndex + 1);
                IndexBufferCollection.Add(startIndex + 0);
            }
            //Bottom face
            if (bottomFace)
            {
                Vector2[] texPos = textures.GetNormalTexture((UInt32)bottomFace.TileNumber, bottomFace.Rotation, bottomFace.Flip);
                Vector2 center = GetCenterPosition(ref texPos[2], ref texPos[1], slopeScalar);
                Coors.Add(new VertexPositionNormalTexture(middleCoordinates.BottomRight, Vector3.Zero, center));
                Coors.Add(new VertexPositionNormalTexture(backCoordinates.BottomRight, Vector3.Zero, texPos[1]));
                Coors.Add(new VertexPositionNormalTexture(backCoordinates.BottomLeft, Vector3.Zero, texPos[0]));

                int startIndex = Coors.Count - 3;
                IndexBufferCollection.Add(startIndex);
                IndexBufferCollection.Add(startIndex + 1);
                IndexBufferCollection.Add(startIndex + 2);
            }

            //Right face
            if (rightFace) //this face is not supported by GTA2, the editor removes this face.
            {
                Vector2[] texPos = textures.GetNormalTexture((UInt32)rightFace.TileNumber, rightFace.Rotation, rightFace.Flip);
                Vector2 center = GetCenterPosition(ref texPos[1], ref texPos[2], slopeScalar);
                Coors.Add(new VertexPositionNormalTexture(middleCoordinates.TopRight, Vector3.Zero, center));
                center = GetCenterPosition(ref texPos[0], ref texPos[3], slopeScalar);
                Coors.Add(new VertexPositionNormalTexture(middleCoordinates.BottomRight, Vector3.Zero, center));
                Coors.Add(new VertexPositionNormalTexture(backCoordinates.BottomRight, Vector3.Zero, texPos[3]));
                Coors.Add(new VertexPositionNormalTexture(backCoordinates.TopRight, Vector3.Zero, texPos[2]));

                int startIndex = Coors.Count - 4;
                IndexBufferCollection.Add(startIndex + 2);
                IndexBufferCollection.Add(startIndex + 1);
                IndexBufferCollection.Add(startIndex + 0);
                IndexBufferCollection.Add(startIndex);
                IndexBufferCollection.Add(startIndex + 3);
                IndexBufferCollection.Add(startIndex + 2);
            }
        }

        protected void SetUpSlopeHigh(byte subType, byte rotation)
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
            var middleCoordinates = new FaceCoordinates(middleTopLeft, middleTopRight, middleBottomRight, middleBottomLeft);

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
            if (Lid.TileNumber > 0)
            {
                RotationType lidRotation = Lid.Rotation;
                lidRotation = RotateEnum(lidRotation, rotation);
                Vector2[] texPos = textures.GetNormalTexture((UInt32)Lid.TileNumber, Lid.Rotation, Lid.Flip);
                Coors.Add(new VertexPositionNormalTexture(frontCoordinates.TopRight, Vector3.Zero, texPos[2]));
                Coors.Add(new VertexPositionNormalTexture(frontCoordinates.BottomRight, Vector3.Zero, texPos[1]));
                Coors.Add(new VertexPositionNormalTexture(middleCoordinates.BottomLeft, Vector3.Zero, texPos[0]));
                Coors.Add(new VertexPositionNormalTexture(middleCoordinates.TopLeft, Vector3.Zero, texPos[3]));

                int startIndex = Coors.Count - 4;
                IndexBufferCollection.Add(startIndex + 0);
                IndexBufferCollection.Add(startIndex + 1);
                IndexBufferCollection.Add(startIndex + 2);
                IndexBufferCollection.Add(startIndex + 0);
                IndexBufferCollection.Add(startIndex + 2);
                IndexBufferCollection.Add(startIndex + 3);
            }

            BlockFace topFace = null;
            BlockFace bottomFace = null;
            BlockFace leftFace = null;
            BlockFace rightFace = null;
            switch (rotation)
            {
                case 0:
                    topFace = Top;
                    bottomFace = Bottom;
                    leftFace = Left;
                    rightFace = Right;
                    break;
                case 1:
                    topFace = Left;
                    bottomFace = Right;
                    leftFace = Bottom;
                    rightFace = Top;
                    break;
                case 2:
                    topFace = Bottom;
                    bottomFace = Top;
                    leftFace = Right;
                    rightFace = Left;
                    break;
                case 3:
                    topFace = Right;
                    bottomFace = Left;
                    leftFace = Top;
                    rightFace = Bottom;
                    break;
            }

            //Top face
            if (topFace)
            {
                Vector2[] texPos = textures.GetNormalTexture((UInt32)topFace.TileNumber, topFace.Rotation, topFace.Flip);
                Vector2 center = GetCenterPosition(ref texPos[0], ref texPos[3], frontSlopeScalar);
                Coors.Add(new VertexPositionNormalTexture(frontCoordinates.TopRight, Vector3.Zero, center)); //was 3
                Coors.Add(new VertexPositionNormalTexture(backCoordinates.TopRight, Vector3.Zero, texPos[0]));
                Coors.Add(new VertexPositionNormalTexture(backCoordinates.TopLeft, Vector3.Zero, texPos[1]));
                center = GetCenterPosition(ref texPos[1], ref texPos[2], middleSlopeScalar);
                Coors.Add(new VertexPositionNormalTexture(middleCoordinates.TopLeft, Vector3.Zero, center));

                int startIndex = Coors.Count - 4;
                IndexBufferCollection.Add(startIndex + 2);
                IndexBufferCollection.Add(startIndex + 1);
                IndexBufferCollection.Add(startIndex + 0);

                IndexBufferCollection.Add(startIndex + 3);
                IndexBufferCollection.Add(startIndex + 2);
                IndexBufferCollection.Add(startIndex + 0);
            }
            //Bottom face
            if (bottomFace)
            {
                Vector2[] texPos = textures.GetNormalTexture((UInt32)bottomFace.TileNumber, bottomFace.Rotation, bottomFace.Flip);
                Vector2 center = GetCenterPosition(ref texPos[2], ref texPos[1], frontSlopeScalar);
                Coors.Add(new VertexPositionNormalTexture(frontCoordinates.BottomRight, Vector3.Zero, center)); //was texPos[2]
                Coors.Add(new VertexPositionNormalTexture(backCoordinates.BottomRight, Vector3.Zero, texPos[1]));
                Coors.Add(new VertexPositionNormalTexture(backCoordinates.BottomLeft, Vector3.Zero, texPos[0]));
                center = GetCenterPosition(ref texPos[3], ref texPos[0], middleSlopeScalar);
                Coors.Add(new VertexPositionNormalTexture(middleCoordinates.BottomLeft, Vector3.Zero, center));

                int startIndex = Coors.Count - 4;
                IndexBufferCollection.Add(startIndex);
                IndexBufferCollection.Add(startIndex + 1);
                IndexBufferCollection.Add(startIndex + 2);

                IndexBufferCollection.Add(startIndex + 0);
                IndexBufferCollection.Add(startIndex + 2);
                IndexBufferCollection.Add(startIndex + 3);
            }

            //ToDo Left face (but probably not supported in GTA2 anyway)

            //Right face
            CreateRightVertices(frontCoordinates, backCoordinates, rotation);
        }

        protected override ILineObstacle GetDefaultLeftCollison()
        {
            var lineObstacle = base.GetDefaultLeftCollison();
            lineObstacle.IsSlope = true;
            return lineObstacle;
        }

        protected override ILineObstacle GetDefaultTopCollison()
        {
            var lineObstacle = base.GetDefaultTopCollison();
            lineObstacle.IsSlope = true;
            return lineObstacle;
        }

        protected override ILineObstacle GetDefaultRightCollison()
        {
            var lineObstacle = base.GetDefaultRightCollison();
            lineObstacle.IsSlope = true;
            return lineObstacle;
        }

        protected override ILineObstacle GetDefaultBottomCollison()
        {
            var lineObstacle = base.GetDefaultBottomCollison();
            lineObstacle.IsSlope = true;
            return lineObstacle;
        }

        public override void GetCollision(List<ILineObstacle> obstacles, bool bulletWall)
        {
            obstacles.Add(GetDefaultLeftCollison());
            obstacles.Add(GetDefaultTopCollison());
            obstacles.Add(GetDefaultRightCollison());
            obstacles.Add(GetDefaultBottomCollison());
        }

    }
}
