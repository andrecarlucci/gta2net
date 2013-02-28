//Created: 27.02.2013

using Hiale.GTA2NET.Core.Helper;

namespace Hiale.GTA2NET.Core.Map
{
    public class BlockFaceEdge : BlockFace
    {
        public static BlockFaceEdge Empty = new BlockFaceEdge();

        /// <summary>
        /// Wall indicates whether or not a car, ped or object should collide with this tile.
        /// </summary>
        public bool Wall { get; internal set; }

        /// <summary>
        /// BulletWall indicates whether or not a bullet should collide with this tile.
        /// </summary>
        public bool BulletWall { get; internal set; }

        private BlockFaceEdge()
        {
            Wall = false;
            BulletWall = false;
        }

        public BlockFaceEdge(ushort value) : base(value)
        {
            Wall = BitHelper.CheckBit(value, 10);
            BulletWall = BitHelper.CheckBit(value, 11);
        }
    }
}
