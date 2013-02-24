//Created 18.09.2010
//23.02.2013 - Old version was crap

using System.Collections.Generic;
using Microsoft.Xna.Framework;

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
            CreateMapCollision();
        }

        private void CreateMapCollision()
        {
            //we create a list of unpassable obsticles of each layer (z coord)
            var unpassableSpace = new List<Obstacle>[_map.CityBlocks.GetLength(2)];
            for (var i = 0; i < unpassableSpace.Length; i++)
                unpassableSpace[i] = new List<Obstacle>();

            for (var z = 0; z < _map.CityBlocks.GetLength(2); z++)
            {
                for (var x = 0; x < _map.CityBlocks.GetLength(0); x++)
                {
                    for (var y = 0; y < _map.CityBlocks.GetLength(1); y++)
                    {
                        var block = _map.CityBlocks[x, y, z];
                        if (block.IsEmpty)
                            continue;
                        if (block.SlopeType == SlopeType.None)
                        {
                            if (block.Left)
                                unpassableSpace[z - 1].Add(new Obstacle(new Vector2(x, y), new Vector2(x, y + 1), ObstacleType.Vertical));
                            if (block.Top)
                                unpassableSpace[z - 1].Add(new Obstacle(new Vector2(x, y), new Vector2(x + 1, y), ObstacleType.Horizontal));
                            if (block.Right)
                                unpassableSpace[z - 1].Add(new Obstacle(new Vector2(x + 1, y), new Vector2(x + 1, y + 1), ObstacleType.Vertical));
                            if (block.Bottom)
                                unpassableSpace[z - 1].Add(new Obstacle(new Vector2(x, y + 1), new Vector2(x + 1, y + 1), ObstacleType.Horizontal));
                        }
                        //ToDo: slope types...
                    }

                }
            }
            OptimizeStraightVertices(unpassableSpace);
        }

        private void OptimizeStraightVertices(List<Obstacle>[] obstacleArray)
        {
            var optimizedObstacles = new List<Obstacle>[obstacleArray.Length];
            for (var z = 0; z < obstacleArray.Length; z++)
            {
                var obstacleGridHorizontal = new bool[256, 256 + 1];
                var obstacleGridVertical = new bool[256 + 1, 256];
                foreach (var obstacle in obstacleArray[z])
                {
                    if (obstacle.Type == ObstacleType.Horizontal)
                        obstacleGridHorizontal[(int) obstacle.Start.X, (int) obstacle.Start.Y] = true;
                    else if (obstacle.Type == ObstacleType.Vertical)
                        obstacleGridVertical[(int)obstacle.Start.X, (int)obstacle.Start.Y] = true;
                }

                var obstacles = new List<Obstacle>();

                //Horizontal
                for (var y = 0; y < obstacleGridHorizontal.GetLength(1); y++)
                {
                    var start = new Vector2();
                    var open = false;
                    for (var x = 0; x < obstacleGridHorizontal.GetLength(0); x++)
                    {
                        if (!obstacleGridHorizontal[x, y])
                        {
                            if (open)
                            {
                                var end = new Vector2(x, y);
                                obstacles.Add(new Obstacle(start, end, ObstacleType.Horizontal));
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
                for (var x = 0; x < obstacleGridVertical.GetLength(0); x++)
                {
                    var start = new Vector2();
                    var open = false;
                    for (var y = 0; y < obstacleGridVertical.GetLength(1); y++)
                    {
                        if (!obstacleGridVertical[x, y])
                        {
                            if (open)
                            {
                                var end = new Vector2(x, y);
                                obstacles.Add(new Obstacle(start, end, ObstacleType.Vertical));
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
                optimizedObstacles[z] = obstacles;
            }
            System.Diagnostics.Debug.WriteLine("OK");
        }


    }
}
