using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Forms;

/*
This is a simple collection of functions I've found useful.
Particulary for about menus, keygens, patchers, trainers, etc...
This file will often be updated as I learn modern dotnet.

Functions:

    Miyako.Common.ShowError(string msg);    // Generic error message box

    Miyako.Music    // Class to interact with ufmod
    Miyako.Music.playSong(string resourceName, int volume);
                                            // Plays a xm file from resources (only) at volume (0-25)
    Miyako.Music.stopSong();                // Stops playback and clears memory
    Miyako.Music.pauseSong();               // Toggles playback between paused and playing
    Miyako.Music.setVolume(int volume);     // Sets the volume (0-25)
    Miyako.Music.uFMOD_Call(string methodName, params object[] args);
                                            // Directly call ufmod functions
        // uFMOD_PlaySong
        // uFMOD_StopSong
        // uFMOD_Jump2Pattern
        // uFMOD_Rewind
        // uFMOD_Pause
        // uFMOD_Resume
        // uFMOD_GetStats;
        // uFMOD_GetRowOrder
        // uFMOD_GetTime
        // uFMOD_GetTitle
        // uFMOD_SetVolume

    Miyako.ResDll   // Class to load unmanaged DLLs from resources
    Miyako.ResDll.ExtractDll(string dllName, byte[] resourceBytes);
                                            // Extracts a DLL from resources to a temp folder
    Miyako.ResDll.LoadDll(string dllName);  // Loads the DLL into memory for use by a program
*/

namespace Miyako
{
    public class Common
    {
        // Miyako.Common.ShowError(string msg);
        // Generic error message box
        public static void ShowError(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public class Music
    {
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

        static bool isPaused = false;

        #endregion

        // Miyako.Music.uFMOD_Call(string methodName, params object[] args);
        // Directly call ufmod functions
        private static object uFMOD_Call(string methodName, params object[] args)
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
                Miyako.Common.ShowError(ex.InnerException.Message);
                return null;

            }
            catch (TypeInitializationException ex)
            {
                Miyako.Common.ShowError(ex.InnerException.Message);
                return null;
            }
            catch (DllNotFoundException ex)
            {
                Miyako.Common.ShowError(ex.Message);
                return null;
            }
        }

        // Miyako.Music.playSong(string resourceName, int volume);
        // Plays a xm file from resources (only) at volume (0-25)
        public static unsafe void playSong(string resourceName, int volume)
        {
            uFMOD_Call(uFMOD_SetVolume, volume);
            using (UnmanagedMemoryStream memoryStream = (UnmanagedMemoryStream)
                Assembly.GetEntryAssembly().GetManifestResourceStream(resourceName))
            {
                if (memoryStream == null)
                {
                    Miyako.Common.ShowError("Unable to load resource.");
                    return;
                }

                object uFMOD_Result = uFMOD_Call(uFMOD_PlaySong,
                    (IntPtr)memoryStream.PositionPointer, (int)memoryStream.Length, ufmod.uFMOD_Flags.XM_MEMORY);

                if ((uFMOD_Result == null) || (IntPtr)uFMOD_Result == IntPtr.Zero)
                {
                    Miyako.Common.ShowError("Unable to play memory buffer.");
                    return;
                }

            }
        }

        // Miyako.Music.stopSong();
        // Stops playback and clears memory
        public static void stopSong()
        {
            uFMOD_Call(uFMOD_StopSong);
        }

        // Miyako.Music.pauseSong();
        // Toggles playback between paused and playing
        public static void pauseSong()
        {
            // Pause the song
            if (isPaused == false)
            {
                uFMOD_Call(uFMOD_Pause);
                isPaused = true;
            }
            // Resume the song
            else
            {
                uFMOD_Call(uFMOD_Resume);
                isPaused = false;
            }
        }

        // Miyako.Music.setVolume(int volume);
        // Sets the volume (0-25)
        public static void setVolume(int volume)
        {
            uFMOD_Call(uFMOD_SetVolume, volume);
        }
    }

    // Use unmanaged dll's as resources.
    public class ResDll
    {
        private static string tempFolder = "";

        // Miyako.ResDll.ExtractDll(string dllName, byte[] resourceBytes);
        // Extracts a DLL from resources to a temp folder
        public static void ExtractDll(string dllName, byte[] resourceBytes)
        {
            Assembly assem = Assembly.GetExecutingAssembly();
            string[] names = assem.GetManifestResourceNames();
            AssemblyName an = assem.GetName();

            // If multiple copies are found, identify uniquely
            tempFolder = String.Format("{0}.{1}.{2}", an.Name, an.ProcessorArchitecture, an.Version);

            string dirName = Path.Combine(Path.GetTempPath(), tempFolder);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            // Add our temp folder to %PATH%
            string path = Environment.GetEnvironmentVariable("PATH");
            string[] pathPieces = path.Split(';');
            bool found = false;
            foreach (string pathPiece in pathPieces)
            {
                if (pathPiece == dirName)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Environment.SetEnvironmentVariable("PATH", dirName + ";" + path);
            }

            // Don't overwrite an existing copy
            string dllPath = Path.Combine(dirName, dllName);
            bool rewrite = true;
            if (File.Exists(dllPath))
            {
                byte[] existing = File.ReadAllBytes(dllPath);
                if (resourceBytes.SequenceEqual(existing))
                {
                    rewrite = false;
                }
            }
            if (rewrite)
            {
                File.WriteAllBytes(dllPath, resourceBytes);
            }
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string lpFileName);

        // Miyako.ResDll.LoadDll(string dllName);
        // Loads the DLL into memory for use by a program
        static public void LoadDll(string dllName)
        {
            if (tempFolder == "")
            {
                throw new Exception("Please call ExtractDll before LoadDll");
            }
            IntPtr h = LoadLibrary(dllName);
            if (h == IntPtr.Zero)
            {
                Exception e = new Win32Exception();
                throw new DllNotFoundException("Unable to load library: " + dllName + " from " + tempFolder, e);
            }
        }

    }
}