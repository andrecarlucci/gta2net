using System.IO;
using Hiale.GTA2NET.Core.Map;

//using Hiale.GTA2NET.Core.Map;

namespace Hiale.GTA2NET.Core.Helper
{
    public static class LoadSave
    {
        //Map load
        public static void Load(this Map.Map map, string filename)
        {
            var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(stream);
            reader.ReadString(); //GTA2.NET
            reader.ReadString(); //Version
            var count = reader.ReadInt32();
            var i = 0;
            var blocks = new BlockInfo[Map.Map.MaxWidth , Map.Map.MaxLength, Map.Map.MaxHeight];
            while (i < count)
            {
                var x = reader.ReadInt32();
                var y = reader.ReadInt32();
                var z = reader.ReadInt32();
                blocks[x, y, z] = new BlockInfo();
                blocks[x, y, z].Load(reader);
                i++;
            }

            for (var z = 0; z < blocks.GetLength(2); z++)
            {
                for (var x = 0; x < blocks.GetLength(0); x++)
                {
                    for (var y = 0; y < blocks.GetLength(1); y++)
                    {
                        if (blocks[x, y, z] == null)
                            blocks[x, y, z] = BlockInfo.Empty;
                    }
                }
            }
            map.CityBlocks = blocks;
        }

        //Map save
        public static void Save(this Map.Map map, string filename)
        {
            var stream = new FileStream(filename, FileMode.Create);
            var writer = new BinaryWriter(stream);
            writer.Write("GTA2.NET");
            writer.Write("0.1"); //we only save blocks at the moment
            var count = 0;
            for (var z = 0; z < map.CityBlocks.GetLength(2); z++)
            {
                for (var x = 0; x < map.CityBlocks.GetLength(0); x++)
                {
                    for (var y = 0; y < map.CityBlocks.GetLength(1); y++)
                    {
                        var block = map.CityBlocks[x, y, z];
                        if (block.IsEmpty)
                            continue;
                        count++;
                    }
                }
            }
            writer.Write(count); //BlocksCount
            for (var z = 0; z < map.CityBlocks.GetLength(2); z++)
            {
                for (var x = 0; x < map.CityBlocks.GetLength(0); x++)
                {
                    for (var y = 0; y < map.CityBlocks.GetLength(1); y++)
                    {
                        var block = map.CityBlocks[x, y, z];
                        if (block.IsEmpty)
                            continue;
                        writer.Write(x);
                        writer.Write(y);
                        writer.Write(z);
                        block.Save(writer);
                    }
                }
            }
        }


        //Block save
        public static void Save(this BlockInfo block, BinaryWriter writer)
        {
            block.Left.Save(writer);
            block.Right.Save(writer);
            block.Top.Save(writer);
            block.Bottom.Save(writer);
            block.Lid.Save(writer);
            writer.Write((byte)block.Arrows);
            writer.Write((byte)block.GroundType);
            writer.Write((byte)block.SlopeType);
        }

        //Block load
        public static void Load(this BlockInfo block, BinaryReader reader)
        {
            block.Left = new BlockFaceEdge(reader.ReadUInt16());
            block.Right = new BlockFaceEdge(reader.ReadUInt16());
            block.Top = new BlockFaceEdge(reader.ReadUInt16());
            block.Bottom = new BlockFaceEdge(reader.ReadUInt16());
            block.Lid = new BlockFaceLid(reader.ReadUInt16());
            block.Arrows = (RoadTrafficType)reader.ReadByte();
            //block.GroundType = (GroundType)reader.ReadByte();
            //block.SlopeType = (SlopeType)reader.ReadByte();
        }

        public static void Save(this BlockFaceEdge face, BinaryWriter writer)
        {
            var value = face.SaveBase();
            value = BitHelper.SetBit(value, 10, face.Wall);
            value = BitHelper.SetBit(value, 11, face.BulletWall);
            writer.Write(value);
        }

        public static void Save(this BlockFaceLid face, BinaryWriter writer)
        {
            var value = face.SaveBase();
            switch (face.LightningLevel)
            {
                case 0:
                    value = BitHelper.SetBit(value, 10, false);
                    value = BitHelper.SetBit(value, 11, false);
                    break;
                case 1:
                    value = BitHelper.SetBit(value, 10, true);
                    value = BitHelper.SetBit(value, 11, false);
                    break;
                case 2:
                    value = BitHelper.SetBit(value, 10, false);
                    value = BitHelper.SetBit(value, 11, true);
                    break;
                case 3:
                    value = BitHelper.SetBit(value, 10, true);
                    value = BitHelper.SetBit(value, 11, true);
                    break;
            }
            writer.Write(value);
        }

        public static ushort SaveBase(this BlockFace blockFace)
        {
            ushort value = 0;

            //Tile number, Bits 0-9
            const int bitCount = 10;
            for (var i = 0; i < bitCount; i++)
            {
                if ((blockFace.TileNumber & (1 << i)) != 0)
                    value = (ushort) (value | (1 << (bitCount - i - 1))); //set Bit ON
                else
                    value = (ushort) (value & ~(1 << (bitCount - i - 1))); //set Bit OFF
            }

            value = BitHelper.SetBit(value, 12, blockFace.Flat);
            value = BitHelper.SetBit(value, 13, blockFace.Flip);

            switch (blockFace.Rotation)
            {
                case RotationType.RotateNone:
                    value = BitHelper.SetBit(value, 14, false);
                    value = BitHelper.SetBit(value, 15, false);
                    break;
                case RotationType.Rotate90:
                    value = BitHelper.SetBit(value, 14, true);
                    value = BitHelper.SetBit(value, 15, false);
                    break;
                case RotationType.Rotate180:
                    value = BitHelper.SetBit(value, 14, false);
                    value = BitHelper.SetBit(value, 15, true);
                    break;
                case RotationType.Rotate270:
                    value = BitHelper.SetBit(value, 14, true);
                    value = BitHelper.SetBit(value, 15, true);
                    break;
            }
            return value;
        }


    }
}
