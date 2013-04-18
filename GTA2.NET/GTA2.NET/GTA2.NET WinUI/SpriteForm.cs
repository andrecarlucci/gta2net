using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Hiale.GTA2NET.WinUI
{
    public partial class SpriteForm : Form
    {
        public SpriteForm()
        {
            InitializeComponent();
        }

        private void MnuFileCloseClick(object sender, EventArgs e)
        {
            Close();
        }

        private void MnuFileOpenClick(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                //
            }
        }

        private void LoadSprites(string fileName)
        {

        }


    }
}
