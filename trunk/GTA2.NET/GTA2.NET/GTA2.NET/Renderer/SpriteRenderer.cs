// GTA2.NET
// 
// File: SpriteRenderer.cs
// Created: 15.02.2010
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
using Hiale.GTA2NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Helper;
using System.IO;
using Hiale.GTA2NET.Logic;

namespace Hiale.GTA2NET.Renderer
{
    public class SpriteRenderer
    {
        readonly BasicEffect effect;

        //Sprite stuff
        Texture2D spriteTexture;
        //List<Sprite> sprites;
        Dictionary<GameplayObject, Sprite> sprites;
        Dictionary<SpriteItem, CompactRectangle> spriteAtlas;
        

        //Triangle stuff, use DynamicVertexBuffer?
        VertexBuffer vertexBuffer;
        List<VertexPositionNormalTexture> verticesCollection;
        VertexPositionNormalTexture[] vertices;
        List<int> indicesCollection;
        int[] indices;

        public SpriteRenderer()
        {
            effect = new BasicEffect(BaseGame.Device);
            sprites = new Dictionary<GameplayObject, Sprite>();
            verticesCollection = new List<VertexPositionNormalTexture>();
            indicesCollection = new List<int>();
        }

        private void CarsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems.Count < 1)
                return;
            var car = (Car) e.NewItems[0];
            if (sprites.ContainsKey(car))
                return;
            var spriteIndex = car.CarInfo.Model;
            sprites.Add(car, new Sprite(car, car.Position3, spriteIndex, spriteTexture, spriteAtlas));
        }

        public void LoadSprites()
        {
            LoadTexture();
            SetUpEffect();
            foreach (var car in MainGame.Cars)
                sprites.Add(car, new Sprite(car, car.Position3, car.CarInfo.Model, spriteTexture, spriteAtlas));
            MainGame.Cars.CollectionChanged += CarsCollectionChanged;
        }

        private void CreateAllVertices()
        {
            verticesCollection.Clear();
            indicesCollection.Clear();
            foreach (var sprite in sprites)
            {
                sprite.Value.SetPosition(sprite.Key); //Update position
                CreateVertices(sprite.Value);
            }
            if (verticesCollection.Count < 1)
                return;
            vertices = verticesCollection.ToArray();
            indices = indicesCollection.ToArray();

            vertexBuffer = new VertexBuffer(BaseGame.Device, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.None);
            vertexBuffer.SetData(vertices);
            BaseGame.Device.SetVertexBuffer(vertexBuffer);

        }

        private void CreateVertices(Sprite sprite)
        {
            verticesCollection.Add(new VertexPositionNormalTexture(sprite.TopRight, Vector3.Zero, sprite.TexturePositionTopRight));
            verticesCollection.Add(new VertexPositionNormalTexture(sprite.BottomRight, Vector3.Zero, sprite.TexturePositionBottomRight));
            verticesCollection.Add(new VertexPositionNormalTexture(sprite.TopLeft, Vector3.Zero, sprite.TexturePositionTopLeft));
            verticesCollection.Add(new VertexPositionNormalTexture(sprite.BottomLeft, Vector3.Zero, sprite.TexturePositionBottomLeft));

            int startIndex = verticesCollection.Count - 4;
            indicesCollection.Add(startIndex);
            indicesCollection.Add(startIndex + 1);
            indicesCollection.Add(startIndex + 2);
            indicesCollection.Add(startIndex + 1);
            indicesCollection.Add(startIndex + 3);
            indicesCollection.Add(startIndex + 2);
        }

        private void LoadTexture()
        {
            var atlasPath = Globals.GraphicsSubDir + Path.DirectorySeparatorChar + MainGame.StyleName + "_" + Globals.SpritesSuffix.ToLower() + ".xml";
            var dict = TextureAtlas.Deserialize<TextureAtlasSprites>(atlasPath);
            spriteAtlas = dict.SpriteDictionary;
            var fs = new FileStream(Globals.GraphicsSubDir + Path.DirectorySeparatorChar +dict.ImagePath, FileMode.Open);
            spriteTexture = Texture2D.FromStream(BaseGame.Device, fs);
            fs.Close();
        }

        private void SetUpEffect()
        {
            effect.Texture = spriteTexture;
            effect.TextureEnabled = true;
            effect.LightingEnabled = false;
        }

        public void DrawSprites()
        {
            CreateAllVertices();
            if (verticesCollection.Count < 1)
                return;
            effect.View = BaseGame.ViewMatrix;
            effect.Projection = BaseGame.ProjectionMatrix;
            effect.World = BaseGame.WorldMatrix;
            effect.GraphicsDevice.BlendState = BaseGame.AlphaBlendingState;

            BaseGame.Device.SetVertexBuffer(vertexBuffer);
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                effect.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3); //XNA 3.1
            }
        }

    }
}
