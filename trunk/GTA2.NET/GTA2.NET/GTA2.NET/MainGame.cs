using System;
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

        /// <summary>
        /// Scalar of the scene
        /// </summary>
        public static Vector3 GlobalScalar { get; set; }

        public static Map Map { get; internal set; }

        /// <summary>
        /// Current style (Maybe this will be deleted later, when all information are extracted)
        /// </summary>
        public static Style Style { get; set; }

        //public static RandomHelper RandomHelper { get; set; }

        /// <summary>
        /// All available car data.
        /// </summary>
        public static List<CarInfo> CarInfos { get; private set; }

        private static EventList<GameplayObject> _cars;
        /// <summary>
        /// The cars currently drive on the map.
        /// </summary>
        public static EventList<GameplayObject> Cars
        {
            get { return MainGame._cars; }
            set { MainGame._cars = value; }
        }

        private static EventList<GameplayObject> _pedestrians;
        /// <summary>
        /// The pedestrian currently walks along the map
        /// </summary>
        public static EventList<GameplayObject> Pedestrians
        {
            get { return MainGame._pedestrians; }
            set { MainGame._pedestrians = value; }
        }


        private static GameplayObject _chasingObject;
        /// <summary>
        /// The object (guy or car) which the player is controlling. The camera chases this object.
        /// </summary>
        public static GameplayObject ChasingObject
        {
            get { return MainGame._chasingObject; }
            set { MainGame._chasingObject = value; }
        }

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

        Hiale.GTA2NET.Renderer.UIRenderer uiRenderer; //ToDo: intigrate better!!!

        AI ai;

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

            _cars = new EventList<GameplayObject>();
            _pedestrians = new EventList<GameplayObject>();

            //_chasingObject = new Car();
            //_chasingObject.Position = new Vector3(65, 181, GetHighestPoint(65, 181));

            MainGame.Style = new Style();
            Style.ReadFromFile("data\\bil.sty");

            Dictionary<int, CarPhysics> carPhysics = CarPhysicReader.ReadFromFile();
            CarInfos = CarInfo.CreateCarInfoCollection(Style.CarInfos, carPhysics);

            BaseGame.ViewMatrix = Matrix.CreateLookAt(new Vector3(65, -181, 10), new Vector3(65, -181, 0), Vector3.Up);
            GameScreens.Push(new InGameScreen());

            uiRenderer = new Hiale.GTA2NET.Renderer.UIRenderer();
            IsFixedTimeStep = false;
            //
            //...

            _chasingObject = new Car(new Vector3(70, 186, GetHighestPoint(70, 186)), CarInfos[9]);
            _chasingObject.PlayerControlled = true;
            //_chasingObject.RotationAngle = MathHelper.ToRadians(90);
            ai = new AI(_chasingObject);
            Cars.Add(_chasingObject);
        }

        //private double _playerForwardDelta;
        //private double _playerRotationDelta;

        /// <summary>
        /// Update
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Update game engine
            base.Update(gameTime);


            float elapsedGameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            PlayerInput input = new PlayerInput();
            HandleInput(ref input);
            _chasingObject.SetPlayerInput(input, elapsedGameTime);

            UpdateObjects(elapsedGameTime);

            
            if (_chasingObject != null)
            {
                Vector3 cameraPos;
                Vector3 lookAt = _chasingObject.Position3;
                lookAt.Y *= -1;
                cameraPos = lookAt;
                cameraPos.Z += 10 * GlobalScalar.Z;
                BaseGame.ViewMatrix = Matrix.CreateLookAt(cameraPos, lookAt, Vector3.Up);
            }
            


            Window.Title = "GTA2.NET - " + WindowTitle + Fps.ToString() + " fps";
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
