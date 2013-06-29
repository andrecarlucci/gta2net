// GTA2.NET
// 
// File: Polygon.cs
// Created: 22.06.2013
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
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Core.Map;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public class Polygon
    {
        public Map.Map Map { get; private set; }

        public int Layer { get; private set; }

        public List<IObstacle> Obstacles { get; set; }

        public Polygon(Map.Map map, int layer, List<IObstacle> obstacles)
        {
            Map = map;
            Layer = layer;
            Obstacles = obstacles;
        }

        public Polygon(Map.Map map, int layer) : this(map, layer, null)
        {
            //
        }

        ///// <summary>
        ///// Creates a polygon out of a List of Lines.
        ///// </summary>
        ///// <param name="sourceSegments"></param>
        ///// <returns></returns>
        //public List<Vector2> CreatePolygon(IEnumerable<LineSegment> sourceSegments)
        //{
        //    bool isRectangle;
        //    return CreatePolygon(sourceSegments, out isRectangle);
        //}

        ///// <summary>
        ///// Creates a polygon out of a List of Lines.
        ///// </summary>
        ///// <param name="sourceSegments"></param>
        ///// <param name="isRectangle">The returning polygon is actually a rectangle</param>
        ///// <returns></returns>
        //public List<Vector2> CreatePolygon(IEnumerable<LineSegment> sourceSegments, out bool isRectangle)
        //{
        //    var polygon = new List<Vector2>();
        //    var directions = new List<Direction>();
        //    var lineSegments = new List<LineSegment>(sourceSegments);

        //    var currentItem = lineSegments.First().Start;
        //    var startPoint = currentItem;
        //    var currentDirection = Direction.None;
        //    while (lineSegments.Count > 0)
        //    {
        //        if (polygon.Count > 0 && startPoint == currentItem)
        //            break;
        //        var preferedLine = Figure.ChooseNextLine(currentItem, lineSegments, currentDirection);
        //        if (preferedLine == null)
        //            break;
        //        lineSegments.Remove(preferedLine);
        //        var previousItem = currentItem;
        //        currentItem = preferedLine.End;
        //        if (preferedLine.Direction == currentDirection)
        //            continue;
        //        currentDirection = preferedLine.Direction;
        //        polygon.Add(previousItem);
        //        directions.Add(currentDirection);
        //    }
        //    FixPolygonStartPoint(polygon, directions);
        //    isRectangle = IsRectangleObstacle(polygon, directions);
        //    return polygon;
        //}

        public void CreateConvexPolygons(IEnumerable<LineSegment> sourceSegments, List<IObstacle> obstacles) //Note: this signature will change
        {
            //bool isRectangle;
            //var sourcePolygon = CreatePolygon(sourceSegments, out isRectangle);
            ////Debug.SavePolygonPicture(sourcePolygon);
            //var convexPolygons = CreateConvexPolygons(sourcePolygon);
            //var blocks = GetAssociatedBlocks(convexPolygons);
            //var fill = CheckLid(blocks);
            //if (fill)
            //{
            //    foreach (var convexPolygon in convexPolygons)
            //    {
            //        AddPolygonObstacle(convexPolygon, false, obstacles);
            //    }
            //}
            //else
            //{
            //    foreach (var convexPolygon in convexPolygons)
            //    {
            //        CreateLineSegments(convexPolygon, obstacles);
            //    }
                
            //}

            //return sourcePolygon;
        }

        private void CreateLineSegments(List<Vector2> polygonVertices, List<IObstacle> obstacles)
        {
            for (int i = 0, j = polygonVertices.Count - 1; i < polygonVertices.Count; j = i++)
                obstacles.Add(new LineObstacle(polygonVertices[i], polygonVertices[j], Layer));
        }

        public void AddPolygonObstacle(List<Vector2> polygonVertices, bool isRectangle, List<IObstacle> obstacles)
        {
            if (isRectangle)
            {
                var minX = float.MaxValue;
                var maxX = float.MinValue;
                var minY = float.MaxValue;
                var maxY = float.MinValue;
                foreach (var polygonVertex in polygonVertices)
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
                var width = maxX - minX;
                var height = maxY - minY;
                var rectangle = new RectangleObstacle(new Vector2(minX, minY), Layer, width, height);
                obstacles.Add(rectangle);
            }
            else
            {
                var polygonObstacle = new PolygonObstacle(Layer) { Vertices = polygonVertices };
                obstacles.Add(polygonObstacle);
            }
        }

        private static bool IsRectangleObstacle(ICollection<Vector2> polygon, ICollection<Direction> directions)
        {
            if (polygon.Count != 4 || directions.Count != 4)
                return false;
            return directions.Contains(Direction.Down) && directions.Contains(Direction.Right) && directions.Contains(Direction.Up) && directions.Contains(Direction.Left);
        }

        private static void FixPolygonStartPoint(IList<Vector2> polygon, IList<Direction> directions)
        {
            if (polygon.Count != directions.Count || polygon.Count < 3)
                return;
            if (directions.First() != directions.Last())
                return;
            polygon.RemoveAt(0);
            directions.RemoveAt(0);
        }

        private static List<Vertices> CreateConvexPolygons(IList<Vector2> polygonVertices)
        {
            return BayazitDecomposer.ConvexPartition(new Vertices(polygonVertices));
        }

        private List<Block> GetAssociatedBlocks(List<Vertices> vertices)
        {
            var blocks = new List<Block>();
            foreach (var convexPolygon in vertices)
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
                        var block = Map.CityBlocks[x, y, Layer];
                        var blockPoints = GetBlockPoints(block);

                        var addBlock = false;
                        foreach (var blockPoint in blockPoints)
                        {
                            bool isOnPolygon;
                            if (pointsCache.TryGetValue(blockPoint, out isOnPolygon))
                                continue;
                            isOnPolygon = VerticesEx.IsPointInPolygonOrEdge(convexPolygon, blockPoint);
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
            Debug.SavePolygonWithBlocksPicture(vertices, blocks);
            return blocks;
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

        private List<Vector2> GetBlockPoints(Block block)
        {
            var obstacles = new List<ILineObstacle>();
            block.GetCollision(obstacles, false);
            if (obstacles.Count == 0)
            {
                obstacles.Add(LineObstacle.DefaultLeft((int)block.Position.X, (int)block.Position.Y, Layer));
                obstacles.Add(LineObstacle.DefaultTop((int)block.Position.X, (int)block.Position.Y, Layer));
                obstacles.Add(LineObstacle.DefaultRight((int)block.Position.X, (int)block.Position.Y, Layer));
                obstacles.Add(LineObstacle.DefaultBottom((int)block.Position.X, (int)block.Position.Y, Layer));
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

        private bool CheckLid(List<Block> blocks)
        {
            //var pos = 0;
            //var neg = 0;
            var closedBlocks = new List<Block>();
            var openBlocks = new List<Block>();
            foreach (var block in blocks)
            {
                if (block.Lid)
                    closedBlocks.Add(block);
                //{
                //    pos++;
                //}
                else
                {
                    var blocksAbove = CheckBlocksAbove(block);
                    if (blocksAbove)
                        closedBlocks.Add(block);
                        //pos++;
                    else
                        openBlocks.Add(block);
                        //neg++;
                }
            }

            Console.WriteLine(closedBlocks.Count + ":" + openBlocks.Count);
            if (openBlocks.Count > 0)
                return false;
            return true;
        }

        private bool CheckBlocksAbove(Block block)
        {
            var blockPoints = GetBlockPoints(block); //ToDo: dictionary
            for (var z = (int)block.Position.Z + 1; z < 9; z++)
            {
                var blockFilled = false;

                //ToDo: optimize!
                var layerObstacles = Obstacles.Where(obstacle => obstacle.Z == z && (obstacle is PolygonObstacle || obstacle is RectangleObstacle)).ToList();

                foreach (var layerObstacle in layerObstacles)
                {
                    var containAll = true;
                    foreach (var blockPoint in blockPoints)
                    {
                        if (!layerObstacle.Bounds.Contains(blockPoint))
                        {
                            containAll = false;
                            break;
                        }
                        //
                    }
                    if (containAll)
                    {
                        if (layerObstacle is RectangleObstacle)
                        {
                            //bound check of rectangle obstacles is enough, block is ok
                            blockFilled = true;
                            break;
                        }

                        var polygonObstacle = layerObstacle as PolygonObstacle;
                        if (polygonObstacle == null)
                            break;
                        containAll = true;
                        foreach (var blockPoint in blockPoints)
                        {
                            if (!polygonObstacle.Contains(blockPoint))
                            {
                                containAll = false;
                                break;
                            }
                            //
                        }
                        if (containAll)
                        {
                            //all points are within the polygon, block is ok
                            blockFilled = true;
                            break;
                        }
                    }
                }
                Console.WriteLine(blockFilled);
                if (blockFilled)
                    return true;
            }
            return false;
        }

    }
}
