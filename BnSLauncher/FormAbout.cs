using System;
using System.Drawing;
using System.Windows.Forms;
using Miyako;

namespace BnSLauncher
{
    public partial class FormAbout : Form
    {
        bool blinked = false;
        private bool mouseDown;         // Is mouse clicked?
        private Point lastLocation;     // Remember window location

        public FormAbout()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            // stop audio
            Music.stopSong();   // Free memory
        }

        private unsafe void FormAbout_Load(object sender, EventArgs e)
        {
            // Play song from resource at full volume
            Music.playSong("BnSLauncher.xmfile.xm", 25);

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (blinked == false)
            {
                label1.Visible = false;
                blinked = true;
            }
            else
            {
                label1.Visible = true;
                blinked = false;
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://miyako.pro");
        }
        // Mouse clicked
        private void moveFormDn(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }
        // Dragging mouse
        private void moveForm(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }
        // Let go of mouse
        private void moveFormUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }
    }
}
