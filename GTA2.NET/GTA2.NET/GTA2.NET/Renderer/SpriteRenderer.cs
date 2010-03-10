//15.02.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Hiale.GTA2NET.Helper;
using System.IO;
using Hiale.GTA2NET.Logic;

namespace Hiale.GTA2NET.Renderer
{
    public class SpriteRenderer
    {
        BasicEffect effect;

        //Sprite stuff
        Texture2D spriteTexture;
        //List<Sprite> sprites;
        Dictionary<MovableObject, Sprite> sprites;
        Dictionary<int, Rectangle> spriteAtlas;
        

        //Triangle stuff, use DynamicVertexBuffer?
        VertexDeclaration vertexDeclaration;
        List<VertexPositionNormalTexture> verticesCollection;
        VertexPositionNormalTexture[] vertices;
        List<int> indicesCollection;
        int[] indices;

        public SpriteRenderer()
        {
            effect = new BasicEffect(BaseGame.Device, null);
            sprites = new Dictionary<MovableObject, Sprite>();
            vertexDeclaration = new VertexDeclaration(BaseGame.Device, VertexPositionNormalTexture.VertexElements);
            verticesCollection = new List<VertexPositionNormalTexture>();
            indicesCollection = new List<int>();
            //MainGame.Cars.ItemAdded += new EventHandler<GenericEventArgs<MovableObject>>(Cars_ItemAdded);
            MainGame.Cars.ItemRemoved += new EventHandler<GenericEventArgs<MovableObject>>(Cars_ItemRemoved);
            //new
            MovableObject.ObjectCreated += new EventHandler<GenericEventArgs<MovableObject>>(MovableObject_ObjectCreated);
        }

        void MovableObject_ObjectCreated(object sender, GenericEventArgs<MovableObject> e)
        {
            if (!sprites.ContainsKey(e.Item))
            {
                MovableObject baseObject = e.Item;
                int spriteIndex = 0;
                if (baseObject is Car)
                {
                    spriteIndex = (baseObject as Car).CarInfo.Model;
                }

                sprites.Add(e.Item, new Sprite(baseObject, baseObject.Position3, spriteIndex, spriteTexture, spriteAtlas));
                e.Item.PositionChanged += new EventHandler(MovableObject_PositionChanged);
                e.Item.RotationChanged += new EventHandler(MovableObject_RotationChanged);
            }
        }

        void Cars_ItemAdded(object sender, GenericEventArgs<MovableObject> e)
        {
            //if (!sprites.ContainsKey(e.Item))
            //{
            //    sprites.Add(e.Item, new Sprite(e.Item.Position, 10, spriteTexture, spriteAtlas));
            //    e.Item.PositionChanged += new EventHandler(MovableObject_PositionChanged);
            //    e.Item.RotationChanged += new EventHandler(MovableObject_RotationChanged);
            //}

        }

        void MovableObject_RotationChanged(object sender, EventArgs e)
        {
            MovableObject moveableObject = (MovableObject)sender;
            Sprite currentSprite = sprites[moveableObject];
            //currentSprite.Rotate(moveableObject.Rotation - currentSprite.Rotation);
 }

        void MovableObject_PositionChanged(object sender, EventArgs e)
        {
            MovableObject moveableObject = (MovableObject)sender;
            Sprite currentSprite = sprites[moveableObject];
            //currentSprite.Rotate(moveableObject.Rotation - currentSprite.Rotation, new Vector2(moveableObject.Position.X, moveableObject.Position.Y));
            //currentSprite.SetPosition(moveableObject.Position);
            currentSprite.SetPosition(moveableObject);

        }

        void Cars_ItemRemoved(object sender, GenericEventArgs<MovableObject> e)
        {
            if (sprites.ContainsKey(e.Item))
                sprites.Remove(e.Item);
        }

        public void LoadSprites()
        {
            LoadTexture();
            SetUpEffect();
        }

        private void CreateAllVertices()
        {
            verticesCollection.Clear();
            indicesCollection.Clear();
            foreach (KeyValuePair<MovableObject, Sprite> kvp in sprites)
            {
                CreateVertices(kvp.Value);
            }
            vertices = verticesCollection.ToArray();
            indices = indicesCollection.ToArray();
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
            string spriteDictPath = "textures\\sprites.xml";
            TextureAtlas dict;
            if (!File.Exists(spriteDictPath))
            {
                string[] spriteFiles = Directory.GetFiles("textures\\sprites");
                dict = ImageHelper.CreateImageDictionary(spriteFiles);
                dict.Serialize(spriteDictPath);
                spriteAtlas = dict.Dictionary;
                MemoryStream stream = new MemoryStream();
                dict.Image.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                stream.Position = 0;
                spriteTexture = Texture2D.FromFile(BaseGame.Device, stream);
                stream.Close();
                dict.Dispose();
            }
            else
            {
                dict = TextureAtlas.Deserialize(spriteDictPath);
                spriteAtlas = dict.Dictionary;
                spriteTexture = Texture2D.FromFile(BaseGame.Device, dict.ImagePath);
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

            //effect.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
            //effect.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            //effect.GraphicsDevice.RenderState.CullMode = CullMode.None;


            //effect.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;

            effect.GraphicsDevice.RenderState.DepthBufferEnable = true; //SpriteBatch disables DepthBuffer automatically, we need to enable it again
            effect.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            effect.GraphicsDevice.VertexDeclaration = vertexDeclaration;
            //effect.GraphicsDevice.Indices = dynIndexBuffer;
            //effect.GraphicsDevice.Vertices[0].SetSource(dynVertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                //Anisotropic
                //effect.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;
                //effect.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Linear;
                //effect.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
                //effect.GraphicsDevice.SamplerStates[0].MaxAnisotropy = 16;

                effect.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Point;
                effect.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Point;
                effect.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Point;

                //effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, verticesCollection.Count, 0, indexBufferCollection.Count / 3);
                effect.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
                pass.End();
            }
            effect.End();
        }

    }
}
