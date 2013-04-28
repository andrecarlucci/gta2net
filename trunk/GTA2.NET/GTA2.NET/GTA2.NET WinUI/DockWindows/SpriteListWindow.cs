// GTA2.NET
// 
// File: SpriteListWindow.cs
// Created: 27.04.2013
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
using System.Drawing;
using System.IO;
using System.Reflection;
using Hiale.GTA2NET.Core;
using Hiale.GTA2NET.Core.Helper;
using WeifenLuo.WinFormsUI.Docking;

namespace Hiale.GTA2NET.WinUI.DockWindows
{
    public partial class SpriteListWindow : DockContent
    {
        public class SpriteEventArgs : EventArgs
        {
            public SpriteItem Sprite;

            public SpriteEventArgs(SpriteItem sprite)
            {
                Sprite = sprite;
            }
        }

        public event EventHandler<SpriteEventArgs> SpriteChanged;

        public SpriteItem SelectedSprite { get; private set; }

        private TextureAtlasSprites _textureAtlas;

        public TextureAtlasSprites TextureAltas
        {
            get { return _textureAtlas; }
            set
            {
                _textureAtlas = value;
                if (value == null)
                    return;
                var spriteAtlas = value.SpriteDictionary;
                var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var spriteImage = Image.FromFile(Extensions.CheckDirectorySeparator(currentDir) + Globals.GraphicsSubDir + Path.DirectorySeparatorChar + value.ImagePath);
                listBoxSprites.SpriteAtlas = spriteAtlas;
                listBoxSprites.SpriteImage = spriteImage;

                listBoxSprites.Items.Clear();
                foreach (var sprite in spriteAtlas)
                    listBoxSprites.Items.Add(sprite);
            }
        }

        public Image Image {get { return listBoxSprites.SpriteImage; }}

        public SpriteListWindow()
        {
            InitializeComponent();
        }

        private void ListBoxSpritesSelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedSprite = TextureAltas.SpriteDictionary[listBoxSprites.SelectedIndex];
            if (SpriteChanged != null)
                SpriteChanged(this, new SpriteEventArgs(SelectedSprite));
        }
    }
}
