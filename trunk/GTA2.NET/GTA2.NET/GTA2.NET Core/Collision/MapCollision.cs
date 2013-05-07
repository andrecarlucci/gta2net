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
using System.Text;
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

        public List<IObstacle> GetObstacles()
        {
            var obstacles = new List<IObstacle>();

            for (var z = _map.Height - 1; z >= 0; z--)
            {
                for (var x = 0; x < _map.Width; x++)
                {
                    for (var y = 0; y < _map.Length; y++)
                    {
                        _map.CityBlocks[x, y, z].GetCollision(obstacles);
                    }
                }
            }

            var coordDict = new Dictionary<Vector2, List<LineObstacle>>();
            foreach (var obstacle in obstacles)
            {
                if (obstacle.Z == 2)
                {
                    if (obstacle is LineObstacle)
                    {
                        var lineObstacle = (LineObstacle) obstacle;
                        if (lineObstacle.Start.X == 58 && lineObstacle.Start.Y == 170 || lineObstacle.End.X == 58 && lineObstacle.End.Y == 170)
                            Console.WriteLine();
                        List<LineObstacle> vectorList;
                        if (coordDict.TryGetValue(lineObstacle.Start, out vectorList))
                        {
                            vectorList.Add(lineObstacle);
                        }
                        else
                        {
                            vectorList = new List<LineObstacle>();
                            vectorList.Add(lineObstacle);
                            coordDict.Add(lineObstacle.Start, vectorList);
                        }

                        if (coordDict.TryGetValue(lineObstacle.End, out vectorList))
                        {
                            vectorList.Add(lineObstacle);
                        }
                        else
                        {
                            vectorList = new List<LineObstacle>();
                            vectorList.Add(lineObstacle);
                            coordDict.Add(lineObstacle.End, vectorList);
                        }
                    }
                }
            }

            int zero = 0;
            int one = 0;
            int two = 0;
            int three = 0;
            int four = 0;
            int more = 0;
            foreach (var VARIABLE in coordDict)
            {
                if (VARIABLE.Value.Count == 1)
                    one++;
                if (VARIABLE.Value.Count == 2)
                    two++;
                if (VARIABLE.Value.Count == 3)
                    three++;
                if (VARIABLE.Value.Count == 4)
                    four++;
                if (VARIABLE.Value.Count > 4)
                    more++;
                if (VARIABLE.Value.Count == 0)
                    zero++;

            }

            return obstacles;
        }

    }
}
