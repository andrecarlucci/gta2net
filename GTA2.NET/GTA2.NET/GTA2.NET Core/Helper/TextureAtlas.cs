// GTA2.NET
// 
// File: TextureAtlas.cs
// Created: 28.01.2010
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
using System.Runtime.InteropServices;
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
            public int SameImageIndex;
        }

        //Based on http://www.blackpawn.com/texts/lightmaps/
        protected class Node
        {
            public Rectangle Rectangle;
            private readonly Node[] _child;
            private int _imageWidth;
            private int _imageHeight;

            public Node(int x, int y, int width, int height)
            {
                Rectangle = new Rectangle(x, y, width, height);
                _child = new Node[2];
                _child[0] = null;
                _child[1] = null;
                _imageWidth = -1;
                _imageHeight = -1;
            }

            private bool IsLeaf()
            {
                return _child[0] == null && _child[1] == null;
            }

            public Node Insert(int imageWidth, int imageHeight)
            {
                if (!IsLeaf())
                {
                    var newNode = _child[0].Insert(imageWidth, imageHeight);
                    return newNode ?? _child[1].Insert(imageWidth, imageHeight);
                }
                if (_imageWidth >= 0 && _imageHeight >= 0)
                    return null;
                if (imageWidth > Rectangle.Width || imageHeight > Rectangle.Height)
                    return null;

                if (imageWidth == Rectangle.Width && imageHeight == Rectangle.Height)
                {
                    _imageWidth = imageWidth;
                    _imageHeight = imageHeight;
                    return this;
                }

                var dw = Rectangle.Width - imageWidth;
                var dh = Rectangle.Height - imageHeight;

                if (dw > dh)
                {
                    _child[0] = new Node(Rectangle.X, Rectangle.Y, imageWidth, Rectangle.Height);
                    _child[1] = new Node(Rectangle.X + imageWidth, Rectangle.Y, Rectangle.Width - imageWidth, Rectangle.Height);
                }
                else
                {
                    _child[0] = new Node(Rectangle.X, Rectangle.Y, Rectangle.Width, imageHeight);
                    _child[1] = new Node(Rectangle.X, Rectangle.Y + imageHeight, Rectangle.Width, Rectangle.Height - imageHeight);
                }
                return _child[0].Insert(imageWidth, imageHeight);
            }
        }

        protected class ImageEntryComparer : IComparer<ImageEntry>
        {
            public bool CompareSize { get; set; }

            public int Compare(ImageEntry x, ImageEntry y)
            {
                if (CompareSize)
                {
                    var xSize = x.Height*1024 + x.Width;
                    var ySize = y.Height*1024 + y.Width;
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

        /// <summary>
        /// Padding to eliminate texture bleeding, it SEEMS that XNA 4.0 fixed it, so it's not needed anymore?
        /// </summary>
        public int Padding { get; set; }

        [XmlIgnore]
        public ZipStorer ZipStore { get; protected set; }

        protected List<ZipStorer.ZipFileEntry> ZipEntries;

        protected Dictionary<uint, int> CrcDictionary; //Helper list to find duplicate images.

        protected Graphics Graphics;

        protected TextureAtlas()
        {
            //needed by xml serializer
            Padding = 1;
            CrcDictionary = new Dictionary<uint, int>();
        }

        protected TextureAtlas(string imagePath, ZipStorer zipStore)
            : this()
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
                //if (!ZipEntries[i].FilenameInZip.StartsWith(directory))
                //    continue;
                var source = GetBitmapFromZip(ZipStore, ZipEntries[i]);
                var entry = new ImageEntry();
                if (!CrcDictionary.ContainsKey(ZipEntries[i].Crc32))
                    CrcDictionary.Add(ZipEntries[i].Crc32, i);
                else
                    entry.SameImageIndex = CrcDictionary[ZipEntries[i].Crc32];
                entry.Index = i;
                entry.FileName = ParsePath(ZipEntries[i].FilenameInZip);
                entry.Width = source.Width + 2*Padding; // Include a single pixel padding around each sprite, to avoid filtering problems if the sprite is scaled or rotated.
                entry.Height = source.Height + 2*Padding;
                entry.ZipEntryIndex = i;
                entries.Add(entry);
                source.Dispose();
            }
            return entries;
        }

        protected static Bitmap GetBitmapFromZip(ZipStorer zipStore, ZipStorer.ZipFileEntry zipFileEntry)
        {
            var memoryStream = new MemoryStream((int) zipFileEntry.FileSize);
            zipStore.ExtractFile(zipFileEntry, memoryStream);
            memoryStream.Position = 0;
            var bmp = (Bitmap) Image.FromStream(memoryStream);
            memoryStream.Close();
            return bmp;
        }

        protected void CreateOutputBitmap(int width, int height)
        {
            Image = new Bitmap(width, height);
            Graphics = Graphics.FromImage(Image);
        }

        protected CompactRectangle Place(ImageEntry entry)
        {
            var source = GetBitmapFromZip(ZipStore, ZipEntries[entry.ZipEntryIndex]);
            var rect = Place(entry, source);
            source.Dispose();
            return rect;
        }

        protected CompactRectangle Place(ImageEntry entry, Bitmap bmp)
        {
            Graphics.DrawImageUnscaled(bmp, entry.X + Padding, entry.Y + Padding);
            return new CompactRectangle(entry.X + Padding, entry.Y + Padding, entry.Width - 2 * Padding, entry.Height - 2 * Padding);
        }

        public virtual void BuildTextureAtlasAsync()
        {
            var worker = new BuildTextureAtlasDelegate(BuildTextureAtlas);
            var completedCallback = new AsyncCallback(BuildTextureAtlasCompleteCallback);

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

        public void BuildTextureAtlas()
        {
            var context = new CancellableContext(null);
            bool cancelled;
            BuildTextureAtlas(context, out cancelled);
        }

        protected abstract void BuildTextureAtlas(CancellableContext context, out bool cancel);

        private void BuildTextureAtlasCompleteCallback(IAsyncResult ar)
        {
            var worker = (BuildTextureAtlasDelegate) ((AsyncResult) ar).AsyncDelegate;
            var async = (AsyncOperation) ar.AsyncState;
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
            async.PostOperationCompleted(e => OnBuildTextureAtlasCompleted((AsyncCompletedEventArgs) e), completedArgs);
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
            //widths.Sort();

            // Extract the maximum and median widths.
            var maxWidth = widths[widths.Count - 1];
            var medianWidth = widths[widths.Count/2];

            // Heuristic assumes an NxN grid of median sized sprites.
            var width = medianWidth*(int) Math.Round(Math.Sqrt(entries.Count));

            // Make sure we never choose anything smaller than our largest sprite.
            width = Math.Max(width, maxWidth);

            return PowerOfTwo(width);
        }

        protected static int GuessOutputHeight(ICollection<ImageEntry> entries, int width)
        {
            var totalArea = entries.Sum(imageEntry => imageEntry.Width*imageEntry.Height);
            var height = (int) Math.Ceiling((float) totalArea/width);
            return PowerOfTwo(height);
        }

        protected static int PowerOfTwo(int minimum)
        {
            uint current;
            var exponent = 0;
            do
            {
                current = (uint) (1 << exponent);
                exponent++;
            } while (current < minimum);
            return (int) current;
        }

        //Based on http://stackoverflow.com/questions/4820212/automatically-trim-a-bitmap-to-minimum-size/4821100#4821100
        public static System.Drawing.Rectangle CalculateTrimRegion(Bitmap source)
        {
            System.Drawing.Rectangle srcRect;
            BitmapData data = null;
            try
            {
                data = source.LockBits(new System.Drawing.Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var buffer = new byte[data.Height * data.Stride];
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);

                int xMin = int.MaxValue,
                    xMax = int.MinValue,
                    yMin = int.MaxValue,
                    yMax = int.MinValue;

                var foundPixel = false;

                // Find xMin
                for (var x = 0; x < data.Width; x++)
                {
                    var stop = false;
                    for (var y = 0; y < data.Height; y++)
                    {
                        var alpha = buffer[y * data.Stride + 4 * x + 3];
                        if (alpha == 0)
                            continue;
                        xMin = x;
                        stop = true;
                        foundPixel = true;
                        break;
                    }
                    if (stop)
                        break;
                }

                // Image is empty...
                if (!foundPixel)
                    return new System.Drawing.Rectangle();

                // Find yMin
                for (var y = 0; y < data.Height; y++)
                {
                    var stop = false;
                    for (var x = xMin; x < data.Width; x++)
                    {
                        var alpha = buffer[y * data.Stride + 4 * x + 3];
                        if (alpha == 0)
                            continue;
                        yMin = y;
                        stop = true;
                        break;
                    }
                    if (stop)
                        break;
                }

                // Find xMax
                for (var x = data.Width - 1; x >= xMin; x--)
                {
                    var stop = false;
                    for (var y = yMin; y < data.Height; y++)
                    {
                        var alpha = buffer[y * data.Stride + 4 * x + 3];
                        if (alpha == 0)
                            continue;
                        xMax = x;
                        stop = true;
                        break;
                    }
                    if (stop)
                        break;
                }

                // Find yMax
                for (var y = data.Height - 1; y >= yMin; y--)
                {
                    var stop = false;
                    for (var x = xMin; x <= xMax; x++)
                    {
                        var alpha = buffer[y * data.Stride + 4 * x + 3];
                        if (alpha == 0)
                            continue;
                        yMax = y;
                        stop = true;
                        break;
                    }
                    if (stop)
                        break;
                }
                srcRect = System.Drawing.Rectangle.FromLTRB(xMin, yMin, xMax + 1, yMax + 1);
            }
            finally
            {
                if (data != null)
                    source.UnlockBits(data);
            }
            return srcRect;
        }

        public static Bitmap TrimBitmap(Bitmap source)
        {
            var srcRect = CalculateTrimRegion(source);
            return source.Clone(srcRect, source.PixelFormat);
        }

        private static string ParsePath(string path)
        {
            var pos = path.LastIndexOf('/');
            return path.Substring(pos + 1, path.Length - pos - Globals.TextureImageFormat.Length - 1);
        }

        public void Serialize(string path)
        {
            var textWriter = new StreamWriter(path);
            var serializer = new XmlSerializer(GetType());
            serializer.Serialize(textWriter, this);
            textWriter.Close();
        }

        public static T Deserialize<T>(string path) where T : TextureAtlas
        {
            var textReader = new StreamReader(path);
            var deserializer = new XmlSerializer(typeof (T));
            var atlas = (T) deserializer.Deserialize(textReader);
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
            catch (Exception)
            {
            }
        }

    }

    public class TextureAtlasTiles : TextureAtlas
    {
        public SerializableDictionary<int, CompactRectangle> TileDictionary { get; set; }

        public TextureAtlasTiles()
        {
            //this constructor is needed by xml serializer
        }

        public TextureAtlasTiles(string imagePath, ZipStorer zipStore) : base(imagePath, zipStore)
        {
            //
        }

        protected override void BuildTextureAtlas(CancellableContext context, out bool cancelled)
        {
            var entries = CreateImageEntries(context, out cancelled);
            if (cancelled)
                return;
            var outputWidth = GuessOutputWidth(entries);
            var outputHeight = GuessOutputHeight(entries, outputWidth);

            var root = new Node(0, 0, outputWidth, outputHeight);

            if (context.IsCancelling)
            {
                cancelled = true;
                return;
            }
            CreateOutputBitmap(outputWidth, outputHeight);
            TileDictionary = new SerializableDictionary<int, CompactRectangle>();
            foreach (var entry in entries)
            {
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return;
                }

                CompactRectangle rect;
                if (entry.SameImageIndex == 0)
                {
                    var node = root.Insert(entry.Width, entry.Height);
                    if (node == null)
                        continue; //no space to put the image, increase the output image?
                    entry.X = node.Rectangle.X;
                    entry.Y = node.Rectangle.Y;
                    rect = Place(entry);
                }
                else
                {
                    rect = TileDictionary[entry.SameImageIndex];
                }
                var index = int.Parse(entry.FileName);
                TileDictionary.Add(index, rect);

                //var node = root.Insert(entry.Width, entry.Height);
                //if (node == null)
                //    continue;
                //entry.X = node.Rectangle.X;
                //entry.Y = node.Rectangle.Y;

                //var rect = entry.SameImageIndex == 0 ? Place(entry) : TileDictionary[entry.SameImageIndex];
                //var index = int.Parse(entry.FileName);
                //TileDictionary.Add(index, rect);
            }
            Image.Save(Globals.GraphicsSubDir + Path.DirectorySeparatorChar + ImagePath, ImageFormat.Png);
            Serialize(Globals.GraphicsSubDir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(ImagePath) + Globals.XmlFormat);

        }
    }

    public class TextureAtlasSprites : TextureAtlas
    {
        public SerializableDictionary<int, SpriteItem> SpriteDictionary { get; set; }

        public TextureAtlasSprites()
        {
            //this constructor is needed by xml serializer
        }

        public TextureAtlasSprites(string imagePath, ZipStorer zipStore, SerializableDictionary<int, SpriteItem> spriteDictionary) : base(imagePath, zipStore)
        {
            SpriteDictionary = spriteDictionary;
        }

        public static void FillSpriteId(SerializableDictionary<int, SpriteItem> spriteDictionary)
        {
            foreach (var spriteItem in spriteDictionary)
                spriteItem.Value.SpriteId = spriteItem.Key;
        }

        protected override void BuildTextureAtlas(CancellableContext context, out bool cancelled)
        {
            var entries = CreateImageEntries(context, out cancelled);
            if (cancelled)
                return;

            // Sort so the largest sprites get arranged first.
            var comparer = new ImageEntryComparer {CompareSize = true};
            entries.Sort(comparer);

            var outputWidth = GuessOutputWidth(entries);
            var outputHeight = GuessOutputHeight(entries, outputWidth);

            if (context.IsCancelling)
            {
                cancelled = true;
                return;
            }

            // Sort the sprites back into index order.
            comparer.CompareSize = false;
            entries.Sort(comparer);

            var root = new Node(0, 0, outputWidth, outputHeight);

            CreateOutputBitmap(outputWidth, outputHeight);
            //SpriteDictionary = new SerializableDictionary<int, SpriteItem>();
            foreach (var entry in entries)
            {
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return;
                }

                CompactRectangle rect;
                if (entry.SameImageIndex == 0)
                {
                    var node = root.Insert(entry.Width, entry.Height);
                    if (node == null)
                        continue; //ToDo: the picture could not be inserted because there were not enough space. Increase the output image?
                    entry.X = node.Rectangle.X;
                    entry.Y = node.Rectangle.Y;
                    rect = Place(entry);
                }
                else
                {
                    rect = SpriteDictionary[entry.SameImageIndex].Rectangle;
                }
                var item = SpriteDictionary[entry.Index];
                item.Rectangle = rect;
            }
            Image.Save(Globals.GraphicsSubDir + Path.DirectorySeparatorChar + ImagePath, ImageFormat.Png);
            Serialize(Globals.GraphicsSubDir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(ImagePath) + Globals.XmlFormat);
        }
    }

    public class TextureAtlasDeltas : TextureAtlas
    {
        public SerializableDictionary<int, DeltaItem> DeltaDictionary { get; set; }

        public TextureAtlasDeltas()
        {
            //this constructor is needed by xml serializer
        }

        public TextureAtlasDeltas(string imagePath, ZipStorer zipStore, SerializableDictionary<int, DeltaItem> deltaDictionary) : base(imagePath, zipStore)
        {
            DeltaDictionary = deltaDictionary;
        }

        public static void FillSpriteId(SerializableDictionary<int, DeltaItem> deltaDictionary)
        {
            foreach (var deltaItem in deltaDictionary)
                deltaItem.Value.SpriteId = deltaItem.Key;
        }

        protected override void BuildTextureAtlas(CancellableContext context, out bool cancelled)
        {
            var entries = CreateImageEntries(context, out cancelled);
            if (cancelled)
                return;

            // Sort so the largest sprites get arranged first.
            var comparer = new ImageEntryComparer {CompareSize = true};
            entries.Sort(comparer);

            var outputWidth = GuessOutputWidth(entries);
            var outputHeight = GuessOutputHeight(entries, outputWidth);

            if (context.IsCancelling)
            {
                cancelled = true;
                return;
            }

            // Sort the sprites back into index order.
            comparer.CompareSize = false;
            entries.Sort(comparer);

            var root = new Node(0, 0, outputWidth, outputHeight);

            CreateOutputBitmap(outputWidth, outputHeight);

            foreach (var entry in entries)
            {
                if (context.IsCancelling)
                {
                    cancelled = true;
                    return;
                }

                var useTrimmedImage = false;

                var bmp = GetBitmapFromZip(ZipStore, ZipEntries[entry.ZipEntryIndex]);
                var drawingRect = CalculateTrimRegion(bmp);
                if ((drawingRect.Width*drawingRect.Height) < (entry.Width*entry.Height))
                    useTrimmedImage = true;

                CompactRectangle rect;
                if (entry.SameImageIndex == 0)
                {
                    if (useTrimmedImage)
                    {
                        entry.Width = drawingRect.Width + 2*Padding;
                        entry.Height = drawingRect.Height + 2*Padding;
                    }

                    //bmp.Dispose();
                    var node = root.Insert(entry.Width, entry.Height); //ToDo: deltas are inserted at the wrong position sometimes.
                    if (node == null)
                    {
                        bmp.Dispose();
                        continue; //ToDo: the picture could not be inserted because there were not enough space. Increase the output image?
                    }
                    entry.X = node.Rectangle.X;
                    entry.Y = node.Rectangle.Y;

                    if (useTrimmedImage)
                    {
                        if (drawingRect.Width*drawingRect.Height > 0)
                        {
                            var trimmedImage = TrimBitmap(bmp);
                            rect = Place(entry, trimmedImage);
                            trimmedImage.Dispose();
                            //ToDo: Change Location relative to the top left position
                        }
                        else
                        {
                            rect = new CompactRectangle();
                        }
                    }
                    else
                    {
                        rect = Place(entry);
                    }
                    bmp.Dispose();
                }
                else
                {
                    bmp.Dispose();
                    var sameEntry = entries[entry.SameImageIndex];
                    int sameSpriteId;
                    int sameDeltaId;
                    ParseFileName(sameEntry.FileName, out sameSpriteId, out sameDeltaId);
                    rect = DeltaDictionary[sameSpriteId].SubItems[sameDeltaId].Rectangle;
                }

                int spriteId;
                int deltaId;
                ParseFileName(entry.FileName, out spriteId, out deltaId);
                DeltaDictionary[spriteId].SubItems[deltaId].Rectangle = rect;
            }
            Image.Save(Globals.GraphicsSubDir + Path.DirectorySeparatorChar + ImagePath, ImageFormat.Png);
            Serialize(Globals.GraphicsSubDir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(ImagePath) + Globals.XmlFormat);
        }

        private static void ParseFileName(string fileName, out int spriteId, out int deltaId)
        {
            var parts = fileName.Split('_');
            spriteId = int.Parse(parts[0]);
            deltaId = int.Parse(parts[1]);
        }

    }
}


