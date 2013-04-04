// GTA2.NET
// 
// File: UIRenderer.cs
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
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace Hiale.GTA2NET.Renderer
{
    public class UiRenderer
    {
        readonly SpriteBatch spriteBatch;
        readonly Texture2D mousePointerTex;
        readonly Texture2D nullTex;

        public UiRenderer()
        {
            const string mouseCursor = "textures\\MouseCursor.png";
            if (!File.Exists(mouseCursor))
                return;
            spriteBatch = new SpriteBatch(BaseGame.Device);
            var fs = new FileStream(mouseCursor, FileMode.Open);
            mousePointerTex = Texture2D.FromStream(BaseGame.Device, fs);
            fs.Close();
            //mousePointerTex = MainGame.Content.Load<Texture2D>("MouseCursor");
            nullTex = new Texture2D(BaseGame.Device, 1, 1);
            Int32[] pixel = {0xFF}; // White. 0xFF is Red, 0xFF0000 is Blue
            nullTex.SetData(pixel);
        }

        public void DrawMouseMounter()
        {
            if (spriteBatch == null)
                return;
            spriteBatch.Begin();

            //Lines
            //DrawLine(new Vector2(0, BaseGame.Height), new Vector2(BaseGame.Width, 0));
            //DrawLine(new Vector2(0, 0), new Vector2(BaseGame.Width, BaseGame.Height));

            spriteBatch.Draw(mousePointerTex, new Vector2(Input.MousePos.X, Input.MousePos.Y), null, Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0f);
            spriteBatch.End();
        }

        private void DrawLine(Vector2 a, Vector2 b)
        {
            var origin = new Vector2(0.5f, 0.0f);
            var diff = b - a;
            var scale = new Vector2(1.0f, diff.Length() / nullTex.Height);
            var angle = (float)(Math.Atan2(diff.Y, diff.X)) - MathHelper.PiOver2;
            spriteBatch.Draw(nullTex, a, null, Color.White, angle, origin, scale, SpriteEffects.None, 1.0f);            
        }

    }
}
