//15.02.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiale.GTA2NET.Core.Helper;
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
            //vertexDeclaration = new VertexDeclaration(BaseGame.Device, VertexPositionNormalTexture.VertexElements); //XNA 3.1
            verticesCollection = new List<VertexPositionNormalTexture>();
            indicesCollection = new List<int>();
            //MainGame.Cars.ItemAdded += new EventHandler<GenericEventArgs<MovableObject>>(Cars_ItemAdded);
            MainGame.Cars.ItemRemoved += new EventHandler<GenericEventArgs<GameplayObject>>(CarsItemRemoved);
            //new
            GameplayObject.ObjectCreated += new EventHandler<GenericEventArgs<GameplayObject>>(MovableObjectObjectCreated);
        }

        private void MovableObjectObjectCreated(object sender, GenericEventArgs<GameplayObject> e)
        {
            if (!sprites.ContainsKey(e.Item))
            {
                GameplayObject baseObject = e.Item;
                int spriteIndex = 0;
                if (baseObject is Car)
                {
                    spriteIndex = (baseObject as Car).CarInfo.Model;
                }

                sprites.Add(e.Item, new Sprite(baseObject, baseObject.Position3, spriteIndex, spriteTexture, spriteAtlas));
                e.Item.PositionChanged += new EventHandler(MovableObjectPositionChanged);
                e.Item.RotationChanged += new EventHandler(MovableObjectRotationChanged);
            }
        }

        private void CarsItemAdded(object sender, GenericEventArgs<GameplayObject> e)
        {
            //if (!sprites.ContainsKey(e.Item))
            //{
            //    sprites.Add(e.Item, new Sprite(e.Item.Position, 10, spriteTexture, spriteAtlas));
            //    e.Item.PositionChanged += new EventHandler(MovableObject_PositionChanged);
            //    e.Item.RotationChanged += new EventHandler(MovableObject_RotationChanged);
            //}

        }

        private void MovableObjectRotationChanged(object sender, EventArgs e)
        {
            GameplayObject moveableObject = (GameplayObject)sender;
            Sprite currentSprite = sprites[moveableObject];
            //currentSprite.Rotate(moveableObject.Rotation - currentSprite.Rotation);
 }

        private void MovableObjectPositionChanged(object sender, EventArgs e)
        {
            GameplayObject moveableObject = (GameplayObject)sender;
            Sprite currentSprite = sprites[moveableObject];
            //currentSprite.Rotate(moveableObject.Rotation - currentSprite.Rotation, new Vector2(moveableObject.Position.X, moveableObject.Position.Y));
            //currentSprite.SetPosition(moveableObject.Position);
            currentSprite.SetPosition(moveableObject);

        }

        private void CarsItemRemoved(object sender, GenericEventArgs<GameplayObject> e)
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
            foreach (KeyValuePair<GameplayObject, Sprite> kvp in sprites)
            {
                CreateVertices(kvp.Value);
            }
            vertices = verticesCollection.ToArray();
            indices = indicesCollection.ToArray();

            vertexBuffer = new VertexBuffer(BaseGame.Device, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.None); //XNA 4.0
            vertexBuffer.SetData<VertexPositionNormalTexture>(vertices); //XNA 4.0
            BaseGame.Device.SetVertexBuffer(vertexBuffer); //XNA 4.0

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
            string spriteDictPath = "Textures\\sprites.xml";
            TextureAtlasSprites dict;
            if (!File.Exists(spriteDictPath))
            {
                //string[] spriteFiles = Directory.GetFiles("textures\\sprites");
                ZipStorer zip = ZipStorer.Open("Textures\\bil.zip", FileAccess.Read);
                dict = new TextureAtlasSprites("Textures\\sprites.png", zip);
                dict.BuildTextureAtlas();
                dict.Serialize(spriteDictPath);
                spriteAtlas = dict.SpriteDictionary;
                MemoryStream stream = new MemoryStream();
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

            //effect.GraphicsDevice.RenderState.DepthBufferEnable = true; //SpriteBatch disables DepthBuffer automatically, we need to enable it again //XNA 3.1
            //effect.GraphicsDevice.RenderState.DepthBufferWriteEnable = true; //XNA 3.1

            effect.GraphicsDevice.BlendState = BaseGame.AlphaBlendingState;

            //BaseGame.Device.BlendState = BlendState.Opaque;
            //BaseGame.Device.DepthStencilState = DepthStencilState.Default;
            //BaseGame.Device.SamplerStates[0] = SamplerState.LinearClamp; //needed?

            BaseGame.Device.SetVertexBuffer(vertexBuffer); //XNA 4.0
            //BaseGame.Device.Indices = 

            //effect.GraphicsDevice.VertexDeclaration = vertexDeclaration; //XNA 3.1
            //effect.GraphicsDevice.Indices = dynIndexBuffer;
            //effect.GraphicsDevice.Vertices[0].SetSource(dynVertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            //effect.Begin(); //XNA 3.1
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                //pass.Begin(); //XNA 3.1

                //Anisotropic
                //effect.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;
                //effect.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Linear;
                //effect.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
                //effect.GraphicsDevice.SamplerStates[0].MaxAnisotropy = 16;

                //effect.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Point;
                //effect.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Point;
                //effect.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Point;

                pass.Apply();
                //effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, verticesCollection.Count, 0, indexBufferCollection.Count / 3);
                effect.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3); //XNA 3.1
                //effect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
                //pass.End(); //XNA 3.1
            }
            //effect.End(); //XNA 3.1
        }

    }
}
