//Created: 27.02.2013

using System.IO;
using Hiale.GTA2NET.Core.Helper;

namespace Hiale.GTA2NET.Core.Map
{
    public class BlockFaceLid : BlockFace
    {
        public static BlockFaceLid Empty = new BlockFaceLid();

        /// <summary>
        /// Lighting level marks which shading level to apply to a lid tile. 0 is normal brightness. 1-3 are increasing levels of darkness.
        /// </summary>
        public byte LightningLevel { get; private set; }

        private BlockFaceLid()
        {
            LightningLevel = 0;
        }

        public BlockFaceLid(ushort value) : base(value)
        {
            var bit10 = BitHelper.CheckBit(value, 10);
            var bit11 = BitHelper.CheckBit(value, 11);
            if (!bit10 && !bit11)
                LightningLevel = 0;
            if (bit10 && !bit11)
                LightningLevel = 1;
            if (bit10 && bit11)
                LightningLevel = 2;
            if (bit10 && bit11)
                LightningLevel = 3;
        }

        public static BlockFaceLid Load(BinaryReader reader)
        {
            var blockFace = new BlockFaceLid();
            BaseLoad(blockFace, reader);
            blockFace.LightningLevel = reader.ReadByte();
            return blockFace;
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write(LightningLevel);
        }
    }
}
