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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Hiale.GTA2NET.Core.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Hiale.GTA2NET.Core;
using Hiale.GTA2NET.GameScreens;
using Hiale.GTA2NET.Core.Map;
using Hiale.GTA2NET.Core.Style;
using Hiale.GTA2NET.Helper;
using Hiale.GTA2NET.Logic;
using Microsoft.Xna.Framework.Input;

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
        public static Style Style { get; set; }

        /// <summary>
        /// All available car data.
        /// </summary>
        public static List<CarInfo> CarInfos { get; private set; }

        /// <summary>
        /// The cars currently drive on the map.
        /// </summary>
        public static ObservableCollection<Car> Cars { get; private set; }

        /// <summary>
        /// The pedestrian currently walks along the map
        /// </summary>
        public static ObservableCollection<Car> Pedestrians { get; private set; }


        /// <summary>
        /// The object (guy or car) which the player is controlling. The camera chases this object.
        /// </summary>
        public static GameplayObject ChasingObject { get; private set; }

        public const float RotationScalar = 50; //was 0.05
        public const float ForwardScalar = 10;

        //Physic stuff
        private World _world;

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
        /// In menu?
        /// </summary>
        /// <returns>Bool</returns>
        public static bool InMenu
        {
            get
            {
                //return GameScreens.Count > 0 &&
                //    GameScreens.Peek().GetType() != typeof(GameScreen);
                return false;
            }
        }

        /// <summary>
        /// In game?
        /// </summary>
        public static bool InGame
        {
            get
            {
                //return GameScreens.Count > 0 &&
                //    GameScreens.Peek().GetType() == typeof(GameScreen);
                return true;
            }
        }

        /// <summary>
        /// ShowMouseCursor
        /// </summary>
        /// <returns>Bool</returns>
        public static bool ShowMouseCursor
        {
            get
            {
                // Only if not in Game, not in splash screen!
                //return GameScreens.Count > 0 &&
                //    GameScreens.Peek().GetType() != typeof(GameScreen) &&
                //    GameScreens.Peek().GetType() != typeof(SplashScreen);
                return true;
            }
        }

        //private static double _elapsedGameTime;
        ///// <summary>
        ///// Time elapsed since the last call to update in ms.
        ///// </summary>
        //public static double ElapsedGameTime
        //{
        //    get { return _elapsedGameTime; }
        //    set { _elapsedGameTime = value; }
        //}

        /// <summary>
        /// Create Game
        /// </summary>
        public MainGame() : base()
        {
            //
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

            //_chasingObject = new Car();
            //_chasingObject.Position = new Vector3(65, 181, GetHighestPoint(65, 181));

            ViewMatrix = Matrix.CreateLookAt(new Vector3(65, -181, 10), new Vector3(65, -181, 0), Vector3.Up);
            //GameScreens.Push(new InGameScreen());

            uiRenderer = new Renderer.UiRenderer();
            //IsFixedTimeStep = false;

            LoadMap();
        }

        private void LoadMap()
        {
            Map = new Map("data\\MP1-comp.gmp");
            //Map = new Map("data\\bil.gmp");

            Style = new Style();
            Style.ReadFromFile("data\\bil.sty");

            //var collision = new MapCollision(Map);
            //collision.CollisionMap(new Vector2(73,192));
            SetupPhysics();

            var carPhysics = CarPhysicReader.ReadFromFile();
            CarInfos = CarInfo.CreateCarInfoCollection(Style.CarInfos, carPhysics);

            Cars = new ObservableCollection<Car>();
            Pedestrians = new ObservableCollection<Car>();

            ChasingObject = new Car(new Vector3(70, 186, GetHighestPoint(70, 186)), CarInfos[9]);
            ChasingObject.PlayerControlled = true;
            //_chasingObject.RotationAngle = MathHelper.ToRadians(90);
            Cars.Add((Car) ChasingObject);

            GameScreens.Push(new InGameScreen());
        }

        private void SetupPhysics()
        {
            if (_world == null)
                _world = new World(new Vector2(0, 10f));
            else
                _world.Clear();
            
            var collision = new MapCollision(Map);
            var obstacles = collision.CollisionMap(new Vector2(73, 192));
            foreach (var obstacle in obstacles)
            {
                Body body = null;
                if (obstacle is RectangleObstacle)
                {
                    var rectObstacle = (RectangleObstacle)obstacle;
                    body = BodyFactory.CreateRectangle(_world, rectObstacle.Width, rectObstacle.Length, 1,
                                                       new Vector2(
                                                           rectObstacle.Position.X + ((float) rectObstacle.Width/2),
                                                           rectObstacle.Position.Y + (float) rectObstacle.Length/2));
                }
                else if (obstacle is PolygonObstacle)
                {
                    var polygonObstacle = (PolygonObstacle) obstacle;
                    try
                    {
                        var verticesList = BayazitDecomposer.ConvexPartition(new Vertices(polygonObstacle.Vertices));
                        body = BodyFactory.CreateCompoundPolygon(_world, verticesList, 1f);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.StackTrace);
                    }
                }
                else if (obstacle is LineObstacle)
                {
                    var lineObstacle = (LineObstacle) obstacle;
                    body = BodyFactory.CreateEdge(_world, lineObstacle.Start, lineObstacle.End);

                }
                if (body != null)
                    body.BodyType = BodyType.Static;
            }
        }


        private bool leftClicked;

        /// <summary>
        /// Update
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Update game engine
            base.Update(gameTime);

            _world.Step((float)(gameTime.ElapsedGameTime.TotalMilliseconds * 0.001));

            var mouseState = Mouse.GetState();

            float elapsedGameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var input = new PlayerInput();
            HandleInput(ref input);
            ChasingObject.SetPlayerInput(input, elapsedGameTime);
            

            UpdateObjects(elapsedGameTime);

            
            if (ChasingObject != null)
            {
                Vector3 lookAt = ChasingObject.Position3;
                lookAt.Y *= -1;
                Vector3 cameraPos = lookAt;
                cameraPos.Z += 10 * GlobalScalar.Z;
                ViewMatrix = Matrix.CreateLookAt(cameraPos, lookAt, Vector3.Up);
            }

            if (mouseState.LeftButton == ButtonState.Pressed && !leftClicked)
            {
                System.Diagnostics.Debug.WriteLine(FindWhereClicked(mouseState));
                leftClicked = true;
            }
            else if (mouseState.LeftButton == ButtonState.Released)
                leftClicked = false;


            Window.Title = "GTA2.NET - " + WindowTitle + Fps.ToString(CultureInfo.InvariantCulture) + " fps";
            //System.Threading.Thread.Sleep(50);
        }

        private void UpdateObjects(float elapsedGameTime)
        {
            //Update all cars
            for (int i = 0; i < Cars.Count; i++)
            {
                Cars[i].Update(elapsedGameTime);
            }

            //Update all pedestrians
            for (int i = 0; i < Pedestrians.Count; i++)
            {
                Pedestrians[i].Update(elapsedGameTime);
            }
        }



        private void HandleInput(ref PlayerInput playerInput)
        {
            //Vector3 cameraPos = BaseGame.CameraPos;
            if (Input.KeyboardUpPressed)
            {
                //_playerForwardDelta = ForwardScalar * _elapsedGameTime;
                playerInput.Forward = 1;
            }
            if (Input.KeyboardDownPressed)
            {
                //_playerForwardDelta = -ForwardScalar * _elapsedGameTime;
                playerInput.Forward = -1;
            }
            if (Input.KeyboardLeftPressed)
            {
                //_playerRotationDelta = -RotationScalar * _elapsedGameTime;
                playerInput.Rotation = -1;
            }
            if (Input.KeyboardRightPressed)
            {
                //_playerRotationDelta = RotationScalar * _elapsedGameTime;
                playerInput.Rotation = 1;
            }

            //float a = 0;
            //float b = 0;
            //ai.DoSomething(ref a, ref b);
            //if (a > 0)
            //    ForwardAmount = ForwardScalar * _elapsedGameTime;
            //else if (a < 0)
            //    ForwardAmount = -ForwardScalar * _elapsedGameTime;

            //if (b > 0)
            //    RotationAbount = -RotationScalar * _elapsedGameTime;
            //else if (b < 0)
            //    RotationAbount = RotationScalar * _elapsedGameTime;


            //if (ForwardAmount != 0)
            //_chasingObject.Move(ref _playerForwardDelta, ref _playerRotationDelta, ref _elapsedGameTime);
            

            //if (_chasingObject != null)
            //{
            //    Vector3 lookAt = _chasingObject.Position3;
            //    lookAt.Y *= -1;
            //    cameraPos = lookAt;
            //    cameraPos.Z += 10 * GlobalScalar.Z;
            //    BaseGame.ViewMatrix = Matrix.CreateLookAt(cameraPos, lookAt, Vector3.Up);
            //}

            //_playerForwardDelta = 0;
            //_playerRotationDelta = 0;
        }

        public static Ray ClickRay;

        private Ray FindWhereClicked(MouseState ms)
        {
            var nearScreenPoint = new Vector3(ms.X, ms.Y, 0);
            var farScreenPoint = new Vector3(ms.X, ms.Y, 1);
            var nearWorldPoint = Device.Viewport.Unproject(nearScreenPoint, ProjectionMatrix, ViewMatrix, Matrix.Identity);
            var farWorldPoint = Device.Viewport.Unproject(farScreenPoint, ProjectionMatrix, ViewMatrix, Matrix.Identity);

            var direction = farWorldPoint - nearWorldPoint;
            direction.Normalize();

            var ray = new Ray(nearWorldPoint, direction);

            ClickRay = ray;

            //var xmin = Math.Min(Math.Abs(nearWorldPoint.X), Math.Abs(farWorldPoint.X));
            //var xmax = Math.Max(Math.Abs(nearWorldPoint.X), Math.Abs(farWorldPoint.X)) + 1;
            //var ymin = Math.Min(Math.Abs(nearWorldPoint.Y), Math.Abs(farWorldPoint.Y));
            //var ymax = Math.Max(Math.Abs(nearWorldPoint.Y), Math.Abs(farWorldPoint.Y)) + 1;
            //var zmin = Math.Min(Math.Abs(nearWorldPoint.Z), Math.Abs(farWorldPoint.Z));
            //zmin = 0;
            //var zmax = Math.Max(Math.Abs(nearWorldPoint.Z), Math.Abs(farWorldPoint.Z)) + 1;

            //for (int z = (int) zmax; z >= zmin; z-- )
            //{
            //    for (int x = (int) xmin; x <= xmax; x++)
            //    {
            //        for (int y = (int) ymin; y <= ymax; y++)
            //        {
            //            BoundingBox box = new BoundingBox(new Vector3(x,y,z), new Vector3(x+1,y+1,z+1));
            //            if (ray.Intersects(box).HasValue)
            //                System.Diagnostics.Debug.WriteLine("OK");
            //        }
            //    }
            //}
            return ray;
        }

        //protected override void Draw(GameTime gameTime)
        //{
        //    base.Draw(gameTime);

        //    System.Diagnostics.Debug.WriteIf(false, false);
        //}

        /// <summary>
        /// Render
        /// </summary>
        protected override void Render()
        {
            // No more game screens?
            if (GameScreens.Count == 0)
            {
                // Before quiting, stop music and play crash sound :)
                //Sound.PlayCrashSound(true);
                //Sound.StopMusic();

                // Then quit
                Exit();
                return;
            }

            // Handle current screen
            if (GameScreens.Peek().Render())
            {
                //// If this was the options screen and the resolution has changed,
                //// apply the changes
                //if (GameScreens.Peek().GetType() == typeof(Options) &&
                //    (BaseGame.Width != GameSettings.Default.ResolutionWidth ||
                //    BaseGame.Height != GameSettings.Default.ResolutionHeight ||
                //    BaseGame.Fullscreen != GameSettings.Default.Fullscreen))
                //{
                //    BaseGame.ApplyResolutionChange();
                //}

                //// Play sound for screen back
                //Sound.Play(Sound.Sounds.ScreenBack);

                GameScreens.Pop();
            }
            uiRenderer.DrawMouseMounter();
        }

        /// <summary>
        /// Add game screen
        /// </summary>
        /// <param name="gameScreen">Game screen</param>
        public static void AddGameScreen(IGameScreen gameScreen)
        {
            // Play sound for screen click
            //Sound.Play(Sound.Sounds.ScreenClick);

            // Add the game screen
            GameScreens.Push(gameScreen);
        }

        public static int GetHighestPoint(int x, int y)
        {
            if (x >= 0 && x < Map.CityBlocks.GetLength(0) && y >= 0 && y < Map.CityBlocks.GetLength(1))
            {
                for (int z = Map.CityBlocks.GetLength(2) - 1; z >= 0; z--)
                {
                    if (!Map.CityBlocks[x, y, z].IsEmpty)
                        return z;
                }
            }
            return -1;
        }

        public static float GetHighestPointF(float x, float y) //not used at the moment
        {
            if (x >= 0 && x < Map.CityBlocks.GetLength(0) && y >= 0 && y < Map.CityBlocks.GetLength(1))
            {
                for (int z = Map.CityBlocks.GetLength(2) - 1; z >= 0; z--)
                {
                    float currentZ = GetHeightF(ref x, ref y, ref z);
                    if (currentZ > -1)
                        return currentZ;
                }
            }
            return -1;
        }

        public static float GetHeightF(ref float x, ref float y, ref int z)
        {
            BlockInfo block = Map.CityBlocks[(int)x, (int)y, z];
            if (!block.IsEmpty)
                return block.IsMovableSlope ? GetSlopeHeight(ref block, ref x, ref y, ref z) : z;
            return -1;
        }

        private static float GetSlopeHeight(ref BlockInfo block, ref float x, ref float y, ref int z)
        {
            float xy;
            int rounded;
            float offset;
            float stepPerUnit = 1f;
            switch (block.SlopeDirection)
            {
                case SlopeDirection.Up:
                    xy = y;
                    rounded = (int) (xy + 1);
                    break;
                case SlopeDirection.Down:
                    xy = y;
                    rounded = (int) xy;
                    break;
                case SlopeDirection.Left:
                    xy = x;
                    rounded = (int) (xy + 1);
                    break;
                case SlopeDirection.Right:
                    xy = x;
                    rounded = (int) xy;
                    break;
                default:
                    return -1; //Shound not happen!
            }
            offset = xy - rounded;
            offset = Math.Abs(offset);
            byte slopeSubType = block.SlopeSubTyp;
            switch (slopeSubType)
            {
                case 26:
                    stepPerUnit = 2;
                    break;
                case 7:
                    stepPerUnit = 8;
                    break;
            }
            switch (block.SlopeType)
            {
                case SlopeType.Up26Low:
                    return (z - 1) + (offset / stepPerUnit);
                case SlopeType.Up26High:
                    return (z - 0.5f) + (offset / stepPerUnit);
                case SlopeType.Down26Low:
                    return (z - 1) + (offset / stepPerUnit);
                case SlopeType.Down26High:
                    return (z - 0.5f) + (offset / stepPerUnit);
                case SlopeType.Left26Low:
                    return (z - 1) + (offset / stepPerUnit);
                case SlopeType.Left26High:
                    return (z - 0.5f) + (offset / stepPerUnit);
                case SlopeType.Right26Low:
                    return (z - 1) + (offset / stepPerUnit);
                case SlopeType.Right26High:
                    return (z - 0.5f) + (offset / stepPerUnit);
                //ToDo: add more slopes!
            }
            return z;
        }

        /// <summary>
        /// Rotate a point from a given location and adjust using the Origin we
        /// are rotating around
        /// </summary>
        /// <param name="point">The point to rotate.</param>
        /// <param name="origin">Used origin for the rotation.</param>
        /// <param name="rotation">Angle in radians.</param>
        /// <returns></returns>
        public static Vector2 RotatePoint(Vector2 point, Vector2 origin, float rotation)
        {
            float outX = 0;
            float outY = 0;
            RotatePointInternal(point.X, point.Y, origin.X, origin.Y, rotation, ref outX, ref outY);
            return new Vector2(outX, outY);
        }

        /// <summary>
        /// Rotate a point from a given location and adjust using the Origin we
        /// are rotating around
        /// </summary>
        /// <param name="point">The point to rotate.</param>
        /// <param name="origin">Used origin for the rotation.</param>
        /// <param name="rotation">Angle in radians.</param>
        /// <returns></returns>
        public static Vector3 RotatePoint3(Vector3 point, Vector3 origin, float rotation)
        {
            float outX = 0;
            float outY = 0;
            RotatePointInternal(point.X, point.Y, origin.X, origin.Y, rotation, ref outX, ref outY);
            return new Vector3(outX, outY, point.Z);
        }

        private static void RotatePointInternal(float inX, float inY, float originX, float originY, float rotation, ref float outX, ref float outY)
        {
            outX = (float)(originX + (inX - originX) * Math.Cos(rotation) - (inY - originY) * Math.Sin(rotation));
            outY = (float)(originY + (inY - originY) * Math.Cos(rotation) + (inX - originX) * Math.Sin(rotation));
        }
    }
}
