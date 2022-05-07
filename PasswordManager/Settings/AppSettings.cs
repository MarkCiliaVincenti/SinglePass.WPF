﻿using PasswordManager.Hotkeys;

namespace PasswordManager.Settings
{
    public class AppSettings : IAppSettings
    {
        public MaterialDesignThemes.Wpf.BaseTheme ThemeMode { get; set; }
        public bool GoogleDriveEnabled { get; set; }
        public Hotkey ShowPopupHotkey { get; set; }
        public WindowSettings MainWindowSettings { get; set; }

        public AppSettings()
        {
            ShowPopupHotkey = Hotkey.Empty;
        }
    }
}
