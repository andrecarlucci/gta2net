// GTA2.NET
// 
// File: GTA2Game.cs
// Created: 28.08.2013
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
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core
{
    /// <summary>
    /// Responsible for the transfer the information to draw.
    /// </summary>
    public class Frame
    {
        public Frame(List<VertexPositionNormalTexture> mapVertexList, List<int> mapIndexList, List<VertexPositionNormalTexture> objectVertexList, List<int> objectIndexList, Vector2 position)
        {
            this.MapVertexList = mapVertexList;
            this.MapIndexList = mapIndexList;
            this.ObjectVertexList = objectVertexList;
            this.ObjectIndexList = objectIndexList;
            this.Position = position;
        }

        /// <summary>
        /// A list with the map vertices's to draw.
        /// </summary>
        public List<VertexPositionNormalTexture> MapVertexList { get; private set; }

        /// <summary>
        /// The way that the map vertices's should be grouped.
        /// </summary>
        public List<int> MapIndexList { get; private set; }

        /// <summary>
        /// A list with the objects vertices's to draw.
        /// </summary>
        public List<VertexPositionNormalTexture> ObjectVertexList { get; private set; }

        /// <summary>
        /// The way that the objects vertices's should be grouped.
        /// </summary>
        public List<int> ObjectIndexList { get; private set; }

        /// <summary>
        /// The center position of this frame.
        /// </summary>
        public Vector2 Position { get; private set; }
    }
}
