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
                        if (block.Top && block.Left.Wall)
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
                        if (block.Right && block.Left.Wall)
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
                        if (block.Bottom && block.Left.Wall)
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
            //for (var z = 0; z < obstacles.Length; z++)
            //{
            //    using (var bmp = new Bitmap(2560, 2560))
            //    {
            //        using (var g = Graphics.FromImage(bmp))
            //        {
            //            g.Clear(Color.White);
            //            foreach (var obstacle in obstacles[z])
            //            {
            //                g.DrawLine(new Pen(Color.Red), new Point((int) obstacle.Start.X*10, (int) obstacle.Start.Y*10), new Point((int) obstacle.End.X*10, (int) obstacle.End.Y*10));
            //            }
            //            bmp.Save(z + ".png", ImageFormat.Png);
            //        }
            //    }
            //}
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
