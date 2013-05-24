// GTA2.NET
// 
// File: BlockFactory.cs
// Created: 22.04.2013
// Created by: João Pires
// 
// 
// Copyright (C) 2010-2013 Hiale
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR
// IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Grand Theft Auto (GTA) is a registred trademark of Rockstar Games.
using System;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Core.Map.Blocks;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Map
{
    public static class BlockFactory
    {
        private static Block[] _blocks;

        static BlockFactory()
        {
            InitializeBlocks();
        }

        //this way, the blocks can be changed not to Load directly at the start
        private static void InitializeBlocks()
        {
            _blocks = new Block[]
                {
                    new CubeBlock(), new Up26LowBlock(), new Up26HighBlock(), new Down26LowBlock(), new Down26HighBlock(), new Left26LowBlock(),
                    new Left26HighBlock(), new Right26LowBlock(), new Right26HighBlock(), new Up7LowBlock(), new Up7High0Block(), new Up7High1Block(), new Up7High2Block(),
                    new Up7High3Block(), new Up7High4Block(), new Up7High5Block(), new Up7High6Block(), new Down7LowBlock(), new Down7High0Block(), new Down7High1Block(),
                    new Down7High2Block(), new Down7High3Block(), new Down7High4Block(), new Down7High5Block(), new Down7High6Block(), new Left7LowBlock(),
                    new Left7High0Block(), new Left7High1Block(), new Left7High2Block(), new Left7High3Block(), new Left7High4Block(), new Left7High5Block(),
                    new Left7High6Block(), new Right7LowBlock(), new Right7High0Block(), new Right7High1Block(), new Right7High2Block(), new Right7High3Block(),
                    new Right7High4Block(), new Right7High5Block(), new Right7High6Block(), new Up45Block(), new Down45Block(), new Left45Block(), new Right45Block(),
                    new DiagonalFacingUpLeftBlock(), new DiagonalFacingUpRightBlock(), new DiagonalFacingDownLeftBlock(), new DiagonalFacingDownRightBlock(),
                    new DiagonalSlopeFacingDownLeftBlock(), new DiagonalSlopeFacingDownRightBlock(), new DiagonalSlopeFacingUpLeftBlock(), new DiagonalSlopeFacingUpRightBlock(),    
                    new PartialBottomBlock(), new PartialBottomLeftBlock(), new PartialBottomRightBlock(), new PartialCenterBlock(), new PartialLeftBlock(),
                    new PartialRightBlock(), new PartialTopBlock(), new PartialTopLeftBlock(), new PartialTopRightBlock(), new CubeBlock(SlopeType.SlopeAbove), 
                };
        }

        public static Block Build(BlockStructure blockStructure, Vector3 position)
        {
            var slopeType = 0;
            for (var i = 2; i < 8; i++)
            {
                if (BitHelper.CheckBit(blockStructure.SlopeType, i))
                    slopeType += (int)Math.Pow(2, i - 2);
            }

            foreach(var block in _blocks)
                if (block.IsSlopeOf((SlopeType)slopeType))
                    return block.DeepCopy(blockStructure, position);

            //return new CubeBlock(blockStructure, position);    //Hack until all the different _blocks are implemented.
            throw new NotSupportedException("Slope Type " + slopeType + " not implemented!"); // <- Must be this.
        }
    }
}
