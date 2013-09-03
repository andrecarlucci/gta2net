// GTA2.NET
// 
// File: CityRenderer.cs
// Created: 08.02.2010
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
using System.Collections.Generic;
using System.IO;
using Hiale.GTA2NET.Core;
using Hiale.GTA2NET.Core.Helper;
using Microsoft.Xna.Framework.Graphics;

namespace Hiale.GTA2NET.Renderer
{
    public class CityRenderer
    {
        private readonly BasicEffect _effect;

        //City textures
        private Texture2D _cityTexture;
        private Dictionary<int, CompactRectangle> _tileAtlas;

        //Triangle stuff
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private List<int> _indexBufferCollection;
        private List<VertexPositionNormalTexture> _cityVerticesCollection;

        public CityRenderer()
        {
            _effect = new BasicEffect(BaseGame.Device);
            _cityVerticesCollection = new List<VertexPositionNormalTexture>();
            _indexBufferCollection = new List<int>();
        }

        public void LoadCity()
        {
            LoadTexture();

            MainGame.Map.CalcCoord();
            _indexBufferCollection = MainGame.Map.IndexBufferCollection;
            _cityVerticesCollection = MainGame.Map.Coors;

            CopyToGraphicsDevice();
            SetUpEffect();
        }
        
        private void SetUpEffect()
        {
            _effect.Texture = _cityTexture;
            _effect.TextureEnabled = true;
            _effect.LightingEnabled = false;
        }

        private void CopyToGraphicsDevice()
        {
            var cubeVertices = _cityVerticesCollection.ToArray();

            _vertexBuffer = new VertexBuffer(BaseGame.Device, typeof(VertexPositionNormalTexture), cubeVertices.Length, BufferUsage.None);
            _vertexBuffer.SetData(cubeVertices);

            var indexBufferData = _indexBufferCollection.ToArray();
            _indexBuffer = new IndexBuffer(BaseGame.Device, typeof(int), indexBufferData.Length, BufferUsage.None);
            _indexBuffer.SetData(indexBufferData);
        }

        private void LoadTexture()
        {
            var atlasPath = Globals.GraphicsSubDir + Path.DirectorySeparatorChar + MainGame.StyleName + Globals.TilesSuffix + Globals.XmlFormat;
            var dict = TextureAtlas.Deserialize<TextureAtlasTiles>(atlasPath);
            _tileAtlas = dict.TileDictionary;
            var fs = new FileStream(Globals.GraphicsSubDir + Path.DirectorySeparatorChar + dict.ImagePath, FileMode.Open, FileAccess.Read); //hack, for some reason when open this file sometimes fail because is already open.
            _cityTexture = Texture2D.FromStream(BaseGame.Device, fs);
            fs.Close();
        }

        public void DrawCity()
        {   
            _effect.View = BaseGame.ViewMatrix;
            _effect.Projection = BaseGame.ProjectionMatrix;
            _effect.World = BaseGame.WorldMatrix;

            // Used to show the "map" inWireframe
            //RasterizerState r = new RasterizerState();
            //r.FillMode = FillMode.WireFrame;
            //_effect.GraphicsDevice.RasterizerState = r;


            _effect.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            _effect.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            _effect.GraphicsDevice.BlendState = BaseGame.AlphaBlendingState;

            BaseGame.Device.SetVertexBuffer(_vertexBuffer);
            BaseGame.Device.Indices = _indexBuffer;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _cityVerticesCollection.Count, 0, _indexBufferCollection.Count / 3);
            }
        }      
    }
}
