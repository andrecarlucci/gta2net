//08.02.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Hiale.GTA2NET.Core.Map;
using Hiale.GTA2NET.Helper;
using Microsoft.Xna.Framework;
using System.IO;

namespace Hiale.GTA2NET.Renderer
{
    public class CityRenderer
    {
        BasicEffect effect;

        //City textures
        Texture2D cityTexture;
        Dictionary<int, Rectangle> tileAtlas;
        Vector2[] texturePosition;
        Vector2 tiles;

        //Triangle stuff
        VertexBuffer vertexBuffer;
        VertexDeclaration vertexDeclaration;
        IndexBuffer indexBuffer;
        List<int> indexBufferCollection;
        List<VertexPositionNormalTexture> cityVerticesCollection;

        //Options
        private const float UnitSize = 1f;
        //private Vector3 globalScalar = Vector3.One;
        bool EnableSkipFace = false;

        public CityRenderer()
        {
            effect = new BasicEffect(BaseGame.Device, null);
            cityVerticesCollection = new List<VertexPositionNormalTexture>();
            indexBufferCollection = new List<int>();
            texturePosition = new Vector2[4];

            MainGame.GlobalScalar = new Vector3(1, 1, 0.5f);
        }

        public void LoadCity()
        {
            LoadTexture();
            LoadMap();
            SetUpCity();
            CopyToGraphicsDevice();
            SetUpEffect();
        }

        private void LoadMap()
        {
            MainGame.Map = new Map();
            //MainGame.Map.ReadFromFile("data\\bil.gmp");
            MainGame.Map.ReadFromFile("data\\MP1-comp.gmp");
            //MainGame.Map.ReadFromFile("data\\MP1-comp1.gmp");
            
        }

        private void SetUpCity()
        {
            for (int z = 0; z < MainGame.Map.CityBlocks.GetLength(2); z++)
            {
                //if (z > 1)
                //    return;
                SetUpLayer(ref z, false);
                SetUpLayer(ref z, true);
            }
        }


        private void SetUpLayer(ref int z, bool LidLayer)
        {
            for (int x = 0; x < MainGame.Map.CityBlocks.GetLength(0); x++)
            {
                for (int y = 0; y < MainGame.Map.CityBlocks.GetLength(1); y++)
                {
                    BlockInfo block = MainGame.Map.CityBlocks[x, y, z];
                    if (!block.IsEmpty && !SkipBlock(ref block))
                    {
                        //if (x == 71 && y == 198)
                        //    System.Diagnostics.Debug.WriteLine("OK");
                        block.Position = new Vector3(x, y, z);
                        switch (block.SlopeType)
                        {
                            case SlopeType.None:
                                SetUpCube(block, LidLayer);
                                break;
                            case SlopeType.Up26Low:
                                SetUpSlope_26Low(block, 1);
                                break;
                            case SlopeType.Up26High:
                                SetUpSlope_26High(block, 1);
                                break;
                            case SlopeType.Down26Low:
                                SetUpSlope_26Low(block, 3);
                                break;
                            case SlopeType.Down26High:
                                SetUpSlope_26High(block, 3);
                                break;
                            case SlopeType.Left26Low:
                                SetUpSlope_26Low(block, 2);
                                break;
                            case SlopeType.Left26High:
                                SetUpSlope_26High(block, 2);
                                break;
                            case SlopeType.Right26Low:
                                SetUpSlope_26Low(block, 0);
                                break;
                            case SlopeType.Right26High:
                                SetUpSlope_26High(block, 0);
                                break;
                            case SlopeType.DiagonalFacingUpLeft:
                                SetUpSlope_Diagonal(block, 1);
                                break;
                            case SlopeType.DiagonalFacingUpRight:
                                SetUpSlope_Diagonal(block, 0);
                                break;
                            case SlopeType.DiagonalFacingDownLeft:
                                SetUpSlope_Diagonal(block, 2);
                                break;
                            case SlopeType.DiagonalFacingDownRight:
                                SetUpSlope_Diagonal(block, 3);
                                break;
                            default:
                                SetUpCube(block, LidLayer);
                                break;
                        }
                    }
                }
            }
        }

        private void SetUpCube(BlockInfo block, bool lidLayer)
        {
            FaceCoordinates frontCoordinates;
            FaceCoordinates backCoordinates;
            PrepareCoordinates(block.Position, out frontCoordinates, out backCoordinates);

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

        private void SetUpSlope_Diagonal(BlockInfo block, byte rotation)
        {
            FaceCoordinates frontCoordinates;
            FaceCoordinates backCoordinates;
            PrepareCoordinates(block.Position, out frontCoordinates, out backCoordinates);

            if (rotation > 0)
            {
                RotateSlope(ref frontCoordinates, rotation);
                RotateSlope(ref backCoordinates, rotation);
            }

            //Front face (diagonal)
            if (block.Lid.TileNumber > 0)
            {
                RotationType lidRotation = block.Lid.Rotation;
                RotateEnum(ref lidRotation, rotation);

                if (block.Lid.Flip) //ToDo: This is just a dirty way! Problem: rotation Bug if flipped
                {
                    if (lidRotation == RotationType.Rotate90)
                        lidRotation = RotationType.Rotate270;
                    else if (lidRotation == RotationType.Rotate270)
                        lidRotation = RotationType.Rotate90;
                }                

                Vector2[] TexPos = GetTexturePositions(tileAtlas[block.Lid.TileNumber], lidRotation, block.Lid.Flip);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.TopLeft, Vector3.Zero, TexPos[3]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.BottomRight, Vector3.Zero, TexPos[1]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.BottomLeft, Vector3.Zero, TexPos[0]));

                int startIndex = cityVerticesCollection.Count - 3;
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
            if (diagonalFace.TileNumber > 0)
            {
                Vector2[] TexPos = GetTexturePositions(tileAtlas[diagonalFace.TileNumber], diagonalFace.Rotation, diagonalFace.Flip);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.TopLeft, Vector3.Zero, TexPos[3]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.BottomRight, Vector3.Zero, TexPos[2]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.BottomRight, Vector3.Zero, TexPos[1]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.TopLeft, Vector3.Zero, TexPos[0]));

                int startIndex = cityVerticesCollection.Count - 4;
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 3);
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex);
            }

            PrepareCoordinates(block.Position, out frontCoordinates, out backCoordinates);
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

        private void SetUpSlope_26Low(BlockInfo block, byte rotation)
        {
            //Sample is a right slope, use it for orientation, it gets rotated to fit all other directions
            FaceCoordinates frontCoordinates;
            FaceCoordinates backCoordinates;
            PrepareCoordinates(block.Position, out frontCoordinates, out backCoordinates);

            Vector3 middleTopLeft = frontCoordinates.TopLeft;
            Vector3 middleTopRight = frontCoordinates.TopRight;
            Vector3 middleBottomRight = frontCoordinates.BottomRight;
            Vector3 middleBottomLeft = frontCoordinates.BottomLeft;
            FaceCoordinates middleCoordinates = new FaceCoordinates(ref middleTopLeft, ref middleTopRight, ref middleBottomRight, ref middleBottomLeft);

            if (rotation > 0)
            {
                RotateSlope(ref frontCoordinates, rotation);
                RotateSlope(ref backCoordinates, rotation);
                RotateSlope(ref middleCoordinates, rotation);
            }

            middleCoordinates.TopLeft.Z -= 0.5f * MainGame.GlobalScalar.Z;
            middleCoordinates.TopRight.Z -= 0.5f * MainGame.GlobalScalar.Z;
            middleCoordinates.BottomRight.Z -= 0.5f * MainGame.GlobalScalar.Z;
            middleCoordinates.BottomLeft.Z -= 0.5f * MainGame.GlobalScalar.Z;

            //Front face (diagonal)
            if (block.Lid.TileNumber > 0)
            {
                RotationType lidRotation = block.Lid.Rotation;
                RotateEnum(ref lidRotation, rotation);
                Vector2[] TexPos = GetTexturePositions(tileAtlas[block.Lid.TileNumber], lidRotation, block.Lid.Flip);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.TopRight, Vector3.Zero, TexPos[2]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.BottomRight, Vector3.Zero, TexPos[1]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.BottomLeft, Vector3.Zero, TexPos[0]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.TopLeft, Vector3.Zero, TexPos[3]));

                int startIndex = cityVerticesCollection.Count - 4;
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
            if (topFace.TileNumber > 0)
            {
                Vector2[] TexPos = GetTexturePositions(tileAtlas[topFace.TileNumber], topFace.Rotation, topFace.Flip);
                Vector2 center = GetCenterPosition(ref TexPos[3], ref TexPos[0]);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.TopRight, Vector3.Zero, center));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.TopRight, Vector3.Zero, TexPos[0]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.TopLeft, Vector3.Zero, TexPos[1]));
 
                int startIndex = cityVerticesCollection.Count - 3;
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex + 0);
            }
            //Bottom face
            if (bottomFace.TileNumber > 0)
            {
                Vector2[] TexPos = GetTexturePositions(tileAtlas[bottomFace.TileNumber], bottomFace.Rotation, bottomFace.Flip);
                Vector2 center = GetCenterPosition(ref TexPos[2], ref TexPos[1]);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.BottomRight, Vector3.Zero, center));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.BottomRight, Vector3.Zero, TexPos[1]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.BottomLeft, Vector3.Zero, TexPos[0]));

                int startIndex = cityVerticesCollection.Count - 3;
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex + 2);
            }

            //if (block.Position.X == 51 && block.Position.Y == 164)
            //    System.Diagnostics.Debug.WriteLine("OK");

            //Right face
            if (rightFace.TileNumber > 0) //this face is not supported by GTA2, the editor removes this face.
            {
                Vector2[] TexPos = GetTexturePositions(tileAtlas[rightFace.TileNumber], rightFace.Rotation, rightFace.Flip);
                Vector2 center = GetCenterPosition(ref TexPos[1], ref TexPos[2]);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.TopRight, Vector3.Zero, center));
                center = GetCenterPosition(ref TexPos[0], ref TexPos[3]);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.BottomRight, Vector3.Zero, center));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.BottomRight, Vector3.Zero, TexPos[3]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.TopRight, Vector3.Zero, TexPos[2]));

                int startIndex = cityVerticesCollection.Count - 4;
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex + 0);
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 3);
                indexBufferCollection.Add(startIndex + 2);
            }           
        }

        private void SetUpSlope_26High(BlockInfo block, byte rotation)
        {
            //Sample is a right slope, use it for orientation, it gets rotated to fit all other directions
            FaceCoordinates frontCoordinates;
            FaceCoordinates backCoordinates;
            PrepareCoordinates(block.Position, out frontCoordinates, out backCoordinates);
            Vector3 middleTopLeft = frontCoordinates.TopLeft;
            Vector3 middleTopRight = frontCoordinates.TopRight;
            Vector3 middleBottomRight = frontCoordinates.BottomRight;
            Vector3 middleBottomLeft = frontCoordinates.BottomLeft;
            FaceCoordinates middleCoordinates = new FaceCoordinates(ref middleTopLeft, ref middleTopRight, ref middleBottomRight, ref middleBottomLeft);

            if (rotation > 0)
            {
                RotateSlope(ref frontCoordinates, rotation);
                RotateSlope(ref backCoordinates, rotation);
                RotateSlope(ref middleCoordinates, rotation);
            }

            middleCoordinates.TopLeft.Z -= 0.5f * MainGame.GlobalScalar.Z;
            middleCoordinates.TopRight.Z -= 0.5f * MainGame.GlobalScalar.Z;
            middleCoordinates.BottomRight.Z -= 0.5f * MainGame.GlobalScalar.Z;
            middleCoordinates.BottomLeft.Z -= 0.5f * MainGame.GlobalScalar.Z;

            //Front face (diagonal)
            if (block.Lid.TileNumber > 0)
            {
                RotationType lidRotation = block.Lid.Rotation;
                RotateEnum(ref lidRotation, rotation);
                Vector2[] TexPos = GetTexturePositions(tileAtlas[block.Lid.TileNumber], lidRotation, block.Lid.Flip);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.TopRight, Vector3.Zero, TexPos[2]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.BottomRight, Vector3.Zero, TexPos[1]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.BottomLeft, Vector3.Zero, TexPos[0]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.TopLeft, Vector3.Zero, TexPos[3]));

                int startIndex = cityVerticesCollection.Count - 4;
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
            if (topFace.TileNumber > 0)
            {
                Vector2[] TexPos = GetTexturePositions(tileAtlas[topFace.TileNumber], topFace.Rotation, topFace.Flip);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.TopRight, Vector3.Zero, TexPos[3]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.TopRight, Vector3.Zero, TexPos[0]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.TopLeft, Vector3.Zero, TexPos[1]));
                Vector2 center = GetCenterPosition(ref TexPos[1], ref TexPos[2]);
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
            if (bottomFace.TileNumber > 0)
            {
                Vector2[] TexPos = GetTexturePositions(tileAtlas[bottomFace.TileNumber], bottomFace.Rotation, bottomFace.Flip);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoordinates.BottomRight, Vector3.Zero, TexPos[2]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.BottomRight, Vector3.Zero, TexPos[1]));                
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoordinates.BottomLeft, Vector3.Zero, TexPos[0]));
                Vector2 center = GetCenterPosition(ref TexPos[3], ref TexPos[0]);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(middleCoordinates.BottomLeft, Vector3.Zero, center));

                int startIndex = cityVerticesCollection.Count - 4;
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
            Vector3 topLeftFront = (new Vector3(0.0f, 0.0f, UnitSize) + position) * scalar;
            Vector3 topRightFront = (new Vector3(UnitSize, 0.0f, UnitSize) + position) * scalar;
            Vector3 bottomLeftFront = (new Vector3(0.0f, -UnitSize, UnitSize) + position) * scalar;
            Vector3 bottomRightFront = (new Vector3(UnitSize, -UnitSize, UnitSize) + position) * scalar;
            frontCoords = new FaceCoordinates(ref topLeftFront, ref topRightFront, ref bottomRightFront, ref bottomLeftFront);

            Vector3 topLeftBack = (new Vector3(0.0f, 0.0f, 0.0f) + position) * scalar;
            Vector3 topRightBack = (new Vector3(UnitSize, 0.0f, 0.0f) + position) * scalar;
            Vector3 bottomLeftBack = (new Vector3(0.0f, -UnitSize, 0.0f) + position) * scalar;
            Vector3 bottomRightBack = (new Vector3(UnitSize, -UnitSize, 0.0f) + position) * scalar;
            backCoords = new FaceCoordinates(ref topLeftBack, ref topRightBack, ref bottomRightBack, ref bottomLeftBack);
        }


        private void CreateFrontVertices(ref FaceCoordinates frontCoords, ref FaceCoordinates backCoords, ref BlockInfo block)
        {
            if (block.Lid.TileNumber > 0)
            {
                Vector2[] TexPos = GetTexturePositions(tileAtlas[block.Lid.TileNumber], block.Lid.Rotation, block.Lid.Flip);
                //Vector2[] TexPos = GetTexturePositions(tileAtlas[991], block.Lid.Rotation, block.Lid.Flip);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoords.TopRight, Vector3.Zero, TexPos[2]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoords.BottomRight, Vector3.Zero, TexPos[1]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoords.TopLeft, Vector3.Zero, TexPos[3]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoords.BottomLeft, Vector3.Zero, TexPos[0]));

                int startIndex = cityVerticesCollection.Count - 4;
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex + 3);
                indexBufferCollection.Add(startIndex + 2);
            }
        }

        private void CreateTopVertices(ref FaceCoordinates frontCoords, ref FaceCoordinates backCoords, ref BlockInfo block)
        {
            if (block.Top.TileNumber > 0 && !SkipFace(block, 0))
            {
                Vector2[] TexPos = GetTexturePositions(tileAtlas[block.Top.TileNumber], block.Top.Rotation, block.Top.Flip);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoords.TopRight, Vector3.Zero, TexPos[0]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoords.TopLeft, Vector3.Zero, TexPos[2]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoords.TopRight, Vector3.Zero, TexPos[3]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoords.TopLeft, Vector3.Zero, TexPos[1]));

                int startIndex = cityVerticesCollection.Count - 4;
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 3);
                indexBufferCollection.Add(startIndex + 1);
            }
        }

        private void CreateBottomVertices(ref FaceCoordinates frontCoords, ref FaceCoordinates backCoords, ref BlockInfo block)
        {
            if (block.Bottom.TileNumber > 0 && !SkipFace(block, 1))
            {
                Vector2[] TexPos = GetTexturePositions(tileAtlas[block.Bottom.TileNumber], block.Bottom.Rotation, block.Bottom.Flip);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoords.BottomRight, Vector3.Zero, TexPos[2]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoords.BottomRight, Vector3.Zero, TexPos[1]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(backCoords.BottomLeft, Vector3.Zero, TexPos[0]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(frontCoords.BottomLeft, Vector3.Zero, TexPos[3]));

                int startIndex = cityVerticesCollection.Count - 4;
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex + 3);
            }
        }

        private void CreateLeftVertices(ref FaceCoordinates frontCoords, ref FaceCoordinates backCoords, ref BlockInfo block)
        {
            CreateLeftVertices(ref frontCoords, ref backCoords, ref block, block.Left, 0);
        }

        private void CreateLeftVertices(ref FaceCoordinates frontCoords, ref FaceCoordinates backCoords, ref BlockInfo block, BlockFace leftFace, byte rotation)
        {
            if (leftFace.TileNumber > 0 && !SkipFace(block, 2))
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
                Vector2[] TexPos = GetTexturePositions(tileAtlas[leftFace.TileNumber], leftFace.Rotation, leftFace.Flip);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(newFront.TopRight, Vector3.Zero, TexPos[3]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(newBack.BottomRight, Vector3.Zero, TexPos[1]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(newFront.BottomRight, Vector3.Zero, TexPos[2]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(newBack.TopRight, Vector3.Zero, TexPos[0]));

                //Left also has a strange index buffer order...
                int startIndex = cityVerticesCollection.Count - 4;
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex + 3);
                indexBufferCollection.Add(startIndex);
            }
        }

        private void CreateRightVertices(ref FaceCoordinates frontCoords, ref FaceCoordinates backCoords, ref BlockInfo block)
        {
            CreateRightVertices(ref frontCoords, ref backCoords, ref block, block.Right, 0);
        }

        private void CreateRightVertices(ref FaceCoordinates frontCoords, ref FaceCoordinates backCoords, ref BlockInfo block, BlockFace rightFace, byte rotation)
        {
            if (rightFace.TileNumber > 0 && !SkipFace(block, 3))
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
                Vector2[] TexPos = GetTexturePositions(tileAtlas[rightFace.TileNumber], rightFace.Rotation, rightFace.Flip);
                cityVerticesCollection.Add(new VertexPositionNormalTexture(newFront.TopLeft, Vector3.Zero, TexPos[2]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(newFront.BottomLeft, Vector3.Zero, TexPos[3]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(newBack.BottomLeft, Vector3.Zero, TexPos[0]));
                cityVerticesCollection.Add(new VertexPositionNormalTexture(newBack.TopLeft, Vector3.Zero, TexPos[1]));

                //...
                int startIndex = cityVerticesCollection.Count - 4;
                indexBufferCollection.Add(startIndex + 2);
                indexBufferCollection.Add(startIndex + 1);
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex);
                indexBufferCollection.Add(startIndex + 3);
                indexBufferCollection.Add(startIndex + 2);
            }
        }

        private static FaceCoordinates CorrectLeftRightVertices(FaceCoordinates coordinates, bool left)
        {
            FaceCoordinates newCoords = new FaceCoordinates();
            

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
            float tileSizeX = 1.0f / 32;
            float tileSizeY = 1.0f / 31;
            
            int tileColumn = sourceRectangle.X / 64;
            int tileRow = sourceRectangle.Y / 64;

            //Then you can determine your UV's for each corner based on which row and column the image you want is located in.  So say you wanted the image that's in the second row from the top, and third column from the left:

            //int tileRow = 1;  // First row is 0
            //int tileColumn = 2;

            Vector2 texTopLeft = new Vector2(tileColumn * tileSizeX, tileRow * tileSizeY);
            Vector2 texTopRight = new Vector2((tileColumn + 1) * tileSizeX, tileRow * tileSizeY);
            Vector2 texBottomLeft = new Vector2(tileColumn * tileSizeX, (tileRow + 1) * tileSizeY);
            Vector2 texBottomRight = new Vector2((tileColumn + 1) * tileSizeX, (tileRow + 1) * tileSizeY);


            double pixelPerWidth = 1/tiles.X;
            double pixelPerHeight = 1/tiles.Y;

            //if (sourceRectangle.X == 1984)
            //    System.Diagnostics.Debug.WriteLine("OK");

            //Vector2 texTopLeft = new Vector2((float)(sourceRectangle.X * pixelPerWidth), (float)(sourceRectangle.Y * pixelPerHeight));
            //Vector2 texTopRight = new Vector2((float)((sourceRectangle.X + sourceRectangle.Width - 1) * pixelPerWidth), (float)(sourceRectangle.Y * pixelPerHeight));
            //Vector2 texBottomRight = new Vector2((float)((sourceRectangle.X + sourceRectangle.Width - 1) * pixelPerWidth), (float)((sourceRectangle.Y + sourceRectangle.Height - 1) * pixelPerHeight));
            //Vector2 texBottomLeft = new Vector2((float)(sourceRectangle.X * pixelPerWidth), (float)((sourceRectangle.Y + sourceRectangle.Height - 1) * pixelPerHeight));
            //Vector2 tempVecor = new Vector2(sourceRectangle.X, sourceRectangle.Y);

            //Vector2 texTopLeft = tempVecor / tiles;
            //Vector2 texTopRight = (tempVecor + new Vector2(64 - 1, 0)) / tiles; //Vectors here could be static, but it only works with 64px tiles here... maybe dynamical tiles will appear somewhen...
            //Vector2 texBottomRight = (tempVecor + new Vector2(64 - 1, 64 - 1)) / tiles;
            //Vector2 texBottomLeft = (tempVecor + new Vector2(0, 64 - 1)) / tiles;
            
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

        private static Vector2 GetCenterPosition(ref Vector2 lowerEnd, ref Vector2 higherEnd)
        {
            Vector2 center;
            center.X = MathHelper.Lerp(lowerEnd.X, higherEnd.X, 0.5f);
            center.Y = MathHelper.Lerp(lowerEnd.Y, higherEnd.Y, 0.5f);
            return center;
        }

        private bool SkipFace(BlockInfo block, byte faceID) //faceID: 0=Top, 1=Bottom, 2=Left, 3=Right
        {
            if (!EnableSkipFace)
                return false;
            int x, y, z;
            x = (int)block.Position.X;
            y = (int)block.Position.Y;
            z = (int)block.Position.Z;
            //if (x == 71 && y == 168)
            //    System.Diagnostics.Debug.WriteLine("OK");
            if (faceID == 0 || faceID == 1)
            {
                if (block.Position.Y >= 1 && block.Position.Y < MainGame.Map.CityBlocks.GetLength(1) - 2)
                {
                    if (faceID == 0 && !MainGame.Map.CityBlocks[x, y - 1, z].IsEmpty)
                        return true;
                    if (faceID == 1 && !MainGame.Map.CityBlocks[x, y + 1, z].IsEmpty)
                        return true;
                }
            }
            if (faceID == 2 || faceID == 3)
            {
                if (block.Position.X >= 1 && block.Position.X < MainGame.Map.CityBlocks.GetLength(0) - 2)
                {
                    if (faceID == 2 && !MainGame.Map.CityBlocks[x - 1, y, z].IsEmpty)
                        return true;
                    if (faceID == 3 && !MainGame.Map.CityBlocks[x + 1, y, z].IsEmpty)
                        return true;
                }
            }
            return false;
        }

        private void SetUpEffect()
        {
            effect.Texture = cityTexture;
            effect.TextureEnabled = true;
            effect.LightingEnabled = false;
        }

        private void CopyToGraphicsDevice()
        {
            VertexPositionNormalTexture[] cubeVertices = cityVerticesCollection.ToArray();
            vertexBuffer = new VertexBuffer(BaseGame.Device, cubeVertices.Length * VertexPositionNormalTexture.SizeInBytes, BufferUsage.None);
            vertexBuffer.SetData<VertexPositionNormalTexture>(cubeVertices);

            vertexDeclaration = new VertexDeclaration(BaseGame.Device, VertexPositionNormalTexture.VertexElements);

            int[] indexBufferData = indexBufferCollection.ToArray();
            indexBuffer = new IndexBuffer(BaseGame.Device, typeof(int), indexBufferData.Length, BufferUsage.None);
            indexBuffer.SetData<int>(indexBufferData);
        }

        private void LoadTexture()
        {
            const string tilesDictPath = "textures\\tiles.xml";
            TextureAtlasTiles dict;
            if (!File.Exists(tilesDictPath))
            {
                //City-tiles: Maybe could be optimized :P
                string[] tileFiles = System.IO.Directory.GetFiles("textures\\tiles");
                dict = ImageHelper.CreateImageDictionary(tileFiles, 64, 64);
                dict.Serialize(tilesDictPath);
                //tileAtlas = dict.Dictionary;
                MemoryStream stream = new MemoryStream();
                dict.Image.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                stream.Position = 0;
                cityTexture = Texture2D.FromFile(BaseGame.Device, stream);
                stream.Close();
                dict.Dispose();                
            }
            else
            {
                dict = (TextureAtlasTiles)TextureAtlas.Deserialize(tilesDictPath, typeof(TextureAtlasTiles));
                tileAtlas = dict.Dictionary;
                cityTexture = Texture2D.FromFile(BaseGame.Device, dict.ImagePath);
            }
            tiles = new Vector2(cityTexture.Width, cityTexture.Height);

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

            //effect.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Point;
            //effect.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Point;
            //effect.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Point;

            effect.GraphicsDevice.RenderState.DepthBufferEnable = true; //SpriteBatch disables DepthBuffer automatically, we need to enable it again
            effect.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            effect.GraphicsDevice.VertexDeclaration = vertexDeclaration;
            effect.GraphicsDevice.Indices = indexBuffer;
            effect.GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                //Anisotropic
                //effect.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;
                //effect.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Linear;
                //effect.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
                //effect.GraphicsDevice.SamplerStates[0].MaxAnisotropy = 16;

                effect.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Point;
                effect.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Point;
                effect.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Point;

                effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, cityVerticesCollection.Count, 0, indexBufferCollection.Count / 3);
                pass.End();
            }
            effect.End();
        }      
    }
}
