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
        private Vector3 _topLeft;
        /// <summary>
        /// Coordinate of the top left point of the sprite.
        /// </summary>
        public Vector3 TopLeft
        {
            get { return _topLeft; }
            set { _topLeft = value; }
        }

        private Vector3 _topRight;
        /// <summary>
        /// Coordinate of the top right point of the sprite.
        /// </summary>
        public Vector3 TopRight
        {
            get { return _topRight; }
            set { _topRight = value; }
        }

        private Vector3 _bottomRight;
        /// <summary>
        /// Coordinate of the bottom right point of the sprite.
        /// </summary>
        public Vector3 BottomRight
        {
            get { return _bottomRight; }
            set { _bottomRight = value; }
        }

        private Vector3 _bottomLeft;
        /// <summary>
        /// Coordinate of the bottom left point of the sprite.
        /// </summary>
        public Vector3 BottomLeft
        {
            get { return _bottomLeft; }
            set { _bottomLeft = value; }
        }

        private float _rotation; //Note: BaseObject field?
        /// <summary>
        /// Angle in degrees of this sprite.
        /// </summary>
        public float Rotation
        {
            get { return _rotation; }
            set
            {
                if (value < 0)
                    value = 360 + value;
                if (value >= 360)
                    value = 0;
                _rotation = value;
            }
        }

        private int spriteIndex;
        public int SpriteIndex
        {
            get { return spriteIndex; }
        }

        private Vector2 _texturePositionTopLeft;
        public Vector2 TexturePositionTopLeft
        {
            get { return _texturePositionTopLeft; }
            set { _texturePositionTopLeft = value; }
        }

        private Vector2 _texturePositionTopRight;
        public Vector2 TexturePositionTopRight
        {
            get { return _texturePositionTopRight; }
            set { _texturePositionTopRight = value; }
        }

        private Vector2 _texturePositionBottomRight;
        public Vector2 TexturePositionBottomRight
        {
            get { return _texturePositionBottomRight; }
            set { _texturePositionBottomRight = value; }
        }

        private Vector2 _texturePositionBottomLeft;
        public Vector2 TexturePositionBottomLeft
        {
            get { return _texturePositionBottomLeft; }
            set { _texturePositionBottomLeft = value; }
        }

        private Vector3 _scalar;

        public Sprite(MovableObject baseObject, Vector3 position, int spriteIndex, Texture2D texture, IDictionary<int, Rectangle> spriteDictionary)
        {
            _scalar = Vector3.One;  
            Rectangle sourceRectangle = spriteDictionary[spriteIndex];
            _scalar.X = sourceRectangle.Width / 64f; //1 Unit = 64px
            _scalar.Y = sourceRectangle.Height / 64f;
            baseObject.SetDimension(sourceRectangle.Width/64f, sourceRectangle.Height/64f);


            SetNeutralPosition(position);


            this.spriteIndex = spriteIndex;

            //texture
            Vector2 baseCoordinate = new Vector2(sourceRectangle.X, sourceRectangle.Y);
            //Vector2 baseCoordinate = new Vector2(sourceRectangle.X + 1, sourceRectangle.Y);
            Vector2 textureSize = new Vector2(texture.Width, texture.Height);
            _texturePositionTopLeft = baseCoordinate / textureSize;
            _texturePositionTopRight = (baseCoordinate + new Vector2(sourceRectangle.Width - 1, 0)) / textureSize;
            _texturePositionBottomRight = (baseCoordinate + new Vector2(sourceRectangle.Width - 1, sourceRectangle.Height - 1)) / textureSize;
            _texturePositionBottomLeft = (baseCoordinate + new Vector2(0, sourceRectangle.Height - 1)) / textureSize;
        }

        private void SetNeutralPosition(Vector3 position)
        {
            TranslatePosition(ref position);

            //Mittelpunkt links oben
            //_topLeft = (new Vector3(0.0f, 0.0f, 0.0f) + position);
            //_topRight = (new Vector3(_scalar.X, 0.0f, 0.0f) + position);
            //_bottomLeft = (new Vector3(0.0f, -_scalar.Y, 0.0f) + position);
            //_bottomRight = (new Vector3(_scalar.X, -_scalar.Y, 0.0f) + position);

            //Mittelpunkt zentriert
            _topLeft = (new Vector3(-0.5f * _scalar.X, 0.5f * _scalar.Y, 0.0f) + position);
            _topRight = (new Vector3(0.5f * _scalar.X, 0.5f * _scalar.Y, 0.0f) + position);
            _bottomLeft = (new Vector3(-0.5f * _scalar.X, -0.5f * _scalar.Y, 0.0f) + position);
            _bottomRight = (new Vector3(0.5f * _scalar.X, -0.5f * _scalar.Y, 0.0f) + position);
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
            //check if object on a slope (is not an integer)
            //display the sprite then a little above, to avoid graphical issuses.
            if (position.Z % 1 != 0)
                position.Z += 0.0001f;
            position.Y *= -1;
            position.Z++;
            position.Z *= MainGame.GlobalScalar.Z;
        }
    }
}
