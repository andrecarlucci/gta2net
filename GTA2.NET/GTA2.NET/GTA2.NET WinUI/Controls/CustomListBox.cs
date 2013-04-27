// GTA2.NET
// 
// File: CustomListBox.cs
// Created: 26.04.2013
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Hiale.GTA2NET.Core.Helper;

namespace Hiale.GTA2NET.WinUI.Controls
{
    public class CustomListBox : ListBox
    {
        public Dictionary<int, SpriteItem> SpriteAtlas { get; set; }
        public Image SpriteImage { get; set; }

        public CustomListBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = 68;

        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0 && e.Index < Items.Count)
            {
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var item = SpriteAtlas[e.Index];
                if (item == null)
                    return;
                var targetWidth = item.Rectangle.Width;
                var targetHeight = item.Rectangle.Height;
                ScaleImage(ref targetWidth, ref targetHeight);
                e.Graphics.DrawImage(SpriteImage, new Rectangle(e.Bounds.X, e.Bounds.Y, targetWidth, targetHeight), item.Rectangle.X, item.Rectangle.Y, item.Rectangle.Width, item.Rectangle.Height, GraphicsUnit.Pixel);
                e.Graphics.DrawString(item.SpriteId.ToString(CultureInfo.InvariantCulture) + " (" + item.Type + ")", new Font("Arial", 12), new SolidBrush(Color.Black), e.Bounds.X + 64, e.Bounds.Y);
            }
            e.DrawFocusRectangle();
        }

        public static void ScaleImage(ref int width, ref int height)
        {
            const int max = 64;
            var ratioX = (double)max / width;
            var ratioY = (double)max / height;
            var ratio = Math.Min(ratioX, ratioY);
            width = (int)(width * ratio);
            height = (int)(height * ratio);
        }
    }
}
