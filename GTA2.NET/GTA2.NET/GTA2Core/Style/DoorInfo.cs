//Created: 18.01.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiale.GTA2NET.Core.Style
{
    public class DoorInfo
    {
        private byte _x;
        /// <summary>
        /// X and Y are the position relative to the centre of the car where a ped graphic should be drawn when on the last frame of getting into the car (or the first frame of getting out of the car) via this door. This is normally the position of the outer edge of the inside of the car when the door is open. There is one special case here. If rx is greater than 64 (or less than –64) then 64 must be subtracted (or added) before rx is used. When this happens, it indicates that peds should enter/exit the car at this door by simply walking straight in, rather than by going through the sit-down/stand-up animation which they use at other doors. This is used, for example, for the sliding doors on a train.
        /// </summary>
        public byte X
        {
            get { return _x; }
            set { _x = value; }
        }

        private byte _y;
        /// <summary>
        /// /// X and Y are the position relative to the centre of the car where a ped graphic should be drawn when on the last frame of getting into the car (or the first frame of getting out of the car) via this door. This is normally the position of the outer edge of the inside of the car when the door is open. There is one special case here. If rx is greater than 64 (or less than –64) then 64 must be subtracted (or added) before rx is used. When this happens, it indicates that peds should enter/exit the car at this door by simply walking straight in, rather than by going through the sit-down/stand-up animation which they use at other doors. This is used, for example, for the sliding doors on a train.
        /// </summary>
        public byte Y
        {
            get { return _y; }
            set { _y = value; }
        }
    }
}
