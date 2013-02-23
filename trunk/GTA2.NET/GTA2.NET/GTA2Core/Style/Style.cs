//Created: 18.01.2010

using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Drawing;
using Hiale.GTA2NET.Core.Helper;

namespace Hiale.GTA2NET.Core.Style
{
    public class Style
    {
        public const string Png = ".png";
        public const string TilesZipDir = "Tiles/";
        public const string SpritesZipDir = "Sprites/";

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

        public string StylePath { get; private set; }

        public Dictionary<int, CarInfo> CarInfos { get; private set; }

        private Dictionary<int, List<int>> _carSprites; //Helper variable to see which sprites are used by more than one model.

        public Style()
        {
            CarInfos = new Dictionary<int, CarInfo>();
            deltas = new List<Delta>();
            Surfaces = new List<Surface>();
            _carSprites = new Dictionary<int, List<int>>();
        }

        public void ReadFromFile(string styleFile)
        {
            ReadFromFile(styleFile, false);
        }

        public void ReadFromFile(string stylePath, bool extractGraphics)
        {
            //extractGraphics = true;
            BinaryReader reader = null;
            try
            {
                if (!File.Exists(stylePath))
                    throw new FileNotFoundException("Style File not found!", stylePath);
                StylePath = stylePath;
                System.Diagnostics.Debug.WriteLine("Reading style file " + stylePath);
                var stream = new FileStream(stylePath, FileMode.Open);
                reader = new BinaryReader(stream);
                System.Text.Encoding encoder = System.Text.Encoding.ASCII;
                reader.ReadBytes(4); //GBMP
                int version = reader.ReadUInt16();
                System.Diagnostics.Debug.WriteLine("Style version: " + version);
                while (stream.Position < stream.Length)
                {
                    string chunkType = encoder.GetString(reader.ReadBytes(4));
                    int chunkSize = (int)reader.ReadUInt32();
                    System.Diagnostics.Debug.WriteLine("Found chunk '" + chunkType + "' with size " + chunkSize.ToString(CultureInfo.InvariantCulture) + ".");
                    switch (chunkType)
                    {
                        case "TILE": //Tiles
                            if (extractGraphics)
                                ReadTiles(reader, chunkSize);
                            else
                                reader.ReadBytes(chunkSize);
                            break;
                        case "PPAL": //Physical Palette
                            if (extractGraphics)
                                ReadPhysicalPalette(reader, chunkSize);
                            else
                                reader.ReadBytes(chunkSize);
                            break;
                        case "SPRB": //Sprite Bases
                            if (extractGraphics)
                                ReadSpriteBases(reader);
                            else
                                reader.ReadBytes(chunkSize);
                            break;
                        case "PALX": //Palette Index
                            if (extractGraphics)
                                ReadPaletteIndexes(reader, chunkSize);
                            else
                                reader.ReadBytes(chunkSize);
                            break;
                        case "OBJI": //Map Objects
                            ReadMapObjects(reader, chunkSize);
                            break;
                        case "FONB": //Font Base
                            if (extractGraphics)
                                ReadFonts(reader);
                            else
                                reader.ReadBytes(chunkSize);
                            break;
                        case "DELX": //Delta Index
                            if (extractGraphics)
                                ReadDeltaIndex(reader, chunkSize);
                            else
                                reader.ReadBytes(chunkSize);
                            break;
                        //case "DELS": //Delta Store
                        //    ReadDeltaStore(reader, chunkSize);
                        //    break; 
                        case "CARI": //Car Info
                            ReadCars(reader, chunkSize);
                            break;
                        case "SPRG": //Sprite Graphics
                            if (extractGraphics)
                                ReadSpritesGraphics(reader, chunkSize);
                            else
                                reader.ReadBytes(chunkSize);
                            break;
                        case "SPRX": //Sprite Index
                            if (extractGraphics)
                                ReadSpriteIndex(reader, chunkSize);
                            else
                                reader.ReadBytes(chunkSize);
                            break;
                        case "PALB": //Palette Base
                            if (extractGraphics)
                                ReadPaletteBase(reader);
                            else
                                reader.ReadBytes(chunkSize);
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
            }
            catch (Exception e)
            {

            }
            finally
            {
                if (reader != null) reader.Close();
            }
            if (extractGraphics)
            {
                string styleFile = Path.GetFileNameWithoutExtension(StylePath);
                //MemoryStream memoryStream = new MemoryStream();
                //ZipStorer zip = ZipStorer.Create(memoryStream, string.Empty);
                var zip = ZipStorer.Create("Textures\\" + styleFile + ".zip", string.Empty);
                SaveTiles(zip);
                SaveSprites(zip);
                zip.Close();


                //Clean-up
                Array.Clear(PaletteIndexes, 0, PaletteIndexes.Length);
                Array.Clear(PhysicalPalettes, 0, PhysicalPalettes.Length);
                Array.Clear(tileData, 0, tileData.Length);
                Array.Clear(spriteData, 0, spriteData.Length);
                Array.Clear(objectInfos, 0, objectInfos.Length);
                //CarStyleInfos.Clear();
                _carSprites.Clear();
                deltas.Clear();
                Surfaces.Clear();
                GC.Collect();
            }
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
            for (var i = 0; i < fontBase.FontCount; i++)
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
            for (var i = 0; i < PaletteIndexes.Length; i++)
            {
                PaletteIndexes[i] = reader.ReadUInt16();
            }

        }

        private void ReadPhysicalPalette(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading physical palettes...");
            PhysicalPalettes = new uint[chunkSize / 4];
            for (var i = 0; i < PhysicalPalettes.Length; i++)
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
            var unused = reader.ReadUInt16(); //unused
            System.Diagnostics.Debug.WriteLine("[UNUSED BASE]: " + unused);
        }

        private void ReadCars(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading car infos...");
            int position = 0;
            int currentSprite = 0;
            var modelList = new List<int>();
            while (position < chunkSize)
            {
                CarInfo carInfo = new CarInfo();
                carInfo.Model = reader.ReadByte();
                carInfo.Sprite = currentSprite;
                modelList.Add(carInfo.Model);
                byte useNewSprite = reader.ReadByte();
                if (useNewSprite > 0)
                {
                    currentSprite++;
                    _carSprites.Add(carInfo.Sprite, modelList);
                    modelList = new List<int>();
                }
                carInfo.Width = reader.ReadByte();
                carInfo.Height = reader.ReadByte();
                var numRemaps = reader.ReadByte();
                carInfo.Passengers = reader.ReadByte();
                carInfo.Wreck = reader.ReadByte();
                carInfo.Rating = reader.ReadByte();
                carInfo.FrontWheelOffset = reader.ReadByte();
                carInfo.RearWheelOffset = reader.ReadByte();
                carInfo.FrontWindowOffset = reader.ReadByte();
                carInfo.RearWindowOffset = reader.ReadByte();
                var infoFlag = reader.ReadByte();
                carInfo.InfoFlags = (CarInfoFlags)infoFlag;
                var infoFlag2 = reader.ReadByte();
                var infoFlags2Value0 = BitHelper.CheckBit(infoFlag2, 0);
                var infoFlags2Value1 = BitHelper.CheckBit(infoFlag2, 1);
                if (infoFlags2Value0)
                    carInfo.InfoFlags += 0x100;
                if (infoFlags2Value1)
                    carInfo.InfoFlags += 0x200;
                for (int i = 0; i < numRemaps; i++)
                {
                    carInfo.RemapList.Add(reader.ReadByte());
                }
                byte numDoors = reader.ReadByte();
                for (var i = 0; i < numDoors; i++)
                {
                    var door = new DoorInfo();
                    door.X = reader.ReadByte();
                    door.Y = reader.ReadByte();
                    carInfo.Doors.Add(door);
                }
                if (!CarInfos.Keys.Contains(carInfo.Model))
                    CarInfos.Add(carInfo.Model, carInfo);
                position = position + 15 + numRemaps + numDoors * 2;
            }
        }

        private void ReadMapObjects(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading map object information...");            
            objectInfos = new ObjectInfo[chunkSize / 2];
            System.Diagnostics.Debug.WriteLine("Found " + objectInfos.Length + " entries");
            for (var i = 0; i < objectInfos.Length; i++)
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
                spriteEntries[i].Ptr = reader.ReadUInt32();
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
            int position = 0;
            while (position < chunkSize)
            {
                var delta = new Delta();
                delta.Sprite = reader.ReadUInt16();
                int deltaCount = reader.ReadByte();
                reader.ReadByte(); //dummy data
                for (var i = 0; i < deltaCount; i++)
                    delta.DeltaSize.Add(reader.ReadUInt16());
                deltas.Add(delta);
                position += 4 + (deltaCount * 2);
            }
        }

        private void ReadDeltaStore(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading delta store");
            var position = 0;
            var i = 0;
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
            var currentType = SurfaceType.Grass;
            var position = 0;
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

        private void SaveTiles(ZipStorer zip)
        {
            var tilesCount = tileData.Length / (64 * 64);
            for (var i = 0; i < tilesCount; i++)
            {
                //SaveTile("textures\\tiles\\" + i + ".png", i);
                SaveTile(zip, ref i);
            }
        }

        private void SaveTile(ZipStorer zip, ref int id)
        {
            UInt32 vpallete = PaletteIndexes[id];
            var bmp = new Bitmap(64, 64); 
            var bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var stride = bmData.Stride;
            var scan0 = bmData.Scan0;
            unsafe
            {
                var p = (byte*)(void*)scan0;
                var nOffset = stride - bmp.Width * 4;
                for (var y = 0; y < bmp.Height; ++y)
                {
                    for (var x = 0; x < bmp.Width; ++x)
                    {
                        UInt32 tileColor = tileData[(y + (id / 4) * 64) * 256 + (x + (id % 4) * 64)];
                        var palID = (vpallete / 64) * 256 * 64 + (vpallete % 64) + tileColor * 64;
                        var baseColor = (PhysicalPalettes[palID]) & 0xFFFFFF;
                        var color = BitConverter.GetBytes(baseColor);
                        p[0] = color[0];
                        p[1] = color[1];
                        p[2] = color[2];
                        var alphaColor = tileColor > 0 ? (byte)0xFF : (byte)0;
                        p[3] = alphaColor;
                        p += 4;
                    }
                    p += nOffset;
                }
            }
            bmp.UnlockBits(bmData);
            var memoryStream = new MemoryStream();
            bmp.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;
            zip.AddStream(ZipStorer.Compression.Deflate, TilesZipDir + id + Png, memoryStream, DateTime.Now, string.Empty);
            memoryStream.Close();
            //bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
        }

        private void SaveSprites(ZipStorer zip)
        {
            //cars
            foreach (KeyValuePair<int, List<int>> carSpriteItem in _carSprites)
            {
                //SaveCarSprite("textures\\sprites\\cars\\", carSpriteItem.Key, carSpriteItem.Value);
                SaveCarSprite(zip, carSpriteItem.Key, carSpriteItem.Value);
            }
            return;
            //Peds
            /*             
            Remaps
            0 	Cop
            1 	Green SWAT cop
            2 	Red SWAT cop
            3 	Yellow SWAT cop
            4 	Soldier
            5 	Redneck #1
            6 	Redneck #2
            7 	SRS Scientist
            8 	Zaibatsu member
            9 	Hare Krishna member
            10 	Russian
            11 	Loonie
            12 	Elvis
            13 	Yakuza
            14 	Fire fighter
            15 	Car jacker
            16 	Medic
            17 	Pickpocket
            18 	Blue pedestrian
            19 	Light blue pedestrian
            20 	Red pedestrian
            21 	Pedestrian
            22 	Prisoner
            23 	Poisened pedestrian
            24 	Poisened pedestrian
            25 	Claude Speed (default playerped)
            26 	Naked pedestrian
            27  t/m 52 	Other normal pedestrians 
            */
            const string path = "textures\\sprites\\peds\\";
            UInt32 remapPalette = PaletteIndexes[paletteBase.Tile + paletteBase.Sprite + paletteBase.CarRemap];
            //int remapPaletteEnd = PaletteIndexes[paletteBase.Tile + paletteBase.Sprite + paletteBase.CarRemap + paletteBase.PedRemap];
            //int remapCount = remapPaletteEnd - remapPalette;
            for (int i = spriteBase.Ped; i < spriteBase.CodeObj; i++)
            {
                UInt32 basePalette = PaletteIndexes[paletteBase.Tile + i];
                //SaveSpriteRemap(path + "\\" + i + "_-1.png", i, (basePalette));
                for (int j = 0; j < 53; j++)
                {
                    Directory.CreateDirectory(path + j);
                    //SaveSpriteRemap(path + j + "\\" + i + "_" + j + ".png", i, (UInt32)(remapPalette + j));
                }
            }
            System.Diagnostics.Debug.WriteLine("Done!");
        }

        //private void SaveCarSprite(string path, int spriteID, IList<int> modelList)
        private void SaveCarSprite(ZipStorer zip, int spriteID, IList<int> modelList)
        {
            UInt32 basePalette = PaletteIndexes[paletteBase.Tile + spriteID];
            UInt32 remapPalette = PaletteIndexes[paletteBase.Tile + paletteBase.Sprite];
            //UInt32 remapPalette = PaletteIndexes[paletteBase.Tile + paletteBase.Sprite + spriteID]; //the doc says, I have to add the spriteID, but it gives wrong results...
            for (var i = 0; i < modelList.Count; i++)
            {
                //SaveSpriteRemap(zip, + spriteID, spriteID, basePalette); //this way, models which use a shared sprite, only get's saved once. (spriteID.png)
                SaveSpriteRemap(zip, + spriteID + "_" + modelList[i] + "_-1", ref spriteID, basePalette); //in this way, the naming sheme is the same as with remap (spriteID_model_remap.png)
                var remapList = CarInfos[modelList[i]].RemapList;
                for (int j = 0; j < remapList.Count; j++)
                {
                    var remapID = remapList[j];
                    var remapIDhack = remapID;
                    if (remapIDhack >= 35) //hack, remap ids above 35 seems to be broken, this fixes them. Don't ask me why!
                        remapIDhack--;
                    SaveSpriteRemap(zip, + spriteID + "_" + modelList[i] + "_" + remapID, ref spriteID, remapPalette + remapIDhack);
                }
            }
        }

        //private void SaveSpriteRemap(string fileName, int id, UInt32 palette)
        private void SaveSpriteRemap(ZipStorer zip, string fileName, ref int id, UInt32 palette)
        {
            int width = spriteEntries[id].Width;
            int height = spriteEntries[id].Height;

            var bmp = new Bitmap(width, height);

            var baseX = (int)(spriteEntries[id].Ptr % 256);
            var baseY = (int)(spriteEntries[id].Ptr / 256);

            var bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var stride = bmData.Stride;
            var scan0 = bmData.Scan0;
            unsafe
            {
                var p = (byte*)(void*)scan0;
                var nOffset = stride - bmp.Width * 4;
                for (int y = 0; y < bmp.Height; ++y)
                {
                    for (int x = 0; x < bmp.Width; ++x)
                    {
                        UInt32 spriteColor = spriteData[(baseX + x) + (baseY + y) * 256];
                        var palID = (palette / 64) * 256 * 64 + (palette % 64) + spriteColor * 64;
                        var baseColor = (PhysicalPalettes[palID]) & 0xFFFFFF;
                        var color = BitConverter.GetBytes(baseColor);
                        p[0] = color[0];
                        p[1] = color[1];
                        p[2] = color[2];
                        var alphaColor = spriteColor > 0 ? (byte)0xFF : (byte)0;
                        p[3] = alphaColor;
                        p += 4;
                    }
                    p += nOffset;
                }
            }
            bmp.UnlockBits(bmData);
            var memoryStream = new MemoryStream();
            bmp.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;
            zip.AddStream(ZipStorer.Compression.Deflate, SpritesZipDir + fileName + Png, memoryStream, DateTime.Now, string.Empty);
            memoryStream.Close();
            //bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
