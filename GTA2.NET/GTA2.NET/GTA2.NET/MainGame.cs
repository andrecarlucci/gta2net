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
using Hiale.GTA2NET.Core.Logic;
using Hiale.GTA2NET.Core.Map;
using Hiale.GTA2NET.GameScreens;
using J2i.Net.XInputWrapper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Hiale.GTA2NET
{
    public class MainGame : BaseGame
    {
        private static readonly Stack<IGameScreen> GameScreens = new Stack<IGameScreen>();

        /// <summary>
        /// Scalar of the scene
        /// </summary>
        public static Vector3 GlobalScalar { get; set; }

        public static Map Map { get; internal set; }

        /// <summary>
        /// Current style (Maybe this will be deleted later, when all information are extracted)
        /// </summary>
        public static string StyleName { get; set; }

        /// <summary>
        /// All available car data.
        /// </summary>
        public static List<CarInfo> CarInfoList { get; private set; }

        /// <summary>
        /// The cars currently drive on the map.
        /// </summary>
        public static ObservableCollection<Car> Cars { get; private set; }

        /// <summary>
        /// The object (guy or car) which the player is controlling. The camera chases this object.
        /// </summary>
        public static Car ChasingObject { get; private set; }

        public const float RotationScalar = 50; //was 0.05
        public const float ForwardScalar = 10;

        //Physic stuff
        private Physics _physics;

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
        }

        Renderer.UiRenderer uiRenderer; //ToDo: intigrate better!!!
        
        static MainGame()
        {
            GlobalScalar = Vector3.One;
        }
        
        /// <summary>
        /// Load stuff
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            var handle = WinUI.NativeWin32.FindWindow(null, Window.Title); //Window.Handle seems to be wrong!
            WinUI.NativeWin32.SetWindowPos(handle, 0, 0, 0, Width, Height, 0);

            ViewMatrix = Matrix.CreateLookAt(new Vector3(65, -181, 10), new Vector3(65, -181, 0), Vector3.Up);

            uiRenderer = new Renderer.UiRenderer();

            LoadMap();
        }

        private void LoadMap()
        {
            Map = new Map(Globals.MapsSubDir + "\\MP1-comp.gmp", "bil");
            StyleName = "bil";

            var carInfo = CarInfo.Deserialize(Globals.MiscSubDir + "\\car" + Globals.XmlFormat);

            _physics = new Physics();
            _physics.Initialize(Map);

            var carPhysics = CarPhysicReader.ReadFromFile();
            CarInfoList = CarInfo.CreateCarInfoCollection(carInfo, carPhysics);

            Cars = new ObservableCollection<Car>();

            ChasingObject = new Car(new Vector3(70, 186, GetHighestPoint(70, 186)), 0, CarInfoList[10]);
            ChasingObject.PlayerControlled = true;
            _physics.AddObject((Car)ChasingObject);
            Cars.Add((Car) ChasingObject);

            _physics.Debug((Car) ChasingObject);

            GameScreens.Push(new InGameScreen());
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
                _physics.SaveWorld("GTA2NET_update.json");
                Exit();
            }

            var elapsedGameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var input = new PlayerInput();
            HandleInput(ref input, controller);
            ChasingObject.Update(input, elapsedGameTime);
           
            if (ChasingObject != null)
            {
                Vector3 lookAt = ChasingObject.Position3;
                lookAt.Y *= -1;
                Vector3 cameraPos = lookAt;
                cameraPos.Z += 10 * GlobalScalar.Z;
                ViewMatrix = Matrix.CreateLookAt(cameraPos, lookAt, Vector3.Up);
            }

            _physics.Update(gameTime);

            Window.Title = "GTA2.NET - " + WindowTitle + Fps.ToString(CultureInfo.InvariantCulture) + " fps";
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
            // No more game screens?
            if (GameScreens.Count == 0)
            {
                Exit();
                return;
            }

            // Handle current screen
            if (GameScreens.Peek().Render())
            {
                GameScreens.Pop();
            }
            uiRenderer.DrawMouseMounter();
        }

        public static int GetHighestPoint(int x, int y)
        {
            if (x >= 0 && x < Map.Width && y >= 0 && y < Map.Length)
            {
                for (int z = Map.Height - 1; z >= 0; z--)
                {
                    if (!Map.GetBlock(new Vector3(x, y, z)).IsEmpty)
                        return z;
                }
            }
            return -1;
        }
    }
}
