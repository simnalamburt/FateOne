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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Media;
using System.Reflection;
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
    using F = System.Windows.Forms;
    using R = Properties.Resources;

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        LogicManager LogicManager = new LogicManager();



        F.NotifyIcon trayIcon = new F.NotifyIcon()
        {
            Icon = Properties.Resources.FateOne,
            Text = Properties.Resources.TrayIconText,
            Visible = true
        };



        // 생성자
        public MainWindow()
        {
            InitializeComponent();

            var asm = Assembly.GetExecutingAssembly();
            var info = FileVersionInfo.GetVersionInfo(asm.Location);
            Title = info.ProductName + " " + info.ProductVersion;

            RenderOptions.SetBitmapScalingMode(KeyboardImage, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(ActivatedImage, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(PausedImage, BitmapScalingMode.NearestNeighbor);

            #region LogicManager 이벤트 핸들링
            LogicManager.Activated += delegate(object sender, EventArgs e)
            {
                // 활성화 그림 보이기
                ActivatedImage.Visibility = Visibility.Visible;
                // 켜는소리
                using (SoundPlayer sound = new SoundPlayer(R.ProgramActivated))
                    sound.Play();
            };
            LogicManager.Deactivated += delegate(object sender, EventArgs e)
            {
                // 활성화 그림 감추기
                ActivatedImage.Visibility = Visibility.Collapsed;
                // 끄는소리
                using (SoundPlayer sound = new SoundPlayer(R.ProgramDeactivated))
                    sound.Play();
            };
            LogicManager.SmartCastingToggled += delegate(object sender, EventArgs e)
            {
                // 토글할때 소리
                using (SoundPlayer sound = new SoundPlayer(R.SmartCastingToggled))
                    sound.Play();
            };
            LogicManager.SmartCastingUntoggled += delegate(object sender, EventArgs e)
            {
                // 언토글할때 소리
                using (SoundPlayer sound = new SoundPlayer(R.SmartCastingUntoggled))
                    sound.Play();
            };

            LogicManager.Paused += (object sender, EventArgs e) => PausedImage.Visibility = Visibility.Visible;
            LogicManager.Resumed += (object sender, EventArgs e) => PausedImage.Visibility = Visibility.Collapsed;
            #endregion

            #region 트레이메뉴 초기화
            F.MenuItem commandOption = new F.MenuItem(Properties.Resources.TrayMenu_CommandOption);
            commandOption.Checked = LogicManager.CommandEnabled;
            commandOption.Click += delegate(object click, EventArgs e)
            {
                commandOption.Checked = !commandOption.Checked;
                LogicManager.CommandEnabled = commandOption.Checked;
            };
            F.MenuItem smartcastingOption = new F.MenuItem(Properties.Resources.TrayMenu_SmartCastingOption);
            smartcastingOption.Checked = LogicManager.SmartCastingEnabled;
            smartcastingOption.Click += delegate(object click, EventArgs e)
            {
                smartcastingOption.Checked = !smartcastingOption.Checked;
                LogicManager.SmartCastingEnabled = smartcastingOption.Checked;
            };
            #endregion
            #region 트레이 초기화
            trayIcon.ContextMenu = new F.ContextMenu(new F.MenuItem[]{
                commandOption,
                smartcastingOption,
                new F.MenuItem("-"),
                new F.MenuItem(Properties.Resources.TrayMenu_Exit, (object click, EventArgs e) => Close())
            });
            trayIcon.DoubleClick += delegate(object senders, EventArgs args)
            {
                Show();
                WindowState = WindowState.Normal;
            };
            #endregion
        }

        // WPF 이벤트 핸들링
        private void onStateChanged(object sender, EventArgs e)
        {
            if (WindowState.Minimized == WindowState) Hide();
        }
        private void onClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            trayIcon.Visible = false;
        }
    }
}
