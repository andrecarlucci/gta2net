//Created: 10.03.2010

using System;
using System.Xml.Serialization;


namespace Hiale.GTA2NET.Helper
{
    [Serializable]
    public struct SpriteItem
    {
        public int Sprite;

        public int Model;

        public int Remap;

        public override string ToString()
        {
            return Sprite.ToString();
        }

    }
}
