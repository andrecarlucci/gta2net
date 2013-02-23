using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Hiale.GTA2NET
{
    /// <summary>
    /// Base game class for all the basic game support.
    /// Connects all our helper classes together and makes our life easier!
    /// </summary>
    public class BaseGame : Microsoft.Xna.Framework.Game
    {
        /// <summary>
        /// Background color
        /// </summary>
        private static readonly Color BackgroundColor = Color.Red;
        /// <summary>
        /// Field of view and near and far plane distances for the
        /// ProjectionMatrix creation.
        /// </summary>
        private const float FieldOfView = MathHelper.PiOver2,
            NearPlane = 1,
            FarPlane = 1750;

        /// <summary>
        /// Graphics device manager, used for the graphics creation and holds
        /// the GraphicsDevice.
        /// </summary>
        public static GraphicsDeviceManager GraphicsManager = null;

        /// <summary>
        /// Our screen resolution: Width and height of visible render area.
        /// </summary>
        protected static int width;

        /// <summary>
        /// Our screen resolution: Width and height of visible render area.
        /// </summary>
        protected static int height;

        /// <summary>
        /// Aspect ratio of our current resolution
        /// </summary>
        private static float aspectRatio = 1.0f;

        /// <summary>
        /// Matrices for shaders. Used in a similar way than in Rocket Commander,
        /// but since we don't have a fixed function pipeline here we just use
        /// these values in the shader. Make sure to set all matrices before
        /// calling a shader. Inside a shader you have to update the shader
        /// parameter too, just setting the WorldMatrix alone does not work.
        /// </summary>
        private static Matrix worldMatrix,
            viewMatrix,
            projectionMatrix;

        /// <summary>
        /// Elapsed time this frame in ms. Always have something valid here
        /// in case we devide through this values!
        /// </summary>
        private static float elapsedTimeThisFrameInMs = 0.001f, totalTimeMs = 0,
            lastFrameTotalTimeMs = 0;

        /// <summary>
        /// Helper for calculating frames per second. 
        /// </summary>
        private static float startTimeThisSecond = 0;

        private static BlendState _alphaBlendState;
        public static BlendState AlphaBlendingState
        {
            get { return _alphaBlendState; }
        }

        /// <summary>
        /// For more accurate frames per second calculations,
        /// just count for one second, then fpsLastSecond is updated.
        /// Btw: Start with 1 to help some tests avoid the devide through zero
        /// problem.
        /// </summary>
        private static int
            frameCountThisSecond = 0,
            totalFrameCount = 0,
            fpsLastSecond = 60;

        /// <summary>
        /// Return true every checkMilliseconds.
        /// </summary>
        /// <param name="checkMilliseconds">Check ms</param>
        /// <returns>Bool</returns>
        public static bool EveryMillisecond(int checkMilliseconds)
        {
            return (int)(lastFrameTotalTimeMs / checkMilliseconds) !=
                (int)(totalTimeMs / checkMilliseconds);
        }

        static public GraphicsDevice Device
        {
            get
            {
                return GraphicsManager.GraphicsDevice;
            }
        }

        /// <summary>
        /// Back buffer depth format
        /// </summary>
        static DepthFormat backBufferDepthFormat = DepthFormat.Depth24; //XNA 3.1, was Depth32
        /// <summary>
        /// Back buffer depth format
        /// </summary>
        /// <returns>Surface format</returns>
        public static DepthFormat BackBufferDepthFormat
        {
            get
            {
                return backBufferDepthFormat;
            }
        }

        /// <summary>
        /// Fullscreen
        /// </summary>
        /// <returns>Bool</returns>
        public static bool Fullscreen
        {
            get
            {
                return GraphicsManager.IsFullScreen;
            }
        }

        /// <summary>
        /// Width
        /// </summary>
        /// <returns>Int</returns>
        public static int Width
        {
            get
            {
                return width;
            }
        }
        /// <summary>
        /// Height
        /// </summary>
        /// <returns>Int</returns>
        public static int Height
        {
            get
            {
                return height;
            }
        }

        /// <summary>
        /// Aspect ratio
        /// </summary>
        /// <returns>Float</returns>
        public static float AspectRatio
        {
            get
            {
                return aspectRatio;
            }
        }

        /// <summary>
        /// Fps
        /// </summary>
        /// <returns>Int</returns>
        public static int Fps
        {
            get
            {
                return fpsLastSecond;
            }
        }

        /// <summary>
        /// Interpolated fps over the last 10 seconds.
        /// Obviously goes down if our framerate is low.
        /// </summary>
        private static float fpsInterpolated = 100.0f;

        /// <summary>
        /// Total frames
        /// </summary>
        /// <returns>Int</returns>
        public static int TotalFrames
        {
            get
            {
                return totalFrameCount;
            }
        }

        /// <summary>
        /// Elapsed time this frame in ms
        /// </summary>
        /// <returns>Int</returns>
        public static float ElapsedTimeThisFrameInMilliseconds
        {
            get
            {
                return elapsedTimeThisFrameInMs;
            }
        }

        /// <summary>
        /// Total time in seconds
        /// </summary>
        /// <returns>Int</returns>
        public static float TotalTime
        {
            get
            {
                return totalTimeMs / 1000.0f;
            }
        }

        /// <summary>
        /// Total time ms
        /// </summary>
        /// <returns>Float</returns>
        public static float TotalTimeMilliseconds
        {
            get
            {
                return totalTimeMs;
            }
        }

        /// <summary>
        /// Move factor per second, when we got 1 fps, this will be 1.0f,
        /// when we got 100 fps, this will be 0.01f.
        /// </summary>
        public static float MoveFactorPerSecond
        {
            get
            {
                return elapsedTimeThisFrameInMs / 1000.0f;
            }
        }

        /// <summary>
        /// World matrix
        /// </summary>
        /// <returns>Matrix</returns>
        public static Matrix WorldMatrix
        {
            get
            {
                return worldMatrix;
            }
            set
            {
                worldMatrix = value;
            }
        }

        /// <summary>
        /// View matrix
        /// </summary>
        /// <returns>Matrix</returns>
        public static Matrix ViewMatrix
        {
            get
            {
                return viewMatrix;
            }
            set
            {
                // Set view matrix, usually only done in ChaseCamera.Update!
                viewMatrix = value;

                // Update camera pos and rotation, used all over the game!
                invViewMatrix = Matrix.Invert(viewMatrix);
                camPos = invViewMatrix.Translation;
                cameraRotation = Vector3.TransformNormal(
                    new Vector3(0, 0, 1), invViewMatrix);
            }
        }

        /// <summary>
        /// Projection matrix
        /// </summary>
        /// <returns>Matrix</returns>
        public static Matrix ProjectionMatrix
        {
            get
            {
                return projectionMatrix;
            }
            set
            {
                projectionMatrix = value;
            }
        }

        /// <summary>
        /// Camera pos, updated each frame in ViewMatrix!
        /// Public to allow easy access from everywhere, will be called a lot each
        /// frame, for example Model.Render uses this for distance checks.
        /// </summary>
        private static Vector3 camPos;

        /// <summary>
        /// Get camera position from inverse view matrix. Similar to method
        /// used in shader. Works only if ViewMatrix is correctly set.
        /// </summary>
        /// <returns>Vector 3</returns>
        public static Vector3 CameraPos
        {
            get
            {
                return camPos;
            }
        }

        /// <summary>
        /// Camera rotation, used to compare objects for visibility.
        /// </summary>
        private static Vector3 cameraRotation = new Vector3(0, 0, 1);

        /// <summary>
        /// Camera rotation
        /// </summary>
        /// <returns>Vector 3</returns>
        public static Vector3 CameraRotation
        {
            get
            {
                return cameraRotation;
            }
        }


        /// <summary>
        /// Remember inverse view matrix.
        /// </summary>
        private static Matrix invViewMatrix;

        /// <summary>
        /// Inverse view matrix
        /// </summary>
        /// <returns>Matrix</returns>
        public static Matrix InverseViewMatrix
        {
            get
            {
                return invViewMatrix;//Matrix.Invert(ViewMatrix);
            }
        }

        /// <summary>
        /// View projection matrix
        /// </summary>
        /// <returns>Matrix</returns>
        public static Matrix ViewProjectionMatrix
        {
            get
            {
                return ViewMatrix * ProjectionMatrix;
            }
        }

        /// <summary>
        /// World view projection matrix
        /// </summary>
        /// <returns>Matrix</returns>
        public static Matrix WorldViewProjectionMatrix
        {
            get
            {
                return WorldMatrix * ViewMatrix * ProjectionMatrix;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected BaseGame()
        {
            // Set graphics
            GraphicsManager = new GraphicsDeviceManager(this);

            ApplyResolutionChange();

            GraphicsManager.PreparingDeviceSettings +=  GraphicsPrepareDevice;

            #if DEBUG
            // Disable vertical retrace to get highest framerates possible for testing performance.
            GraphicsManager.SynchronizeWithVerticalRetrace = false;
            #endif
        }

        /// <summary>
        /// Initialize
        /// </summary>
        protected override void Initialize()
        {
            #if !XBOX360
            // Add screenshot capturer. Note: Don't do this in constructor,
            // we need the correct window name for screenshots!
            //this.Components.Add(new ScreenshotCapturer(this));
            #endif

            base.Initialize();

            //GameSettings.Initialize(); --> ToDo
            //ApplyResolutionChange();
            //Sound.SetVolumes(GameSettings.Default.SoundVolume, GameSettings.Default.MusicVolume);

            //Init the static screens
            //Highscores.Initialize();

            // Replaces static Constructors with simple inits.
            //Log.Initialize();

            // Set depth format
            backBufferDepthFormat = GraphicsManager.PreferredDepthStencilFormat;

            // Update resolution if it changes
            GraphicsManager.DeviceReset += GraphicsDeviceReset;
            GraphicsDeviceReset(null, EventArgs.Empty);

            // Create matrices for our shaders, this makes it much easier
            // to manage all the required matrices since there is no fixed
            // function support and theirfore no Device.Transform class.
            WorldMatrix = Matrix.Identity;

            // ViewMatrix is updated in camera class
            ViewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 250), Vector3.Zero, Vector3.Up);

            _alphaBlendState = CreateAlphaBlendingState();

            // Projection matrix is set by DeviceReset
        }

        /// <summary>
        /// Update
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Update all input states
            Input.Update();

            lastFrameTotalTimeMs = totalTimeMs;
            //elapsedTimeThisFrameInMs = (float)gameTime.ElapsedRealTime.TotalMilliseconds; //XNA 3.1
            elapsedTimeThisFrameInMs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            totalTimeMs += elapsedTimeThisFrameInMs;

            // Make sure elapsedTimeThisFrameInMs is never 0
            if (elapsedTimeThisFrameInMs <= 0)
                elapsedTimeThisFrameInMs = 0.001f;

            // Increase frame counter for FramesPerSecond
            frameCountThisSecond++;
            totalFrameCount++;

            // One second elapsed?
            if (totalTimeMs - startTimeThisSecond > 1000.0f)
            {
                // Calc fps
                fpsLastSecond = (int)(frameCountThisSecond * 1000.0f /
                    (totalTimeMs - startTimeThisSecond));

                // Reset startSecondTick and repaintCountSecond
                startTimeThisSecond = totalTimeMs;
                frameCountThisSecond = 0;

                fpsInterpolated =
                    MathHelper.Lerp(fpsInterpolated, fpsLastSecond, 0.1f);

                // Check out if our framerate is running very low. Then we can improve
                // rendering by reducing the number of objects we draw.
                //if (fpsInterpolated < 5)
                //    Model.MaxViewDistance = 50;
                //else if (fpsInterpolated < 12)
                //    Model.MaxViewDistance = 70;
                //else if (fpsInterpolated < 16)
                //    Model.MaxViewDistance = 90;
                //else if (fpsInterpolated < 20)
                //    Model.MaxViewDistance = 120;
                //else if (fpsInterpolated < 25)
                //    Model.MaxViewDistance = 150;
                //else if (fpsInterpolated < 30 ||
                //    HighDetail == false)
                //    Model.MaxViewDistance = 175;
            }

            // Update sound and music
            //Sound.Update();
        }

        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="gameTime">Game time</param>
        protected override void Draw(GameTime gameTime)
        {
            try
            {
                // Clear anyway, makes unit tests easier and fixes problems if
                // we don't have the z buffer cleared (some issues with line
                // rendering might happen else). Performance drop is not significant!
                ClearBackground();

                // Get our sprites ready to draw...
                //Texture.additiveSprite.Begin(SpriteBlendMode.Additive);
                //Texture.alphaSprite.Begin(SpriteBlendMode.AlphaBlend);

                // Handle custom user render code
                Render();

                // Render all models we remembered this frame.
                //meshRenderManager.Render();

                // Render all 3d lines
                //lineManager3D.Render();

                // Render UI and font texts, this also handles all collected
                // screen sprites (on top of 3d game code)
                //UIRenderer.Render(lineManager2D);

                //PostUIRender();

                //Handle drawing the Trophy
                //if (RacingGameManager.InGame && RacingGameManager.Player.Victory)
                //{
                //    Texture.alphaSprite.Begin(SpriteBlendMode.AlphaBlend);

                //    int rank = GameScreens.Highscores.GetRankFromCurrentTime(
                //        RacingGameManager.Player.LevelNum,
                //        (int)RacingGameManager.Player.BestTimeMilliseconds);

                //    // Show one of the trophies
                //    BaseGame.UI.GetTrophyTexture(
                //        // Select right one
                //        rank == 0 ? UIRenderer.TrophyType.Gold :
                //        rank == 1 ? UIRenderer.TrophyType.Silver :
                //        UIRenderer.TrophyType.Bronze).
                //        RenderOnScreen(new Rectangle(
                //        BaseGame.Width / 2 - BaseGame.Width / 8,
                //        BaseGame.Height / 2 - BaseGame.YToRes(10),
                //        BaseGame.Width / 4, BaseGame.Height * 2 / 5));

                //    Texture.alphaSprite.End();
                //}

                //ui.RenderTextsAndMouseCursor();
            }
            // Only catch exceptions here in release mode, when debugging
            // we want to see the source of the error. In release mode
            // we want to play and not be annoyed by some bugs ^^
            #if !DEBUG
            catch (Exception ex)
            {
                //Log.Write("Render loop error: " + ex.ToString());
                //if (renderLoopErrorCount++ > 100)
                //    throw;
            }
            #endif
            finally
            {
                // Dummy block to prevent error in debug mode
            }

            base.Draw(gameTime);

            // Apply device changes
            if (mustApplyDeviceChanges)
            {
                GraphicsManager.ApplyChanges();
                mustApplyDeviceChanges = false;
            }
        }

        /// <summary>
        /// Render
        /// </summary>
        protected virtual void Render()
        {

        }

        /// <summary>
        /// Clear background
        /// </summary>
        public static void ClearBackground()
        {
            Device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, BackgroundColor, 1.0f, 0);
            //Device.Clear(ClearOptions.Target, BackgroundColor, 0.0f, 0);
        }


        private void GraphicsPrepareDevice(object sender, PreparingDeviceSettingsEventArgs e)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                PresentationParameters presentParams =
                    e.GraphicsDeviceInformation.PresentationParameters;

                presentParams.RenderTargetUsage = RenderTargetUsage.PlatformContents;
                if (GraphicsManager.PreferredBackBufferHeight == 720)
                {
                    //presentParams.MultiSampleType = MultiSampleType.FourSamples; //XNA 3.1
                    #if !DEBUG
                    presentParams.PresentationInterval = PresentInterval.One;
                    #endif
                }
                else
                {
                    //presentParams.MultiSampleType = MultiSampleType.TwoSamples; //XNA 3.1
                    #if !DEBUG
                    presentParams.PresentationInterval = PresentInterval.Two;
                    #endif
                }
            }
        }

        private static void GraphicsDeviceReset(object sender, EventArgs e)
        {
            // Update width and height
            width = Device.Viewport.Width;
            height = Device.Viewport.Height;
            aspectRatio = (float)width / (float)height;
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(FieldOfView, aspectRatio, NearPlane, FarPlane);

            // Re-Set device
            // Restore z buffer state
            //BaseGame.Device.RenderState.DepthBufferEnable = true; //XNA 3.1
            //BaseGame.Device.RenderState.DepthBufferWriteEnable = true; //XNA 3.1
            Device.DepthStencilState = DepthStencilState.Default;
            Device.SamplerStates[0] = SamplerState.PointWrap;
            // Set u/v addressing back to wrap
            //BaseGame.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;  //not needed ???
            //BaseGame.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            // Restore normal alpha blending
            //BaseGame.Device.RenderState.AlphaBlendEnable = true; //XNA 3.1
            //BaseGame.Device.RenderState.SourceBlend = Blend.SourceAlpha; //XNA 3.1
            //BaseGame.Device.RenderState.DestinationBlend = Blend.InverseSourceAlpha; //XNA 3.1
            Device.BlendState = CreateAlphaBlendingState();

            // Set 128 and greate alpha compare for Model.Render
            //BaseGame.Device.RenderState.ReferenceAlpha = 128;
            //BaseGame.Device.RenderState.AlphaFunction = CompareFunction.Greater;

            // Recreate all render-targets
            //foreach (RenderToTexture renderToTexture in remRenderToTextures)
            //    renderToTexture.HandleDeviceReset();
        }

        private static bool mustApplyDeviceChanges = false;
        internal static void ApplyResolutionChange()
        {
            //Disabled by Hiale, use fixed for now
            int resolutionWidth = 1000;
            int resolutionHeight = 800;
            //int resolutionWidth = GameSettings.Default == null ? 0 :
            //    GameSettings.Default.ResolutionWidth;
            //int resolutionHeight = GameSettings.Default == null ? 0 :
            //    GameSettings.Default.ResolutionHeight;

            // Use current desktop resolution if autodetect is selected.
            if (resolutionWidth <= 0 ||
                resolutionHeight <= 0)
            {
                resolutionWidth =
                    GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                resolutionHeight =
                    GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }

            #if XBOX360
            // Xbox 360 graphics settings are fixed
            graphicsManager.IsFullScreen = true;
            graphicsManager.PreferredBackBufferWidth =
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphicsManager.PreferredBackBufferHeight =
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            #else
            GraphicsManager.PreferredBackBufferWidth = resolutionWidth;
            GraphicsManager.PreferredBackBufferHeight = resolutionHeight;
            //GraphicsManager.IsFullScreen = false; //ANX not implemented

            mustApplyDeviceChanges = true;
            #endif
        }

        public static BlendState CreateAlphaBlendingState()
        {
            var blendState = new BlendState();
            blendState.ColorBlendFunction = BlendFunction.Add;
            blendState.ColorSourceBlend = Blend.SourceAlpha;
            blendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
            blendState.AlphaBlendFunction = BlendFunction.Add;
            blendState.AlphaSourceBlend = Blend.SourceAlpha;
            blendState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
            return blendState;
        }

        #region Helper methods for 3d-calculations
        /// <summary>
        /// Epsilon (1/1000000) for comparing stuff which is nearly equal.
        /// </summary>
        public const float Epsilon = 0.000001f;

        /// <summary>
        /// Convert 3D vector to 2D vector, this is kinda the oposite of
        /// GetScreenPlaneVector (not shown here). This can be useful for user
        /// input/output, because we will often need the actual position on screen
        /// of an object in 3D space from the users view to handle it the right
        /// way. Used for lens flare and asteroid optimizations.
        /// </summary>
        /// <param name="point">3D world position</param>
        /// <return>Resulting 2D screen position</return>
        public static Point Convert3DPointTo2D(Vector3 point)
        {
            Vector4 result4 = Vector4.Transform(point,
                ViewProjectionMatrix);

            if (result4.W == 0)
                result4.W = BaseGame.Epsilon;
            Vector3 result = new Vector3(
                result4.X / result4.W,
                result4.Y / result4.W,
                result4.Z / result4.W);

            // Output result from 3D to 2D
            return new Point(
                (int)Math.Round(+result.X * (width / 2)) + (width / 2),
                (int)Math.Round(-result.Y * (height / 2)) + (height / 2));
        }

        public static Vector3 Convert2DPointTo3D(Vector2 point)
        {
            return Device.Viewport.Unproject(new Vector3(point.X, point.Y, 0), projectionMatrix, viewMatrix, worldMatrix);
        }

        /// <summary>
        /// Is point in front of camera?
        /// </summary>
        /// <param name="point">Position to check.</param>
        /// <returns>Bool</returns>
        public static bool IsInFrontOfCamera(Vector3 point)
        {
            Vector4 result = Vector4.Transform(
                new Vector4(point.X, point.Y, point.Z, 1),
                ViewProjectionMatrix);

            // Is result in front?
            return result.Z > result.W - NearPlane;
        }

        /// <summary>
        /// Helper to check if a 3d-point is visible on the screen.
        /// Will basically do the same as IsInFrontOfCamera and Convert3DPointTo2D,
        /// but requires less code and is faster. Also returns just an bool.
        /// Will return true if point is visble on screen, false otherwise.
        /// Use the offset parameter to include points into the screen that are
        /// only a couple of pixel outside of it.
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="checkOffset">Check offset in percent of total
        /// screen</param>
        /// <returns>Bool</returns>
        public static bool IsVisible(Vector3 point, float checkOffset)
        {
            Vector4 result = Vector4.Transform(
                new Vector4(point.X, point.Y, point.Z, 1),
                ViewProjectionMatrix);

            // Point must be in front of camera, else just skip everything.
            if (result.Z > result.W - NearPlane)
            {
                Vector2 screenPoint = new Vector2(
                    result.X / result.W, result.Y / result.W);

                // Change checkOffset depending on how depth we are into the scene
                // for very near objects (z < 5) pass all tests!
                // for very far objects (z >> 5) only pass if near to +- 1.0f
                float zDist = Math.Abs(result.Z);
                if (zDist < 5.0f)
                    return true;
                checkOffset = 1.0f + (checkOffset / zDist);

                return
                    screenPoint.X >= -checkOffset && screenPoint.X <= +checkOffset &&
                    screenPoint.Y >= -checkOffset && screenPoint.Y <= +checkOffset;
            }

            // Point is not in front of camera, return false.
            return false;
        }
        #endregion

        #region Application active
        // Check if app is currently active
        static bool isAppActive = true;
        /// <summary>
        /// Is app active
        /// </summary>
        /// <returns>Bool</returns>
        public static bool IsAppActive
        {
            get
            {
                return isAppActive;
            }
        }

        /// <summary>
        /// On activated
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        protected override void OnActivated(object sender, EventArgs args)
        {
            base.OnActivated(sender, args);
            isAppActive = true;
        }

        /// <summary>
        /// On deactivated
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        protected override void OnDeactivated(object sender, EventArgs args)
        {
            base.OnDeactivated(sender, args);
            isAppActive = false;
        }
        #endregion
    }
}
