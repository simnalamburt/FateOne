using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace FateOne
{
    /// <summary>
    /// 윈도우 핸들로부터 클라이언트 영역 직사각형을 가져옵니다.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct windowRect
    {
        #region Win32 API 상호운용
        const int GWL_STYLE = -16;
        const int GWL_EXSTYLE = -20;
        #endregion
        
        public int left;
        public int top;
        public int right;
        public int bottom;

        #region 생성자 정의
        public windowRect(IntPtr hWnd)
        {
            NativeMethods.GetWindowRect(hWnd, out this);
            windowRect delta = new windowRect();
            NativeMethods.AdjustWindowRectEx(ref delta, NativeMethods.GetWindowLong(hWnd, GWL_STYLE), false, NativeMethods.GetWindowLong(hWnd, GWL_EXSTYLE));
            left -= delta.left;
            right -= delta.right + 1;
            top -= delta.top;
            bottom -= delta.bottom + 1;
        } 
        #endregion
    }

    /// <summary>
    /// 마우스의 화면 상 2차원 좌표를 표시합니다.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct position
    {
        public int x;
        public int y;
    }
}
