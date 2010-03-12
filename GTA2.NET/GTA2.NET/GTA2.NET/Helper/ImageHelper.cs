//Created: 28.01.2010

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Core.Style;
using Rectangle=Microsoft.Xna.Framework.Rectangle;

namespace Hiale.GTA2NET.Helper
{
    public class ImageEntry
    {
        public int Index;
        public string FileName;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int ZipEntryIndex;
    }

    public class ImageEntryComparer : IComparer<ImageEntry>
    {

        #region IComparer<ImageEntry> Members

        public bool CompareSize { get; set; }

        public int Compare(ImageEntry x, ImageEntry y)
        {
            //if (x.Height > y.Height)
            //    return -1;
            //if (x.Height < y.Height)
            //    return 1;
            //return 0;
            if (CompareSize)
            {
                int xSize = x.Height * 1024 + x.Width;
                int ySize = y.Height * 1024 + y.Width;
                return ySize.CompareTo(xSize);
            }
            return x.Index.CompareTo(y.Index);
        }

        #endregion
    }

    //public class ImageHelper
    //{
        //private const int MaxTextureDimension = 4096;

        //public static TextureAtlasTiles CreateImageDictionary(string[] images, int baseWidth, int baseHeight)
        //{
        //    int columns = (int)Math.Ceiling(Math.Sqrt(images.Length));
        //    int rows = columns;
        //    if (columns * (rows - 1) >= images.Length) //sometimes, the last raw is unused, check if space is still enough without last row...
        //        rows--; //...remove if unused
        //    if (columns * baseWidth > MaxTextureDimension || rows * baseHeight > MaxTextureDimension) //DirectX9, max texture size
        //        throw new NotSupportedException("Too many images to create a texture or the sum of all dimensions exceed " + MaxTextureDimension + "x" + MaxTextureDimension + " pixels.");
        //    SerializableDictionary<int, Microsoft.Xna.Framework.Rectangle> dict = new SerializableDictionary<int, Microsoft.Xna.Framework.Rectangle>();
        //    Bitmap bmp = new Bitmap(columns * baseWidth, rows * baseHeight);
        //    Graphics gfx = Graphics.FromImage(bmp);
            
        //    int currentX = 0;
        //    int currentY = 0;
        //    for (int i = 0; i < images.Length; i++)
        //    {
        //        Bitmap src = (Bitmap)Image.FromFile(images[i]);

        //        //Transparency hack
        //        if (i == 0)
        //            src = new Bitmap(64, 64);
        //        gfx.DrawImageUnscaled(src, new System.Drawing.Point(currentX * baseWidth, currentY * baseHeight));
        //        src.Dispose();
        //        Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(currentX * baseWidth, currentY * baseWidth, baseWidth, baseHeight);
        //        dict.Add(int.Parse(System.IO.Path.GetFileNameWithoutExtension(images[i])), rect); //ToDo: Crash when non-numeric files...
        //        currentX++;
        //        if (currentX * baseWidth >= bmp.Width)
        //        {
        //            currentX = 0;
        //            currentY++;
        //        }
        //    }
        //    string imagePath = "textures\\tiles.png";
        //    bmp.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
        //    //return new TextureAtlasTiles(bmp, imagePath, dict);
        //    return null;
        //}
        
        //public static TextureAtlas CreateImageDictionaryOld(ZipStorer zip, bool sprites)
        //{
        //    const int maxWidth = 4096;
        //    const int maxHeight = 4096;
        //    const int gutterSize = 2;
        //    int sizeAllImages = 0;
        //    List<ImageEntry> entries = new List<ImageEntry>();
        //    List<ZipStorer.ZipFileEntry> zipEntries = zip.ReadCentralDir();
        //    for (int i = 0; i < zipEntries.Count; i++)
        //    {
        //        if (!zipEntries[i].FilenameInZip.StartsWith(Style.TilesZipDir) && !sprites)
        //            continue;
        //        if (!zipEntries[i].FilenameInZip.StartsWith(Style.SpritesZipDir) && sprites)
        //            continue;
        //        MemoryStream memoryStream = new MemoryStream((int)zipEntries[i].FileSize);
        //        zip.ExtractFile(zipEntries[i], memoryStream);
        //        memoryStream.Position = 0;
        //        Bitmap src = (Bitmap)Image.FromStream(memoryStream);
        //        memoryStream.Close();

        //        sizeAllImages += src.Width * src.Height;
        //        ImageEntry entry = new ImageEntry();
        //        entry.FileName = ParsePath(zipEntries[i].FilenameInZip);
        //        entry.Width = src.Width;
        //        entry.Height = src.Height;
        //        entry.ZipEntryIndex = i;
        //        entries.Add(entry);
        //        src.Dispose();
        //    }

        //    SerializableDictionary<int, Rectangle> dictTiles = null;
        //    SerializableDictionary<SpriteItem, Rectangle> dictSprites = null;

        //    if (sprites)
        //    {
        //        entries.Sort(new ImageEntryComparer());
        //        dictSprites = new SerializableDictionary<SpriteItem, Rectangle>();
        //    }
        //    else
        //    {
        //       dictTiles = new SerializableDictionary<int, Rectangle>();
        //    }

        //    Bitmap bmp = new Bitmap(maxWidth, maxHeight);
        //    Graphics gfx = Graphics.FromImage(bmp);
        //    int currentX = 0;
        //    int currentY = 0;
        //    int heighestItem = 0;
        //    for (int i = 0; i < entries.Count; i++)
        //    {
        //        MemoryStream memoryStream = new MemoryStream((int)zipEntries[entries[i].ZipEntryIndex].FileSize);
        //        zip.ExtractFile(zipEntries[entries[i].ZipEntryIndex], memoryStream);
        //        memoryStream.Position = 0;
        //        Bitmap src = (Bitmap)Image.FromStream(memoryStream);
        //        memoryStream.Close();

        //        if (src.Height > heighestItem)
        //            heighestItem = src.Height;
        //        if (currentX + src.Width + gutterSize > maxWidth)
        //        {
        //            currentX = 0;
        //            currentY += heighestItem + gutterSize;
        //            heighestItem = src.Height;
        //        }
        //        gfx.DrawImageUnscaled(src, currentX, currentY);
        //        Rectangle rect = new Rectangle(currentX, currentY, src.Width, src.Height);
        //        if (sprites)
        //        {
        //            string fileName = entries[i].FileName;
        //            SpriteItem item;
        //            try
        //            {
        //                item = ParseFileName(fileName);
        //            }
        //            catch (Exception)
        //            {
        //                continue;
        //            }
        //            dictSprites.Add(item, rect);
        //        }
        //        else
        //        {
        //            try
        //            {
        //                int index = int.Parse(entries[i].FileName);
        //                dictTiles.Add(index, rect);
        //            }
        //            catch (Exception)
        //            {
        //                continue;
        //            }
        //        }
        //        currentX += src.Width + gutterSize;
        //        src.Dispose();
        //    }
        //    const string imagePath = "textures\\sprites.png";
        //    bmp.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
        //    return null;
        //    //if (sprites)
        //    //    return new TextureAtlasSprites(bmp, imagePath, dictSprites);
        //    //else
        //    //    return new TextureAtlasTiles(bmp, imagePath, dictTiles);
        //}


        //private static string ParsePath(string path)
        //{
        //    int pos = path.LastIndexOf('/');
        //    return path.Substring(pos + 1, path.Length - pos - Style.Png.Length - 1);
        //}

        //private static SpriteItem ParseFileName(string fileName)
        //{
        //    try
        //    {
        //        SpriteItem item = new SpriteItem();
        //        string[] parts = fileName.Split('_');
        //        item.Sprite = int.Parse(parts[0]);
        //        item.Remap = -1;
        //        if (parts.Length == 3)
        //        {
        //            item.Model = int.Parse(parts[1]);
        //            item.Remap = int.Parse(parts[2]);
        //        }
        //        return item;
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}
    //}
}
