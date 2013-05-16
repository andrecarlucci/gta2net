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

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public class MapCollision
    {
        private static Dictionary<Direction, int> _baseDirectionPriority;

        private readonly Map.Map _map;

        public MapCollision(Map.Map map)
        {
            _map = map;
            PrepareBasePriorityTable();
        }

        private static void PrepareBasePriorityTable()
        {
            if (_baseDirectionPriority != null)
                return;
            _baseDirectionPriority = new Dictionary<Direction, int> {{Direction.UpLeft, 1}, {Direction.Left, 2}, {Direction.DownLeft, 3}, {Direction.Down, 4}, {Direction.DownRight, 5}, {Direction.Right, 6}, {Direction.UpRight, 7}, {Direction.Up, 8}};
        }

        public List<IObstacle> GetObstacles()
        {
            int currentLayer = 2;
            var obstacles = new List<IObstacle>();
            var rawObstacles = GetBlockObstacles(currentLayer);
            var nodes = GetAllObstacleNodes(rawObstacles);

            while (nodes.Count > 0)
            {
                var origin = nodes.Keys.First();
                var currentFigureForlorn = new List<LineSegment>();
                var forlornNodesStart = new List<Vector2>(); //sometimes several 'branches' sprout out of the obstacle, they can be removed if they are inside an obstacle figure.
                var switchPoints = new Dictionary<Vector2, SwitchPoint>();

                var currentFigure = WalkFigure(origin, nodes, forlornNodesStart, switchPoints);

                if (GetConnectedNodes(origin, nodes).Count <= 2)
                    switchPoints.Remove(origin); //remove start point, it's not really a switch point

                var lines = CreateLines(forlornNodesStart, origin, nodes, currentFigure, switchPoints, currentFigureForlorn);
                foreach (var lineObstacle in lines)
                {
                    lineObstacle.Z = currentLayer;
                    obstacles.Add(lineObstacle);
                }

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

                var polygonVertices = CreatePolygon(currentFigure, switchPoints);
                if (polygonVertices.Count > 3)
                {
                    var polygon = new PolygonObstacle(2) {Vertices = polygonVertices};
                    obstacles.Add(polygon);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("DEBUG");
                }
            }
            return obstacles;
        }

        private List<ILineObstacle> GetBlockObstacles(int z)
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

        private static Dictionary<Vector2, List<ILineObstacle>> GetAllObstacleNodes(List<ILineObstacle> obstacles)
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

        private static List<LineSegment> WalkFigure(Vector2 origin, Dictionary<Vector2, List<ILineObstacle>> nodes, List<Vector2> forlornNodesStart, Dictionary<Vector2, SwitchPoint> switchPoints)
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
                    switchPoints.Add(currentItem.EndPoint, new SwitchPoint(new List<Vector2>(connectedNodes)));
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

        private static List<LineObstacle> CreateLines(List<Vector2> forlornNodesStart, Vector2 origin, Dictionary<Vector2, List<ILineObstacle>> nodes, List<LineSegment> currentFigure, Dictionary<Vector2, SwitchPoint> switchPoints, List<LineSegment> currentFigureForlorn)
        {
            var lines = new List<LineObstacle>();
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
                    
                    var currentLineStart = Vector2.Zero;
                    var currentPosition = currentItem;
                    var currentDirection = Direction.None;
                    for (var i = 0; i < forlormLines.Count; i++)
                    {
                        currentFigure.Remove(forlormLines[i]);
                        currentFigureForlorn.Add(forlormLines[i]);

                        var directedLine = forlormLines[i]; //we need to create a new LineSegment object because we need it ordered
                        if (directedLine.EndPoint == currentPosition)
                            directedLine = directedLine.SwapPoints();
                        currentPosition = directedLine.EndPoint;
                        if (directedLine.Direction != currentDirection)
                        {
                            if (currentDirection != Direction.None)
                                lines.Add(new LineObstacle(currentLineStart, directedLine.StartPoint));
                            currentLineStart = directedLine.StartPoint;
                            currentDirection = directedLine.Direction;
                        }
                        if (i == forlormLines.Count - 1) //last item
                            lines.Add(new LineObstacle(currentLineStart, directedLine.EndPoint));
                    }
                    SwitchPoint switchPoint;
                    if (!switchPoints.TryGetValue(forlormRoot, out switchPoint))
                        continue;
                    if (switchPoint.EndPoints.Count > 0)
                        switchPoint.EndPoints.Remove(forlormLines.Last().EndPoint);
                    if (switchPoint.EndPoints.Count == 0)
                        forlornNodes.Enqueue(forlormRoot);
                    if (switchPoint.EndPoints.Count == 1)
                        switchPoints.Remove(forlormRoot);
                }
            }
            return lines;
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

        private static List<Vector2> CreatePolygon(List<LineSegment> sourceSegments, Dictionary<Vector2, SwitchPoint> switchPoints)
        {
            var polygon = new List<Vector2>();
            var lineSegments = new List<LineSegment>(sourceSegments);
            var currentItem = lineSegments.First().StartPoint;
            var origin = currentItem;
            var currentDirection = Direction.None;
            while (lineSegments.Count > 0)
            {
                if (switchPoints.ContainsKey(currentItem))
                {
                    System.Diagnostics.Debug.WriteLine(currentItem);
                }
                var currentLines = lineSegments.Where(lineSegment => lineSegment.StartPoint == currentItem).ToList();
                currentLines.AddRange(lineSegments.Where(lineSegment => lineSegment.EndPoint == currentItem).ToList());
                var minPriority = int.MaxValue;
                LineSegment preferedLine = null;
                LineSegment directedLine = null;
                foreach (var currentLine in currentLines)
                {
                    if (currentItem == currentLine.EndPoint)
                        directedLine = currentLine.SwapPoints();
                    else if (currentItem == currentLine.StartPoint)
                        directedLine = currentLine;
                    var currentPriority = GetDirectionPriority(currentDirection, directedLine.Direction);
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
                if (currentItem == origin)
                    break;
                currentDirection = preferedLine.Direction;
            }
            return polygon;
        }

        public static int GetDirectionPriority(Direction baseDirection, Direction newDirection)
        {
            var priority = _baseDirectionPriority[newDirection];
            if (baseDirection == Direction.None)
                baseDirection = Direction.Down;
            priority += 4 - _baseDirectionPriority[baseDirection];
            if (priority < 0)
                priority = 8 + priority;
            if (priority > 8)
                priority = priority - 8;
            return priority;
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
