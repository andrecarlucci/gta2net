// GTA2.NET
// 
// File: Down45.cs
// Created: 03.08.2013
// Created by: João Pires
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
using System.Drawing;
using System.IO;
using Hiale.GTA2NET.Core.Helper;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Map
{
    /// <summary>
    /// Responsable for store the textures to be used in the map.
    /// </summary>
    public class Textures
    {
        public enum SquareTexturePosition{TopLeft, TopCenter, TopRight, CenterLeft, CenterCenter, CenterRight, BottonLeft, BottonCenter, BottonRight};
        public enum RectangleTexturePosition{Top, CenterHorizontal, Botton, Left, CenterVertical, Right};

        private Dictionary<int, CompactRectangle> tileAtlas;
        private float pixelPerWidth;
        private float pixelPerHeight;

        /// <summary>
        /// Creates a new instance of Textures
        /// </summary>
        /// <param name="name">The name of texture file to load.</param>
        public Textures(String name)
        {
            String path = Globals.GraphicsSubDir + Path.DirectorySeparatorChar + name + Globals.TilesSuffix;

            TextureAtlasTiles dict = TextureAtlas.Deserialize<TextureAtlasTiles>(path+Globals.XmlFormat);
            tileAtlas = dict.TileDictionary;

            Bitmap bmp = (Bitmap)Image.FromFile(path+Globals.TextureImageFormat);
            pixelPerHeight = 1f / bmp.Height;
            pixelPerWidth = 1f / bmp.Width;
        }

        /// <summary>
        /// Gets the coordinates for the texture acording to its rotation and flip.
        /// </summary>
        /// <remarks>The return values are stored 0->BottonLeft; 1->BottonRight; 2->TopRight; 3->TopLeft</remarks>
        /// <param name="tileID">The ID of the texture.</param>
        /// <param name="rotation">The rotation to aply to the texture.</param>
        /// <param name="flip">If the Texture must be fliped.</param>
        /// <returns>A array with 4 positions where each position represent one of the vertices od the texture, it will be a square with 64px.</returns>
        public Vector2[] GetNormalTexture(UInt32 tileID, RotationType rotation, Boolean flip)
        {
            var texturePosition = new Vector2[4];
            CompactRectangle tex = tileAtlas[(int)tileID];

            var texTopLeft = new Vector2((tex.X + 1) * pixelPerWidth, (tex.Y + 1) * pixelPerHeight);
            var texTopRight = new Vector2((tex.X + tex.Width - 1) * pixelPerWidth, (tex.Y + 1) * pixelPerHeight);
            var texBottomLeft = new Vector2((tex.X + 1) * pixelPerWidth, (tex.Y + tex.Height - 1) * pixelPerHeight);
            var texBottomRight = new Vector2((tex.X + tex.Width - 1) * pixelPerWidth, (tex.Y + tex.Height - 1) * pixelPerHeight);

            if (flip)
            {
                Vector2 helper = texTopLeft;
                texTopLeft = texTopRight;
                texTopRight = helper;

                helper = texBottomLeft;
                texBottomLeft = texBottomRight;
                texBottomRight = helper;

                if (rotation == RotationType.Rotate90) //special cases.
                {
                    rotation = RotationType.Rotate270;
                }
                else if (rotation == RotationType.Rotate270)
                {
                    rotation = RotationType.Rotate90;
                }
            }

            switch (rotation)
            {
                case RotationType.RotateNone:
                    texturePosition[0] = texBottomLeft;
                    texturePosition[1] = texBottomRight;
                    texturePosition[2] = texTopRight;
                    texturePosition[3] = texTopLeft;
                    break;
                case RotationType.Rotate90:
                    texturePosition[3] = texBottomLeft;
                    texturePosition[0] = texBottomRight;
                    texturePosition[1] = texTopRight;
                    texturePosition[2] = texTopLeft;
                    break;
                case RotationType.Rotate180:
                    texturePosition[2] = texBottomLeft;
                    texturePosition[3] = texBottomRight;
                    texturePosition[0] = texTopRight;
                    texturePosition[1] = texTopLeft;
                    break;
                case RotationType.Rotate270:
                    texturePosition[1] = texBottomLeft;
                    texturePosition[2] = texBottomRight;
                    texturePosition[3] = texTopRight;
                    texturePosition[0] = texTopLeft;
                    break;
            }
            return texturePosition;
        }

        /// <summary>
        /// Gets the coordinates for the texture acording to its rotation and flip.
        /// </summary>
        /// <remarks>The return values are stored 0->BottonLeft; 1->BottonRight; 2->TopRight; 3->TopLeft</remarks>
        /// <param name="tileID">The ID of the texture.</param>
        /// <param name="rotation">The rotation to aply to the texture.</param>
        /// <param name="flip">If the Texture must be fliped.</param>
        /// <param name="position">The position of the texture inside the 64px texture.</param>
        /// <returns>A array with 4 positions where each position represent one of the vertices od the texture, it will be a square with 24px.</returns>
        public Vector2[] GetLittleTexture(UInt32 tileID, RotationType rotation, Boolean flip, SquareTexturePosition position)
        {
            Vector2[] texture = GetNormalTexture(tileID, rotation, flip);
            Vector2[] newTexture = new Vector2[4];

            switch (position){
                case SquareTexturePosition.BottonLeft:
                    newTexture = bottomLeft(texture, rotation, flip);
                    break;                   
            }

            return newTexture;
        }


        /// <summary>
        /// Gets the coordinates for the texture acording to its rotation and flip.
        /// </summary>
        /// <remarks>The return values are stored 0->BottonLeft; 1->BottonRight; 2->TopRight; 3->TopLeft</remarks>
        /// <param name="tileID">The ID of the texture.</param>
        /// <param name="rotation">The rotation to aply to the texture.</param>
        /// <param name="flip">If the Texture must be fliped.</param>
        /// <param name="position">The position of the texture inside the 64px texture.</param>
        /// <param name="vertical">Is a vertical Rectangle.</param>
        /// <returns>A array with 4 positions where each position represent one of the vertices od the texture, it will be a rectangle 64pxX24px.</returns>
        public Vector2[] GetRectangleTexture(UInt32 tileID, RotationType rotation, Boolean flip, RectangleTexturePosition position)
        {
            Vector2[] texture = GetNormalTexture(tileID, rotation, flip);

            switch (position)
            {
                case RectangleTexturePosition.Botton:
                    return bottomRectangle(texture, rotation);
                case RectangleTexturePosition.Top:
                    return topRectangle(texture, rotation);
                case RectangleTexturePosition.CenterHorizontal:
                    break;
                case RectangleTexturePosition.Left:
                    return leftRectangle(texture, rotation);
                case RectangleTexturePosition.Right:
                    return rightRectangle(texture, rotation);
            }
            throw new NotImplementedException();
        }

        #region rectangles
        /// <summary>
        /// Gets the Top coordinates of a rectangle texture.
        /// </summary>
        /// <param name="texture">Original Texture.</param>
        /// <param name="rotation">The rotation to aply to the texture.</param>
        /// <returns>A array with 4 positions where each position represent one of the vertices of the texture, it will be a rectangle 64pxX24px.</returns>
        private Vector2[] topRectangle(Vector2[] texture, RotationType rotation)
        {
            Vector2[] newTexture = new Vector2[4];
            float x, y;

            switch (rotation)
            {
                case RotationType.RotateNone:
                    //LeftBotton
                    x = texture[0].X;
                    y = (float)(texture[2].Y + (pixelPerHeight * 24));
                    newTexture[0] = new Vector2(x, y);

                    //RightBotton
                    x = texture[2].X;
                    y = (float)(texture[2].Y + (pixelPerHeight * 24));
                    newTexture[1] = new Vector2(x, y);

                    //RightTop
                    newTexture[2] = texture[2];

                    //LeftTop
                    newTexture[3] = texture[3];
                    break;

                case RotationType.Rotate90:
                    //LeftBotton
                    x = texture[3].X + (pixelPerWidth * 24);
                    y = texture[3].Y;
                    newTexture[0] = new Vector2(x, y);

                    //RightBotton
                    x = texture[2].X + (pixelPerWidth * 24);
                    y = texture[2].Y;
                    newTexture[1] = new Vector2(x, y);

                    //RightTop
                    newTexture[2] = texture[2];

                    //LeftTop
                    newTexture[3] = texture[3];
                    break;

                case RotationType.Rotate180:
                    //LeftBotton
                    x = texture[0].X;
                    y = (float)(texture[2].Y - (pixelPerHeight * 24));
                    newTexture[0] = new Vector2(x, y);

                    //RightBotton
                    x = texture[2].X;
                    y = (float)(texture[2].Y - (pixelPerHeight * 24));
                    newTexture[1] = new Vector2(x, y);

                    //RightTop
                    newTexture[2] = texture[2];

                    //LeftTop
                    newTexture[3] = texture[3];
                    break;

                case RotationType.Rotate270:
                    //LeftBotton
                    x = texture[3].X - (pixelPerWidth * 24);
                    y = texture[3].Y;
                    newTexture[0] = new Vector2(x, y);

                    //RightBotton
                    x = texture[2].X - (pixelPerWidth * 24);
                    y = texture[2].Y;
                    newTexture[1] = new Vector2(x, y);

                    //RightTop
                    newTexture[2] = texture[2];

                    //LeftTop
                    newTexture[3] = texture[3];
                    break;
            }
            return newTexture;
        }

        /// <summary>
        /// Gets the Bottom coordinates of a rectangle texture.
        /// </summary>
        /// <param name="texture">Original Texture.</param>
        /// <param name="rotation">The rotation to aply to the texture.</param>
        /// <returns>A array with 4 positions where each position represent one of the vertices of the texture, it will be a rectangle 64pxX24px.</returns>
        private Vector2[] bottomRectangle(Vector2[] texture, RotationType rotation)
        {
            Vector2[] newTexture = new Vector2[4];
            float x, y;

            switch (rotation)
            {
                case RotationType.RotateNone:
                    //LeftBotton
                    newTexture[0] = texture[0];

                    //RightBotton
                    newTexture[1] = texture[1];

                    //RightTop
                    x = (float)(texture[1].X);
                    y = (float)(texture[0].Y - (pixelPerHeight * 24));
                    newTexture[2] = new Vector2(x, y);

                    //LeftTop
                    x = texture[0].X;
                    y = (float)(texture[0].Y - (pixelPerHeight * 24));
                    newTexture[3] = new Vector2(x, y);
                    break;

                case RotationType.Rotate90:
                    //LeftBotton
                    newTexture[0] = texture[0];

                    //RightBotton
                    newTexture[1] = texture[1];

                    //RightTop
                    x = (float)(texture[1].X - (pixelPerWidth * 24));
                    y = (float)(texture[1].Y);
                    newTexture[2] = new Vector2(x, y);

                    //LeftTop
                    x = (float)(texture[0].X - (pixelPerWidth * 24));
                    y = texture[0].Y;
                    newTexture[3] = new Vector2(x, y);
                    break;

                case RotationType.Rotate180:
                    //LeftBotton
                    newTexture[0] = texture[0];

                    //RightBotton
                    newTexture[1] = texture[1];

                    //RightTop
                    x = (float)(texture[1].X);
                    y = (float)(texture[0].Y + (pixelPerHeight * 24));
                    newTexture[2] = new Vector2(x, y);

                    //LeftTop
                    x = texture[0].X;
                    y = (float)(texture[0].Y + (pixelPerHeight * 24));
                    newTexture[3] = new Vector2(x, y);                    
                    break;

                case RotationType.Rotate270:
                    //LeftBotton
                    newTexture[0] = texture[0];

                    //RightBotton
                    newTexture[1] = texture[1];

                    //RightTop
                    x = (float)(texture[1].X + (pixelPerWidth * 24));
                    y = (float)(texture[1].Y);
                    newTexture[2] = new Vector2(x, y);

                    //LeftTop
                    x = (float)(texture[0].X + (pixelPerWidth * 24));
                    y = texture[0].Y;
                    newTexture[3] = new Vector2(x, y);
                    break;
            }
            return newTexture;
        }

        /// <summary>
        /// Gets the Right coordinates of a rectangle texture.
        /// </summary>
        /// <param name="texture">Original Texture.</param>
        /// <param name="rotation">The rotation to aply to the texture.</param>
        /// <returns>A array with 4 positions where each position represent one of the vertices of the texture, it will be a rectangle 64pxX24px.</returns>
        private Vector2[] rightRectangle(Vector2[] texture, RotationType rotation)
        {
            Vector2[] newTexture = new Vector2[4];
            float x, y;

            switch (rotation)
            {
                case RotationType.RotateNone:
                    //LeftBotton
                    x = texture[1].X - (pixelPerWidth * 24);
                    y = texture[1].Y;
                    newTexture[0] = new Vector2(x, y);

                    //RightBotton
                    newTexture[1] = texture[1];

                    //RightTop
                    newTexture[2] = texture[2];

                    //LeftTop
                    x = texture[2].X - (pixelPerWidth * 24);
                    y = texture[2].Y;
                    newTexture[3] = new Vector2(x, y);
                    break;

                case RotationType.Rotate90:
                    //LeftBotton
                    x = texture[1].X;
                    y = texture[1].Y + (pixelPerHeight * 24);
                    newTexture[0] = new Vector2(x, y);

                    //RightBotton
                    newTexture[1] = texture[1];

                    //RightTop
                    newTexture[2] = texture[2];

                    //LeftTop
                    x = texture[2].X;
                    y = texture[2].Y + (pixelPerHeight * 24);
                    newTexture[3] = new Vector2(x, y);
                    break;

                case RotationType.Rotate180:
                    //LeftBotton
                    x = texture[1].X + (pixelPerWidth * 24);
                    y = texture[1].Y;
                    newTexture[0] = new Vector2(x, y);

                    //RightBotton
                    newTexture[1] = texture[1];

                    //RightTop
                    newTexture[2] = texture[2];

                    //LeftTop
                    x = texture[2].X + (pixelPerWidth * 24);
                    y = texture[2].Y;
                    newTexture[3] = new Vector2(x, y);
                    break;

                case RotationType.Rotate270:
                    //LeftBotton
                    x = texture[1].X;
                    y = texture[1].Y - (pixelPerHeight * 24);
                    newTexture[0] = new Vector2(x, y);

                    //RightBotton
                    newTexture[1] = texture[1];

                    //RightTop
                    newTexture[2] = texture[2];

                    //LeftTop
                    x = texture[2].X;
                    y = texture[2].Y - (pixelPerHeight * 24);
                    newTexture[3] = new Vector2(x, y);
                    break;
            }
            return newTexture;
        }

        /// <summary>
        /// Gets the Left coordinates of a rectangle texture.
        /// </summary>
        /// <param name="texture">Original Texture.</param>
        /// <param name="rotation">The rotation to aply to the texture.</param>
        /// <returns>A array with 4 positions where each position represent one of the vertices of the texture, it will be a rectangle 64pxX24px.</returns>
        private Vector2[] leftRectangle(Vector2[] texture, RotationType rotation)
        {
            Vector2[] newTexture = new Vector2[4];
            float x, y;

            switch (rotation)
            {
                case RotationType.RotateNone:
                    //LeftBotton                    
                    newTexture[0] = texture[0];

                    //RightBotton
                    x = (float)(texture[0].X + (pixelPerWidth * 24));
                    y = (float)(texture[0].Y);
                    newTexture[1] = new Vector2(x, y);

                    //RightTop
                    x = (float)(texture[0].X + (pixelPerWidth * 24));
                    y = (float)(texture[2].Y);
                    newTexture[2] = new Vector2(x, y);

                    //LeftTop
                    newTexture[3] = texture[3];
                    break;

                case RotationType.Rotate90:
                    //LeftBotton
                    newTexture[0] = texture[0];

                    //RightBotton
                    x = texture[0].X;
                    y = texture[0].Y - (pixelPerHeight * 24);
                    newTexture[1] = new Vector2(x, y);

                    //RightTop
                    x = texture[3].X;
                    y = texture[3].Y - (pixelPerHeight * 24);
                    newTexture[2] = new Vector2(x, y);

                    //LeftTop
                    newTexture[3] = texture[3];
                    break;

                case RotationType.Rotate180:
                    //LeftBotton                    
                    newTexture[0] = texture[0];

                    //RightBotton
                    x = (float)(texture[0].X - (pixelPerWidth * 24));
                    y = (float)(texture[0].Y);
                    newTexture[1] = new Vector2(x, y);

                    //RightTop
                    x = (float)(texture[0].X - (pixelPerWidth * 24));
                    y = (float)(texture[2].Y);
                    newTexture[2] = new Vector2(x, y);

                    //LeftTop
                    newTexture[3] = texture[3];
                    break;

                case RotationType.Rotate270:
                    //LeftBotton
                    newTexture[0] = texture[0];

                    //RightBotton
                    x = texture[0].X;
                    y = texture[0].Y + (pixelPerHeight * 24);
                    newTexture[1] = new Vector2(x, y);

                    //RightTop
                    x = texture[3].X;
                    y = texture[3].Y + (pixelPerHeight * 24);
                    newTexture[2] = new Vector2(x, y);

                    //LeftTop
                    newTexture[3] = texture[3];
                    break;
            }
            return newTexture;
        }

        /// <summary>
        /// Gets the Center Vertical coordinates of a rectangle texture.
        /// </summary>
        /// <param name="texture">Original Texture.</param>
        /// <param name="rotation">The rotation to aply to the texture.</param>
        /// <returns>A array with 4 positions where each position represent one of the vertices of the texture, it will be a rectangle 64pxX24px.</returns>
        private Vector2[] centerVerticalRectangle(Vector2[] texture, RotationType rotation)
        {
            Vector2[] newTexture = new Vector2[4];
            float x, y;

            switch (rotation)
            {
                case RotationType.RotateNone:
                    //LeftBotton                    
                    x = (float)(texture[0].X);
                    y = (float)(texture[0].Y - (pixelPerWidth * 20));
                    newTexture[0] = new Vector2(x, y);

                    //RightBotton
                    x = (float)(texture[1].X);
                    y = (float)(texture[0].Y - (pixelPerWidth * 20));
                    newTexture[1] = new Vector2(x, y);

                    //RightTop
                    x = (float)(texture[2].X);
                    y = (float)(texture[2].Y + (pixelPerWidth * 20));
                    newTexture[2] = texture[2];

                    //LeftTop
                    x = (float)(texture[3].X);
                    y = (float)(texture[2].Y + (pixelPerWidth * 20));
                    newTexture[3] = new Vector2(x, y);
                    break;

                case RotationType.Rotate90:
                    //LeftBotton
                    newTexture[0] = texture[0];

                    //RightBotton
                    x = (float)(texture[1].X - (pixelPerWidth * 24));
                    y = (float)(texture[1].Y);
                    newTexture[1] = new Vector2(x, y);

                    //RightTop
                    newTexture[2] = texture[2];

                    //LeftTop
                    x = (float)(texture[2].X - (pixelPerWidth * 24));
                    y = (float)(texture[2].Y);
                    newTexture[3] = new Vector2(x, y);
                    break;

                case RotationType.Rotate180:
                    //LeftBotton                    
                    newTexture[0] = texture[0];

                    //RightBotton
                    x = (float)(texture[1].X - (pixelPerWidth * 24));
                    y = (float)(texture[1].Y);
                    newTexture[1] = new Vector2(x, y);

                    //RightTop
                    newTexture[2] = texture[2];

                    //LeftTop
                    x = (float)(texture[2].X - (pixelPerWidth * 24));
                    y = (float)(texture[2].Y);
                    newTexture[3] = new Vector2(x, y);
                    break;

                case RotationType.Rotate270:
                    //LeftBotton                    
                    newTexture[0] = texture[0];

                    //RightBotton
                    x = (float)(texture[1].X - (pixelPerWidth * 24));
                    y = (float)(texture[1].Y);
                    newTexture[1] = new Vector2(x, y);

                    //RightTop
                    newTexture[2] = texture[2];

                    //LeftTop
                    x = (float)(texture[2].X - (pixelPerWidth * 24));
                    y = (float)(texture[2].Y);
                    newTexture[3] = new Vector2(x, y);
                    break;
            }
            return newTexture;
        }

        #endregion

        #region sqares
        /// <summary>
        /// Gets the BottomLeft coordinates of a texture.
        /// </summary>
        /// <remarks>The return values are stored 0->BottonLeft; 1->BottonRight; 2->TopRight; 3->TopLeft</remarks>
        /// <param name="texture">Original Texture.</param>
        /// <param name="rotation">The rotation to aply to the texture.</param>
        /// <param name="flip">If the Texture must be fliped.</param>
        /// <returns>A array with 4 positions where each position represent one of the vertices of the texture, it will be a square 24pxX24px.</returns>
        private Vector2[] bottomLeft(Vector2[] texture, RotationType rotation, Boolean flip)
        {
            Vector2[] newTexture = new Vector2[4];
            float x, y;

            switch (rotation)
            {
                case RotationType.RotateNone:
                    if (flip)
                    {
                        //LeftBotton
                        newTexture[0] = texture[0];

                        //RightBotton
                        x = (float)(texture[0].X - (pixelPerWidth * 24));
                        y = texture[0].Y;
                        newTexture[1] = new Vector2(x, y);

                        //RightTop
                        x = (float)(texture[0].X - (pixelPerWidth * 24));
                        y = (float)(texture[0].Y - (pixelPerHeight * 24));
                        newTexture[2] = new Vector2(x, y);

                        //LeftTop
                        x = texture[0].X;
                        y = (float)(texture[0].Y - (pixelPerHeight * 24));
                        newTexture[3] = new Vector2(x, y);
                    }
                    else
                    {
                        //LeftBotton
                        newTexture[0] = texture[0];

                        //RightBotton
                        x = (float)(texture[0].X + (pixelPerWidth * 24));
                        y = texture[0].Y;
                        newTexture[1] = new Vector2(x, y);

                        //RightTop
                        x = (float)(texture[0].X + (pixelPerWidth * 24));
                        y = (float)(texture[0].Y - (pixelPerHeight * 24));
                        newTexture[2] = new Vector2(x, y);

                        //LeftTop
                        x = texture[0].X;
                        y = (float)(texture[0].Y - (pixelPerHeight * 24));
                        newTexture[3] = new Vector2(x, y);
                    }
                    break;

                case RotationType.Rotate90:
                    if (flip)
                    {    //LeftBotton
                        newTexture[0] = texture[0];

                        //RightBotton
                        x = texture[0].X;
                        y = (float)(texture[0].Y + (pixelPerHeight * 24));
                        newTexture[1] = new Vector2(x, y);

                        //RightTop
                        x = (float)(texture[0].X - (pixelPerWidth * 24));
                        y = (float)(texture[0].Y + (pixelPerHeight * 24));
                        newTexture[2] = new Vector2(x, y);

                        //LeftTop
                        x = (float)(texture[0].X - (pixelPerWidth * 24));
                        y = texture[0].Y;
                        newTexture[3] = new Vector2(x, y);
                    }
                    else
                    {
                        //LeftBotton
                        newTexture[0] = texture[0];

                        //RightBotton
                        x = texture[0].X;
                        y = (float)(texture[0].Y - (pixelPerHeight * 24));
                        newTexture[1] = new Vector2(x, y);

                        //RightTop
                        x = (float)(texture[0].X - (pixelPerWidth * 24));
                        y = (float)(texture[0].Y - (pixelPerHeight * 24));
                        newTexture[2] = new Vector2(x, y);

                        //LeftTop
                        x = (float)(texture[0].X - (pixelPerWidth * 24));
                        y = texture[0].Y;
                        newTexture[3] = new Vector2(x, y);
                    }
                    break;

                case RotationType.Rotate180:
                    if (flip) {
                        //LeftBotton
                        newTexture[0] = texture[0];

                        //RightBotton
                        x = (float)(texture[0].X + (pixelPerWidth * 24));
                        y = texture[0].Y;
                        newTexture[1] = new Vector2(x, y);

                        //RightTop
                        x = (float)(texture[0].X + (pixelPerWidth * 24));
                        y = (float)(texture[0].Y + (pixelPerHeight * 24));
                        newTexture[2] = new Vector2(x, y);

                        //LeftTop
                        x = texture[0].X;
                        y = (float)(texture[0].Y + (pixelPerHeight * 24));
                        newTexture[3] = new Vector2(x, y);
                    }
                    else
                    {
                        //LeftBotton
                        newTexture[0] = texture[0];

                        //RightBotton
                        x = (float)(texture[0].X - (pixelPerWidth * 24));
                        y = texture[0].Y;
                        newTexture[1] = new Vector2(x, y);

                        //RightTop
                        x = (float)(texture[0].X - (pixelPerWidth * 24));
                        y = (float)(texture[0].Y + (pixelPerHeight * 24));
                        newTexture[2] = new Vector2(x, y);

                        //LeftTop
                        x = texture[0].X;
                        y = (float)(texture[0].Y + (pixelPerHeight * 24));
                        newTexture[3] = new Vector2(x, y);
                    }
                    break;

                case RotationType.Rotate270:
                    if (flip)
                    {
                        //LeftBotton
                        newTexture[0] = texture[0];

                        //RightBotton                        
                        x = texture[0].X;
                        y = (float)(texture[0].Y - (pixelPerHeight * 24));
                        newTexture[1] = new Vector2(x, y);

                        //RightTop
                        x = (float)(texture[0].X + (pixelPerWidth * 24));
                        y = (float)(texture[0].Y - (pixelPerHeight * 24));
                        newTexture[2] = new Vector2(x, y);

                        //LeftTop
                        x = (float)(texture[0].X + (pixelPerWidth * 24));
                        y = texture[0].Y;
                        newTexture[3] = new Vector2(x, y);
                    }
                    else
                    {
                        //LeftBotton
                        newTexture[0] = texture[0];

                        //RightBotton                        
                        x = texture[0].X;
                        y = (float)(texture[0].Y + (pixelPerHeight * 24));
                        newTexture[1] = new Vector2(x, y);

                        //RightTop
                        x = (float)(texture[0].X + (pixelPerWidth * 24));
                        y = (float)(texture[0].Y + (pixelPerHeight * 24));
                        newTexture[2] = new Vector2(x, y);

                        //LeftTop
                        x = (float)(texture[0].X + (pixelPerWidth * 24));
                        y = texture[0].Y;
                        newTexture[3] = new Vector2(x, y);
                    }
                    break;
            }
            return newTexture;
        }
        #endregion
    }
}
