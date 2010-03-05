//Created: 28.01.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Hiale.GTA2.Core
{
    public class CarPhysicReader
    {
        private const string fileName = "data\\nyc.gci";

        public CarPhysicReader()
        {
            
        }

        public List<CarPhysics> ReadFromFile()
        {
            if (!File.Exists(fileName))
                throw new Exception();
            StreamReader reader = new StreamReader(fileName);
            string text = reader.ReadToEnd();
            reader.Close();
            List<CarPhysics> cars = new List<CarPhysics>();
            try
            {
                
                Regex RegEx = new Regex(@"f?(.*?)\ ?\{(.*)\}", RegexOptions.Multiline);
                MatchCollection matches = RegEx.Matches(text);
                CarPhysics CurrentCar = null;
                for (int i = 0; i < matches.Count; i++)
                {
                    string value = matches[i].Groups[1].Value;
                    string type = matches[i].Groups[2].Value;
                    if (value.Length < 1) //value is empty --> title / {carname}
                    {
                        CurrentCar = new CarPhysics();
                        CurrentCar.Name = type;
                        continue;
                    }
                    switch (type)
                    {
                        case "model":
                            CurrentCar.Model = int.Parse(value);
                            break;
                        case "turbo":
                            CurrentCar.Turbo = (value == "1" ? true : false);
                            break;
                        case "value":
                            CurrentCar.Value = int.Parse(value);
                            break;
                        case "mass":
                            CurrentCar.Mass = ParseFloat(value);
                            break;
                        case "front drive bias":
                            CurrentCar.FrontDriveBias = ParseFloat(value);
                            break;
                        case "front mass bias":
                            CurrentCar.FrontMassBias = ParseFloat(value);
                            break;
                        case "brake friction":
                            CurrentCar.BrakeFriction = ParseFloat(value);
                            break;
                        case "turn in":
                            CurrentCar.TurnIn = ParseFloat(value);
                            break;
                        case "turn ratio":
                            CurrentCar.TurnRatio = ParseFloat(value);
                            break;
                        case "rear end stability":
                            CurrentCar.RearEndStability = ParseFloat(value);
                            break;
                        case "handbrake slide value":
                            CurrentCar.HandbrakeSlideValue = ParseFloat(value);
                            break;
                        case "thrust":
                            CurrentCar.Thrust = ParseFloat(value);
                            break;
                        case "max_speed":
                            CurrentCar.MaxSpeed = ParseFloat(value);
                            break;
                        case "anti strength":
                            CurrentCar.AntiStrength = ParseFloat(value);
                            break;
                        case "skid threshhold":
                            CurrentCar.SkidThreshold = ParseFloat(value);
                            break;
                        case "gear1 multiplier":
                            CurrentCar.Gear1Multiplier = ParseFloat(value);
                            break;
                        case "gear2 multiplier":
                            CurrentCar.Gear2Multiplier = ParseFloat(value);
                            break;
                        case "gear3 multiplier":
                            CurrentCar.Gear3Multiplier = ParseFloat(value);
                            break;
                        case "gear2 speed":
                            CurrentCar.Gear2Speed = ParseFloat(value);
                            break;
                        case "gear3 speed":
                            CurrentCar.Gear3Speed = ParseFloat(value);
                            cars.Add(CurrentCar);
                            break;
                        //default:
                        //    System.Diagnostics.Debug.WriteLine("UNKNOWN: " + type);
                        //    break;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("The file \"nyc.gci\" (car handling data) contains invalid data.");
            }
            return cars;
        }

        private float ParseFloat(string s)
        {
            return float.Parse(s, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }
    }
}
