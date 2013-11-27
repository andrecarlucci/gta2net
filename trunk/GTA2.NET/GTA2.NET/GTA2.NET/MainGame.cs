// GTA2.NET
// 
// File: MainGame.cs
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

using Hiale.GTA2NET.Core;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Core.Logic;
using Hiale.GTA2NET.Core.Map;
using Hiale.GTA2NET.GameScreens;
using J2i.Net.XInputWrapper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace Hiale.GTA2NET
{
    public class MainGame : BaseGame
    {
        /// <summary>
        /// Scalar of the scene
        /// </summary>
        public static Vector3 GlobalScalar { get; set; }

        public const float RotationScalar = 50; //was 0.05
        public const float ForwardScalar = 10;

        private static string _windowTitle;
        /// <summary>
        /// Title of the game window;
        /// </summary>
        public static string WindowTitle
        {
            get
            {
                if (string.IsNullOrEmpty(_windowTitle))
                    _windowTitle = string.Empty;
                return _windowTitle;
            }
            set
            {
                if (value.Length > 0)
                    _windowTitle = value + " - ";
            }
        }

        /// <summary>
        /// Create Game
        /// </summary>
        public MainGame() : base()
        {
            mapName = "MP1-comp";
            styleName = "bil";
            mapPath = Globals.MapsSubDir + Path.DirectorySeparatorChar + mapName + Globals.MapFileExtension; 
            stylePath = Globals.GraphicsSubDir + Path.DirectorySeparatorChar + styleName + Globals.TilesSuffix + Globals.TextureImageFormat;
            spritesPath = Globals.GraphicsSubDir + Path.DirectorySeparatorChar + "sprites" + Globals.TextureImageFormat;
        }

        private GTA2Game game;
        private Frame frame;
        private Effect mapEffect;
        private Effect spritesEffect;

        /// <summary>
        /// Textures used in the map blocks
        /// </summary>
        private Texture2D tiles;
        /// <summary>
        /// Textures used in "moving" parts i.e peds, powerups, cars, etc.
        /// </summary>
        private Texture2D sprites;

        private String mapName;
        private String styleName;
        private String mapPath;
        private String stylePath;
        private String spritesPath;

        private VertexBuffer mapVertexBuffer;
        private IndexBuffer mapIndexBuffer;
        private VertexBuffer spritesVertexBuffer;
        private IndexBuffer spritesIndexBuffer;

        /// <summary>
        /// Load stuff
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            var handle = WinUI.NativeWin32.FindWindow(null, Window.Title); //Window.Handle seems to be wrong!
            WinUI.NativeWin32.SetWindowPos(handle, 0, 0, 0, Width, Height, 0);

            LoadTexture();
            game = new GTA2Game(mapPath, styleName);

            mapEffect = Content.Load<Effect>(@"Content\Effect2");
            mapEffect.Parameters["ModelTexture"].SetValue(tiles);

            spritesEffect = Content.Load<Effect>(@"Content\Effect1");
            spritesEffect.Parameters["ModelTexture"].SetValue(sprites);

            CameraPos = new Vector3(65, -180, 8);
            ViewMatrix = Matrix.CreateLookAt(CameraPos, new Vector3(CameraPos.X, CameraPos.Y, 0), Vector3.Up);
        }

        private void LoadTexture()
        {
            //TODO: handle the case where the fiel don't exist.
            var fs = new FileStream(stylePath, FileMode.Open, FileAccess.Read);
            tiles = Texture2D.FromStream(BaseGame.Device, fs);
            fs.Close();

            fs = new FileStream(spritesPath, FileMode.Open, FileAccess.Read);
            sprites = Texture2D.FromStream(BaseGame.Device, fs);
            fs.Close();
        }

        /// <summary>
        /// Update
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Update game engine
            base.Update(gameTime);

            var controller = XboxController.RetrieveController(0);
            controller.UpdateState();

            if (controller.IsBackPressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            var elapsedGameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var input = new PlayerInput();
            HandleInput(ref input, controller);
            
            if (Input.KeyboardUpPressed || controller.RightTrigger > 0)
            {
                //_playerForwardDelta = ForwardScalar * _elapsedGameTime;
                CameraPos = new Vector3(CameraPos.X, CameraPos.Y + 1, CameraPos.Z);
            }
            else if (Input.KeyboardDownPressed || controller.LeftTrigger > 0)
            {
                //_playerForwardDelta = -ForwardScalar * _elapsedGameTime;
                CameraPos = new Vector3(CameraPos.X , CameraPos.Y - 1, CameraPos.Z);
            }
            if (Input.KeyboardLeftPressed || controller.IsDPadLeftPressed)
            {
                //_playerRotationDelta = -RotationScalar * _elapsedGameTime;
                CameraPos = new Vector3(CameraPos.X - 1, CameraPos.Y, CameraPos.Z);
            }
            else if (Input.KeyboardRightPressed || controller.IsDPadRightPressed)
            {
                //_playerRotationDelta = RotationScalar * _elapsedGameTime;
                CameraPos = new Vector3(CameraPos.X +1, CameraPos.Y, CameraPos.Z);
            }
            if (Input.Keyboard.IsKeyDown(Keys.X))
            {
                //_playerForwardDelta = ForwardScalar * _elapsedGameTime;
                CameraPos = new Vector3(CameraPos.X, CameraPos.Y, CameraPos.Z + 1);
            }
            else if (Input.Keyboard.IsKeyDown(Keys.Z))
            {
                //_playerForwardDelta = -ForwardScalar * _elapsedGameTime;
                CameraPos = new Vector3(CameraPos.X, CameraPos.Y, CameraPos.Z - 1);
            }
            
            game.Input(input);
            game.Update(elapsedGameTime);
            frame = game.getPosition(new Vector2(CameraPos.X, -CameraPos.Y));

            ViewMatrix = Matrix.CreateLookAt(CameraPos, new Vector3(CameraPos.X, CameraPos.Y, 0), Vector3.Up);

            Window.Title = "GTA2.NET - " + WindowTitle + Fps.ToString(CultureInfo.InvariantCulture) + " fps X: " + CameraPos.X.ToString() + "Y: " + CameraPos.Y.ToString() + "Z: " +CameraPos.Z;
        }

        private void HandleInput(ref PlayerInput playerInput, XboxController controller)
        {
            //Vector3 cameraPos = BaseGame.CameraPos;
            if (Input.KeyboardUpPressed || controller.RightTrigger > 0)
            {
                //_playerForwardDelta = ForwardScalar * _elapsedGameTime;
                playerInput.Forward = 1;
            }
            else if (Input.KeyboardDownPressed || controller.LeftTrigger > 0)
            {
                //_playerForwardDelta = -ForwardScalar * _elapsedGameTime;
                playerInput.Forward = -1;
            }
            if (Input.KeyboardLeftPressed || controller.IsDPadLeftPressed)
            {
                //_playerRotationDelta = -RotationScalar * _elapsedGameTime;
                playerInput.Rotation = 1;
            }
            else if (Input.KeyboardRightPressed || controller.IsDPadRightPressed)
            {
                //_playerRotationDelta = RotationScalar * _elapsedGameTime;
                playerInput.Rotation = -1;
            }
        }

        /// <summary>
        /// Render
        /// </summary>
        protected override void Render()
        {
            mapEffect.Parameters["World"].SetValue(WorldMatrix);
            mapEffect.Parameters["View"].SetValue(ViewMatrix);
            mapEffect.Parameters["Projection"].SetValue(ProjectionMatrix);

            spritesEffect.Parameters["World"].SetValue(WorldMatrix);
            spritesEffect.Parameters["View"].SetValue(ViewMatrix);
            spritesEffect.Parameters["Projection"].SetValue(ProjectionMatrix);

            // Used to show the "map" inWireframe
            RasterizerState r = new RasterizerState();
            r.FillMode = FillMode.WireFrame;
            //spritesEffect.GraphicsDevice.RasterizerState = r;

            drawMap();
            drawObjects();
        }

        void drawMap()
        {
            var cubeVertices = frame.MapVertexList.ToArray();

            mapVertexBuffer = new VertexBuffer(BaseGame.Device, typeof(VertexPositionNormalTexture), cubeVertices.Length, BufferUsage.None);
            mapVertexBuffer.SetData(cubeVertices);

            var indexBufferData = frame.MapIndexList.ToArray();
            mapIndexBuffer = new IndexBuffer(BaseGame.Device, typeof(int), indexBufferData.Length, BufferUsage.None);
            mapIndexBuffer.SetData(indexBufferData);

            mapEffect.GraphicsDevice.SetVertexBuffer(mapVertexBuffer);
            mapEffect.GraphicsDevice.Indices = mapIndexBuffer;

            foreach (var pass in mapEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                mapEffect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, frame.MapVertexList.Count, 0, frame.MapIndexList.Count / 3);
            }
        }

        void drawObjects()
        {
            var cubeVertices = frame.ObjectVertexList.ToArray();

            spritesVertexBuffer = new VertexBuffer(BaseGame.Device, typeof(VertexPositionNormalTexture), cubeVertices.Length, BufferUsage.None);
            spritesVertexBuffer.SetData(cubeVertices);

            var indexBufferData = frame.ObjectIndexList.ToArray();
            spritesIndexBuffer = new IndexBuffer(BaseGame.Device, typeof(int), indexBufferData.Length, BufferUsage.None);
            spritesIndexBuffer.SetData(indexBufferData);

            spritesEffect.GraphicsDevice.SetVertexBuffer(spritesVertexBuffer);
            spritesEffect.GraphicsDevice.Indices = spritesIndexBuffer;

            foreach (var pass in spritesEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                spritesEffect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, frame.ObjectVertexList.Count, 0, frame.ObjectIndexList.Count / 3);
            }
        }
    }
}
