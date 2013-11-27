// GTA2.NET
// 
// File: PartialLeftBlock.cs
// Created: 05.05.2013
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
using Hiale.GTA2NET.Core.Helper;

namespace Hiale.GTA2NET.Core.Map.Blocks
{
    public class PartialLeftBlock : Block
    {
        public PartialLeftBlock() : base()
        {
            SlopeType = SlopeType.PartialBlockLeft;
        }

        public PartialLeftBlock(BlockStructure blockStructure, Vector3 pos) : base(blockStructure, pos) 
        {
            SlopeType = SlopeType.PartialBlockLeft;
        }

        public override void SetUpCube()
        {
            FaceCoordinates frontCoordinates;
            FaceCoordinates backCoordinates;
            PrepareCoordinates(0, 0, PartialBlockScalar, 1, out frontCoordinates, out backCoordinates);

            Vector2[] texture = textures.GetRectangleTexture((UInt32)Lid.TileNumber, Lid.Rotation, Lid.Flip, Textures.RectangleTexturePosition.Left);
            CreateFrontVertices(frontCoordinates, texture);

            // Top face
            texture = textures.GetRectangleTexture((UInt32)Top.TileNumber, Top.Rotation, Top.Flip, Textures.RectangleTexturePosition.Left);
            CreateTopVertices(frontCoordinates, backCoordinates, texture);

            // Bottom face
            texture = textures.GetRectangleTexture((UInt32)Bottom.TileNumber, Bottom.Rotation, Bottom.Flip, Textures.RectangleTexturePosition.Left);
            CreateBottomVertices(frontCoordinates, backCoordinates, texture);

            // Left face
            texture = textures.GetNormalTexture((UInt32)Left.TileNumber, Left.Rotation, Left.Flip);
            CreateLeftVertices(frontCoordinates, backCoordinates, texture);

            // Right face
            texture = textures.GetNormalTexture((UInt32)Right.TileNumber, Right.Rotation, Right.Flip);
            CreateRightVertices(frontCoordinates, backCoordinates, texture);
        }
        
        public override void GetCollision(List<ILineObstacle> obstacles, bool bulletWall)
        {
            if (DoesWallCollide(Left, bulletWall))
                obstacles.Add(GetDefaultLeftCollison());
            if (DoesWallCollide(Top, bulletWall))
                obstacles.Add(new LineObstacle(new Vector2(Position.X, Position.Y), new Vector2(Position.X + PartialBlockScalar, Position.Y), (int)Position.Z));
            if (DoesWallCollide(Right, bulletWall))
                obstacles.Add(new LineObstacle(new Vector2(Position.X + PartialBlockScalar, Position.Y), new Vector2(Position.X + PartialBlockScalar, Position.Y + 1), (int)Position.Z));
            if (DoesWallCollide(Bottom, bulletWall))
                obstacles.Add(new LineObstacle(new Vector2(Position.X + PartialBlockScalar, Position.Y + 1), new Vector2(Position.X, Position.Y + 1), (int) Position.Z));
        }
    }
}
