using System;
using System.Collections.Generic;

namespace Hiale.GTA2NET.Core.Collision
{
    public class SubMatrix<T>
    {
        public static void FindAllRectangles(T item)
        {
            
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

        //    Console.ReadKey();
        //}

        public static void MaxSubmatrix(T[,] matrix, T positiveValue)
        {
            
        }

        public static void MaxSubmatrix(int[,] matrix)
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
                    if (matrix[i, j] == 1)
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
                for (int j = m - 1; j >= 0; j--)
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
                    tempArea = (i - d[j])*(d2[j] - d1[j] - 1);
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

            Console.WriteLine();
            Console.WriteLine(maxArea);
            Console.WriteLine(String.Format("({0}, {1}) to ({2}, {3})", x1, y1, x2, y2));

            Console.WriteLine();
            Console.WriteLine("Original");

            for (var y = 0; y < n; y++)
            {
                for (var x = 0; x < m; x++)
                    Console.Write(String.Format("{0} ", matrix[y, x]));
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("Maximum submatrix");

            for (var y = 0; y < n; y++)
            {
                for (var x = 0; x < m; x++)
                {
                    if (x >= x1 && x <= x2 && y >= y1 && y <= y2)
                        Console.Write("- ");
                    else
                        Console.Write(String.Format("{0} ", matrix[y, x]));
                }
                Console.WriteLine();
            }
        }
    }
}