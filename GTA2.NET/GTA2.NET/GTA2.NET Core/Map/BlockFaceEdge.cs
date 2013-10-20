// GTA2.NET
// 
// File: BlockFaceEdge.cs
// Created: 27.02.2013
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

using Hiale.GTA2NET.Core.Helper;
using System.Collections.Generic;
using System.Linq;

namespace Hiale.GTA2NET.Core.Map
{
    public class BlockFaceEdge : BlockFace
    {
        public static BlockFaceEdge Empty = new BlockFaceEdge();

        /// <summary>
        /// Wall indicates whether or not a car, ped or object should collide with this tile.
        /// </summary>
        public bool Wall { get; internal set; }

        /// <summary>
        /// BulletWall indicates whether or not a bullet should collide with this tile.
        /// </summary>
        public bool BulletWall { get; internal set; }

        private BlockFaceEdge()
        {
            Wall = false;
            BulletWall = false;
        }

        public BlockFaceEdge(ushort value) : base(value)
        {
            Wall = BitHelper.CheckBit(value, 10);
            BulletWall = BitHelper.CheckBit(value, 11);
        }
    }

    internal static class BlockFaceEdgeWallFix
    {
        private class TileWallCollisionFix
        {
            public uint TileNumber;
            public int CollisionCount;
            public int NoBulletCollision;
            public int NoCollisionCount;
            public readonly List<BlockFaceEdge> BlockFaces = new List<BlockFaceEdge>(); 

            public override string ToString()
            {
                return TileNumber + " (" + CollisionCount + ":" + NoBulletCollision + ":" + NoCollisionCount + ")";
            }
        }

        private static readonly Dictionary<uint, TileWallCollisionFix> TileWallCollisionFixes = new Dictionary<uint, TileWallCollisionFix>();
        private static readonly List<uint> MismatchTiles = new List<uint>(); 

        public static void FixTileWallCollision(this BlockFaceEdge blockFace)
        {
            if (blockFace.TileNumber == 0)
                return;
            TileWallCollisionFix tileWallCollisionFix;
            var add = false;
            if (!TileWallCollisionFixes.TryGetValue(blockFace.TileNumber, out tileWallCollisionFix))
            {
                tileWallCollisionFix = new TileWallCollisionFix();
                add = true;
            }
            if (blockFace.Wall)
            {
                if (blockFace.BulletWall)
                    tileWallCollisionFix.CollisionCount++;
                else
                    tileWallCollisionFix.NoBulletCollision++;
                if (tileWallCollisionFix.NoCollisionCount > 0 && !MismatchTiles.Contains(blockFace.TileNumber))
                    MismatchTiles.Add(blockFace.TileNumber);
            }
            else
            {
                tileWallCollisionFix.NoCollisionCount++;
                if (tileWallCollisionFix.CollisionCount > 0 && !MismatchTiles.Contains(blockFace.TileNumber))
                    MismatchTiles.Add(blockFace.TileNumber);
            }
            tileWallCollisionFix.TileNumber = blockFace.TileNumber;
            tileWallCollisionFix.BlockFaces.Add(blockFace);
            if (add)
                TileWallCollisionFixes.Add(blockFace.TileNumber, tileWallCollisionFix);
        }

        private static List<TileWallCollisionFix> GetMismatchTiles()
        {
            var list = new List<TileWallCollisionFix>(MismatchTiles.Count);
            list.AddRange(MismatchTiles.Select(mismatchTile => TileWallCollisionFixes[mismatchTile]));
            TileWallCollisionFixes.Clear();
            MismatchTiles.Clear();
            return list;
        }

        public static void FixAllMismatchTiles()
        {
            var list = GetMismatchTiles();
            foreach (var tileWallCollisionFix in list)
            {
                foreach (var blockFace in tileWallCollisionFix.BlockFaces)
                {
                    blockFace.Wall = true;
                    if (tileWallCollisionFix.CollisionCount > 0)
                        blockFace.BulletWall = true;
                }
            }
        }
    }

}
