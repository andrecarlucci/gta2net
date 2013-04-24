// GTA2.NET
// 
// File: SpriteItem.cs
// Created: 10.03.2010
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
using System.Globalization;
using System.Xml.Serialization;
using Hiale.GTA2NET.Core.Style;

namespace Hiale.GTA2NET.Core.Helper
{
    [Serializable]
    public class SpriteItem
    {
        [XmlIgnore] //this is the key of the dictionary
        public int SpriteId;

        public SpriteType Type;

        /// <summary>
        /// Default palette which this sprite uses
        /// </summary>
        public int DefaultPalette { get; set; }

        /// <summary>
        /// Start index of the remap palettes. Add a RemapList item to this value to get the actual palette.
        /// </summary>
        public int RemapPaletteBase { get; set; }

        /// <summary>
        /// RemapList stores a list of virtual palette numbers, representing all of the alternative palettes which can sensibly be applied to this sprite.
        /// </summary>
        [XmlArrayItem("Remap")]
        public List<byte> RemapList { get; set; }

        public CompactRectangle Rectangle { get; set; }

        [XmlIgnore] //saved in separate file
        public List<DeltaSubItem> DeltaItems { get; set; } 

        private SpriteItem()
        {
            //XML Serializer
        }

        public SpriteItem(SpriteType type)
        {
            Type = type;
        }

        public SpriteItem(SpriteType type, int defaultPalette, int remapPaletteBase) : this(type)
        {
            DefaultPalette = defaultPalette;
            RemapPaletteBase = remapPaletteBase;
        }

        public SpriteItem(SpriteType type, int defaultPalette, int remapPaletteBase, List<byte> remapList) : this(type, defaultPalette, remapPaletteBase)
        {
            RemapList = remapList;
        }

        public override string ToString()
        {
            return SpriteId.ToString(CultureInfo.InvariantCulture) + " - " + Type;
        }

    }
}
