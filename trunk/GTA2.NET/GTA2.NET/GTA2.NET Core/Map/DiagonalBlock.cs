// GTA2.NET
// 
// File: DiagonalBlock.cs
// Created: 16.05.2013
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
    public abstract class DiagonalBlock : Block
    {
        protected DiagonalBlock()
        {
            //
        }

        protected DiagonalBlock(BlockStructure blockStructure, Vector3 pos) : base(blockStructure, pos)
        {
            //
        }

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

            //
            //LID
            //
            if (Lid)
            {
                RotationType lidRotation = Lid.Rotation;
                lidRotation = RotateEnum(lidRotation, rotation);

                if (Lid.Flip) //ToDo: This is just a dirty way! Problem: rotation Bug if flipped
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

                Vector2[] texPos = textures.GetNormalTexture((UInt32)Lid.TileNumber, lidRotation, Lid.Flip);
                Coors.Add(new VertexPositionNormalTexture(frontCoordinates.TopLeft, Vector3.Zero, texPos[3]));
                Coors.Add(new VertexPositionNormalTexture(frontCoordinates.BottomRight, Vector3.Zero, texPos[1]));
                Coors.Add(new VertexPositionNormalTexture(frontCoordinates.BottomLeft, Vector3.Zero, texPos[0]));

                int startIndex = Coors.Count - 3;
                IndexBufferCollection.Add(startIndex);
                IndexBufferCollection.Add(startIndex + 1);
                IndexBufferCollection.Add(startIndex + 2);
            }

            //int TileNumber = 0;
            BlockFace diagonalFace = null;
            switch (rotation)
            {
                case 0:
                    diagonalFace = Right;
                    break;
                case 1:
                    diagonalFace = Left;
                    break;
                case 2:
                    diagonalFace = Left;
                    break;
                case 3:
                    diagonalFace = Right;
                    break;
            }

            //Diagonal face
            if (diagonalFace)
            {
                Vector2[] texPos = textures.GetNormalTexture((UInt32)diagonalFace.TileNumber, diagonalFace.Rotation, diagonalFace.Flip);
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

        public override void GetCollision(List<ILineObstacle> obstacles, bool bulletWall)
        {
            //
        }
        
    }
}
