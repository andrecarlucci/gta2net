using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Hiale.GTA2NET.Core.Map
{
    public class Map
    {
        private bool loaded;
        private BlockInfo[, ,] cityBlocks;

        private List<Zone> Zones;
        private List<MapObject> Objects;
        private List<TileAnimation> Animations;
        private List<Light> Lights;        

        public Map()
        {
            cityBlocks = new BlockInfo[256, 256, 8];

            Zones = new List<Zone>();
            Objects = new List<MapObject>();
            Animations = new List<TileAnimation>();
            Lights = new List<Light>();            

            for (int i = 0; i < cityBlocks.GetLength(0); i++)
            {
                for (int j = 0; j < cityBlocks.GetLength(1); j++)
                {
                    for (int k = 0; k < cityBlocks.GetLength(2); k++)
                    {
                        cityBlocks[i, j, k] = new BlockInfo();
                    }
                }
            }          
        }

        /// <summary>
        /// 
        /// </summary>
        public BlockInfo[, ,] CityBlocks
        {
            get
            {
                if (!loaded)
                    throw new ArgumentException();
                return cityBlocks;
            }
        }

        public void ReadFromFile(string fileName)
        {
            System.Diagnostics.Debug.WriteLine("Loading map from file \"" + fileName + "\"");
            FileStream stream = new FileStream(fileName, FileMode.Open);
            BinaryReader reader = new BinaryReader(stream);
            System.Text.Encoding encoder = System.Text.Encoding.ASCII;
            reader.ReadBytes(4); //GBMP
            int Version = reader.ReadUInt16();
            System.Diagnostics.Debug.WriteLine("Map version: " + Version);
            while (stream.Position < stream.Length)
            {
                string chunkType = encoder.GetString(reader.ReadBytes(4));
                int chunkSize = (int)reader.ReadUInt32();
                System.Diagnostics.Debug.WriteLine("Found chunk '" + chunkType + "' with size " + chunkSize.ToString());
                switch (chunkType)
                {
                    case "DMAP":
                        ReadDMAP(reader);
                        break;
                    case "ZONE":
                        ReadZones(reader, chunkSize, encoder);
                        break;
                    case "MOBJ":
                        ReadObjects(reader, chunkSize);
                        break;
                    case "ANIM":
                        ReadAnimation(reader, chunkSize);
                        break;
                    case "LGHT":
                        ReadLights(reader, chunkSize);
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine("Skipping chunk...");
                        reader.ReadBytes(chunkSize);
                        break;
                }
            }
            reader.Close();
            loaded = true;
        }

        private void ReadDMAP(BinaryReader reader)
        {
            uint[,] baseOffsets = new uint[256, 256];
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    baseOffsets[i,j] = reader.ReadUInt32();
                }
            }
            uint ColumnCount = reader.ReadUInt32();
            uint[] Columns = new uint[ColumnCount];
            for (int i = 0; i < ColumnCount; i++)
            {
                Columns[i] = reader.ReadUInt32();
            }
            uint BlockCount = reader.ReadUInt32();
            BlockInfo[] Blocks = new BlockInfo[BlockCount];
            for (int i = 0; i < BlockCount; i++)
            {
                BlockInfo blockInfo = new BlockInfo();
                blockInfo.Left = new BlockFace(reader.ReadUInt16(), false);
                blockInfo.Right = new BlockFace(reader.ReadUInt16(), false);
                blockInfo.Top = new BlockFace(reader.ReadUInt16(), false);
                blockInfo.Bottom = new BlockFace(reader.ReadUInt16(), false);
                blockInfo.Lid = new BlockFace(reader.ReadUInt16(), true);
                blockInfo.Arrows = (RoadTrafficType)reader.ReadByte(); //ToDo: Check, don't know if this works...
                //blockInfo.SlopeType = reader.ReadByte();
                blockInfo.ParseSlope(reader.ReadByte());
                Blocks[i] = blockInfo;
            }
            CreateUncompressedMap(baseOffsets, Columns, Blocks);
        }

        private void ReadZones(BinaryReader reader, int ChunkSize, System.Text.Encoding encoder)
        {
            int Position = 0;
            while (Position < ChunkSize)
            {
                Zone zone = new Zone();
                zone.Type = (ZoneType)reader.ReadSByte();
                zone.Rectangle = new Microsoft.Xna.Framework.Rectangle(reader.ReadSByte(), reader.ReadSByte(), reader.ReadSByte(), reader.ReadSByte());
                int NameLength = reader.ReadSByte();
                zone.Name = encoder.GetString(reader.ReadBytes(NameLength));
                Zones.Add(zone);
                Position = Position + 6 + NameLength;
            }
        }

        private void ReadAnimation(BinaryReader reader, int ChunkSize)
        {
            int Position = 0;
            while (Position < ChunkSize)
            {
                TileAnimation anim = new TileAnimation();
                anim.BaseTile = reader.ReadUInt16();
                anim.FrameRate = reader.ReadByte();
                anim.Repeat = reader.ReadByte();
                byte AnimLength = reader.ReadByte();
                reader.ReadByte(); //Unused
                for (byte i = 0; i < AnimLength; i++)
                {
                    anim.Tiles.Add(reader.ReadUInt16());
                }
                Animations.Add(anim);
                Position = Position + 6 + AnimLength * 2;
            }
        }

        private void ReadObjects(BinaryReader reader, int ChunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Map objects not implemented yet!");
            int Position = 0;
            while (Position < ChunkSize)
            {
                //ToDo
                reader.ReadUInt16(); //x
                reader.ReadUInt16(); //y
                reader.ReadByte(); //Rotation
                reader.ReadByte(); //Type

            }
        }

        private void ReadLights(BinaryReader reader, int ChunkSize)
        {
            int Position = 0;
            while (Position < ChunkSize)
            {
                Light light = new Light();
                byte[] color = reader.ReadBytes(4);
                light.Color = new Color(color[1], color[2], color[3], color[0]);
                light.X = reader.ReadUInt16();
                light.Y = reader.ReadUInt16();
                light.Z = reader.ReadUInt16();
                light.Radius = reader.ReadUInt16();
                light.Intensity = reader.ReadByte();
                light.Shape = reader.ReadByte();
                light.OnTime = reader.ReadByte();
                light.OffTime = reader.ReadByte();
                Lights.Add(light);
                Position = Position + 16;
            }            
        }

        private void CreateUncompressedMap(uint[,] baseOffsets, uint[] Columns, BlockInfo[] Blocks)
        {
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    uint ColumnIndex = baseOffsets[j,i];
                    uint Height = Columns[ColumnIndex] & 0xFF;
                    uint Offset = (Columns[ColumnIndex] & 0xFF00) >> 8;
                    for (int bk = 0; bk < Height; bk++)
                    {
                        if (bk >= Offset)
                        {
                            cityBlocks[i, j, bk] = Blocks[Columns[ColumnIndex + bk - Offset + 1]];
                        }
                    }
                }
            }
        }

    }
}
