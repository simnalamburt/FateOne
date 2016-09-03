// FateOne, The alternative of ChaosOne just for Fate/Another
// Copyright (C) 2012-2016  Hyeon Kim
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

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
