﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ww.Utilities
{
    public static class OpenWindowGetter
    {
        /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
        /// https://stackoverflow.com/questions/7268302/get-the-titles-of-all-open-windows
        /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
        public static IDictionary<IntPtr, string> GetOpenWindows()
        {
            IntPtr shellWindow = GetShellWindow();
            var windows = new Dictionary<IntPtr, string>();

            EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                var builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);

                windows[hWnd] = builder.ToString();
                return true;
            }, 0);

            return windows;
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
