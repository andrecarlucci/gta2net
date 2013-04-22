using System;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Core.Map.Blocks;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Map
{

    public static class BlockFactory
    {
        private static BlockInfo[] blocks = { new Empty(), new Cube() };

        public static BlockInfo Build(blockInfo blockInfo, Vector3 pos)
        {
            int slopeType = 0;
            for (int i = 2; i < 8; i++)
            {
                if (BitHelper.CheckBit(blockInfo.slope_type, i))
                    slopeType += (int)Math.Pow(2, i - 2);
            }

            foreach(BlockInfo b in blocks)
                if (b.IsMe((SlopeType)slopeType))
                    return b.DeepCopy(blockInfo, pos);

            return new Cube(blockInfo, pos);    //Hack until all the diferent blocks are implemented.
            //throw new NotSupportedException(); <- Must be this.
        }
    }
}
