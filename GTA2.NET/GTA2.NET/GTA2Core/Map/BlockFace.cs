//Created: 25.01.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        private ushort baseValue;

        private bool _IsCeiling;
        /// <summary>
        /// IsCeiling indicates if this face is a ceiling (true) or left, right, top, bottom. Several properties cannot be used in either selection.
        /// </summary>
        public bool IsCeiling
        {
            get { return _IsCeiling; }
        }

        private int _TileNumber;
        /// <summary>
        /// Tile graphic number simply indicates which of the possible 1024 tile graphics to draw on this surface. It serves as an index into the tile information in the style file. A value of 0 means leave it blank. 992-1022 are reserved for internal use by the game engine. 1023 is used as a dummy tile number to mark 3-sided diagonal slopes.
        /// </summary>
        public int TileNumber
        {
            get { return _TileNumber; }
        }

        private bool _Wall;
        /// <summary>
        /// Wall indicates whether or not a car, ped or object should collide with this tile.
        /// </summary>
        public bool Wall
        {
            get
            {
                if (IsCeiling)
                    throw new NotSupportedException("A ceiling cannot have a Wall property! Use LightningLevel instead.");
                return _Wall;
            }
        }

        private bool _BulletWall;
        /// <summary>
        /// BulletWall indicates whether or not a bullet should collide with this tile.
        /// </summary>
        public bool BulletWall
        {
            get
            {
                if (IsCeiling)
                    throw new NotSupportedException("A ceiling cannot have a ButtetWall property! Use LightningLevel instead.");
                return _BulletWall;
            }
        }

        private byte _LightningLevel;
        /// <summary>
        /// Lighting level marks which shading level to apply to a lid tile. 0 is normal brightness. 1-3 are increasing levels of darkness.
        /// </summary>
        public byte LightningLevel
        {
            get 
            {
                if (!IsCeiling)
                    throw new NotSupportedException("A left, right, top & bottom face cannot have a LightningLevel property! Use Wall and BulletWall instead.");
                return _LightningLevel;
            }      
        }

        private bool _Flat;
        /// <summary>
        /// Flat indicates whether or not this tile should be treated as a flat. This means that it gets drawn transparently, and (except for a lid ) the tile opposite is used as the graphic for the reverse side.
        /// If both matching sides of a block (i.e. top and bottom or left and right) are flat, then both tiles are drawn at both positions.
        /// </summary>
        public bool Flat
        {
            get { return _Flat; }
        }

        private bool _Flip;
        /// <summary>
        /// Flip indicates whether or not this tile is to be drawn flipped left to right.
        /// </summary>
        public bool Flip
        {
            get { return _Flip; }
        }

        private RotationType _Rotation;
        /// <summary>
        /// Rotation describes a rotation to turn this tile by when drawing it
        /// </summary>
        public RotationType Rotation
        {
            get { return _Rotation; }
        }

        /// <summary>
        /// Represents a face (left, right, top, bottom, lid) of a block.
        /// </summary>
        /// <param name="value">Base ushort value of this face.</param>
        /// <param name="ceiling">Whether this face is a ceiling face.</param>
        public BlockFace(ushort value, bool ceiling)
        {
            baseValue = value;
            _IsCeiling = ceiling;

            if (value == 0)
                return;

            //parse ushort value
            //Bits 0-9: Tile number
            int tile = 0;
            for (int i = 0; i < 10; i++)
            {
                tile = tile + (value & (int)Math.Pow(2, i));
            }
            _TileNumber = tile;

            if (IsCeiling)
            {
                _Wall = Helper.CheckBit(value, 10); //Bit 10
                _BulletWall = Helper.CheckBit(value, 11); //Bit 11
            }
            else
            {
                bool bit10 = Helper.CheckBit(value, 10);
                bool bit11 = Helper.CheckBit(value, 11);
                if (!bit10 && !bit11)
                    _LightningLevel = 0;
                if (bit10 && !bit11)
                    _LightningLevel = 1;
                if (bit10 && bit11)
                    _LightningLevel = 2;
                if (bit10 && bit11)
                    _LightningLevel = 3;
            }

            _Flat = Helper.CheckBit(value, 12); //Bit 12
            _Flip = Helper.CheckBit(value, 13); //Bit 13

            bool bit14 = Helper.CheckBit(value, 14);
            bool bit15 = Helper.CheckBit(value, 15);
            if (!bit14 && !bit15)
                _Rotation = RotationType.RotateNone;
            if (bit14 && !bit15)
                _Rotation = RotationType.Rotate90;
            if (!bit14 && bit15)
                _Rotation = RotationType.Rotate180;
            if (bit14 && bit15)
                _Rotation = RotationType.Rotate270;
        }

        public BlockFace(int tileNumber, RotationType rotation) //remove, just for debugging purpose
        {
            _TileNumber = tileNumber;
            _Rotation = rotation;
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
            return TileNumber.ToString() + rotation;
        }


    }
}
