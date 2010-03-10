//Created: 09.02.2010

namespace Hiale.GTA2NET.Core.Map
{
    public enum GroundType : byte
    {
        Air = 0, Road = 1, Pavement = 2, Field = 3
    }

    public enum SlopeType : byte
    {
        None = 0,
        Up26Low = 1,
        Up26High = 2,
        Down26Low = 3,
        Down26High = 4,
        Left26Low = 5,
        Left26High = 6,
        Right26Low = 7,
        Right26High = 8,
        //add more...
        DiagonalFacingUpLeft = 45,
        DiagonalFacingUpRight = 46,
        DiagonalFacingDownLeft = 47,
        DiagonalFacingDownRight = 48,        
    }
}
