using System;
using System.Runtime.InteropServices;

namespace SimpleFFmpegGUI
{
    public static class MediaInfoModule
    {
        private const string DllName = "MediaInfo.DLL";

        [DllImport(DllName)]
        internal static extern IntPtr MediaInfo_Open(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string FileName);

        [DllImport(DllName)]
        private static extern IntPtr MediaInfo_Inform(IntPtr Handle, UIntPtr Reserved);

        [DllImport(DllName)]
        private static extern IntPtr MediaInfo_Option(IntPtr Handle, string option, string Value);

        [DllImport(DllName)]
        private static extern IntPtr MediaInfo_New();

        [DllImport(DllName)]
        private static extern void MediaInfo_Delete(IntPtr Handle);

        public static string GetInfo(string path)
        {
            var handle = MediaInfo_New();
            MediaInfo_Open(handle, path);
            MediaInfo_Option(handle, "Complete", "");
            string result = Marshal.PtrToStringUni(MediaInfo_Inform(handle, (UIntPtr)0)).Trim();
            MediaInfo_Delete(handle);
            return result;
        }
    }
}