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
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Hiale.GTA2NET.Core.Helper;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public class Figure : IXmlSerializable
    {
        public List<LineSegment> Lines { get; private set; }

        private SerializableDictionary<Vector2, List<LineSegment>> Nodes { get; set; }

        private SerializableDictionary<Vector2, SwitchPoint> SwitchPoints { get; set; }

        private List<Vector2> ForlornStartNodes { get; set; }

        private static Dictionary<Direction, int> _baseDirectionPriority;

        private Figure()
        {
            PrepareBasePriorityTable();
            Lines = new List<LineSegment>();
            Nodes = new SerializableDictionary<Vector2, List<LineSegment>>();
            ForlornStartNodes = new List<Vector2>();
            SwitchPoints = new SerializableDictionary<Vector2, SwitchPoint>();
        }

        public Figure(Vector2 origin, Dictionary<Vector2, List<LineSegment>> nodes) : this()
        {
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
                //if (start != null && start.Value.X == 99 && start.Value.Y == 174 || end != null && end.Value.X == 99 && end.Value.Y == 174)
                //    Console.WriteLine();
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

        public List<LineSegment> Tokenize() //ToDo: return value: List<IObstacle>
        {
            var obstacles = new List<LineSegment>();

            GetForlorn(obstacles);
            if (Lines.Count == 0)
                return obstacles;

            if (SwitchPoints.Count > 0)
            {
                var guid = Guid.NewGuid();
                Save("debug\\" + guid + ".xml");
                MapCollision.SaveSegmentsPicture(Lines, guid.ToString());
                var switchPointValues = SwitchPoints.Select(switchPoint => switchPoint.Value.EndPoints).ToList();
                var combinations = GetCombinations(switchPointValues);
            }
            else
            {
                bool isRectangle;
                var polygonVertices = CreatePolygon(Lines, out isRectangle);
                if (isRectangle)
                {
                    var width = polygonVertices[2].X - polygonVertices[0].X;
                    var height = polygonVertices[1].Y - polygonVertices[0].Y;
                    //var rectangle = new RectangleObstacle(polygonVertices[0], currentLayer, width, height);
                    //obstacles.Add(rectangle);
                }
            }
            return obstacles;
        }

        private static void DebugPolygon(List<Vector2> polygon)
        {
            using (var bmp = new System.Drawing.Bitmap(2560, 2560))
            {
                using (var g = System.Drawing.Graphics.FromImage(bmp))
                {
                    var points = new System.Drawing.Point[polygon.Count];
                    for (var i = 0; i < polygon.Count; i++)
                        points[i] = new System.Drawing.Point((int)polygon[i].X * 10, (int)polygon[i].Y * 10);
                    g.FillPolygon(new System.Drawing.SolidBrush(System.Drawing.Color.OrangeRed), points);
                }
                bmp.Save("debug\\polygon.png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private void GetForlorn(List<LineSegment> obstacles)
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
                    obstacles.Add(line);
                    Lines.Remove(line);
                    RemoveNode(currentItem, line);
                    RemoveNode(forlornRoot, line);
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
            var listToRemove = (from switchPoint in SwitchPoints where switchPoint.Value.EndPoints.Count == 2 select switchPoint.Key).ToList();
            foreach (var key in listToRemove)
                SwitchPoints.Remove(key);
        }

        private Vector2 GetforlornRoot(Vector2 forlornStart, out List<LineSegment> forlornLines, out Vector2 previousItem)
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

        private List<Vector2> CreatePolygon(List<LineSegment> sourceSegments, out bool isRectangle)
        {
            isRectangle = false;
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
                var currentLines = GetLines(currentItem, lineSegments);
                //var currentLines = lineSegments.Where(lineSegment => lineSegment.Start == currentItem).ToList(); //ToDo
                //currentLines.AddRange(lineSegments.Where(lineSegment => lineSegment.End == currentItem).ToList());
                if (currentLines.Count == 0)
                    break;
                var minPriority = int.MaxValue;
                LineSegment preferedLine = null;
                LineSegment directedLine = null;
                LineSegment tempLine = null;
                foreach (var currentLine in currentLines)
                {
                    if (currentItem == currentLine.End)
                        tempLine = new LineSegment(currentLine.End, currentLine.Start);
                    else if (currentItem == currentLine.Start)
                        tempLine = currentLine;
                    var currentPriority = GetDirectionPriority(currentDirection, tempLine.Direction);
                    if (currentPriority >= minPriority)
                        continue;
                    minPriority = currentPriority;
                    preferedLine = currentLine;
                    directedLine = tempLine;
                }
                if (preferedLine == null)
                    return new List<Vector2>();
                lineSegments.Remove(preferedLine);
                var previousItem = currentItem;
                currentItem = directedLine.End;
                if (directedLine.Direction == currentDirection)
                    continue;
                currentDirection = directedLine.Direction;
                polygon.Add(previousItem);
                directions.Add(currentDirection);
            }
            FixPolygonStartPoint(polygon, directions);
            isRectangle = IsRectangleObstacle(polygon, directions);
            return polygon;
        }

        private static int GetDirectionPriority(Direction baseDirection, Direction newDirection)
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

        private void CheckCombination(SwitchPointCombination combination)
        {
            //var lines = new List<LineSegment>();
            //var visitedItems = new List<Vector2>();
            //LineSegment lineSegment;
            //Vector2 currentItem = Lines[0].Start;
            //Vector2 previousItem = currentItem;
            //var count = 0;
            //while (true)
            //{
            //    if (visitedItems.Contains(currentItem))
            //    {
            //        break;
            //    }
            //    else
            //    {
            //        visitedItems.Add(currentItem);
            //    }

            //    if (currentItem == combination.Origin)
            //    {
            //        lineSegment = GetLine(currentItem, combination.Target, false);
            //        count++;
            //        currentItem = combination.Target;
            //        continue;

            //    }
            //    var connectedNodes = GetConnectedNodes(currentItem);
            //    connectedNodes.Remove(previousItem);
            //    if (connectedNodes.Count == 0)
            //        break;
            //    count++;
            //    currentItem = connectedNodes[0];
            //    previousItem = currentItem;
            //}

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

        private void RemoveNode(Vector2 key, LineSegment line)
        {
            List<LineSegment> segments;
            if (!Nodes.TryGetValue(key, out segments))
                return;
            segments.Remove(line);
            segments.Remove(new LineSegment(line.End, line.Start));
            if (segments.Count == 0)
                Nodes.Remove(key);
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

        private List<LineSegment> GetLines(Vector2 origin, IList<LineSegment> sourceLines)
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

        private static List<List<T>> GetCombinations<T>(ICollection<List<T>> input)
        {
            var selected = new T[input.Count];
            var result = new List<List<T>>();
            GetCombinations(selected, 0, input, result);
            return result;
        }

        private static void GetCombinations<T>(IList<T> selected, int index, IEnumerable<IEnumerable<T>> remaining, ICollection<List<T>> output)
        {
            // ReSharper disable PossibleMultipleEnumeration
            var nextList = remaining.FirstOrDefault();
            if (nextList == null)
                output.Add(new List<T>(selected));
            else
            {
                foreach (var i in nextList)
                {
                    selected[index] = i;
                    GetCombinations(selected, index + 1, remaining.Skip(1), output);
                }
            }
            // ReSharper restore PossibleMultipleEnumeration
        }

        private static void PrepareBasePriorityTable()
        {
            if (_baseDirectionPriority != null)
                return;
            _baseDirectionPriority = new Dictionary<Direction, int> { { Direction.UpLeft, 1 }, { Direction.Left, 2 }, { Direction.DownLeft, 3 }, { Direction.Down, 4 }, { Direction.DownRight, 5 }, { Direction.Right, 6 }, { Direction.UpRight, 7 }, { Direction.Up, 8 } };
        }

        public static Figure Load(string fileName)
        {
            var deserializer = new XmlSerializer(typeof (Figure));
            TextReader reader = new StreamReader(fileName);
            var figure = (Figure)deserializer.Deserialize(reader);
            reader.Close();
            return figure;
        }

        public void Save(string fileName)
        {
            var settings = new XmlWriterSettings {Indent = true};
            var xmlWriter = XmlWriter.Create(fileName, settings);
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            var serializer = new XmlSerializer(typeof (Figure));
            serializer.Serialize(xmlWriter, this, ns);
            xmlWriter.Close();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var doc = new XmlDocument { PreserveWhitespace = false };
            doc.Load(reader);

            Lines.Clear();
            var lineSegments = doc.GetElementsByTagName("LineSegments")[0];
            foreach (XmlNode lineSegment in lineSegments)
                Lines.Add( (LineSegment)new XmlSerializer(typeof(LineSegment)).Deserialize(XmlReader.Create(new StringReader(lineSegment.OuterXml))));

            Nodes.Clear();
            var nodes = doc.GetElementsByTagName("Nodes")[0];
            var xmlReader = XmlReader.Create(new StringReader(nodes.OuterXml));
            xmlReader.MoveToContent();
            Nodes.ReadXml(xmlReader);

            SwitchPoints.Clear();
            var switchPoints = doc.GetElementsByTagName("SwitchPoints")[0];
            xmlReader = XmlReader.Create(new StringReader(switchPoints.OuterXml));
            xmlReader.MoveToContent();
            SwitchPoints.ReadXml(xmlReader);

            ForlornStartNodes.Clear();
            var forlornStartNodes = doc.GetElementsByTagName("ForlornStartNodes")[0];
            foreach (XmlNode forlornStartNode in forlornStartNodes)
                ForlornStartNodes.Add((Vector2)new XmlSerializer(typeof(Vector2)).Deserialize(XmlReader.Create(new StringReader(forlornStartNode.OuterXml))));
        }

        public void WriteXml(XmlWriter writer)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            writer.WriteStartElement("LineSegments");
            foreach (var lineSegment in Lines)
                new XmlSerializer(typeof(LineSegment)).Serialize(writer, lineSegment, ns);
            writer.WriteEndElement();

            writer.WriteStartElement("Nodes");
            Nodes.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("SwitchPoints");
            SwitchPoints.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("ForlornStartNodes");
            foreach (var forlornStartNode in ForlornStartNodes)
                new XmlSerializer(typeof(Vector2)).Serialize(writer, forlornStartNode, ns);
            writer.WriteEndElement();
        }
    }
}
