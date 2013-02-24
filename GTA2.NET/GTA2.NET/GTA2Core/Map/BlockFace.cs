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

    public class BlockFace
    {
        /// <summary>
        /// IsCeiling indicates if this face is a ceiling (true) or left, right, top, bottom. Several properties cannot be used in either selection.
        /// </summary>
        public bool IsCeiling { get; private set; }

        /// <summary>
        /// Tile graphic number simply indicates which of the possible 1024 tile graphics to draw on this surface. It serves as an index into the tile information in the style file. A value of 0 means leave it blank. 992-1022 are reserved for internal use by the game engine. 1023 is used as a dummy tile number to mark 3-sided diagonal slopes.
        /// </summary>
        public int TileNumber { get; private set; }

        private readonly bool _wall;
        /// <summary>
        /// Wall indicates whether or not a car, ped or object should collide with this tile.
        /// </summary>
        public bool Wall
        {
            get
            {
                if (IsCeiling)
                    throw new NotSupportedException("A ceiling cannot have a Wall property! Use LightningLevel instead.");
                return _wall;
            }
        }

        private readonly bool _bulletWall;
        /// <summary>
        /// BulletWall indicates whether or not a bullet should collide with this tile.
        /// </summary>
        public bool BulletWall
        {
            get
            {
                if (IsCeiling)
                    throw new NotSupportedException("A ceiling cannot have a ButtetWall property! Use LightningLevel instead.");
                return _bulletWall;
            }
        }

        private readonly byte _lightningLevel;
        /// <summary>
        /// Lighting level marks which shading level to apply to a lid tile. 0 is normal brightness. 1-3 are increasing levels of darkness.
        /// </summary>
        public byte LightningLevel
        {
            get 
            {
                if (!IsCeiling)
                    throw new NotSupportedException("A left, right, top & bottom face cannot have a LightningLevel property! Use Wall and BulletWall instead.");
                return _lightningLevel;
            }      
        }

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

        /// <summary>
        /// Represents a face (left, right, top, bottom, lid) of a block.
        /// </summary>
        /// <param name="value">Base ushort value of this face.</param>
        /// <param name="ceiling">Whether this face is a ceiling face.</param>
        public BlockFace(ushort value, bool ceiling)
        {
            IsCeiling = ceiling;

            if (value == 0)
                return;

            //parse ushort value
            //Bits 0-9: Tile number
            int tile = 0;
            for (int i = 0; i < 10; i++)
            {
                tile = tile + (value & (int)Math.Pow(2, i));
            }
            TileNumber = tile;

            if (IsCeiling)
            {
                _wall = BitHelper.CheckBit(value, 10); //Bit 10
                _bulletWall = BitHelper.CheckBit(value, 11); //Bit 11
            }
            else
            {
                bool bit10 = BitHelper.CheckBit(value, 10);
                bool bit11 = BitHelper.CheckBit(value, 11);
                if (!bit10 && !bit11)
                    _lightningLevel = 0;
                if (bit10 && !bit11)
                    _lightningLevel = 1;
                if (bit10 && bit11)
                    _lightningLevel = 2;
                if (bit10 && bit11)
                    _lightningLevel = 3;
            }

            Flat = BitHelper.CheckBit(value, 12); //Bit 12
            Flip = BitHelper.CheckBit(value, 13); //Bit 13

            bool bit14 = BitHelper.CheckBit(value, 14);
            bool bit15 = BitHelper.CheckBit(value, 15);
            if (!bit14 && !bit15)
                Rotation = RotationType.RotateNone;
            if (bit14 && !bit15)
                Rotation = RotationType.Rotate90;
            if (!bit14 && bit15)
                Rotation = RotationType.Rotate180;
            if (bit14 && bit15)
                Rotation = RotationType.Rotate270;
        }

        public BlockFace(int tileNumber, RotationType rotation) //remove, just for debugging purpose
        {
            TileNumber = tileNumber;
            Rotation = rotation;
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
