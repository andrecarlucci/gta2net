//Created: 26.01.2010

using System;

namespace Hiale.GTA2NET.Core.Map
{
    /// <summary>
    /// Dummy cars use this to see where they are allowed to drive (i.e. which side of the road to drive on, where the corners & junctions are, etc.). The green arrows must be set up perfectly for the game to function.
    /// </summary>
    [Flags]
    public enum RoadTrafficType : byte
    {
        GreenLeft = 1,
        GreenRight = 2,
        GreenUp = 4,
        GreenDown = 8,
        RedLeft = 16,
        RedRight = 32,
        RedUp = 64,
        RedDown = 128
    }
}
