// GTA2.NET
// 
// File: RoadTrafficType.cs
// Created: 21.02.2013
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
