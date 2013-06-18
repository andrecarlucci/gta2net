// GTA2.NET
// 
// File: FigureSplitter.cs
// Created: 10.06.2013
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
    /// <summary>
    /// A Splitter Figure is a Figure which was a part of a complex Figure. This kind of Figure exists only temporary.
    /// See example SplitterFigure.png: The whole figure includes three elements. Two squares and one line. The red squares are split and processed separately.
    /// </summary>
    public class SplitterFigure : Figure 
    {
        public List<LineSegment> RemainingLines { get; set; }

        public List<Vector2> SwitchPointKeys { get; set; }

        public List<Vector2> Polygon { get; set; }

        public bool IsRectangle { get; set; }

        public SplitterFigure(int layer) : base(layer)
        {
            RemainingLines = new List<LineSegment>();
            SwitchPointKeys = new List<Vector2>();
        }

        private void GenerateLocalSwitchPoints()
        {
            SwitchPoints.Clear();
            foreach (var switchPointKey in SwitchPointKeys)
            {
                var connectedNodes = GetConnectedNodes(switchPointKey);
                foreach (var connectedNode in connectedNodes)
                    AddSwitchPoint(switchPointKey, connectedNode);
            }
            UpdateSwitchPoints();
            SwitchPointKeys.Clear();
        }

        public void SeparateSwitchPoints(SerializableDictionary<Vector2, SwitchPoint> switchPoints)
        {
            GenerateLocalSwitchPoints();

            var switchPointsToRemove = new Dictionary<Vector2, SwitchPoint>();
            foreach (var switchPoint in switchPoints)
            {
                foreach (var endPoint in switchPoint.Value.EndPoints)
                {
                    if (Geometry.IsPointInPolygonOrEdge(Polygon, endPoint) && Geometry.IsPointInPolygonOrEdge(Polygon, switchPoint.Key))
                    {
                        var line = GetLine(switchPoint.Key, endPoint, false, RemainingLines);
                        if (line != null)
                            RemainingLines.Remove(line);
                        AddSwitchPoint(switchPoint.Key, endPoint, switchPointsToRemove);
                    }
                }
            }

            foreach (var switchPoint in switchPointsToRemove)
            {
                foreach (var endPoint in switchPoint.Value.EndPoints)
                    switchPoints[switchPoint.Key].EndPoints.Remove(endPoint);
                if (switchPoints[switchPoint.Key].EndPoints.Count == 0)
                    switchPoints.Remove(switchPoint.Key);
            }

            var linesToRemove = RemainingLines.Where(remainingLine => Geometry.IsPointInPolygonOrEdge(Polygon, remainingLine.Start) && Geometry.IsPointInPolygonOrEdge(Polygon, remainingLine.End)).ToList();
            foreach (var lineSegment in linesToRemove)
                RemainingLines.Remove(lineSegment);
        }

        public override bool Equals(object obj)
        {
            var item = obj as SplitterFigure;
            if (item == null)
                return false;
            return GetHashCode() == item.GetHashCode();
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyFieldInGetHashCode
            return Lines.Aggregate(0, (current, lineSegment) => (current*397) ^ lineSegment.Start.X.GetHashCode() ^ lineSegment.Start.Y.GetHashCode() ^ lineSegment.End.X.GetHashCode() ^ lineSegment.End.Y.GetHashCode());
            // ReSharper restore NonReadonlyFieldInGetHashCode
        }
    }
}
