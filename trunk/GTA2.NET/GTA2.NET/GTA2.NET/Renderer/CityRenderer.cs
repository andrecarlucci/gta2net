//08.02.2010

using System.Collections.Generic;
using Hiale.GTA2NET.Core.Helper;
using Microsoft.Xna.Framework.Graphics;
using Hiale.GTA2NET.Core.Map;
using Hiale.GTA2NET.Helper;
using Microsoft.Xna.Framework;
using System.IO;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Hiale.GTA2NET.Renderer
{
    public class CityRenderer
    {
        BasicEffect effect;

        //City textures
        Texture2D cityTexture;
        Dictionary<int, Rectangle> tileAtlas;
        Vector2[] texturePosition;

        //Triangle stuff
        VertexBuffer vertexBuffer;
        //VertexDeclaration vertexDeclaration;
        IndexBuffer indexBuffer;
        List<int> indexBufferCollection;
        List<VertexPositionNormalTexture> cityVerticesCollection;

        //Options
        private const float UnitSize = 1f;

        public CityRenderer()
        {
            effect = new BasicEffect(BaseGame.Device);
            cityVerticesCollection = new List<VertexPositionNormalTexture>();
            indexBufferCollection = new List<int>();
            texturePosition = new Vector2[4];

            MainGame.GlobalScalar = new Vector3(1, 1, 0.5f);
        }

        public void LoadCity()
        {
            LoadTexture();
            //LoadMap();
            SetUpCity();
            CopyToGraphicsDevice();
            SetUpEffect();
        }

        //private void LoadMap()
        //{
        //    MainGame.Map = new Map();
        //    MainGame.Map.ReadFromFile("data\\MP1-comp.gmp");
        //    //MainGame.Map.ReadFromFile("data\\bil.gmp");

        //    //var collision = new MapCollision(MainGame.Map);
        //}

        private void SetUpCity()
        {
            for (var z = 0; z < MainGame.Map.CityBlocks.GetLength(2); z++)
            {
                SetUpLayer(ref z, false);
                SetUpLayer(ref z, true);
            }
        }


        private void SetUpLayer(ref int z, bool lidLayer)
        {
            for (var x = 0; x < MainGame.Map.CityBlocks.GetLength(0); x++)
            {
                for (var y = 0; y < MainGame.Map.CityBlocks.GetLength(1); y++)
                {
                    var block = MainGame.Map.CityBlocks[x, y, z];
                    if (block.IsEmpty || SkipBlock(ref block))
                        continue;
                    var pos = new Vector3(x, y, z);
                    switch (block.SlopeType)
                    {
                        case SlopeType.None:
                            SetUpCube(block, pos, lidLayer);
                            break;
                        case SlopeType.Up26Low:
                            SetUpSlope_Low(block, pos, 26, 1);
                            break;
                        case SlopeType.Up26High:
                            SetUpSlope_High(block, pos, 26, 1);
                            break;
                        case SlopeType.Down26Low:
                            SetUpSlope_Low(block, pos, 26, 3);
                            break;
                        case SlopeType.Down26High:
                            SetUpSlope_High(block, pos, 26, 3);
                            break;
                        case SlopeType.Left26Low:
                            SetUpSlope_Low(block, pos, 26, 2);
                            break;
                        case SlopeType.Left26High:
                            SetUpSlope_High(block, pos, 26, 2);
                            break;
                        case SlopeType.Right26Low:
                            SetUpSlope_Low(block, pos, 26, 0);
                            break;
                        case SlopeType.Right26High:
                            SetUpSlope_High(block, pos, 26, 0);
                            break;
                        case SlopeType.Up7Low:
                            SetUpSlope_Low(block, pos, 7, 1);
                            break;
                        case SlopeType.Up7High0:
                            SetUpSlope_High(block, pos, 7, 1);
                            break;
                        case SlopeType.Up7High1:
                            SetUpSlope_High(block, pos, 8, 1);
                            break;
                        case SlopeType.Up7High2:
                            SetUpSlope_High(block, pos, 9, 1);
                            break;
                        case SlopeType.Up7High3:
                            SetUpSlope_High(block, pos, 10, 1);
                            break;
                        case SlopeType.Up7High4:
                            SetUpSlope_High(block, pos, 11, 1);
                            break;
                        case SlopeType.Up7High5:
                            SetUpSlope_High(block, pos, 12, 1);
                            break;
                        case SlopeType.Up7High6:
                            SetUpSlope_High(block, pos, 13, 1);
                            break;
                        case SlopeType.Down7Low:
                            SetUpSlope_Low(block, pos, 7, 3);
                            break;
                        case SlopeType.Down7High0:
                            SetUpSlope_High(block, pos, 7, 3);
                            break;
                        case SlopeType.Down7High1:
                            SetUpSlope_High(block, pos, 8, 3);
                            break;
                        case SlopeType.Down7High2:
                            SetUpSlope_High(block, pos, 9, 3);
                            break;
                        case SlopeType.Down7High3:
                            SetUpSlope_High(block, pos, 10, 3);
                            break;
                        case SlopeType.Down7High4:
                            SetUpSlope_High(block, pos, 11, 3);
                            break;
                        case SlopeType.Down7High5:
                            SetUpSlope_High(block, pos, 12, 3);
                            break;
                        case SlopeType.Down7High6:
                            SetUpSlope_High(block, pos, 13, 3);
                            break;
                        case SlopeType.Left7Low:
                            SetUpSlope_Low(block, pos, 7, 2);
                            break;
                        case SlopeType.Left7High0:
                            SetUpSlope_High(block, pos, 7, 2);
                            break;
                        case SlopeType.Left7High1:
                            SetUpSlope_High(block, pos, 8, 2);
                            break;
                        case SlopeType.Left7High2:
                            SetUpSlope_High(block, pos, 9, 2);
                            break;
                        case SlopeType.Left7High3:
                            SetUpSlope_High(block, pos, 10, 2);
                            break;
                        case SlopeType.Left7High4:
                            SetUpSlope_High(block, pos, 11, 2);
                            break;
                        case SlopeType.Left7High5:
                            SetUpSlope_High(block, pos, 12, 2);
                            break;
                        case SlopeType.Left7High6:
                            SetUpSlope_High(block, pos, 13, 2);
                            break;
                        case SlopeType.Right7Low:
                            SetUpSlope_Low(block, pos, 7, 0);
                            break;
                        case SlopeType.Right7High0:
                            SetUpSlope_High(block, pos, 7, 0);
                            break;
                        case SlopeType.Right7High1:
                            SetUpSlope_High(block, pos, 8, 0);
                            break;
                        case SlopeType.Right7High2:
                            SetUpSlope_High(block, pos, 9, 0);
                            break;
                        case SlopeType.Right7High3:
                            SetUpSlope_High(block, pos, 10, 0);
                            break;
                        case SlopeType.Right7High4:
                            SetUpSlope_High(block, pos, 11, 0);
                            break;
                        case SlopeType.Right7High5:
                            SetUpSlope_High(block, pos, 12, 0);
                            break;
                        case SlopeType.Right7High6:
                            SetUpSlope_High(block, pos, 13, 0);
                            break;
                        case SlopeType.Up45:
                            SetUpSlope_Low(block, pos, 45, 1);
                            break;
                        case SlopeType.Down45:
                            SetUpSlope_Low(block, pos, 45, 3);
                            break;
                        case SlopeType.Left45:
                            SetUpSlope_Low(block, pos, 45, 2);
                            break;
                        case SlopeType.Right45:
                            SetUpSlope_Low(block, pos, 45, 0);
                            break;
                        case SlopeType.DiagonalFacingUpLeft:
                            SetUpSlopeDiagonal(block, pos, 1);
                            break;
                        case SlopeType.DiagonalFacingUpRight:
                            SetUpSlopeDiagonal(block, pos, 0);
                            break;
                        case SlopeType.DiagonalFacingDownLeft:
                            SetUpSlopeDiagonal(block, pos, 2);
                            break;
                        case SlopeType.DiagonalFacingDownRight:
                            SetUpSlopeDiagonal(block, pos, 3);
                            break;
                        default:
                            SetUpCube(block, pos, lidLayer);
                            break;
                    }
                }
            }
        }

        private void SetUpCube(BlockInfo block, Vector3 pos, bool lidLayer)
        {
            FaceCoordinates frontCoordinates;
            FaceCoordinates backCoordinates;
            PrepareCoordinates(pos, out frontCoordinates, out backCoordinates);

            // Front face
            if (lidLayer)
            {
                CreateFrontVertices(ref frontCoordinates, ref backCoordinates, ref block);
                return;
            }
            // Top face
            CreateTopVertices(ref frontCoordinates, ref backCoordinates, ref block);
            // Bottom face
            CreateBottomVertices(ref frontCoordinates, ref backCoordinates, ref block);
            // Left face
            CreateLeftVertices(ref frontCoordinates, ref backCoordinates, ref block);
            // Right face
            CreateRightVertices(ref frontCoordinates, ref backCoordinates, ref block);
        }

        #region Slopes

        /// <summary>
        /// Roteate a slope by 90° (rotation = 1), 180° (rotation = 2) or 270° (rotation = 3)
        /// </summary>
        /// <param name="frontCoordinates"></param>
        /// <param name="rotation"></param>
        private static void RotateSlope(ref FaceCoordinates frontCoordinates, byte rotation)
        {
            for (var i = 0; i < rotation; i++)
            {
                var topLeft = frontCoordinates.BottomLeft;
                var topRight = frontCoordinates.TopLeft;
                var bottomRight = frontCoordinates.TopRight;
                var bottomLeft = frontCoordinates.BottomRight;

                frontCoordinates.TopLeft = topLeft;
                frontCoordinates.TopRight = topRight;
                frontCoordinates.BottomRight = bottomRight;
                frontCoordinates.BottomLeft = bottomLeft;
            }
        }

        private static void RotateEnum(ref RotationType type, byte rotation)
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
        }

        private void SetUpSlopeDiagonal(BlockInfo block, Vector3 pos, byte rotation)
        {
            FaceCoordinates frontCoordinates;
            FaceCoordinates backCoordinates;
            PrepareCoordinates(pos, out frontCoordinates, out backCoordinates);

            if (rotation > 0)
            {
                RotateSlope(ref frontCoordinates, rotation);
                RotateSlope(ref backCoordinates, rotation);
            }

            //Front face (diagonal)
            if (block.Lid)
            {
                RotationType lidRotation = block.Lid.Rotation;
                RotateEnum(ref lidRotation, rotation);

                if (block.Lid.Flip) //ToDo: This is just a dirty way! Problem: rotation Bug if flipped
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

                var texPos = GetTexturePositions(tileAtlas[block.Lid.TileNumber], lidRotation, block.Lid.Flip);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.TopLeft, Vector3.Zero, texPos[3]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.BottomRight, Vector3.Zero, texPos[1]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.BottomLeft, Vector3.Zero, texPos[0]));

                var startIndex = cityVerticesCollection.Count - 3;
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex + 2);
            }

            //int TileNumber = 0;
            BlockFace diagonalFace = null;
            switch (rotation)
            {
                case 0:
                    diagonalFace = block.Right;
                    break;
                case 1:
                    diagonalFace = block.Left;
                    break;
                case 2:
                    diagonalFace = block.Left;
                    break;
                case 3:
                    diagonalFace = block.Right;
                    break;
            }

            //Diagonal face
            if (diagonalFace)
            {
                var texPos = GetTexturePositions(tileAtlas[diagonalFace.TileNumber], diagonalFace.Rotation, diagonalFace.Flip);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.TopLeft, Vector3.Zero, texPos[3]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.BottomRight, Vector3.Zero, texPos[2]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.BottomRight, Vector3.Zero, texPos[1]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.TopLeft, Vector3.Zero, texPos[0]));

                var startIndex = cityVerticesCollection.Count - 4;
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 3);
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex);
            }

            PrepareCoordinates(pos, out frontCoordinates, out backCoordinates);
            switch (rotation)
            {
                case 0: //Facing up right
                    CreateBottomVertices(ref frontCoordinates, ref backCoordinates, ref block);
                    CreateLeftVertices(ref frontCoordinates, ref backCoordinates, ref block);
                    break;
                case 1: //Facing up left
                    CreateBottomVertices(ref frontCoordinates, ref backCoordinates, ref block);
                    CreateRightVertices(ref frontCoordinates, ref backCoordinates, ref block);
                    break;
                case 2: //Facing down left --> BUG
                    CreateTopVertices(ref frontCoordinates, ref backCoordinates, ref block);
                    CreateRightVertices(ref frontCoordinates, ref backCoordinates, ref block);
                    break;
                case 3: //Facing down right --> BUG
                    CreateTopVertices(ref frontCoordinates, ref backCoordinates, ref block);
                    CreateLeftVertices(ref frontCoordinates, ref backCoordinates, ref block);
                    break;
            }
        }

        private void SetUpSlope_Low(BlockInfo block, Vector3 pos, byte subType, byte rotation)
        {
            //Sample is a right slope, use it for orientation, it gets rotated to fit all other directions
            FaceCoordinates frontCoordinates;
            FaceCoordinates backCoordinates;
            PrepareCoordinates(pos, out frontCoordinates, out backCoordinates);

            var slopeScalar = 1f;
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

            var middleTopLeft = frontCoordinates.TopLeft;
            var middleTopRight = frontCoordinates.TopRight;
            var middleBottomRight = frontCoordinates.BottomRight;
            var middleBottomLeft = frontCoordinates.BottomLeft;
            var middleCoordinates = new FaceCoordinates(ref middleTopLeft, ref middleTopRight, ref middleBottomRight, ref middleBottomLeft);

            if (rotation > 0)
            {
                RotateSlope(ref frontCoordinates, rotation);
                RotateSlope(ref backCoordinates, rotation);
                RotateSlope(ref middleCoordinates, rotation);
            }

            middleCoordinates.TopLeft.Z -= slopeScalar * MainGame.GlobalScalar.Z;
            middleCoordinates.TopRight.Z -= slopeScalar * MainGame.GlobalScalar.Z;
            middleCoordinates.BottomRight.Z -= slopeScalar * MainGame.GlobalScalar.Z;
            middleCoordinates.BottomLeft.Z -= slopeScalar * MainGame.GlobalScalar.Z;

            //Front face (diagonal)
            if (block.Lid)
            {
                RotationType lidRotation = block.Lid.Rotation;
                RotateEnum(ref lidRotation, rotation);
                var texPos = GetTexturePositions(tileAtlas[block.Lid.TileNumber], lidRotation, block.Lid.Flip);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.TopRight, Vector3.Zero, texPos[2]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.BottomRight, Vector3.Zero, texPos[1]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.BottomLeft, Vector3.Zero, texPos[0]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.TopLeft, Vector3.Zero, texPos[3]));

                var startIndex = cityVerticesCollection.Count - 4;
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex + 3);
            }

            BlockFace topFace = null;
            BlockFace bottomFace = null;
            BlockFace rightFace = null;
            switch (rotation)
            {
                case 0:
                    topFace = block.Top;
                    bottomFace = block.Bottom;
                    rightFace = block.Right;
                    break;
                case 1:
                    topFace = block.Left;
                    bottomFace = block.Right;
                    rightFace = block.Top;
                    break;
                case 2:
                    topFace = block.Bottom;
                    bottomFace = block.Top;
                    rightFace = block.Left;
                    break;
                case 3:
                    topFace = block.Right;
                    bottomFace = block.Left;
                    rightFace = block.Bottom;
                    break;

            }

            //Top face
            if (topFace)
            {
                var texPos = GetTexturePositions(tileAtlas[topFace.TileNumber], topFace.Rotation, topFace.Flip);
                var center = GetCenterPosition(ref texPos[3], ref texPos[0], slopeScalar);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.TopRight, Vector3.Zero, center));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.TopRight, Vector3.Zero, texPos[0]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.TopLeft, Vector3.Zero, texPos[1]));
 
                int startIndex = cityVerticesCollection.Count - 3;
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex + 0);
            }
            //Bottom face
            if (bottomFace)
            {
                var texPos = GetTexturePositions(tileAtlas[bottomFace.TileNumber], bottomFace.Rotation, bottomFace.Flip);
                var center = GetCenterPosition(ref texPos[2], ref texPos[1], slopeScalar);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.BottomRight, Vector3.Zero, center));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.BottomRight, Vector3.Zero, texPos[1]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.BottomLeft, Vector3.Zero, texPos[0]));

                int startIndex = cityVerticesCollection.Count - 3;
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex + 2);
            }

            //Right face
            if (rightFace) //this face is not supported by GTA2, the editor removes this face.
            {
                var texPos = GetTexturePositions(tileAtlas[rightFace.TileNumber], rightFace.Rotation, rightFace.Flip);
                var center = GetCenterPosition(ref texPos[1], ref texPos[2], slopeScalar);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.TopRight, Vector3.Zero, center));
                center = GetCenterPosition(ref texPos[0], ref texPos[3], slopeScalar);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.BottomRight, Vector3.Zero, center));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.BottomRight, Vector3.Zero, texPos[3]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.TopRight, Vector3.Zero, texPos[2]));

                var startIndex = cityVerticesCollection.Count - 4;
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex + 0);
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 3);
                indexBufferCollection.Add(startIndex + 2);
            }           
        }

        private void SetUpSlope_High(BlockInfo block, Vector3 pos, byte subType, byte rotation)
        {
            //Sample is a right slope, use it for orientation, it gets rotated to fit all other directions
            FaceCoordinates frontCoordinates;
            FaceCoordinates backCoordinates;
            PrepareCoordinates(pos, out frontCoordinates, out backCoordinates);

            var middleSlopeScalar = 1f;
            var frontSlopeScalar = 0f;
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

            var middleTopLeft = frontCoordinates.TopLeft;
            var middleTopRight = frontCoordinates.TopRight;
            var middleBottomRight = frontCoordinates.BottomRight;
            var middleBottomLeft = frontCoordinates.BottomLeft;
            var middleCoordinates = new FaceCoordinates(ref middleTopLeft, ref middleTopRight, ref middleBottomRight, ref middleBottomLeft);

            if (rotation > 0)
            {
                RotateSlope(ref frontCoordinates, rotation);
                RotateSlope(ref backCoordinates, rotation);
                RotateSlope(ref middleCoordinates, rotation);
            }

            frontCoordinates.TopLeft.Z -= frontSlopeScalar * MainGame.GlobalScalar.Z;
            frontCoordinates.TopRight.Z -= frontSlopeScalar * MainGame.GlobalScalar.Z;
            frontCoordinates.BottomRight.Z -= frontSlopeScalar * MainGame.GlobalScalar.Z;
            frontCoordinates.BottomLeft.Z -= frontSlopeScalar * MainGame.GlobalScalar.Z;

            middleCoordinates.TopLeft.Z -= middleSlopeScalar * MainGame.GlobalScalar.Z;
            middleCoordinates.TopRight.Z -= middleSlopeScalar * MainGame.GlobalScalar.Z;
            middleCoordinates.BottomRight.Z -= middleSlopeScalar * MainGame.GlobalScalar.Z;
            middleCoordinates.BottomLeft.Z -= middleSlopeScalar * MainGame.GlobalScalar.Z;

            //Front face (diagonal)
            if (block.Lid)
            {
                var lidRotation = block.Lid.Rotation;
                RotateEnum(ref lidRotation, rotation);
                var texPos = GetTexturePositions(tileAtlas[block.Lid.TileNumber], lidRotation, block.Lid.Flip);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.TopRight, Vector3.Zero, texPos[2]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.BottomRight, Vector3.Zero, texPos[1]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.BottomLeft, Vector3.Zero, texPos[0]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.TopLeft, Vector3.Zero, texPos[3]));

                var startIndex = cityVerticesCollection.Count - 4;
                indexBufferCollection.Add(startIndex + 0);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex + 0);
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex + 3);
            }

            BlockFace topFace = null;
            BlockFace bottomFace = null;
            BlockFace leftFace = null;
            BlockFace rightFace = null;
            switch (rotation)
            {
                case 0:
                    topFace = block.Top;
                    bottomFace = block.Bottom;
                    leftFace = block.Left;
                    rightFace = block.Right;
                    break;
                case 1:
                    topFace = block.Left;
                    bottomFace = block.Right;
                    leftFace = block.Bottom;
                    rightFace = block.Top;
                    break;
                case 2:
                    topFace = block.Bottom;
                    bottomFace = block.Top;
                    leftFace = block.Right;
                    rightFace = block.Left;
                    break;
                case 3:
                    topFace = block.Right;
                    bottomFace = block.Left;
                    leftFace = block.Top;
                    rightFace = block.Bottom;
                    break;
            }

            //Top face
            if (topFace)
            {
                var texPos = GetTexturePositions(tileAtlas[topFace.TileNumber], topFace.Rotation, topFace.Flip);
                var center = GetCenterPosition(ref texPos[0], ref texPos[3], frontSlopeScalar);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.TopRight, Vector3.Zero, center)); //was 3
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.TopRight, Vector3.Zero, texPos[0]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.TopLeft, Vector3.Zero, texPos[1]));
                center = GetCenterPosition(ref texPos[1], ref texPos[2], middleSlopeScalar);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.TopLeft, Vector3.Zero, center));

                int startIndex = cityVerticesCollection.Count - 4;
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex + 0);

                indexBufferCollection.Add(startIndex + 3);
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex + 0);
            }
            //Bottom face
            if (bottomFace)
            {
                var texPos = GetTexturePositions(tileAtlas[bottomFace.TileNumber], bottomFace.Rotation, bottomFace.Flip);
                var center = GetCenterPosition(ref texPos[2], ref texPos[1], frontSlopeScalar);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.BottomRight, Vector3.Zero, center)); //was texPos[2]
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.BottomRight, Vector3.Zero, texPos[1]));                
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.BottomLeft, Vector3.Zero, texPos[0]));
                center = GetCenterPosition(ref texPos[3], ref texPos[0], middleSlopeScalar);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.BottomLeft, Vector3.Zero, center));

                var startIndex = cityVerticesCollection.Count - 4;
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex + 2);

                indexBufferCollection.Add(startIndex + 0);
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex + 3);
            }

            //ToDo Left face (but probably not supported in GTA2 anyway)

            //Right face
            CreateRightVertices(ref frontCoordinates, ref backCoordinates, ref block, rightFace, rotation);
        }

        #endregion

        private static bool SkipBlock(ref BlockInfo block)
        {
            if (block.Lid.TileNumber == 608 || block.Lid.TileNumber == 1023 || block.Lid.TileNumber == 6)
                return true; //608 = Water, 1023 = dummy, 6 = crap?!
            return false;
        }

        private static void PrepareCoordinates(Vector3 position, out FaceCoordinates frontCoords, out FaceCoordinates backCoords)
        {
            PrepareCoordinates(position, MainGame.GlobalScalar, out frontCoords, out backCoords);            
        }

        private static void PrepareCoordinates(Vector3 position, Vector3 scalar, out FaceCoordinates frontCoords, out FaceCoordinates backCoords)
        {
            position.Y *= -1;

            //Coordinates of the cube
            var topLeftFront = (new Vector3(0.0f, 0.0f, UnitSize) + position) * scalar;
            var topRightFront = (new Vector3(UnitSize, 0.0f, UnitSize) + position) * scalar;
            var bottomLeftFront = (new Vector3(0.0f, -UnitSize, UnitSize) + position) * scalar;
            var bottomRightFront = (new Vector3(UnitSize, -UnitSize, UnitSize) + position) * scalar;
            frontCoords = new FaceCoordinates(ref topLeftFront, ref topRightFront, ref bottomRightFront, ref bottomLeftFront);

            var topLeftBack = (new Vector3(0.0f, 0.0f, 0.0f) + position) * scalar;
            var topRightBack = (new Vector3(UnitSize, 0.0f, 0.0f) + position) * scalar;
            var bottomLeftBack = (new Vector3(0.0f, -UnitSize, 0.0f) + position) * scalar;
            var bottomRightBack = (new Vector3(UnitSize, -UnitSize, 0.0f) + position) * scalar;
            backCoords = new FaceCoordinates(ref topLeftBack, ref topRightBack, ref bottomRightBack, ref bottomLeftBack);
        }


        private void CreateFrontVertices(ref FaceCoordinates frontCoords, ref FaceCoordinates backCoords, ref BlockInfo block)
        {
            if (!block.Lid)
                return;
            var texPos = GetTexturePositions(tileAtlas[block.Lid.TileNumber], block.Lid.Rotation, block.Lid.Flip);
            //Vector2[] TexPos = GetTexturePositions(tileAtlas[991], block.Lid.Rotation, block.Lid.Flip);
            cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoords.TopRight, Vector3.Zero, texPos[2]));
            cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoords.BottomRight, Vector3.Zero, texPos[1]));
            cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoords.TopLeft, Vector3.Zero, texPos[3]));
            cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoords.BottomLeft, Vector3.Zero, texPos[0]));

            var startIndex = cityVerticesCollection.Count - 4;
            indexBufferCollection.Add(startIndex);
            indexBufferCollection.Add(startIndex + 1);
            indexBufferCollection.Add(startIndex + 2);
            indexBufferCollection.Add(startIndex + 1);
            indexBufferCollection.Add(startIndex + 3);
            indexBufferCollection.Add(startIndex + 2);
        }

        private void CreateTopVertices(ref FaceCoordinates frontCoords, ref FaceCoordinates backCoords, ref BlockInfo block)
        {
            if (!block.Top)
                return;
            var texPos = GetTexturePositions(tileAtlas[block.Top.TileNumber], block.Top.Rotation, block.Top.Flip);
            cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoords.TopRight, Vector3.Zero, texPos[0]));
            cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoords.TopLeft, Vector3.Zero, texPos[2]));
            cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoords.TopRight, Vector3.Zero, texPos[3]));
            cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoords.TopLeft, Vector3.Zero, texPos[1]));

            var startIndex = cityVerticesCollection.Count - 4;
            indexBufferCollection.Add(startIndex);
            indexBufferCollection.Add(startIndex + 1);
            indexBufferCollection.Add(startIndex + 2);
            indexBufferCollection.Add(startIndex);
            indexBufferCollection.Add(startIndex + 3);
            indexBufferCollection.Add(startIndex + 1);
        }

        private void CreateBottomVertices(ref FaceCoordinates frontCoords, ref FaceCoordinates backCoords, ref BlockInfo block)
        {
            if (!block.Bottom)
                return;
            var texPos = GetTexturePositions(tileAtlas[block.Bottom.TileNumber], block.Bottom.Rotation, block.Bottom.Flip);
            cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoords.BottomRight, Vector3.Zero, texPos[2]));
            cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoords.BottomRight, Vector3.Zero, texPos[1]));
            cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoords.BottomLeft, Vector3.Zero, texPos[0]));
            cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoords.BottomLeft, Vector3.Zero, texPos[3]));

            var startIndex = cityVerticesCollection.Count - 4;
            indexBufferCollection.Add(startIndex);
            indexBufferCollection.Add(startIndex + 1);
            indexBufferCollection.Add(startIndex + 2);
            indexBufferCollection.Add(startIndex);
            indexBufferCollection.Add(startIndex + 2);
            indexBufferCollection.Add(startIndex + 3);
        }

        private void CreateLeftVertices(ref FaceCoordinates frontCoords, ref FaceCoordinates backCoords, ref BlockInfo block)
        {
            CreateLeftVertices(ref frontCoords, ref backCoords, ref block, block.Left, 0);
        }

        private void CreateLeftVertices(ref FaceCoordinates frontCoords, ref FaceCoordinates backCoords, ref BlockInfo block, BlockFace leftFace, byte rotation)
        {
            if (!leftFace)
                return;
            var newFront = new FaceCoordinates();
            var newBack = new FaceCoordinates();
            switch (rotation)
            {
                case 0:
                    newFront = CorrectLeftRightVertices(frontCoords, true);
                    newBack = CorrectLeftRightVertices(backCoords, true);
                    break;
                case 2:
                    newFront = CorrectLeftRightVertices(frontCoords, false);
                    newBack = CorrectLeftRightVertices(backCoords, false);
                    break;
            }
            var texPos = GetTexturePositions(tileAtlas[leftFace.TileNumber], leftFace.Rotation, leftFace.Flip);
            cityVerticesCollection.Add(new VertexPositionNormalTexture(newFront.TopRight, Vector3.Zero, texPos[3]));
            cityVerticesCollection.Add(new VertexPositionNormalTexture(newBack.BottomRight, Vector3.Zero, texPos[1]));
            cityVerticesCollection.Add(new VertexPositionNormalTexture(newFront.BottomRight, Vector3.Zero, texPos[2]));
            cityVerticesCollection.Add(new VertexPositionNormalTexture(newBack.TopRight, Vector3.Zero, texPos[0]));

            //Left also has a strange index buffer order...
            var startIndex = cityVerticesCollection.Count - 4;
            indexBufferCollection.Add(startIndex + 2);
            indexBufferCollection.Add(startIndex + 1);
            indexBufferCollection.Add(startIndex);
            indexBufferCollection.Add(startIndex + 1);
            indexBufferCollection.Add(startIndex + 3);
            indexBufferCollection.Add(startIndex);
        }

        private void CreateRightVertices(ref FaceCoordinates frontCoords, ref FaceCoordinates backCoords, ref BlockInfo block)
        {
            CreateRightVertices(ref frontCoords, ref backCoords, ref block, block.Right, 0);
        }

        private void CreateRightVertices(ref FaceCoordinates frontCoords, ref FaceCoordinates backCoords, ref BlockInfo block, BlockFace rightFace, byte rotation)
        {
            if (!rightFace)
                return;
            var newFront = new FaceCoordinates();
            var newBack = new FaceCoordinates();
            switch (rotation)
            {
                case 0:
                    newFront = CorrectLeftRightVertices(frontCoords, false);
                    newBack = CorrectLeftRightVertices(backCoords, false);
                    break;
                case 2:
                    newFront = CorrectLeftRightVertices(frontCoords, true);
                    newBack = CorrectLeftRightVertices(backCoords, true);
                    break;
            }
            //ToDo: Add more rotation codes...
            var texPos = GetTexturePositions(tileAtlas[rightFace.TileNumber], rightFace.Rotation, rightFace.Flip);
            cityVerticesCollection.Add(new VertexPositionNormalTexture(newFront.TopLeft, Vector3.Zero, texPos[2]));
            cityVerticesCollection.Add(new VertexPositionNormalTexture(newFront.BottomLeft, Vector3.Zero, texPos[3]));
            cityVerticesCollection.Add(new VertexPositionNormalTexture(newBack.BottomLeft, Vector3.Zero, texPos[0]));
            cityVerticesCollection.Add(new VertexPositionNormalTexture(newBack.TopLeft, Vector3.Zero, texPos[1]));

            //...
            var startIndex = cityVerticesCollection.Count - 4;
            indexBufferCollection.Add(startIndex + 2);
            indexBufferCollection.Add(startIndex + 1);
            indexBufferCollection.Add(startIndex);
            indexBufferCollection.Add(startIndex);
            indexBufferCollection.Add(startIndex + 3);
            indexBufferCollection.Add(startIndex + 2);
        }

        private static FaceCoordinates CorrectLeftRightVertices(FaceCoordinates coordinates, bool left)
        {
            var newCoords = new FaceCoordinates();

            float value;
            if (left)
                value = -MainGame.GlobalScalar.X;
            else
                value = MainGame.GlobalScalar.X;

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

        private Vector2[] GetTexturePositions(Rectangle sourceRectangle, RotationType rotation, bool flip)
        {
            double pixelPerWidth = 1f/cityTexture.Width;
            double pixelPerHeight = 1f/cityTexture.Height;

            var texTopLeft = new Vector2((float)((sourceRectangle.X + 1) * pixelPerWidth), (float)((sourceRectangle.Y + 1) * pixelPerHeight));
            var texTopRight = new Vector2((float)((sourceRectangle.X + sourceRectangle.Width - 1) * pixelPerWidth), (float)((sourceRectangle.Y + 1) * pixelPerHeight));
            var texBottomRight = new Vector2((float)((sourceRectangle.X + sourceRectangle.Width - 1) * pixelPerWidth), (float)((sourceRectangle.Y + sourceRectangle.Height - 1) * pixelPerHeight));
            var texBottomLeft = new Vector2((float)((sourceRectangle.X + 1) * pixelPerWidth), (float)((sourceRectangle.Y + sourceRectangle.Height - 1) * pixelPerHeight));
            
            if (flip)
            {
                var helper = texTopLeft;
                texTopLeft = texTopRight;
                texTopRight = helper;
                helper = texBottomLeft;
                texBottomLeft = texBottomRight;
                texBottomRight = helper;
                switch (rotation)
                {
                    case RotationType.Rotate90:
                        rotation = RotationType.Rotate270;
                        break;
                    case RotationType.Rotate270:
                        rotation = RotationType.Rotate90;
                        break;
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

        private static Vector2 GetCenterPosition(ref Vector2 lowerEnd, ref Vector2 higherEnd, float amount) //ToDo: method name
        {
            Vector2 center;
            center.X = MathHelper.Lerp(lowerEnd.X, higherEnd.X, amount);
            center.Y = MathHelper.Lerp(lowerEnd.Y, higherEnd.Y, amount);
            return center;
        }

        private void SetUpEffect()
        {
            effect.Texture = cityTexture;
            effect.TextureEnabled = true;
            effect.LightingEnabled = false;
        }

        private void CopyToGraphicsDevice()
        {
            var cubeVertices = cityVerticesCollection.ToArray();

            vertexBuffer = new VertexBuffer(BaseGame.Device, typeof(VertexPositionNormalTexture), cubeVertices.Length, BufferUsage.None);
            vertexBuffer.SetData(cubeVertices);

            var indexBufferData = indexBufferCollection.ToArray();
            indexBuffer = new IndexBuffer(BaseGame.Device, typeof(int), indexBufferData.Length, BufferUsage.None);
            indexBuffer.SetData(indexBufferData);
        }

        private void LoadTexture()
        {
            const string tilesDictPath = "Textures\\tiles.xml";
            TextureAtlasTiles dict;
            if (!File.Exists(tilesDictPath))
            {
                var zip = ZipStorer.Open("Textures\\bil.zip", FileAccess.Read);
                dict = new TextureAtlasTiles( "Textures\\tiles.png", zip);
                dict.BuildTextureAtlas();
                dict.Serialize(tilesDictPath);
                tileAtlas = dict.TileDictionary;
                var stream = new MemoryStream();
                dict.Image.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                stream.Position = 0;
                cityTexture = Texture2D.FromStream(BaseGame.Device, stream);
                stream.Close();
                dict.Dispose();                
            }
            else
            {
                dict = (TextureAtlasTiles)TextureAtlas.Deserialize(tilesDictPath, typeof(TextureAtlasTiles));
                tileAtlas = dict.TileDictionary;
                var fs = new FileStream(dict.ImagePath, FileMode.Open);
                cityTexture = Texture2D.FromStream(BaseGame.Device, fs);
                fs.Close();
            }
        }

        public void DrawCity()
        {   
            effect.View = BaseGame.ViewMatrix;
            effect.Projection = BaseGame.ProjectionMatrix;
            effect.World = BaseGame.WorldMatrix;

            //effect.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
            //effect.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            //effect.GraphicsDevice.RenderState.CullMode = CullMode.None;
            //effect.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;


            effect.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            effect.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            effect.GraphicsDevice.BlendState = BaseGame.AlphaBlendingState;

            BaseGame.Device.SetVertexBuffer(vertexBuffer);
            BaseGame.Device.Indices = indexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                //pass.Begin(); //XNA 3.1

                //Anisotropic
                //effect.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;
                //effect.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Linear;
                //effect.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
                //effect.GraphicsDevice.SamplerStates[0].MaxAnisotropy = 16;

                //effect.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Point;
                //effect.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Point;
                //effect.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Point;
                pass.Apply();
                effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, cityVerticesCollection.Count, 0, indexBufferCollection.Count / 3);
            }
        }      
    }
}
