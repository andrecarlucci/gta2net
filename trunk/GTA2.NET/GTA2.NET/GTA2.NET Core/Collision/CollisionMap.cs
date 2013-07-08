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
    public class CollisionMap
    {
        private readonly Map.Map _map;

        public CollisionMap(Map.Map map)
        {
            _map = map;
        }

        public ObstacleCollection GetObstacles()
        {
            var obstacles = new ObstacleCollection();
            for (var i = 7; i >= 0; i--)
                GetObstaclesPerLayer(i, obstacles);
            obstacles.RemoveUnnecessary();
            return obstacles;
        }

        private void GetObstaclesPerLayer(int currentLayer, ObstacleCollection obstacles)
        {
            var rawObstacles = GetBlockObstacles(currentLayer);
            var nodes = new LineNodeDictionary();
            nodes.CreateLineNodes(rawObstacles);

            while (nodes.Count > 0)
            {
                var currentFigure = new RawFigure(_map, currentLayer, nodes);
                nodes.Purge(currentFigure.Lines);
                currentFigure.Optimize();
                currentFigure.Tokenize(obstacles);
            }

            nodes.Clear();
            nodes.CreateLineNodes(rawObstacles, true);

            while (nodes.Count > 0)
            {
                var currentFigure = new SlopeFigure(_map, currentLayer, nodes);
                nodes.Purge(currentFigure.Lines);
                currentFigure.Optimize();
                currentFigure.Tokenize(obstacles);
            }
        }

        private IList<ILineObstacle> GetBlockObstacles(int z)
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
    }
}
