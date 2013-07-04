// GTA2.NET
// 
// File: MapCollision.cs
// Created: 09.03.2013
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
using System.Linq;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public class MapCollision
    {
        private readonly Map.Map _map;

        public MapCollision(Map.Map map)
        {
            _map = map;
        }

        public ObstacleCollection GetObstacles()
        {
            var obstacles = new ObstacleCollection();
            for (var i = 7; i >= 0; i--)
                GetObstaclesPerLayer(i, obstacles);
            RemoveUnnecessaryObstacles(obstacles);
            return obstacles;
        }

        private void GetObstaclesPerLayer(int currentLayer, ObstacleCollection obstacles)
        {
            var rawObstacles = GetBlockObstacles(currentLayer);
            var nodes = new LineNodeDictionary(rawObstacles);

            while (nodes.Count > 0)
            {
                var currentFigure = new RawFigure(_map, currentLayer, nodes);
                nodes.Purge(currentFigure.Lines);
                currentFigure.Optimize();
                currentFigure.Tokenize(obstacles);
            }
        }

        private IEnumerable<ILineObstacle> GetBlockObstacles(int z)
        {
            var obstacles = new List<ILineObstacle>();
            for (var x = 0; x < _map.Width; x++)
            {
                for (var y = 0; y < _map.Length; y++)
                {
                    _map.CityBlocks[x, y, z].GetCollision(obstacles, false);
                }
            }
            return obstacles;
        }

        /// <summary>
        /// Removes obstacles which are within other obstacles
        /// </summary>
        /// <param name="obstacles"></param>
        private static void RemoveUnnecessaryObstacles(ICollection<IObstacle> obstacles)
        {
            var itemsToRemove = new List<IObstacle>();
            foreach (var target in obstacles)
            {
                foreach (var source in obstacles)
                {
                    if (source == target)
                        continue;

                    if (source.Z != target.Z)
                        continue;

                    var targetPolygon = target as PolygonObstacle;
                    var targetRectangle = target as RectangleObstacle;
                    if (targetPolygon == null && targetRectangle == null)
                        break;

                    var sourcePoints = new List<Vector2>();

                    var sourceLine = source as LineObstacle;
                    if (sourceLine != null)
                    {
                        sourcePoints.Add(sourceLine.Start);
                        sourcePoints.Add(sourceLine.End);
                    }
                    var sourceRectangle = source as RectangleObstacle;
                    if (sourceRectangle != null)
                    {
                        sourcePoints.Add(new Vector2(sourceRectangle.X, sourceRectangle.Y));
                        sourcePoints.Add(new Vector2(sourceRectangle.X + sourceRectangle.Width, sourceRectangle.Y));
                        sourcePoints.Add(new Vector2(sourceRectangle.X, sourceRectangle.Y + sourceRectangle.Length));
                        sourcePoints.Add(new Vector2(sourceRectangle.X + sourceRectangle.Width, sourceRectangle.Y + sourceRectangle.Length));
                    }
                    var sourcePolygon = source as PolygonObstacle;
                    if (sourcePolygon != null)
                    {
                        sourcePoints.AddRange(sourcePolygon.Vertices);
                    }

                    var containsAll = true;
                    if (targetPolygon != null)
                    {
                        if (sourcePoints.Any(sourcePoint => !targetPolygon.Contains(sourcePoint)))
                            containsAll = false;
                    }
                    if (targetRectangle != null)
                    {
                        if (sourcePoints.Any(sourcePoint => !targetRectangle.Contains(sourcePoint)))
                            containsAll = false;
                    }
                    if (containsAll && !itemsToRemove.Contains(source))
                        itemsToRemove.Add(source);
                }
            }
            foreach (var item in itemsToRemove)
                obstacles.Remove(item);
        }
    }
}
