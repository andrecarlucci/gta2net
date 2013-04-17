// GTA2.NET
// 
// File: CarInfo.cs
// Created: 19.01.2010
// 
// 
// Copyright (C) 2010-2013 Hiale
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR
// IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Grand Theft Auto (GTA) is a registred trademark of Rockstar Games.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Core.Style;

namespace Hiale.GTA2NET.Core
{
    /// <summary>
    /// Various parameters are stored for each car. A car info structure is stored for each distinct type of car. 
    /// </summary>
    [Serializable]
    public class CarInfo
    {
        public CarInfo()
        {
            RemapList = new List<byte>();
            Doors = new List<DoorInfo>();
        }

        public static void Serialize(SerializableDictionary<int, CarInfo> carInfo, string fileName)
        {
            var textWriter = new StreamWriter(fileName);
            var serializer = new XmlSerializer(typeof(SerializableDictionary<int, CarInfo>));
            serializer.Serialize(textWriter, carInfo);
            textWriter.Close();
        }

        public static SerializableDictionary<int, CarInfo> Deserialize(string fileName)
        {
            TextReader textReader = new StreamReader(fileName);
            var deserializer = new XmlSerializer(typeof(SerializableDictionary<int, CarInfo>));
            var dict = (SerializableDictionary<int, CarInfo>)deserializer.Deserialize(textReader);
            textReader.Close();
            return dict;
        }

        public static List<CarInfo> CreateCarInfoCollection(Dictionary<int, CarInfo> carInfo, Dictionary<int, CarPhysics> carPhysics)
        {
            var carInfoCollection = new List<CarInfo>();
            foreach (var carInfoItem in carInfo)
            {
                foreach (var carPhysicsItem in carPhysics)
                {
                    if (carInfoItem.Key != carPhysicsItem.Key)
                        continue;
                    carInfoItem.Value.Physics = carPhysicsItem.Value;
                    carInfoCollection.Add(carInfoItem.Value);
                }

            }
            return carInfoCollection;
        }

        /// <summary>
        /// Physical behavior of the car.
        /// </summary>
        [XmlIgnore]
        public CarPhysics Physics { get; private set; }

        /// <summary>
        /// Model is the car model number. Every type of car has a unique model number. It can be used as an ID.
        /// </summary>
        public int Model { get; set; }

        public string Name
        {
            get
            {
                return Physics != null ? Physics.Name : "Model: " + Model;
            }
        }

        /// <summary>
        /// Sprite number.
        /// </summary>
        public int Sprite { get; set; }

        /// <summary>
        /// Width of the car in pixels. This is required here because it may be different from the width of the car for collision purposes.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// /// <summary>
        /// Height of the car in pixels. This is required here because it may be different from the height of the car for collision purposes.
        /// </summary>
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Passengers is the number of passengers which this car can carry ( not including the driver ).
        /// </summary>
        public byte Passengers { get; set; }

        /// <summary>
        /// Wreck is the wreck graphic number to use when this car is wrecked (0-8, or 99 if can’t wreck).
        /// </summary>
        public byte Wreck { get; set; }

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
        public byte Rating { get; set; } //ToDo enum

        /// <summary>
        /// FrontWheelOffset is the distances in pixels from the centre of the car to the front axle.
        /// </summary>
        public byte FrontWheelOffset { get; set; }

        /// <summary>
        /// RearWheelOffset is the distances in pixels from the centre of the car to the back axle.
        /// </summary>
        public byte RearWheelOffset { get; set; }

        /// <summary>
        /// FrontWindowOffset is the distances in pixels from the centre of the car to the front window.
        /// </summary>
        public byte FrontWindowOffset { get; set; }

        /// <summary>
        /// RearWindowOffset is the distances in pixels from the centre of the car to the back window.
        /// </summary>
        public byte RearWindowOffset { get; set; }

        //private byte _NumRemaps;
        ///// <summary>
        ///// ???
        ///// </summary>
        //public byte NumRemaps
        //{
        //    get { return _NumRemaps; }
        //    set { _NumRemaps = value; }
        //}

        /// <summary>
        /// RemapList stores a list of virtual palette numbers, representing all of the alternative palettes which can sensibly be applied to this car.
        /// </summary>
        public List<byte> RemapList { get; set; }

        ///// <summary>
        ///// InfoFlags is a bitmap with the following fields:
        ///// bit	value	name	meaning
        ///// 0	0x01	ped_jump	1 if this car is too high for a ped to jump,  else 0
        ///// 1	0x02	emerg_lights	1 if this car has emergency lights (e.g. police car), else 0
        ///// 2	0x04	roof_lights	1 if this car has roof lights (come on with headlights), else 0
        ///// 3	0x08	cab	1 if this car can be used as an artic cab, else 0
        ///// 4	0x10	trailer	1 if this car can be used as an artic trailer, else 0
        ///// 5	0x20	forhire_lights	1 if this car has forhire lights (e.g. taxi) else 0
        ///// 6	0x40	roof_decal	1 if this car has a roof decal (e.g. TV van) else 0
        ///// 7	0x80	rear_emerg_lights	1 if this car has rear emergency lights ( else 0 )
        ///// NOTE:
        ///// -	no car can have more than one out of emerg_lights, roof_lights, forhire_lights & roof decal.
        ///// -	no car cab be both cab and trailer
        ///// -	a car with rear emerg lights must have emerg lights as well
        ///// </summary>
        //public byte InfoFlagsBase { get; set; }

        ///// <summary>
        ///// InfoFlags2 is a bitmap with the following fields:
        ///// bit	value	name	meaning
        ///// 0	0x01	collide_over	1 if this car can drive over other cars, else 0
        ///// 1	0x02	popup	1 if this car has popup headlights, else 0
        ///// </summary>
        //public byte InfoFlags2Base { get; set; }

        public CarInfoFlags InfoFlags { get; set; }

        /// <summary>
        /// A list of doors is stored for each car.
        /// </summary>
        public List<DoorInfo> Doors { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}