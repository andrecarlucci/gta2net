// GTA2.NET
// 
// File: FigureSolver.cs
// Created: 12.06.2013
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
using Hiale.GTA2NET.Core.Helper;

namespace Hiale.GTA2NET.Core.Collision
{
    public class FigureSolver
    {
        public static SplitterFigure Solve(List<SplitterFigure> figureSplitters)
        {
            SplitterFigure preferedFigure = null;
            var maxValue = float.MinValue;
            foreach (var figureSplitter in figureSplitters)
            {
                bool isRectangle;
                var polygon = new Polygon(figureSplitter.Map, figureSplitter.Layer);
                var polygonVertices = polygon.CreatePolygon(figureSplitter.Lines, out isRectangle);
                var polygonArea = Geometry.CalculatePolygonArea(polygonVertices);
                if (polygonArea <= maxValue)
                    continue;
                preferedFigure = figureSplitter;
                maxValue = polygonArea;
                preferedFigure.Polygon = polygonVertices;
                preferedFigure.IsRectangle = isRectangle;
            }
            return preferedFigure;
        }
    }
}
