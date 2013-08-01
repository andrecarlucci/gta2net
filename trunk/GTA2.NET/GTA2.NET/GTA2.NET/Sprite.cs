// GTA2.NET
// 
// File: Sprite.cs
// Created: 15.02.2010
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
using Hiale.GTA2NET.Core;
using Hiale.GTA2NET.Core.Logic;
using Hiale.GTA2NET.Core.Style;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        public Sprite(GameplayObject baseObject, Vector3 position, int spriteIndex, Texture2D texture, IDictionary<int, SpriteItem> spriteDictionary)
        {
            var item = new SpriteItem(SpriteType.Car);

            //Bus
            //item.Sprite = 10;
            //item.Model = 11;
            //item.Remap = 4;

            //Cop
            //item.Sprite = 11;
            //item.Model = 12;
            //item.Remap = 36;

            item.SpriteId = 10;
            //item.Model = 10;
            //item.Remap = -1;

            var sourceRectangle = spriteDictionary[item.SpriteId].Rectangle;
            Width = sourceRectangle.Width / 64f; //1 Unit = 64px
            Height = sourceRectangle.Height / 64f;
            _widthHalf = Width / 2;
            _heightHalf = Height / 2;

            SetNeutralPosition(position);


            SpriteIndex = spriteIndex;

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

        public void SetPosition(ISprite baseObject)
        {
            Vector3 center = baseObject.Position3;
            float rotation = baseObject.RotationAngle;

            TopLeft = TranslatePosition(new Vector3(center.X + baseObject.SpriteTopLeft.X, center.Y + baseObject.SpriteTopLeft.Y, baseObject.Position3.Z));
            TopRight = TranslatePosition(new Vector3(center.X + baseObject.SpriteTopRight.X, center.Y + baseObject.SpriteTopRight.Y, baseObject.Position3.Z));
            BottomRight = TranslatePosition(new Vector3(center.X + baseObject.SpriteBottomRight.X, center.Y + baseObject.SpriteBottomRight.Y, baseObject.Position3.Z));
            BottomLeft = TranslatePosition(new Vector3(center.X + baseObject.SpriteBottomLeft.X, center.Y + baseObject.SpriteBottomLeft.Y, baseObject.Position3.Z));


            //Vector3 currentPoint = new Vector3(center.X - _widthHalf, center.Y - _heightHalf, center.Z);
            //currentPoint = MainGame.RotatePoint3(currentPoint, center, rotation);
            //TranslatePosition(ref currentPoint);
            ////TopLeft = currentPoint;

            //currentPoint = new Vector3(center.X + _widthHalf, center.Y - _heightHalf, center.Z);
            //currentPoint = MainGame.RotatePoint3(currentPoint, center, rotation);
            //TranslatePosition(ref currentPoint);
            ////TopRight = currentPoint;

            //currentPoint = new Vector3(center.X + _widthHalf, center.Y + _heightHalf, center.Z);
            //currentPoint = MainGame.RotatePoint3(currentPoint, center, rotation);
            //TranslatePosition(ref currentPoint);
            ////BottomRight = currentPoint;

            //currentPoint = new Vector3(center.X - _widthHalf, center.Y + _heightHalf, center.Z);
            //currentPoint = MainGame.RotatePoint3(currentPoint, center, rotation);
            //TranslatePosition(ref currentPoint);
            ////BottomLeft = currentPoint;
        }


        private static Vector3 TranslatePosition(Vector3 position)
        {
            TranslatePosition(ref position);
            return position;
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
