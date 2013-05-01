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
        private readonly SpriteListWindow _spriteListWindow;
        private readonly SpritePreviewWindow _spritePreviewWindow;
        private readonly SpriteDeltasWindow _spriteDeltasWindow;
        private readonly SpriteRemapsWindow _spriteRemapsWindow;
        private readonly DeserializeDockContent _deserializeDockContent;
        private const string LayoutFile = "SpriteFormLayout.xml";

        private LockBitmap _spriteImage;
        private LockBitmap _paletteImage;
        private LockBitmap _deltaImage;

        private byte[,] _spriteData;
        private byte[,] _deltaData;

        private readonly string _currentPath ;

        private bool _shouldPaint;

        public SpriteForm()
        {
            InitializeComponent();

            _currentPath = Extensions.CheckDirectorySeparator(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            _spriteListWindow = new SpriteListWindow();
            _spriteListWindow.SpriteChanged += SpriteListWindowSpriteChanged;
            _spritePreviewWindow = new SpritePreviewWindow();
            _spriteDeltasWindow = new SpriteDeltasWindow();
            _spriteDeltasWindow.CheckedDeltaItemsChanged += SpriteDeltasWindowCheckedDeltaItemsChanged;
            _spriteRemapsWindow = new SpriteRemapsWindow();
            _spriteRemapsWindow.RemapChanged += SpriteRemapsWindowRemapChanged;

            _deserializeDockContent += DeserializeDockContent;

            var layoutFile = _currentPath + LayoutFile;
            try
            {
                dockPanel.LoadFromXml(layoutFile, _deserializeDockContent);
            }
            catch (Exception)
            {
                var stream = Assembly.GetAssembly(GetType()).GetManifestResourceStream(GetType().Namespace + ".Resources.SpriteFormDefaultLayout.xml");
                dockPanel.LoadFromXml(stream, _deserializeDockContent);
                if (stream != null)
                    stream.Close();
            }

            //Thread
            LoadPalette(_currentPath + Globals.GraphicsSubDir + Path.DirectorySeparatorChar + Globals.PaletteSuffix + Globals.TextureImageFormat);
            LoadSprites(_currentPath + Globals.GraphicsSubDir + Path.DirectorySeparatorChar + Globals.SpritesSuffix + Globals.XmlFormat);
            LoadDeltas(_currentPath + Globals.GraphicsSubDir + Path.DirectorySeparatorChar + Globals.DeltasSuffix + Globals.XmlFormat);
            CreateSpriteData(_spriteListWindow.TextureAltas);
        }

        private void SpriteRemapsWindowRemapChanged(object sender, SpriteRemapsWindow.RemapEventArgs e)
        {
            if (_shouldPaint)
                PaintSprite();
        }

        private void SpriteDeltasWindowCheckedDeltaItemsChanged(object sender, SpriteDeltasWindow.CheckedDeltaItemsEventArgs e)
        {
            if (_shouldPaint)
                PaintSprite();
        }

        private void SpriteListWindowSpriteChanged(object sender, SpriteListWindow.SpriteEventArgs e)
        {
            _shouldPaint = false;
            _spriteDeltasWindow.DeltaItems = e.Sprite.DeltaItems;
            _spriteRemapsWindow.Remaps = e.Sprite.RemapList;
            PaintSprite();
            _shouldPaint = true;
        }

        private IDockContent DeserializeDockContent(string persistString)
        {
            if (persistString == typeof(SpriteListWindow).ToString())
                return _spriteListWindow;
            if (persistString == typeof(SpritePreviewWindow).ToString())
                return _spritePreviewWindow;
            if (persistString == typeof(SpriteDeltasWindow).ToString())
                return _spriteDeltasWindow;
            if (persistString == typeof(SpriteRemapsWindow).ToString())
                return _spriteRemapsWindow;
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

            _spriteListWindow.TextureAltas = dict;

            _spriteImage = new LockBitmap((Bitmap) _spriteListWindow.Image);
        }

        private void LoadPalette(string fileName)
        {
            var image = Image.FromFile(fileName);
            _paletteImage = new LockBitmap((Bitmap) image);
        }

        private void CreateSpriteData(TextureAtlasSprites sprites)
        {
            _spriteImage.LockBits();
            _spriteData = new byte[_spriteImage.Width,_spriteImage.Height];
            _deltaImage.LockBits();
            _deltaData = new byte[_deltaImage.Width,_deltaImage.Height];
            var colorCache = new Dictionary<Color, byte>();
            _paletteImage.LockBits();
            foreach (var sprite in sprites.SpriteDictionary)
            {
                //actual Sprite
                colorCache.Clear();
                var palette = sprite.Value.DefaultPalette;
                for (var x = sprite.Value.Rectangle.X; x < sprite.Value.Rectangle.X + sprite.Value.Rectangle.Width; x++)
                {
                    for (var y = sprite.Value.Rectangle.Y; y < sprite.Value.Rectangle.Y + sprite.Value.Rectangle.Height; y++)
                    {
                        var color = _spriteImage.GetPixel(x, y);
                        if (color.ToArgb() == 0)
                            continue;
                        byte value;
                        if (colorCache.TryGetValue(color, out value))
                        {
                            _spriteData[x, y] = value;
                            continue;
                        }
                        for (byte i = 0; i < 256; i++)
                        {
                            if (color == _paletteImage.GetPixel(palette, i))
                            {
                                _spriteData[x,y] = i;
                                colorCache.Add(color, i);
                                break;
                            }
                        }
                        
                    }
                }
                //Deltas
                if (sprite.Value.DeltaItems == null)
                    continue;
                foreach (var delta in sprite.Value.DeltaItems)
                {
                    for (var x = delta.Rectangle.X; x < delta.Rectangle.X + delta.Rectangle.Width; x++)
                    {
                        for (var y = delta.Rectangle.Y; y < delta.Rectangle.Y + delta.Rectangle.Height; y++)
                        {
                            var color = _deltaImage.GetPixel(x, y);
                            if (color.ToArgb() == 0)
                                continue;
                            byte value;
                            if (colorCache.TryGetValue(color, out value))
                            {
                                _deltaData[x, y] = value;
                                continue;
                            }
                            for (byte i = 0; i < 256; i++)
                            {
                                if (color == _paletteImage.GetPixel(palette, i))
                                {
                                    _deltaData[x, y] = i;
                                    colorCache.Add(color, i);
                                    break;
                                }
                                if (i == 255)
                                {
                                    System.Diagnostics.Debug.WriteLine("Color not found!");
                                    //DetectPalette(paletteImage, deltaImage, delta.Rectangle);
                                }
                            }
                        }
                    }

                }

            }
            _deltaImage.UnlockBits();
            _spriteImage.UnlockBits();
        }

        private static int DetectPalette(LockBitmap palettes, LockBitmap lockBmp, CompactRectangle rect)
        {
            var max = 0;
            var value = -1;
            var dict = new Dictionary<int, List<Color>>();
            for (var i = 0; i < palettes.Width; i++)
            {
                dict.Add(i, new List<Color>());
                for (var x = rect.X; x < rect.X + rect.Width; x++)
                {
                    for (var y = rect.Y; y < rect.Y + rect.Height; y++)
                    {
                        var orgColor = lockBmp.GetPixel(x, y);
                        for (var c = 0; c < 256; c++)
                        {
                            if (orgColor != palettes.GetPixel(i, c))
                                continue;
                            if (!dict[i].Contains(orgColor))
                                dict[i].Add(orgColor);
                            if (dict[i].Count <= max)
                                continue;
                            value = i;
                            max = dict[i].Count;
                        }
                    }
                }
            }
            return value;
        }

        private void LoadDeltas(string fileName)
        {
            var dict = TextureAtlas.Deserialize<TextureAtlasDeltas>(fileName);
            _deltaImage = new LockBitmap((Bitmap) Image.FromFile(Extensions.CheckDirectorySeparator(Path.GetDirectoryName(fileName)) + dict.ImagePath));
            TextureAtlasSprites.MergeDeltas(_spriteListWindow.TextureAltas.SpriteDictionary, dict.DeltaDictionary);
        }

        private void PaintSprite()
        {
            var item = _spriteListWindow.SelectedSprite;
            var remap = _spriteRemapsWindow.SelectedItem;
            int palette = remap.Key == -1 ? _spriteListWindow.SelectedSprite.DefaultPalette : remap.Palette;
            var activeDeltas = _spriteDeltasWindow.CheckedDeltaItems;

            Image previousImage = null;
            if (_spritePreviewWindow.Image != null)
                previousImage = _spritePreviewWindow.Image;
            var outputBitmap = new LockBitmap(new Bitmap(item.Rectangle.Width, item.Rectangle.Height));
            outputBitmap.LockBits();
            for (var x = 0; x < item.Rectangle.Width; x++)
            {
                for (var y = 0; y < item.Rectangle.Height; y++)
                {
                    var value = _spriteData[item.Rectangle.X + x, item.Rectangle.Y + y];
                    if (value != 0)
                        outputBitmap.SetPixel(x, y, _paletteImage.GetPixel(palette, value));
                }

            }
            foreach (var delta in activeDeltas)
            {
                for (var x = 0; x < delta.Rectangle.Width; x++)
                {
                    for (var y = 0; y < delta.Rectangle.Height; y++)
                    {
                        var value = _deltaData[delta.Rectangle.X + x, delta.Rectangle.Y + y];
                        if (value != 0)
                            outputBitmap.SetPixel(delta.RelativePosition.X + x, delta.RelativePosition.Y + y, _paletteImage.GetPixel(palette, value));
                    }
                }
            }

            outputBitmap.UnlockBits();
            _spritePreviewWindow.Image = outputBitmap.Source;
            if (previousImage != null)
                previousImage.Dispose();
        }

        private void MuFileSaveSpriteClick(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "PNG Images (*.png)|*.png|All Files (*.*)|*.*";
                dlg.FileName = _spriteListWindow.SelectedSprite.SpriteId.ToString(CultureInfo.InvariantCulture);
                if (dlg.ShowDialog() == DialogResult.OK)
                    _spritePreviewWindow.Image.Save(dlg.FileName, ImageFormat.Png);
            }
        }

        private void SpriteFormFormClosing(object sender, FormClosingEventArgs e)
        {
            _deltaImage.Dispose();
            _paletteImage.Dispose();
            _spriteImage.Dispose();
            dockPanel.SaveAsXml(_currentPath + LayoutFile);
        }

        private void MnuWindowSpriteListClick(object sender, EventArgs e)
        {
            _spriteListWindow.Show(dockPanel);
        }

        private void MnuWindowPreviewClick(object sender, EventArgs e)
        {
            _spritePreviewWindow.Show(dockPanel);
        }

        private void MnuWindowsDeltasClick(object sender, EventArgs e)
        {
            _spriteDeltasWindow.Show(dockPanel);
        }

        private void MnuWindowRemapsClick(object sender, EventArgs e)
        {
            _spriteRemapsWindow.Show(dockPanel);
        }
    }
}
