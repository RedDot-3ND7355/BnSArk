using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;

namespace BnSLauncher
{
    public partial class FormAbout : Form
    {
        bool blinked = false;

        public FormAbout()
        {
            InitializeComponent();
        }

        private void ShowError(string msg)
        {
            MessageBox.Show(this, msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        #region uFMOD_xxx

        private const string uFMOD_PlaySong = "PlaySong";
        private const string uFMOD_StopSong = "StopSong";
        private const string uFMOD_Jump2Pattern = "Jump2Pattern";
        private const string uFMOD_Rewind = "Rewind";
        private const string uFMOD_Pause = "Pause";
        private const string uFMOD_Resume = "Resume";
        private const string uFMOD_GetStats = "GetStats";
        private const string uFMOD_GetRowOrder = "GetRowOrder";
        private const string uFMOD_GetTime = "GetTime";
        private const string uFMOD_GetTitle = "GetTitle";
        private const string uFMOD_SetVolume = "SetVolume";

        #endregion

        private object uFMOD_Call(string methodName, params object[] args)
        {
            try
            {
                Type ufmodType = Type.GetType("ufmod.uFMOD");
                MethodInfo ufmodFunc = ufmodType.GetMethod(methodName);
                if (ufmodFunc == null)
                    throw new NotSupportedException("The requested method is not supported.");
                return ufmodFunc.Invoke(null, args);
            }
            catch (TargetInvocationException ex)
            {
                ShowError(ex.InnerException.Message);
                return null;

            }
            catch (TypeInitializationException ex)
            {
                ShowError(ex.InnerException.Message);
                return null;
            }
            catch (DllNotFoundException ex)
            {
                ShowError(ex.Message);
                return null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            // stop audio
            uFMOD_Call(uFMOD_StopSong);
        }

        private unsafe void FormAbout_Load(object sender, EventArgs e)
        {
            // play audio
            using (UnmanagedMemoryStream memoryStream = (UnmanagedMemoryStream)
    Assembly.GetEntryAssembly().GetManifestResourceStream("BnSLauncher.xmfile.xm"))
            {
                if (memoryStream == null)
                {
                    ShowError("Unable to load resource.");
                    return;
                }

                object uFMOD_Result = uFMOD_Call(uFMOD_PlaySong,
                    (IntPtr)memoryStream.PositionPointer, (int)memoryStream.Length, ufmod.uFMOD_Flags.XM_MEMORY);

                if ((uFMOD_Result == null) || (IntPtr)uFMOD_Result == IntPtr.Zero)
                {
                    ShowError("Unable to play memory buffer.");
                    return;
                }
                
            }
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
            System.Diagnostics.Process.Start("http://miyakoproductions.tumblr.com");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://sns.qsoftstudios.com");
        }
    }
}
