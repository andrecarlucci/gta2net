using System;
using ANX.Framework.Graphics;
using ANX.Framework;
using Color = ANX.Framework.Color;

namespace Hiale.GTA2NET.Renderer
{
    public class UiRenderer
    {
        SpriteBatch spriteBatch;
        Texture2D mousePointerTex;
        Texture2D nullTex;

        public UiRenderer()
        {
            spriteBatch = new SpriteBatch(BaseGame.Device);
            //FileStream fs = new FileStream("textures\\MouseCursor.png", FileMode.Open);
            //mousePointerTex = Texture2D.FromStream(BaseGame.Device, fs); //ANX-not implemented
            //mousePointerTex = new Texture2D(BaseGame.Device, 64, 64);
            //fs.Close();
            mousePointerTex = MainGame.Content.Load<Texture2D>("MouseCursor");
            //nullTex = Texture2D.FromFile(BaseGame.Device, "textures\\Null.bmp");
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

        void DrawLine(Vector2 a, Vector2 b)
        {
            var origin = new Vector2(0.5f, 0.0f);
            var diff = b - a;
            var scale = new Vector2(1.0f, diff.Length() / nullTex.Height);
            var angle = (float)(Math.Atan2(diff.Y, diff.X)) - MathHelper.PiOver2;
            spriteBatch.Draw(nullTex, a, null, Color.Black, angle, origin, scale, SpriteEffects.None, 1.0f);            
        }

    }
}
