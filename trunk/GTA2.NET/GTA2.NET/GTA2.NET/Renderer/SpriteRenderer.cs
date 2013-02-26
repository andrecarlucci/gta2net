//20.02.2013

using System;
using System.Collections.Generic;
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
        Dictionary<SpriteItem, Rectangle> spriteAtlas;
        

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
            const string spriteDictPath = "Textures\\sprites.xml";
            TextureAtlasSprites dict;
            if (!File.Exists(spriteDictPath))
            {
                var zip = ZipStorer.Open("Textures\\bil.zip", FileAccess.Read);
                dict = new TextureAtlasSprites("Textures\\sprites.png", zip);
                dict.BuildTextureAtlas();
                dict.Serialize(spriteDictPath);
                spriteAtlas = dict.SpriteDictionary;
                var stream = new MemoryStream();
                dict.Image.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                stream.Position = 0;
                spriteTexture = Texture2D.FromStream(BaseGame.Device, stream);
                stream.Close();
                dict.Dispose();
            }
            else
            {
                dict = (TextureAtlasSprites)TextureAtlas.Deserialize(spriteDictPath, typeof(TextureAtlasSprites));
                spriteAtlas = dict.SpriteDictionary;
                FileStream fs = new FileStream(dict.ImagePath, FileMode.Open);
                spriteTexture = Texture2D.FromStream(BaseGame.Device, fs);
                fs.Close();
                //spriteTexture = MainGame.Content.Load<Texture2D>("sprites");
            }            
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
