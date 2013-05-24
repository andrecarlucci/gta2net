// GTA2.NET
// 
// File: InGameScreen.cs
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
