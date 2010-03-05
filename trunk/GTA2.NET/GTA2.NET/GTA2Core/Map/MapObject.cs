//Created 17.01.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiale.GTA2NET.Core.Map
{
    /// <summary>
    /// Map objects are items, such as bins, piles of rubbish & trees which can be positioned on top of any valid ground block in the map.
    /// </summary>
    public class MapObject
    {
        float _x;
        public float x
        {
            get { return _x; }
            set { _x = value; }
        }

        float _y;
        public float y
        {
            get { return _y; }
            set { _y = value; }
        }

        byte _Rotation;
        /// <summary>
        /// Rotation of the object, which maps the 360° of a circle onto values from 0 to 255, with 0 representing 0° and 128 representing 180°.
        /// </summary>
        public byte Rotation
        {
            get { return _Rotation; }
            set { _Rotation = value; }
        }

        byte _Type;
        /// <summary>
        /// Type is a code which identifies the type of object. It serves as an index into the object information in the style file.
        /// </summary>
        public byte Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

    }
}
