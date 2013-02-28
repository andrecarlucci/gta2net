//Created 18.09.2010
//23.02.2013 - Old version was crap

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Xna.Framework;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

namespace Hiale.GTA2NET.Core.Map
{
    /// <summary>
    /// Represents unpassable space
    /// </summary>
    public class MapCollision
    {
        private readonly Map _map;

        public MapCollision(Map map)
        {
            _map = map;
        }

        public void FloodFill(Vector2 start)
        {
            //var blocks = new SlopeType[256,256];
            var blocks = new CollisionMapType[256, 256];

            int z = 2; //fix for now

            var stack = new Stack<Vector2>();
            stack.Push(start);
            do
            {
                var currentPos = stack.Pop();
                DebugThis((int)currentPos.X, (int)currentPos.Y);
                var currentBlock = _map.CityBlocks[(int)currentPos.X, (int)currentPos.Y, z];
                if (CheckBlockBounds(currentPos))
                {
                    switch (currentBlock.SlopeType)
                    {
                        case SlopeType.DiagonalFacingUpLeft:
                        case SlopeType.DiagonalFacingUpRight:
                        case SlopeType.DiagonalFacingDownLeft:
                        case SlopeType.DiagonalFacingDownRight:
                        case SlopeType.PartialCentreBlock:
                            blocks[(int) currentPos.X, (int) currentPos.Y] = CollisionMapType.Special;
                            continue;
                    }


                    //blocks[(int) currentPos.X, (int) currentPos.Y] = SlopeType.None; //SET
                    blocks[(int) currentPos.X, (int) currentPos.Y] = CollisionMapType.Free;
                }

                BlockInfo newBlock;
                var newPos = new Vector2(currentPos.X + 1, currentPos.Y); //right
                DebugThis((int)newPos.X, (int)newPos.Y);
                if (CheckBlockBounds(newPos) && blocks[(int)newPos.X, (int)newPos.Y] == CollisionMapType.None)
                {
                    newBlock = _map.CityBlocks[(int) newPos.X, (int) newPos.Y, z];
                    if (newBlock.SlopeType != SlopeType.None)
                    {
                        blocks[(int)newPos.X, (int)newPos.Y] = CollisionMapType.Special;
                    }
                    else if ((!currentBlock.Right.Wall && !newBlock.Left.Wall))
                        stack.Push(newPos);
                    else
                        blocks[(int)newPos.X, (int)newPos.Y] = CollisionMapType.Forbidden;
                }
                newPos = new Vector2(currentPos.X, currentPos.Y + 1); //bottom
                DebugThis((int)newPos.X, (int)newPos.Y);
                if (CheckBlockBounds(newPos) && blocks[(int) newPos.X, (int) newPos.Y] == CollisionMapType.None)
                {
                    newBlock = _map.CityBlocks[(int)newPos.X, (int)newPos.Y, z];
                    if (newBlock.SlopeType != SlopeType.None)
                    {
                        blocks[(int)newPos.X, (int)newPos.Y] = CollisionMapType.Special;
                    }
                    else if ((!currentBlock.Bottom.Wall && !newBlock.Top.Wall))
                        stack.Push(newPos);
                    else
                        blocks[(int)newPos.X, (int)newPos.Y] = CollisionMapType.Forbidden;
                }
                newPos = new Vector2(currentPos.X - 1, currentPos.Y); //left
                DebugThis((int)newPos.X, (int)newPos.Y);
                if (CheckBlockBounds(newPos) && blocks[(int)newPos.X, (int)newPos.Y] == CollisionMapType.None)
                {
                    newBlock = _map.CityBlocks[(int)newPos.X, (int)newPos.Y, z];
                    if (newBlock.SlopeType != SlopeType.None)
                    {
                        blocks[(int)newPos.X, (int)newPos.Y] = CollisionMapType.Special;
                    }
                    else if ((!currentBlock.Left.Wall && !newBlock.Right.Wall))
                        stack.Push(newPos);
                    else
                        blocks[(int)newPos.X, (int)newPos.Y] = CollisionMapType.Forbidden;
                }
                newPos = new Vector2(currentPos.X, currentPos.Y - 1); //top
                DebugThis((int) newPos.X, (int) newPos.Y);
                if (CheckBlockBounds(newPos) && blocks[(int)newPos.X, (int)newPos.Y] == CollisionMapType.None)
                {
                    newBlock = _map.CityBlocks[(int)newPos.X, (int)newPos.Y, z];
                    if (newBlock.SlopeType != SlopeType.None)
                    {
                        blocks[(int) newPos.X, (int) newPos.Y] = CollisionMapType.Special;
                    }
                    else if ((!currentBlock.Top.Wall && !newBlock.Bottom.Wall))
                        stack.Push(newPos);
                    else
                        blocks[(int)newPos.X, (int)newPos.Y] = CollisionMapType.Forbidden;
                }

            } while (stack.Count > 0);

            using (var bmp = new Bitmap(2560, 2560))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);
                    //foreach (var obstacle in obstacles[z])
                    //{
                    //    g.DrawLine(new Pen(Color.Red), new Point((int)obstacle.Start.X * 10, (int)obstacle.Start.Y * 10), new Point((int)obstacle.End.X * 10, (int)obstacle.End.Y * 10));
                    //}

                    for (var x = 0; x < blocks.GetLength(0); x++ )
                    {
                        for (var y = 0; y < blocks.GetLength(1); y++)
                        {
                            if (DebugThis(x,y))
                            {
                                g.FillRectangle(new SolidBrush(Color.Turquoise), x * 10, y * 10, 10, 10);
                                continue;
                            }
                            if (blocks[x, y] == CollisionMapType.Forbidden)
                                g.FillRectangle(new SolidBrush(Color.Red), x*10, y*10, 10, 10);
                            if (blocks[x, y] == CollisionMapType.Free)
                                g.FillRectangle(new SolidBrush(Color.Green), x * 10, y * 10, 10, 10);
                            if (blocks[x, y] == CollisionMapType.Special)
                                g.FillRectangle(new SolidBrush(Color.Blue), x * 10, y * 10, 10, 10);
                            if (blocks[x, y] == CollisionMapType.None)
                                g.FillRectangle(new SolidBrush(Color.Yellow), x * 10, y * 10, 10, 10);
                        }
                    }


                        bmp.Save(z + ".png", ImageFormat.Png);
                }
            }

            System.Diagnostics.Debug.WriteLine(blocks);

        }

        private bool DebugThis(int x, int y)
        {
            if (x == 115 && y == 193)
            {
                System.Diagnostics.Debug.WriteLine("OK");
                return true;
            }
            return false;
        }

        private bool CheckBlockBounds(Vector2 newPos)
        {
            return (newPos.X > -1) && (newPos.Y > -1) && (newPos.X < _map.Width) && (newPos.Y < _map.Length);
        }

        public List<Obstacle>[] CreateMapVertices()
        {
            //we create a list of unpassable obsticles of each layer (z coord)
            var obstacles = new List<Obstacle>[_map.CityBlocks.GetLength(2)];
            var straightObstacles = new List<Obstacle>[_map.CityBlocks.GetLength(2)];
            for (var i = 0; i < obstacles.Length; i++)
            {
                obstacles[i] = new List<Obstacle>();
                straightObstacles[i] = new List<Obstacle>();
            }

            for (var z = 0; z < _map.CityBlocks.GetLength(2); z++)
            {
                for (var x = 0; x < _map.CityBlocks.GetLength(0); x++)
                {
                    for (var y = 0; y < _map.CityBlocks.GetLength(1); y++)
                    {
                        var block = _map.CityBlocks[x, y, z];
                        if (block.IsEmpty)
                            continue;
                        if (block.Left && block.Left.Wall)
                        {
                            switch (block.SlopeType)
                            {
                                case SlopeType.DiagonalFacingUpLeft:
                                case SlopeType.DiagonalSlopeFacingUpLeft:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x, y + 1), new Vector2(x + 1, y), ObstacleType.Other));
                                    break;
                                case SlopeType.DiagonalFacingDownLeft:
                                case SlopeType.DiagonalSlopeFacingDownLeft:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x, y), new Vector2(x + 1, y + 1), ObstacleType.Other));
                                    break;
                                case SlopeType.PartialBlockRight:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y), new Vector2(x + 1, y), ObstacleType.Vertical));
                                    break;
                                case SlopeType.PartialBlockTop:
                                case SlopeType.PartialBlockTopLeft:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x, y), new Vector2(x, y + BlockInfo.PartialBlockScalar), ObstacleType.Vertical));
                                    break;
                                case SlopeType.PartialBlockBottom:
                                case SlopeType.PartialBlockBottomLeft:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x, y + 1 - BlockInfo.PartialBlockScalar), new Vector2(x, y + 1), ObstacleType.Vertical));
                                    break;
                                case SlopeType.PartialBlockTopRight:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y), new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y + BlockInfo.PartialBlockScalar), ObstacleType.Vertical));
                                    break;
                                case SlopeType.PartialBlockBottomRight:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y + 1 - BlockInfo.PartialBlockScalar), new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y + 1), ObstacleType.Vertical));
                                    break;
                                default:
                                    straightObstacles[z - 1].Add(new Obstacle(new Vector2(x, y), new Vector2(x, y + 1), ObstacleType.Vertical));
                                    break;
                            }
                        }
                        if (block.Top && block.Top.Wall)
                        {
                            switch (block.SlopeType)
                            {
                                case SlopeType.PartialBlockLeft:
                                case SlopeType.PartialBlockTopLeft:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x, y), new Vector2(x + BlockInfo.PartialBlockScalar, y), ObstacleType.Horizontal));
                                    break;
                                case SlopeType.PartialBlockRight:
                                case SlopeType.PartialBlockTopRight:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y), new Vector2(x + 1, y), ObstacleType.Horizontal));
                                    break;
                                case SlopeType.PartialBlockBottom:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x, y + 1 - BlockInfo.PartialBlockScalar), new Vector2(x + 1, y + 1 - BlockInfo.PartialBlockScalar), ObstacleType.Horizontal));
                                    break;
                                case SlopeType.PartialBlockBottomRight:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y + 1 - BlockInfo.PartialBlockScalar), new Vector2(x + 1, y + 1 - BlockInfo.PartialBlockScalar), ObstacleType.Horizontal));
                                    break;
                                case SlopeType.PartialBlockBottomLeft:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x, y + 1 - BlockInfo.PartialBlockScalar), new Vector2(x + BlockInfo.PartialBlockScalar, y + 1 - BlockInfo.PartialBlockScalar), ObstacleType.Horizontal));
                                    break;
                                default:
                                    straightObstacles[z - 1].Add(new Obstacle(new Vector2(x, y), new Vector2(x + 1, y), ObstacleType.Horizontal));
                                    break;
                            }
                        }
                        if (block.Right && block.Right.Wall)
                        {
                            switch (block.SlopeType)
                            {
                                case SlopeType.DiagonalFacingUpRight:
                                case SlopeType.DiagonalSlopeFacingUpRight:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x, y), new Vector2(x + 1, y + 1), ObstacleType.Other));
                                    break;
                                case SlopeType.DiagonalFacingDownRight:
                                case SlopeType.DiagonalSlopeFacingDownRight:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x, y + 1), new Vector2(x + 1, y), ObstacleType.Other));
                                    break;
                                case SlopeType.PartialBlockLeft:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x + BlockInfo.PartialBlockScalar, y), new Vector2(x + BlockInfo.PartialBlockScalar, y + 1), ObstacleType.Vertical));
                                    break;
                                case SlopeType.PartialBlockTop:
                                case SlopeType.PartialBlockTopRight:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1, y), new Vector2(x + 1, y + BlockInfo.PartialBlockScalar), ObstacleType.Vertical));
                                    break;
                                case SlopeType.PartialBlockBottom:
                                case SlopeType.PartialBlockBottomRight:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1, y + 1 - BlockInfo.PartialBlockScalar), new Vector2(x + 1, y + 1), ObstacleType.Vertical));
                                    break;
                                case SlopeType.PartialBlockTopLeft:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x + BlockInfo.PartialBlockScalar, y), new Vector2(x + BlockInfo.PartialBlockScalar, y + BlockInfo.PartialBlockScalar), ObstacleType.Vertical));
                                    break;
                                case SlopeType.PartialBlockBottomLeft:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x + BlockInfo.PartialBlockScalar, y + 1 - BlockInfo.PartialBlockScalar), new Vector2(x + BlockInfo.PartialBlockScalar, y + 1), ObstacleType.Vertical));
                                    break;
                                default:
                                    straightObstacles[z - 1].Add(new Obstacle(new Vector2(x + 1, y), new Vector2(x + 1, y + 1), ObstacleType.Vertical));
                                    break;
                            }
                        }
                        if (block.Bottom && block.Bottom.Wall)
                        {
                            switch (block.SlopeType)
                            {
                                case SlopeType.PartialBlockLeft:
                                case SlopeType.PartialBlockBottomLeft:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x, y + 1), new Vector2(x + BlockInfo.PartialBlockScalar, y + 1), ObstacleType.Horizontal));
                                    break;
                                case SlopeType.PartialBlockRight:
                                case SlopeType.PartialBlockBottomRight:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y + 1), new Vector2(x + 1, y + 1), ObstacleType.Horizontal));
                                    break;
                                case SlopeType.PartialBlockTop:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x, y + BlockInfo.PartialBlockScalar), new Vector2(x + 1, y + BlockInfo.PartialBlockScalar), ObstacleType.Horizontal));
                                    break;
                                case SlopeType.PartialBlockTopLeft:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x, y + BlockInfo.PartialBlockScalar), new Vector2(x + BlockInfo.PartialBlockScalar, y + BlockInfo.PartialBlockScalar), ObstacleType.Horizontal));
                                    break;
                                case SlopeType.PartialBlockTopRight:
                                    obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y + BlockInfo.PartialBlockScalar), new Vector2(x + 1, y + BlockInfo.PartialBlockScalar), ObstacleType.Horizontal));
                                    break;
                                default:
                                    straightObstacles[z - 1].Add(new Obstacle(new Vector2(x, y + 1), new Vector2(x + 1, y + 1), ObstacleType.Horizontal));
                                    break;
                            }
                        }
                    }

                }
            }
            OptimizeStraightVertices(straightObstacles, obstacles);

            //for Debug
            for (var z = 0; z < obstacles.Length; z++)
            {
                using (var bmp = new Bitmap(2560, 2560))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.White);
                        foreach (var obstacle in obstacles[z])
                        {
                            g.DrawLine(new Pen(Color.Red), new Point((int)obstacle.Start.X * 10, (int)obstacle.Start.Y * 10), new Point((int)obstacle.End.X * 10, (int)obstacle.End.Y * 10));
                        }
                        bmp.Save(z + ".png", ImageFormat.Png);
                    }
                }
            }
            return obstacles;
        }

        /// <summary> 
        /// Combines straight obstacles to optimize collision detection.
        /// </summary>
        /// <param name="straightObstacles"></param>
        /// <param name="obstacles"></param>
        private void OptimizeStraightVertices(List<Obstacle>[] straightObstacles, List<Obstacle>[] obstacles)
        {
            //var optimizedObstacles = new List<Obstacle>[straightObstacles.Length];
            for (var z = 0; z < straightObstacles.Length; z++)
            {
                var obstaclesHorizontal = new bool[256, 256 + 1];
                var obstaclesVertical = new bool[256 + 1, 256];
                foreach (var straightObstacle in straightObstacles[z])
                {
                    if (straightObstacle.Type == ObstacleType.Horizontal)
                        obstaclesHorizontal[(int) straightObstacle.Start.X, (int) straightObstacle.Start.Y] = true;
                    else if (straightObstacle.Type == ObstacleType.Vertical)
                        obstaclesVertical[(int)straightObstacle.Start.X, (int)straightObstacle.Start.Y] = true;
                }

                //Horizontal
                for (var y = 0; y < obstaclesHorizontal.GetLength(1); y++)
                {
                    var start = new Vector2();
                    var open = false;
                    for (var x = 0; x < obstaclesHorizontal.GetLength(0); x++)
                    {
                        if (!obstaclesHorizontal[x, y])
                        {
                            if (open)
                            {
                                var end = new Vector2(x, y);
                                obstacles[z] .Add(new Obstacle(start, end, ObstacleType.Horizontal));
                                open = false;
                            }
                            continue;
                        }
                        if (open)
                            continue;
                        open = true;
                        start = new Vector2(x, y);
                    }
                }

                //Vertical
                for (var x = 0; x < obstaclesVertical.GetLength(0); x++)
                {
                    var start = new Vector2();
                    var open = false;
                    for (var y = 0; y < obstaclesVertical.GetLength(1); y++)
                    {
                        if (!obstaclesVertical[x, y])
                        {
                            if (open)
                            {
                                var end = new Vector2(x, y);
                                obstacles[z].Add(new Obstacle(start, end, ObstacleType.Vertical));
                                open = false;
                            }
                            continue;
                        }
                        if (open)
                            continue;
                        open = true;
                        start = new Vector2(x, y);
                    }
                }

            }
        }


    }
}
