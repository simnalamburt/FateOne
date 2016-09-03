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
