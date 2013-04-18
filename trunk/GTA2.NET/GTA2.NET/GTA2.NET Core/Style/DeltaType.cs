// GTA2.NET
// 
// File: DeltaType.cs
// Created: 18.04.2013
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
using System.Linq;
using System.Text;

namespace Hiale.GTA2NET.Core.Style
{
    public enum DeltaType
    {
        DentRearLeft = 0,
        DentRearRight = 1,
        DentFrontRight = 2,
        DentFrontLeft = 3,
        FrontShieldDamaged = 4,
        BrakeLightRight = 5, //there seem to be no left version of this
        HeadLightRight = 6, //same here
        DriverDoorAlmostClosed = 7,
        DriverDoorSlightlyOpened = 8,
        DriverDoorAlmostOpen = 9,
        DriverDoorOpen = 10,
    }
}

//http://en.wikigta.org/wiki/Creating_vehicles_%28GTA2%29#Deltas
//Rear Right Dent
//Rear Left Dent
//Front Left Dent
//Front Right Dent
//Damaged Windsreen
//Left Brake Light
//Left Headlight
//Left Front Door 1
//Left Front Door 2
//Left Front Door 3
//Left Front Door 4
//Left Back Door/FBI Light 1
//Left Back Door/FBI Light 2
//Left Back Door/FBI Light 3
//Left Back Door/FBI Light 4
//Emergency/Roof Light/Decal
//Emergency Light
//Right Rear Emergency Light
//Left Rear Emergency Light
