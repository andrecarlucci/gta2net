//Created: 27.02.2013

using System.IO;
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

        public static BlockFaceEdge Load(BinaryReader reader)
        {
            var blockFace = new BlockFaceEdge();
            BaseLoad(blockFace, reader);
            blockFace.Wall = reader.ReadBoolean();
            blockFace.BulletWall = reader.ReadBoolean();
            return blockFace;
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write(Wall);
            writer.Write(BulletWall);
        }

    }
}
