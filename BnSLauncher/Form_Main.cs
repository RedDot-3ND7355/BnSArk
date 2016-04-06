using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using Miyako;
using System.Net.NetworkInformation;
using System.Net;

namespace BnSLauncher
{
    public partial class FormMain : Form
    {

        string InstallPath = "";
        string DatPath = "";
        string TempPath = Path.GetTempPath();
        string AppPath = Path.GetDirectoryName(Application.ExecutablePath);
        string IP = "64.25.35.100";
        string regionID = "0";
        string languageID = "English";
        string LaunchPath = "";
        string NoTextureStreamingBool = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Miyako\BnSArk", "NoTextureStreaming", "false");
        string UnattendedBool = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Miyako\BnSArk", "Unattended", "false");
        string NoTextureStreaming = "";
        string Unattended = "";


        public FormMain()
        {
            InitializeComponent();
            // Load our unamanaged DLLs
            ResDll.ExtractDll("ufmod.dll", Properties.Resources.ufmod);
            ResDll.LoadDll("ufmod.dll");
            /*
            File.Delete(TempPath + "\\About.exe");
            File.WriteAllBytes(TempPath + "\\About.exe", BnSLauncher.Properties.Resources.About);
            File.WriteAllBytes(TempPath + "\\hspogg.dll", BnSLauncher.Properties.Resources.hspogg);
            File.WriteAllBytes(TempPath + "\\theme.ogg", BnSLauncher.Properties.Resources.theme);
            */
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string NoTextureStreamingBool = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Miyako\BnSArk", "NoTextureStreaming", "false");
            string UnattendedBool = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Miyako\BnSArk", "Unattended", "false");
            if (NoTextureStreamingBool == "true")
            {
                NoTextureStreaming = "-NOTEXTURESTREAMING";
            } else { NoTextureStreaming = ""; }
            if (UnattendedBool == "true")
            {
                Unattended = "-UNATTENDED";
            } else { Unattended = ""; }

            Process proc = new Process();
            proc.StartInfo.FileName = LaunchPath;
            //  /launchbylauncher -lang:english /CompanyID:"12" /ChannelGroupIndex:"-1" -lang:English -lite:2 -region:0
            proc.StartInfo.Arguments = "-lang:" + languageID + " -lite:2 -region:" + regionID + "/sesskey /launchbylauncher  /CompanyID:12 /ChannelGroupIndex:-1 -USEALLAVAILABLECORES " + Unattended + " " + NoTextureStreaming;
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
            if (!Directory.Exists(AppPath + "\\patch")) { Directory.CreateDirectory(AppPath + "\\patch"); }
            if (!Directory.Exists(AppPath + "\\mods")) { Directory.CreateDirectory(AppPath + "\\mods"); }
            if (!Directory.Exists(AppPath + "\\splash")) { Directory.CreateDirectory(AppPath + "\\splash"); }
            if (!Directory.Exists(AppPath + "\\data")) { Directory.CreateDirectory(AppPath + "\\data"); }

            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedValue = "English";
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedValue = "NA";
            label3.Text = new Ping().Send(IP).RoundtripTime.ToString() + "ms";
            // Find Client.exe and set file paths
            // Check the registry
            if (NoTextureStreamingBool == "true") {
                NoTextureStreaming = "-NOTEXTURESTREAMING";
            }
            if (UnattendedBool == "true") {
                Unattended = "-UNATTENDED";
            }

            InstallPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\NCWest\BnS", "BaseDir", null);
            if (InstallPath != null)
            {
                DatPath = InstallPath + "\\contents\\Local\\NCWEST\\data\\";
                LaunchPath = InstallPath + "\\bin\\Client.exe";
            }
            // registry path not found, check for side-by-side install
            else if (File.Exists(".\\Client.exe"))
            {
                DatPath = "..\\contents\\Local\\NCWEST\\data\\";
                LaunchPath = ".\\Client.exe";

            }
            // client.exe couldn't be found!
            else
            {
                MessageBox.Show("Error: Could not find Client.exe! Please place in the BnS folder with Client.exe");
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
            /*
            Process about = new Process();
            about.StartInfo.UseShellExecute = false;
            about.StartInfo.FileName = TempPath + "About.exe";
            about.Start();
            while (!about.HasExited)
            {
                Application.DoEvents(); // This keeps your form responsive by processing events
            }
            */
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
            proc.StartInfo.Arguments = "-lang:" + languageID + " -lite:2 -region:" + regionID + "/sesskey /launchbylauncher  /CompanyID:12 /ChannelGroupIndex:-1 -USEALLAVAILABLECORES " + Unattended + " " + NoTextureStreaming;
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
            /*
            Process about = new Process();
            about.StartInfo.UseShellExecute = false;
            about.StartInfo.FileName = TempPath + "About.exe";
            about.Start();
            while (!about.HasExited)
            {
                Application.DoEvents(); // This keeps your form responsive by processing events
            }
            */
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            try {
                FormMain.ActiveForm.Text = "BnSArk v1.5 (Patching Client)";
                if (File.Exists(AppPath + "\\patch\\config.dat")) { File.Delete(AppPath + "\\patch\\config.dat"); }
                WebClient Client = new WebClient();
                Client.DownloadFileCompleted += new AsyncCompletedEventHandler(patchDownloaded);
                Client.DownloadFileAsync(new Uri("http://www.miyako.pro/files/Games/Blade%20and%20Soul/bnsark/patch/config.dat"), AppPath + "\\patch\\config.dat");
            }
            catch
            {
                FormMain.ActiveForm.Text = "BnSArk v1.5";
                MessageBox.Show("Error: Unable to retrieve patch!");
                
            }
        }
        private void patchDownloaded (object sender, EventArgs e)
        {
            FormMain.ActiveForm.Text = "BnSArk v1.5";
            try
            {
                File.Copy(AppPath + "\\patch\\config.dat", DatPath + "config.dat", true);
                MessageBox.Show("Client has been patched to enable launching.");
            }
            catch
            {
                MessageBox.Show("Error: Unable to patch client!");
            }
            
        }

        private void chatSupportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://miyako.pro/chat");
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://miyako.pro/chat");
        }
    }
}
