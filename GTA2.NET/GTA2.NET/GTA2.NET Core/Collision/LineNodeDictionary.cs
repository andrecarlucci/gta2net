// GTA2.NET
// 
// File: LineNodeDictionary.cs
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
using Hiale.GTA2NET.Core.Helper;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public class LineNodeDictionary : SerializableDictionary<Vector2, List<LineSegment>>
    {
        //internal LineNodeDictionary()
        //{
            
        //}

        //public LineNodeDictionary(IEnumerable<ILineObstacle> obstacles)
        //{
        //    CreateLineNodes(obstacles);
        

        public virtual void CreateLineNodes(IEnumerable<ILineObstacle> obstacles)
        {
            CreateLineNodes(obstacles, false);
        }

        public virtual void CreateLineNodes(IEnumerable<ILineObstacle> obstacles, bool slopeMode)
        {
            //var nodes = new Dictionary<Vector2, List<LineSegment>>();
            foreach (var lineObstacle in obstacles)
            {
                var isSlope = lineObstacle.IsSlope;
                if (slopeMode)
                    isSlope = !isSlope;
                if (isSlope)
                    continue;

                //start point
                var newLine = new LineSegment(lineObstacle.Start, lineObstacle.End);
                InsertLine(newLine);

                //end point
                newLine = new LineSegment(lineObstacle.End, lineObstacle.Start);
                InsertLine(newLine);
            }
        }

        protected void InsertLine(LineSegment newLine)
        {
            List<LineSegment> vectorList;

            if (TryGetValue(newLine.Start, out vectorList))
                vectorList.Add(newLine);
            else
            {
                vectorList = new List<LineSegment> { newLine };
                Add(newLine.Start, vectorList);
            }
        }

        public void Purge(IEnumerable<LineSegment> lines)
        {
            foreach (var line in lines)
            {
                Remove(line.Start);
                Remove(line.End);
            }
        }
    }

}
