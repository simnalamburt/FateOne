using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace FateOne
{
    /// <summary>
    /// 저수준 입력 시스템을 관리합니다.
    /// 저수준 키·마우스 이벤트 후킹, 키·마우스 이벤트 발생 등의 기능을 제공합니다.
    /// </summary>
    public class llInputManager : IDisposable
    {
        #region Win32 API 상호운용
        const int WH_KEYBOARD_LL = 13;
        const int WH_MOUSE_LL = 14;

        readonly IntPtr WM_KEYDOWN = new IntPtr(0x100);
        readonly IntPtr WM_KEYUP = new IntPtr(0x101);
        readonly IntPtr WM_SYSKEYDOWN = new IntPtr(0x104);
        readonly IntPtr WM_SYSKEYUP = new IntPtr(0x105);

        const int LLKHF_INJECTED = 0x10;
        #endregion

        

        // public static
        #region ProcessInput, ProcessKey, ProcessMouse 정적 메서드 정의
        public static void ProcessInput(params Input[] inputs)
        {
            NativeMethods.SendInput(inputs.Length, inputs, Input.Size);
        }
        public static void ProcessKey(Key key)
        {
            NativeMethods.SendInput(2, new[] { new Input(key, InputState.Down), new Input(key, InputState.Up), }, Input.Size);
        }
        public static void ProcessMouse(MouseButton button)
        {
            NativeMethods.SendInput(2, new[] { new Input(button, InputState.Down), new Input(button, InputState.Up), }, Input.Size);
        }
        #endregion
        #region MousePosition 정적 프로퍼티 정의
        /// <summary>
        /// 현재 마우스의 위치를 가져오거나, 설정합니다.
        /// </summary>
        public static position MousePosition
        {
            get
            {
                position ret;
                NativeMethods.GetCursorPos(out ret);
                return ret;
            }
            set
            {
                NativeMethods.SetCursorPos(value.x, value.y);
            }
        }
        #endregion
        // internal static
        #region keyboardHookStruct, mouseHookStruct 구조체 정의
        internal struct keyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }
        internal struct mouseHookStruct
        {
            public position point;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }
        #endregion
        #region keyboardHookCallback, mouseHookCallback 대리자 정의
        internal delegate IntPtr keyboardHookCallback(int code, IntPtr wParam, ref keyboardHookStruct lParam);
        internal delegate IntPtr mouseHookCallback(int code, IntPtr wParam, ref mouseHookStruct lParam);
        #endregion



        // public
        #region Hooked 프로퍼티 정의
        /// <summary>
        /// true로 설정할경우 키보드와 마우스 후킹이 시작되고, false로 설정할경우 후킹이 해제됩니다.
        /// </summary>
        public bool Hooked
        {
            set
            {
                if (disposed) throw new ObjectDisposedException("llInputManager was disposed.");

                if (value != hooked)
                {
                    hooked = value;
                    if (value)
                    {
                        hDLL = NativeMethods.LoadLibrary("User32");
                        keyboardHookProcInstance = keyboardHookProc;
                        keyboardHook = NativeMethods.Hook(WH_KEYBOARD_LL, keyboardHookProcInstance, hDLL, 0);
                        mouseHookProcInstance = mouseHookProc;
                        mouseHook = NativeMethods.Hook(WH_MOUSE_LL, mouseHookProcInstance, hDLL, 0);
                    }
                    else
                    {
                        NativeMethods.Unhook(keyboardHook);
                        NativeMethods.Unhook(mouseHook);
                        NativeMethods.FreeLibrary(hDLL);
                    }
                }
            }
            get
            {
                if (disposed) throw new ObjectDisposedException("llInputManager was disposed.");
                return hooked;
            }
        }
        bool hooked = false;

        IntPtr hDLL, keyboardHook, mouseHook;
        keyboardHookCallback keyboardHookProcInstance;
        mouseHookCallback mouseHookProcInstance;
        #endregion
        #region KeyDown, KeyUp 이벤트 노출
        /// <summary>
        /// 키가 눌려졌을 때 발생합니다. Hooked가 false로 설정되어있을 경우 작동하지 않습니다.
        /// </summary>
        public event EventHandler<llKeyEventArgs> KeyDown = delegate { };

        /// <summary>
        /// 키가 놓여졌을 때 발생합니다. Hooked가 false로 설정되어있을 경우 작동하지 않습니다.
        /// </summary>
        public event EventHandler<llKeyEventArgs> KeyUp = delegate { };
        #endregion
        #region MouseMove 이벤트 노출
        /// <summary>
        /// 마우스가 움직였을 때 발생합니다. Hooked가 false로 설정되어있을 경우 작동하지 않습니다.
        /// </summary>
        public event EventHandler<llMouseEventArgs> MouseMove = delegate { };
        #endregion
        // private
        #region keyboardHookProc, mouseHookProc 콜백 구현
        IntPtr keyboardHookProc(int code, IntPtr wParam, ref keyboardHookStruct lParam)
        {
            if (code >= 0)
            {
                if (0 == (lParam.flags & LLKHF_INJECTED))
                {
                    llKeyEventArgs kea = new llKeyEventArgs(KeyInterop.KeyFromVirtualKey(lParam.vkCode));
                    if (wParam == (IntPtr)WM_KEYDOWN || wParam == WM_SYSKEYDOWN) KeyDown(this, kea);
                    else if (wParam == WM_KEYUP || wParam == WM_SYSKEYUP) KeyUp(this, kea);
                    if (kea.Handled) return (IntPtr)1;
                }
                else
                {
                    lParam.flags ^= LLKHF_INJECTED;
                }
            }

            return NativeMethods.CallNextHook(keyboardHook, code, wParam, ref lParam);
        }
        IntPtr mouseHookProc(int code, IntPtr wParam, ref mouseHookStruct lParam)
        {
            if (code >= 0)
            {
                if (0 == (lParam.flags & LLKHF_INJECTED))
                {
                    llMouseEventArgs mea = new llMouseEventArgs(lParam.point);
                    MouseMove(this, mea);
                    if (mea.Handled) return (IntPtr)1;
                }
                else
                {
                    lParam.flags ^= LLKHF_INJECTED;
                }
            }
            return NativeMethods.CallNextHook(mouseHook, code, wParam, ref lParam);
        }
        #endregion



        #region Dispose 정의
        bool disposed = false;

        /// <summary>
        /// FateOne.llInputManager에서 사용하는 모든 리소스를 해제합니다.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                // 관리되지 않는 리소스 정리
                Hooked = false;

                disposed = true;
            }
        }
        #endregion
        #region 종료자
        ~llInputManager()
        {
            Dispose(false);
        }
        #endregion
    }
    
    /// <summary>
    /// 발생시킬 입력에 대한 명세입니다.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Input
    {
        #region Win32 API 상호운용
        const int VK_SHIFT = 0x10;
        const int VK_CONTROL = 0x11;
        const int VK_MENU = 0x12;

        const int MAPVK_VK_TO_VSC = 0;
        #endregion
        #region 내부 타입 정의
        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)]
            public MouseInput mi;
            [FieldOffset(0)]
            public KeyboardInput ki;
            [FieldOffset(0)]
            public HardwareInput hi;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct MouseInput
        {
            public int dx;
            public int dy;
            public MouseData mouseData;
            public MouseEventFlag dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct KeyboardInput
        {
            public short wVk;
            public short wScan;
            public KeyEventFlag dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct HardwareInput
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        enum InputType : int { Mouse = 0, Keyboard = 1, Hardware = 2 }
        enum MouseData : int { None = 0, XButton1 = 1, XButton2 = 2 }
        enum MouseEventFlag : int
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            HWheel = 0x1000,
            MoveNoCoalesce = 0x2000,
            VirtualDesk = 0x4000,
            Absolut = 0x8000
        }
        enum KeyEventFlag : int
        {
            ExtendedKey = 1,
            KeyDown = 0,
            KeyUp = 2,
            ScanCode = 8,
            Unicode = 4
        }
        #endregion

        InputType type;
        InputUnion u;

        public static int Size { get { return Marshal.SizeOf(typeof(Input)); } }



        /// <summary>
        /// 마우스 이벤트 명세를 생성합니다.
        /// </summary>
        /// <param name="button">이벤트를 발생시킬 System.Windows.Input.MouseButton 입니다.</param>
        /// <param name="state">버튼을 누르는 이벤트인지, 떼는 이벤트인지 결정합니다.</param>
        public Input(MouseButton button, InputState state)
        {
            type = InputType.Mouse;
            u = new InputUnion();
            switch (button)
            {
                case MouseButton.Left:
                    u.mi.dwFlags = MouseEventFlag.LeftDown;
                    u.mi.mouseData = MouseData.None;
                    break;
                case MouseButton.Middle:
                    u.mi.dwFlags = MouseEventFlag.MiddleDown;
                    u.mi.mouseData = MouseData.None;
                    break;
                case MouseButton.Right:
                    u.mi.dwFlags = MouseEventFlag.RightDown;
                    u.mi.mouseData = MouseData.None;
                    break;
                case MouseButton.XButton1:
                    u.mi.dwFlags = MouseEventFlag.XDown;
                    u.mi.mouseData = MouseData.XButton1;
                    break;
                case MouseButton.XButton2:
                    u.mi.dwFlags = MouseEventFlag.XDown;
                    u.mi.mouseData = MouseData.XButton2;
                    break;
            }
            if (InputState.Up == state) u.mi.dwFlags = (MouseEventFlag)(((int)u.mi.dwFlags) << 1);
        }
        /// <summary>
        /// 키 이벤트 명세를 생성합니다.
        /// </summary>
        /// <param name="key">이벤트를 발생시킬 System.Windows.Input.Key 입니다.</param>
        /// <param name="state">키를 누르는 이벤트인지, 떼는 이벤트인지 결정합니다.</param>
        public Input(Key key, InputState state)
        {
            type = InputType.Keyboard;
            u = new InputUnion();

            switch (key)
            {
                case Key.LeftShift:
                case Key.RightShift:
                    u.ki.wVk = VK_SHIFT;
                    break;
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    u.ki.wVk = VK_CONTROL;
                    break;
                case Key.LeftAlt:
                case Key.RightAlt:
                    u.ki.wVk = VK_MENU;
                    break;
                default:
                    u.ki.wVk = (short)KeyInterop.VirtualKeyFromKey(key);
                    break;
            }

            u.ki.wScan = (short)NativeMethods.MapVirtualKey(u.ki.wVk, MAPVK_VK_TO_VSC);
            if (InputState.Up == state) u.ki.dwFlags = KeyEventFlag.KeyUp;
        }
        /// <summary>
        /// 하드웨어 이벤트 명세를 생성합니다.
        /// </summary>
        /// <param name="uMsg">입력장치에 의해 생성된 메세지입니다.</param>
        /// <param name="wParamL">The low-order word of the lParam parameter for uMsg.</param>
        /// <param name="wParamH">The high-order word of the lParam parameter for uMsg.</param>
        public Input(int uMsg, short wParamL, short wParamH)
        {
            type = InputType.Hardware;
            u = new InputUnion();
        }
    }

    /// <summary>
    /// 입력 이벤트의 가능한 상태를 지정합니다.
    /// </summary>
    public enum InputState { Up, Down }
    
    /// <summary>
    /// 저수준 키 이벤트에 대한 데이터를 제공합니다.
    /// </summary>
    public class llKeyEventArgs : EventArgs
    {
        /// <summary>
        /// 이벤트가 발생한 System.Windows.Input.Key 입니다.
        /// </summary>
        public readonly Key Key;
        
        /// <summary>
        /// 해당 키 이벤트가 처리되었는지 처리되지 않았는지 가져오거나, 설정합니다.
        /// 한번 true로 설정된 Handled 속성은 false로 설정될 수 없습니다.
        /// </summary>
        public bool Handled
        {
            get
            {
                return handled;
            }
            set
            {
                handled |= value;
            }
        }
        bool handled = false;

        /// <summary>
        /// 저수준 키 이벤트에 대한 데이터를 생성합니다.
        /// </summary>
        /// <param name="key">이벤트가 발생한 System.Windows.Input.Key 입니다.</param>
        public llKeyEventArgs(Key key)
        {
            Key = key;
        }
    }

    /// <summary>
    /// 저수준 마우스 이벤트에 대한 데이터를 제공합니다.
    /// </summary>
    public class llMouseEventArgs : EventArgs
    {
        /// <summary>
        /// 마우스 이벤트가 발생한 위치입니다.
        /// </summary>
        public readonly position Position;
        /// <summary>
        /// 마우스 이벤트가 발생한 위치의 x좌표입니다.
        /// </summary>
        public int x { get { return Position.x; } }
        /// <summary>
        /// 마우스 이벤트가 발생한 위치의 y좌표입니다.
        /// </summary>
        public int y { get { return Position.y; } }

        /// <summary>
        /// 해당 마우스 이벤트가 처리되었는지 처리되지 않았는지 가져오거나, 설정합니다.
        /// 한번 true로 설정된 Handled 속성은 false로 설정될 수 없습니다.
        /// </summary>
        public bool Handled
        {
            get
            {
                return handled;
            }
            set
            {
                handled |= value;
            }
        }
        bool handled = false;

        /// <summary>
        /// 저수준 마우스 이벤트에 대한 데이터를 생성합니다.
        /// </summary>
        /// <param name="position">마우스 이벤트가 발생한 위치입니다.</param>
        public llMouseEventArgs(position position)
        {
            Position = position;
        }
    }
}
