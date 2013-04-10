// GTA2.NET
// 
// File: Style.cs
// Created: 21.02.2013
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
        //private ushort[] PaletteIndexes;
        //private uint[] PhysicalPalettes;
        //private PaletteBase paletteBase;
        //private byte[] tileData;
        //private byte[] spriteData;
        //private SpriteEntry[] spriteEntries;
        //private SpriteBase spriteBase;

        //private FontBase fontBase;

        //private ObjectInfo[] objectInfos;
        //private List<Delta> deltas;
        //private List<Surface> Surfaces;

        public string StylePath { get; private set; }

        //public Dictionary<int, CarInfo> CarInfos { get; private set; }

        //private readonly Dictionary<int, List<int>> _carSprites; //Helper variable to see which sprites are used by more than one model.

        public event EventHandler<ProgressMessageChangedEventArgs> ConvertStyleFileProgressChanged;
        public event AsyncCompletedEventHandler ConvertStyleFileCompleted;

        private delegate void ConvertStyleFileDelegate(string styleFile, bool extractGraphics, CancellableContext context, out bool cancelled);
        private readonly object _sync = new object();
        public bool IsBusy { get; private set; }
        private CancellableContext _convertStyleFileContext;
        private readonly object _syncTextureAtlasFinished = new object();
        private readonly List<TextureAtlas> _runningAtlas = new List<TextureAtlas>();
        private readonly Dictionary<TextureAtlas, MemoryStream> _memoryStreams = new Dictionary<TextureAtlas, MemoryStream>();
        private static readonly AutoResetEventValueExchange<bool> WaitHandle = new AutoResetEventValueExchange<bool>(false);

        //public Style()
        //{
            //CarInfos = new Dictionary<int, CarInfo>();
            //deltas = new List<Delta>();
            //Surfaces = new List<Surface>();
            //_carSprites = new Dictionary<int, List<int>>();
        //}

        public IAsyncResult ReadFromFileAsync(string stylePath)
        {
            var worker = new ConvertStyleFileDelegate(ReadFromFile);
            var completedCallback = new AsyncCallback(ConversionCompletedCallback);

            lock (_sync)
            {
                if (IsBusy)
                    throw new InvalidOperationException("The control is currently busy.");

                var async = AsyncOperationManager.CreateOperation(null);
                var context = new CancellableContext(async);
                bool cancelled;

                var result = worker.BeginInvoke(stylePath, true, context, out cancelled, completedCallback, async);

                IsBusy = true;
                _convertStyleFileContext = context;
                return result;
            }
        }

        public void ReadFromFile(string stylePath)
        {
            var context = new CancellableContext(null);
            bool cancelled;
            ReadFromFile(stylePath, false, context, out cancelled);
        }

        private void ReadFromFile(string stylePath, bool extractGraphics, CancellableContext context, out bool cancelled)
        {
            cancelled = false;

            var paletteIndexes = new ushort[] { };
            var physicalPalettes = new uint[] { };
            var paletteBase = new PaletteBase();
            var tileData = new byte[] { };
            var spriteData = new byte[] { };
            var spriteEntries = new SpriteEntry[] { };
            var spriteBase = new SpriteBase();
            var fontBase = new FontBase();

            var carInfos = new Dictionary<int, CarInfo>();
            var carSprites = new Dictionary<int, List<int>>(); //Helper variable to see which sprites are used by more than one model.

            BinaryReader reader = null;
            try
            {
                if (!File.Exists(stylePath))
                    throw new FileNotFoundException("Style File not found!", stylePath);
                StylePath = stylePath;
                System.Diagnostics.Debug.WriteLine("Reading style file " + stylePath);
                var stream = new FileStream(stylePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                reader = new BinaryReader(stream);
                System.Text.Encoding encoder = System.Text.Encoding.ASCII;
                reader.ReadBytes(4); //GBMP
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
                    //var eArgs = new ProgressMessageChangedEventArgs(0, chunkType, null);
                    //context.Async.Post(e => OnConvertStyleFileProgressChanged((ProgressMessageChangedEventArgs) e), eArgs);

                    switch (chunkType)
                    {
                        case "TILE": //Tiles
                            if (extractGraphics)
                                tileData = ReadTiles(reader, chunkSize);
                            else
                                reader.ReadBytes(chunkSize);
                            break;
                        case "PPAL": //Physical Palette
                            if (extractGraphics)
                                physicalPalettes = ReadPhysicalPalette(reader, chunkSize);
                            else
                                reader.ReadBytes(chunkSize);
                            break;
                        case "SPRB": //Sprite Bases
                            if (extractGraphics)
                                spriteBase = ReadSpriteBases(reader);
                            else
                                reader.ReadBytes(chunkSize);
                            break;
                        case "PALX": //Palette Index
                            if (extractGraphics)
                                paletteIndexes = ReadPaletteIndexes(reader, chunkSize);
                            else
                                reader.ReadBytes(chunkSize);
                            break;
                        case "OBJI": //Map Objects
                            ReadMapObjects(reader, chunkSize);
                            break;
                        case "FONB": //Font Base
                            if (extractGraphics)
                                fontBase = ReadFonts(reader, spriteBase.Font);
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
                            carInfos = ReadCars(reader, chunkSize, carSprites);
                            break;
                        case "SPRG": //Sprite Graphics
                            if (extractGraphics)
                                spriteData = ReadSpritesGraphics(reader, chunkSize);
                            else
                                reader.ReadBytes(chunkSize);
                            break;
                        case "SPRX": //Sprite Index
                            if (extractGraphics)
                                spriteEntries = ReadSpriteIndex(reader, chunkSize);
                            else
                                reader.ReadBytes(chunkSize);
                            break;
                        case "PALB": //Palette Base
                            if (extractGraphics)
                                paletteBase = ReadPaletteBase(reader);
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
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            if (!extractGraphics)
                return;

            var styleFile = Path.GetFileNameWithoutExtension(StylePath);
            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream();
                using (var zip = ZipStorer.Create(memoryStream, string.Empty))
                {
                    if (context.IsCancelling)
                    {
                        cancelled = true;
                        return;
                    }
                    SaveTiles(tileData, physicalPalettes, paletteIndexes, zip, context);
                    if (context.IsCancelling)
                    {
                        cancelled = true;
                        return;

                    }
                    SaveSprites(spriteData, physicalPalettes, paletteIndexes, paletteBase, spriteBase, carInfos, carSprites, spriteEntries, zip, context);
                }
                memoryStream.Position = 0;
                using (var stream = new FileStream(Globals.GraphicsSubDir + "\\" + styleFile + ".zip", FileMode.Create, FileAccess.Write))
                {
                    var bytes = new byte[memoryStream.Length];
                    memoryStream.Read(bytes, 0, (int) memoryStream.Length);
                    stream.Write(bytes, 0, bytes.Length);
                }
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return;
                }

                var zipStream1 = new MemoryStream(memoryStream.ToArray());
                var zipStream2 = new MemoryStream(memoryStream.ToArray());

                TextureAtlas atlas = CreateTextureAtlas<TextureAtlasTiles>(ZipStorer.Open(zipStream1, FileAccess.Read), styleFile + "_" + Globals.TilesSuffix.ToLower());
                _memoryStreams.Add(atlas, zipStream1);
                _runningAtlas.Add(atlas);
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return;
                }

                atlas = CreateTextureAtlas<TextureAtlasSprites>(ZipStorer.Open(zipStream2, FileAccess.Read), styleFile + "_" + Globals.SpritesSuffix.ToLower());
                _memoryStreams.Add(atlas, zipStream2);
                _runningAtlas.Add(atlas);
                WaitHandle.WaitOne();
                cancelled = WaitHandle.Value;
            }
            finally
            {
                if (memoryStream != null)
                    memoryStream.Dispose();

                //Clean-up
                //Array.Clear(PaletteIndexes, 0, PaletteIndexes.Length);
                //Array.Clear(PhysicalPalettes, 0, PhysicalPalettes.Length);
                //Array.Clear(tileData, 0, tileData.Length);
                //Array.Clear(spriteData, 0, spriteData.Length);
                //Array.Clear(objectInfos, 0, objectInfos.Length);
                //CarStyleInfos.Clear();
                //_carSprites.Clear();
                //deltas.Clear();
                //Surfaces.Clear();

                GC.Collect();
            }
        }

        public T CreateTextureAtlas<T>(ZipStorer inputZip, string outputFile) where T : TextureAtlas, new()
        {
            var args = new object[2];
            args[0] = outputFile + Globals.TextureImageFormat;
            //args[0] = Globals.GraphicsSubDir + Path.DirectorySeparatorChar + outputFile + Globals.TextureImageFormat;
            args[1] = inputZip;
            var atlas = (T) Activator.CreateInstance(typeof (T), args);
            atlas.BuildTextureAtlasCompleted += AtlasBuildTextureAtlasCompleted;
            atlas.BuildTextureAtlasAsync();
            //atlas.Serialize(Globals.GraphicsSubDir + Path.DirectorySeparatorChar + outputFile + ".xml");
            return atlas;
        }

        private void AtlasBuildTextureAtlasCompleted(object sender, AsyncCompletedEventArgs e)
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

        private void ConversionCompletedCallback(IAsyncResult ar)
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

        private byte[] ReadTiles(BinaryReader reader, int chunkSize)
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
                    Base = new UInt16[256],
                    SpriteBase = new UInt16[256]
                };
            fontBase.Base[0] = spriteBaseFont;
            for (var i = 0; i < fontBase.FontCount; i++)
            {
                fontBase.Base[i] = reader.ReadUInt16();
                if (i > 0)
                    fontBase.SpriteBase[i] = (UInt16)(fontBase.SpriteBase[i - 1] + fontBase.Base[i]);
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

        private static uint[] ReadPhysicalPalette(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading physical palettes...");
            var physicalPalettes = new uint[chunkSize / 4];
            for (var i = 0; i < physicalPalettes.Length; i++)
                physicalPalettes[i] = reader.ReadUInt32();
            return physicalPalettes;
        }

        private static SpriteBase ReadSpriteBases(BinaryReader reader)
        {
            var spriteBase = new SpriteBase();
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
            return spriteBase;
        }

        private Dictionary<int, CarInfo> ReadCars(BinaryReader reader, int chunkSize, Dictionary<int, List<int>> carSprites)
        {
            System.Diagnostics.Debug.WriteLine("Reading car infos...");
            var carInfos = new Dictionary<int, CarInfo>();
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
                if (!carInfos.Keys.Contains(carInfo.Model))
                    carInfos.Add(carInfo.Model, carInfo);
                position = position + 15 + numRemaps + numDoors * 2;
            }
            return carInfos;
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

        private IList<Delta> ReadDeltaIndex(BinaryReader reader, int chunkSize)
        {
            System.Diagnostics.Debug.WriteLine("Reading delta index");
            var deltas = new List<Delta>();
            var position = 0;
            while (position < chunkSize)
            {
                var delta = new Delta { Sprite = reader.ReadUInt16() };
                int deltaCount = reader.ReadByte();
                reader.ReadByte(); //dummy data
                for (var i = 0; i < deltaCount; i++)
                    delta.DeltaSize.Add(reader.ReadUInt16());
                deltas.Add(delta);
                position += 4 + (deltaCount * 2);
            }
            return deltas;
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

        private IList<Surface> ReadSurfaces(BinaryReader reader, int chunkSize)
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

        private static void SaveTiles(byte[] tileData, uint[] physicalPalettes, ushort[] paletteIndexes, ZipStorer zip, CancellableContext asyncContext) //should be ok
        {
            var tilesCount = tileData.Length / (64 * 64);
            for (var i = 0; i < tilesCount; i++)
            {
                if (asyncContext.IsCancelling)
                    return;
                SaveTile(tileData, physicalPalettes, paletteIndexes[i], zip, ref i);
            }
        }

        private static void SaveTile(byte[] tileData, uint[] physicalPalettes, uint vPalette, ZipStorer zip, ref int id) //should be ok
        {
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
                        uint tileColor = tileData[(y + (id / 4) * 64) * 256 + (x + (id % 4) * 64)];
                        var palId = (vPalette / 64) * 256 * 64 + (vPalette % 64) + tileColor * 64;
                        var baseColor = (physicalPalettes[palId]) & 0xFFFFFF;
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
            zip.AddStream(ZipStorer.Compression.Deflate, Globals.TilesSuffix + "/" + id + Globals.TextureImageFormat, memoryStream, DateTime.Now, string.Empty);
            memoryStream.Close();
        }

        private void SaveSprites(byte[] spriteData, uint[] physicalPalettes, ushort[] paletteIndexes, PaletteBase paletteBase, SpriteBase spriteBase, Dictionary<int, CarInfo> carInfos, Dictionary<int, List<int>> carSprites, SpriteEntry[] spriteEntries, ZipStorer zip, CancellableContext asyncContext)
        {
            //cars
            foreach (var carSpriteItem in carSprites)
            {
                if (asyncContext.IsCancelling)
                    return;
                //SaveCarSprite(zip, carSpriteItem.Key, carSpriteItem.Value);
                SaveCarSprite(spriteData, physicalPalettes, paletteIndexes, paletteBase, spriteEntries[carSpriteItem.Key], carInfos, zip, carSpriteItem.Key, carSpriteItem.Value);
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
            const string PATH = "gfx\\sprites\\peds\\";
            var remapPalette = paletteIndexes[paletteBase.Tile + paletteBase.Sprite + paletteBase.CarRemap];
            //int remapPaletteEnd = PaletteIndexes[paletteBase.Tile + paletteBase.Sprite + paletteBase.CarRemap + paletteBase.PedRemap];
            //int remapCount = remapPaletteEnd - remapPalette;
            for (var i = spriteBase.Ped; i < spriteBase.CodeObj; i++)
            {
                var basePalette = paletteIndexes[paletteBase.Tile + i];
                //SaveSpriteRemap(path + "\\" + i + "_-1.png", i, (basePalette));
                SaveSpriteRemap(spriteData, physicalPalettes, spriteEntries[i], basePalette, zip, PATH + "\\" + i + "_-1.png");
                for (var j = 0; j < 53; j++)
                {
                    Directory.CreateDirectory(PATH + j);
                    //SaveSpriteRemap(path + j + "\\" + i + "_" + j + ".png", i, (UInt32)(remapPalette + j));
                    SaveSpriteRemap(spriteData, physicalPalettes, spriteEntries[i], (UInt32)(remapPalette + j), zip, PATH + j + "\\" + i + "_" + j + ".png");
                }
            }
            System.Diagnostics.Debug.WriteLine("Done!");
        }

        private void SaveCarSprite(byte[] spriteData, uint[] physicalPalettes, ushort[] paletteIndexes, PaletteBase paletteBase, SpriteEntry spriteEntry, IDictionary<int, CarInfo> carInfos, ZipStorer zip, int spriteId, IEnumerable<int> modelList)
        {
            var basePalette = paletteIndexes[paletteBase.Tile + spriteId];
            var remapPalette = paletteIndexes[paletteBase.Tile + paletteBase.Sprite];
            //UInt32 remapPalette = PaletteIndexes[paletteBase.Tile + paletteBase.Sprite + spriteID]; //the doc says, I have to add the spriteID, but it gives wrong results...
            foreach (var model in modelList)
            {
                //SaveSpriteRemap(zip, + spriteID, spriteID, basePalette); //this way, models which use a shared sprite, only get's saved once. (spriteID.png)
                SaveSpriteRemap(spriteData, physicalPalettes, spriteEntry, basePalette, zip, spriteId + "_" + model + "_-1"); //in this way, the naming sheme is the same as with remap (spriteID_model_remap.png)
                var remapList = carInfos[model].RemapList;
                foreach (var remapId in remapList)
                {
                    var remapIDhack = remapId;
                    if (remapIDhack >= 35) //hack, remap ids above 35 seems to be broken, this fixes them. Don't ask me why!
                        remapIDhack--;
                    SaveSpriteRemap(spriteData, physicalPalettes, spriteEntry, (uint)(remapPalette + remapIDhack), zip, spriteId + "_" + model + "_" + remapId);
                }
            }
        }

        //private void SaveSpriteRemap(string fileName, int id, UInt32 palette)
        private static void SaveSpriteRemap(byte[] spriteData, uint[] physicalPalettes, SpriteEntry spriteEntry, uint palette, ZipStorer zip, string fileName) //should be ok
        {
            //int width = spriteEntries[id].Width;
            //int height = spriteEntries[id].Height;

            //var bmp = new Bitmap(width, height);

            //var baseX = (int)(spriteEntries[id].Ptr % 256);
            //var baseY = (int)(spriteEntries[id].Ptr / 256);

            //int width = spriteEntry.Width;
            //int height = spriteEntry.Height;

            var bmp = new Bitmap(spriteEntry.Width, spriteEntry.Height);

            var baseX = (int)(spriteEntry.Ptr % 256);
            var baseY = (int)(spriteEntry.Ptr / 256);

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
                        uint spriteColor = spriteData[(baseX + x) + (baseY + y) * 256];
                        var palId = (palette / 64) * 256 * 64 + (palette % 64) + spriteColor * 64;
                        var baseColor = (physicalPalettes[palId]) & 0xFFFFFF;
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
            zip.AddStream(ZipStorer.Compression.Deflate, Globals.SpritesSuffix + "/" + fileName + Globals.TextureImageFormat, memoryStream, DateTime.Now, string.Empty);
            memoryStream.Close();
            //bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
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
