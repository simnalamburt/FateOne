using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FateOne
{
    /// <summary>
    /// FateOne 키 매핑 논리를 관리합니다.
    /// </summary>
    public class LogicManager
    {
        #region 프로그램 상수
        public const int KeyDelay = 25;
        public const int AutoMouseDelay = 20;

        static class Keys
        {
            public const Key Power = Key.F9;

            public const Key Pause = Key.Enter;

            public const Key Purchase = Key.Tab;
            public const Key HolyGrail = Key.Z;
            public const Key Attribute = Key.X;
            public const Key Storage = Key.C;

            public const Key Command1 = Key.D1;
            public const Key Command2 = Key.D2;
            public const Key Command3 = Key.D3;
            public const Key Command4 = Key.D4;
            public const Key Command5 = Key.D5;
            public const Key Command6 = Key.D6;

            public const Key Item1 = Key.B;
            public const Key Item2 = Key.N;
            public const Key Item4 = Key.G;
            public const Key Item5 = Key.H;
            public const Key Item7 = Key.T;
            public const Key Item8 = Key.Y;

            public const Key Servant = Key.Space;

            public const Key SkillQ = Key.Q;
            public const Key SkillW = Key.W;
            public const Key SkillE = Key.E;
            public const Key SkillR = Key.R;
            public const Key SkillD = Key.D;
            public const Key SkillF = Key.F;
            public const Key SkillG = Key.V;
            public const Key Stats = Key.S;

            public const Key AbilityPoint = Key.LeftCtrl;
            public const Key SmartCasting = Key.LeftShift;
            public const Key SmartCastingToggle = Key.CapsLock;
            public const Key SelfCasting = Key.LeftAlt;

            public const Key AutoMouse = Key.F8;
        }
        #endregion

        llInputManager mapper = new llInputManager() { Hooked = true };



        // public
        #region Active, Activated, Deactivated 정의
        /// <summary>
        /// true로 설정할경우 모든 키매핑이 활성화되고
        /// false로 설정할경우 Keys.Power를 제외한 모든 키매핑이 비활성화됩니다
        /// </summary>
        public bool Active
        {
            set
            {
                if (value != active)
                {
                    active = value;
                    if (value)
                    {
                        // 키매핑 활성화
                        mapper.KeyDown += new EventHandler<llKeyEventArgs>(onKeyDown);
                        mapper.KeyUp += new EventHandler<llKeyEventArgs>(onKeyUp);

                        // 마우스가두기 활성화
                        mapper.MouseMove += new EventHandler<llMouseEventArgs>(onMouseMove);
                        Process client = Process.GetProcessesByName(Properties.Resources.ProcessName).FirstOrDefault();
                        if (client != null)
                        {
                            IntPtr hWnd = client.MainWindowHandle;
                            if (hWnd != IntPtr.Zero)
                                MouseTrap = new windowRect(hWnd);
                        }

                        // 이벤트 발생
                        Activated(this, EventArgs.Empty);
                    }
                    else
                    {
                        // 키매핑 비활성화
                        mapper.KeyDown -= new EventHandler<llKeyEventArgs>(onKeyDown);
                        mapper.KeyUp -= new EventHandler<llKeyEventArgs>(onKeyUp);

                        // 마우스가두기 비활성화
                        mapper.MouseMove -= new EventHandler<llMouseEventArgs>(onMouseMove);
                        MouseTrap = null;

                        // 필드 정리
                        Pause = false;
                        SmartCastingToggle = false;
                        Focus = FocusPosition.Servant;
                        MouseTrap = null;
                        AutoMouse = false;

                        // 이벤트 발생
                        Deactivated(this, EventArgs.Empty);
                    }
                }
            }
            get { return active; }
        }
        bool active = false;

        /// <summary>
        /// 속성 Active가 true로 설정될 경우 발생합니다.
        /// </summary>
        public event EventHandler Activated = delegate { };

        /// <summary>
        /// 속성 Active가 false로 설정될 경우 발생합니다.
        /// </summary>
        public event EventHandler Deactivated = delegate { };
        #endregion
        #region Pause, Paused, Resumed 정의
        /// <summary>
        /// true로 설정할경우 Keys.Power과 Keys.Pause를 제외한 모든 키매핑이 비활성화되고,
        /// false로 설정할경우 모든 키매핑이 다시 활성화됩니다.
        /// </summary>
        public bool Pause
        {
            set
            {
                if (value != pause)
                {
                    pause = value;

                    // 이벤트 발생
                    if (value) Paused(this, EventArgs.Empty);
                    else Resumed(this, EventArgs.Empty);
                }
            }
            get { return pause; }
        }
        bool pause = false;

        /// <summary>
        /// 속성 Pause가 true로 설정될 경우 발생합니다.
        /// </summary>
        public event EventHandler Paused = delegate { };

        /// <summary>
        /// 속성 Pause가 false로 설정될 경우 발생합니다.
        /// </summary>
        public event EventHandler Resumed = delegate { };
        #endregion
        #region SmartCastingToggle, SmartCastingToggled, SmartCastingUntoggled
        /// <summary>
        /// 스마트 캐스팅 토글 여부를 가져오거나, 설정합니다.
        /// </summary>
        public bool SmartCastingToggle
        {
            set
            {
                if (value != smartCastingToggle)
                {
                    smartCastingToggle = value;
                    if (value) SmartCastingToggled(this, EventArgs.Empty);
                    else SmartCastingUntoggled(this, EventArgs.Empty);
                }
            }
            get { return smartCastingToggle; }
        }
        bool smartCastingToggle;

        /// <summary>
        /// 스마트 캐스팅 토글이 활성화되었을 때 발생합니다.
        /// </summary>
        public event EventHandler SmartCastingToggled = delegate { };

        /// <summary>
        /// 스마트 캐스팅 토글이 비활성화되었을 때 발생합니다.
        /// </summary>
        public event EventHandler SmartCastingUntoggled = delegate { };
        #endregion

        #region CommandEnabled 정의
        /// <summary>
        /// true로 설정할경우 령주 키매핑이 활성화되고, false로 설정할경우 령주 키매핑이 비활성화 됩니다.
        /// 기본값은 true 입니다.
        /// </summary>
        public bool CommandEnabled { get; set; }
        #endregion
        #region SmartCastingEnabled
        /// <summary>
        /// true로 설정할경우 스마트 캐스팅을 쓸 수 있게되고, false로 설정할경우 스마트 캐스팅을 사용할 수 없게 됩니다.
        /// 토글키를 사용한 스마트 캐스팅은 이 속성과 관계 없이 사용할 수 있습니다.
        /// 기본값은 true 입니다.
        /// </summary>
        public bool SmartCastingEnabled { get; set; }
        #endregion

        // private
        #region Focus 정의
        FocusPosition Focus
        {
            set
            {
                if (value != focus)
                {
                    switch (value)
                    {
                        case FocusPosition.Servant: llInputManager.ProcessKey(Key.F1); goto IsDefined;
                        case FocusPosition.Purchase: llInputManager.ProcessKey(Key.D0); goto IsDefined;
                        case FocusPosition.HolyGrail: llInputManager.ProcessKey(Key.F3); goto IsDefined;
                        case FocusPosition.Storage: llInputManager.ProcessKey(Key.F2); goto IsDefined;
                        case FocusPosition.Attribute:
                            lock (mapper)
                            {
                                llInputManager.ProcessKey(Key.F3);
                                Thread.Sleep(KeyDelay);
                                llInputManager.ProcessKey(Key.Z);
                            }
                            goto IsDefined;
                        IsDefined:
                            focus = value;
                            break;
                    }
                }
            }
            get { return focus; }
        }
        FocusPosition focus = FocusPosition.Servant;
        enum FocusPosition
        {
            Servant = Keys.Servant,
            Purchase = Keys.Purchase,
            HolyGrail = Keys.HolyGrail,
            Storage = Keys.Storage,
            Attribute = Keys.Attribute
        }
        #endregion
        #region MouseTrap 정의
        /// <summary>
        /// WindowRect를 설정할경우 마우스가 설정된 직사각형 범위를 벗어나지 못하게 됩니다.
        /// null 로 설정할경우 마우스 가두기가 비활성화됩니다.
        /// </summary>
        windowRect? MouseTrap { get; set; }
        #endregion
        #region AutoMouse 정의
        bool AutoMouse
        {
            set
            {
                if (value != autoMouse)
                {
                    autoMouse = value;
                    if (value) new Thread(() =>
                    {
                        while (autoMouse)
                        {
                            llInputManager.ProcessMouse(MouseButton.Left);
                            Thread.Sleep(AutoMouseDelay);
                        }
                    }).Start();
                }
            }
            get { return autoMouse; }
        }
        volatile bool autoMouse;
        #endregion



        // 생성자
        public LogicManager()
        {
            mapper.KeyDown += new EventHandler<llKeyEventArgs>(onPowerKeyDown);
            CommandEnabled = true;
            SmartCastingEnabled = true;
        }

        // mapper 이벤트 핸들링
        void onPowerKeyDown(object sender, llKeyEventArgs e)
        {
            /// 부팅버튼
            /// 부작용 인정 안함
            if (Keys.Power == e.Key)
            {
                Active = !Active;
                e.Handled = true;
            }
        }
        void onKeyDown(object sender, llKeyEventArgs e)
        {
            #region 사전처리
            if (Keys.Pause == e.Key)
            {
                Pause = !Pause;
                return;
            }
            #endregion
            #region 탈출조건
            // Alt+Tab, Alt+F4 일때 : 시스템 비활성화
            if (0 != (Keyboard.Modifiers & ModifierKeys.Alt) && (Key.F4 == e.Key || Key.Tab == e.Key))
            {
                Active = false;
                return;
            }

            // 일시정지상태 일때
            if (Pause) return;
            #endregion

            Key reservedKey;
            switch (e.Key)
            {
                #region 상점, 성배, 창고, 특성
                case Keys.Purchase:
                case Keys.HolyGrail:
                case Keys.Storage:
                case Keys.Attribute:
                    if (e.Key == (Key)Focus) e.Handled = true;
                    else if (FocusPosition.Servant != Focus) return;

                    Focus = (FocusPosition)e.Key;
                    e.Handled = true;
                    return;

                #endregion

                #region 령주
                case Keys.Command1: reservedKey = Key.Q; goto Command;
                case Keys.Command2: reservedKey = Key.W; goto Command;
                case Keys.Command3: reservedKey = Key.E; goto Command;
                case Keys.Command4: reservedKey = Key.R; goto Command;
                case Keys.Command5: reservedKey = Key.A; goto Command;
                case Keys.Command6: reservedKey = Key.S; goto Command;
                Command:
                    if (CommandEnabled)
                    {
                        lock (mapper)
                        {
                            llInputManager.ProcessKey(Key.F3);
                            Thread.Sleep(KeyDelay);
                            llInputManager.ProcessInput(
                                new Input(Key.F, InputState.Down),
                                new Input(Key.F, InputState.Up),
                                new Input(reservedKey, InputState.Down),
                                new Input(reservedKey, InputState.Up));
                            if (Keys.Command6 != e.Key)
                                llInputManager.ProcessKey(Key.F1);
                        }
                        e.Handled = true;
                    }
                    return;
                #endregion



                #region 서번트
                case Keys.Servant:
                    llInputManager.ProcessInput(
                        new Input(Key.F1, InputState.Down),
                        new Input(Key.F1, InputState.Up),
                        new Input(Key.F1, InputState.Down),
                        new Input(Key.F1, InputState.Up));
                    e.Handled = true;
                    return;
                #endregion

                #region 스마트 캐스팅 토글
                case Keys.SmartCastingToggle:
                    SmartCastingToggle = !SmartCastingToggle;
                    e.Handled = true;
                    return;
                #endregion
                #region 스킬·아이템, 어빌리티 투자, 스마트 캐스팅, 셀프 캐스팅
                case Keys.Item1: reservedKey = Key.NumPad1; goto SmartCasting_SelfCasting;
                case Keys.Item2: reservedKey = Key.NumPad2; goto SmartCasting_SelfCasting;
                case Keys.Item4: reservedKey = Key.NumPad4; goto SmartCasting_SelfCasting;
                case Keys.Item5: reservedKey = Key.NumPad5; goto SmartCasting_SelfCasting;
                case Keys.Item7: reservedKey = Key.NumPad7; goto SmartCasting_SelfCasting;
                case Keys.Item8: reservedKey = Key.NumPad8; goto SmartCasting_SelfCasting;
                case Keys.SkillD: reservedKey = Key.D; goto SmartCasting_SelfCasting;
                case Keys.SkillF: reservedKey = Key.F; goto SmartCasting_SelfCasting;
                case Keys.SkillG: reservedKey = Key.G; goto SmartCasting_SelfCasting;
                case Keys.SkillQ: reservedKey = Key.Q; goto SmartCasting_SelfCasting;
                case Keys.SkillW: reservedKey = Key.W; goto SmartCasting_SelfCasting;
                case Keys.SkillE: reservedKey = Key.E; goto SmartCasting_SelfCasting;
                case Keys.SkillR: reservedKey = Key.R; goto SmartCasting_SelfCasting;
                case Keys.Stats: reservedKey = Key.B; goto AbilityPoint;

                SmartCasting_SelfCasting:
                    if (FocusPosition.Servant != Focus) return;
                    #region 처리
                    if (SmartCastingToggle)
                    {
                        lock (mapper)
                        {
                            llInputManager.ProcessInput(
                                new Input(reservedKey, InputState.Down),
                                new Input(reservedKey, InputState.Up));
                            Thread.Sleep(KeyDelay);
                            llInputManager.ProcessInput(
                                new Input(MouseButton.Left, InputState.Down),
                                new Input(MouseButton.Left, InputState.Up),
                                new Input(Key.Escape, InputState.Down),
                                new Input(Key.Escape, InputState.Up));
                        }
                        e.Handled = true;
                        return;
                    }
                    if (SmartCastingEnabled)
                    {
                        if (Keyboard.IsKeyDown(Keys.SmartCasting))
                        {
                            lock (mapper)
                            {
                                llInputManager.ProcessInput(
                                    new Input(Keys.SmartCasting, InputState.Up),
                                    new Input(reservedKey, InputState.Down),
                                    new Input(reservedKey, InputState.Up));
                                Thread.Sleep(KeyDelay);
                                llInputManager.ProcessInput(
                                    new Input(MouseButton.Left, InputState.Down),
                                    new Input(MouseButton.Left, InputState.Up),
                                    new Input(Key.Escape, InputState.Down),
                                    new Input(Key.Escape, InputState.Up),
                                    new Input(Keys.SmartCasting, InputState.Down));
                            }
                            e.Handled = true;
                            return;
                        }
                    }
                    if (Keyboard.IsKeyDown(Keys.SelfCasting))
                    {
                        // TODO 셀프캐스팅 완성
                        e.Handled = true;
                        return;
                    }
                    #endregion
                    #region 검사지점
                    switch (e.Key)
                    {
                        case Keys.Item1:
                        case Keys.Item2:
                        case Keys.Item4:
                        case Keys.Item5:
                        case Keys.Item7:
                        case Keys.Item8:
                        case Keys.SkillD:
                        case Keys.SkillF:
                        case Keys.SkillG:
                            goto NormalCasting;
                    }
                    #endregion
                AbilityPoint:
                    #region 처리
                    if (Keyboard.IsKeyDown(Keys.AbilityPoint))
                    {
                        llInputManager.ProcessInput(
                            new Input(Keys.AbilityPoint, InputState.Up),
                            new Input(Key.O, InputState.Down),
                            new Input(Key.O, InputState.Up),
                            new Input(reservedKey, InputState.Down),
                            new Input(reservedKey, InputState.Up),
                            new Input(Key.Escape, InputState.Down),
                            new Input(Key.Escape, InputState.Up),
                            new Input(Keys.AbilityPoint, InputState.Down));
                        e.Handled = true;
                        return;
                    }
                    #endregion
                    #region 검사지점
                    if (Keys.Stats == e.Key) return;
                    #endregion
                NormalCasting:
                    #region 처리
                    llInputManager.ProcessKey(reservedKey);
                    e.Handled = true;
                    #endregion
                    return;
                #endregion
                
                #region 오토마우스
                case Keys.AutoMouse:
                    AutoMouse = true;
                    e.Handled = true;
                    return;
                #endregion
            }
        }
        void onKeyUp(object sender, llKeyEventArgs e)
        {
            #region 탈출조건
            // Alt+Tab 일때
            if (0 != (Keyboard.Modifiers & ModifierKeys.Alt) && (Key.F4 == e.Key || Key.Tab == e.Key))
                return;
            // 일시정지상태 일때
            if (Pause) return;
            #endregion

            switch (e.Key)
            {
                #region 상점, 성배, 창고, 특성
                case Keys.Purchase:
                case Keys.HolyGrail:
                case Keys.Storage:
                case Keys.Attribute:
                    if ((FocusPosition)e.Key == Focus)
                    {
                        Focus = FocusPosition.Servant;
                        e.Handled = true;
                    }
                    return;
                #endregion

                #region 오토마우스
                case Keys.AutoMouse:
                    AutoMouse = false;
                    e.Handled = true;
                    return;
                #endregion
            }
        }
        void onMouseMove(object sender, llMouseEventArgs e)
        {
            if (MouseTrap != null)
            {
                bool updated = false;
                position pos = e.Position;
                if (pos.x < MouseTrap.Value.left)
                {
                    updated = true;
                    pos.x = MouseTrap.Value.left;
                }
                else if (pos.x > MouseTrap.Value.right)
                {
                    updated = true;
                    pos.x = MouseTrap.Value.right;
                }
                if (pos.y < MouseTrap.Value.top)
                {
                    updated = true;
                    pos.y = MouseTrap.Value.top;
                }
                else if (pos.y > MouseTrap.Value.bottom)
                {
                    updated = true;
                    pos.y = MouseTrap.Value.bottom;
                }
                if (updated)
                {
                    llInputManager.MousePosition = pos;
                    e.Handled = true;
                }
            }
        }
    }
}
