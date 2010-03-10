//Created: 26.01.2010

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
