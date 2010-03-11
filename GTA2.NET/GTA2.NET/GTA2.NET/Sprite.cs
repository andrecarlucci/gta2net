//Created: 15.02.2010

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

        //private float _rotation; //Note: BaseObject field?
        ///// <summary>
        ///// Angle in degrees of this sprite.
        ///// </summary>
        //public float Rotation
        //{
        //    get { return _rotation; }
        //    set
        //    {
        //        if (value < 0)
        //            value = 360 + value;
        //        if (value >= 360)
        //            value = 0;
        //        _rotation = value;
        //    }
        //}

        private int spriteIndex;
        public int SpriteIndex
        {
            get { return spriteIndex; }
        }

        public Vector2 TexturePositionTopLeft { get; private set; }

        public Vector2 TexturePositionTopRight { get; private set; }

        public Vector2 TexturePositionBottomRight { get; private set; }

        public Vector2 TexturePositionBottomLeft { get; private set; }

        private Vector3 _scalar;

        public Sprite(MovableObject baseObject, Vector3 position, int spriteIndex, Texture2D texture, IDictionary<SpriteItem, Rectangle> spriteDictionary)
        {
            _scalar = Vector3.One;  
            SpriteItem item = new SpriteItem();

            //Bus
            item.Sprite = 10;
            item.Model = 11;
            item.Remap = 4;

            //Cop
            //item.Sprite = 11;
            //item.Model = 12;
            //item.Remap = 36;

            Rectangle sourceRectangle = spriteDictionary[item];
            _scalar.X = sourceRectangle.Width / 64f; //1 Unit = 64px
            _scalar.Y = sourceRectangle.Height / 64f;
            baseObject.SetDimension(sourceRectangle.Width/64f, sourceRectangle.Height/64f);


            SetNeutralPosition(position);


            this.spriteIndex = spriteIndex;

            //texture
            Vector2 baseCoordinate = new Vector2(sourceRectangle.X, sourceRectangle.Y);
            Vector2 textureSize = new Vector2(texture.Width, texture.Height);
            TexturePositionTopLeft = baseCoordinate / textureSize;
            TexturePositionTopRight = (baseCoordinate + new Vector2(sourceRectangle.Width - 1, 0)) / textureSize;
            TexturePositionBottomRight = (baseCoordinate + new Vector2(sourceRectangle.Width - 1, sourceRectangle.Height - 1)) / textureSize;
            TexturePositionBottomLeft = (baseCoordinate + new Vector2(0, sourceRectangle.Height - 1)) / textureSize;
        }

        private void SetNeutralPosition(Vector3 position)
        {
            TranslatePosition(ref position);

            TopLeft = (new Vector3(-0.5f * _scalar.X, 0.5f * _scalar.Y, 0.0f) + position);
            TopRight = (new Vector3(0.5f * _scalar.X, 0.5f * _scalar.Y, 0.0f) + position);
            BottomLeft = (new Vector3(-0.5f * _scalar.X, -0.5f * _scalar.Y, 0.0f) + position);
            BottomRight = (new Vector3(0.5f * _scalar.X, -0.5f * _scalar.Y, 0.0f) + position);
        }

        public void SetPosition(MovableObject baseObject)
        {
            Vector3 currentPoint = baseObject.TopLeft3;
            TranslatePosition(ref currentPoint);
            TopLeft = currentPoint;

            currentPoint = baseObject.TopRight3;
            TranslatePosition(ref currentPoint);
            TopRight = currentPoint;

            currentPoint = baseObject.BottomLeft3;
            TranslatePosition(ref currentPoint);
            BottomLeft = currentPoint;

            currentPoint = baseObject.BottomRight3;
            TranslatePosition(ref currentPoint);
            BottomRight = currentPoint;
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
