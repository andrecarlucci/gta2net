using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Renderer
{
    public class UIRenderer
    {
        SpriteBatch spriteBatch;
        Texture2D mousePointerTex;
        Texture2D nullTex;

        public UIRenderer()
        {
            spriteBatch = new SpriteBatch(BaseGame.Device);
            FileStream fs = new FileStream("textures\\MouseCursor.png", FileMode.Open);
            mousePointerTex = Texture2D.FromStream(BaseGame.Device, fs);
            fs.Close();
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
            Vector2 Origin = new Vector2(0.5f, 0.0f);
            Vector2 diff = b - a;
            float angle;
            Vector2 Scale = new Vector2(1.0f, diff.Length() / nullTex.Height);
            angle = (float)(Math.Atan2(diff.Y, diff.X)) - MathHelper.PiOver2;
            spriteBatch.Draw(nullTex, a, null, Color.Black, angle, Origin, Scale, SpriteEffects.None, 1.0f);            
        }

    }
}
