// GTA2.NET
// 
// File: TextureAtlas.cs
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Xml.Serialization;
using System.IO;
using Hiale.GTA2NET.Core.Helper.Threading;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Hiale.GTA2NET.Core.Helper
{
    /// <summary>
    /// Holds information where certail tiles or sprites are put on the image.
    /// </summary>
    [Serializable]
    public abstract class TextureAtlas : IDisposable
    {
        protected class ImageEntry
        {
            public int Index;
            public string FileName;
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public int ZipEntryIndex;
            public int SameSpriteIndex;
        }

        protected class ImageEntryComparer : IComparer<ImageEntry>
        {
            public bool CompareSize { get; set; }

            public int Compare(ImageEntry x, ImageEntry y)
            {
                if (CompareSize)
                {
                    var xSize = x.Height * 1024 + x.Width;
                    var ySize = y.Height * 1024 + y.Width;
                    return ySize.CompareTo(xSize);
                }
                return x.Index.CompareTo(y.Index);
            }
        }

        public event AsyncCompletedEventHandler BuildTextureAtlasCompleted;

        private delegate void BuildTextureAtlasDelegate(CancellableContext context, out bool cancelled);
        private readonly object _sync = new object();
        [XmlIgnore]
        public bool IsBusy { get; private set; }
        private CancellableContext _buildTextureAtlasContext;

        /// <summary>
        /// Image with all the tiles or sprites on it.
        /// </summary>
        [XmlIgnore]
        public Image Image { get; protected set; }

        /// <summary>
        /// Path to image file, used by serialization
        /// </summary>
        public string ImagePath { get; set; }

        [XmlIgnore]
        public ZipStorer ZipStore { get; protected set; }

        protected string ImageDirName;

        protected List<ZipStorer.ZipFileEntry> ZipEntries;

        protected Dictionary<UInt32, int> CrcDictionary; //Helper list to find duplicate images.

        protected Graphics Graphics;

        protected TextureAtlas()
        {
            //needed by xml serializer
            CrcDictionary = new Dictionary<UInt32, int>();
        }

        protected TextureAtlas(string imagePath, ZipStorer zipStore) : this()
        {
            ImagePath = imagePath;
            ZipStore = zipStore;
        }

        protected List<ImageEntry> CreateImageEntries(CancellableContext context, out bool cancelled)
        {
            cancelled = false;
            var entries = new List<ImageEntry>();
            CrcDictionary.Clear();
            ZipEntries = ZipStore.ReadCentralDir();
            for (var i = 0; i < ZipEntries.Count; i++)
            {
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return null;
                }
                if (!ZipEntries[i].FilenameInZip.StartsWith(ImageDirName))
                    continue;
                var source = GetBitmapFromZip(ZipStore, ZipEntries[i]);
                var entry = new ImageEntry();
                if (!CrcDictionary.ContainsKey(ZipEntries[i].Crc32))
                    CrcDictionary.Add(ZipEntries[i].Crc32, i);
                else
                    entry.SameSpriteIndex = CrcDictionary[ZipEntries[i].Crc32];
                entry.Index = i;
                entry.FileName = ParsePath(ZipEntries[i].FilenameInZip);
                entry.Width = source.Width + 2;  // Include a single pixel padding around each sprite, to avoid filtering problems if the sprite is scaled or rotated.
                entry.Height = source.Height + 2;
                entry.ZipEntryIndex = i;
                entries.Add(entry);
                source.Dispose();
            }
            return entries;
        }

        protected static Bitmap GetBitmapFromZip(ZipStorer zipStore, ZipStorer.ZipFileEntry zipFileEntry)
        {
            var memoryStream = new MemoryStream((int)zipFileEntry.FileSize);
            zipStore.ExtractFile(zipFileEntry, memoryStream);
            memoryStream.Position = 0;
            var bmp = (Bitmap)Image.FromStream(memoryStream);
            memoryStream.Close();
            return bmp;
        }

        protected static void FindFreeSpace(List<ImageEntry> entries, ref int outputWidth, ref int outputHeight, CancellableContext context, out bool cancelled)
        {
            cancelled = false;
            outputWidth = GuessOutputWidth(entries);
            outputHeight = 0;

            // Choose positions for each sprite, one at a time.
            for (var i = 0; i < entries.Count; i++)
            {
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return;
                }
                if (entries[i].SameSpriteIndex > 0)
                    continue;
                PositionSprite(entries, i, outputWidth, context, out cancelled);
                if (cancelled)
                    return;
                outputHeight = Math.Max(outputHeight, entries[i].Y + entries[i].Height);
            }
        }

        protected void CreateOutputBitmap(int width, int height)
        {
            Image = new Bitmap(width, height);
            Graphics = Graphics.FromImage(Image);
        }

        protected Rectangle PaintAndGetRectangle(ImageEntry entry)
        {
            var source = GetBitmapFromZip(ZipStore, ZipEntries[entry.ZipEntryIndex]);
            Graphics.DrawImageUnscaled(source, entry.X + 1, entry.Y + 1);
            source.Dispose();
            return new Rectangle(entry.X + 1, entry.Y + 1, entry.Width - 2, entry.Height - 2);
        }

        public virtual void BuildTextureAtlasAsync()
        {
            var worker = new BuildTextureAtlasDelegate(BuildTextureAtlas);
            var completedCallback = new AsyncCallback(ConversionCompletedCallback);

            lock (_sync)
            {
                if (IsBusy)
                    throw new InvalidOperationException("The control is currently busy.");

                var async = AsyncOperationManager.CreateOperation(null);
                var context = new CancellableContext(async);
                bool cancelled;

                worker.BeginInvoke(context, out cancelled, completedCallback, async);

                IsBusy = true;
                _buildTextureAtlasContext = context;
            }
        }

        public abstract void BuildTextureAtlas();

        protected abstract void BuildTextureAtlas(CancellableContext context, out bool cancel);

        private void ConversionCompletedCallback(IAsyncResult ar)
        {
            var worker = (BuildTextureAtlasDelegate)((AsyncResult)ar).AsyncDelegate;
            var async = (AsyncOperation)ar.AsyncState;
            bool cancelled;

            // finish the asynchronous operation
            worker.EndInvoke(out cancelled, ar);

            // clear the running task flag
            lock (_sync)
            {
                IsBusy = false;
                _buildTextureAtlasContext = null;
            }

            // raise the completed event
            var completedArgs = new AsyncCompletedEventArgs(null, cancelled, null);
            async.PostOperationCompleted(e => OnBuildTextureAtlasCompleted((AsyncCompletedEventArgs)e), completedArgs);
        }

        public void CancelBuildTextureAtlas()
        {
            lock (_sync)
            {
                if (_buildTextureAtlasContext != null)
                    _buildTextureAtlasContext.Cancel();
            }
        }

        /// <summary>
        /// Heuristic guesses what might be a good output width for a list of sprites.
        /// </summary>
        protected static int GuessOutputWidth(ICollection<ImageEntry> entries)
        {
            // Gather the widths of all our sprites into a temporary list.
            var widths = entries.Select(entry => entry.Width).ToList();

            // Sort the widths into ascending order.
            widths.Sort();

            // Extract the maximum and median widths.
            var maxWidth = widths[widths.Count - 1];
            var medianWidth = widths[widths.Count / 2];

            // Heuristic assumes an NxN grid of median sized sprites.
            var width = medianWidth * (int)Math.Round(Math.Sqrt(entries.Count));

            // Make sure we never choose anything smaller than our largest sprite.
            return Math.Max(width, maxWidth);
        }

        /// <summary>
        /// Works out where to position a single sprite.
        /// </summary>
       protected static void PositionSprite(List<ImageEntry> entries, int index, int outputWidth, CancellableContext context, out bool cancelled)
        {
            cancelled = false;

            var x = 0;
            var y = 0;

            while (true)
            {
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return;
                }

                // Is this position free for us to use?
                var intersects = FindIntersectingSprite(entries, index, x, y, context, out cancelled);
                if (cancelled)
                    return;

                if (intersects < 0)
                {
                    entries[index].X = x;
                    entries[index].Y = y;

                    return;
                }

                // Skip past the existing sprite that we collided with.
                x = entries[intersects].X + entries[intersects].Width;

                // If we ran out of room to move to the right,
                // try the next line down instead.
                if (x + entries[index].Width > outputWidth)
                {
                    x = 0;
                    y++;
                }
            }
        }

        /// <summary>
        /// Checks if a proposed sprite position collides with anything
        /// that we already arranged.
        /// </summary>
        protected static int FindIntersectingSprite(List<ImageEntry> entries, int index, int x, int y, CancellableContext context, out bool cancelled)
        {
            cancelled = false;

            var w = entries[index].Width;
            var h = entries[index].Height;

            for (var i = 0; i < index; i++)
            {
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return -1;
                }

                if (entries[i].X >= x + w)
                    continue;

                if (entries[i].X + entries[i].Width <= x)
                    continue;

                if (entries[i].Y >= y + h)
                    continue;

                if (entries[i].Y + entries[i].Height <= y)
                    continue;

                return i;
            }

            return -1;
        }

        private static string ParsePath(string path)
        {
            var pos = path.LastIndexOf('/');
            return path.Substring(pos + 1, path.Length - pos - Globals.TextureImageFormat.Length - 1);
        }

        public void Serialize(string path)
        {
            TextWriter textWriter = new StreamWriter(path);
            var serializer = new XmlSerializer(GetType());
            serializer.Serialize(textWriter, this);
            textWriter.Close();
        }

        public static T Deserialize<T>(string path) where T: TextureAtlas
        {
            TextReader textReader = new StreamReader(path);
            var deserializer = new XmlSerializer(typeof (T));
            var atlas = (T)deserializer.Deserialize(textReader);
            textReader.Close();
            return atlas;
        }

        protected virtual void OnBuildTextureAtlasCompleted(AsyncCompletedEventArgs e)
        {
            if (BuildTextureAtlasCompleted != null)
                BuildTextureAtlasCompleted(this, e);
        }

        /// <summary>
        /// Disposes the image when not needed anymore.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (Image != null)
                    Image.Dispose();
                if (Graphics != null)
                    Graphics.Dispose();
            }
            catch (Exception) { }
        }

    }

    public class TextureAtlasTiles : TextureAtlas
    {
        public SerializableDictionary<int, Rectangle> TileDictionary { get; set; }

        public TextureAtlasTiles()
        {
            //this constructor is needed by xml serializer
        }

        public TextureAtlasTiles(string imagePath, ZipStorer zipStore) : base(imagePath, zipStore)
        {
            ImageDirName = Globals.TilesSuffix + "/";
        }

        public override void BuildTextureAtlas()
        {
            var context = new CancellableContext(null);
            bool cancelled;
            BuildTextureAtlas(context, out cancelled);
        }

        protected override void BuildTextureAtlas(CancellableContext context, out bool cancelled)
        {
            cancelled = false;
            var entries = CreateImageEntries(context, out cancelled);
            if (cancelled)
                return;
            var outputWidth = GuessOutputWidth(entries);
            var outputHeight = 0;
            FindFreeSpace(entries, ref outputWidth, ref outputHeight, context, out cancelled);
            CreateOutputBitmap(outputWidth, outputHeight);
            TileDictionary = new SerializableDictionary<int, Rectangle>();
            foreach (var entry in entries)
            {
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return;
                }
                var rect = entry.SameSpriteIndex == 0 ? PaintAndGetRectangle(entry) : TileDictionary[entry.SameSpriteIndex];
                try
                {
                    var index = int.Parse(entry.FileName);
                    TileDictionary.Add(index, rect);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            Image.Save(Globals.GraphicsSubDir + Path.DirectorySeparatorChar + ImagePath, ImageFormat.Png);
        }
    }

    public class TextureAtlasSprites : TextureAtlas
    {
        public SerializableDictionary<SpriteItem, Rectangle> SpriteDictionary { get; set; }

        private readonly Dictionary<int, SpriteItem> _duplicateDictionary; //Helper list to find duplicate images.

        public TextureAtlasSprites()
        {
            //this constructor is needed by xml serializer
        }

        public TextureAtlasSprites(string imagePath, ZipStorer zipStore) : base(imagePath, zipStore)
        {
            ImageDirName = Globals.SpritesSuffix + "/";
            _duplicateDictionary = new Dictionary<int, SpriteItem>();
        }

        public override void BuildTextureAtlas()
        {
            var context = new CancellableContext(null);
            bool cancelled;
            BuildTextureAtlas(context, out cancelled);
        }

        protected override void BuildTextureAtlas(CancellableContext context, out bool cancelled)
        {
            cancelled = false;

            var entries = CreateImageEntries(context, out cancelled);

            // Sort so the largest sprites get arranged first.
            var comparer = new ImageEntryComparer {CompareSize = true};
            entries.Sort(comparer);

            var outputWidth = 0;
            var outputHeight = 0;
            FindFreeSpace(entries, ref outputWidth, ref outputHeight, context, out cancelled);
            if (cancelled)
                return;

            // Sort the sprites back into index order.
            comparer.CompareSize = false;
            entries.Sort(comparer);

            CreateOutputBitmap(outputWidth, outputHeight);
            SpriteDictionary = new SerializableDictionary<SpriteItem, Rectangle>();
            foreach (var entry in entries)
            {
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return;
                }
                var rect = entry.SameSpriteIndex == 0 ? PaintAndGetRectangle(entry) : SpriteDictionary[_duplicateDictionary[entry.SameSpriteIndex]];
                var fileName = entry.FileName;
                SpriteItem item;
                try
                {
                    item = ParseFileName(fileName);
                }
                catch (Exception)
                {
                    continue;
                }
                _duplicateDictionary.Add(entry.Index, item);
                SpriteDictionary.Add(item, rect);
            }
            Image.Save(Globals.GraphicsSubDir + Path.DirectorySeparatorChar + ImagePath, ImageFormat.Png);
        }

        private static SpriteItem ParseFileName(string fileName)
        {
            var item = new SpriteItem();
            var parts = fileName.Split('_');
            item.Sprite = int.Parse(parts[0]);
            item.Remap = -1;
            if (parts.Length == 3)
            {
                item.Model = int.Parse(parts[1]);
                item.Remap = int.Parse(parts[2]);
            }
            return item;
        }
    }
}
