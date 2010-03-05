using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

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
            else if (x.Height < y.Height)
                return 1;            
            return 0;
        }

        #endregion
    }

    public class ImageHelper
    {
        private const int MaxTextureDimension = 4096;

        public static TextureAtlas CreateImageDictionary(string[] Images, int BaseWidth, int BaseHeight)
        {
            int columns = (int)Math.Ceiling(Math.Sqrt(Images.Length));
            int rows = columns;
            if (columns * (rows - 1) >= Images.Length) //sometimes, the last raw is unused, check if space is still enough without last row...
                rows--; //...remove if unused
            if (columns * BaseWidth > MaxTextureDimension || rows * BaseHeight > MaxTextureDimension) //DirectX9, max texture size
                throw new NotSupportedException("Too many images to create a texture or the sum of all dimensions exceed " + MaxTextureDimension + "x" + MaxTextureDimension + " pixels.");
            SerializableDictionary<int, Microsoft.Xna.Framework.Rectangle> dict = new SerializableDictionary<int, Microsoft.Xna.Framework.Rectangle>();
            Bitmap bmp = new Bitmap(columns * BaseWidth, rows * BaseHeight);
            Graphics gfx = Graphics.FromImage(bmp);
            
            int CurrentX = 0;
            int CurrentY = 0;
            for (int i = 0; i < Images.Length; i++)
            {
                Bitmap src = (Bitmap)Image.FromFile(Images[i]);

                //Transparency hack
                if (i == 0)
                    src = new Bitmap(64, 64);
                gfx.DrawImageUnscaled(src, new System.Drawing.Point(CurrentX * BaseWidth, CurrentY * BaseHeight));
                src.Dispose();
                Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(CurrentX * BaseWidth, CurrentY * BaseWidth, BaseWidth, BaseHeight);
                dict.Add(int.Parse(System.IO.Path.GetFileNameWithoutExtension(Images[i])), rect); //ToDo: Crash when non-numeric files...
                CurrentX++;
                if (CurrentX * BaseWidth >= bmp.Width)
                {
                    CurrentX = 0;
                    CurrentY++;
                }
            }
            string imagePath = "textures\\tiles.png";
            bmp.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
            return new TextureAtlas(bmp, imagePath, dict);
        }

        public static TextureAtlas CreateImageDictionary(string[] Images)
        {
            int MaxWidth = 2048;
            int MaxHeight = 2048;
            int sizeallImages = 0;
            List<ImageEntry> Entries = new List<ImageEntry>();

            for (int i = 0; i < Images.Length; i++)
            {
                Bitmap src = (Bitmap)Image.FromFile(Images[i]);
                sizeallImages += src.Width * src.Height;
                ImageEntry entry = new ImageEntry();
                entry.FileName = Images[i];
                entry.Width = src.Width;
                entry.Height = src.Height;
                Entries.Add(entry);                
                src.Dispose();
            }
            Entries.Sort(new ImageEntryComparer());

            SerializableDictionary<int, Microsoft.Xna.Framework.Rectangle> dict = new SerializableDictionary<int, Microsoft.Xna.Framework.Rectangle>();
            Bitmap bmp = new Bitmap(MaxWidth, MaxHeight);
            Graphics gfx = Graphics.FromImage(bmp);
            int CurrentX = 0;
            int CurrentY = 0;
            int HeighestItem = 0;
            for (int i = 0; i < Entries.Count; i++)
            {
                Bitmap src = (Bitmap)Image.FromFile(Entries[i].FileName);
                if (src.Height > HeighestItem)
                    HeighestItem = src.Height;
                if (CurrentX + src.Width > MaxWidth)
                {
                    CurrentX = 0;
                    CurrentY += HeighestItem;
                    HeighestItem = src.Height;
                }
                gfx.DrawImageUnscaled(src, CurrentX, CurrentY);
                Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(CurrentX, CurrentY, src.Width, src.Height);
                dict.Add(int.Parse(System.IO.Path.GetFileNameWithoutExtension(Entries[i].FileName)), rect); //ToDo: Crash when non-numeric files...
                CurrentX += src.Width;
                src.Dispose();
            }
            string imagePath = "textures\\sprites.png";
            bmp.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
            return new TextureAtlas(bmp, imagePath, dict);            
        }
    }
}
