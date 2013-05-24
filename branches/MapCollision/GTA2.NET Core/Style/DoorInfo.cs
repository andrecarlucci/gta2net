// GTA2.NET
// 
// File: DoorInfo.cs
// Created: 18.01.2010
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

namespace Hiale.GTA2NET.Core.Style
{
    /// <summary>
    /// Represent a door in relative position to the centre of the car.
    /// </summary>
    [Serializable]
    public struct DoorInfo
    {
        /// <summary>
        /// X and Y are the position relative to the centre of the car where a ped graphic should be drawn when on the last frame of getting into the car (or the first frame of getting out of the car) via this door. This is normally the position of the outer edge of the inside of the car when the door is open. There is one special case here. If rx is greater than 64 (or less than –64) then 64 must be subtracted (or added) before rx is used. When this happens, it indicates that peds should enter/exit the car at this door by simply walking straight in, rather than by going through the sit-down/stand-up animation which they use at other doors. This is used, for example, for the sliding doors on a train.
        /// </summary>
        public byte X { get; set; }

        /// <summary>
        /// /// X and Y are the position relative to the centre of the car where a ped graphic should be drawn when on the last frame of getting into the car (or the first frame of getting out of the car) via this door. This is normally the position of the outer edge of the inside of the car when the door is open. There is one special case here. If rx is greater than 64 (or less than –64) then 64 must be subtracted (or added) before rx is used. When this happens, it indicates that peds should enter/exit the car at this door by simply walking straight in, rather than by going through the sit-down/stand-up animation which they use at other doors. This is used, for example, for the sliding doors on a train.
        /// </summary>
        public byte Y { get; set; }

        public override string ToString()
        {
            return X + " " + Y;
        }

    }
}
