// GTA2.NET
// 
// File: Figure.cs
// Created: 23.05.2013
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
    public class Figure
    {
        public List<LineSegment> Lines { get; private set; }

        private Dictionary<Vector2, List<LineSegment>> Nodes { get; set; }

        private Dictionary<Vector2, SwitchPoint> SwitchPoints { get; set; }

        private List<Vector2> ForlornStartNodes { get; set; }

        public Figure(Vector2 origin, Dictionary<Vector2, List<LineSegment>> nodes)
        {
            Nodes = new Dictionary<Vector2, List<LineSegment>>();
            ForlornStartNodes = new List<Vector2>();
            SwitchPoints = new Dictionary<Vector2, SwitchPoint>();
            Lines = WalkFigure(origin, nodes);
        }

        private List<LineSegment> WalkFigure(Vector2 origin, Dictionary<Vector2, List<LineSegment>> nodes)
        {
            var lines = new List<LineSegment>();
            var visitedItems = new List<Vector2>();
            var nodesToVisit = new Stack<LineSegment>();
            var lineSegment = new LineSegment(origin, origin);
            nodesToVisit.Push(lineSegment);
            while (nodesToVisit.Count > 0)
            {
                var currentItem = nodesToVisit.Pop();
                var connectedNodes = GetConnectedNodes(currentItem.End, nodes);
                connectedNodes.Remove(currentItem.Start);
                if (connectedNodes.Count == 0)
                    ForlornStartNodes.Add(currentItem.End);
                if (connectedNodes.Count > 1 && !SwitchPoints.ContainsKey(currentItem.End))
                    SwitchPoints.Add(currentItem.End, new SwitchPoint(new List<Vector2>(connectedNodes)));
                foreach (var connectedNode in connectedNodes)
                {
                    if (visitedItems.Contains(connectedNode))
                        continue;
                    lineSegment = AddLine(currentItem.End, connectedNode, lines);
                    nodesToVisit.Push(lineSegment);
                }
                visitedItems.Add(currentItem.End);
            }

            if (GetConnectedNodes(origin, nodes).Count <= 2)
                SwitchPoints.Remove(origin); //remove start point, it's not really a switch point

            return lines;
        }

        public List<LineSegment> Optimize()
        {
            var optimizedLines = new List<LineSegment>();
            var optimizedSwitchPoints = new Dictionary<Vector2, SwitchPoint>();
            Nodes.Clear();
            var currentDirection = Direction.None;
            var start = new Vector2();
            LineSegment previousItem = null;

            for (var i = 0; i < Lines.Count; i++)
            {
                var directedLine = GetLine(Lines[i].Start, Lines[i].End, true);
                if (previousItem != null && !HasConnection(previousItem.End, directedLine.Start))
                    currentDirection = Direction.None;

                var isLast = i == Lines.Count - 1;
                var isSwitchPoint = SwitchPoints.ContainsKey(start);

                if ((directedLine.Direction != currentDirection || isSwitchPoint) && !isLast)
                {
                    if (previousItem != null)
                    {
                        AddLine(start, previousItem.End, optimizedLines);
                        if (isSwitchPoint)
                            AddSwitchPoint(start, previousItem.End, optimizedSwitchPoints);
                    }
                    currentDirection = directedLine.Direction;
                    start = directedLine.Start;
                }

                if (isLast)
                {
                    AddLine(start, directedLine.End, optimizedLines);
                    if (isSwitchPoint)
                    {
                        AddSwitchPoint(start, previousItem.End, optimizedSwitchPoints);
                        AddSwitchPoint(start, directedLine.End, optimizedSwitchPoints);
                    }
                }
                previousItem = directedLine;
            }
            Lines = optimizedLines;
            SwitchPoints = optimizedSwitchPoints;
            return optimizedLines;
        }

        public List<IObstacle> Tokenize()
        {
            GetForlorn();
            return new List<IObstacle>();

            var combinationsCount = SwitchPoints.Sum(switchPoint => switchPoint.Value.EndPoints.Count);
            var combinations = new List<SwitchPointCombination>(combinationsCount);

            foreach (var switchPoint in SwitchPoints)
            {
                for (var i = 0; i < switchPoint.Value.EndPoints.Count; i++)
                    combinations.Add(new SwitchPointCombination(switchPoint.Key, switchPoint.Value.EndPoints[i]));
            }

            foreach (var switchPointCombination in combinations)
            {
                CheckCombination(switchPointCombination);
            }

            return new List<IObstacle>();
        }

        private List<LineObstacle> GetForlorn()
        {
            var lineObstacles = new List<LineObstacle>();
            var forlornNodes = new Queue<Vector2>();
            foreach (var forlornNodeStart in ForlornStartNodes)
                forlornNodes.Enqueue(forlornNodeStart);
            while (forlornNodes.Count > 0)
            {
                var currentItem = forlornNodes.Dequeue();
                List<LineSegment> forlornLines;
                var forlornRoot = GetforlornRoot(currentItem, out forlornLines);

                foreach (var line in forlornLines)
                    Lines.Remove(line);
                //ToDo: remove from Nodes

                if (SwitchPoints.Count == 0)
                    continue;
                SwitchPoint switchPoint;
                if (!SwitchPoints.TryGetValue(forlornRoot, out switchPoint))
                    continue;
                if (switchPoint.EndPoints.Count > 0)
                    switchPoint.EndPoints.Remove(forlornLines.Last().End); //ToDo Bug here: not always .End
                if (switchPoint.EndPoints.Count == 0)
                    forlornNodes.Enqueue(forlornRoot);
                if (switchPoint.EndPoints.Count == 1)
                    SwitchPoints.Remove(forlornRoot);
            }
            return lineObstacles;
        }

        private Vector2 GetforlornRoot(Vector2 forlornStart, out List<LineSegment> forlornLines)
        {
            forlornLines = new List<LineSegment>();
            var currentItem = forlornStart;
            var previousItem = currentItem;
            var switchMode = false;
            do
            {
                //go through the nodes until a node with more than 2 connections are found
                var connectedNodes = GetConnectedNodes(currentItem);
                //foreach (var lineSegment in Lines)
                //{
                //    if (lineSegment.Start == currentItem)
                //    {
                //        if (connectedNodesTemp.Contains(lineSegment.End) && !connectedNodes.Contains(lineSegment.End))
                //            connectedNodes.Add(lineSegment.End);
                //    }
                //    else if (lineSegment.End == currentItem)
                //    {
                //        if (connectedNodesTemp.Contains(lineSegment.Start) && !connectedNodes.Contains(lineSegment.Start))
                //            connectedNodes.Add(lineSegment.Start);
                //    }
                //}
                if (connectedNodes.Count >= 3 || forlornLines.Count == Lines.Count)
                    break;
                if (connectedNodes.Count == 2)
                    connectedNodes.Remove(previousItem);
                previousItem = currentItem;
                currentItem = connectedNodes[0];
                forlornLines.Add(GetLine(previousItem, currentItem, false));
                //foreach (var lineSegment in Lines)
                //{
                //    var pointA = switchMode ? lineSegment.End : lineSegment.Start;
                //    var pointB = switchMode ? lineSegment.Start : lineSegment.End;
                //    if (pointA != currentItem || pointB != previousItem)
                //        continue;
                //    forlornLines.Add(lineSegment);
                //    break;
                //}
            } while (true);
            return currentItem;
        }

        private void CheckCombination(SwitchPointCombination combination)
        {
            var lines = new List<LineSegment>();
            var visitedItems = new List<Vector2>();
            LineSegment lineSegment;
            Vector2 currentItem = Lines[0].Start;
            Vector2 previousItem = currentItem;
            var count = 0;
            while (true)
            {
                if (visitedItems.Contains(currentItem))
                {
                    break;
                }
                else
                {
                    visitedItems.Add(currentItem);
                }

                if (currentItem == combination.Origin)
                {
                    lineSegment = GetLine(currentItem, combination.Target, false);
                    count++;
                    currentItem = combination.Target;
                    continue;

                }
                var connectedNodes = GetConnectedNodes(currentItem);
                connectedNodes.Remove(previousItem);
                if (connectedNodes.Count == 0)
                    break;
                count++;
                currentItem = connectedNodes[0];
                previousItem = currentItem;
            }

        }

        private LineSegment AddLine(Vector2 start, Vector2 end, ICollection<LineSegment> list)
        {
            var line = new LineSegment(start, end);
            list.Add(line);
            AddToNodes(start, line);
            AddToNodes(end, line);
            return line;
        }

        private void AddToNodes(Vector2 key, LineSegment line)
        {
            List<LineSegment> segments;
            if (Nodes.TryGetValue(key, out segments))
            {
                segments.Add(line);
            }
            else
            {
                segments = new List<LineSegment> { line };
                Nodes.Add(key, segments);
            }
        }

        private static SwitchPoint AddSwitchPoint(Vector2 key, Vector2 target, IDictionary<Vector2, SwitchPoint> dictionary)
        {
            SwitchPoint switchPoint;
            if (dictionary.TryGetValue(key, out switchPoint))
            {
                switchPoint.EndPoints.Add(target);
            }
            else
            {
                switchPoint = new SwitchPoint(new List<Vector2>());
                switchPoint.EndPoints.Add(target);
                dictionary.Add(key, switchPoint);
            }
            return switchPoint;
        }

        private List<Vector2> GetConnectedNodes(Vector2 origin)
        {
            return GetConnectedNodes(origin, Nodes);
        }

        private static List<Vector2> GetConnectedNodes(Vector2 origin, IDictionary<Vector2, List<LineSegment>> nodes)
        {
            List<LineSegment> lineSegments;
            if (nodes.TryGetValue(origin, out lineSegments))
            {
                var vectorList = new List<Vector2>();
                foreach (var lineSegment in lineSegments)
                {
                    if (origin == lineSegment.Start && !vectorList.Contains(lineSegment.End))
                        vectorList.Add(lineSegment.End);
                    if (origin == lineSegment.End && !vectorList.Contains(lineSegment.Start))
                        vectorList.Add(lineSegment.Start);
                }
                return vectorList;
            }
            return new List<Vector2>();
        }

        private bool HasConnection(Vector2 start, Vector2 end)
        {
            if (start == end)
                return true;
            return GetLine(start, end, false) != null;
        }

        private LineSegment GetLine(Vector2 start, Vector2 end, bool directed)
        {
            foreach (var lineSegment in Lines)
            {
                if (lineSegment.Start == start && lineSegment.End == end)
                    return lineSegment;
                if (lineSegment.Start == end && lineSegment.End == start)
                    return directed ? new LineSegment(lineSegment.End, lineSegment.Start) : lineSegment;
            }
            return null;
        }
    }
}
