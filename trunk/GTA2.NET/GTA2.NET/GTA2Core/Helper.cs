using System;

namespace Hiale.GTA2NET.Core
{
    public static class Helper
    {
        public static bool CheckBit(int value, int bitOffset)
        {
            if (bitOffset > 31)
                throw new NotSupportedException();
            int bitValue = (int)Math.Pow(2, bitOffset);
            return (value & bitValue) == bitValue;
        }
    }
}
