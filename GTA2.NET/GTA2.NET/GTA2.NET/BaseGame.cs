// GTA2.NET
// 
// File: BaseGame.cs
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;


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
        private static readonly Color BackgroundColor = Color.Black;

        /// <summary>
        /// Field of view and near and far plane distances for the
        /// ProjectionMatrix creation.
        /// </summary>
        private const float FieldOfView = MathHelper.PiOver2, //PiOver2 = 90°C
                            NearPlane = 1,
                            FarPlane = 20;

        /// <summary>
        /// Graphics device manager, used for the graphics creation and holds
        /// the GraphicsDevice.
        /// </summary>
        public static GraphicsDeviceManager GraphicsManager = null;

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

        public static BlendState AlphaBlendingState { get; set; }

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
        static DepthFormat backBufferDepthFormat = DepthFormat.Depth24;
        
        /// <summary>
        /// Width
        /// </summary>
        /// <returns>Int</returns>
        public static int Width { get; set; }

        /// <summary>
        /// Height
        /// </summary>
        /// <returns>Int</returns>
        public static int Height { get; set; }

        /// <summary>
        /// Aspect ratio
        /// </summary>
        /// <returns>Float</returns>
        public static float AspectRatio
        {
            get
            {
                return Width / (float)Height;
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
            base.Initialize();

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

            AlphaBlendingState = CreateAlphaBlendingState();
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
                fpsLastSecond = (int)(frameCountThisSecond * 1000.0f / (totalTimeMs - startTimeThisSecond));

                // Reset startSecondTick and repaintCountSecond
                startTimeThisSecond = totalTimeMs;
                frameCountThisSecond = 0;

                fpsInterpolated = MathHelper.Lerp(fpsInterpolated, fpsLastSecond, 0.1f);
            }
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
            }
            // Only catch exceptions here in release mode, when debugging
            // we want to see the source of the error. In release mode
            // we want to play and not be annoyed by some bugs ^^
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
        }

        private void GraphicsPrepareDevice(object sender, PreparingDeviceSettingsEventArgs e)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                PresentationParameters presentParams =
                    e.GraphicsDeviceInformation.PresentationParameters;

                presentParams.RenderTargetUsage = RenderTargetUsage.PlatformContents;
            }
        }

        private static void GraphicsDeviceReset(object sender, EventArgs e)
        {
            // Update width and height
            Width = Device.Viewport.Width;
            Height = Device.Viewport.Height;
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);

            // Re-Set device
            // Restore z buffer state
            Device.DepthStencilState = DepthStencilState.Default;
            Device.SamplerStates[0] = SamplerState.PointWrap;
            Device.BlendState = CreateAlphaBlendingState();
        }

        private static bool mustApplyDeviceChanges = false;
        internal static void ApplyResolutionChange()
        {
            //Disabled by Hiale, use fixed for now
            int resolutionWidth = 1600;
            int resolutionHeight = 900;

            // Use current desktop resolution if autodetect is selected.
            if (resolutionWidth <= 0 || resolutionHeight <= 0)
            {
                resolutionWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                resolutionHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }

            GraphicsManager.PreferredBackBufferWidth = resolutionWidth;
            GraphicsManager.PreferredBackBufferHeight = resolutionHeight;
            GraphicsManager.IsFullScreen = false;

            mustApplyDeviceChanges = true;
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

        #region Application active
        // Check if app is currently active
        static bool isAppActive = true;


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
