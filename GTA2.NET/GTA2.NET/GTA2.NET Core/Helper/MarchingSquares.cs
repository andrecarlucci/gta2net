//10.03.2013
//Based on http://devblog.phillipspiess.com/2010/02/23/better-know-an-algorithm-1-marching-squares/

// Licensed under the creative commons Atrribution 3.0 license
// You are free to share, copy, distribute, and transmit this work
// You are free to alter this work
// If you use this work, please attribute it somewhere in the supporting
// documentation of your work. (A mention in a readme or credits, for example.)
// Please leave a comment or email me if you use this work, so I am compelled
// to produce more of it's kind.
// 2010 - Phillip Spiess

using Hiale.GTA2NET.Core.Collision;
using System.Collections.Generic;
using Hiale.GTA2NET.Core.Map;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Helper
{
    public class MarchingSquares
    {
        // A simple enumeration to represent the direction we
        // just moved, and the direction we will next move.
        private enum StepDirection
        {
            None,
            Up,
            Left,
            Down,
            Right
        }

        public static List<Vector2> FindPolygonPoints(CollisionMapType[,,] blockCollisions, BlockInfo[,,] blocks,  Vector2 start, int z)
        {
            var specialSlopes = new List<Vector2>();
            var rawPolygon = IdentifyPerimeter(blockCollisions, blocks, start, z, specialSlopes);
            ProcessSlopes(rawPolygon, specialSlopes, blocks, z);
            var polygon = OptimizePolygon(rawPolygon, start);
            return polygon;
        }

        private static List<Vector2> OptimizePolygon(IList<Vector2> rawPolygon, Vector2 start)
        {
            var polygon = new List<Vector2>();

            var oldX = -1;
            var oldY = -1;
            var end = false;

            rawPolygon.Add(rawPolygon[0]); //add start point to the end

            for (var i = 0; i < rawPolygon.Count; i++)
            {
                var j = i;
                while ((int) rawPolygon[j].X == oldX && (int) rawPolygon[j].Y != oldY)
                {
                    if (j + 1 >= rawPolygon.Count)
                    {
                        end = true;
                        break;
                    }
                    j++;
                }
                while ((int) rawPolygon[j].Y == oldY && (int) rawPolygon[j].X != oldX)
                {
                    if (j + 1 >= rawPolygon.Count)
                    {
                        end = true;
                        break;
                    }
                    j++;
                }
                if (end)
                    break;
                if ((j - i) >= 2)
                    i = j - 1;
                polygon.Add(new Vector2(rawPolygon[i].X, rawPolygon[i].Y));
                oldX = (int) rawPolygon[i].X;
                oldY = (int) rawPolygon[i].Y;
            }

            if (polygon[0] == polygon[polygon.Count - 1])
                polygon.RemoveAt(polygon.Count - 1);

            return polygon;
        }

        private static void ProcessSlopes(IList<Vector2> polygon, IEnumerable<Vector2> specialSlopes, BlockInfo[, ,] blocks, int z)
        {
            foreach (var specialSlope in specialSlopes)
            {
                var x = (int) specialSlope.X;
                var y = (int) specialSlope.Y;
                int index;
                switch (blocks[x, y, z].SlopeType)
                {
                    case SlopeType.DiagonalFacingUpLeft:
                        RemovePoint(x + 1, y + 1, polygon);
                        break;
                    case SlopeType.DiagonalFacingUpRight:
                        RemovePoint(x, y + 1, polygon);
                        break;
                    case SlopeType.DiagonalFacingDownLeft:
                        RemovePoint(x + 1, y, polygon);
                        break;
                    case SlopeType.DiagonalFacingDownRight:
                        RemovePoint(x, y, polygon);
                        break;
                }
            }
        }

        private static void RemovePoint(int x, int y, IList<Vector2> polygon)
        {
            var indexToRemove = -1;
            for (var i = 0; i < polygon.Count; i++)
            {
                if ((int) polygon[i].X != x || (int) polygon[i].Y != y)
                    continue;
                indexToRemove = i;
                break;
            }
            if (indexToRemove > -1)
                polygon.RemoveAt(indexToRemove);
        }


        // Performs the main while loop of the algorithm
        private static IList<Vector2> IdentifyPerimeter(CollisionMapType[, ,] blockCollisions, BlockInfo[, ,] blocks, Vector2 start, int z, List<Vector2> specialSlopes)
        {
            var startX = (int) start.X;
            var startY = (int) start.Y;

            if (startX < 0)
                startX = 0;
            if (startX > blockCollisions.GetLength(0))
                startX = blockCollisions.GetLength(0);
            if (startY < 0)
                startY = 0;
            if (startY > blockCollisions.GetLength(1))
                startY = blockCollisions.GetLength(1);

            var pointList = new List<Vector2>();

            var x = startX;
            var y = startY;

            var nextStep = StepDirection.None;

            // The main while loop, continues stepping until we return to our initial points
            do
            {
                // Evaluate our state, and set up our next direction
                Step(x, y, z, blockCollisions, blocks, ref nextStep, specialSlopes);

                // If our current point is within our array bounds, add it to the list of points
                if (x >= 0 && x < blockCollisions.GetLength(0) && y >= 0 && y < blockCollisions.GetLength(1))
                    pointList.Add(new Vector2(x, y));

                switch (nextStep)
                {
                    case StepDirection.Up:
                        y--;
                        break;
                    case StepDirection.Left:
                        x--;
                        break;
                    case StepDirection.Down:
                        y++;
                        break;
                    case StepDirection.Right:
                        x++;
                        break;
                }
            } while (x != startX || y != startY);

            return pointList;

        }

        // Determines and sets the state of the 4 blocks that represent our current state, and sets our current and previous directions
        private static void Step(int x, int y, int z, CollisionMapType[, ,] blockCollisions, BlockInfo[, ,] blocks, ref StepDirection nextStep, List<Vector2> specialSlopes)
        {
            // Scan our 4 pixel area
            var upLeft = IsBlockOccupied(x - 1, y - 1, z, blockCollisions, blocks, specialSlopes);
            var upRight = IsBlockOccupied(x, y - 1, z, blockCollisions, blocks, specialSlopes);
            var downLeft = IsBlockOccupied(x - 1, y, z, blockCollisions, blocks, specialSlopes);
            var downRight = IsBlockOccupied(x, y, z, blockCollisions, blocks, specialSlopes);

            // Store our previous step
            var previousStep = nextStep;

            // Determine which state we are in
            var state = 0;

            if (upLeft)
                state |= 1;
            if (upRight)
                state |= 2;
            if (downLeft)
                state |= 4;
            if (downRight)
                state |= 8;

            // State now contains a number between 0 and 15
            // representing our state.
            // In binary, it looks like 0000-1111 (in binary)

            // An example. Let's say the top two pixels are filled,
            // and the bottom two are empty.
            // Stepping through the if statements above with a state 
            // of 0b0000 initially produces:
            // Upper Left == true ==>  0b0001
            // Upper Right == true ==> 0b0011
            // The others are false, so 0b0011 is our state 
            // (That's 3 in decimal.)

            // Looking at the chart above, we see that state
            // corresponds to a move right, so in our switch statement
            // below, we add a case for 3, and assign Right as the
            // direction of the next step. We repeat this process
            // for all 16 states.

            // So we can use a switch statement to determine our
            // next direction based on
            switch (state)
            {
                case 1:
                    nextStep = StepDirection.Up;
                    break;
                case 2:
                    nextStep = StepDirection.Right;
                    break;
                case 3:
                    nextStep = StepDirection.Right;
                    break;
                case 4:
                    nextStep = StepDirection.Left;
                    break;
                case 5:
                    nextStep = StepDirection.Up;
                    break;
                case 6:
                    nextStep = previousStep == StepDirection.Up ? StepDirection.Left : StepDirection.Right;
                    break;
                case 7:
                    nextStep = StepDirection.Right;
                    break;
                case 8:
                    nextStep = StepDirection.Down;
                    break;
                case 9:
                    nextStep = previousStep == StepDirection.Right ? StepDirection.Up : StepDirection.Down;
                    break;
                case 10:
                    nextStep = StepDirection.Down;
                    break;
                case 11:
                    nextStep = StepDirection.Down;
                    break;
                case 12:
                    nextStep = StepDirection.Left;
                    break;
                case 13:
                    nextStep = StepDirection.Up;
                    break;
                case 14:
                    nextStep = StepDirection.Left;
                    break;
                default:
                    nextStep = StepDirection.None;
                    break;
            }
        }

        private static bool IsBlockOccupied(int x, int y, int z, CollisionMapType[, ,] blockCollisions, BlockInfo[, ,] blocks, List<Vector2> specialSlopes)
        {
            if (x < 0 || y < 0 || x >= blockCollisions.GetLength(0) || y >= blockCollisions.GetLength(1))
                return false;

            if (blocks[x, y, z].SlopeType.EqualsAnyOf(SlopeType.DiagonalFacingUpLeft,
                                                      SlopeType.DiagonalFacingUpRight,
                                                      SlopeType.DiagonalFacingDownLeft,
                                                      SlopeType.DiagonalFacingDownRight))
            {
                var slopeVector = new Vector2(x, y);
                if (!specialSlopes.Contains(slopeVector))
                    specialSlopes.Add(slopeVector);
            }

            return blockCollisions[x, y, z] == CollisionMapType.Block;
        }
    }
}

