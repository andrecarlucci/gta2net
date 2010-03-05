//Created: 19.01.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiale.GTA2NET.Core.Style
{
    /// <summary>
    /// Various parameters are stored for each car. A car info structure is stored for each distinct type of car. 
    /// </summary>
    public class CarInfo
    {

        public CarInfo()
        {
            RemapList = new List<byte>();
            Doors = new List<DoorInfo>();
        }

        private int _Model;
        /// <summary>
        /// Model is the car model number. Every distinct type of car has a unique model number.
        /// </summary>
        public int Model
        {
            get { return _Model; }
            set { _Model = value; }
        }

        private byte _Sprite;
        /// <summary>
        /// Sprite is the relative car sprite number. At least one sprite is stored for every car. The sprite number for each car is simply: car sprite number + car sprite base. In practice, the relative sprite number is actually filled in here by the game when the style is loaded. The style file only needs to store here the number of sprites used by the car ( 0 or 1 ). If a car has 0 sprites, it shares the graphic of the preceding one.
        /// </summary>
        public byte Sprite
        {
            get { return _Sprite; }
            set { _Sprite = value; }
        }

        private int _Width;
        /// <summary>
        /// Width of the car in pixels. This is required here because it may be different from the width of the car for collision purposes.
        /// </summary>
        public int Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        private int _Height;
        /// <summary>
        /// /// <summary>
        /// Height of the car in pixels. This is required here because it may be different from the height of the car for collision purposes.
        /// </summary>
        /// </summary>
        public int Height
        {
            get { return _Height; }
            set { _Height = value; }
        }

        private byte _Passengers;
        /// <summary>
        /// Passengers is the number of passengers which this car can carry ( not including the driver ).
        /// </summary>
        public byte Passengers
        {
            get { return _Passengers; }
            set { _Passengers = value; }
        }

        private byte _Wreck;
        /// <summary>
        /// Wreck is the wreck graphic number to use when this car is wrecked (0-8, or 99 if can’t wreck).
        /// </summary>
        public byte Wreck
        {
            get { return _Wreck; }
            set { _Wreck = value; }
        }

        private byte _Rating;
        /// <summary>
        /// Rating is the quality rating for this car – used to decide how often it is created in different areas of the city. Values are
        /// 1	bad
        /// 2	bad x 2
        /// 3	bad x 3
        /// 11	average
        /// 12	average x 2
        /// 13	average x 3
        /// 21	good
        /// 22	good x 2
        /// 23	good x 3
        /// 99	not recycled
        /// </summary>
        public byte Rating
        {
            get { return _Rating; }
            set { _Rating = value; }
        }

        private byte _FrontWheelOffset;
        /// <summary>
        /// FrontWheelOffset is the distances in pixels from the centre of the car to the front axle.
        /// </summary>
        public byte FrontWheelOffset
        {
            get { return _FrontWheelOffset; }
            set { _FrontWheelOffset = value; }
        }

        private byte _RearWheelOffset;
        /// <summary>
        /// RearWheelOffset is the distances in pixels from the centre of the car to the back axle.
        /// </summary>
        public byte RearWheelOffset
        {
            get { return _RearWheelOffset; }
            set { _RearWheelOffset = value; }
        }

        private byte _FrontWindowOffset;
        /// <summary>
        /// FrontWindowOffset is the distances in pixels from the centre of the car to the front window.
        /// </summary>
        public byte FrontWindowOffset
        {
            get { return _FrontWindowOffset; }
            set { _FrontWindowOffset = value; }
        }

        private byte _RearWindowOffset;
        /// <summary>
        /// RearWindowOffset is the distances in pixels from the centre of the car to the back window.
        /// </summary>
        public byte RearWindowOffset
        {
            get { return _RearWindowOffset; }
            set { _RearWindowOffset = value; }
        }

        //private byte _NumRemaps;
        ///// <summary>
        ///// ???
        ///// </summary>
        //public byte NumRemaps
        //{
        //    get { return _NumRemaps; }
        //    set { _NumRemaps = value; }
        //}

        private List<byte> _RemapList;
        /// <summary>
        /// RemapList stores a list of virtual palette numbers, representing all of the alternative palettes which can sensibly be applied to this car. Note that these palette numbers are relative to the start of the car remap palette area.
        /// </summary>
        public List<byte> RemapList
        {
            get { return _RemapList; }
            set { _RemapList = value; }
        }

        private byte _InfoFlags;
        /// <summary>
        /// InfoFlags is a bitmap with the following fields:
        /// bit	value	name	meaning
        /// 0	0x01	ped_jump	1 if this car is too high for a ped to jump,  else 0
        /// 1	0x02	emerg_lights	1 if this car has emergency lights (e.g. police car), else 0
        /// 2	0x04	roof_lights	1 if this car has roof lights (come on with headlights), else 0
        /// 3	0x08	cab	1 if this car can be used as an artic cab, else 0
        /// 4	0x10	trailer	1 if this car can be used as an artic trailer, else 0
        /// 5	0x20	forhire_lights	1 if this car has forhire lights (e.g. taxi) else 0
        /// 6	0x40	roof_decal	1 if this car has a roof decal (e.g. TV van) else 0
        /// 7	0x80	rear_emerg_lights	1 if this car has rear emergency lights ( else 0 )
        /// NOTE:
        /// -	no car can have more than one out of emerg_lights, roof_lights, forhire_lights & roof decal.
        /// -	no car cab be both cab and trailer
        /// -	a car with rear emerg lights must have emerg lights as well
        /// </summary>
        public byte InfoFlags
        {
            get { return _InfoFlags; }
            set { _InfoFlags = value; }
        }

        private byte _InfoFlags2;
        /// <summary>
        /// InfoFlags2 is a bitmap with the following fields:
        /// bit	value	name	meaning
        /// 0	0x01	collide_over	1 if this car can drive over other cars, else 0
        /// 1	0x02	popup	1 if this car has popup headlights, else 0
        /// </summary>
        public byte InfoFlags2
        {
            get { return _InfoFlags2; }
            set { _InfoFlags2 = value; }
        }

        private List<DoorInfo> _Doors;
        /// <summary>
        /// A list of doors is stored for each car.
        /// </summary>
        public List<DoorInfo> Doors
        {
            get { return _Doors; }
            set { _Doors = value; }
        }

    }
}
