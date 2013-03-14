// GTA2.NET
// 
// File: Submatrix.cs
// Created: 03.03.2013
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
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public class SubMatrix
    {
        public static void FindAllRectangles(CollisionMapType[,,] matrix, int z, List<IObstacle> obstacles)
        {
            do
            {
                var rect = MaxSubmatrix(matrix, z, CollisionMapType.Block);
                if (rect.Width*rect.Height <= 0)
                    break;
                obstacles.Add(new RectangleObstacle(new Vector2(rect.Y, rect.X),z, rect.Height, rect.Width));
                for (var i = rect.X; i < rect.X + rect.Width; i++)
                {
                    for (var j = rect.Y; j < rect.Y + rect.Height; j++)
                        matrix[j, i, z] = CollisionMapType.None;
                }
            } while (true);
        }


        //static void Main(string[] args)
        //{
        //    int[,] one =
        //        {
        //            {0, 1, 1, 1, 0},
        //            {0, 1, 1, 1, 0},
        //            {0, 1, 0, 0, 0},
        //            {0, 0, 0, 0, 0},
        //            {0, 0, 0, 1, 0}
        //        };

        //    MaxSubmatrix(one);

        //    System.Diagnostics.Debug.ReadKey();
        //}

        //In this method, x and y are swapped, ToDo: change this
        public static Rectangle MaxSubmatrix(CollisionMapType[,,] matrix, int z, CollisionMapType wantedValue)
        {
            var n = matrix.GetLength(0); // Number of rows
            var m = matrix.GetLength(1); // Number of columns

            int maxArea = -1, tempArea = -1;

            // Top-left corner (x1, y1); bottom-right corner (x2, y2)
            int x1 = 0, y1 = 0, x2 = 0, y2 = 0;

            // Maximum row containing a 1 in this column
            var d = new int[m];

            // Initialize array to -1
            for (var i = 0; i < m; i++)
                d[i] = -1;

            // Furthest left column for rectangle
            var d1 = new int[m];

            // Furthest right column for rectangle
            var d2 = new int[m];

            var stack = new Stack<int>();

            // Work down from top row, searching for largest rectangle
            for (var i = 0; i < n; i++)
            {
                // 1. Determine previous row to contain a '1'
                for (var j = 0; j < m; j++)
                {
                    if (matrix[i, j, z] != wantedValue) //CHANGE HERE
                        d[j] = i;
                }
                stack.Clear();

                // 2. Determine the left border positions
                for (var j = 0; j < m; j++)
                {
                    while (stack.Count > 0 && d[stack.Peek()] <= d[j])
                        stack.Pop();

                    // If stack is empty, use -1; i.e. all the way to the left
                    d1[j] = (stack.Count == 0) ? -1 : stack.Peek();
                    stack.Push(j);
                }
                stack.Clear();

                // 3. Determine the right border positions
                for (var j = m - 1; j >= 0; j--)
                {
                    while (stack.Count > 0 && d[stack.Peek()] <= d[j])
                        stack.Pop();

                    d2[j] = (stack.Count == 0) ? m : stack.Peek();
                    stack.Push(j);
                }

                // 4. See if we've found a new maximum submatrix
                for (var j = 0; j < m; j++)
                {
                    // (i - d[j]) := current row - last row in this column to contain a 1
                    // (d2[j] - d1[j] - 1) := right border - left border - 1
                    tempArea = (i - d[j]) * (d2[j] - d1[j] - 1);
                    if (tempArea > maxArea)
                    {
                        maxArea = tempArea;

                        // Top left
                        x1 = d1[j] + 1;
                        y1 = d[j] + 1;

                        // Bottom right
                        x2 = d2[j] - 1;
                        y2 = i;
                    }
                }
            }
            var rect = new Rectangle(x1, y1, x2 - x1 + 1, y2 - y1 + 1);
            return rect;
        }
    }
}