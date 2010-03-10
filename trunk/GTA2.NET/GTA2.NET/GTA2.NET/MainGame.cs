using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Hiale.GTA2NET.Core;
using Hiale.GTA2NET.GameScreens;
using Hiale.GTA2NET.Core.Map;
using Hiale.GTA2NET.Core.Style;
using Hiale.GTA2NET.Helper;
using Hiale.GTA2NET.Logic;

namespace Hiale.GTA2NET
{
    public class MainGame : BaseGame
    {
        private static readonly Stack<IGameScreen> GameScreens = new Stack<IGameScreen>();

        private static Vector3 _globalScalar = Vector3.One;
        /// <summary>
        /// Scalar of the scene
        /// </summary>
        public static Vector3 GlobalScalar
        {
            get { return _globalScalar; }
            set { _globalScalar = value; }
        }

        /// <summary>
        /// Current _map
        /// </summary>
        private static Map _map;
        public static Map Map
        {
            get { return MainGame._map; }
            internal set { MainGame._map = value; }
        }

        private static Style _style;
        /// <summary>
        /// Current _style
        /// </summary>
        public static Style Style
        {
            get { return MainGame._style; }
            set { MainGame._style = value; }
        }

        /// <summary>
        /// All available car data.
        /// </summary>
        public static List<CarInfo> CarInfos { get; private set; }

        private static EventList<MovableObject> _cars;
        /// <summary>
        /// The cars currently drive on the map.
        /// </summary>
        public static EventList<MovableObject> Cars
        {
            get { return MainGame._cars; }
            set { MainGame._cars = value; }
        }

        private static EventList<MovableObject> _pedestrians;
        /// <summary>
        /// The pedestrian currently walks along the map
        /// </summary>
        public static EventList<MovableObject> Pedestrians
        {
            get { return MainGame._pedestrians; }
            set { MainGame._pedestrians = value; }
        }

        private static MovableObject _chasingObject;
        /// <summary>
        /// The object (guy or car) which the player is controlling. The camera chases this object.
        /// </summary>
        public static MovableObject ChasingObject
        {
            get { return MainGame._chasingObject; }
            set { MainGame._chasingObject = value; }
        }

        private const float RotationScalar = 0.05f;
        private const float ForwardScalar = 0.004f;

        private static string _windowTitle;
        /// <summary>
        /// Title of the game window;
        /// </summary>
        public static string WindowTitle
        {
            get
            {
                if (string.IsNullOrEmpty(_windowTitle))
                    _windowTitle = "GTA2.NET";
                return _windowTitle;
            }
            set { _windowTitle = value; }
        }

        /// <summary>
        /// In menu
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

        private static float _elapsedGameTime;
        /// <summary>
        /// Time elapsed since the last call to update in ms.
        /// </summary>
        public static float ElapsedGameTime
        {
            get { return _elapsedGameTime; }
            set { _elapsedGameTime = value; }
        }

        /// <summary>
        /// Create Game
        /// </summary>
        public MainGame() : base()
        {
            //
        }

        Hiale.GTA2NET.Renderer.UIRenderer uiRenderer; //ToDo: intigrate better!!!
        
        /// <summary>
        /// Load stuff
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            _cars = new EventList<MovableObject>();
            _pedestrians = new EventList<MovableObject>();

            //_chasingObject = new Car();
            //_chasingObject.Position = new Vector3(65, 181, GetHighestPoint(65, 181));

            MainGame._style = new Style();
            _style.ReadFromFile("data\\bil.sty");

            Dictionary<int, CarPhysics> carPhysics = CarPhysicReader.ReadFromFile();
            CarInfos = CarInfo.CreateCarInfoCollection(Style.CarInfos, carPhysics);

            BaseGame.ViewMatrix = Matrix.CreateLookAt(new Vector3(65, -181, 10), new Vector3(65, -181, 0), Vector3.Up);
            GameScreens.Push(new InGameScreen());

            uiRenderer = new Hiale.GTA2NET.Renderer.UIRenderer();
            IsFixedTimeStep = false;
            //
            //...

            _chasingObject = new Car(new Vector3(69, 186, GetHighestPoint(69, 186)), CarInfos[9]);
            Cars.Add(_chasingObject);
        }

        /// <summary>
        /// Update
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Update game engine
            base.Update(gameTime);


            _elapsedGameTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            

            // Update player and game logic
            HandleInput();
            Window.Title = WindowTitle + " - " + Fps.ToString() + " fps";
            //System.Threading.Thread.Sleep(20);
        }

        private float ForwardAmount; //ToDo: better name
        private float RotationAbount;

        private void HandleInput()
        {
            Vector3 cameraPos = BaseGame.CameraPos;
            if (Input.KeyboardUpPressed)
            {
                ForwardAmount = ForwardScalar * _elapsedGameTime;
            }
            if (Input.KeyboardDownPressed)
            {
                ForwardAmount = -ForwardScalar * _elapsedGameTime;
            }
            if (Input.KeyboardLeftPressed)
            {
                RotationAbount = -RotationScalar * _elapsedGameTime;
            }
            if (Input.KeyboardRightPressed)
            {
                RotationAbount = RotationScalar * _elapsedGameTime;
            }

            //if (ForwardAmount != 0)
            _chasingObject.Move(ref ForwardAmount, ref RotationAbount, ref _elapsedGameTime);
            

            if (_chasingObject != null)
            {
                Vector3 lookAt = _chasingObject.Position3;
                lookAt.Y *= -1;
                cameraPos = lookAt;
                cameraPos.Z += 10 * GlobalScalar.Z;
                BaseGame.ViewMatrix = Matrix.CreateLookAt(cameraPos, lookAt, Vector3.Up);
            }

            ForwardAmount = 0;
            RotationAbount = 0;
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

        public static float GetHighestPointF(float x, float y)
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
            {
                if (block.IsLowSlope || block.IsHighSlope)
                {
                    switch (block.SlopeType)
                    {
                        case SlopeType.Up26Low:
                            {
                                int roundedY = (int)y + 1;
                                float offset = y - roundedY;
                                return (z - 1) - (offset / 2);
                            }
                        case SlopeType.Up26High:
                            {
                                int roundedY = (int)y + 1;
                                float offset = roundedY - y;
                                return (z - 0.5f) + (offset / 2);
                            }
                        case SlopeType.Right26Low:
                            {
                                int roundedX = (int)x;
                                float offset = x - roundedX;
                                return (z - 1) + (offset / 2);
                            }
                        case SlopeType.Right26High:
                            {
                                int roundedX = (int)x;
                                float offset = x - roundedX;
                                return (z - 0.5f) + (offset / 2);
                            }
                        case SlopeType.Down26Low:
                            {
                                int roundedY = (int)y;
                                float offset = y - roundedY;
                                return (z - 1) + (offset / 2);
                            }
                        case SlopeType.Down26High:
                            {
                                int roundedY = (int)y;
                                float offset = y - roundedY;
                                return (z - 0.5f) + (offset / 2);
                            }
                        case SlopeType.Left26Low:
                            {
                                int roundedX = (int)x + 1;
                                float offset = x - roundedX;
                                return (z - 1) - (offset / 2);
                            }
                        case SlopeType.Left26High:
                            {
                                int roundedX = (int)x + 1;
                                float offset = roundedX - x;
                                return (z - 0.5f) + (offset / 2);
                            }
                    }
                }
                return z;
            }
            return -1;
        }

        //public static void RotateSprite(ref Hiale.GTA2NET.Helper.FaceCoordinates coordinates, ref float rotation)
        //{
        //    Vector2 center = new Vector2((coordinates.BottomRight.X - coordinates.TopLeft.X) / 2 + coordinates.TopLeft.X, (coordinates.BottomLeft.Y - coordinates.TopRight.Y) / 2 + coordinates.TopRight.Y);
        //    RotateSprite(ref coordinates, ref rotation, ref center);
        //}

        //public static void RotateSprite(ref Hiale.GTA2NET.Helper.FaceCoordinates coordinates, ref float rotation, ref Vector2 origin)
        //{
        //    float radiants = MathHelper.ToRadians(rotation);
        //    coordinates.TopLeft = RotatePoint(coordinates.TopLeft, ref radiants, ref origin);
        //    coordinates.TopRight = RotatePoint(coordinates.TopRight, ref radiants, ref origin);
        //    coordinates.BottomRight = RotatePoint(coordinates.BottomRight, ref radiants, ref origin);
        //    coordinates.BottomLeft = RotatePoint(coordinates.BottomLeft, ref radiants, ref origin);
        //}

        ////Idea: Create a lookat table for speed!
        //private static Vector3 RotatePoint(Vector3 point, ref float radians, ref Vector2 origin)
        //{
        //    return new Vector3(origin.X + ((float)Math.Cos(radians) * (point.X - origin.X) - (float)Math.Sin(radians) * (point.Y - origin.Y)), origin.Y + ((float)Math.Sin(radians) * (point.X - origin.X) + (float)Math.Cos(radians) * (point.Y - origin.Y)), point.Z);
        //}


    }
}
