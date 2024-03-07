using System;
using System.Runtime.InteropServices;

namespace ww.Utilities
{
    public static class RemoteDesktop
    {
        /// <summary>
        /// Gets the name of the client system.
        /// </summary>
        public static string GetClientMachineName()
        {
            IntPtr buffer = IntPtr.Zero;
            string clientName = null;

            if (NativeMethods.WTSQuerySessionInformation(
                NativeMethods.WTS_CURRENT_SERVER_HANDLE,
                NativeMethods.WTS_CURRENT_SESSION,
                NativeMethods.WTS_INFO_CLASS.WTSClientName,
                out buffer,
                out int bytesReturned))
            {
                clientName = Marshal.PtrToStringUni(
                    buffer,
                    bytesReturned / 2 /* Because the DllImport uses CharSet.Unicode */
                    );
                NativeMethods.WTSFreeMemory(buffer);
                clientName = clientName.Replace("\0", "");
            }
            return clientName;
        }


        private static class NativeMethods
        {
            public static readonly IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;
            public const int WTS_CURRENT_SESSION = -1;

            public enum WTS_INFO_CLASS
            {
                WTSClientName = 10
            }

            [DllImport("Wtsapi32.dll", CharSet = CharSet.Unicode)]
            public static extern bool WTSQuerySessionInformation(
                IntPtr hServer,
                Int32 sessionId,
                WTS_INFO_CLASS wtsInfoClass,
                out IntPtr ppBuffer,
                out Int32 pBytesReturned);

            /// <summary>
            /// The WTSFreeMemory function frees memory allocated by a Terminal
            /// Services function.
            /// </summary>
            /// <param name="memory">Pointer to the memory to free.</param>
            [DllImport("wtsapi32.dll", ExactSpelling = true, SetLastError = false)]
            public static extern void WTSFreeMemory(IntPtr memory);
        }
    }
}
