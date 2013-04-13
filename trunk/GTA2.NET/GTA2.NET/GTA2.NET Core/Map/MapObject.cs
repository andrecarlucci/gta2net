// GTA2.NET
// 
// File: MapObject.cs
// Created: 17.01.2010
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
namespace Hiale.GTA2NET.Core.Map
{
    /// <summary>
    /// Map objects are items, such as bins, piles of rubbish & trees which can be positioned on top of any valid ground block in the map.
    /// </summary>
    public struct MapObject
    {
        public float X;

        public float Y;

        /// <summary>
        /// Rotation of the object, which maps the 360° of a circle onto values from 0 to 255, with 0 representing 0° and 128 representing 180°.
        /// </summary>
        public byte Rotation;

        /// <summary>
        /// Type is a code which identifies the type of object. It serves as an index into the object information in the style file.
        /// </summary>
        public byte Type;
    }
}
