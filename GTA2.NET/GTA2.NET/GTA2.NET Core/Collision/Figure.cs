// GTA2.NET
// 
// File: Figure.cs
// Created: 12.06.2013
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
    public class Figure
    {
        public Map.Map Map { get; protected set; }

        public int Layer { get; protected set; }

        public List<LineSegment> Lines { get; protected set; }

        /// <summary>
        /// A special point where several lines connect.
        /// See SwitchPoints.png: The blue circles show  Switch Points.
        /// </summary>
        protected internal SerializableDictionary<Vector2, SwitchPoint> SwitchPoints { get; set; }
        
        /// <summary>
        /// A List Forlorn (= a line which emerges out of a figure) start nodes.
        /// See Forlorn.png: The magenta lines show forlorn lines. They go to a dead end. The blue circles show these ForlornStartNodes (dead ends)
        /// </summary>
        protected internal List<Vector2> ForlornStartNodes { get; set; }

        private static Dictionary<Direction, int> _baseDirectionPriority;

        public Figure()
        {
            PrepareBasePriorityTable();
            Lines = new List<LineSegment>();
            ForlornStartNodes = new List<Vector2>();
            SwitchPoints = new SerializableDictionary<Vector2, SwitchPoint>();
        }

        public Figure(Map.Map map, int layer) : this()
        {
            Map = map;
            Layer = layer;
        }


        /// <summary>
        /// Creates IObstacle objects of this figure.
        /// </summary>
        /// <returns></returns>
        public virtual void Tokenize(List<IObstacle> obstacles)
        {
            if (Lines.Count == 0)
                return;
            GetForlorn(obstacles);
            if (Lines.Count == 0)
                return;

            List<VerticesEx> polygons;
            if (SwitchPoints.Count > 0)
                polygons = SplitPolygon(Lines, obstacles);
            else
                polygons = new List<VerticesEx> {CreatePolygon(Lines)};
            ProcessPolygons(polygons, obstacles);
        }

        protected static void PrepareBasePriorityTable()
        {
            if (_baseDirectionPriority != null)
                return;
            _baseDirectionPriority = new Dictionary<Direction, int> { { Direction.UpLeft, 1 }, { Direction.Left, 2 }, { Direction.DownLeft, 3 }, { Direction.Down, 4 }, { Direction.DownRight, 5 }, { Direction.Right, 6 }, { Direction.UpRight, 7 }, { Direction.Up, 8 } };
        }

        //Line stuff

        protected virtual LineSegment AddLine(Vector2 start, Vector2 end, ICollection<LineSegment> list)
        {
            var line = new LineSegment(start, end);
            list.Add(line);
            return line;
        }

        protected virtual LineSegment GetLine(Vector2 start, Vector2 end)
        {
            return GetLine(start, end, false);
        }

        protected virtual LineSegment GetLine(Vector2 start, Vector2 end, bool directed)
        {
            return GetLine(start, end, directed, Lines);
        }

        protected static LineSegment GetLine(Vector2 start, Vector2 end, bool directed, IEnumerable<LineSegment> sourceLines)
        {
            foreach (var lineSegment in sourceLines)
            {
                if (lineSegment.Start == start && lineSegment.End == end)
                    return lineSegment;
                if (lineSegment.Start == end && lineSegment.End == start)
                    return directed ? lineSegment.Invert() : lineSegment;
            }
            return null;
        }

        protected virtual bool HasConnection(Vector2 start, Vector2 end)
        {
            return HasConnection(start, end, Lines);
        }

        protected static bool HasConnection(Vector2 start, Vector2 end, IEnumerable<LineSegment> sourceLines)
        {
            if (start == end)
                return true;
            return GetLine(start, end, false, sourceLines) != null;
        }

        protected virtual List<LineSegment> GetLines(Vector2 origin)
        {
            return GetLines(origin, Lines);
        }

        protected static List<LineSegment> GetLines(Vector2 origin, IEnumerable<LineSegment> sourceLines)
        {
            var lines = new List<LineSegment>();
            foreach (var lineSegment in sourceLines)
            {
                if (lineSegment.Start == origin)
                    lines.Add(lineSegment);
                else if (lineSegment.End == origin)
                    lines.Add(lineSegment);
            }
            return lines;
        }

        protected virtual List<Vector2> GetConnectedNodes(Vector2 origin)
        {
            return GetConnectedNodes(origin, Lines);
        }

        protected static List<Vector2> GetConnectedNodes(Vector2 origin, IEnumerable<LineSegment> sourceSegments)
        {
            var vectorList = new List<Vector2>();
            foreach (var lineSegment in sourceSegments)
            {
                if (lineSegment.Start == origin)
                    vectorList.Add(lineSegment.End);
                else if (lineSegment.End == origin)
                    vectorList.Add(lineSegment.Start);
            }
            return vectorList;
        }

        //Forlorn stuff

        /// <summary>
        /// Removes Forlorn out of the figure and converts them to LineObstacles.
        /// </summary>
        /// <param name="obstacles"></param>
        protected virtual void GetForlorn(List<IObstacle> obstacles)
        {
            var forlornNodes = new Queue<Vector2>();
            foreach (var forlornNodeStart in ForlornStartNodes)
                forlornNodes.Enqueue(forlornNodeStart);
            while (forlornNodes.Count > 0)
            {
                var currentItem = forlornNodes.Dequeue();
                List<LineSegment> forlornLines;
                Vector2 lastItemEndPoint; //this is needed for Switch Points below
                var forlornRoot = GetforlornRoot(currentItem, out forlornLines, out lastItemEndPoint);

                foreach (var line in forlornLines)
                {
                    obstacles.Add(new LineObstacle(line.Start, line.End, Layer));
                    Lines.Remove(line);
                }

                if (SwitchPoints.Count == 0)
                    continue;
                SwitchPoint switchPoint;
                if (!SwitchPoints.TryGetValue(forlornRoot, out switchPoint))
                    continue;
                if (switchPoint.EndPoints.Count > 0)
                    switchPoint.EndPoints.Remove(lastItemEndPoint);
                if (switchPoint.EndPoints.Count == 1)
                {
                    forlornNodes.Enqueue(forlornRoot);
                    SwitchPoints.Remove(forlornRoot);
                }
            }
            ForlornStartNodes.Clear();
            UpdateSwitchPoints();
        }

        protected virtual Vector2 GetforlornRoot(Vector2 forlornStart, out List<LineSegment> forlornLines, out Vector2 previousItem)
        {
            forlornLines = new List<LineSegment>();
            var currentItem = forlornStart;
            previousItem = currentItem;
            do
            {
                //go through the nodes until a node with more than 2 connections are found
                var connectedNodes = GetConnectedNodes(currentItem);
                if (connectedNodes.Count >= 3 || forlornLines.Count == Lines.Count)
                    break;
                if (connectedNodes.Count == 2)
                    connectedNodes.Remove(previousItem);
                previousItem = currentItem;
                if (connectedNodes.Count == 0)
                    break;
                currentItem = connectedNodes[0];
                forlornLines.Add(GetLine(previousItem, currentItem, false));
            } while (true);
            return currentItem;
        }

        //Switch Point stuff

        protected virtual void AddSwitchPoint(Vector2 key, Vector2 target)
        {
            AddSwitchPoint(key, target, SwitchPoints);
        }

        protected static void AddSwitchPoint(Vector2 key, Vector2 target, IDictionary<Vector2, SwitchPoint> dictionary)
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
        }

        /// <summary>
        /// Finds removed switchpoints which now describe forlorn ends. Add these to ForlornStartNodes. Afterwards all unneeded SwitchPoints are removed.
        /// </summary>
        protected virtual void UpdateSwitchPoints()
        {
            var forlornStartNodes = (from switchPoint in SwitchPoints where switchPoint.Value.EndPoints.Count == 1 select switchPoint.Key).ToList();
            foreach (var forlornStartNode in forlornStartNodes)
            {
                ForlornStartNodes.Add(forlornStartNode);
            }
            var listToRemove = (from switchPoint in SwitchPoints where switchPoint.Value.EndPoints.Count < 3  select switchPoint.Key).ToList();
            foreach (var key in listToRemove)
                SwitchPoints.Remove(key);
        }

        //Polygon

        //Work-in-Progress method
        protected void ProcessPolygons(List<VerticesEx> polygons, List<IObstacle> obstacles)
        {
            foreach (var polygon in polygons)
            {
                obstacles.Add(new PolygonObstacle(polygon, Layer));
            }
        }

        /// <summary>
        /// Creates a polygon out of a List of Lines.
        /// </summary>
        /// <param name="sourceSegments"></param>
        /// <returns></returns>
        public VerticesEx CreatePolygon(IEnumerable<LineSegment> sourceSegments)
        {
            var polygon = new VerticesEx();
            var directions = new List<Direction>();
            var lineSegments = new List<LineSegment>(sourceSegments);

            var currentItem = lineSegments.First().Start;
            var startPoint = currentItem;
            var currentDirection = Direction.None;
            while (lineSegments.Count > 0)
            {
                if (polygon.Count > 0 && startPoint == currentItem)
                    break;
                var preferedLine = ChooseNextLine(currentItem, lineSegments, currentDirection);
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
            //isRectangle = IsRectangleObstacle(polygon, directions);
            return polygon;
        }

        //private static bool IsRectangleObstacle(ICollection<Vector2> polygon, ICollection<Direction> directions)
        //{
        //    if (polygon.Count != 4 || directions.Count != 4)
        //        return false;
        //    return directions.Contains(Direction.Down) && directions.Contains(Direction.Right) && directions.Contains(Direction.Up) && directions.Contains(Direction.Left);
        //}

        private static void FixPolygonStartPoint(IList<Vector2> polygon, IList<Direction> directions)
        {
            if (polygon.Count != directions.Count || polygon.Count < 3)
                return;
            if (directions.First() != directions.Last())
                return;
            polygon.RemoveAt(0);
            directions.RemoveAt(0);
        }

        /// <summary>
        /// Splites a multi part figure into single parts.
        /// </summary>
        /// <param name="sourceSegments"></param>
        /// <param name="obstacles"></param>
        protected virtual List<VerticesEx> SplitPolygon(List<LineSegment> sourceSegments, List<IObstacle> obstacles)
        {
            //Debug.SaveSegmentsPicture(sourceSegments, "current");
            var verticesCombinations = new List<VerticesEx>();
            var polygonLinesDict = new Dictionary<VerticesEx, List<LineSegment>>();
            foreach (var switchPoint in SwitchPoints)
            {
                foreach (var endPoint in switchPoint.Value.EndPoints)
                {
                    var startPoint = switchPoint.Key;
                    var polygon = new VerticesEx();
                    
                    var polygonLines = new List<LineSegment>();

                    var remainingLines = new List<LineSegment>(sourceSegments);
                    var currentItem = startPoint;
                    var currentDirection = Direction.None;
                    do
                    {
                        if (currentItem == startPoint && polygon.Count > 0)
                        {
                            //Debug.SavePolygonPicture(polygon);
                            if (!verticesCombinations.Contains(polygon))
                            {
                                verticesCombinations.Add(polygon);
                                polygonLinesDict.Add(polygon, polygonLines);
                            }
                            break;
                        }
                        if (polygon.Contains(currentItem))
                            break;
                        polygon.Add(currentItem);
                        LineSegment preferedLine;
                        if (currentItem == startPoint)
                            preferedLine = GetLine(currentItem, endPoint, true);
                        else
                            preferedLine = ChooseNextLine(currentItem, remainingLines, currentDirection);
                        if (preferedLine == null)
                            break;
                        currentItem = preferedLine.End;
                        currentDirection = preferedLine.Direction;
                        polygonLines.Add(preferedLine);
                        remainingLines.Remove(preferedLine);
                    } while (true);
                }
            }

            verticesCombinations = RemoveUnnecessaryPolygons(verticesCombinations);

            var forlornLines = GetPolygonForlornLines(sourceSegments, verticesCombinations, polygonLinesDict);
            obstacles.AddRange(forlornLines.Select(forlornLine => new LineObstacle(forlornLine.Start, forlornLine.End, Layer)));

            return verticesCombinations;
        }

        private static IEnumerable<LineSegment> GetPolygonForlornLines(IEnumerable<LineSegment> sourceSegments, IEnumerable<VerticesEx> verticesCombinations, IDictionary<VerticesEx, List<LineSegment>> polygonLinesDict)
        {
            //Find lines which belongs to no polygon i.e. are forlorn
            var forlornLines = new List<LineSegment>(sourceSegments);
            var linesToRemove = new List<LineSegment>();
            foreach (var vertices in verticesCombinations)
            {
                List<LineSegment> polygonLines;
                if (!polygonLinesDict.TryGetValue(vertices, out polygonLines))
                    continue;
                foreach (var line in polygonLines.Where(line => !linesToRemove.Contains(line)))
                    linesToRemove.Add(line);
            }
            foreach (var lineSegment in linesToRemove)
                forlornLines.Remove(lineSegment);
            return forlornLines;
        }
        
        /// <summary>
        /// Removes vertices combinations which contain other vertices. We need the smallest possible combination.
        /// </summary>
        /// <param name="verticesCombinations"></param>
        /// <returns></returns>
        private static List<VerticesEx> RemoveUnnecessaryPolygons(List<VerticesEx> verticesCombinations)
        {
            var itemsToRemove = new List<VerticesEx>();
            foreach (var verticesCombination in verticesCombinations)
            {
                foreach (var verticesCombination2 in verticesCombinations)
                {
                    if (verticesCombination == verticesCombination2)
                        continue;
                    var contains = verticesCombination.Contains(verticesCombination2);
                    if (contains && !itemsToRemove.Contains(verticesCombination))
                        itemsToRemove.Add(verticesCombination);
                }
            }
            foreach (var item in itemsToRemove)
                verticesCombinations.Remove(item);
            return verticesCombinations;
        }

        protected internal static LineSegment ChooseNextLine(Vector2 currentItem, IEnumerable<LineSegment> lineSegments, Direction currentDirection)
        {
            var currentLines = GetLines(currentItem, lineSegments);
            if (currentLines.Count == 0)
                return null;
            var priority = int.MinValue;
            LineSegment tempLine = null;
            LineSegment preferedLine = null;
            foreach (var currentLine in currentLines)
            {
                if (currentItem == currentLine.End)
                    tempLine = currentLine.Invert();
                else if (currentItem == currentLine.Start)
                    tempLine = currentLine;
                var currentPriority = currentLines.Count == 1 ? 1 : GetLinePriority(currentDirection, tempLine);
                if (currentPriority <= priority)
                    continue;
                priority = currentPriority;
                preferedLine = tempLine;
            }
            return preferedLine;
        }

        protected static int GetLinePriority(Direction baseDirection, LineSegment line)
        {
            var priority = _baseDirectionPriority[line.Direction];
            if (baseDirection == Direction.None)
                baseDirection = Direction.Down;
            priority += 4 - _baseDirectionPriority[baseDirection];
            if (priority < 0)
                priority = 8 + priority;
            if (priority > 8)
                priority = priority - 8;
            return priority;
        }
    }
}
