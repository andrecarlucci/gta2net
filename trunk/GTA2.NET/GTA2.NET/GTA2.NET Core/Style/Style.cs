// GTA2.NET
// 
// File: Style.cs
// Created: 18.01.2010
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Runtime.Remoting.Messaging;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Core.Helper.Threading;

namespace Hiale.GTA2NET.Core.Style
{
    public class Style
    {
        private struct StyleData
        {
            public ushort[] PaletteIndexes;
            public Palette[] Palettes;
            public PaletteBase PaletteBase;
            public byte[] TileData;
            public byte[] SpriteData;
            public SpriteEntry[] SpriteEntries;
            public SpriteBase SpriteBase;
            public FontBase FontBase;
            public DeltaIndex[] DeltaIndexes; 
            public byte[] DeltaData;
            
            public SerializableDictionary<int, CarInfo> CarInfo;
            public Dictionary<int, List<int>> CarSprites; //Helper variable to see which sprites are used by more than one model.

            public DateTime OriginalDateTime;

        }

        private const bool EXPORT_REMAPS = false; //don't draw remaps for now, we might should do something with Palette as well

        public string StylePath { get; private set; }
        public event EventHandler<ProgressMessageChangedEventArgs> ConvertStyleFileProgressChanged;
        public event AsyncCompletedEventHandler ConvertStyleFileCompleted;

        private delegate void ConvertStyleFileDelegate(string styleFile, CancellableContext context, out bool cancelled);
        private readonly object _sync = new object();
        public bool IsBusy { get; private set; }
        private CancellableContext _convertStyleFileContext;
        private readonly object _syncTextureAtlasFinished = new object();
        private readonly List<TextureAtlas> _runningAtlas = new List<TextureAtlas>();
        private readonly Dictionary<TextureAtlas, MemoryStream> _memoryStreams = new Dictionary<TextureAtlas, MemoryStream>();
        private static readonly AutoResetEventValueExchange<bool> WaitHandle = new AutoResetEventValueExchange<bool>(false);

        public IAsyncResult ReadFromFileAsync(string stylePath)
        {
            var worker = new ConvertStyleFileDelegate(ReadFromFile);
            var completedCallback = new AsyncCallback(BuildTextureAtlasCompletedCallback);

            lock (_sync)
            {
                if (IsBusy)
                    throw new InvalidOperationException("The control is currently busy.");

                var async = AsyncOperationManager.CreateOperation(null);
                var context = new CancellableContext(async);
                bool cancelled;

                var result = worker.BeginInvoke(stylePath, context, out cancelled, completedCallback, async);

                IsBusy = true;
                _convertStyleFileContext = context;
                return result;
            }
        }

        public void ReadFromFile(string stylePath)
        {
            var context = new CancellableContext(null);
            bool cancelled;
            ReadFromFile(stylePath, context, out cancelled);
        }

        private void ReadFromFile(string stylePath, CancellableContext context, out bool cancelled)
        {
            cancelled = false;

            var styleData = new StyleData
                {
                    PaletteIndexes = new ushort[] {},
                    Palettes = new Palette[] {},
                    PaletteBase = new PaletteBase(),
                    TileData = new byte[] {},
                    SpriteData = new byte[] {},
                    SpriteEntries = new SpriteEntry[] {},
                    SpriteBase = new SpriteBase(),
                    FontBase = new FontBase(),
                    DeltaData = new byte[] {},
                    DeltaIndexes = new DeltaIndex[] {},
                    CarInfo = new SerializableDictionary<int, CarInfo>(),
                    CarSprites = new Dictionary<int, List<int>>()
                };

            BinaryReader reader = null;
            try
            {
                if (!File.Exists(stylePath))
                    throw new FileNotFoundException("Style File not found!", stylePath);
                StylePath = stylePath;
                System.Diagnostics.Debug.WriteLine("Reading style file " + stylePath);
                var stream = new FileStream(stylePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                styleData.OriginalDateTime = File.GetLastWriteTime(stylePath);
                reader = new BinaryReader(stream);
                var encoder = System.Text.Encoding.ASCII;
                var magicNumber = encoder.GetString(reader.ReadBytes(4)); //GBMP
                if (magicNumber != "GBST")
                    throw new FormatException("Wrong style format!");
                int version = reader.ReadUInt16();
                System.Diagnostics.Debug.WriteLine("Style version: " + version);
                while (stream.Position < stream.Length)
                {
                    var chunkType = encoder.GetString(reader.ReadBytes(4));
                    var chunkSize = (int) reader.ReadUInt32();
                    System.Diagnostics.Debug.WriteLine("Found chunk '" + chunkType + "' with size " + chunkSize.ToString(CultureInfo.InvariantCulture) + ".");

                    if (context.IsCancelling)
                    {
                        cancelled = true;
                        return;
                    }
                    switch (chunkType)
                    {
                        case "TILE": //Tiles
                            styleData.TileData = ReadTiles(reader, chunkSize);
                            break;
                        case "PPAL": //Physical Palette
                            styleData.Palettes = ReadPhysicalPalette(reader, chunkSize);
                            break;
                        case "SPRB": //Sprite Bases
                            styleData.SpriteBase = ReadSpriteBases(reader);
                            break;
                        case "PALX": //Palette Index
                            styleData.PaletteIndexes = ReadPaletteIndexes(reader, chunkSize);
                            break;
                        case "OBJI": //Map Objects
                            ReadMapObjects(reader, chunkSize);
                            break;
                        case "FONB": //Font Base
                            styleData.FontBase = ReadFonts(reader, styleData.SpriteBase.Font);
                            break;
                        case "DELX": //Delta Index
                            styleData.DeltaIndexes = ReadDeltaIndex(reader, chunkSize);
                            break;
                        case "DELS": //Delta Store
                            styleData.DeltaData = ReadDeltaStore(reader, chunkSize);
                            break;
                        case "CARI": //Car Info
                            styleData.CarInfo = ReadCars(reader, chunkSize, styleData.CarSprites);
                            break;
                        case "SPRG": //Sprite Graphics
                            styleData.SpriteData = ReadSpritesGraphics(reader, chunkSize);
                            break;
                        case "SPRX": //Sprite Index
                            styleData.SpriteEntries = ReadSpriteIndex(reader, chunkSize);
                            break;
                        case "PALB": //Palette Base
                            styleData.PaletteBase = ReadPaletteBase(reader);
                            break;
                        case "SPEC": //Undocumented
                            ReadSurfaces(reader, chunkSize);
                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine("Skipping chunk '" +  chunkType + "'...");
                            reader.ReadBytes(chunkSize);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            
            SaveData(styleData, context, out cancelled);
        }

        private void SaveData(StyleData styleData, CancellableContext context, out bool cancelled)
        {
            var styleFile = Path.GetFileNameWithoutExtension(StylePath);
            CarInfo.Serialize(styleData.CarInfo, Globals.MiscSubDir + Path.DirectorySeparatorChar + styleFile + Globals.CarStyleSuffix + Globals.XmlFormat);
            Palette.SavePalettes(styleData.Palettes, Globals.GraphicsSubDir + Path.DirectorySeparatorChar + styleFile + Globals.PaletteSuffix + Globals.TextureImageFormat);

            var memoryStreamTiles = new MemoryStream();
            using (var zip = ZipStorer.Create(memoryStreamTiles, string.Empty))
            {
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return;
                }
                SaveTiles(styleData, zip, context);
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return;
                }

            }
            memoryStreamTiles.Position = 0;
            using (var stream = new FileStream(Globals.GraphicsSubDir + Path.DirectorySeparatorChar + styleFile + "_tiles.zip", FileMode.Create, FileAccess.Write))
            {
                var bytes = new byte[memoryStreamTiles.Length];
                memoryStreamTiles.Read(bytes, 0, (int)memoryStreamTiles.Length);
                stream.Write(bytes, 0, bytes.Length);
            }
            if (context.IsCancelling)
            {
                cancelled = true;
                return;
            }
            var memoryStreamSprites = new MemoryStream();
            using (var zip = ZipStorer.Create(memoryStreamSprites, string.Empty))
            {
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return;

                }
                SaveSprites(styleData, zip, context);
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return;
                }
            }
            memoryStreamSprites.Position = 0;
            using (var stream = new FileStream(Globals.GraphicsSubDir + Path.DirectorySeparatorChar + styleFile + "_sprites.zip", FileMode.Create, FileAccess.Write))
            {
                var bytes = new byte[memoryStreamSprites.Length];
                memoryStreamSprites.Read(bytes, 0, (int)memoryStreamSprites.Length);
                stream.Write(bytes, 0, bytes.Length);
            }
            if (context.IsCancelling)
            {
                cancelled = true;
                return;
            }
            var memoryStreamDeltas = new MemoryStream();
            using (var zip = ZipStorer.Create(memoryStreamDeltas, string.Empty))
            {
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return;
                }
                SaveDeltas(styleData, zip, context);
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return;
                }
            }
            memoryStreamDeltas.Position = 0;
            using (var stream = new FileStream(Globals.GraphicsSubDir + Path.DirectorySeparatorChar + styleFile + "_deltas.zip", FileMode.Create, FileAccess.Write))
            {
                var bytes = new byte[memoryStreamDeltas.Length];
                memoryStreamDeltas.Read(bytes, 0, (int)memoryStreamDeltas.Length);
                stream.Write(bytes, 0, bytes.Length);
            }

            memoryStreamTiles.Position = 0;
            memoryStreamSprites.Position = 0;
            memoryStreamDeltas.Position = 0;

            memoryStreamDeltas.Close();

            TextureAtlas atlas = CreateTextureAtlas<TextureAtlasTiles>(ZipStorer.Open(memoryStreamTiles, FileAccess.Read), styleFile + Globals.TilesSuffix);
            _memoryStreams.Add(atlas, memoryStreamTiles);
            _runningAtlas.Add(atlas);
            if (context.IsCancelling)
            {
                cancelled = true;
                return;
            }

            atlas = CreateTextureAtlas<TextureAtlasSprites>(ZipStorer.Open(memoryStreamSprites, FileAccess.Read), styleFile + Globals.SpritesSuffix);
            _memoryStreams.Add(atlas, memoryStreamSprites);
            _runningAtlas.Add(atlas);
            WaitHandle.WaitOne();
            cancelled = WaitHandle.Value;
        }

        public T CreateTextureAtlas<T>(ZipStorer inputZip, string outputFile) where T : TextureAtlas, new()
        {
            var args = new object[2];
            args[0] = outputFile + Globals.TextureImageFormat;
            args[1] = inputZip;
            var atlas = (T) Activator.CreateInstance(typeof (T), args);
            atlas.BuildTextureAtlasCompleted += BuildTextureAtlasCompleted;
            atlas.BuildTextureAtlasAsync();
            return atlas;
        }

        private void BuildTextureAtlasCompleted(object sender, AsyncCompletedEventArgs e)
        {
            lock (_syncTextureAtlasFinished)
            {
                var textureAtlas = (TextureAtlas) sender;
                if (_memoryStreams.ContainsKey(textureAtlas))
                    _memoryStreams[textureAtlas].Dispose();
                _runningAtlas.Remove((TextureAtlas)sender);
                if (_runningAtlas.Count > 0)
                    return;
                WaitHandle.Value = e.Cancelled;
                WaitHandle.Set();
            }
        }

        private void BuildTextureAtlasCompletedCallback(IAsyncResult ar)
        {
            // get the original worker delegate and the AsyncOperation instance
            var worker = (ConvertStyleFileDelegate)((AsyncResult)ar).AsyncDelegate;
            var async = (AsyncOperation)ar.AsyncState;
            bool cancelled;

            // finish the asynchronous operation
            worker.EndInvoke(out cancelled, ar);

            // clear the running task flag
            lock (_sync)
            {
                IsBusy = false;
                _convertStyleFileContext = null;
            }

            // raise the completed event
            var completedArgs = new AsyncCompletedEventArgs(null, cancelled, null);
            async.PostOperationCompleted(e => OnConvertStyleFileCompleted((AsyncCompletedEventArgs)e), completedArgs);
        }

        public void CancelConvertStyle()
        {
            lock (_sync)
            {
                if (_convertStyleFileContext != null)
                    _convertStyleFileContext.Cancel();
                foreach (var textureAtlas in _runningAtlas)
                    textureAtlas.CancelBuildTextureAtlas();
            }
        }

        private static byte[] ReadTiles(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading tiles... Found " + chunkSize / (64 * 64) + " tiles");
            var tileData = reader.ReadBytes(chunkSize);
            return tileData;
        }

        private static FontBase ReadFonts(BinaryReader reader, ushort spriteBaseFont)
        {
            System.Diagnostics.Debug.WriteLine("Reading fonts...");
            var fontBase = new FontBase
                {
                    FontCount = reader.ReadUInt16(),
                    Base = new ushort[256],
                    SpriteBase = new ushort[256]
                };
            fontBase.Base[0] = spriteBaseFont;
            for (var i = 0; i < fontBase.FontCount; i++)
            {
                fontBase.Base[i] = reader.ReadUInt16();
                if (i > 0)
                    fontBase.SpriteBase[i] = (ushort)(fontBase.SpriteBase[i - 1] + fontBase.Base[i]);
                System.Diagnostics.Debug.WriteLine("Font: " + i + " (" + fontBase.Base[i] + " characters, Spritebase: " + fontBase.SpriteBase[i]);
            }
            return fontBase;
        }

        private static ushort[] ReadPaletteIndexes(BinaryReader reader, int chunkSize)
        {
            var paletteIndexes = new ushort[16384];
            System.Diagnostics.Debug.WriteLine("Reading " + chunkSize / 2 + " palette entries");
            for (var i = 0; i < paletteIndexes.Length; i++)
                paletteIndexes[i] = reader.ReadUInt16();
            return paletteIndexes;
        }

        private static Palette[] ReadPhysicalPalette(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading physical palettes...");
            var palettesCount = chunkSize / 1024;
            var palettes = new Palette[palettesCount];
            for (var i = 0; i < palettes.Length; i++)
                palettes[i] = new Palette();

            for (var i = 0; i < palettesCount / 64; i++)
            {
                for (var j = 0; j < 256; j++)
                {
                    for (var k = 0; k < 64; k++)
                    {
                        var x = i * 64 + k;
                        palettes[x].Parse(reader.ReadBytes(4), j);
                    }
                }
            }
            return palettes;
        }

        private static SpriteBase ReadSpriteBases(BinaryReader reader)
        {
            var spriteBase = new SpriteBase();
            System.Diagnostics.Debug.WriteLine("Reading sprite bases...");
            spriteBase.Car = 0;
            System.Diagnostics.Debug.WriteLine("Car base: " + spriteBase.Car);
            spriteBase.Ped = reader.ReadUInt16();
            System.Diagnostics.Debug.WriteLine("Ped base: " + spriteBase.Ped);
            spriteBase.CodeObj = (ushort)(spriteBase.Ped + reader.ReadUInt16());
            System.Diagnostics.Debug.WriteLine("CodeObj base: " + spriteBase.CodeObj);
            spriteBase.MapObj = (ushort)(spriteBase.CodeObj + reader.ReadUInt16());
            System.Diagnostics.Debug.WriteLine("MapObj base: " + spriteBase.MapObj);
            spriteBase.User = (ushort)(spriteBase.MapObj + reader.ReadUInt16());
            System.Diagnostics.Debug.WriteLine("User base: " + spriteBase.User);
            spriteBase.Font = (ushort)(spriteBase.User + reader.ReadUInt16());
            System.Diagnostics.Debug.WriteLine("Font base: " + spriteBase.Font);
            var unused = reader.ReadUInt16(); //unused
            System.Diagnostics.Debug.WriteLine("[UNUSED BASE]: " + unused);
            return spriteBase;
        }

        private static SerializableDictionary<int, CarInfo> ReadCars(BinaryReader reader, int chunkSize, Dictionary<int, List<int>> carSprites)
        {
            System.Diagnostics.Debug.WriteLine("Reading car infos...");
            var carInfoDict = new SerializableDictionary<int, CarInfo>();
            var position = 0;
            var currentSprite = 0;
            var modelList = new List<int>();
            while (position < chunkSize)
            {
                var carInfo = new CarInfo { Model = reader.ReadByte(), Sprite = currentSprite };
                modelList.Add(carInfo.Model);
                var useNewSprite = reader.ReadByte();
                if (useNewSprite > 0)
                {
                    currentSprite++;
                    carSprites.Add(carInfo.Sprite, modelList);
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
                var numDoors = reader.ReadByte();
                for (var i = 0; i < numDoors; i++)
                {
                    var door = new DoorInfo { X = reader.ReadByte(), Y = reader.ReadByte() };
                    carInfo.Doors.Add(door);
                }
                if (!carInfoDict.Keys.Contains(carInfo.Model))
                    carInfoDict.Add(carInfo.Model, carInfo);
                position = position + 15 + numRemaps + numDoors * 2;
            }
            return carInfoDict;
        }

        private static ObjectInfo[] ReadMapObjects(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading map object information...");
            var objectInfos = new ObjectInfo[chunkSize / 2];
            System.Diagnostics.Debug.WriteLine("Found " + objectInfos.Length + " entries");
            for (var i = 0; i < objectInfos.Length; i++)
            {
                objectInfos[i].Model = reader.ReadByte();
                objectInfos[i].Sprites = reader.ReadByte();
            }
            return objectInfos;
        }

        private static byte[] ReadSpritesGraphics(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading sprites...");
            var spriteData = reader.ReadBytes(chunkSize);
            return spriteData;
        }

        private static SpriteEntry[] ReadSpriteIndex(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading sprite indexes... Found " + chunkSize / 8 + " entries");
            var spriteEntries = new SpriteEntry[chunkSize / 8];
            for (var i = 0; i < spriteEntries.Length; i++)
            {
                spriteEntries[i] = new SpriteEntry
                {
                    Ptr = reader.ReadUInt32(),
                    Width = reader.ReadByte(),
                    Height = reader.ReadByte(),
                    Pad = reader.ReadUInt16()
                };
            }
            return spriteEntries;
        }

        private static PaletteBase ReadPaletteBase(BinaryReader reader)
        {
            var paletteBase = new PaletteBase
            {
                Tile = reader.ReadUInt16(),
                Sprite = reader.ReadUInt16(),
                CarRemap = reader.ReadUInt16(),
                PedRemap = reader.ReadUInt16(),
                CodeObjRemap = reader.ReadUInt16(),
                MapObjRemap = reader.ReadUInt16(),
                UserRemap = reader.ReadUInt16(),
                FontRemap = reader.ReadUInt16()
            };
            return paletteBase;
        }

        private static DeltaIndex[] ReadDeltaIndex(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading delta indexes...");
            var deltas = new List<DeltaIndex>();
            var position = 0;
            while (position < chunkSize)
            {
                var delta = new DeltaIndex { Sprite = reader.ReadUInt16() };
                int deltaCount = reader.ReadByte();
                reader.ReadByte(); //dummy data
                for (var i = 0; i < deltaCount; i++)
                    delta.DeltaSize.Add(reader.ReadUInt16());
                deltas.Add(delta);
                position += 4 + (deltaCount * 2);
            }
            return deltas.ToArray();
        }

        private static byte[] ReadDeltaStore(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading delta store...");
            var deltaData = reader.ReadBytes(chunkSize);
            return deltaData;
        }

        private static IList<Surface> ReadSurfaces(BinaryReader reader, int chunkSize)
        {
            var surfaces = new List<Surface>();
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
                    surfaces.Add(currentSurface);
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
            return surfaces;
        }

        private static void SaveTiles(StyleData styleData, ZipStorer zip, CancellableContext context)
        {
            var tilesCount = styleData.TileData.Length / (64 * 64);
            for (var i = 0; i < tilesCount; i++)
            {
                if (context.IsCancelling)
                    return;
                SaveTile(styleData, zip, ref i);
            }
        }

        private static void SaveTile(StyleData styleData, ZipStorer zip, ref int id)
        {
            using (var bmp = new Bitmap(64, 64))
            {
                var bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                var stride = bmData.Stride;
                var scan0 = bmData.Scan0;
                unsafe
                {
                    var p = (byte*) (void*) scan0;
                    var nOffset = stride - bmp.Width*4;
                    for (var y = 0; y < bmp.Height; ++y)
                    {
                        for (var x = 0; x < bmp.Width; ++x)
                        {
                            uint tileColor = styleData.TileData[(y + (id/4)*64)*256 + (x + (id%4)*64)];
                            var palId = (styleData.PaletteIndexes[id]/64)*256*64 + (styleData.PaletteIndexes[id]%64) + tileColor*64;
                            var paletteIndex = palId%64;
                            p[0] = styleData.Palettes[paletteIndex].Colors[tileColor].B;
                            p[1] = styleData.Palettes[paletteIndex].Colors[tileColor].G;
                            p[2] = styleData.Palettes[paletteIndex].Colors[tileColor].R;
                            p[3] = tileColor > 0 ? (byte) 255 : (byte) 0;
                            p += 4;
                        }
                        p += nOffset;
                    }
                }
                bmp.UnlockBits(bmData);
                var memoryStream = new MemoryStream();
                bmp.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0;
                zip.AddStream(ZipStorer.Compression.Deflate, id + Globals.TextureImageFormat, memoryStream, styleData.OriginalDateTime, string.Empty);
                memoryStream.Close();
            }
        }

        private static void SaveSprites(StyleData styleData, ZipStorer zip, CancellableContext context)
        {
            //cars
            foreach (var carSpriteItem in styleData.CarSprites)
            {
                if (context.IsCancelling)
                    return;
                //SaveCarSprite(zip, carSpriteItem.Key, carSpriteItem.Value);
                SaveCarSprite(styleData, zip, carSpriteItem.Key, carSpriteItem.Value);
            }

            //Peds
            #region Peds
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
            #endregion Peds
            var remapPalette = styleData.PaletteIndexes[styleData.PaletteBase.Tile + styleData.PaletteBase.Sprite + styleData.PaletteBase.CarRemap];
            for (var i = styleData.SpriteBase.Ped; i < styleData.SpriteBase.CodeObj; i++)
            {
                var basePalette = styleData.PaletteIndexes[styleData.PaletteBase.Tile + i];
                SaveSpriteRemap(styleData, styleData.SpriteEntries[i], basePalette, zip, "Peds/" + i);
                if (EXPORT_REMAPS)
                {
                    for (var j = 0; j < 53; j++)
                    {
                        //SaveSpriteRemap(path + j + "\\" + i + "_" + j + ".png", i, (UInt32)(remapPalette + j));
                        SaveSpriteRemap(styleData, styleData.SpriteEntries[i], (UInt32) (remapPalette + j), zip, "Peds/ " + j + i + "_" + j);
                    }
                }
            }

            //Code Obj
            for (var i = styleData.SpriteBase.CodeObj; i < styleData.SpriteBase.MapObj; i++)
            {
                var basePalette = styleData.PaletteIndexes[styleData.PaletteBase.Tile + i];
                SaveSpriteRemap(styleData, styleData.SpriteEntries[i], basePalette, zip, "CodeObj/" + i);
            }

            //Map obj
            for (var i = styleData.SpriteBase.MapObj; i < styleData.SpriteBase.User; i++)
            {
                var basePalette = styleData.PaletteIndexes[styleData.PaletteBase.Tile + i];
                SaveSpriteRemap(styleData, styleData.SpriteEntries[i], basePalette, zip, "MapObj/" + i);
            }

            //User
            for (var i = styleData.SpriteBase.User; i < styleData.SpriteBase.Font; i++)
            {
                var basePalette = styleData.PaletteIndexes[styleData.PaletteBase.Tile + i];
                SaveSpriteRemap(styleData, styleData.SpriteEntries[i], basePalette, zip, "User/" + i);
            }

            ////Font
            //for (var i = styleData.SpriteBase.Font; i < styleData.SpriteEntries.Length; i++)
            //{
            //    var basePalette = styleData.PaletteIndexes[styleData.PaletteBase.Tile + i];
            //    SaveSpriteRemap(styleData, styleData.SpriteEntries[i], basePalette, zip, "Font/" + i);
            //}

        }

        private static void SaveCarSprite(StyleData styleData, ZipStorer zip, int spriteId, IEnumerable<int> modelList)
        {
            var basePalette = styleData.PaletteIndexes[styleData.PaletteBase.Tile + spriteId];
            var remapPalette = styleData.PaletteIndexes[styleData.PaletteBase.Tile + styleData.PaletteBase.Sprite];
            var spriteEntry = styleData.SpriteEntries[spriteId];
            //UInt32 remapPalette = PaletteIndexes[paletteBase.Tile + paletteBase.Sprite + spriteID]; //the doc says, I have to add the spriteID, but it gives wrong results...
            foreach (var model in modelList)
            {
                SaveSpriteRemap(styleData, spriteEntry, basePalette, zip, "Cars/" + spriteId + "_" + model + "_-1"); //in this way, the naming sheme is the same as with remap (spriteID_model_remap.png)
                if (EXPORT_REMAPS)
                {
                    var remapList = styleData.CarInfo[model].RemapList;
                    foreach (var remapId in remapList)
                    {
                        var remapIDhack = remapId;
                        if (remapIDhack >= 35) //hack, remap ids above 35 seems to be broken, this fixes them. Don't ask me why!
                            remapIDhack--;
                        SaveSpriteRemap(styleData, spriteEntry, (uint) (remapPalette + remapIDhack), zip, "Cars/" + spriteId + "_" + model + "_" + remapId);
                    }
                }
            }
        }

        private static void SaveSpriteRemap(StyleData styleData, SpriteEntry spriteEntry, uint palette, ZipStorer zip, string fileName)
        {
            using (var bmp = new Bitmap(spriteEntry.Width, spriteEntry.Height))
            {
                var baseX = (int) (spriteEntry.Ptr%256);
                var baseY = (int) (spriteEntry.Ptr/256);

                var bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                var stride = bmData.Stride;
                var scan0 = bmData.Scan0;
                unsafe
                {
                    var p = (byte*) (void*) scan0;
                    var nOffset = stride - bmp.Width*4;
                    for (var y = 0; y < bmp.Height; ++y)
                    {
                        for (var x = 0; x < bmp.Width; ++x)
                        {
                            uint spriteColor = styleData.SpriteData[(baseX + x) + (baseY + y)*256];
                            //var palId = (palette / 64) * 256 * 64 + (palette % 64) + spriteColor * 64;
                            p[0] = styleData.Palettes[palette].Colors[spriteColor].B;
                            p[1] = styleData.Palettes[palette].Colors[spriteColor].G;
                            p[2] = styleData.Palettes[palette].Colors[spriteColor].R;
                            p[3] = spriteColor > 0 ? (byte) 255 : (byte) 0;
                            p += 4;
                        }
                        p += nOffset;
                    }
                }
                bmp.UnlockBits(bmData);
                var memoryStream = new MemoryStream();
                bmp.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0;
                zip.AddStream(ZipStorer.Compression.Deflate, fileName + Globals.TextureImageFormat, memoryStream, styleData.OriginalDateTime, string.Empty);
                memoryStream.Close();
            }
        }

        private static void SaveDeltas(StyleData styleData, ZipStorer zip, CancellableContext context)
        {
            const uint palette = 31; //for now
            for (int i = 0; i < styleData.DeltaIndexes.Length; i++)
            {
                for (uint j = 0; j < styleData.DeltaIndexes[i].DeltaSize.Count; j++)
                {
                    if (context.IsCancelling)
                        return;
                    SaveDelta(styleData, styleData.DeltaIndexes[i].Sprite, palette, j, zip, styleData.DeltaIndexes[i].Sprite + "_" + j + Globals.TextureImageFormat);
                }
            }
        }

        private static void SaveDelta(StyleData styleData, int spriteId, uint palette, uint deltaId, ZipStorer zip, string fileName)
        {
            var offset = 0;
            foreach (var deltaIndex in styleData.DeltaIndexes)
            {
                if (deltaIndex.Sprite == spriteId)
                {
                    var spriteEntry = styleData.SpriteEntries[spriteId];
                    if (deltaIndex.DeltaSize.Count > 0)
                    {
                        for (var i = 0; i < deltaId; i++)
                            offset += deltaIndex.DeltaSize[i];

                        using (Bitmap bmp = new Bitmap(spriteEntry.Width, spriteEntry.Height))
                        {
                            var pos = 0;
                            var recordLen = 0;
                            var deltaLen = offset + deltaIndex.DeltaSize[(int) deltaId];
                            while (offset < deltaLen)
                            {
                                pos = BitConverter.ToUInt16(styleData.DeltaData, offset) + pos + recordLen;
                                var x = pos%256;
                                var y = pos/256;
                                offset += 2;
                                recordLen = styleData.DeltaData[offset];
                                offset++;
                                for (var i = 0; i < recordLen; i++)
                                {
                                    var color = styleData.Palettes[palette].Colors[styleData.DeltaData[offset]];
                                    var imagePosX = x + i;
                                    var imagePosY = y;
                                    bmp.SetPixel(imagePosX, imagePosY, Color.FromArgb(255, color.R, color.G, color.B));
                                    offset++;
                                }
                            }

                            var memoryStream = new MemoryStream();
                            bmp.Save(memoryStream, ImageFormat.Png);
                            memoryStream.Position = 0;
                            zip.AddStream(ZipStorer.Compression.Deflate, fileName + Globals.TextureImageFormat, memoryStream, styleData.OriginalDateTime, string.Empty);
                            memoryStream.Close();
                        }
                    }
                }
                else
                {
                    offset += deltaIndex.DeltaSize.Sum();
                }
            }
        }

        protected virtual void OnConvertStyleFileProgressChanged(ProgressMessageChangedEventArgs e)
        {
            if (ConvertStyleFileProgressChanged != null)
                ConvertStyleFileProgressChanged(this, e);
        }

        protected virtual void OnConvertStyleFileCompleted(AsyncCompletedEventArgs e)
        {
            if (ConvertStyleFileCompleted != null)
                ConvertStyleFileCompleted(this, e);
        }
    }
}
