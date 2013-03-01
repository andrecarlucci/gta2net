//14.02.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiale.GTA2NET.Core.Style
{
    public enum SurfaceType : byte
    {
        Grass, RoadSpecial, Water, Electrified, ElectrifiedPlatform, WoodFloor, MetalFloor, MetalWall, GrassWall
    }

    public class Surface
    {
        public SurfaceType Type { get; set; }

        public List<int> Tiles { get; set; }

        public Surface(SurfaceType type)
        {
            Tiles = new List<int>();
        }

    }
}
