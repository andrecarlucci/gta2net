// GTA2.NET
// 
// File: Sprite.cs
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
using System;
using System.Collections.Generic;
using Hiale.GTA2NET.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Hiale.GTA2NET.Logic;

namespace Hiale.GTA2NET
{
    public class Sprite
    {
        /// <summary>
        /// Width of the sprite.
        /// </summary>
        public float Width { get; private set; }

        /// <summary>
        /// Height of the sprite.
        /// </summary>
        public float Height { get; private set; }

        /// <summary>
        /// Coordinate of the top left point of the sprite.
        /// </summary>
        public Vector3 TopLeft { get; private set; }

        /// <summary>
        /// Coordinate of the top right point of the sprite.
        /// </summary>
        public Vector3 TopRight { get; private set; }

        /// <summary>
        /// Coordinate of the bottom right point of the sprite.
        /// </summary>
        public Vector3 BottomRight { get; private set; }

        /// <summary>
        /// Coordinate of the bottom left point of the sprite.
        /// </summary>
        public Vector3 BottomLeft { get; private set; }

        public int SpriteIndex { get; private set; }

        public Vector2 TexturePositionTopLeft { get; private set; }

        public Vector2 TexturePositionTopRight { get; private set; }

        public Vector2 TexturePositionBottomRight { get; private set; }

        public Vector2 TexturePositionBottomLeft { get; private set; }

        private readonly float _widthHalf;

        private readonly float _heightHalf;

        public Sprite(GameplayObject baseObject, Vector3 position, int spriteIndex, Texture2D texture, IDictionary<SpriteItem, Rectangle> spriteDictionary)
        {
            SpriteItem item = new SpriteItem();

            //Bus
            //item.Sprite = 10;
            //item.Model = 11;
            //item.Remap = 4;

            //Cop
            //item.Sprite = 11;
            //item.Model = 12;
            //item.Remap = 36;

            item.Sprite = 9;
            item.Model = 10;
            item.Remap = 29;

            Rectangle sourceRectangle = spriteDictionary[item];
            Width = sourceRectangle.Width / 64f; //1 Unit = 64px
            Height = sourceRectangle.Height / 64f;
            _widthHalf = Width / 2;
            _heightHalf = Height / 2;

            SetNeutralPosition(position);


            this.SpriteIndex = spriteIndex;

            //Texture
            double pixelPerWidth = 1f / texture.Width;
            double pixelPerHeight = 1f / texture.Height;

            TexturePositionTopLeft = new Vector2((float)((sourceRectangle.X + 1) * pixelPerWidth), (float)((sourceRectangle.Y + 1) * pixelPerHeight));
            TexturePositionTopRight = new Vector2((float)((sourceRectangle.X + sourceRectangle.Width - 1) * pixelPerWidth), (float)((sourceRectangle.Y + 1) * pixelPerHeight));
            TexturePositionBottomRight = new Vector2((float)((sourceRectangle.X + sourceRectangle.Width - 1) * pixelPerWidth), (float)((sourceRectangle.Y + sourceRectangle.Height - 1) * pixelPerHeight));
            TexturePositionBottomLeft = new Vector2((float)((sourceRectangle.X + 1) * pixelPerWidth), (float)((sourceRectangle.Y + sourceRectangle.Height - 1) * pixelPerHeight));
        }

        private void SetNeutralPosition(Vector3 position)
        {
            TranslatePosition(ref position);

            TopLeft = (new Vector3(-0.5f * Width, 0.5f * Height, 0.0f) + position);
            TopRight = (new Vector3(0.5f * Width, 0.5f * Height, 0.0f) + position);
            BottomLeft = (new Vector3(-0.5f * Width, -0.5f * Height, 0.0f) + position);
            BottomRight = (new Vector3(0.5f * Width, -0.5f * Height, 0.0f) + position);
        }

        public void SetPosition(GameplayObject baseObject)
        {
            Vector3 center = baseObject.Position3;
            float rotation = baseObject.RotationAngle;

            Vector3 currentPoint = new Vector3(center.X - _widthHalf, center.Y - _heightHalf, center.Z);
            currentPoint = MainGame.RotatePoint3(currentPoint, center, rotation);
            TranslatePosition(ref currentPoint);
            TopLeft = currentPoint;

            currentPoint = new Vector3(center.X + _widthHalf, center.Y - _heightHalf, center.Z);
            currentPoint = MainGame.RotatePoint3(currentPoint, center, rotation);
            TranslatePosition(ref currentPoint);
            TopRight = currentPoint;

            currentPoint = new Vector3(center.X + _widthHalf, center.Y + _heightHalf, center.Z);
            currentPoint = MainGame.RotatePoint3(currentPoint, center, rotation);
            TranslatePosition(ref currentPoint);
            BottomRight = currentPoint;

            currentPoint = new Vector3(center.X - _widthHalf, center.Y + _heightHalf, center.Z);
            currentPoint = MainGame.RotatePoint3(currentPoint, center, rotation);
            TranslatePosition(ref currentPoint);
            BottomLeft = currentPoint;
        }


        /// <summary>
        /// Translate a "game position" into a 3d position.
        /// </summary>
        /// <param name="position"></param>
        private static void TranslatePosition(ref Vector3 position)
        {
            position.Y *= -1;
            position.Z++;
            position.Z *= MainGame.GlobalScalar.Z;
        }
    }
}
