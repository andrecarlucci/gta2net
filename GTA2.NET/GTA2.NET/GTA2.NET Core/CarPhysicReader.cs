// GTA2.NET
// 
// File: CarPhysicReader.cs
// Created: 28.01.2010
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
using System.Text.RegularExpressions;

namespace Hiale.GTA2NET.Core
{
    public class CarPhysicReader
    {
        private const string FileName = "misc\\nyc.gci";

        public static Dictionary<int,CarPhysics> ReadFromFile()
        {
            if (!File.Exists(FileName))
                throw new FileNotFoundException("File not found!", FileName);
            var reader = new StreamReader(FileName);
            string text = reader.ReadToEnd();
            reader.Close();
            var cars = new Dictionary<int, CarPhysics>();
            try
            {
                var regEx = new Regex(@"f?(.*?)\ ?\{(.*)\}", RegexOptions.Multiline);
                MatchCollection matches = regEx.Matches(text);
                CarPhysics currentCar = null;
                for (var i = 0; i < matches.Count; i++)
                {
                    var value = matches[i].Groups[1].Value;
                    var type = matches[i].Groups[2].Value;
                    if (value.Length < 1) //value is empty --> title / {carname}
                    {
                        currentCar = new CarPhysics();
                        currentCar.Name = type;
                        continue;
                    }
                    switch (type)
                    {
                        case "model":
                            currentCar.Model = int.Parse(value);
                            break;
                        case "turbo":
                            currentCar.Turbo = (value == "1" ? true : false);
                            break;
                        case "value":
                            currentCar.Value = int.Parse(value);
                            break;
                        case "mass":
                            currentCar.Mass = ParseFloat(value);
                            break;
                        case "front drive bias":
                            currentCar.FrontDriveBias = ParseFloat(value);
                            break;
                        case "front mass bias":
                            currentCar.FrontMassBias = ParseFloat(value);
                            break;
                        case "brake friction":
                            currentCar.BrakeFriction = ParseFloat(value);
                            break;
                        case "turn in":
                            currentCar.TurnIn = ParseFloat(value);
                            break;
                        case "turn ratio":
                            currentCar.TurnRatio = ParseFloat(value);
                            break;
                        case "rear end stability":
                            currentCar.RearEndStability = ParseFloat(value);
                            break;
                        case "handbrake slide value":
                            currentCar.HandbrakeSlideValue = ParseFloat(value);
                            break;
                        case "thrust":
                            currentCar.Thrust = ParseFloat(value);
                            break;
                        case "max_speed":
                            currentCar.MaxSpeed = ParseFloat(value);
                            break;
                        case "anti strength":
                            currentCar.AntiStrength = ParseFloat(value);
                            break;
                        case "skid threshhold":
                            currentCar.SkidThreshold = ParseFloat(value);
                            break;
                        case "gear1 multiplier":
                            currentCar.Gear1Multiplier = ParseFloat(value);
                            break;
                        case "gear2 multiplier":
                            currentCar.Gear2Multiplier = ParseFloat(value);
                            break;
                        case "gear3 multiplier":
                            currentCar.Gear3Multiplier = ParseFloat(value);
                            break;
                        case "gear2 speed":
                            currentCar.Gear2Speed = ParseFloat(value);
                            break;
                        case "gear3 speed":
                            currentCar.Gear3Speed = ParseFloat(value);
                            cars.Add(currentCar.Model, currentCar);
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

        private static float ParseFloat(string s)
        {
            return float.Parse(s, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }
    }
}
