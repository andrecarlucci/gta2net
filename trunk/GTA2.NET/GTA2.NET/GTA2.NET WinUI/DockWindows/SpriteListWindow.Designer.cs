namespace Hiale.GTA2NET.WinUI.DockWindows
{
    partial class SpriteListWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listBoxSprites = new Hiale.GTA2NET.WinUI.Controls.CustomListBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.listBoxSprites);
            this.groupBox1.Location = new System.Drawing.Point(14, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(257, 332);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sprites";
            // 
            // listBoxSprites
            // 
            this.listBoxSprites.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxSprites.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxSprites.FormattingEnabled = true;
            this.listBoxSprites.IntegralHeight = false;
            this.listBoxSprites.ItemHeight = 68;
            this.listBoxSprites.Location = new System.Drawing.Point(6, 19);
            this.listBoxSprites.Name = "listBoxSprites";
            this.listBoxSprites.Size = new System.Drawing.Size(245, 307);
            this.listBoxSprites.SpriteAtlas = null;
            this.listBoxSprites.SpriteImage = null;
            this.listBoxSprites.TabIndex = 0;
            this.listBoxSprites.SelectedIndexChanged += new System.EventHandler(this.ListBoxSpritesSelectedIndexChanged);
            // 
            // SpriteListWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 356);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HideOnClose = true;
            this.Name = "SpriteListWindow";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Document;
            this.Text = "Sprites";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private Controls.CustomListBox listBoxSprites;
    }
}