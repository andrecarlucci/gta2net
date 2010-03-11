//Created: 28.01.2010

using System;
using System.Collections.Generic;
using System.Drawing;
using Rectangle=Microsoft.Xna.Framework.Rectangle;

namespace Hiale.GTA2NET.Helper
{
    public struct ImageEntry
    {
        public string FileName;
        public int Width;
        public int Height;
    }

    public class ImageEntryComparer : IComparer<ImageEntry>
    {

        #region IComparer<ImageEntry> Members

        public int Compare(ImageEntry x, ImageEntry y)
        {
            if (x.Height > y.Height)
                return -1;
            if (x.Height < y.Height)
                return 1;
            return 0;
        }

        #endregion
    }

    public class ImageHelper
    {
        private const int MaxTextureDimension = 4096;

        public static TextureAtlasTiles CreateImageDictionary(string[] images, int baseWidth, int baseHeight)
        {
            int columns = (int)Math.Ceiling(Math.Sqrt(images.Length));
            int rows = columns;
            if (columns * (rows - 1) >= images.Length) //sometimes, the last raw is unused, check if space is still enough without last row...
                rows--; //...remove if unused
            if (columns * baseWidth > MaxTextureDimension || rows * baseHeight > MaxTextureDimension) //DirectX9, max texture size
                throw new NotSupportedException("Too many images to create a texture or the sum of all dimensions exceed " + MaxTextureDimension + "x" + MaxTextureDimension + " pixels.");
            SerializableDictionary<int, Microsoft.Xna.Framework.Rectangle> dict = new SerializableDictionary<int, Microsoft.Xna.Framework.Rectangle>();
            Bitmap bmp = new Bitmap(columns * baseWidth, rows * baseHeight);
            Graphics gfx = Graphics.FromImage(bmp);
            
            int currentX = 0;
            int currentY = 0;
            for (int i = 0; i < images.Length; i++)
            {
                Bitmap src = (Bitmap)Image.FromFile(images[i]);

                //Transparency hack
                if (i == 0)
                    src = new Bitmap(64, 64);
                gfx.DrawImageUnscaled(src, new System.Drawing.Point(currentX * baseWidth, currentY * baseHeight));
                src.Dispose();
                Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(currentX * baseWidth, currentY * baseWidth, baseWidth, baseHeight);
                dict.Add(int.Parse(System.IO.Path.GetFileNameWithoutExtension(images[i])), rect); //ToDo: Crash when non-numeric files...
                currentX++;
                if (currentX * baseWidth >= bmp.Width)
                {
                    currentX = 0;
                    currentY++;
                }
            }
            string imagePath = "textures\\tiles.png";
            bmp.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
            return new TextureAtlasTiles(bmp, imagePath, dict);
        }

        //public static TextureAtlas CreateImageDictionary1(string[] images)
        //{
        //    const int maxWidth = 2048;
        //    const int maxHeight = 2048;
        //    int sizeallImages = 0;
        //    List<ImageEntry> entries = new List<ImageEntry>();

        //    for (int i = 0; i < images.Length; i++)
        //    {
        //        Bitmap src = (Bitmap)Image.FromFile(images[i]);
        //        sizeallImages += src.Width * src.Height;
        //        ImageEntry entry = new ImageEntry();
        //        entry.FileName = images[i];
        //        entry.Width = src.Width;
        //        entry.Height = src.Height;
        //        entries.Add(entry);                
        //        src.Dispose();
        //    }
        //    entries.Sort(new ImageEntryComparer());

        //    SerializableDictionary<int, Microsoft.Xna.Framework.Rectangle> dict = new SerializableDictionary<int, Microsoft.Xna.Framework.Rectangle>();
        //    Bitmap bmp = new Bitmap(maxWidth, maxHeight);
        //    Graphics gfx = Graphics.FromImage(bmp);
        //    int currentX = 0;
        //    int currentY = 0;
        //    int heighestItem = 0;
        //    for (int i = 0; i < entries.Count; i++)
        //    {
        //        Bitmap src = (Bitmap)Image.FromFile(entries[i].FileName);
        //        if (src.Height > heighestItem)
        //            heighestItem = src.Height;
        //        if (currentX + src.Width > maxWidth)
        //        {
        //            currentX = 0;
        //            currentY += heighestItem;
        //            heighestItem = src.Height;
        //        }
        //        gfx.DrawImageUnscaled(src, currentX, currentY);
        //        Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(currentX, currentY, src.Width, src.Height);
        //        dict.Add(int.Parse(System.IO.Path.GetFileNameWithoutExtension(entries[i].FileName)), rect); //ToDo: Crash when non-numeric files...
        //        currentX += src.Width;
        //        src.Dispose();
        //    }
        //    const string imagePath = "textures\\sprites.png";
        //    bmp.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
        //    return new TextureAtlas(bmp, imagePath, dict);            
        //}

        public static TextureAtlasSprites CreateImageDictionary(string[] images)
        {
            const int maxWidth = 2048;
            const int maxHeight = 2048;
            int gutterSize = 2;
            int sizeallImages = 0;
            List<ImageEntry> entries = new List<ImageEntry>();

            for (int i = 0; i < images.Length; i++)
            {
                Bitmap src = (Bitmap)Image.FromFile(images[i]);
                sizeallImages += src.Width * src.Height;
                ImageEntry entry = new ImageEntry();
                entry.FileName = images[i];
                entry.Width = src.Width;
                entry.Height = src.Height;
                entries.Add(entry);
                src.Dispose();
            }
            entries.Sort(new ImageEntryComparer());

            SerializableDictionary<SpriteItem, Microsoft.Xna.Framework.Rectangle> dict = new SerializableDictionary<SpriteItem, Microsoft.Xna.Framework.Rectangle>();
            Bitmap bmp = new Bitmap(maxWidth, maxHeight);
            Graphics gfx = Graphics.FromImage(bmp);
            int currentX = 0;
            int currentY = 0;
            int heighestItem = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                Bitmap src = (Bitmap)Image.FromFile(entries[i].FileName);
                if (src.Height > heighestItem)
                    heighestItem = src.Height;
                if (currentX + src.Width + gutterSize > maxWidth)
                {
                    currentX = 0;
                    currentY += heighestItem + gutterSize;
                    heighestItem = src.Height;
                }
                gfx.DrawImageUnscaled(src, currentX, currentY);
                Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(currentX, currentY, src.Width, src.Height);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(entries[i].FileName);
                SpriteItem item;
                try
                {
                    item = ParseFileName(fileName);
                }
                catch (Exception)
                {
                    continue;
                }
                dict.Add(item, rect);
                //dict.Add(int.Parse(System.IO.Path.GetFileNameWithoutExtension(entries[i].FileName)), rect); //ToDo: Crash when non-numeric files...
                currentX += src.Width + gutterSize;
                src.Dispose();
            }
            const string imagePath = "textures\\sprites.png";
            bmp.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
            return new TextureAtlasSprites(bmp, imagePath, dict);
        }

        private static SpriteItem ParseFileName(string fileName)
        {
            try
            {
                SpriteItem item = new SpriteItem();
                string[] parts = fileName.Split('_');
                item.Sprite = int.Parse(parts[0]);
                item.Remap = -1;
                if (parts.Length == 3)
                {
                    item.Model = int.Parse(parts[1]);
                    item.Remap = int.Parse(parts[2]);
                }
                return item;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
