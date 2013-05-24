// GTA2.NET
// 
// File: SpriteRemapsWindow.cs
// Created: 27.04.2013
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
using Hiale.GTA2NET.Core.Style;
using WeifenLuo.WinFormsUI.Docking;

namespace Hiale.GTA2NET.WinUI.DockWindows
{
    public partial class SpriteRemapsWindow : DockContent
    {
        public class RemapEventArgs : EventArgs
        {
            public Remap Remap;

            public RemapEventArgs(Remap remap)
            {
                Remap = remap;
            }
        }

        public IList<Remap> Remaps
        {
            get
            {
                var list = new List<Remap>();
                for (var i = 1; i < radioListBoxRemaps.Items.Count; i++)
                    list.Add((Remap)radioListBoxRemaps.Items[i]);
                return list;
            }
            set
            {
                radioListBoxRemaps.Items.Clear();
                foreach (var remap in value)
                    radioListBoxRemaps.Items.Add(remap);
                radioListBoxRemaps.Items.Insert(0, "Default");
                radioListBoxRemaps.SelectedIndex = 0;
            }
        }

        public event EventHandler<RemapEventArgs> RemapChanged; 

        //-1 = Default
        public Remap SelectedItem
        {
            get
            {
                if (radioListBoxRemaps.SelectedIndex == 0)
                    return new Remap(-1, -1);
                return (Remap) radioListBoxRemaps.SelectedItem;
            }
            set
            {
                if (value.Key == -1)
                {
                    radioListBoxRemaps.SelectedIndex = 0;
                    return;
                }
                if (value.Key > byte.MaxValue)
                    throw new ArgumentException();
                for (var i = 1; i < radioListBoxRemaps.Items.Count; i++)
                {
                    if ((int) radioListBoxRemaps.Items[i] != value.Key)
                        continue;
                    radioListBoxRemaps.SelectedIndex = i;
                    break;
                }
            }
        }

        public SpriteRemapsWindow()
        {
            InitializeComponent();
        }

        private void RadioListBoxRemapsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (RemapChanged != null)
                RemapChanged(this, new RemapEventArgs(SelectedItem));

        }
    }
}
