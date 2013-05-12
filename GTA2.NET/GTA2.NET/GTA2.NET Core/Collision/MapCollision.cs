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
            public LineDirection Direction;

            public LineSegment(Vector2 startPoint, Vector2 endPoint)
            {
                StartPoint = startPoint;
                EndPoint = endPoint;
                Direction = LineDirection.Other;
                Direction = CalculateDirection(startPoint, endPoint);
            }

            private static LineDirection CalculateDirection(Vector2 startPoint, Vector2 endPoint)
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (startPoint.X == endPoint.X)
                {
                    if (startPoint.Y == endPoint.Y)
                        return LineDirection.Other;
                    if (startPoint.Y < endPoint.Y)
                        return LineDirection.Down;
                    if (startPoint.Y > endPoint.Y)
                        return LineDirection.Up;
                }
                if (startPoint.X < endPoint.X)
                {
                    if (startPoint.Y == endPoint.Y)
                        return LineDirection.Right;
                    if (startPoint.Y < endPoint.Y)
                        return LineDirection.DownRight;
                    if (startPoint.Y >  endPoint.Y)
                        return LineDirection.UpRight;
                }
                if (startPoint.X > endPoint.X)
                {
                    if (startPoint.Y == endPoint.Y)
                        return LineDirection.Left;
                    if (startPoint.Y < endPoint.Y)
                        return LineDirection.DownLeft;
                    if (startPoint.Y > endPoint.Y)
                        return LineDirection.UpLeft;
                }
                return LineDirection.Other;
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }

            public override string ToString()
            {
                return StartPoint + " - " + EndPoint;
            }
        }

        private enum LineDirection
        {
            Other,
            Left,
            Up,
            Right,
            Down,
            UpLeft,
            UpRight,
            DownLeft,
            DownRight,
        }

        private class SwitchPoint : LineSegment
        {
            public List<Vector2> EndPoints { get; set; }

            private new Vector2 EndPoint
            {
                get { return EndPoints[0]; }
                set
                {
                    if (EndPoints.Count > 0)
                        EndPoints[0] = value;
                    else
                        EndPoints.Add(value);
                }
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

            var nodes = GetAllObstacleNodes(obstacles, 2);

            Console.WriteLine();
            while (nodes.Count > 0)
            {
                var origin = nodes.Keys.First();
                var lineSegment = new LineSegment(origin, origin);
                var visitedItems = new List<Vector2>();
                var currentFigure = new List<LineSegment>();
                var forlornNodesStart = new List<Vector2>(); //sometimes several 'branches' sprout out of the obstacle, they can be removed if they are inside an obstacle figure.
                var switchPoints = new Dictionary<Vector2, SwitchPoint>();

                var nodesToVisit = new Stack<LineSegment>();
                nodesToVisit.Push(lineSegment);
                while (nodesToVisit.Count > 0)
                {
                    var currentItem = nodesToVisit.Pop();
                    var connectedNodes = GetConnectedNodes(currentItem.EndPoint, nodes);
                    connectedNodes.Remove(currentItem.StartPoint);
                    if (connectedNodes.Count == 0)
                        forlornNodesStart.Add(currentItem.EndPoint);
                    if (connectedNodes.Count > 1 && !switchPoints.ContainsKey(currentItem.EndPoint))
                    {
                        switchPoints.Add(currentItem.EndPoint, new SwitchPoint(currentItem.EndPoint, new List<Vector2>(connectedNodes)));
                    }
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

                if (GetConnectedNodes(origin, nodes).Count <= 2)
                    switchPoints.Remove(origin); //remove start point, it's not really a switch point

                var forlornNodes = new Queue<Vector2>();
                foreach (var forlornNodeStart in forlornNodesStart)
                    forlornNodes.Enqueue(forlornNodeStart);
                while (forlornNodes.Count > 0)
                {
                    var currentItem = forlornNodes.Dequeue();
                    List<LineSegment> forlormLines;
                    var forlormRoot = GetForlormRoot(currentItem, nodes, currentFigure, out forlormLines);
                    
                    foreach (var forlormLine in forlormLines)
                        currentFigure.Remove(forlormLine);

                    SwitchPoint switchPoint;
                    if (switchPoints.TryGetValue(forlormRoot, out switchPoint))
                    {
                        if (switchPoint.EndPoints.Count > 0)
                            //switchPoint.EndPoints.RemoveAt(0); //ToDo: Actually, I should remove the proper item, not "just the first one"
                            switchPoint.EndPoints.Remove(forlormLines.Last().EndPoint);
                        if (switchPoint.EndPoints.Count == 0)
                            forlornNodes.Enqueue(forlormRoot);
                    }
                }

                foreach (var segment in currentFigure)
                {
                    nodes.Remove(segment.EndPoint);
                }

                using (var bmp = new Bitmap(2560, 2560))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        foreach (var segment in currentFigure)
                        {
                            g.DrawLine(new Pen(new SolidBrush(System.Drawing.Color.Red), 1), segment.StartPoint.X * 10, segment.StartPoint.Y * 10, segment.EndPoint.X * 10, segment.EndPoint.Y * 10);
                        }
                    }
                    bmp.Save("Test.png", ImageFormat.Png);
                }
            }

            //using (Bitmap bmp = new Bitmap(2560, 2560))
            //{
            //    using (Graphics g = Graphics.FromImage(bmp))
            //    {
            //        foreach (var obstacle in obstacleList)
            //        {
            //            var points = new PointF[obstacle.Count];
            //            for (var i = 0; i < obstacle.Count; i++)
            //                points[i] = new PointF(obstacle[i].X*10, obstacle[i].Y*10);
            //            //g.DrawLines(new Pen(System.Drawing.Color.Red), points);
            //            foreach (var pointF in points)
            //            {
            //                g.DrawRectangle(new Pen(System.Drawing.Color.Red), pointF.X, pointF.Y, 1, 1);
            //            }
                        
            //        }
            //    }
            //    bmp.Save("Test.png", ImageFormat.Png);
            //}


            return obstacles;
        }

        private static Dictionary<Vector2, List<ILineObstacle>> GetAllObstacleNodes(List<IObstacle> obstacles, int layer)
        {
            var nodes = new Dictionary<Vector2, List<ILineObstacle>>();
            foreach (var obstacle in obstacles)
            {
                if (obstacle.Z == layer)
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
            }
            return nodes;
        }

        private Vector2 GetForlormRoot(Vector2 forlormStart, IDictionary<Vector2, List<ILineObstacle>> nodes, List<LineSegment> lineSegments, out List<LineSegment> forlormLines)
        {
            forlormLines = new List<LineSegment>();
            var currentItem = forlormStart;
            var previousItem = currentItem;
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
                    if (lineSegment.StartPoint != currentItem)
                        continue;
                    forlormLines.Add(lineSegment);
                    break;
                }
            } while (true);
            return currentItem;
        }

        //private LineSegment GetLineSegment(Vector2 endPoint, List<LineSegment> lineSegments)
        //{
        //    return lineSegments.FirstOrDefault(lineSegment => lineSegment.EndPoint == endPoint);
        //}

        private void OptimizeFigure(List<LineSegment> segments)
        {
            
        }


        private List<Vector2> GetConnectedNodes(Vector2 origin, IDictionary<Vector2, List<ILineObstacle>> nodes)
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

        //private List<Vector2> GetConnectedNodes(Vector2 origin, IDictionary<Vector2, List<LineObstacle>> coordDict, List<Vector2> visitedItems)
        //{
        //    List<LineObstacle> lineObstacles;
        //    if (coordDict.TryGetValue(origin, out lineObstacles))
        //    {
        //        var vectorList = new List<Vector2>();
        //        foreach (var lineObstacle in lineObstacles)
        //        {
        //            Vector2? currentItem = null;
        //            if (lineObstacle.Start == origin)
        //                currentItem = lineObstacle.End;
        //            if (lineObstacle.End == origin)
        //                currentItem = lineObstacle.Start;
        //            if (currentItem == null || visitedItems.Contains(currentItem.Value))
        //                continue;
        //            vectorList.Add(currentItem.Value);
        //            visitedItems.Add(currentItem.Value);
        //        }
        //        return vectorList;
        //    }
        //    return new List<Vector2>();
        //}

    }
}
