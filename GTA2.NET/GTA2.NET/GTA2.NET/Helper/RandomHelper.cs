// GTA2.NET
// 
// File: RandomHelper.cs
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
using System.Collections.Generic;
using System.Text;

namespace Hiale.GTA2NET.Helper
{
    public static class RandomHelper
    {
        private static Random _trueRandom;

        public static Random TrueRandom
        {
            get
            {
                if (_trueRandom == null)
                    _trueRandom = new Random();
                return _trueRandom;
            }
            private set { _trueRandom = value; }
        }

        private static int _seed = 924826379;
        public static int Seed
        {
            get { return _seed; }
            private set { _seed = value; }
        }

        private static Random _fakeRandom;
        public static Random FakeRandom
        {
            get
            {
                if (_fakeRandom == null)
                    _fakeRandom = new Random(Seed);
                return _fakeRandom;
            }
            private set { _fakeRandom = value; }
        }

        /// <summary>
        /// Get random int
        /// </summary>
        /// <param name="trueRandom">ToDo</param>
        /// <param name="max">Maximum</param>
        /// <returns>Int</returns>
        public static int GetRandomInt(bool trueRandom, int max)
        {
            return trueRandom ? TrueRandom.Next(max) : FakeRandom.Next(max);
        }

        /// <summary>
        /// Get random float between min and max
        /// </summary>
        /// <param name="trueRandom">ToDo</param>
        /// <param name="min">Min</param>
        /// <param name="max">Max</param>
        /// <returns>Float</returns>
        public static float GetRandomFloat(bool trueRandom, float min, float max)
        {
            float formula = (max - min) + min;
            return trueRandom ? (float) TrueRandom.NextDouble() * formula : (float) FakeRandom.NextDouble() * formula;
        }
    }
}