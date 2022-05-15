﻿using PasswordManager.Controls;
using PasswordManager.Helpers;
using PasswordManager.Settings;
using PasswordManager.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PasswordManager.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MaterialWindow
    {
        private readonly AppSettingsService _appSettingsService;

        private MainWindowViewModel ViewModel => DataContext as MainWindowViewModel;

        public MainWindow(
            MainWindowViewModel mainViewModel,
            AppSettingsService appSettingsService)
        {
            InitializeComponent();

            _appSettingsService = appSettingsService;

            var windowSettings = _appSettingsService.MainWindowSettings;
            if (windowSettings is not null)
            {
                var windowRect = new Rect(windowSettings.Left, windowSettings.Top, windowSettings.Width, windowSettings.Height);
                if (WindowPositionHelper.IsOnPrimaryScreen(windowRect))
                {
                    Left = windowSettings.Left;
                    Top = windowSettings.Top;
                    Width = windowSettings.Width;
                    Height = windowSettings.Height;
                    WindowState = windowSettings.WindowState;
                }
            }

            mainViewModel.CredentialSelected += Vm_CredentialSelected;
            DataContext = mainViewModel;
        }

        private void Vm_CredentialSelected(CredentialViewModel credVM)
        {
            var passStringLength = credVM?.PasswordFieldVM?.Value?.Length ?? 0;
            PasswordsControl.CredentialsDialog.PasswordFieldBox.Password = new string('*', passStringLength);
            PasswordsControl.CredentialsListBox.ScrollIntoView(credVM);
        }

        private void MaterialWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //TODO: Shutdown process on main window close or hide window and cancel depend on settings
            Hide();
            ShowInTaskbar = false;
            e.Cancel = true;
        }

        private void ListBoxItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var anyCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (anyCtrlPressed)
                e.Handled = true;
        }

        private async void MaterialWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool visibility && visibility && ViewModel.SelectedNavigationItem is PasswordsViewModel)
            {
                await Task.Delay(10);
                PasswordsControl.SearchTextBox.Focus();
            }
        }

        private void MaterialWindow_Closed(object sender, EventArgs e)
        {
            var saveRequired = false;
            if (ViewModel.SettingsVM.ThemeMode != _appSettingsService.ThemeMode)
            {
                _appSettingsService.ThemeMode = ViewModel.SettingsVM.ThemeMode;
                saveRequired = true;
            }

            if (!ViewModel.SettingsVM.ShowPopupHotkey.Equals(_appSettingsService.ShowPopupHotkey))
            {
                _appSettingsService.ShowPopupHotkey = ViewModel.SettingsVM.ShowPopupHotkey;
                saveRequired = true;
            }

            if (ViewModel.PasswordsVM.Sort != _appSettingsService.Sort)
            {
                _appSettingsService.Sort = ViewModel.PasswordsVM.Sort;
                saveRequired = true;
            }

            if (ViewModel.PasswordsVM.Order != _appSettingsService.Order)
            {
                _appSettingsService.Order = ViewModel.PasswordsVM.Order;
                saveRequired = true;
            }

            // Avoid minimized state
            if (WindowState != WindowState.Minimized)
            {
                var currentWindowSettings = new WindowSettings()
                {
                    Left = Left,
                    Top = Top,
                    Width = Width,
                    Height = Height,
                    WindowState = WindowState
                };
                if (!currentWindowSettings.Equals(_appSettingsService.MainWindowSettings))
                {
                    _appSettingsService.MainWindowSettings = currentWindowSettings;
                    saveRequired = true;
                }
            }

            if (saveRequired)
            {
                // Save settings and wait to avoid file corruptions
                _appSettingsService.Save().Wait();
            }
        }
    }
}
