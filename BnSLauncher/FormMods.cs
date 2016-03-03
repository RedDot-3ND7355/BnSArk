using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using FastColoredTextBoxNS;

namespace BnSLauncher
{
    public partial class FormMods : Form
    {
        // Prototype variables
        Style BlueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Bold);
        Style RedStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);
        Style MaroonStyle = new TextStyle(Brushes.Maroon, null, FontStyle.Regular);
        Style GreenStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
        string InstallPath = "";
        string ModPath = "";
        string SplashPath = "";
        string DatPath = "";
        string NewDat = "";
        string NewSplash = "";
        string NewSplashID = "";
        string SelectedMod = "";
        string EnabledMods = "";
        string ActiveDataFile = "";
        string TempPath = Path.GetTempPath();
        string AppPath = Path.GetDirectoryName(Application.ExecutablePath);
        string XmlSavePath = "";
        bool PathsFound = false;
        // Memory Addresses and Offsets
        /*
        string ClientProcess = "Client.exe";
        string SlidersBaseModule = "bsengine_Shipping.dll";
        string SlidersBaseAddr = "0x01FD4318";
        string SlidersGonFOfs = "0x10C0";
        string SlidersGonMOfs = "0x96C";
        string SlidersJinFOfs = "0x12D8";
        string SlidersJinMOfs = "0xB84";
        string SlidersLynFOfs = "0x11CC";
        string SlidersLynMOfs = "0xA78";
        string SlidersYunFOfs = "0xFB4";
        */

        public FormMods()
        {
            InitializeComponent();
            //set delay interval (10 ms)
            fastColoredTextBox1.DelayedEventsInterval = 10;
        }

        // Clear memory on exit
        private void FormMods_Exit(object sender, EventArgs e)
        {
            pictureBox1.Image.Dispose();
        }

        private void FormMods_Load(object sender, EventArgs e)
        {
            // Set tool tip values
            //toolTip1.SetToolTip(label1, "Not Available Yet...");
            // Set config.dat as default for the editor
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedValue = "config.dat";

            // Find the UPK Mods in the /mods/ folder
            try
            {
                DirectoryInfo dinfo = new DirectoryInfo(@".\\mods\\");
                DirectoryInfo[] Files = dinfo.GetDirectories();
                foreach (DirectoryInfo file in Files)
                {
                    checkedListBox1.Items.Add(file.Name);
                }
                checkedListBox1.SelectedIndex = 0;
                toolStripStatusLabel1.Text = "Mod Folder Found!";
            }
            catch
            {
                toolStripStatusLabel1.Text = "Mod Folder Not Found!";
            }
            // Select Mods
            try {
                using (StreamReader reader = new StreamReader(".\\mods.db"))
                {
                    reader.ReadToEnd().Split(',').ToList().ForEach(item =>
                    {
                        var index = checkedListBox1.Items.IndexOf(item);
                        checkedListBox1.SetItemChecked(index, true);
                    });
                }
            }
            catch
            {
                toolStripStatusLabel1.Text = "Loaded Database";
            }
            
            // Find Client.exe and set file paths
            // Check the registry
            InstallPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\NCWest\BnS", "BaseDir", null);
            if (InstallPath != null)
            {
                ModPath = InstallPath + "\\contents\\Local\\NCWEST\\ENGLISH\\CookedPC\\mod";
                SplashPath = InstallPath + "\\contents\\Local\\NCWEST\\ENGLISH\\Splash\\";
                DatPath = InstallPath + "\\contents\\Local\\NCWEST\\data\\";
                PathsFound = true;
            }
            // registry path not found, check for side-by-side install
            else if (File.Exists(".\\Client.exe"))
            {
                ModPath = "..\\contents\\Local\\NCWEST\\ENGLISH\\CookedPC\\mod";
                SplashPath = "..\\contents\\Local\\NCWEST\\ENGLISH\\Splash\\";
                DatPath = "..\\contents\\Local\\NCWEST\\data\\";
                PathsFound = true;
            }
            // client.exe couldn't be found!
            else
            {
                MessageBox.Show("Error: Could not find Client.exe! Please place in the BnS folder with Client.exe");
                PathsFound = false;
            }

            // Extract bnsdat.exe to %tempdir%
            File.WriteAllBytes(TempPath + "\\bnsdat.exe", BnSLauncher.Properties.Resources.bnsdat);

            // Load the active splash screen in the splash replacement tab.
            if (PathsFound == true)
            {
                // Backup the splash screen
                pictureBox1.Image = Image.FromFile(SplashPath + "Splash.bmp");
                if (Directory.Exists(SplashPath + "backup"))
                {
                    if (File.Exists(SplashPath + "backup\\Splash.bmp"))
                    {
                        //return;
                    }
                    else if (!File.Exists(SplashPath + "backup\\Splash.bmp"))
                    {
                        File.Copy(SplashPath + "Splash.bmp", SplashPath + "backup\\Splash.bmp");
                    }
                }
                if (!Directory.Exists(SplashPath + "backup"))
                {
                    Directory.CreateDirectory(SplashPath + "backup");
                    File.Copy(SplashPath + "Splash.bmp", SplashPath + "backup\\Splash.bmp");
                }
                // Backup the data files
                if (Directory.Exists(DatPath + "backup"))
                {
                    if (File.Exists(DatPath + "backup\\config.dat"))
                    {
                       // return;
                    }
                    else if (!File.Exists(DatPath + "backup\\config.dat"))
                    {
                        File.Copy(DatPath + "config.dat", DatPath + "backup\\config.dat");
                        File.Copy(DatPath + "xml.dat", DatPath + "backup\\xml.dat");
                    }
                }
                if (!Directory.Exists(DatPath + "backup"))
                {
                    Directory.CreateDirectory(DatPath + "backup");
                    File.Copy(DatPath + "config.dat", DatPath + "backup\\config.dat");
                    File.Copy(DatPath + "xml.dat", DatPath + "backup\\xml.dat");
                }
            }
            else
            {
            }

            // populate tree view
            treeView1.Nodes.Clear();
            DirectoryInfo directoryInfo = new DirectoryInfo(@".\\data\\config.dat.files\\");
            if (directoryInfo.Exists)
            {
                treeView1.AfterSelect += treeView1_AfterSelect;
                BuildTree(directoryInfo, treeView1.Nodes);
                treeView1.Nodes[0].Expand();
            }
        }

        // Start section for UPK Manager
        private void button2_Click(object sender, EventArgs e)
        {
            // Restore Mods
            DirectoryInfo dir = new DirectoryInfo(ModPath);

            try {
                foreach (FileInfo fi in dir.GetFiles())
                {
                    fi.Delete();
                }
            }
            catch
            {
                toolStripStatusLabel1.Text = "Error: Unable to restore client!";
            }


            foreach (int i in checkedListBox1.CheckedIndices)
            {
                checkedListBox1.SetItemCheckState(i, CheckState.Unchecked);
            }
            EnabledMods = String.Join(",", checkedListBox1.CheckedItems.Cast<string>().ToArray());
            using (StreamWriter writer = new StreamWriter(".\\mods.db"))
            {
                // Use string interpolation syntax to make code clearer.
                writer.WriteLine(EnabledMods + ",");
            }
            toolStripStatusLabel1.Text = "Client Restored!";
        }


        private void button1_Click(object sender, EventArgs e)
        {
            // Apply Mods
            try
            {
                foreach (var dir in checkedListBox1.CheckedItems.Cast<string>().ToArray())
                {
                    foreach (var file in Directory.GetFiles(@".\\mods\\" + dir + "\\", "*.upk"))
                    {
                        if (File.Exists(Path.Combine(ModPath, Path.GetFileName(file)))) {
                            toolStripStatusLabel1.Text = "Error: Conflicting Mods!";
                            break;
                        }
                        else
                        {
                            File.Copy(file, Path.Combine(ModPath, Path.GetFileName(file)), false);
                        }
                    }
                }
            }
            catch {
                toolStripStatusLabel1.Text = "Error: Unable to apply mods!";
            }

            EnabledMods = String.Join(",", checkedListBox1.CheckedItems.Cast<string>().ToArray());
            using (StreamWriter writer = new StreamWriter(".\\mods.db"))
            {
                // Use string interpolation syntax to make code clearer.
                writer.WriteLine(EnabledMods + ",");
            }
            toolStripStatusLabel1.Text = "Mods Applied!";
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedMod = checkedListBox1.SelectedItem.ToString();
            textBox12.Text = File.ReadAllText(".\\mods\\" + SelectedMod + "\\desc.txt");
            pictureBox2.Image = Image.FromFile(".\\mods\\" + SelectedMod + "\\preview.png");
        }

        // End section for upk manager


        // Start section for Splash Screen replacer
        // Browse for image
        private void button3_Click_1(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Browse for Splash Screen";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Dispose();
                NewSplashID = openFileDialog1.FileName;
                NewSplash = NewSplashID;
                textBox1.Text = NewSplash;
                pictureBox1.Image = Image.FromFile(NewSplash);
                toolStripStatusLabel1.Text = "Splash Screen Loaded!";
            }
        }

        // Apply new image
        private void button4_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Dispose();
            try
            {
                File.Copy(NewSplash, SplashPath + "Splash.bmp", true);
                toolStripStatusLabel1.Text = "Splash Screen Applied!";
            }
            catch
            {
                toolStripStatusLabel1.Text = "Error: Could not replace image!";
            }
            pictureBox1.Image = Image.FromFile(SplashPath + "Splash.bmp");
        }

        // Restore old image
        private void button5_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Dispose();
            try
            {
            File.Copy(SplashPath + "backup\\Splash.bmp", SplashPath + "Splash.bmp", true);
            toolStripStatusLabel1.Text = "Splash Screen Restored!";
            }
            catch
            {
                toolStripStatusLabel1.Text = "Error: Could not replace image!";
            }
            pictureBox1.Image = Image.FromFile(SplashPath + "Splash.bmp");
            }
        // End of Splash Screen replacer

        // Data File Editor

        /*
        Start of Data File Explorer
        */

        // Choose either config.dat or xml.dat to work in.
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem.ToString() == "config.dat")
            {
                // set config.dat as active
                ActiveDataFile = "config.dat";
                treeView1.Nodes.Clear();
                DirectoryInfo directoryInfo = new DirectoryInfo(@".\\data\\config.dat.files\\");
                if (directoryInfo.Exists)
                {
                    treeView1.AfterSelect += treeView1_AfterSelect;
                    BuildTree(directoryInfo, treeView1.Nodes);
                    treeView1.Nodes[0].Expand();
                }
            }
            else
            {
                // set xml.dat as active
                ActiveDataFile = "xml.dat";
                treeView1.Nodes.Clear();
                DirectoryInfo directoryInfo = new DirectoryInfo(@".\\data\\xml.dat.files\\");
                if (directoryInfo.Exists)
                {
                    treeView1.AfterSelect += treeView1_AfterSelect;
                    BuildTree(directoryInfo, treeView1.Nodes);
                    treeView1.Nodes[0].Expand();
                }
            }
        }


        // Compile Data Files
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Compiling data file...";
            if (ActiveDataFile == "config.dat")
            {
                Process bnsdat = new Process();
                bnsdat.StartInfo.UseShellExecute = false;
                bnsdat.StartInfo.CreateNoWindow = true;
                bnsdat.StartInfo.FileName = TempPath + "bnsdat.exe";
                bnsdat.StartInfo.Arguments = "-c \"" + AppPath + "\\data\\config.dat.files\"";
                bnsdat.EnableRaisingEvents = true;
                bnsdat.StartInfo.RedirectStandardOutput = true;
                bnsdat.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);
                bnsdat.Start();
                bnsdat.BeginOutputReadLine();
                while (!bnsdat.HasExited)
                {
                    Application.DoEvents(); // This keeps your form responsive by processing events
                }
            }
            else
            {
                Process bnsdat = new Process();
                bnsdat.StartInfo.UseShellExecute = false;
                bnsdat.StartInfo.CreateNoWindow = true;
                bnsdat.StartInfo.FileName = TempPath + "bnsdat.exe";
                bnsdat.StartInfo.Arguments = "-c \"" + AppPath + "\\data\\xml.dat.files\"";
                bnsdat.EnableRaisingEvents = true;
                bnsdat.StartInfo.RedirectStandardOutput = true;
                bnsdat.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);
                bnsdat.Start();
                bnsdat.BeginOutputReadLine();
                while (!bnsdat.HasExited)
                {
                    Application.DoEvents(); // This keeps your form responsive by processing events
                }
                
            }
        }
        void SortOutputHandler(object sender, DataReceivedEventArgs e)
        {
            Trace.WriteLine(e.Data);
            this.BeginInvoke(new MethodInvoker(() =>
            {
                toolStripStatusLabel1.Text = e.Data ?? "Task complete, please check for errors.";
            }));
        }

        // Apply Data Files
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (ActiveDataFile == "config.dat")
            {
                NewDat = ".\\data\\config.dat";
                try
                {
                    File.Copy(NewDat, DatPath + "config.dat", true);
                    toolStripStatusLabel1.Text = "Patched config.dat";
                }
                catch
                {
                    toolStripStatusLabel1.Text = "Error: Could not apply patch!";
                }
            }
            else
            {
                NewDat = ".\\data\\xml.dat";
                try
                {
                    File.Copy(NewDat, DatPath + "xml.dat", true);
                    toolStripStatusLabel1.Text = "Patched xml.dat";
                }
                catch
                {
                    toolStripStatusLabel1.Text = "Error: Could not apply patch!";
                }
            }
        }
        // Decompile Data Files
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Decompiling data file...";
            if (ActiveDataFile == "config.dat")
            {
                Process bnsdat = new Process();
                bnsdat.StartInfo.UseShellExecute = false;
                bnsdat.StartInfo.CreateNoWindow = true;
                bnsdat.StartInfo.FileName = TempPath + "bnsdat.exe";
                bnsdat.StartInfo.Arguments = "-x \"" + AppPath + "\\data\\config.dat\"";
                bnsdat.EnableRaisingEvents = true;
                bnsdat.StartInfo.RedirectStandardOutput = true;
                bnsdat.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);
                bnsdat.Start();
                bnsdat.BeginOutputReadLine();
                while (!bnsdat.HasExited)
                {
                    Application.DoEvents(); // This keeps your form responsive by processing events
                }
                treeView1.Nodes.Clear();
                DirectoryInfo directoryInfo = new DirectoryInfo(@".\\data\\config.dat.files\\");
                if (directoryInfo.Exists)
                {
                    treeView1.AfterSelect += treeView1_AfterSelect;
                    BuildTree(directoryInfo, treeView1.Nodes);
                    treeView1.Nodes[0].Expand();
                }
            }
            else
            {
                Process bnsdat = new Process();
                bnsdat.StartInfo.UseShellExecute = false;
                bnsdat.StartInfo.CreateNoWindow = true;
                bnsdat.StartInfo.FileName = TempPath + "bnsdat.exe";
                bnsdat.StartInfo.Arguments = "-x \"" + AppPath + "\\data\\xml.dat\"";
                bnsdat.EnableRaisingEvents = true;
                bnsdat.StartInfo.RedirectStandardOutput = true;
                bnsdat.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);
                bnsdat.Start();
                bnsdat.BeginOutputReadLine();
                while (!bnsdat.HasExited)
                {
                    Application.DoEvents(); // This keeps your form responsive by processing events
                }
                treeView1.Nodes.Clear();
                DirectoryInfo directoryInfo = new DirectoryInfo(@".\\data\\xml.dat.files\\");
                if (directoryInfo.Exists)
                {
                    treeView1.AfterSelect += treeView1_AfterSelect;
                    BuildTree(directoryInfo, treeView1.Nodes);
                    treeView1.Nodes[0].Expand();
                }
            }
        }
        // Restore Data Files
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (ActiveDataFile == "config.dat")
            {
                try
                {
                    File.Copy(DatPath + "backup\\config.dat", DatPath + "config.dat", true);
                    toolStripStatusLabel1.Text = "Restored config.dat!";
                }
                catch
                {
                    toolStripStatusLabel1.Text = "Error: Could not restore config.dat!";
                }
            }
            else
            {
                try
                {
                    File.Copy(DatPath + "backup\\xml.dat", DatPath + "xml.dat", true);
                    toolStripStatusLabel1.Text = "Restored xml.dat!";
                }
                catch
                {
                    toolStripStatusLabel1.Text = "Error: Could not restore xml.dat!";
                }
            }
        }
        // Update Data Files From Game
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            try {
                // Backup the data files
                    File.Copy(DatPath + "config.dat", DatPath + "backup\\config.dat", true);
                    File.Copy(DatPath + "xml.dat", DatPath + "backup\\xml.dat", true);
                    File.Copy(DatPath + "config.dat", AppPath + "\\data\\config.dat", true);
                    File.Copy(DatPath + "xml.dat", AppPath + "\\data\\xml.dat", true);
                    toolStripStatusLabel1.Text = "Updated Data Files";
            }
            catch { toolStripStatusLabel1.Text = "Error: Upable to update Data Files!"; }
        }

        // Select a file to edit
        private void BuildTree(DirectoryInfo directoryInfo, TreeNodeCollection addInMe)
        {
            TreeNode curNode = addInMe.Add(directoryInfo.Name);

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                curNode.Nodes.Add(file.FullName, file.Name);
            }
            foreach (DirectoryInfo subdir in directoryInfo.GetDirectories())
            {
                BuildTree(subdir, curNode.Nodes);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try {
                if (e.Node.Name.EndsWith(".xml"))
                {
                    this.fastColoredTextBox1.Clear();
                    StreamReader reader = new StreamReader(e.Node.Name);
                    this.fastColoredTextBox1.Text = reader.ReadToEnd();
                    reader.Close();
                    groupBox10.Text = "Currently Editing: " + treeView1.SelectedNode.Text;
                }
            }
            catch { toolStripStatusLabel1.Text = "Error: Unable to open file!";  }
        }
        /*
        End of Data File Explorer
        */

        /*
        Start of Code Editor
        */
        private void fctb_TextChanged(object sender, TextChangedEventArgs e)
        {
            //highlight only visible area of text
            HTMLSyntaxHighlight(fastColoredTextBox1.VisibleRange);
        }
        private void fctb_VisibilityChanged(object sender, EventArgs e)
        {
            //highlight only visible area of text
            HTMLSyntaxHighlight(fastColoredTextBox1.VisibleRange);
        }
        private void HTMLSyntaxHighlight(Range range)
        {
            //clear style of changed range
            range.ClearStyle(BlueStyle, MaroonStyle, RedStyle, GreenStyle);
            //tag brackets highlighting
            range.SetStyle(MaroonStyle, @"<|/>|</|>");
            //tag name
            range.SetStyle(MaroonStyle, @"<(?<range>[!\w]+)");
            //end of tag
            range.SetStyle(MaroonStyle, @"</(?<range>\w+)>");
            //attributes
            range.SetStyle(RedStyle, @"(?<range>\S+?)='[^']*'|(?<range>\S+)=""[^""]*""|(?<range>\S+)=\S+");
            //attribute values
            range.SetStyle(BlueStyle, @"\S+?=(?<range>'[^']*')|\S+=(?<range>""[^""]*"")|\S+=(?<range>\S+)");
            //comment values
            range.SetStyle(GreenStyle, @"<!--(?<range>\S+?)-->");
        }

        // Find Text in Editor
        private void findCrtlFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fastColoredTextBox1.ShowFindDialog();
        }
        // Replace Text in Editor
        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fastColoredTextBox1.ShowReplaceDialog();
        }
        // Save the open file in the editor
        private void button8_Click(object sender, EventArgs e)
        {
            try {
                XmlSavePath = ".\\data\\" + treeView1.SelectedNode.FullPath.ToString();
            }
            catch { toolStripStatusLabel1.Text = "Error: No file open in editor!"; }
            try
            {
                File.WriteAllText(XmlSavePath, fastColoredTextBox1.Text);
                toolStripStatusLabel1.Text = "Saved file!";
            }
            catch
            {
                toolStripStatusLabel1.Text = "Error: Failed to save file!";
            }
        }
        /*
        End of Code Editor
        */
    }
}
