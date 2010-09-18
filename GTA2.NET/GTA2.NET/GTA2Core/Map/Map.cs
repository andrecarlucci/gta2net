using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

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
            int version = reader.ReadUInt16();
            System.Diagnostics.Debug.WriteLine("Map version: " + version);
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
            uint columnCount = reader.ReadUInt32();
            uint[] columns = new uint[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                columns[i] = reader.ReadUInt32();
            }
            uint blockCount = reader.ReadUInt32();
            BlockInfo[] blocks = new BlockInfo[blockCount];
            for (int i = 0; i < blockCount; i++)
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
                blocks[i] = blockInfo;
            }
            CreateUncompressedMap(baseOffsets, columns, blocks);
        }

        private void ReadZones(BinaryReader reader, int chunkSize, System.Text.Encoding encoder)
        {
            int position = 0;
            while (position < chunkSize)
            {
                Zone zone = new Zone();
                zone.Type = (ZoneType)reader.ReadSByte();
                zone.Rectangle = new Microsoft.Xna.Framework.Rectangle(reader.ReadSByte(), reader.ReadSByte(), reader.ReadSByte(), reader.ReadSByte());
                int nameLength = reader.ReadSByte();
                zone.Name = encoder.GetString(reader.ReadBytes(nameLength));
                Zones.Add(zone);
                position = position + 6 + nameLength;
            }
        }

        private void ReadAnimation(BinaryReader reader, int chunkSize)
        {
            int position = 0;
            while (position < chunkSize)
            {
                TileAnimation anim = new TileAnimation();
                anim.BaseTile = reader.ReadUInt16();
                anim.FrameRate = reader.ReadByte();
                anim.Repeat = reader.ReadByte();
                byte animLength = reader.ReadByte();
                reader.ReadByte(); //Unused
                for (byte i = 0; i < animLength; i++)
                {
                    anim.Tiles.Add(reader.ReadUInt16());
                }
                Animations.Add(anim);
                position = position + 6 + animLength * 2;
            }
        }

        private void ReadObjects(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Map objects not implemented yet!");
            int position = 0;
            while (position < chunkSize)
            {
                //ToDo
                reader.ReadUInt16(); //x
                reader.ReadUInt16(); //y
                reader.ReadByte(); //Rotation
                reader.ReadByte(); //Type

            }
        }

        private void ReadLights(BinaryReader reader, int chunkSize)
        {
            int position = 0;
            while (position < chunkSize)
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
                position = position + 16;
            }            
        }

        private void CreateUncompressedMap(uint[,] baseOffsets, uint[] columns, BlockInfo[] blocks)
        {
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    uint columnIndex = baseOffsets[j,i];
                    uint height = columns[columnIndex] & 0xFF;
                    uint offset = (columns[columnIndex] & 0xFF00) >> 8;
                    for (int k = 0; k < height; k++)
                    {
                        if (k >= offset)
                        {
                            cityBlocks[i, j, k] = blocks[columns[columnIndex + k - offset + 1]];
                        }
                    }
                }
            }
        }

    }
}
