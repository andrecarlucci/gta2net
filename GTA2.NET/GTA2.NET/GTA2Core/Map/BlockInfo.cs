//Created: 17.01.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiale.GTA2.Core.Map;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Map
{
    public class BlockInfo
    {
        private Vector3 position;
        /// <summary>
        /// Position of this block in a map.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }


        private BlockFace _Left;
        public BlockFace Left
        {
            get { return _Left; }
            set { _Left = value; }
        }

        private BlockFace _Right;
        public BlockFace Right
        {
            get { return _Right; }
            set { _Right = value; }
        }

        private BlockFace _Top;
        public BlockFace Top
        {
            get { return _Top; }
            set { _Top = value; }
        }

        private BlockFace _Bottom;
        public BlockFace Bottom
        {
            get { return _Bottom; }
            set { _Bottom = value; }
        }

        private BlockFace _Lid;
        public BlockFace Lid
        {
            get { return _Lid; }
            set { _Lid = value; }
        }

        private RoadTrafficType _Arrows;
        /// <summary>
        /// ToDo
        /// </summary>
        public RoadTrafficType Arrows
        {
            get { return _Arrows; }
            set { _Arrows = value; }
        }

        //private byte _Arrows;
        ///// <summary>
        ///// ToDo - enum
        ///// </summary>
        //public byte Arrows
        //{
        //    get { return _Arrows; }
        //    set { _Arrows = value; }
        //}
        

        private byte _BaseSlopeType;
        /// <summary>
        /// ToDo - enum
        /// </summary>
        public byte BaseSlopeType
        {
            get { return _BaseSlopeType; }
        }

        private GroundType _GroundType;
        public GroundType GroundType
        {
            get { return _GroundType; }
        }

        private SlopeType _SlopeType;
        public SlopeType SlopeType
        {
            get { return _SlopeType; }
        }

        public void ParseSlope(byte SlopeType)
        {
            _BaseSlopeType = SlopeType;

            if (SlopeType == 0)
                return;

            int groundType = 0;
            groundType += (SlopeType & 1);
            groundType += (SlopeType & 2);
            _GroundType = (GroundType)groundType;

            if (SlopeType < 4)
                return;

            int slopeType = 0;
            for (int i = 2; i < 8; i++)
            {
                if (Helper.CheckBit(SlopeType, i))
                    slopeType += (int)Math.Pow(2, i - 2);
            }
            _SlopeType = (SlopeType)slopeType;
        }

        public bool IsEmpty
        {
            get
            {
                if ((Left == null && Right == null && Top == null && Bottom == null && Lid == null) || ((Left.TileNumber == 0 && Right.TileNumber == 0 && Top.TileNumber == 0 && Bottom.TileNumber == 0 && Lid.TileNumber == 0)))
                    return true;
                return false;
            }
        }

        public bool IsLidOnly
        {
            get
            {
                if (IsEmpty)
                    return false;
                if (Lid != null && Lid.TileNumber > 0 && (Left == null || Left.TileNumber == 0) && (Right == null || Right.TileNumber == 0) && (Top == null || Top.TileNumber == 0) && (Bottom == null || Bottom.TileNumber == 0))
                    return true;
                return false;
            }
        }

        public bool IsDiagonalSlope
        {
            get
            {
                return SlopeType == SlopeType.DiagonalFacingDownLeft || SlopeType == SlopeType.DiagonalFacingDownRight || SlopeType == SlopeType.DiagonalFacingUpLeft || SlopeType == SlopeType.DiagonalFacingUpRight;
            }
        }

        public bool IsLowSlope
        {
            get
            {
                return SlopeType == SlopeType.Up26Low || SlopeType == SlopeType.Down26Low || SlopeType == SlopeType.Left26Low || SlopeType == SlopeType.Right26Low;
            }
        }

        public bool IsHighSlope
        {
            get
            {
                return SlopeType == SlopeType.Up26High || SlopeType == SlopeType.Down26High || SlopeType == SlopeType.Left26High || SlopeType == SlopeType.Right26High;
            }
        }

        //public bool IsAir()
        //{
        //    //int Bit0 = SlopeType & 1;
        //    //int Bit1 = SlopeType & 2;
        //    return ((SlopeType & 1) != 1) && ((SlopeType & 2) != 2); //Bit 0 and Bit 1 are both 0
        //}

        //public bool IsRoad()
        //{
        //    return ((SlopeType & 1) == 1) && ((SlopeType & 2) != 2); //Bit 0 is 1 and Bit 1 is 0
        //}

        //public bool IsPavement()
        //{
        //    return ((SlopeType & 1) != 1) && ((SlopeType & 2) == 2); //Bit 0 is 0 and Bit 1 is 1
        //}

        //public bool IsField()
        //{
        //    return ((SlopeType & 1) == 1) && ((SlopeType & 2) == 2); //Bit 0 and Bit 1 are both 1
        //}

        public BlockInfo()
        {

        }

        public override string ToString()
        {
            if (IsEmpty)
                return "[empty block]";
            else
            {
                return "Lid: " + Lid + " Left: " + Left + " Top: " + Top + " Right: " + Right + " Bottom: " + Bottom + " Ground: " + GroundType.ToString();
            }
        }
    }
}
