// GTA2.NET
// 
// File: SpriteForm.cs
// Created: 18.04.2013
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
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Hiale.GTA2NET.Core;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.WinUI.DockWindows;
using WeifenLuo.WinFormsUI.Docking;

namespace Hiale.GTA2NET.WinUI
{
    public partial class SpriteForm : Form
    {
        private readonly SpriteListWindow spriteListWindow;
        private readonly SpritePreviewWindow spritePreviewWindow;
        private readonly SpriteDeltasWindow spriteDeltasWindow;
        private readonly SpriteRemapsWindow spriteRemapsWindow;
        private readonly DeserializeDockContent m_deserializeDockContent;
        private const string LayoutFile = "SpriteFormLayout.xml";


        private SerializableDictionary<int, SpriteItem> spriteAtlas;
        private Image spriteImage;
        private Image deltaImage;

        private readonly string currentPath ;

        public SpriteForm()
        {
            InitializeComponent();

            currentPath = Extensions.CheckDirectorySeparator(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            spriteListWindow = new SpriteListWindow();
            spriteListWindow.SpriteChanged += SpriteListWindowSpriteChanged;
            spritePreviewWindow = new SpritePreviewWindow();
            spriteDeltasWindow = new SpriteDeltasWindow();
            spriteDeltasWindow.CheckedDeltaItemsChanged += SpriteDeltasWindowCheckedDeltaItemsChanged;
            spriteRemapsWindow = new SpriteRemapsWindow();

            m_deserializeDockContent += MDeserializeDockContent;

            var layoutFile = currentPath + LayoutFile;
            try
            {
                dockPanel.LoadFromXml(layoutFile, m_deserializeDockContent);
            }
            catch (Exception)
            {
                var stream = Assembly.GetAssembly(GetType()).GetManifestResourceStream(GetType().Namespace + ".Resources.SpriteFormDefaultLayout.xml");
                dockPanel.LoadFromXml(stream, m_deserializeDockContent);
                if (stream != null)
                    stream.Close();
            }

            //Thread
            LoadSprites(currentPath + Globals.GraphicsSubDir + Path.DirectorySeparatorChar + Globals.SpritesSuffix + Globals.XmlFormat);
            LoadDeltas(currentPath + Globals.GraphicsSubDir + Path.DirectorySeparatorChar + Globals.DeltasSuffix + Globals.XmlFormat);
        }

        private void SpriteDeltasWindowCheckedDeltaItemsChanged(object sender, SpriteDeltasWindow.CheckedDeltaItemsEventArgs e)
        {
            PaintSprite(spriteListWindow.SelectedSprite, e.CheckedDeltaItems);
        }

        private void SpriteListWindowSpriteChanged(object sender, SpriteListWindow.SpriteEventArgs e)
        {
            var currentItem = e.Sprite;
            PaintSprite(currentItem, new List<DeltaSubItem>());

            spriteDeltasWindow.DeltaItems = currentItem.DeltaItems;
            spriteRemapsWindow.Remaps = currentItem.RemapList;
        }

        private IDockContent MDeserializeDockContent(string persistString)
        {
            if (persistString == typeof(SpriteListWindow).ToString())
                return spriteListWindow;
            if (persistString == typeof(SpritePreviewWindow).ToString())
                return spritePreviewWindow;
            if (persistString == typeof(SpriteDeltasWindow).ToString())
                return spriteDeltasWindow;
            if (persistString == typeof(SpriteRemapsWindow).ToString())
                return spriteRemapsWindow;
            return null;
        }

        private void MnuFileCloseClick(object sender, EventArgs e)
        {
            Close();
        }

        private void LoadSprites(string fileName)
        {
            var dict = TextureAtlas.Deserialize<TextureAtlasSprites>(fileName);
            TextureAtlasSprites.FillSpriteId(dict.SpriteDictionary);

            spriteListWindow.TextureAltas = dict;

            spriteAtlas = dict.SpriteDictionary;
            spriteImage = Image.FromFile(Extensions.CheckDirectorySeparator(Path.GetDirectoryName(fileName)) + dict.ImagePath);
        }

        private void LoadDeltas(string fileName)
        {
            var dict = TextureAtlas.Deserialize<TextureAtlasDeltas>(fileName);
            deltaImage = Image.FromFile(Extensions.CheckDirectorySeparator(Path.GetDirectoryName(fileName)) + dict.ImagePath);
            TextureAtlasSprites.MergeDeltas(spriteAtlas, dict.DeltaDictionary);
        }

        private void PaintSprite(SpriteItem item, IList<DeltaSubItem> activeDeltas)
        {
            Image previousImage = null;
            if (spritePreviewWindow.Image != null)
                previousImage = spritePreviewWindow.Image;
            var bmp = new Bitmap(item.Rectangle.Width, item.Rectangle.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.DrawImage(spriteImage, new Rectangle(0, 0, item.Rectangle.Width, item.Rectangle.Height), item.Rectangle.X, item.Rectangle.Y, item.Rectangle.Width, item.Rectangle.Height, GraphicsUnit.Pixel);
                foreach (var delta in activeDeltas)
                {
                    g.DrawImage(deltaImage, new Rectangle(delta.RelativePosition.X, delta.RelativePosition.Y, delta.Rectangle.Width, delta.Rectangle.Height), delta.Rectangle.X, delta.Rectangle.Y, delta.Rectangle.Width, delta.Rectangle.Height, GraphicsUnit.Pixel);
                }
            }
            spritePreviewWindow.Image = bmp;
            if (previousImage != null)
                previousImage.Dispose();
        }

        private void MuFileSaveSpriteClick(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "PNG Images (*.png)|*.png|All Files (*.*)|*.*";
                dlg.FileName = spriteListWindow.SelectedSprite.SpriteId.ToString(CultureInfo.InvariantCulture);
                if (dlg.ShowDialog() == DialogResult.OK)
                    spritePreviewWindow.Image.Save(dlg.FileName, ImageFormat.Png);
            }
        }


        private void SpriteFormFormClosing(object sender, FormClosingEventArgs e)
        {
            dockPanel.SaveAsXml(currentPath + LayoutFile);
        }

        private void MnuWindowSpriteListClick(object sender, EventArgs e)
        {
            spriteListWindow.Show(dockPanel);
        }

        private void MnuWindowPreviewClick(object sender, EventArgs e)
        {
            spritePreviewWindow.Show(dockPanel);
        }

        private void MnuWindowsDeltasClick(object sender, EventArgs e)
        {
            spriteDeltasWindow.Show(dockPanel);
        }

        private void MnuWindowRemapsClick(object sender, EventArgs e)
        {
            spriteRemapsWindow.Show(dockPanel);
        }
    }
}
