// GTA2.NET
// 
// File: RawFigure.cs
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
using Hiale.GTA2NET.Core.Helper;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public class RawFigure : Figure
    {
        private LineNodeDictionary Nodes { get; set; }

        private RawFigure() : base()
        {
            Nodes = new LineNodeDictionary();
        }

        public RawFigure(int layer, LineNodeDictionary nodes) : base(layer)
        {
            PrepareBasePriorityTable();
            Nodes = new LineNodeDictionary();
            Lines = WalkFigure(nodes);
        }

        private List<LineSegment> WalkFigure(LineNodeDictionary nodes)
        {
            var lines = new List<LineSegment>();
            var visitedItems = new List<Vector2>();
            var nodesToVisit = new Stack<LineSegment>();
            var origin = nodes.Keys.First();
            var lineSegment = new LineSegment(origin, origin);
            nodesToVisit.Push(lineSegment);
            while (nodesToVisit.Count > 0)
            {
                var currentItem = nodesToVisit.Pop();
                var connectedNodes = GetConnectedNodes(currentItem.End, nodes);
                if (connectedNodes.Count == 1) //only the item I come from
                    ForlornStartNodes.Add(currentItem.End);
                if (connectedNodes.Count > 2 && !SwitchPoints.ContainsKey(currentItem.End))
                    SwitchPoints.Add(currentItem.End, new SwitchPoint(new List<Vector2>(connectedNodes)));
                connectedNodes.Remove(currentItem.Start);
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

        /// <summary>
        /// Tries to to merge neighbor lines.
        /// </summary>
        public void Optimize()
        {
            var optimizedLines = new List<LineSegment>();
            var optimizedSwitchPoints = new SerializableDictionary<Vector2, SwitchPoint>();
            Nodes.Clear();
            var currentDirection = Direction.None;
            Vector2? start = null;
            Vector2? end = null;

            for (var i = 0; i < Lines.Count; i++)
            {
                var directedLine = GetLine(Lines[i].Start, Lines[i].End, true);
                if (end != null && !HasConnection(end.Value, directedLine.Start))
                    currentDirection = Direction.None;

                var isLast = i == Lines.Count - 1;
                var isSwitchPoint = start != null && SwitchPoints.ContainsKey(start.Value);
                var isSwitchPointInvented = end != null && SwitchPoints.ContainsKey(end.Value);

                if (directedLine.Direction != currentDirection || isSwitchPointInvented)
                {
                    if (start != null)
                    {
                        AddLine(start.Value, end.Value, optimizedLines);
                        if (isSwitchPoint)
                            AddSwitchPoint(start.Value, end.Value, optimizedSwitchPoints);
                        if (isSwitchPointInvented)
                            AddSwitchPoint(end.Value, start.Value, optimizedSwitchPoints);
                    }
                    currentDirection = directedLine.Direction;
                    start = directedLine.Start;
                }

                if (isLast && start != null)
                {
                    AddLine(start.Value, directedLine.End, optimizedLines);
                    if (isSwitchPoint)
                        AddSwitchPoint(start.Value, directedLine.End, optimizedSwitchPoints);
                    if (isSwitchPointInvented)
                        AddSwitchPoint(directedLine.End, start.Value, optimizedSwitchPoints);
                    if (SwitchPoints.ContainsKey(directedLine.Start))
                        AddSwitchPoint(directedLine.Start, directedLine.End, optimizedSwitchPoints);
                    if (SwitchPoints.ContainsKey(directedLine.End))
                        AddSwitchPoint(directedLine.End, directedLine.Start, optimizedSwitchPoints);
                }
                end = directedLine.End;
            }
            Lines = optimizedLines;
            SwitchPoints = optimizedSwitchPoints;
        }

        protected override LineSegment AddLine(Vector2 start, Vector2 end, ICollection<LineSegment> list)
        {
            var line = base.AddLine(start, end, list);
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

        private void RemoveNode(Vector2 key, LineSegment line)
        {
            List<LineSegment> segments;
            if (!Nodes.TryGetValue(key, out segments))
                return;
            segments.Remove(line);
            segments.Remove(line.Invert());
            if (segments.Count == 0)
                Nodes.Remove(key);
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
    }
}
