// GTA2.NET
// 
// File: GTA2Game.cs
// Created: 22.08.2013
// Created by: João Pires
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Core.Logic;
using Hiale.GTA2NET.Core.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hiale.GTA2NET.Core
{
    /// <summary>
    /// Responsible for the simulation of the game
    /// </summary>
    public class GTA2Game
    {
        private Map.Map map;
        //Physic stuff
        private Physics _physics;
        private Sprites sprites;
        private List<CarInfo> CarInfoList;
        private PlayerInput playerInput;

        public GTA2Game(string mapName, string styleName)
        {
            map = new Map.Map(mapName, styleName);
            map.CalcCoord();

            _physics = new Physics();
            _physics.Initialize(map);

            sprites = new Sprites();
        }

        /// <summary>
        /// Receives the input of the player.
        /// </summary>
        /// <param name="input">The input from the player.</param>
        public void Input(PlayerInput input)
        {
            playerInput = input;
        }

        /// <summary>
        /// Update the simulation.
        /// </summary>
        /// <param name="elapsedTime">The time elapsed from the las update.</param>
        public void Update(float elapsedTime) //ToDo: put this method private. Before is necessary to create a simulation thread.
        {
        }

        /// <summary>
        /// Gets the 3D data from map centered in pos.
        /// </summary>
        /// <param name="pos">The center of square</param>
        /// <param name="size">The size of the square</param>
        /// <returns></returns>
        public ModelData getPosition(Vector2 pos, uint size = 10)
        {
            Contract.Requires(map.ValidPosition(new Vector3(pos, 2)));

            //ToDo: work the case when some blocks don't exist because they are outside the map. In that case the frontier blocks should be repeated.
            List<VertexPositionNormalTexture> VertexPosList = new List<VertexPositionNormalTexture>();
            List<int> IndexBufferList = new List<int>();

            for (uint i = (uint)pos.X - size; i < pos.X + size; i++)
            {
                for (uint j = (uint)(pos.Y) - size; j < pos.Y + size; j++)
                {
                    for (uint k = 0; k < map.Height; k++)
                    {
                        Block block = map.GetBlock(new Vector3(i, j, k));
                        int idx = 0;
                        foreach (VertexPositionNormalTexture vx in block.Coors)
                        {
                            VertexPosList.Add(vx);
                            idx++;
                        }
                        int c = VertexPosList.Count - idx;
                        foreach (int ib in block.IndexBufferCollection)
                        {
                            IndexBufferList.Add(c + ib);
                        }
                    }
                }
            }
            return new ModelData(VertexPosList, IndexBufferList, null, null);
        }
    }
}
