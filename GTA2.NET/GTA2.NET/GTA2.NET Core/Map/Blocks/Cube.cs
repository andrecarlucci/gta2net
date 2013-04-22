using System;
using Hiale.GTA2NET.Core.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hiale.GTA2NET.Core.Map.Blocks
{
    public class Cube : BlockInfo
    {
        public Cube(blockInfo blockInfo, Vector3 pos):base(blockInfo, pos)
        {
        }

        public Cube(): base()
        {
        }

        public override BlockInfo DeepCopy()
        {
            return new Cube(this.blockInfo, this.Position);
        }

        public override BlockInfo DeepCopy(blockInfo blockInfo, Vector3 pos)
        {
            return new Cube(blockInfo, Position);
        }

        public override bool IsMe(SlopeType slopeType)
        {
            return (slopeType == SlopeType.None);
            //return true;
        }


        public override bool IsEmpty
        {
            get
            {
                return false;
            }
        }

        protected override void SetUpCube()
        {
            
            FaceCoordinates frontCoordinates;
            FaceCoordinates backCoordinates;
            PrepareCoordinates(this.Position, out frontCoordinates, out backCoordinates);
            if (this.Position == new Vector3(68, 187, 3))
                System.Console.Write("");
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
            if (this.Lid.TileNumber > 0)
            {
                Vector2[] texPos = GetTexturePositions(tileAtlas[this.Lid.TileNumber], this.Lid.Rotation, this.Lid.Flip);
                this.Coors.Add(new VertexPositionNormalTexture(frontCoords.TopRight, Vector3.Zero, texPos[2]));
                this.Coors.Add(new VertexPositionNormalTexture(frontCoords.BottomRight, Vector3.Zero, texPos[1]));
                this.Coors.Add(new VertexPositionNormalTexture(frontCoords.TopLeft, Vector3.Zero, texPos[3]));
                this.Coors.Add(new VertexPositionNormalTexture(frontCoords.BottomLeft, Vector3.Zero, texPos[0]));

                int startIndex = Coors.Count - 4;
                this.IndexBufferCollection.Add(startIndex);
                this.IndexBufferCollection.Add(startIndex + 1);
                this.IndexBufferCollection.Add(startIndex + 2);
                this.IndexBufferCollection.Add(startIndex + 1);
                this.IndexBufferCollection.Add(startIndex + 3);
                this.IndexBufferCollection.Add(startIndex + 2);
            }
        }

        private void CreateTopVertices(FaceCoordinates frontCoords, FaceCoordinates backCoords)
        {
            if (this.Top.TileNumber > 0)
            {
                Vector2[] texPos = GetTexturePositions(tileAtlas[this.Top.TileNumber], this.Lid.Rotation, this.Lid.Flip);
                Coors.Add(new VertexPositionNormalTexture(frontCoords.TopRight, Vector3.Zero, texPos[0]));
                Coors.Add(new VertexPositionNormalTexture(backCoords.TopLeft, Vector3.Zero, texPos[2]));
                Coors.Add(new VertexPositionNormalTexture(backCoords.TopRight, Vector3.Zero, texPos[3]));
                Coors.Add(new VertexPositionNormalTexture(frontCoords.TopLeft, Vector3.Zero, texPos[1]));

                int startIndex = Coors.Count - 4;
                IndexBufferCollection.Add(startIndex);
                IndexBufferCollection.Add(startIndex + 1);
                IndexBufferCollection.Add(startIndex + 2);
                IndexBufferCollection.Add(startIndex);
                IndexBufferCollection.Add(startIndex + 3);
                IndexBufferCollection.Add(startIndex + 1);
            }
        }

        private void CreateBottomVertices(FaceCoordinates frontCoords, FaceCoordinates backCoords)
        {
            if (this.Bottom.TileNumber > 0)
            {
                Vector2[] texPos = GetTexturePositions(tileAtlas[this.Bottom.TileNumber], this.Lid.Rotation, this.Lid.Flip);
                Coors.Add(new VertexPositionNormalTexture(frontCoords.BottomRight, Vector3.Zero, texPos[2]));
                Coors.Add(new VertexPositionNormalTexture(backCoords.BottomRight, Vector3.Zero, texPos[1]));
                Coors.Add(new VertexPositionNormalTexture(backCoords.BottomLeft, Vector3.Zero, texPos[0]));
                Coors.Add(new VertexPositionNormalTexture(frontCoords.BottomLeft, Vector3.Zero, texPos[3]));

                int startIndex = Coors.Count - 4;
                IndexBufferCollection.Add(startIndex);
                IndexBufferCollection.Add(startIndex + 1);
                IndexBufferCollection.Add(startIndex + 2);
                IndexBufferCollection.Add(startIndex);
                IndexBufferCollection.Add(startIndex + 2);
                IndexBufferCollection.Add(startIndex + 3);
            }
        }

        private void CreateLeftVertices(FaceCoordinates frontCoords, FaceCoordinates backCoords, Byte rotation)
        {
            if (this.Left.TileNumber > 0)
            {
                FaceCoordinates newFront = new FaceCoordinates();
                FaceCoordinates newBack = new FaceCoordinates();
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
                Vector2[] texPos = GetTexturePositions(tileAtlas[this.Left.TileNumber], this.Lid.Rotation, this.Lid.Flip);
                Coors.Add(new VertexPositionNormalTexture(newFront.TopRight, Vector3.Zero, texPos[3]));
                Coors.Add(new VertexPositionNormalTexture(newBack.BottomRight, Vector3.Zero, texPos[1]));
                Coors.Add(new VertexPositionNormalTexture(newFront.BottomRight, Vector3.Zero, texPos[2]));
                Coors.Add(new VertexPositionNormalTexture(newBack.TopRight, Vector3.Zero, texPos[0]));

                //Left also has a strange index buffer order...
                int startIndex = Coors.Count - 4;
                IndexBufferCollection.Add(startIndex + 2);
                IndexBufferCollection.Add(startIndex + 1);
                IndexBufferCollection.Add(startIndex);
                IndexBufferCollection.Add(startIndex + 1);
                IndexBufferCollection.Add(startIndex + 3);
                IndexBufferCollection.Add(startIndex);
            }
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
                Vector2[] texPos = GetTexturePositions(tileAtlas[this.Right.TileNumber], this.Lid.Rotation, this.Lid.Flip);
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


    }
}
