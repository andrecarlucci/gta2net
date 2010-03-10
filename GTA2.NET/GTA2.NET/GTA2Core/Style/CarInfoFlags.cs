//Created: 09.03.2010

using System;

namespace Hiale.GTA2NET.Core.Style
{
    [Flags]
    public enum CarInfoFlags : short 
    {
        PedJump = 0x01,
        EmergencyLights = 0x02,
        RoofLights = 0x04,
        Cab = 0x08,
        Trailer = 0x10,
        ForhireLights = 0x20,
        RoofDecal = 0x40,
        RearEmergencyLights = 0x80,
        CallideOver = 0x100, //Info flags 2
        Popup = 0x200, //Info flags 2
    }
}
