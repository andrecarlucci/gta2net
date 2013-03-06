using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Map
{
    public class Map
    {
        public const int MaxWidth = 256;
        public const int MaxLength = 256;
        public const int MaxHeight = 8;

        private bool _loaded;
        private BlockInfo[, ,] _cityBlocks;

        private readonly List<Zone> Zones;
        private List<MapObject> Objects;
        private readonly List<TileAnimation> Animations;
        private readonly List<Light> Lights;
      
        public int Width //x
        {
            get { return CityBlocks.GetLength(0); }
        }

        public int Length //y
        {
            get { return CityBlocks.GetLength(1); }
        }

        public int Height //z
        {
            get { return CityBlocks.GetLength(2); }
        }

        public Map()
        {
            _cityBlocks = new BlockInfo[MaxWidth, MaxLength, MaxHeight];

            Zones = new List<Zone>();
            Objects = new List<MapObject>();
            Animations = new List<TileAnimation>();
            Lights = new List<Light>();            

            for (var i = 0; i < _cityBlocks.GetLength(0); i++)
            {
                for (var j = 0; j < _cityBlocks.GetLength(1); j++)
                {
                    for (var k = 0; k < _cityBlocks.GetLength(2); k++)
                    {
                        _cityBlocks[i, j, k] = new BlockInfo();
                    }
                }
            }          
        }

        public Map(string fileName) :this()
        {
            ReadFromFile(fileName);
        }

        /// <summary>
        /// 
        /// </summary>
        public BlockInfo[, ,] CityBlocks
        {
            get
            {
                if (!_loaded)
                    throw new ArgumentException();
                return _cityBlocks;
            }
            internal set {
                _cityBlocks = value;
                _loaded = true;
            }
        }

        public void ReadFromFile(string fileName)
        {
            System.Diagnostics.Debug.WriteLine("Loading map from file \"" + fileName + "\"");
            var stream = new FileStream(fileName, FileMode.Open);
            var reader = new BinaryReader(stream);
            System.Text.Encoding encoder = System.Text.Encoding.ASCII;
            reader.ReadBytes(4); //GBMP
            int version = reader.ReadUInt16();
            System.Diagnostics.Debug.WriteLine("Map version: " + version);
            while (stream.Position < stream.Length)
            {
                var chunkType = encoder.GetString(reader.ReadBytes(4));
                var chunkSize = (int)reader.ReadUInt32();
                System.Diagnostics.Debug.WriteLine("Found chunk '" + chunkType + "' with size " + chunkSize.ToString(CultureInfo.InvariantCulture));
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
            _loaded = true;
        }

        private void ReadDMAP(BinaryReader reader)
        {
            var baseOffsets = new uint[256, 256];
            for (var i = 0; i < 256; i++)
            {
                for (var j = 0; j < 256; j++)
                    baseOffsets[i,j] = reader.ReadUInt32();
            }
            var columnCount = reader.ReadUInt32();
            var columns = new uint[columnCount];
            for (var i = 0; i < columnCount; i++)
                columns[i] = reader.ReadUInt32();
            var blockCount = reader.ReadUInt32();
            var blocks = new BlockInfo[blockCount];
            for (var i = 0; i < blockCount; i++)
            {
                var blockInfo = new BlockInfo();
                blockInfo.Left = new BlockFaceEdge(reader.ReadUInt16());
                blockInfo.Right = new BlockFaceEdge(reader.ReadUInt16());
                blockInfo.Top = new BlockFaceEdge(reader.ReadUInt16());
                blockInfo.Bottom = new BlockFaceEdge(reader.ReadUInt16());
                blockInfo.Lid = new BlockFaceLid(reader.ReadUInt16());
                blockInfo.Arrows = (RoadTrafficType)reader.ReadByte(); //ToDo: Check, don't know if this works...
                blockInfo.ParseSlope(reader.ReadByte());
                blocks[i] = blockInfo;
            }
            CreateUncompressedMap(baseOffsets, columns, blocks);
        }

        private void ReadZones(BinaryReader reader, int chunkSize, System.Text.Encoding encoder)
        {
            var position = 0;
            while (position < chunkSize)
            {
                var zone = new Zone();
                zone.Type = (ZoneType)reader.ReadSByte();
                zone.Rectangle = new Rectangle(reader.ReadSByte(), reader.ReadSByte(), reader.ReadSByte(), reader.ReadSByte());
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
                var anim = new TileAnimation();
                anim.BaseTile = reader.ReadUInt16();
                anim.FrameRate = reader.ReadByte();
                anim.Repeat = reader.ReadByte();
                var animLength = reader.ReadByte();
                reader.ReadByte(); //Unused
                for (byte i = 0; i < animLength; i++)
                    anim.Tiles.Add(reader.ReadUInt16());
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
                var light = new Light();
                var color = reader.ReadBytes(4);
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
            for (var i = 0; i < 256; i++)
            {
                for (var j = 0; j < 256; j++)
                {
                    var columnIndex = baseOffsets[j,i];
                    var height = columns[columnIndex] & 0xFF;
                    var offset = (columns[columnIndex] & 0xFF00) >> 8;
                    for (var k = 0; k < height; k++)
                    {
                        if (k >= offset)
                        {
                            _cityBlocks[i, j, k] = blocks[columns[columnIndex + k - offset + 1]];
                        }
                    }
                }
            }
        }
    }
}
