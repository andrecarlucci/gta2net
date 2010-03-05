//Created: 26.01.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiale.GTA2NET.Core.Map
{
    public class TileAnimation
    {
        private int _BaseTile;
        /// <summary>
        /// Is the base tile of the animation.
        /// </summary>
        public int BaseTile
        {
            get { return _BaseTile; }
            set { _BaseTile = value; }
        }

        private int _FrameRate;
        /// <summary>
        /// FrameRate is the number of game frames that each frame of the animation is displayed for.
        /// </summary>
        public int FrameRate
        {
            get { return _FrameRate; }
            set { _FrameRate = value; }
        }

        private int _Repeat;
        /// <summary>
        /// Repeat is the number of times the animation will be played. 0 means play forever.
        /// </summary>
        public int Repeat
        {
            get { return _Repeat; }
            set { _Repeat = value; }
        }


        private List<int> _Tiles;
        /// <summary>
        /// Tiles is a collection that stores the tile numbers that make up the animation.
        /// </summary>
        public List<int> Tiles
        {
            get { return _Tiles; }
            set { _Tiles = value; }
        }

        public TileAnimation()
        {
            Tiles = new List<int>();
        }
    }
}
