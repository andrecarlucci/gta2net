//Created: 29.01.2010

using System;

namespace Hiale.GTA2NET.Core.Helper
{
    public static class BitHelper
    {
        public static bool CheckBit(int value, int bitOffset)
        {
            if (bitOffset < 0 || bitOffset >= sizeof(int) * 8)
                throw new ArgumentOutOfRangeException();
            //var bitValue = (int)Math.Pow(2, bitOffset);
            //return (value & bitValue) == bitValue;
            return (value & (1 << bitOffset)) == (1 << bitOffset);
        }

        public static ushort SetBit(ushort value, byte bitOffset, bool bitValue)
        {
            if (bitOffset >= sizeof(ushort) * 8)
                throw new ArgumentOutOfRangeException();
            if (bitValue)
                return (ushort) (value | (1 << bitOffset)); ////turn Bit on
            return (ushort)(value & ~(1 << bitOffset)); //turn Bit off
        }

    }
}
