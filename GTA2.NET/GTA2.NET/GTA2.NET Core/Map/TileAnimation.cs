// GTA2.NET
// 
// File: TileAnimation.cs
// Created: 21.02.2013
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
using System.Collections.Generic;

namespace Hiale.GTA2NET.Core.Map
{
    public class TileAnimation
    {
        /// <summary>
        /// Is the base tile of the animation.
        /// </summary>
        public int BaseTile { get; set; }

        /// <summary>
        /// FrameRate is the number of game frames that each frame of the animation is displayed for.
        /// </summary>
        public int FrameRate { get; set; }

        /// <summary>
        /// Repeat is the number of times the animation will be played. 0 means play forever.
        /// </summary>
        public int Repeat { get; set; }

        /// <summary>
        /// Tiles is a collection that stores the tile numbers that make up the animation.
        /// </summary>
        public List<int> Tiles { get; set; }

        public TileAnimation()
        {
            Tiles = new List<int>();
        }
    }
}
