//14.02.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiale.GTA2.Core.Style
{
    public enum SurfaceType : byte
    {
        Grass, RoadSpecial, Water, Electrified, ElectrifiedPlatform, WoodFloor, MetalFloor, MetalWall, GrassWall
    }

    public class Surface
    {
        private SurfaceType type;
        public SurfaceType Type
        {
            get { return type; }
            set { type = value; }
        }

        private List<int> tiles;
        public List<int> Tiles
        {
            get { return tiles; }
            set { tiles = value; }
        }

        public Surface(SurfaceType Type)
        {
            Tiles = new List<int>();
        }

    }
}
