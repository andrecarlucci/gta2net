using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace Hiale.GTA2NET.Renderer
{
    public class UiRenderer
    {
        readonly SpriteBatch spriteBatch;
        readonly Texture2D mousePointerTex;
        readonly Texture2D nullTex;

        public UiRenderer()
        {
            spriteBatch = new SpriteBatch(BaseGame.Device);
            var fs = new FileStream("textures\\MouseCursor.png", FileMode.Open);
            mousePointerTex = Texture2D.FromStream(BaseGame.Device, fs);
            fs.Close();
            //mousePointerTex = MainGame.Content.Load<Texture2D>("MouseCursor");
            nullTex = new Texture2D(BaseGame.Device, 1, 1);
            Int32[] pixel = { 0xFF }; // White. 0xFF is Red, 0xFF0000 is Blue
            nullTex.SetData(pixel);
        }

        public void DrawMouseMounter()
        {
            spriteBatch.Begin();

            //Lines
            //DrawLine(new Vector2(0, BaseGame.Height), new Vector2(BaseGame.Width, 0));
            //DrawLine(new Vector2(0, 0), new Vector2(BaseGame.Width, BaseGame.Height));

            spriteBatch.Draw(mousePointerTex, new Vector2(Input.MousePos.X, Input.MousePos.Y), null, Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0f);
            spriteBatch.End();
        }

        private void DrawLine(Vector2 a, Vector2 b)
        {
            var origin = new Vector2(0.5f, 0.0f);
            var diff = b - a;
            var scale = new Vector2(1.0f, diff.Length() / nullTex.Height);
            var angle = (float)(Math.Atan2(diff.Y, diff.X)) - MathHelper.PiOver2;
            spriteBatch.Draw(nullTex, a, null, Color.White, angle, origin, scale, SpriteEffects.None, 1.0f);            
        }

    }
}
