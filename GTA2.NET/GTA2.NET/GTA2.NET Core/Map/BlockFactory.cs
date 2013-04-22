using System;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Core.Map.Blocks;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Map
{

    public static class BlockFactory
    {
        private static BlockInfo[] blocks = { new Empty(), new Cube(), new Up26Low(), new Up26High(), new Down26Low(), new Down26High(), new Left26Low(),
                                              new Left26High(), new Right26Low(), new Right26High(), new Up7Low(), new Up7High0(), new Up7High1(), new Up7High2(), 
                                              new Up7High3(), new Up7High4(), new Up7High5(), new Up7High6(), new Down7Low(), new Down7High0(), new Down7High1(), 
                                              new Down7High2(), new Down7High3(), new Down7High4(), new Down7High5(), new Down7High6(), new Left7Low(),
                                              new Left7High0(), new Left7High1(), new Left7High2(), new Left7High3(), new Left7High4(), new Left7High5(),
                                              new Left7High6(), new Right7Low(), new Right7High0(), new Right7High1(), new Right7High2(), new Right7High3(), 
                                              new Right7High4(), new Right7High5(), new Right7High6(), new Up45(), new Down45(), new Left45(), new Right45(),
                                              new DiagonalFacingUpLeft(), new DiagonalFacingUpRight(), new DiagonalFacingDownLeft(), new DiagonalFacingDownRight()
                                            };

        public static BlockInfo Build(blockInfo blockInfo, Vector3 pos)
        {
            int slopeType = 0;
            for (int i = 2; i < 8; i++)
            {
                if (BitHelper.CheckBit(blockInfo.SlopeType, i))
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
