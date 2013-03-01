//Created: 25.01.2010

using System;
using Hiale.GTA2NET.Core.Helper;

namespace Hiale.GTA2NET.Core.Map
{
    public enum RotationType
    {
        RotateNone,
        Rotate90,
        Rotate180,
        Rotate270
    }

    public enum BlockFaceDirection
    {
        Left,
        Right,
        Top,
        Bottom,
        Lid
    }

    public abstract class BlockFace
    {
        /// <summary>
        /// Tile graphic number simply indicates which of the possible 1024 tile graphics to draw on this surface. It serves as an index into the tile information in the style file. A value of 0 means leave it blank. 992-1022 are reserved for internal use by the game engine. 1023 is used as a dummy tile number to mark 3-sided diagonal slopes.
        /// </summary>
        public int TileNumber { get; private set; }

        /// <summary>
        /// Flat indicates whether or not this tile should be treated as a flat. This means that it gets drawn transparently, and (except for a lid ) the tile opposite is used as the graphic for the reverse side.
        /// If both matching sides of a block (i.e. top and bottom or left and right) are flat, then both tiles are drawn at both positions.
        /// </summary>
        public bool Flat { get; private set; }

        /// <summary>
        /// Flip indicates whether or not this tile is to be drawn flipped left to right.
        /// </summary>
        public bool Flip { get; private set; }

        /// <summary>
        /// Rotation describes a rotation to turn this tile by when drawing it
        /// </summary>
        public RotationType Rotation { get; private set; }

        protected BlockFace()
        {
            TileNumber = 0;
            Flat = false;
            Flip = false;
            Rotation = RotationType.RotateNone;
        }

        /// <summary>
        /// Represents a face (left, right, top, bottom, lid) of a block.
        /// </summary>
        /// <param name="value">Base ushort value of this face.</param>
        protected BlockFace(ushort value)
        {
            if (value == 0)
                return;

            //parse ushort value
            //Bits 0-9: Tile number
            var tile = 0;
            for (var i = 0; i < 10; i++)
                tile = tile + (value & (int)Math.Pow(2, i));
            TileNumber = tile;

            Flat = BitHelper.CheckBit(value, 12); //Bit 12
            Flip = BitHelper.CheckBit(value, 13); //Bit 13

            var bit14 = BitHelper.CheckBit(value, 14);
            var bit15 = BitHelper.CheckBit(value, 15);
            if (!bit14 && !bit15)
                Rotation = RotationType.RotateNone;
            if (bit14 && !bit15)
                Rotation = RotationType.Rotate90;
            if (!bit14 && bit15)
                Rotation = RotationType.Rotate180;
            if (bit14 && bit15)
                Rotation = RotationType.Rotate270;
        }

        public static implicit operator bool(BlockFace blockface)
        {
            return blockface.TileNumber > 0;
        }

        public override string ToString()
        {
            string rotation = string.Empty;
            switch (Rotation)
            {
                case RotationType.RotateNone:
                    rotation = " (0";
                    break;
                case RotationType.Rotate90:
                    rotation = " (90";
                    break;
                case RotationType.Rotate180:
                    rotation = " (180";
                    break;
                case RotationType.Rotate270:
                    rotation = " (270";
                    break;
            }
            if (Flip)
                rotation += " flip)";
            else
                rotation += ")";
            return TileNumber + rotation;
        }


    }
}
