//Created: 18.01.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using Hiale.GTA2.Core.Style;

namespace Hiale.GTA2NET.Core.Style
{
    public class Style
    {
        private ushort[] PaletteIndexes;
        private uint[] PhysicalPalettes;
        private PaletteBase paletteBase;
        private byte[] tileData;
        private byte[] spriteData;
        private SpriteEntry[] spriteEntries;
        private SpriteBase spriteBase;

        private FontBase fontBase;

        private ObjectInfo[] objectInfos;
        private List<Delta> deltas;
        private List<Surface> Surfaces;

        public Dictionary<int, CarInfo> CarInfos { get; private set; }

        public Style()
        {
            CarInfos = new Dictionary<int, CarInfo>();
            deltas = new List<Delta>();
            Surfaces = new List<Surface>();
            //PhysicalPalettes = new ushort[16384];
            //Palettes = new uint[64 * 64 * 256];
            //Read("data\\bil.sty");
        }

        public void ReadFromFile(string styleFile)
        {
            System.Diagnostics.Debug.WriteLine("Reading style file " + styleFile);
            FileStream stream = new FileStream(styleFile, FileMode.Open);
            BinaryReader reader = new BinaryReader(stream);
            System.Text.Encoding encoder = System.Text.Encoding.ASCII;
            reader.ReadBytes(4); //GBMP
            int version = reader.ReadUInt16();
            System.Diagnostics.Debug.WriteLine("Style version: " + version);
            while (stream.Position < stream.Length)
            {
                string chunkType = encoder.GetString(reader.ReadBytes(4));
                int chunkSize = (int)reader.ReadUInt32();
                System.Diagnostics.Debug.WriteLine("Found chunk '" + chunkType + "' with size " + chunkSize.ToString() + ".");
                switch (chunkType)
                {
                    case "TILE": //Tiles
                        ReadTiles(reader, chunkSize);
                        break;
                    case "PPAL": //Physical Palette
                        ReadPhysicalPalette(reader, chunkSize);
                        break;
                    case "SPRB": //Sprite Bases
                        ReadSpriteBases(reader);
                        break;
                    case "PALX": //Palette Index
                        ReadPaletteIndexes(reader, chunkSize);
                        break;
                    case "OBJI": //Map Objects
                        ReadMapObjects(reader, chunkSize);
                        break;
                    case "FONB": //Font Base
                        ReadFonts(reader);
                        break;
                    case "DELX": //Delta Index
                        ReadDeltaIndex(reader, chunkSize);
                        break;
                    //case "DELS": //Delta Store
                    //    ReadDeltaStore(reader, ChunkSize);
                    //    break; 
                    case "CARI": //Car Info
                        ReadCars(reader, chunkSize);
                        break;
                    case "SPRG": //Sprite Graphics
                        ReadSpritesGraphics(reader, chunkSize);
                        break;
                    case "SPRX": //Sprite Index
                        ReadSpriteIndex(reader, chunkSize);
                        break;
                    case "PALB": //Palette Base
                        ReadPaletteBase(reader);
                        break;
                    case "SPEC": //Undocumented
                        ReadSurfaces(reader, chunkSize);
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine("Skipping chunk...");
                        reader.ReadBytes(chunkSize);
                        break;
                }
            }
            reader.Close();
            //SaveTiles();
            //SaveSprites();

            //Clean-up
            Array.Clear(PaletteIndexes, 0, PaletteIndexes.Length);
            Array.Clear(PhysicalPalettes, 0, PhysicalPalettes.Length);
            Array.Clear(tileData, 0, tileData.Length);
            Array.Clear(spriteData, 0, spriteData.Length);
            Array.Clear(objectInfos, 0, objectInfos.Length);
            //CarStyleInfos.Clear();
            deltas.Clear();
            Surfaces.Clear();
            GC.Collect();
        }

        private void ReadTiles(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading tiles... Found " + chunkSize / (64 * 64) + " tiles");
            tileData = reader.ReadBytes(chunkSize);
        }

        private void ReadFonts(BinaryReader reader)
        {
            System.Diagnostics.Debug.WriteLine("Reading fonts...");
            fontBase.FontCount = reader.ReadUInt16();
            fontBase.Base = new UInt16[256];
            fontBase.SpriteBase = new UInt16[256];
            fontBase.Base[0] = spriteBase.Font;
            for (int i = 0; i < fontBase.FontCount; i++)
            {
                fontBase.Base[i] = reader.ReadUInt16();
                if (i > 0)
                    fontBase.SpriteBase[i] = (UInt16)(fontBase.SpriteBase[i - 1] + fontBase.Base[i]);
                System.Diagnostics.Debug.WriteLine("Font: " + i + " (" + fontBase.Base[i] + " characters, Spritebase: " + fontBase.SpriteBase[i]);
            }
        }

        private void ReadPaletteIndexes(BinaryReader reader, int chunkSize)
        {            
            PaletteIndexes = new ushort[16384];
            System.Diagnostics.Debug.WriteLine("Reading " + chunkSize / 2 + " palette entries");
            for (int i = 0; i < PaletteIndexes.Length; i++)
            {
                PaletteIndexes[i] = reader.ReadUInt16();
            }

        }

        private void ReadPhysicalPalette(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading physical palettes...");
            PhysicalPalettes = new uint[chunkSize / 4];
            for (int i = 0; i < PhysicalPalettes.Length; i++)
            {
                PhysicalPalettes[i] = reader.ReadUInt32();
            } 
        }

        private void ReadSpriteBases(BinaryReader reader)
        {
            System.Diagnostics.Debug.WriteLine("Reading sprite bases...");
            spriteBase.Car = 0;
            System.Diagnostics.Debug.WriteLine("Car base: " + spriteBase.Car);
            spriteBase.Ped = reader.ReadUInt16();
            System.Diagnostics.Debug.WriteLine("Ped base: " + spriteBase.Ped);
            spriteBase.CodeObj = (UInt16)(spriteBase.Ped + reader.ReadUInt16());
            System.Diagnostics.Debug.WriteLine("CodeObj base: " + spriteBase.CodeObj);
            spriteBase.MapObj = (UInt16)(spriteBase.CodeObj + reader.ReadUInt16());
            System.Diagnostics.Debug.WriteLine("MapObj base: " + spriteBase.MapObj);
            spriteBase.User = (UInt16)(spriteBase.MapObj + reader.ReadUInt16());
            System.Diagnostics.Debug.WriteLine("User base: " + spriteBase.User);
            spriteBase.Font = (UInt16)(spriteBase.User + reader.ReadUInt16());
            System.Diagnostics.Debug.WriteLine("Font base: " + spriteBase.Font);
            UInt16 unused = reader.ReadUInt16(); //unused
            System.Diagnostics.Debug.WriteLine("[UNUSED BASE]: " + unused);
        }

        private void ReadCars(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading car infos...");
            int position = 0;
            while (position < chunkSize)
            {
                CarInfo carInfo = new CarInfo();
                carInfo.Model = reader.ReadByte();
                carInfo.Sprite = reader.ReadByte();
                carInfo.Width = reader.ReadByte();
                carInfo.Height = reader.ReadByte();
                byte numRemaps = reader.ReadByte();
                carInfo.Passengers = reader.ReadByte();
                carInfo.Wreck = reader.ReadByte();
                carInfo.Rating = reader.ReadByte();
                carInfo.FrontWheelOffset = reader.ReadByte();
                carInfo.RearWheelOffset = reader.ReadByte();
                carInfo.FrontWindowOffset = reader.ReadByte();
                carInfo.RearWindowOffset = reader.ReadByte();
                carInfo.InfoFlags = reader.ReadByte();
                carInfo.InfoFlags2 = reader.ReadByte();
                for (int i = 0; i < numRemaps; i++)
                {
                    carInfo.RemapList.Add(reader.ReadByte());
                }
                byte numDoors = reader.ReadByte();
                for (int i = 0; i < numDoors; i++)
                {
                    DoorInfo door = new DoorInfo();
                    door.X = reader.ReadByte();
                    door.Y = reader.ReadByte();
                    carInfo.Doors.Add(door);
                }
                CarInfos.Add(carInfo.Model, carInfo);
                position = position + 15 + numRemaps + numDoors * 2;
            }
        }

        private void ReadMapObjects(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading map object information...");            
            objectInfos = new ObjectInfo[chunkSize / 2];
            System.Diagnostics.Debug.WriteLine("Found " + objectInfos.Length + " entries");
            for (int i = 0; i < objectInfos.Length; i++)
            {
                objectInfos[i].Model = reader.ReadByte();
                objectInfos[i].Sprites = reader.ReadByte();
            }
        }

        private void ReadSpritesGraphics(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading sprites...");
            spriteData = reader.ReadBytes(chunkSize);
        }

        private void ReadSpriteIndex(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading sprite indexes... Found " + chunkSize / 8 + " entries");
            spriteEntries = new SpriteEntry[chunkSize / 8];
            for (int i = 0; i < spriteEntries.Length; i++)
            {
                spriteEntries[i] = new SpriteEntry();
                spriteEntries[i].ptr = reader.ReadUInt32();
                spriteEntries[i].Width = reader.ReadByte();
                spriteEntries[i].Height = reader.ReadByte();
                spriteEntries[i].Pad = reader.ReadUInt16();
            }

        }

        private void ReadPaletteBase(BinaryReader reader)
        {
            paletteBase = new PaletteBase();
            paletteBase.Tile = reader.ReadUInt16();
            paletteBase.Sprite = reader.ReadUInt16();
            paletteBase.CarRemap = reader.ReadUInt16();
            paletteBase.PedRemap = reader.ReadUInt16();
            paletteBase.CodeObjRemap = reader.ReadUInt16();
            paletteBase.MapObjRemap = reader.ReadUInt16();
            paletteBase.UserRemap = reader.ReadUInt16();
            paletteBase.FontRemap = reader.ReadUInt16();
        }

        private void ReadDeltaIndex(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading delta index");
            int Position = 0;
            while (Position < chunkSize)
            {
                Delta delta = new Delta();
                delta.Sprite = reader.ReadUInt16();
                int DeltaCount = reader.ReadByte();
                reader.ReadByte(); //dummy data
                for (int i = 0; i < DeltaCount; i++)
                {
                    delta.DeltaSize.Add(reader.ReadUInt16());
                }
                deltas.Add(delta);
                Position += 4 + (DeltaCount * 2);
            }
        }

        private void ReadDeltaStore(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading delta store");
            int position = 0;
            int i = 0;
            while (position < chunkSize)
            {
                i++;
                int offset = reader.ReadUInt16();
                byte length = reader.ReadByte();
                reader.ReadBytes(length);
                position += 3 + length;
            }
            System.Diagnostics.Debug.WriteLine(i);
        }

        private void ReadSurfaces(BinaryReader reader, int chunkSize)
        {
            SurfaceType currentType = SurfaceType.Grass;
            int position = 0;
            Surface currentSurface = null;
            while (position < chunkSize)
            {
                if (position == 0)
                {
                    //reader.ReadBytes(2); //Skip 2 bytes
                    currentSurface = new Surface(currentType);
                }
                int value = reader.ReadUInt16();
                if (value == 0)
                {
                    Surfaces.Add(currentSurface);
                    if (currentType != SurfaceType.GrassWall)
                    {
                        currentType++;
                        currentSurface = new Surface(currentType);
                    }
                }
                else
                {
                    currentSurface.Tiles.Add(value);
                }
                position += 2;
            }
        }

        private void SaveTiles()
        {
            int tilesCount = tileData.Length / (64 * 64);
            for (int i = 0; i < tilesCount; i++)
            {
                SaveTile("textures\\tiles\\" + i + ".png", i);
            }
        }

        private void SaveTile(string fileName, int ID)
        {
            UInt32 vpallete = PaletteIndexes[ID];
            Bitmap bmp = new Bitmap(64, 64); 
            for  (int y = 63; y >= 0; y--)
            {
                for (int x = 0; x < 64; x++)
                {
                    UInt32 tileColor = tileData[(y+(ID / 4)*64)*256+(x+(ID % 4)*64)];
                    UInt32 palID = (vpallete/64)*256*64+(vpallete % 64) + tileColor * 64;
                    UInt32 baseColor = (PhysicalPalettes[palID]) & 0xFFFFFF;
                    Color rgbColor = Color.FromArgb((int)baseColor);
                    UInt32 alphaColor = 0xFF - (tileColor > 0 ? (UInt32)0 : (UInt32)1) * 0xFF;
                    bmp.SetPixel(x, y, System.Drawing.Color.FromArgb((int)alphaColor, rgbColor)); //ToDo: Optimize! http://www.bobpowell.net/lockingbits.htm
                }
            }
            bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
        }

        private void SaveSprites()
        {
            for (int i = 0; i < spriteEntries.Length; i++)
            {
                SaveSprite("textures\\sprites\\" + i + ".png", i);
            }
        }

        private void SaveSprite(string fileName, int id)
        {
            UInt32 vpallete = PaletteIndexes[paletteBase.Tile + id];

            int width = spriteEntries[id].Width;
            int height = spriteEntries[id].Height;

            Bitmap bmp = new Bitmap(width, height);

            int baseX = (int)(spriteEntries[id].ptr % 256);
            int baseY = (int)(spriteEntries[id].ptr / 256);
            int baseXXX = (int)(spriteEntries[id].ptr - baseX - baseY * 256); //always 0
            if (baseXXX != 0)
                System.Diagnostics.Debug.WriteLine("Debug");

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    UInt32 spriteColor = spriteData[baseXXX + ((baseX + x) + (baseY + y) * 256)];
                    UInt32 palID = (vpallete / 64) * 256 * 64 + (vpallete % 64) + spriteColor * 64;
                    UInt32 baseColor = (PhysicalPalettes[palID]) & 0xFFFFFF;
                    Color rgbColor = Color.FromArgb((int)baseColor);

                    UInt32 alphaColor = spriteData[spriteEntries[id].ptr + x + y * 256];
                    UInt32 alphaColorFinal = 0xFF - (alphaColor > 0 ? (UInt32)0 : (UInt32)1) * 0xFF;
                    //AlphaColorFinal = (AlphaColorFinal == 0 ? (UInt32)255 : 0);
                    bmp.SetPixel(x, y, Color.FromArgb((int)alphaColorFinal, rgbColor));
                }
            }

            //bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
