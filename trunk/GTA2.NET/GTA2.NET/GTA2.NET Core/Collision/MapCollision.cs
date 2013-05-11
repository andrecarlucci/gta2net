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
        private struct LineSegment
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

            var nodes = new Dictionary<Vector2, List<LineObstacle>>();
            foreach (var obstacle in obstacles)
            {
                if (obstacle.Z == 2)
                {
                    if (obstacle is LineObstacle)
                    {
                        var lineObstacle = (LineObstacle) obstacle;
                        List<LineObstacle> vectorList;
                        if (nodes.TryGetValue(lineObstacle.Start, out vectorList))
                        {
                            vectorList.Add(lineObstacle);
                        }
                        else
                        {
                            vectorList = new List<LineObstacle> {lineObstacle};
                            nodes.Add(lineObstacle.Start, vectorList);
                        }

                        if (nodes.TryGetValue(lineObstacle.End, out vectorList))
                        {
                            vectorList.Add(lineObstacle);
                        }
                        else
                        {
                            vectorList = new List<LineObstacle> {lineObstacle};
                            nodes.Add(lineObstacle.End, vectorList);
                        }
                    }
                }
            }

            Console.WriteLine();
            while (nodes.Count > 0)
            {
                var origin = nodes.Keys.First();
                var lineSegment = new LineSegment(origin, origin);
                var visitedItems = new List<Vector2>();
                var currentFigure = new List<LineSegment>();
                var forlornNodesStart = new List<Vector2>(); //sometimes several 'branches' sprout out of the obstacle, they can be removed if they are inside an obstacle figure.
                var switchPoints = new List<Vector2>();

                var nodesToVisit = new Stack<LineSegment>();
                nodesToVisit.Push(lineSegment);
                while (nodesToVisit.Count > 0)
                {
                    var currentItem = nodesToVisit.Pop();
                    var connectedNodes = GetConnectedNodes(currentItem.EndPoint, nodes);
                    connectedNodes.Remove(currentItem.StartPoint);
                    if (connectedNodes.Count == 0)
                        forlornNodesStart.Add(currentItem.EndPoint);
                    if (connectedNodes.Count > 1 && !switchPoints.Contains(currentItem.EndPoint))
                        switchPoints.Add(currentItem.EndPoint);
                    foreach (var connectedNode in connectedNodes)
                    {
                        //if (connectedNode == currentItem.StartPoint)
                        //    continue;
                        if (visitedItems.Contains(connectedNode))
                            continue;
                        lineSegment = new LineSegment(currentItem.EndPoint, connectedNode);
                        nodesToVisit.Push(lineSegment);
                        currentFigure.Add(lineSegment);
                    }
                    visitedItems.Add(currentItem.EndPoint);
                }

                if (GetConnectedNodes(origin, nodes).Count <= 2)
                    switchPoints.RemoveAt(0); //remove start point, it's not really a switch point

                var forlornNodes = new List<Vector2>();
                foreach (var forlornNodeStart in forlornNodesStart)
                {
                    var forlormRoot = GetForlormRoot(forlornNodeStart, nodes);
                }


                foreach (var segment in currentFigure)
                {
                    nodes.Remove(segment.EndPoint);
                }

                using (Bitmap bmp = new Bitmap(2560, 2560))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
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

        private Vector2 GetForlormRoot(Vector2 forlormStart, IDictionary<Vector2, List<LineObstacle>> nodes)
        {
            var currentItem = forlormStart;
            var previousItem = currentItem;
            List<Vector2> connectedNodes;
            do
            {
                //go through the nodes until a node with more than 2 connections are found
                connectedNodes = GetConnectedNodes(currentItem, nodes);
                if (connectedNodes.Count == 2)
                {
                    connectedNodes.Remove(previousItem);
                    previousItem = currentItem;
                    currentItem = connectedNodes[0];
                }
                else if (connectedNodes.Count == 1)
                {
                    previousItem = currentItem;
                    currentItem = connectedNodes[0];
                }
            } while (connectedNodes.Count < 3);
            return currentItem;
        }

        private void OptimizeFigure(List<LineSegment> segments)
        {
            
        }


        private List<Vector2> GetConnectedNodes(Vector2 origin, IDictionary<Vector2, List<LineObstacle>> nodes)
        {
            List<LineObstacle> lineObstacles;
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
