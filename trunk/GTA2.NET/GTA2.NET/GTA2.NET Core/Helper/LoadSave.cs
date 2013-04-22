// GTA2.NET
// 
// File: LoadSave.cs
// Created: 04.03.2013
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
using System.IO;
using Hiale.GTA2NET.Core.Map;
using Hiale.GTA2NET.Core.Map.Blocks;

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
                blocks[x, y, z] = new Empty();
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
                            blocks[x, y, z] = new Empty();
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
            block.Left.SaveBlockFaceEdge(writer);
            block.Right.SaveBlockFaceEdge(writer);
            block.Top.SaveBlockFaceEdge(writer);
            block.Bottom.SaveBlockFaceEdge(writer);
            block.Lid.SaveBlockFaceLid(writer);
            writer.Write((byte)block.Arrows);

            byte value = 0;
            switch (block.GroundType)
            {
                case GroundType.Air:
                    value = (byte) BitHelper.SetBit(value, 0, false);
                    value = (byte) BitHelper.SetBit(value, 1, false);
                    break;
                case GroundType.Road:
                    value = (byte) BitHelper.SetBit(value, 0, false);
                    value = (byte) BitHelper.SetBit(value, 1, false);
                    break;
                case GroundType.Pavement:
                    value = (byte) BitHelper.SetBit(value, 0, false);
                    value = (byte) BitHelper.SetBit(value, 1, false);
                    break;
                case GroundType.Field:
                    value = (byte) BitHelper.SetBit(value, 0, true);
                    value = (byte) BitHelper.SetBit(value, 1, true);
                    break;
            }

            var slope = (byte) block.SlopeType;
            for (var i = 2; i < 8; i++)
            {
                if ((slope & (1 << i - 2)) != 0)
                    value = (byte)(value | (1 << i)); //set Bit ON
                else
                    value = (byte)(value & ~(1 << i)); //set Bit OFF
            }
            writer.Write(value);
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
            block.ParseSlope(reader.ReadByte());
        }

        public static ushort SaveBlockFace(this BlockFace blockFace)
        {
            ushort value = 0;

            //Tile number, Bits 0-9
            for (var i = 0; i < 10; i++)
            {
                if ((blockFace.TileNumber & (1 << i)) != 0)
                    value = (ushort)(value | (1 << i)); //set Bit ON
                else
                    value = (ushort)(value & ~(1 << i)); //set Bit OFF
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

        public static void SaveBlockFaceEdge(this BlockFaceEdge face, BinaryWriter writer)
        {
            var value = face.SaveBlockFace();
            value = BitHelper.SetBit(value, 10, face.Wall);
            value = BitHelper.SetBit(value, 11, face.BulletWall);
            writer.Write(value);
        }

        public static void SaveBlockFaceLid(this BlockFaceLid face, BinaryWriter writer)
        {
            var value = face.SaveBlockFace();
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
    }
}
