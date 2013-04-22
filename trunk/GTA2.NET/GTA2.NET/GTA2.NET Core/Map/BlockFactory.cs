// GTA2.NET
// 
// File: BlockFactory.cs
// Created: 22.04.2013
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
        private static readonly BlockInfo[] Blocks = { new Empty(), new Cube() };

        public static BlockInfo Build(blockInfo blockInfo, Vector3 pos)
        {
            var slopeType = 0;
            for (var i = 2; i < 8; i++)
            {
                if (BitHelper.CheckBit(blockInfo.SlopeType, i))
                    slopeType += (int)Math.Pow(2, i - 2);
            }

            foreach(var b in Blocks)
                if (b.IsMe((SlopeType)slopeType))
                    return b.DeepCopy(blockInfo, pos);

            return new Cube(blockInfo, pos);    //Hack until all the diferent blocks are implemented.
            //throw new NotSupportedException(); <- Must be this.
        }
    }
}
