//Created: 19.01.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiale.GTA2NET.Core.Style
{
    public class PaletteBase
    {
        private ushort _Tile;
        public ushort Tile
        {
            get { return _Tile; }
            set { _Tile = value; }
        }

        private ushort _Sprite;
        public ushort Sprite
        {
            get { return _Sprite; }
            set { _Sprite = value; }
        }

        private ushort _CarRemap;
        public ushort CarRemap
        {
            get { return _CarRemap; }
            set { _CarRemap = value; }
        }

        private ushort _PedRemap;
        public ushort PedRemap
        {
            get { return _PedRemap; }
            set { _PedRemap = value; }
        }

        private ushort _CodeObjRemap;
        public ushort CodeObjRemap
        {
            get { return _CodeObjRemap; }
            set { _CodeObjRemap = value; }
        }

        private ushort _MapObjRemap;
        public ushort MapObjRemap
        {
            get { return _MapObjRemap; }
            set { _MapObjRemap = value; }
        }

        private ushort _UserRemap;
        public ushort UserRemap
        {
            get { return _UserRemap; }
            set { _UserRemap = value; }
        }

        private ushort _FontRemap;
        public ushort FontRemap
        {
            get { return _FontRemap; }
            set { _FontRemap = value; }
        }
    }
}
