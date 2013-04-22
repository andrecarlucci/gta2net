// GTA2.NET
// 
// File: Cube.cs
// Created: 22.04.2013
// Created by: João Pires
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
using Microsoft.Xna.Framework.Graphics;

namespace Hiale.GTA2NET.Core.Map.Blocks
{
    public class Cube : BlockInfo
    {
        public Cube() : base()
        {
            this.SlopeType = SlopeType.None;
        }

        public Cube(blockInfo blockInfo, Vector3 pos) : base(blockInfo, pos)
        {
            this.SlopeType = SlopeType.None;
        }        

        public override BlockInfo DeepCopy()
        {
            return new Cube(blockInfo,  Position);
        }

        public override BlockInfo DeepCopy(blockInfo blockInfo, Vector3 pos)
        {
            return new Cube(blockInfo, pos);
        }
        
        public override bool IsEmpty
        {
            get
            {
                return false;
            }
        }

        public override void SetUpCube()
        {
            FaceCoordinates frontCoordinates;
            FaceCoordinates backCoordinates;
            PrepareCoordinates(Position, out frontCoordinates, out backCoordinates);

            CreateFrontVertices(frontCoordinates);

            // Top face
            CreateTopVertices(frontCoordinates, backCoordinates);
            // Bottom face
            CreateBottomVertices(frontCoordinates, backCoordinates);
            // Left face
            CreateLeftVertices(frontCoordinates, backCoordinates, 0);
            // Right face
            CreateRightVertices( frontCoordinates,  backCoordinates, 0);
        }

        private void CreateFrontVertices(FaceCoordinates frontCoords)
        {
            if (!Lid)
                return;
            
            var texPos = GetTexturePositions(tileAtlas[Lid.TileNumber], Lid.Rotation, Lid.Flip);
            this.Coors.Add(new VertexPositionNormalTexture(frontCoords.TopRight, Vector3.Zero, texPos[2]));
            this.Coors.Add(new VertexPositionNormalTexture(frontCoords.BottomRight, Vector3.Zero, texPos[1]));
            this.Coors.Add(new VertexPositionNormalTexture(frontCoords.TopLeft, Vector3.Zero, texPos[3]));
            this.Coors.Add(new VertexPositionNormalTexture(frontCoords.BottomLeft, Vector3.Zero, texPos[0]));

            var startIndex = Coors.Count - 4;
            this.IndexBufferCollection.Add(startIndex);
            this.IndexBufferCollection.Add(startIndex + 1);
            this.IndexBufferCollection.Add(startIndex + 2);
            this.IndexBufferCollection.Add(startIndex + 1);
            this.IndexBufferCollection.Add(startIndex + 3);
            this.IndexBufferCollection.Add(startIndex + 2);
            
        }

        private void CreateTopVertices(FaceCoordinates frontCoords, FaceCoordinates backCoords)
        {
            if (!Top)
                return;
            
            var texPos = GetTexturePositions(tileAtlas[this.Top.TileNumber], this.Lid.Rotation, this.Lid.Flip);
            Coors.Add(new VertexPositionNormalTexture(frontCoords.TopRight, Vector3.Zero, texPos[0]));
            Coors.Add(new VertexPositionNormalTexture(backCoords.TopLeft, Vector3.Zero, texPos[2]));
            Coors.Add(new VertexPositionNormalTexture(backCoords.TopRight, Vector3.Zero, texPos[3]));
            Coors.Add(new VertexPositionNormalTexture(frontCoords.TopLeft, Vector3.Zero, texPos[1]));

            var startIndex = Coors.Count - 4;
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex + 1);
            IndexBufferCollection.Add(startIndex + 2);
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex + 3);
            IndexBufferCollection.Add(startIndex + 1);            
        }

        private void CreateBottomVertices(FaceCoordinates frontCoords, FaceCoordinates backCoords)
        {
            if (!Bottom)
                return;
            
            var texPos = GetTexturePositions(tileAtlas[this.Bottom.TileNumber], this.Lid.Rotation, this.Lid.Flip);
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

        private void CreateLeftVertices(FaceCoordinates frontCoords, FaceCoordinates backCoords, Byte rotation)
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
            var texPos = GetTexturePositions(tileAtlas[this.Left.TileNumber], this.Lid.Rotation, this.Lid.Flip);
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
            if (!Right)
                return;

            var newFront = new FaceCoordinates();
            var newBack = new FaceCoordinates();
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
            var texPos = GetTexturePositions(tileAtlas[this.Right.TileNumber], this.Lid.Rotation, this.Lid.Flip);
            Coors.Add(new VertexPositionNormalTexture(newFront.TopLeft, Vector3.Zero, texPos[2]));
            Coors.Add(new VertexPositionNormalTexture(newFront.BottomLeft, Vector3.Zero, texPos[3]));
            Coors.Add(new VertexPositionNormalTexture(newBack.BottomLeft, Vector3.Zero, texPos[0]));
            Coors.Add(new VertexPositionNormalTexture(newBack.TopLeft, Vector3.Zero, texPos[1]));

            //...
            var startIndex = Coors.Count - 4;
            IndexBufferCollection.Add(startIndex + 2);
            IndexBufferCollection.Add(startIndex + 1);
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex);
            IndexBufferCollection.Add(startIndex + 3);
            IndexBufferCollection.Add(startIndex + 2);
        }

        protected FaceCoordinates CorrectLeftRightVertices(FaceCoordinates coordinates, Boolean left)
        {
            var newCoords = new FaceCoordinates();

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
    }
}
