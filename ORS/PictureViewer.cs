using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ORS
{
    public partial class PictureViewer : Form
    {
        System.Windows.Forms.Timer _countdownTimer = new System.Windows.Forms.Timer();
        uint timerTicked = 0;

        public PictureViewer(string picture)
        {
            InitializeComponent();
            try
            {
                Image imageToDisplay = ResizeImage(picture, 512, 512, true);
                pictureBox1.Image = imageToDisplay;
            }
            catch
            {
                MessageBox.Show("Could not display " + picture + ". This might be because you tried to trick me by renaming a file that was not really an image to have an image-like extension. Or maybe the file is just corrupt.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }          
            _countdownTimer.Interval = 500; //ms
            _countdownTimer.Tick += new EventHandler(_countdownTimer_Tick);
            _countdownTimer.Enabled = true;
        }

        void _countdownTimer_Tick(object sender, EventArgs e)
        {
            if (timerTicked > 2)
                this.Dispose();
            timerTicked++;
        }

        public Image ResizeImage(string OriginalFile, int NewWidth, int MaxHeight, bool OnlyResizeIfWider)
        {
            System.Drawing.Image FullsizeImage = System.Drawing.Image.FromFile(OriginalFile);

            // Prevent using images internal thumbnail
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);

            if (OnlyResizeIfWider)
            {
                if (FullsizeImage.Width <= NewWidth)
                {
                    NewWidth = FullsizeImage.Width;
                }
            }

            int NewHeight = FullsizeImage.Height * NewWidth / FullsizeImage.Width;
            if (NewHeight > MaxHeight)
            {
                // Resize with height instead
                NewWidth = FullsizeImage.Width * MaxHeight / FullsizeImage.Height;
                NewHeight = MaxHeight;
            }

            System.Drawing.Image NewImage = FullsizeImage.GetThumbnailImage(NewWidth, NewHeight, null, IntPtr.Zero);
            return NewImage;
        }

        private void PictureViewer_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
        }
    }
}
