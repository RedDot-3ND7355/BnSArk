using System;
using System.Runtime.InteropServices;

namespace ufmod
{
    public class uFMOD_PlatformChecker
    {
        public uFMOD_PlatformChecker()
        {
            if (IntPtr.Size != 4) // e.g., non 32-bit system or non WoW-process
                throw new NotSupportedException("uFMOD supports only 32-bit mode.");
        }

    }

    public enum uFMOD_Flags
    {
        XM_RESOURCE = 0,
        XM_MEMORY = 1,
        XM_FILE = 2,
        XM_NOLOOP = 8,
        XM_SUSPENDED = 16
    }

    public enum uFMOD_Volume
    {
        uFMOD_MIN_VOL = 0,
        uFMOD_MAX_VOL = 25,
        uFMOD_DEFAULT_VOL = 25
    }

    public static class uFMOD
    {
        private static readonly uFMOD_PlatformChecker PlatformChecker = new uFMOD_PlatformChecker();

        private const string uFMOD_lib = "ufmod.dll";

        /// <summary>
        /// Loads the given XM song and starts playing it immediately,
        /// unless XM_SUSPENDED is specified. It will stop any currently
        /// playing song before loading the new one.
        /// Remarks:  
        /// If no valid song is specified and there is one currently being
        /// played, uFMOD_PlaySong just stops playback.
        /// </summary>
        /// <param name="lpXM">
        /// Specifies the song to play. If this parameter is 0, any
        /// currently playing song is stopped. In such a case, function
        /// does not return a meaningful value. fdwSong parameter
        /// determines whether this value is interpreted as a filename,
        /// as a resource identifier or a pointer to an image of the song in memory.
        /// </param>
        /// <param name="param">
        /// If XM_RESOURCE is specified, this parameter should be the
        /// handle to the executable file that contains the resource to
        /// be loaded. A 0 value refers to the executable module itself.
        /// If XM_MEMORY is specified, this parameter should be the size
        /// of the image of the song in memory.
        /// If XM_FILE is specified, this parameter is ignored.
        /// </param>
        /// <param name="fdwSong">
        /// Flags for playing the song. The following values are defined:
        /// XM_FILE lpXM points to filename. param is ignored.
        /// XM_MEMORY lpXM points to an image of a song in memory.
        /// param is the image size. Once, uFMOD_PlaySong
        /// returns, it's safe to free/discard the memory buffer.
        /// XM_RESOURCE lpXM specifies the name of the resource.
        /// param identifies the module whose executable file
        /// contains the resource.
        /// The resource type must be RT_RCDATA.
        /// XM_NOLOOP An XM track plays repeatedly by default. Specify
        /// this flag to play it only once.
        /// XM_SUSPENDED The XM track is loaded in a suspended state,
        /// and will not play until the uFMOD_Resume function
        /// is called. This is useful for preloading a song
        /// or testing an XM track for validity.
        /// </param>
        /// <returns>
        /// On success, returns a pointer to an open WINMM output device handle.
        /// Returns nil on failure. If you are familiar with WINMM, you'll know
        /// hat this handle might be useful for :)
        /// </returns>
        [DllImport(uFMOD_lib, EntryPoint = "uFMOD_PlaySong")]
        public static extern IntPtr PlaySong(IntPtr lpXM, int param, uFMOD_Flags fdwSong);

        /// <summary>
        /// Stops the currently playing song.
        /// </summary>
        public static void StopSong()
        {
            PlaySong(IntPtr.Zero, 0, 0);
        }

        /// <summary>
        /// Jumps to the specified pattern index.
        /// Remarks:
        /// uFMOD doesn't automatically perform Note Off effects before jumping
        /// to the target pattern. In other words, the original pattern will
        /// remain in the mixer until it fades out. You can use this feature to
        /// your advantage. If you don't like it, just insert leading Note Off
        /// commands in all patterns intended to be used as uFMOD_Jump2Pattern targets.
        /// if the pattern index lays outside of the bounds of the pattern order
        /// table, calling this function jumps to pattern 0, effectively
        /// rewinding playback.
        /// </summary>
        /// <param name="pat">Next zero based pattern index.</param>
        [DllImport(uFMOD_lib, EntryPoint = "uFMOD_Jump2Pattern")]
        public static extern void Jump2Pattern(int pat);

        /// <summary>
        /// 
        /// </summary>
        public static void Rewind()
        {
            Jump2Pattern(0);
        }

        /// <summary>
        /// Pauses the currently playing song, if any.
        /// Remarks:
        /// While paused you can still control the volume (uFMOD_SetVolume) and
        /// the pattern order (uFMOD_Jump2Pattern). The RMS volume coefficients
        /// (uFMOD_GetStats) will go down to 0 and the progress tracker
        /// (uFMOD_GetTime) will "freeze" while the song is paused.
        /// uFMOD_Pause doesn't perform the request immediately. Instead, it
        /// signals to pause when playback reaches next chunk of data, which may
        /// take up to about 40ms. This way, uFMOD_Pause performs asynchronously
        /// and returns very fast. It is not cumulative. So, calling
        /// uFMOD_Pause many times in a row has the same effect as calling it once.
        /// If you need synchronous pause/resuming, you can use WINMM
        /// waveOutPause/waveOutRestart functions.
        /// </summary>
        [DllImport(uFMOD_lib, EntryPoint = "uFMOD_Pause")]
        public static extern void Pause();

        /// <summary>
        /// Resumes the currently paused song, if any.
        /// Remarks:
        /// uFMOD_Resume doesn't perform the request immediately. Instead, it
        /// signals to resume when an internal thread gets a time slice, which
        /// may take some milliseconds to happen. Usually, calling Sleep(0)
        /// immediately after uFMOD_Resume causes it to resume faster.
        /// uFMOD_Resume is not cumulative. So, calling it many times in a row
        /// has the same effect as calling it once.
        /// If you need synchronous pause/resuming, you can use WINMM
        /// waveOutPause/waveOutRestart functions.
        /// </summary>
        [DllImport(uFMOD_lib, EntryPoint = "uFMOD_Resume")]
        public static extern void Resume();

        /// <summary>
        /// Remarks:
        /// This function is useful for updating a VU meter. It's recommended
        /// to rescale the output to log10 (decibels or dB for short), because
        /// human ears track volume changes in a dB scale. You may call
        /// uFMOD_GetStats() as often as you like, but take in mind that uFMOD
        /// updates both channel RMS volumes every 20-40ms, depending on the
        /// output sampling rate. So, calling uFMOD_GetStats about 16 times a
        /// second whould be quite enough to track volume changes very closely.
        /// </summary>
        /// <returns>
        /// Returns the current RMS volume coefficients in (L)eft and (R)ight channels.
        /// low-order word: RMS volume in R channel
        /// hi-order word: RMS volume in L channel
        /// Range from 0 (silence) to $7FFF (maximum) on each channel.
        /// </returns>
        [DllImport(uFMOD_lib, EntryPoint = "uFMOD_GetStats")]
        public static extern int GetStats();

        /// <summary>
        /// Remarks:
        /// This function is useful for synchronization. uFMOD updates both
        /// row and order values every 20-40ms, depending on the output sampling
        /// rate. So, calling uFMOD_GetRowOrder about 16 times a second whould be
        /// quite enough to track row and order progress very closely.
        /// </summary>
        /// <returns>
        /// Returns the currently playing row and order.
        /// low-order word: row
        /// hi-order word: order
        /// </returns>
        [DllImport(uFMOD_lib, EntryPoint = "uFMOD_GetRowOrder")]
        public static extern int GetRowOrder();

        /// <summary>
        /// Remarks:
        /// This function is useful for synchronizing purposes. In fact, it is
        /// more precise than a regular timer in Win32. Multimedia applications
        /// can use uFMOD_GetTime to synchronize GFX to sound, for example. An
        /// XM player can use this function to update a progress meter.
        /// </summary>
        /// <returns>
        /// Returns the time in milliseconds since the song was started.
        /// </returns>
        [DllImport(uFMOD_lib, EntryPoint = "uFMOD_GetTime")]
        public static extern int GetTime();

        /// <summary>
        /// Remarks:
        /// Not every song has a title, so be prepared to get an empty string.
        /// The string format may be ANSI or Unicode debending on the UF_UFS
        /// settings used while recompiling the library.
        /// </summary>
        /// <returns>
        /// Returns the current song's title.
        /// </returns>
        [DllImport(uFMOD_lib, EntryPoint = "uFMOD_GetTitle", CharSet = CharSet.Unicode)]
        public static extern string GetTitle();

        /// <summary>
        /// Sets the global volume. The volume scale is linear.
        /// Remarks:
        /// uFMOD internally converts the given values to a logarithmic scale (dB).
        /// Maximum volume is set by default. The volume value is preserved across
        /// uFMOD_PlaySong calls. You can set the desired volume level before
        /// actually starting to play a song.
        /// You can use WINMM waveOutSetVolume function to control the L and R
        /// channels volumes separately. It also has a wider range than
        /// uFMOD_SetVolume, sometimes allowing to amplify the sound volume as well,
        /// as opposed to uFMOD_SetVolume only being able to attenuate it. The bad
        /// things about waveOutSetVolume is that it may produce clicks and it's
        /// hardware dependent.
        /// </summary>
        /// <param name="vol">
        /// New volume. Range: from uFMOD_MIN_VOL (muting) to uFMOD_MAX_VOL
        /// (maximum volume). Any value above uFMOD_MAX_VOL maps to maximum volume.
        /// </param>
        [DllImport(uFMOD_lib, EntryPoint = "uFMOD_SetVolume")]
        public static extern void SetVolume(uFMOD_Volume vol);
    }
}