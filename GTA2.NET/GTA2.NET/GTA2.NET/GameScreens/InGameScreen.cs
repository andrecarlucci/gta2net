using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Hiale.GTA2NET.Renderer;

namespace Hiale.GTA2NET.GameScreens
{
    public class InGameScreen : IGameScreen
    {
        readonly CityRenderer cityRenderer;
        readonly SpriteRenderer spriteRenderer;

        public InGameScreen()
        {
            cityRenderer = new CityRenderer();
            cityRenderer.LoadCity();
            spriteRenderer = new SpriteRenderer();
            spriteRenderer.LoadSprites();
        }

        public bool Render()
        {            
            if (Input.GamePadBackJustPressed) //move to another place
                return true;            

            cityRenderer.DrawCity();
            spriteRenderer.DrawSprites();

            return false;
        }
    }
}
