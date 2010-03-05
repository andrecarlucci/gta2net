using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiale.GTA2NET.Core.Map
{
    class CompressedMap
    {
        UInt32[,] basee;
        UInt32 ColumnWords;
        UInt32[] Columns;
        UInt32 NumBlocks;
        BlockInfo[] Blocks;

        public CompressedMap()
        {

        }

    }
}
