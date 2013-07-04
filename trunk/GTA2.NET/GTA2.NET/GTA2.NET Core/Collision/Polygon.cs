// GTA2.NET
// 
// File: Polygon.cs
// Created: 29.06.2013
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
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Core.Map;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public class Polygon : VerticesEx
    {
        public List<LineSegment> Lines { get; private set; }

        public Polygon()
        {
            Lines = new List<LineSegment>();
        }

        public bool CheckIfFilled(Map.Map map, int layer, ObstacleCollection obstacles)
        {
            Simplify();
            var convexPolygons = BayazitDecomposer.ConvexPartition(this);
            var blockPointsDictionary = new Dictionary<Block, List<Vector2>>();
            var blocks = GetAssociatedBlocks(convexPolygons, map, layer, blockPointsDictionary);
            var layerObstacles = CreateLayerObstacles(layer, obstacles); //ToDo: Don't call this method every time, the result does not change...
            return CheckLid(blocks, map, layer, layerObstacles, blockPointsDictionary);
        }

        private void Simplify()
        {
            var vertices = SimplifyTools.CollinearSimplify(this);
            Clear();
            AddRange(vertices);
        }

        private static Dictionary<int, List<IObstacle>> CreateLayerObstacles(int layer, ObstacleCollection obstacles)
        {
            var dict = new Dictionary<int, List<IObstacle>>();
            for (var z = layer + 1; z < 8; z++)
            {
                var layerObstacles = obstacles.Where(obstacle => obstacle.Z == z && (obstacle is PolygonObstacle || obstacle is RectangleObstacle)).ToList();
                dict.Add(z, layerObstacles);
            }
            return dict;
        }

        private static bool CheckLid(IEnumerable<Block> blocks, Map.Map map, int layer, Dictionary<int, List<IObstacle>> layerObstacles, Dictionary<Block, List<Vector2>> blockPointsDictionary)
        {
            var openBlocks = 0;
            foreach (var block in blocks)
            {
                if (block.Lid)
                    continue;
                var blocksAbove = CheckBlocksAbove(block, map, layer, layerObstacles, blockPointsDictionary);
                if (!blocksAbove)
                    openBlocks++;
            }
            return openBlocks == 0;
        }

        private static IEnumerable<Block> GetAssociatedBlocks(IEnumerable<List<Vector2>> convexPolygons, Map.Map map, int layer, IDictionary<Block, List<Vector2>> blockPointsDictionary)
        {
            var blocks = new List<Block>();
            foreach (var convexPolygon in convexPolygons)
            {
                float minX;
                float maxX;
                float minY;
                float maxY;
                CalculateBounds(convexPolygon, out minX, out maxX, out minY, out maxY);
                maxX = (float)Math.Ceiling(maxX);
                maxY = (float)Math.Ceiling(maxY);

                var pointsCache = new Dictionary<Vector2, bool>();
                for (var y = (int)minY; y < maxY; y++)
                {
                    for (var x = (int)minX; x < maxX; x++)
                    {
                        var block = map.CityBlocks[x, y, layer];
                        List<Vector2> blockPoints;
                        if (!blockPointsDictionary.TryGetValue(block, out blockPoints))
                        {
                            blockPoints = GetBlockPoints(block, layer);
                            blockPointsDictionary.Add(block, blockPoints);
                        }
                        var addBlock = false;
                        foreach (var blockPoint in blockPoints)
                        {
                            bool isOnPolygon;
                            if (pointsCache.TryGetValue(blockPoint, out isOnPolygon))
                                continue;
                            isOnPolygon = IsPointInPolygonOrEdge(convexPolygon, blockPoint);
                            pointsCache.Add(blockPoint, isOnPolygon);
                            if (!isOnPolygon)
                                continue;
                            addBlock = true;
                            break;
                        }
                        if (addBlock && !blocks.Contains(block))
                            blocks.Add(block);
                    }
                }
            }
            return blocks;
        }

        private static bool CheckBlocksAbove(Block block, Map.Map map, int layer, Dictionary<int, List<IObstacle>> layerObstacles, Dictionary<Block, List<Vector2>> blockPointsDictionary)
        {
            List<Vector2> blockPoints;
            if (!blockPointsDictionary.TryGetValue(block, out blockPoints))
            {
                blockPoints = GetBlockPoints(block, layer);
                blockPointsDictionary.Add(block, blockPoints);
            }
            for (var z = (int)block.Position.Z + 1; z < 8; z++)
            {
                if (map.CityBlocks[(int)block.Position.X, (int)block.Position.Y, z].Lid)
                    return true;

                var blockFilled = false;
                foreach (var layerObstacle in layerObstacles[z])
                {
                    var containAll = false;

                    var polygonObstacle = layerObstacle as PolygonObstacle;
                    if (polygonObstacle != null)
                        containAll = blockPoints.All(polygonObstacle.Contains);

                    var rectangleObstacle = layerObstacle as RectangleObstacle;
                    if (rectangleObstacle != null)
                        containAll = blockPoints.All(rectangleObstacle.Contains);

                    if (!containAll)
                        continue;
                    //all points are within the polygon, block is ok
                    blockFilled = true;
                    break;
                }
                if (blockFilled)
                    return true;
            }
            return false;
        }

        public static void CalculateBounds(IList<Vector2> convexPolygon, out float minX, out float maxX, out float minY, out float maxY)
        {
            if (convexPolygon.Count == 0)
            {
                minX = 0;
                maxX = 0;
                minY = 0;
                maxY = 0;
                return;
            }
            minX = float.MaxValue;
            maxX = float.MinValue;
            minY = float.MaxValue;
            maxY = float.MinValue;
            foreach (var polygonVertex in convexPolygon)
            {
                if (polygonVertex.X < minX)
                    minX = polygonVertex.X;
                if (polygonVertex.X > maxX)
                    maxX = polygonVertex.X;
                if (polygonVertex.Y < minY)
                    minY = polygonVertex.Y;
                if (polygonVertex.Y > maxY)
                    maxY = polygonVertex.Y;
            }
            maxX = (float)Math.Ceiling(maxX);
            maxY = (float)Math.Ceiling(maxY);
        }

        private static List<Vector2> GetBlockPoints(Block block, int layer)
        {
            var obstacles = new List<ILineObstacle>();
            block.GetCollision(obstacles, false);
            if (obstacles.Count == 0)
            {
                obstacles.Add(LineObstacle.DefaultLeft((int)block.Position.X, (int)block.Position.Y, layer));
                obstacles.Add(LineObstacle.DefaultTop((int)block.Position.X, (int)block.Position.Y, layer));
                obstacles.Add(LineObstacle.DefaultRight((int)block.Position.X, (int)block.Position.Y, layer));
                obstacles.Add(LineObstacle.DefaultBottom((int)block.Position.X, (int)block.Position.Y, layer));
            }
            var blockPoints = new List<Vector2>();
            foreach (var lineObstacle in obstacles)
            {
                if (!blockPoints.Contains(lineObstacle.Start))
                    blockPoints.Add(lineObstacle.Start);
                if (!blockPoints.Contains(lineObstacle.End))
                    blockPoints.Add(lineObstacle.End);
            }
            return blockPoints;
        }
    }
}
