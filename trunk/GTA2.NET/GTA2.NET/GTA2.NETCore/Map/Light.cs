//Created 17.01.2010

using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Map
{
    public struct Light
    {
        public Color Color;

        public ushort X;

        public ushort Y;

        public ushort Z;

        public ushort Radius;

        public byte Intensity;

        public byte Shape;

        public byte OnTime;

        public byte OffTime;
    }
}
