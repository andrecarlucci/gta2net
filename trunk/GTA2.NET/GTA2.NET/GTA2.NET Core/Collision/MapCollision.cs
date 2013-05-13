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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public class MapCollision
    {
        private class LineSegment
        {
            public Vector2 StartPoint;
            public Vector2 EndPoint;
            public Direction Direction;

            public LineSegment(Vector2 startPoint, Vector2 endPoint)
            {
                StartPoint = startPoint;
                EndPoint = endPoint;
                Direction = Direction.None;
                Direction = CalculateDirection(startPoint, endPoint);
            }

            private static Direction CalculateDirection(Vector2 startPoint, Vector2 endPoint)
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (startPoint.X == endPoint.X)
                {
                    if (startPoint.Y == endPoint.Y)
                        return Direction.None;
                    if (startPoint.Y < endPoint.Y)
                        return Direction.Down;
                    if (startPoint.Y > endPoint.Y)
                        return Direction.Up;
                }
                if (startPoint.X < endPoint.X)
                {
                    if (startPoint.Y == endPoint.Y)
                        return Direction.Right;
                    if (startPoint.Y < endPoint.Y)
                        return Direction.DownRight;
                    if (startPoint.Y >  endPoint.Y)
                        return Direction.UpRight;
                }
                if (startPoint.X > endPoint.X)
                {
                    if (startPoint.Y == endPoint.Y)
                        return Direction.Left;
                    if (startPoint.Y < endPoint.Y)
                        return Direction.DownLeft;
                    if (startPoint.Y > endPoint.Y)
                        return Direction.UpLeft;
                }
                return Direction.None;
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }

            public LineSegment SwapPoints()
            {
                return new LineSegment(EndPoint, StartPoint);
            }

            public override string ToString()
            {
                return StartPoint + " - " + EndPoint;
            }
        }

        private class SwitchPoint : LineSegment
        {
            public List<Vector2> EndPoints { get; private set; }

            private new Vector2 EndPoint
            {
                get { return EndPoints[0]; }
            }

            public SwitchPoint(Vector2 startPoint, List<Vector2> endPoints) : base(startPoint, endPoints.Count > 0 ? endPoints[0] : Vector2.Zero)
            {
                EndPoints = endPoints;
            }

            public override string ToString()
            {
                return EndPoint + " - " + EndPoints.Count + " Endpoints";
            }

        }

        private static Dictionary<Direction, int> _directionPriority;

        private readonly Map.Map _map;

        public MapCollision(Map.Map map)
        {
            _map = map;
            PreparePriorityTable();
        }

        private static void PreparePriorityTable()
        {
            if (_directionPriority != null)
                return;
            _directionPriority = new Dictionary<Direction, int> {{Direction.UpLeft, 1}, {Direction.Left, 2}, {Direction.DownLeft, 3}, {Direction.Down, 4}, {Direction.DownRight, 5}, {Direction.Right, 6}, {Direction.UpRight, 7}, {Direction.Up, 8}};
        }

        public List<IObstacle> GetObstacles()
        {
            var obstacles = GetBlockObstacles(2);

            var nodes = GetAllObstacleNodes(obstacles);

            Console.WriteLine();
            while (nodes.Count > 0)
            {
                var origin = nodes.Keys.First();
                var currentFigureForlorn = new List<LineSegment>();
                var forlornNodesStart = new List<Vector2>(); //sometimes several 'branches' sprout out of the obstacle, they can be removed if they are inside an obstacle figure.
                var switchPoints = new Dictionary<Vector2, SwitchPoint>();

                var currentFigure = VisitFigure(origin, nodes, forlornNodesStart, switchPoints);

                if (GetConnectedNodes(origin, nodes).Count <= 2)
                    switchPoints.Remove(origin); //remove start point, it's not really a switch point

                RemoveForlorm(forlornNodesStart, origin, nodes, currentFigure, currentFigureForlorn, switchPoints);
                

                foreach (var segment in currentFigure)
                {
                    nodes.Remove(segment.StartPoint);
                    nodes.Remove(segment.EndPoint);
                }
                foreach (var segment in currentFigureForlorn)
                {
                    nodes.Remove(segment.StartPoint);
                    nodes.Remove(segment.EndPoint);
                }

                var polygon = CreatePolygon(currentFigure);

                SaveSegmentsPicture(currentFigure);
                SavePolygonPicture(polygon);
            }
            return obstacles;
        }

        private List<IObstacle> GetBlockObstacles(int z)
        {
            var obstacles = new List<IObstacle>();
            for (var x = 0; x < _map.Width; x++)
            {
                for (var y = 0; y < _map.Length; y++)
                {
                    _map.CityBlocks[x, y, z].GetCollision(obstacles);
                }
            }
            return obstacles;
        }

        private static Dictionary<Vector2, List<ILineObstacle>> GetAllObstacleNodes(List<IObstacle> obstacles)
        {
            var nodes = new Dictionary<Vector2, List<ILineObstacle>>();
            foreach (var obstacle in obstacles)
            {
                if (obstacle is LineObstacle)
                {
                    ILineObstacle lineObstacle;
                    if (obstacle is SlopeLineObstacle)
                        continue;
                    //lineObstacle = (SlopeLineObstacle) obstacle;
                    else
                        lineObstacle = (LineObstacle)obstacle;
                    List<ILineObstacle> vectorList;
                    if (nodes.TryGetValue(lineObstacle.Start, out vectorList))
                    {
                        vectorList.Add(lineObstacle);
                    }
                    else
                    {
                        vectorList = new List<ILineObstacle> { lineObstacle };
                        nodes.Add(lineObstacle.Start, vectorList);
                    }

                    if (nodes.TryGetValue(lineObstacle.End, out vectorList))
                    {
                        vectorList.Add(lineObstacle);
                    }
                    else
                    {
                        vectorList = new List<ILineObstacle> { lineObstacle };
                        nodes.Add(lineObstacle.End, vectorList);
                    }
                }
            }
            return nodes;
        }

        private static List<LineSegment> VisitFigure(Vector2 origin, Dictionary<Vector2, List<ILineObstacle>> nodes, List<Vector2> forlornNodesStart, Dictionary<Vector2, SwitchPoint> switchPoints)
        {
            var currentFigure = new List<LineSegment>();
            var visitedItems = new List<Vector2>();
            var nodesToVisit = new Stack<LineSegment>();
            var lineSegment = new LineSegment(origin, origin);
            nodesToVisit.Push(lineSegment);
            while (nodesToVisit.Count > 0)
            {
                var currentItem = nodesToVisit.Pop();
                var connectedNodes = GetConnectedNodes(currentItem.EndPoint, nodes);
                connectedNodes.Remove(currentItem.StartPoint);
                if (connectedNodes.Count == 0)
                    forlornNodesStart.Add(currentItem.EndPoint);
                if (connectedNodes.Count > 1 && !switchPoints.ContainsKey(currentItem.EndPoint))
                    switchPoints.Add(currentItem.EndPoint, new SwitchPoint(currentItem.EndPoint, new List<Vector2>(connectedNodes)));
                foreach (var connectedNode in connectedNodes)
                {
                    if (visitedItems.Contains(connectedNode))
                        continue;
                    lineSegment = new LineSegment(currentItem.EndPoint, connectedNode);
                    nodesToVisit.Push(lineSegment);
                    currentFigure.Add(lineSegment);
                }
                visitedItems.Add(currentItem.EndPoint);
            }
            return currentFigure;
        }

        private static List<Vector2> GetConnectedNodes(Vector2 origin, IDictionary<Vector2, List<ILineObstacle>> nodes)
        {
            List<ILineObstacle> lineObstacles;
            if (nodes.TryGetValue(origin, out lineObstacles))
            {
                var vectorList = new List<Vector2>();
                foreach (var lineObstacle in lineObstacles)
                {
                    Vector2? currentItem = null;
                    if (lineObstacle.Start == origin)
                        currentItem = lineObstacle.End;
                    if (lineObstacle.End == origin)
                        currentItem = lineObstacle.Start;
                    if (currentItem == null)
                        continue;
                    if (currentItem == origin)
                        continue;
                    if (!vectorList.Contains(currentItem.Value))
                        vectorList.Add(currentItem.Value);
                }
                return vectorList;
            }
            return new List<Vector2>();
        }

        private static void RemoveForlorm(List<Vector2> forlornNodesStart, Vector2 origin, Dictionary<Vector2, List<ILineObstacle>> nodes, List<LineSegment> currentFigure, List<LineSegment> currentFigureForlorn, Dictionary<Vector2, SwitchPoint> switchPoints)
        {
            if (switchPoints.Count > 0)
            {
                var forlornNodes = new Queue<Vector2>();
                foreach (var forlornNodeStart in forlornNodesStart)
                    forlornNodes.Enqueue(forlornNodeStart);
                while (forlornNodes.Count > 0)
                {
                    var currentItem = forlornNodes.Dequeue();
                    List<LineSegment> forlormLines;
                    var forlormRoot = GetForlormRoot(currentItem, origin, nodes, currentFigure, out forlormLines);
                    foreach (var forlormLine in forlormLines)
                    {
                        currentFigure.Remove(forlormLine);
                        currentFigureForlorn.Add(forlormLine);
                    }

                    SwitchPoint switchPoint;
                    if (switchPoints.TryGetValue(forlormRoot, out switchPoint))
                    {
                        if (switchPoint.EndPoints.Count > 0)
                            switchPoint.EndPoints.Remove(forlormLines.Last().EndPoint);
                        if (switchPoint.EndPoints.Count == 0)
                            forlornNodes.Enqueue(forlormRoot);
                    }
                }
            }
        }

        private static Vector2 GetForlormRoot(Vector2 forlormStart, Vector2 origin, IDictionary<Vector2, List<ILineObstacle>> nodes, List<LineSegment> lineSegments, out List<LineSegment> forlormLines)
        {
            forlormLines = new List<LineSegment>();
            var currentItem = forlormStart;
            var previousItem = currentItem;
            var switchMode = false;
            do
            {
                //go through the nodes until a node with more than 2 connections are found
                var connectedNodes = GetConnectedNodes(currentItem, nodes);
                if (connectedNodes.Count >= 3)
                    break;
                if (connectedNodes.Count == 2)
                    connectedNodes.Remove(previousItem);
                previousItem = currentItem;
                currentItem = connectedNodes[0];
                foreach (var lineSegment in lineSegments)
                {
                    var linePoint = switchMode ? lineSegment.EndPoint : lineSegment.StartPoint;
                    if (linePoint != currentItem)
                        continue;
                    forlormLines.Add(lineSegment);
                    break;
                }
                if (currentItem == origin)
                    switchMode = true;
            } while (true);
            return currentItem;
        }

        private static List<Vector2> CreatePolygon(List<LineSegment> sourceSegments)
        {
            var polygon = new List<Vector2>();
            var lineSegments = new List<LineSegment>(sourceSegments);
            var currentItem = lineSegments.First().StartPoint;
            var currentDirection = Direction.None;
            while (lineSegments.Count > 0)
            {
                //polygon.Add(currentItem);
                var currentLines = lineSegments.Where(lineSegment => lineSegment.StartPoint == currentItem).ToList();
                currentLines.AddRange(lineSegments.Where(lineSegment => lineSegment.EndPoint == currentItem).ToList());
                var minPriority = int.MaxValue;
                LineSegment preferedLine = null;
                foreach (var currentLine in currentLines)
                {
                    var currentPriority = _directionPriority[currentLine.Direction];
                    if (currentPriority >= minPriority)
                        continue;
                    minPriority = currentPriority;
                    preferedLine = currentLine;
                }
                if (preferedLine == null)
                    return new List<Vector2>();
                lineSegments.Remove(preferedLine);
                var previousItem = currentItem;
                if (currentItem == preferedLine.EndPoint)
                {
                    currentItem = preferedLine.StartPoint;
                    preferedLine = preferedLine.SwapPoints();
                }
                else if (currentItem == preferedLine.StartPoint)
                    currentItem = preferedLine.EndPoint;
                if (preferedLine.Direction == currentDirection)
                    continue;
                polygon.Add(previousItem);
                currentDirection = preferedLine.Direction;
            }
            return polygon;
        }

        private static void SaveSegmentsPicture(List<LineSegment> segments)
        {
            using (var bmp = new Bitmap(2560, 2560))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    foreach (var segment in segments)
                    {
                        g.DrawLine(new Pen(new SolidBrush(System.Drawing.Color.Red), 1), segment.StartPoint.X * 10, segment.StartPoint.Y * 10, segment.EndPoint.X * 10, segment.EndPoint.Y * 10);
                    }
                }
                bmp.Save("Segments.png", ImageFormat.Png);
            }
        }

        private static void SavePolygonPicture(List<Vector2> polygon)
        {
            if (polygon.Count > 3)
            {
                using (var bmp = new Bitmap(2560, 2560))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        var points = new PointF[polygon.Count];
                        for (var i = 0; i < polygon.Count; i++)
                            points[i] = new PointF(polygon[i].X * 10, polygon[i].Y * 10);
                        g.DrawPolygon(new Pen(System.Drawing.Color.Red), points);
                    }
                    bmp.Save("Polygon.png", ImageFormat.Png);
                }
            }
        }

    }
}
