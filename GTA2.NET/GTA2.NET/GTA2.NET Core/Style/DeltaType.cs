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
using System.ComponentModel;

namespace Hiale.GTA2NET.Core.Style
{
    public enum DeltaType
    {
        [Description("rear right dent")]
        RearRightDent = 0,
        [Description("rear left dent")]
        RearLeftDent = 1,
        [Description("front left dent")]
        FrontLeftDent = 2,
        [Description("front right dent")]
        FrontRightDent = 3,
        [Description("windscreen damage")]
        WindscreenDamage = 4,
        [Description("left brake light")]
        LeftBrakeLight = 5,
        [Description("left headlight")]
        LeftHeadlight = 6,
        [Description("left front door (almost closed)")]
        LeftFrontDoor1 = 7,
        [Description("left front door (slightly opened)")]
        LeftFrontDoor2 = 8,
        [Description("left front door (almost open)")]
        LeftFrontDoor3 = 9,
        [Description("left front door (open)")]
        LeftFrontDoor4 = 10,
        [Description("left back door (almost closed)/FBI light animation")]
        LeftBackDoor1 = 11,
        [Description("left back door (slightly opened)/FBI light animation")]
        LeftBackDoor2 = 12,
        [Description("left back door (almost open)/FBI light animation")]
        LeftBackDoor3 = 13,
        [Description("left back door (open)/FBI light animation")]
        LeftBackDoor4 = 14,
        [Description("emergency/roof lights/decal")]
        RoofLights = 15,
        [Description("emergency lights")]
        EmergencyLights = 16,
        [Description("right rear emergency light")]
        RightRearEmergencyLight = 17,
        [Description("Left rear emergency light")]
        LeftRearEmergLight = 18,
        [Description("Racing Car (Sprite #17) roof No. 8")]
        RacingCarNumber = 19,
        [Description("Racing Car (Sprite #17) roof No. 9")]
        RacingCarNumber2 = 20,
        [Description("right brake light (mirror)")]
        RightBrakeLight = 22,
        [Description("right headlight (mirror)")]
        RightHeadlight = 23,
        [Description("right front door (almost closed) (mirror)")]
        RightFrontDoor1 = 24,
        [Description("right front door (slightly opened) (mirror)")]
        RightFrontDoor2 = 25,
        [Description("right front door (almost open) (mirror)")]
        RightFrontDoor3 = 26,
        [Description("right front door (open) (mirror)")]
        RightFrontDoor4 = 27,
        [Description("right back door (almost closed) (mirror)")]
        RightBackDoor1 = 28,
        [Description("right back door (slightly opened) (mirror)")]
        RightBackDoor2 = 29,
        [Description("right back door (almost open) (mirror)")]
        RightBackDoor3 = 30,
        [Description("right back door (open) (mirror)")]
        RightBackDoor4 = 31
    }
}
