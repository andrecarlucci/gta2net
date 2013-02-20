using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ANX.Framework;
using Hiale.GTA2NET.Renderer;

namespace Hiale.GTA2NET.GameScreens
{
    public class InGameScreen : IGameScreen
    {
        CityRenderer cityRenderer;
        SpriteRenderer spriteRenderer;

        public InGameScreen()
        {
            cityRenderer = new CityRenderer();
            cityRenderer.LoadCity();
            spriteRenderer = new SpriteRenderer();
            spriteRenderer.LoadSprites();
        }

        public bool Render()
        {            
            if (Input.GamePadBackJustPressed)
                return true;            

            cityRenderer.DrawCity();
            spriteRenderer.DrawSprites();

            return false;
        }
    }
}
