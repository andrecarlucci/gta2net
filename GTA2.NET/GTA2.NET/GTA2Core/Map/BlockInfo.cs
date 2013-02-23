//Created: 17.01.2010

using System;
using Microsoft.Xna.Framework;
using Hiale.GTA2NET.Core.Helper;

namespace Hiale.GTA2NET.Core.Map
{
    public class BlockInfo
    {
        /// <summary>
        /// Position of this block in a map.
        /// </summary>
        public Vector3 Position { get; set; } //remove?

        public BlockFace Left { get; set; }

        public BlockFace Right { get; set; }

        public BlockFace Top { get; set; }

        public BlockFace Bottom { get; set; }

        public BlockFace Lid { get; set; }

        /// <summary>
        /// ToDo
        /// </summary>
        public RoadTrafficType Arrows { get; set; }


        /// <summary>
        /// ToDo - enum
        /// </summary>
        public byte BaseSlopeType { get; private set; }

        public GroundType GroundType { get; private set; }

        public SlopeType SlopeType { get; private set; }

        public void ParseSlope(byte type)
        {
            BaseSlopeType = type;

            if (type == 0)
                return;

            int groundType = 0;
            groundType += (type & 1);
            groundType += (type & 2);
            GroundType = (GroundType)groundType;

            if (type < 4)
                return;

            int slopeType = 0;
            for (int i = 2; i < 8; i++)
            {
                if (BitHelper.CheckBit(type, i))
                    slopeType += (int)Math.Pow(2, i - 2);
            }
            SlopeType = (SlopeType)slopeType;
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

        //public bool IsDiagonalSlope
        //{
        //    get
        //    {
        //        return SlopeType == SlopeType.DiagonalFacingDownLeft || SlopeType == SlopeType.DiagonalFacingDownRight || SlopeType == SlopeType.DiagonalFacingUpLeft || SlopeType == SlopeType.DiagonalFacingUpRight;
        //    }
        //}

        //public bool IsLowSlope
        //{
        //    get
        //    {
        //        return SlopeType == SlopeType.Up26Low || SlopeType == SlopeType.Down26Low || SlopeType == SlopeType.Left26Low || SlopeType == SlopeType.Right26Low;
        //    }
        //}

        //public bool IsHighSlope
        //{
        //    get
        //    {
        //        return SlopeType == SlopeType.Up26High || SlopeType == SlopeType.Down26High || SlopeType == SlopeType.Left26High || SlopeType == SlopeType.Right26High;
        //    }
        //}

        public bool IsMovableSlope
        {
            get
            {
                byte slope = (byte)SlopeType;
                return slope != 0 && slope < 41;
            }
        }

        public SlopeDirection SlopeDirection
        {
            get
            {
                switch (SlopeType)
                {
                    case SlopeType.Up26Low:
                    case SlopeType.Up26High:
                    case SlopeType.Up7Low:
                    case SlopeType.Up7High0:
                    case SlopeType.Up7High1:
                    case SlopeType.Up7High2:
                    case SlopeType.Up7High3:
                    case SlopeType.Up7High4:
                    case SlopeType.Up7High5:
                    case SlopeType.Up7High6:
                    case SlopeType.Up45:
                        return SlopeDirection.Up;
                    case SlopeType.Down26Low:
                    case SlopeType.Down26High:
                    case SlopeType.Down7Low:
                    case SlopeType.Down7High0:
                    case SlopeType.Down7High1:
                    case SlopeType.Down7High2:
                    case SlopeType.Down7High3:
                    case SlopeType.Down7High4:
                    case SlopeType.Down7High5:
                    case SlopeType.Down7High6:
                    case SlopeType.Down45:
                        return SlopeDirection.Down;
                    case SlopeType.Left26Low:
                    case SlopeType.Left26High:
                    case SlopeType.Left7Low:
                    case SlopeType.Left7High0:
                    case SlopeType.Left7High1:
                    case SlopeType.Left7High2:
                    case SlopeType.Left7High3:
                    case SlopeType.Left7High4:
                    case SlopeType.Left7High5:
                    case SlopeType.Left7High6:
                    case SlopeType.Left45:
                        return SlopeDirection.Left;
                    case SlopeType.Right26Low:
                    case SlopeType.Right26High:
                    case SlopeType.Right7Low:
                    case SlopeType.Right7High0:
                    case SlopeType.Right7High1:
                    case SlopeType.Right7High2:
                    case SlopeType.Right7High3:
                    case SlopeType.Right7High4:
                    case SlopeType.Right7High5:
                    case SlopeType.Right7High6:
                    case SlopeType.Right45:
                        return SlopeDirection.Right;
                }
                return SlopeDirection.None;
            }
        }

        public bool IsDiagonalSlope
        {
            get
            {
                byte slope = (byte)SlopeType;
                return slope >= 45 && slope <= 48;
            }
        }

        public byte SlopeSubTyp
        {
            get
            {
                byte slope = (byte)SlopeType;
                if (slope > 0 && slope < 9)
                    return 26;
                if (slope > 8 && slope < 41)
                    return 7;
                if (slope > 40 && slope < 45)
                    return 45;
                return 0;
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
