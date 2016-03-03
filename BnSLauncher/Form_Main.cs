using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.Win32;
using System.IO;
using System.Windows;



namespace BnSLauncher
{
    public partial class FormMain : Form
    {

        public FormMain()
        {
            InitializeComponent();
        }

        string IP = "64.25.35.100";
        string regionID = "0";
        string languageID = "English";
        string InstallPath = "";
        string LaunchPath = "";


        private void button1_Click(object sender, EventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = LaunchPath;
            proc.StartInfo.Arguments = "-lang:" + languageID + " -lite:2 -region:" + regionID + "/sesskey /launchbylauncher /LoginMode:2 /CompanyID:12 /ChannelGroupIndex:-1";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            try
            {
                proc.Start();
            }
            catch
            {
                MessageBox.Show("Error: Could not start Client.exe!");
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedValue = "English";
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedValue = "NA";
            label3.Text = new Ping().Send(IP).RoundtripTime.ToString() + "ms";
            InstallPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\NCWest\BnS", "BaseDir", null);
            if (InstallPath != null)
            {
                LaunchPath = InstallPath + "\\bin\\Client.exe";
            }
            else if (File.Exists(".\\Client.exe"))
            {
                LaunchPath = ".\\Client.exe";
            }
            else
            {
                MessageBox.Show("Error: Could not find Client.exe! Please place in the BnS folder with Client.exe");
            }

            if (!File.Exists(".\\ufmod.dll"))
            {
                MessageBox.Show("Error: Could not find ufmod.dll!");
                Application.Exit();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label3.Text = new Ping().Send(IP).RoundtripTime.ToString() + "ms";

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "EU")
            {
                IP = "206.127.148.171";
                regionID = "1";
                label3.Text = new Ping().Send(IP).RoundtripTime.ToString() + "ms";
            }
            else
            {
                IP = "64.25.35.100";
                regionID = "0";
                label3.Text = new Ping().Send(IP).RoundtripTime.ToString() + "ms";
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem.ToString() == "French")
            {
                languageID = "French";
            }
            else if (comboBox2.SelectedItem.ToString() == "German")
            {
                languageID = "German";
            }
            else
            {
                languageID = "English";
            }
        }

        private void modsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormMods frmMods = new FormMods();
            frmMods.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout frmAbout = new FormAbout();
            frmAbout.Show();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();//shows the program on taskbar
            this.WindowState = FormWindowState.Normal;//undoes the minimized state of the form
            notifyIcon1.Visible = false;//hides tray icon again
        }

        private void FormMain_Resize(object sender, EventArgs e)
	{
	    if (this.WindowState == FormWindowState.Minimized)//this code gets fired on every resize
	    {
            //so we check if the form was minimized
	        Hide();//hides the program on the taskbar
	        notifyIcon1.Visible = true;//shows our tray icon
            notifyIcon1.ShowBalloonTip(50, "BnS Launcher", "Minimized to tray.", ToolTipIcon.Info);
	    }
	}

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = LaunchPath;
            proc.StartInfo.Arguments = "-lang:" + languageID + " -lite:2 -region:" + regionID + "/sesskey /launchbylauncher /LoginMode:2 /CompanyID:12 /ChannelGroupIndex:-1";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            try
            {
                proc.Start();
            }
            catch
            {
                MessageBox.Show("Error: Could not start Client.exe!");
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            FormMods frmMods = new FormMods();
            frmMods.Show();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            FormAbout frmAbout = new FormAbout();
            frmAbout.Show();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
