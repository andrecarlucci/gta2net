// GTA2.NET
// 
// File: SlopeFigure.cs
// Created: 07.07.2013
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
using FarseerPhysics.Common.PolygonManipulation;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Core.Map;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public class SlopeFigure : RawFigure
    {
        public SlopeFigure(Map.Map map, int layer, LineNodeDictionary nodes) : base(map, layer, nodes)
        {

        }

        public override void Tokenize(ObstacleCollection obstacles)
        {
            var polygons = SplitPolygon(Lines, obstacles);
            var blocks = Polygon.GetAssociatedBlocks(polygons, Map, Layer).ToList();
            var polygonEdges = new List<LineSegment>();
            foreach (var polygon in polygons)
            {
                for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
                    polygonEdges.Add(GetLine(polygon[i], polygon[j], false, Lines));
            }
            var intersectionLines = polygonEdges.GroupBy(x => x).Where(group => group.Count() > 1).Select(group => group.Key).ToList(); //finds duplicated items

            var blockLines = new Dictionary<Block, List<LineSegment>>(); //create a dictionary of block which intersection line they belong to. Because if the intersection line does not match,
                                                                         //all intersecting lines of that block must be set invalid. Otherwise they engine would not not create a separate polygon.
            var mismatchBlocks = new List<Block>();

            foreach (var intersectionLine in intersectionLines)
            {
                Block blockX;
                Block blockY;
                Orientation orientation;
                GetNeighborBlocks(intersectionLine, blocks, out blockX, out blockY, out orientation);
                if (BlocksMatch(blockX, blockY, orientation))
                {
                    Lines.Remove(intersectionLine);
                }
                else
                {
                    if (blockX != null & !mismatchBlocks.Contains(blockX))
                        mismatchBlocks.Add(blockX);
                    if (blockY != null & !mismatchBlocks.Contains(blockY))
                        mismatchBlocks.Add(blockY);
                }
                AddBlockLines(blockLines, blockX, intersectionLine);
                AddBlockLines(blockLines, blockY, intersectionLine);
            }
            foreach (var mismatchBlock in mismatchBlocks)
            {
                //Restore all lines of the mismatched blocks
                List<LineSegment> blockLineSegments;
                if (!blockLines.TryGetValue(mismatchBlock, out blockLineSegments))
                    continue;
                foreach (var blockSegment in blockLineSegments.Where(blockSegment => !Lines.Contains(blockSegment)))
                    Lines.Add(blockSegment);
            }

            polygons.Clear();
            polygons = SplitPolygon(Lines, obstacles);
            foreach (var polygon in polygons)
            {
                var simplePolygon = SimplifyTools.CollinearSimplify(polygon);
                AddSlopeObstacle(simplePolygon, VerticesEx.IsRectangle(simplePolygon), obstacles, Layer);
            }
        }

        private static void AddBlockLines(IDictionary<Block, List<LineSegment>> blockLines, Block block, LineSegment intersectionLine)
        {
            if (block == null)
                return;
            List<LineSegment> blockLineSegments;
            if (blockLines.TryGetValue(block, out blockLineSegments))
            {
                blockLineSegments.Add(intersectionLine);
            }
            else
            {
                blockLineSegments = new List<LineSegment> {intersectionLine};
                blockLines.Add(block, blockLineSegments);
            }
        }

        private void GetNeighborBlocks(LineSegment line, IEnumerable<Block> blocks, out Block blockX, out Block blockY, out Orientation orientation)
        {
            blockX = null;
            blockY = null;
            orientation = Orientation.Other;
            foreach (var block in blocks)
            {
                var found = false;
                var blockLines = Polygon.GetBlockLines(block, Layer).Where(item => item.IsSlope);
                foreach (var lineObstacle in blockLines)
                {
                    
                    if (line.Start == lineObstacle.Start && line.End == lineObstacle.End)
                        found = true;
                    else if (line.End == lineObstacle.Start && line.Start == lineObstacle.End)
                        found = true;
                    if (found)
                        break;
                }
                if (!found)
                    continue;
                if (blockX == null)
                    blockX = block;
                else
                {
                    blockY = block;
                    //ok, we have both blocks
                    var isVertical = line.Direction == Direction.Down || line.Direction == Direction.Up;
                    var isHorizontal = line.Direction == Direction.Left || line.Direction == Direction.Right;

                    if (isVertical)
                    {
                        orientation = Orientation.Vertical;
                        if (blockX.Position.X < blockY.Position.X)
                            return;
                    }
                    if (isHorizontal)
                    {
                        orientation = Orientation.Horizontal;
                        if (blockX.Position.Y < blockY.Position.Y)
                            return;
                    }
                    var blockTemp = blockX;
                    blockX = blockY;
                    blockY = blockTemp;
                    return;
                }
            }
        }

        private static bool BlocksMatch(Block blockX, Block blockY, Orientation orientation)
        {
            //well, I should do it more dynamically...
            if (blockX == null || blockY == null)
                return true;

            if (blockX.SlopeDirection == blockY.SlopeDirection && blockX.SlopeSubTyp == blockY.SlopeSubTyp)
            {
                if ((blockX.SlopeDirection == SlopeDirection.Up || blockX.SlopeDirection == SlopeDirection.Down) && orientation == Orientation.Vertical)
                    return true;
                if ((blockX.SlopeDirection == SlopeDirection.Right || blockX.SlopeDirection == SlopeDirection.Left) && orientation == Orientation.Horizontal)
                    return true;
            }

            if (blockX.SlopeType == SlopeType.Up26High && blockY.SlopeType == SlopeType.Up26Low && orientation == Orientation.Horizontal)
                return true;
            if (blockX.SlopeType == SlopeType.Down26Low && blockY.SlopeType == SlopeType.Down26High && orientation == Orientation.Horizontal)
                return true;
            if (blockX.SlopeType == SlopeType.Left26High && blockY.SlopeType == SlopeType.Left26Low && orientation == Orientation.Vertical)
                return true;
            if (blockX.SlopeType == SlopeType.Right26Low && blockY.SlopeType == SlopeType.Right26High && orientation == Orientation.Vertical)
                return true;

            //Up
            if (blockX.SlopeType == SlopeType.Up7High0 && blockY.SlopeType == SlopeType.Up7Low && orientation == Orientation.Horizontal)
                return true;
            if (blockX.SlopeType == SlopeType.Up7High1 && blockY.SlopeType == SlopeType.Up7High0 && orientation == Orientation.Horizontal)
                return true;
            if (blockX.SlopeType == SlopeType.Up7High2 && blockY.SlopeType == SlopeType.Up7High1 && orientation == Orientation.Horizontal)
                return true;
            if (blockX.SlopeType == SlopeType.Up7High3 && blockY.SlopeType == SlopeType.Up7High2 && orientation == Orientation.Horizontal)
                return true;
            if (blockX.SlopeType == SlopeType.Up7High4 && blockY.SlopeType == SlopeType.Up7High3 && orientation == Orientation.Horizontal)
                return true;
            if (blockX.SlopeType == SlopeType.Up7High5 && blockY.SlopeType == SlopeType.Up7High4 && orientation == Orientation.Horizontal)
                return true;
            if (blockX.SlopeType == SlopeType.Up7High6 && blockY.SlopeType == SlopeType.Up7High5 && orientation == Orientation.Horizontal)
                return true;

            //Down
            if (blockX.SlopeType == SlopeType.Down7Low && blockY.SlopeType == SlopeType.Down7High0 && orientation == Orientation.Horizontal)
                return true;
            if (blockX.SlopeType == SlopeType.Down7High0 && blockY.SlopeType == SlopeType.Down7High1 && orientation == Orientation.Horizontal)
                return true;
            if (blockX.SlopeType == SlopeType.Down7High1 && blockY.SlopeType == SlopeType.Down7High2 && orientation == Orientation.Horizontal)
                return true;
            if (blockX.SlopeType == SlopeType.Down7High2 && blockY.SlopeType == SlopeType.Down7High3 && orientation == Orientation.Horizontal)
                return true;
            if (blockX.SlopeType == SlopeType.Down7High3 && blockY.SlopeType == SlopeType.Down7High4 && orientation == Orientation.Horizontal)
                return true;
            if (blockX.SlopeType == SlopeType.Down7High4 && blockY.SlopeType == SlopeType.Down7High5 && orientation == Orientation.Horizontal)
                return true;
            if (blockX.SlopeType == SlopeType.Down7High5 && blockY.SlopeType == SlopeType.Down7High6 && orientation == Orientation.Horizontal)
                return true;

            //Left
            if (blockX.SlopeType == SlopeType.Left7High0 && blockY.SlopeType == SlopeType.Left7Low && orientation == Orientation.Vertical)
                return true;
            if (blockX.SlopeType == SlopeType.Left7High1 && blockY.SlopeType == SlopeType.Left7High0 && orientation == Orientation.Vertical)
                return true;
            if (blockX.SlopeType == SlopeType.Left7High2 && blockY.SlopeType == SlopeType.Left7High1 && orientation == Orientation.Vertical)
                return true;
            if (blockX.SlopeType == SlopeType.Left7High3 && blockY.SlopeType == SlopeType.Left7High2 && orientation == Orientation.Vertical)
                return true;
            if (blockX.SlopeType == SlopeType.Left7High4 && blockY.SlopeType == SlopeType.Left7High3 && orientation == Orientation.Vertical)
                return true;
            if (blockX.SlopeType == SlopeType.Left7High5 && blockY.SlopeType == SlopeType.Left7High4 && orientation == Orientation.Vertical)
                return true;
            if (blockX.SlopeType == SlopeType.Left7High6 && blockY.SlopeType == SlopeType.Left7High5 && orientation == Orientation.Vertical)
                return true;

            //Right
            if (blockX.SlopeType == SlopeType.Right7Low && blockY.SlopeType == SlopeType.Right7High0 && orientation == Orientation.Vertical)
                return true;
            if (blockX.SlopeType == SlopeType.Right7High0 && blockY.SlopeType == SlopeType.Right7High1 && orientation == Orientation.Vertical)
                return true;
            if (blockX.SlopeType == SlopeType.Right7High1 && blockY.SlopeType == SlopeType.Right7High2 && orientation == Orientation.Vertical)
                return true;
            if (blockX.SlopeType == SlopeType.Right7High2 && blockY.SlopeType == SlopeType.Right7High3 && orientation == Orientation.Vertical)
                return true;
            if (blockX.SlopeType == SlopeType.Right7High3 && blockY.SlopeType == SlopeType.Right7High4 && orientation == Orientation.Vertical)
                return true;
            if (blockX.SlopeType == SlopeType.Right7High4 && blockY.SlopeType == SlopeType.Right7High5 && orientation == Orientation.Vertical)
                return true;
            if (blockX.SlopeType == SlopeType.Right7High5 && blockY.SlopeType == SlopeType.Right7High6 && orientation == Orientation.Vertical)
                return true;

            if (blockX.SlopeType == SlopeType.Up45 && blockY.SlopeType == SlopeType.Down45 && orientation == Orientation.Horizontal)
                return true;
            if (blockX.SlopeType == SlopeType.Down45 && blockY.SlopeType == SlopeType.Up45 && orientation == Orientation.Horizontal)
                return true;

            if (blockX.SlopeType == SlopeType.Left45 && blockY.SlopeType == SlopeType.Right45 && orientation == Orientation.Vertical)
                return true;
            if (blockX.SlopeType == SlopeType.Right45 && blockY.SlopeType == SlopeType.Left45 && orientation == Orientation.Vertical)
                return true;

            return false;
        }

        protected static void AddSlopeObstacle(List<Vector2> polygonVertices, bool isRectangle, ObstacleCollection obstacles, int layer)
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
                var rectangle = new RectangleObstacle(minX, minY, width, height, layer) {IsSlope = true};
                obstacles.Add(rectangle);
            }
            else
            {
                var polygonObstacle = new PolygonObstacle(polygonVertices, layer) {IsSlope = true};
                obstacles.Add(polygonObstacle);
            }
        }
    }
}
