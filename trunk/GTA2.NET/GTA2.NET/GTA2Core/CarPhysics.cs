//Created: 28.01.2010
//Source: http://www.gtaforums.com/index.php?showtopic=147090

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiale.GTA2.Core
{
    public class CarPhysics
    {
        private string _Name;
        /// <summary>
        /// The car's name.
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private int _Model;
        /// <summary>
        /// Model number used to asociate car handlings with .sty file sprites
        /// </summary>
        public int Model
        {
            get { return _Model; }
            set { _Model = value; }
        }

        private bool _Turbo;
        /// <summary>
        /// If set the car accelerates faster and stronger at lower speeds to reach highier speeds faster
        /// </summary>
        public bool Turbo
        {
            get { return _Turbo; }
            set { _Turbo = value; }
        }

        private int _Value;
        /// <summary>
        /// The car's cost
        /// </summary>
        public int Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        private float _Mass;
        /// <summary>
        /// The car's mass - more means that the car will be less vurnelable to all kind of damages, and the car will steer less responsive.
        /// </summary>
        public float Mass
        {
            get { return _Mass; }
            set { _Mass = value; }
        }

        private float _FrontDriveBias;
        /// <summary>
        /// Highier values makes front of car more responsive.
        /// </summary>
        public float FrontDriveBias
        {
            get { return _FrontDriveBias; }
            set { _FrontDriveBias = value; }
        }

        private float _FrontMassBias;
        /// <summary>
        /// Highier values makes car steering less responsive at highier speeds.
        /// </summary>
        public float FrontMassBias
        {
            get { return _FrontMassBias; }
            set { _FrontMassBias = value; }
        }

        private float _BrakekFriction;
        /// <summary>
        /// Power of the brakes, setting it to zero leaves car without brakes...
        /// </summary>
        public float BrakeFriction
        {
            get { return _BrakekFriction; }
            set { _BrakekFriction = value; }
        }

        private float _TurnIn;
        /// <summary>
        /// How responsive the car steers - more = faster
        /// </summary>
        public float TurnIn
        {
            get { return _TurnIn; }
            set { _TurnIn = value; }
        }

        private float _TurnRatio;
        /// <summary>
        /// For how much the car steers, max=1.0, higher values can cause "rotating on spot" effect.
        /// </summary>
        public float TurnRatio
        {
            get { return _TurnRatio; }
            set { _TurnRatio = value; }
        }

        private float _RearEndStability;
        /// <summary>
        /// How stable the car is when you go backwards.
        /// </summary>
        public float RearEndStability
        {
            get { return _RearEndStability; }
            set { _RearEndStability = value; }
        }

        private float _HandbrakeSlideValue;
        /// <summary>
        /// Set this to lower to increase the "lock up" when you use handbrake to steer with large speeds
        /// </summary>
        public float HandbrakeSlideValue
        {
            get { return _HandbrakeSlideValue; }
            set { _HandbrakeSlideValue = value; }
        }

        private float _Thrust;
        /// <summary>
        /// The car engine's power, more thrust will result in faster acceleration.
        /// </summary>
        public float Thrust
        {
            get { return _Thrust; }
            set { _Thrust = value; }
        }

        private float _MaxSpeed;
        /// <summary>
        /// The maximum speed that car can reach (in gear 3).
        /// </summary>
        public float MaxSpeed
        {
            get { return _MaxSpeed; }
            set { _MaxSpeed = value; }
        }

        private float _AntiStrength;
        /// <summary>
        /// Set to zero to make car invulnerable to all psyhical damages except drowning (not even carbombs will hurt you), higher values makes car more vulnerable.
        /// </summary>
        public float AntiStrength
        {
            get { return _AntiStrength; }
            set { _AntiStrength = value; }
        }

        private float _SkidThreshold;
        /// <summary>
        /// Lower value will make your car leave skid alaways when you steer, highier values will leave skids on highier speeds.
        /// </summary>
        public float SkidThreshold
        {
            get { return _SkidThreshold; }
            set { _SkidThreshold = value; }
        }

        /*
         * gear multipliers should divide the max_speed into 3 gear "speed zones"
         * ideal would be (0.250, 0.500, 1.000) but if you use something like (0.333, 0.333, 0.333)
         * the car's max speed will never be reached because the gear 3 multiplier is not 1.000 which means full speed of gear 3.
         */

        private float _Gear1Multiplier;
        /// <summary>
        /// ToDo
        /// </summary>
        public float Gear1Multiplier
        {
            get { return _Gear1Multiplier; }
            set { _Gear1Multiplier = value; }
        }

        private float _Gear2Multiplier;
        /// <summary>
        /// ToDo
        /// </summary>
        public float Gear2Multiplier
        {
            get { return _Gear2Multiplier; }
            set { _Gear2Multiplier = value; }
        }

        private float _Gear3Multiplier;
        /// <summary>
        /// ToDo
        /// </summary>
        public float Gear3Multiplier
        {
            get { return _Gear3Multiplier; }
            set { _Gear3Multiplier = value; }
        }

        private float _Gear2Speed;
        /// <summary>
        /// Above this speed the car switches to gear 2.
        /// </summary>
        public float Gear2Speed
        {
            get { return _Gear2Speed; }
            set { _Gear2Speed = value; }
        }

        private float _Gear3Speed;
        /// <summary>
        /// Above this speed the car switches to gear 3.
        /// </summary>
        public float Gear3Speed
        {
            get { return _Gear3Speed; }
            set { _Gear3Speed = value; }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
