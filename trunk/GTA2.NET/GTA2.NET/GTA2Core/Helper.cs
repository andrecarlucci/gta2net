using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiale.GTA2NET.Core
{
    public static class Helper
    {
        public static bool CheckBit(int value, int BitOffset)
        {
            if (BitOffset > 31)
                throw new NotSupportedException();
            int BitValue = (int)Math.Pow(2, BitOffset);
            return (value & BitValue) == BitValue;
        }
    }
}
