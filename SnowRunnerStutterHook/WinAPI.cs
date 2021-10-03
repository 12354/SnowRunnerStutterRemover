using System;
using System.Runtime.InteropServices;

namespace SnowRunnerStutterHook
{
    class WinAPI
    {
        public const int GWL_WNDPROC = -4;
        public const int WM_DEVICECHANGE = 0x0219;

        [DllImport("user32.dll")]
        public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll",CharSet = CharSet.Unicode)]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern int RegOpenKeyEx(UIntPtr hKey, string subKey, int ulOptions, int samDesired, out UIntPtr hkResult);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate int RegOpenKeyExDelegate(UIntPtr hKey, string subKey, int ulOptions, int samDesired, out UIntPtr hkResult);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr WndProcDelegate(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        
    }
}
