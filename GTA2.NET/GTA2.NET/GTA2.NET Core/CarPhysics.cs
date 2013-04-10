// GTA2.NET
// 
// File: CarPhysics.cs
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

namespace Hiale.GTA2NET.Core
{
    [Serializable]
    public class CarPhysics
    {
        /// <summary>
        /// The car's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Model number used to asociate car handlings with .sty file sprites
        /// </summary>
        public int Model { get; set; }

        /// <summary>
        /// If set the car accelerates faster and stronger at lower speeds to reach highier speeds faster
        /// </summary>
        public bool Turbo { get; set; }

        /// <summary>
        /// The car's cost
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The car's mass - more means that the car will be less vurnelable to all kind of damages, and the car will steer less responsive.
        /// </summary>
        public float Mass { get; set; }

        /// <summary>
        /// Highier values makes front of car more responsive.
        /// </summary>
        public float FrontDriveBias { get; set; }

        /// <summary>
        /// Highier values makes car steering less responsive at highier speeds.
        /// </summary>
        public float FrontMassBias { get; set; }

        /// <summary>
        /// Power of the brakes, setting it to zero leaves car without brakes...
        /// </summary>
        public float BrakeFriction { get; set; }

        /// <summary>
        /// How responsive the car steers - more = faster
        /// </summary>
        public float TurnIn { get; set; }

        /// <summary>
        /// For how much the car steers, max=1.0, higher values can cause "rotating on spot" effect.
        /// </summary>
        public float TurnRatio { get; set; }

        /// <summary>
        /// How stable the car is when you go backwards.
        /// </summary>
        public float RearEndStability { get; set; }

        /// <summary>
        /// Set this to lower to increase the "lock up" when you use handbrake to steer with large speeds
        /// </summary>
        public float HandbrakeSlideValue { get; set; }

        /// <summary>
        /// The car engine's power, more thrust will result in faster acceleration.
        /// </summary>
        public float Thrust { get; set; }

        /// <summary>
        /// The maximum speed that car can reach (in gear 3).
        /// </summary>
        public float MaxSpeed { get; set; }

        /// <summary>
        /// Set to zero to make car invulnerable to all psyhical damages except drowning (not even carbombs will hurt you), higher values makes car more vulnerable.
        /// </summary>
        public float AntiStrength { get; set; }

        /// <summary>
        /// Lower value will make your car leave skid alaways when you steer, highier values will leave skids on highier speeds.
        /// </summary>
        public float SkidThreshold { get; set; }

        /*
         * gear multipliers should divide the max_speed into 3 gear "speed zones"
         * ideal would be (0.250, 0.500, 1.000) but if you use something like (0.333, 0.333, 0.333)
         * the car's max speed will never be reached because the gear 3 multiplier is not 1.000 which means full speed of gear 3.
         */

        /// <summary>
        /// ToDo
        /// </summary>
        public float Gear1Multiplier { get; set; }

        /// <summary>
        /// ToDo
        /// </summary>
        public float Gear2Multiplier { get; set; }

        /// <summary>
        /// ToDo
        /// </summary>
        public float Gear3Multiplier { get; set; }

        /// <summary>
        /// Above this speed the car switches to gear 2.
        /// </summary>
        public float Gear2Speed { get; set; }

        /// <summary>
        /// Above this speed the car switches to gear 3.
        /// </summary>
        public float Gear3Speed { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
