//Created: 18.01.2010


namespace Hiale.GTA2NET.Core.Style
{
    /// <summary>
    /// Represent a door in relative position to the centre of the car.
    /// </summary>
    public struct DoorInfo
    {
        /// <summary>
        /// X and Y are the position relative to the centre of the car where a ped graphic should be drawn when on the last frame of getting into the car (or the first frame of getting out of the car) via this door. This is normally the position of the outer edge of the inside of the car when the door is open. There is one special case here. If rx is greater than 64 (or less than –64) then 64 must be subtracted (or added) before rx is used. When this happens, it indicates that peds should enter/exit the car at this door by simply walking straight in, rather than by going through the sit-down/stand-up animation which they use at other doors. This is used, for example, for the sliding doors on a train.
        /// </summary>
        public byte X;

        /// <summary>
        /// /// X and Y are the position relative to the centre of the car where a ped graphic should be drawn when on the last frame of getting into the car (or the first frame of getting out of the car) via this door. This is normally the position of the outer edge of the inside of the car when the door is open. There is one special case here. If rx is greater than 64 (or less than –64) then 64 must be subtracted (or added) before rx is used. When this happens, it indicates that peds should enter/exit the car at this door by simply walking straight in, rather than by going through the sit-down/stand-up animation which they use at other doors. This is used, for example, for the sliding doors on a train.
        /// </summary>
        public byte Y;

        public override string ToString()
        {
            return X + " " + Y;
        }

    }
}
