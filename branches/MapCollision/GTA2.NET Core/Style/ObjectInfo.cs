// GTA2.NET
// 
// File: ObjectInfo.cs
// Created: 20.01.2010
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
namespace Hiale.GTA2NET.Core.Style
{
    /// <summary>
    /// A very small amount of type information is stored for each type of map object.
    /// Many other properties of objects are required for GTA2, but these are stored in code header files, not in the style file, because they are more likely to be updated by programmers than by artists, and the editor does not need to know about them.
    /// Note that there are two different kinds of object – map objects and code objects. Map objects (e.g. bins and hot dog stands) can be placed by the editor and hence have their information stored here. Code objects (e.g. blood and skidmarks) can only be placed by the code so no information is stored for them here.
    /// </summary>
    public struct ObjectInfo
    {
        /// <summary>
        /// Model is the object model number. Every distinct type of object has a unique model number. Objects placed in the map using the editor are represented by a model number.
        /// </summary>
        public byte Model { get; set; }
        /// <summary>
        /// Sprites is the number of sprites stored for this model.
        /// </summary>
        public byte Sprites { get; set; }
    }
}
