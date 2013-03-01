//Created: 29.01.2010

using System;

namespace Hiale.GTA2NET.Core.Helper
{
    public static class BitHelper
    {
        public static bool CheckBit(int value, int bitOffset)
        {
            if (bitOffset > 31)
                throw new NotSupportedException();
            var bitValue = (int)Math.Pow(2, bitOffset);
            return (value & bitValue) == bitValue;
        }
    }
}
