//Created 17.01.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Hiale.GTA2NET.Core.Map
{
    class Light
    {
        private Color _Color;
        public Color Color
        {
            get { return _Color; }
            set { _Color = value; }
        }

        private ushort _x;
        public ushort X
        {
            get { return _x; }
            set { _x = value; }
        }

        private ushort _y;
        public ushort Y
        {
            get { return _y; }
            set { _y = value; }
        }

        private ushort _Z;
        public ushort Z
        {
            get { return _Z; }
            set { _Z = value; }
        }

        private ushort _Radius;
        public ushort Radius
        {
            get { return _Radius; }
            set { _Radius = value; }
        }

        private byte _Intensity;
        public byte Intensity
        {
            get { return _Intensity; }
            set { _Intensity = value; }
        }

        private byte _Shape;
        public byte Shape
        {
            get { return _Shape; }
            set { _Shape = value; }
        }

        private byte _OnTime;
        public byte OnTime
        {
            get { return _OnTime; }
            set { _OnTime = value; }
        }

        private byte _OffTime;
        public byte OffTime
        {
            get { return _OffTime; }
            set { _OffTime = value; }
        }

    }
}
