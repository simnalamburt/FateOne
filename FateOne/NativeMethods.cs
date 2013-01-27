using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace FateOne
{
    /// <summary>
    /// Win32 API들을 노출합니다.
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hModule);


        [DllImport("user32.dll", EntryPoint = "SetWindowsHookEx")]
        internal static extern IntPtr Hook(int idHook, llInputManager.keyboardHookCallback callback, IntPtr hInstance, uint threadId);
        [DllImport("user32.dll", EntryPoint = "SetWindowsHookEx")]
        internal static extern IntPtr Hook(int idHook, llInputManager.mouseHookCallback callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll", EntryPoint = "CallNextHookEx")]
        internal static extern IntPtr CallNextHook(IntPtr idHook, int nCode, IntPtr wParam, ref llInputManager.keyboardHookStruct lParam);
        [DllImport("user32.dll", EntryPoint = "CallNextHookEx")]
        internal static extern IntPtr CallNextHook(IntPtr idHook, int nCode, IntPtr wParam, ref llInputManager.mouseHookStruct lParam);

        [DllImport("user32.dll", EntryPoint = "UnhookWindowsHookEx")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool Unhook(IntPtr hInstance);


        [DllImport("user32.dll")]
        internal static extern int SendInput(int nInputs, [MarshalAs(UnmanagedType.LPArray), In]Input[] pInputs, int cbSize);


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out position point);


        [DllImport("user32.dll")]
        internal static extern int MapVirtualKey(int code, int mapType);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out windowRect rect);

        [DllImport("User32.dll")]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AdjustWindowRectEx(ref windowRect rect, int dwStyle, [MarshalAs(UnmanagedType.Bool)]bool bMenu, int dwExStyle);
    }
}
