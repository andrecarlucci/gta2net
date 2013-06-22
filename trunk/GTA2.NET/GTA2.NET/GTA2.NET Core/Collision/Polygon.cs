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

        public Polygon(Map.Map map, int layer)
        {
            Map = map;
            Layer = layer;

        }

        /// <summary>
        /// Creates a polygon out of a List of Lines.
        /// </summary>
        /// <param name="sourceSegments"></param>
        /// <returns></returns>
        public List<Vector2> CreatePolygon(IEnumerable<LineSegment> sourceSegments)
        {
            bool isRectangle;
            return CreatePolygon(sourceSegments, out isRectangle);
        }

        /// <summary>
        /// Creates a polygon out of a List of Lines.
        /// </summary>
        /// <param name="sourceSegments"></param>
        /// <param name="isRectangle">The returning polygon is actually a rectangle</param>
        /// <returns></returns>
        public List<Vector2> CreatePolygon(IEnumerable<LineSegment> sourceSegments, out bool isRectangle)
        {
            var polygon = new List<Vector2>();
            var directions = new List<Direction>();
            var lineSegments = new List<LineSegment>(sourceSegments);

            var currentItem = lineSegments.First().Start;
            var startPoint = currentItem;
            var currentDirection = Direction.None;
            while (lineSegments.Count > 0)
            {
                if (polygon.Count > 0 && startPoint == currentItem)
                    break;
                var preferedLine = Figure.ChooseNextLine(currentItem, lineSegments, currentDirection);
                if (preferedLine == null)
                    break;
                lineSegments.Remove(preferedLine);
                var previousItem = currentItem;
                currentItem = preferedLine.End;
                if (preferedLine.Direction == currentDirection)
                    continue;
                currentDirection = preferedLine.Direction;
                polygon.Add(previousItem);
                directions.Add(currentDirection);
            }
            FixPolygonStartPoint(polygon, directions);
            //GetAssociatedBlocks(polygon);
            isRectangle = IsRectangleObstacle(polygon, directions);
            return polygon;
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

        //Work-in-Progress method
        public void GetAssociatedBlocks(List<Vector2> polygonVertices)
        {
            var convexPolygons = BayazitDecomposer.ConvexPartition(new Vertices(polygonVertices));
            foreach (var convexPolygon in convexPolygons)
            {
                Debug.SavePolygonPicture(convexPolygon);
                var minX = float.MaxValue;
                var maxX = float.MinValue;
                var minY = float.MaxValue;
                var maxY = float.MinValue;
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

                var blocks = new List<Block>();
                var pointsCache = new Dictionary<Vector2, bool>();
                for (var y = (int)minY; y < maxY; y++)
                {
                    for (var x = (int)minX; x < maxX; x++)
                    {
                        var obstacles = new List<ILineObstacle>();
                        var block = Map.CityBlocks[x, y, Layer];
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

                        var addBlock = false;
                        foreach (var blockPoint in blockPoints)
                        {
                            bool isOnPolygon;
                            if (pointsCache.TryGetValue(blockPoint, out isOnPolygon))
                                continue;
                            isOnPolygon = Geometry.IsPointInPolygonOrEdge(convexPolygon, blockPoint);
                            pointsCache.Add(blockPoint, isOnPolygon);
                            if (!isOnPolygon)
                                continue;
                            addBlock = true;
                            break;
                        }
                        if (addBlock)
                            blocks.Add(block);
                    }
                }
                Debug.SavePolygonWithBlocksPicture(convexPolygon, blocks);
            }
        }

    }
}
