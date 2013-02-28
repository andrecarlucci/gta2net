
using Hiale.GTA2NET.Renderer;

namespace Hiale.GTA2NET.GameScreens
{
    public class InGameScreen : IGameScreen
    {
        readonly CityRenderer _cityRenderer;
        readonly SpriteRenderer _spriteRenderer;

        public InGameScreen()
        {
            _cityRenderer = new CityRenderer();
            _cityRenderer.LoadCity();
            _spriteRenderer = new SpriteRenderer();
            _spriteRenderer.LoadSprites();
        }

        public bool Render()
        {            
            if (Input.GamePadBackJustPressed) //move to another place
                return true;        
            _cityRenderer.DrawCity();
            _spriteRenderer.DrawSprites();
            return false;
        }
    }
}
