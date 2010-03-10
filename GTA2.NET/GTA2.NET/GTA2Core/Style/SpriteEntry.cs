//Created: 20.01.2010

using System;

namespace Hiale.GTA2NET.Core.Style
{
    public struct SpriteEntry
    {
        public UInt32 Ptr;
        public byte Width;
        public byte Height;
        public ushort Pad; //Unused

        public override string ToString()
        {
            return Width + " x " + Height + " (Ptr: " + Ptr + ")";
        }
    }
}
