﻿using System;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using LiveCaptionsTranslator.utils;

namespace LiveCaptionsTranslator
{
    public partial class MainWindow : FluentWindow
    {
        public OverlayWindow? OverlayWindow { get; set; } = null;
        public bool IsAutoHeight { get; set; } = true;

        public MainWindow()
        {
            InitializeComponent();
            ApplicationThemeManager.ApplySystemTheme();

            Loaded += (s, e) =>
            {
                SystemThemeWatcher.Watch(
                    this,
                    WindowBackdropType.Mica,
                    true
                );
                RootNavigation.Navigate(typeof(CaptionPage));
            };

            var windowState = WindowHandler.LoadState(this, Translator.Setting);
            WindowHandler.RestoreState(this, windowState);

            ToggleTopmost(Translator.Setting.MainWindow.Topmost);
            ShowLogCard(Translator.Setting.MainWindow.CaptionLogEnabled);
        }

        private void TopmostButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleTopmost(!this.Topmost);
        }

        private void OverlayModeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var symbolIcon = button?.Icon as SymbolIcon;

            if (OverlayWindow == null)
            {
                // Caption + Translation
                symbolIcon.Symbol = SymbolRegular.TextUnderlineDouble20;

                OverlayWindow = new OverlayWindow();
                OverlayWindow.SizeChanged +=
                    (s, e) => WindowHandler.SaveState(OverlayWindow, Translator.Setting);
                OverlayWindow.LocationChanged +=
                    (s, e) => WindowHandler.SaveState(OverlayWindow, Translator.Setting);

                var windowState = WindowHandler.LoadState(OverlayWindow, Translator.Setting);
                WindowHandler.RestoreState(OverlayWindow, windowState);
                OverlayWindow.Show();
            }
            else if (!OverlayWindow.IsTranslationOnly)
            {
                // Translation Only
                symbolIcon.Symbol = SymbolRegular.TextAddSpaceBefore24;

                OverlayWindow.IsTranslationOnly = true;
                OverlayWindow.Focus();
            }
            else
            {
                // Closed
                symbolIcon.Symbol = SymbolRegular.WindowNew20;

                OverlayWindow.IsTranslationOnly = false;
                OverlayWindow.Close();
                OverlayWindow = null;
            }
        }

        private void LogOnlyButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var symbolIcon = button?.Icon as SymbolIcon;

            Translator.LogOnlyFlag = !Translator.LogOnlyFlag;
            symbolIcon.Filled = Translator.LogOnlyFlag;
        }

        private void JsonLoggingButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var symbolIcon = button?.Icon as SymbolIcon;

            Translator.JsonLoggingEnabled = !Translator.JsonLoggingEnabled;
            symbolIcon.Filled = Translator.JsonLoggingEnabled;

            if (Translator.JsonLoggingEnabled)
            {
                var logger = CaptionLogger.GetInstance();
                var logPath = logger.GetCurrentLogPath();
                ShowNotification("JSON Logging Started", $"Saving to: {logPath}");
            }
            else
            {
                ShowNotification("JSON Logging Stopped", "Caption logging has been stopped");
            }
        }

        private void CaptionLogButton_Click(object sender, RoutedEventArgs e)
        {
            Translator.Setting.MainWindow.CaptionLogEnabled = !Translator.Setting.MainWindow.CaptionLogEnabled;
            ShowLogCard(Translator.Setting.MainWindow.CaptionLogEnabled);
            CaptionPage.Instance?.AutoHeight();
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            var window = sender as Window;
            WindowHandler.SaveState(window, Translator.Setting);
        }
        
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainWindow_LocationChanged(sender, e);
            IsAutoHeight = false;
        }

        public void ToggleTopmost(bool enabled)
        {
            var button = TopmostButton as Button;
            var symbolIcon = button?.Icon as SymbolIcon;
            symbolIcon.Filled = enabled;
            this.Topmost = enabled;
            Translator.Setting.MainWindow.Topmost = enabled;
        }

        public void ShowLogCard(bool enabled)
        {
            if (CaptionLogButton.Icon is SymbolIcon icon)
            {
                if (enabled)
                    icon.Symbol = SymbolRegular.History24;
                else
                    icon.Symbol = SymbolRegular.HistoryDismiss24;
                CaptionPage.Instance?.CollapseTranslatedCaption(enabled);
            }
        }

        public void AutoHeightAdjust(int minHeight = -1, int maxHeight = -1)
        {
            if (minHeight > 0 && Height < minHeight)
            {
                Height = minHeight;
                IsAutoHeight = true;
            }
            if (IsAutoHeight && maxHeight > 0 && Height > maxHeight)
                Height = maxHeight;
        }

        private void ShowNotification(string title, string message)
        {
            // Create a simple message for notification
            // Since Snackbar implementation is causing issues, we'll comment this out for now
            // and just log to console
            Console.WriteLine($"{title}: {message}");
            
            // TODO: Fix notification when building with correct WPF UI library version
            /*
            var notificationIcon = new SymbolIcon { Symbol = SymbolRegular.Info16 };
            RootSnackbar.Show(
                title,
                message,
                ControlAppearance.Secondary,
                notificationIcon,
                TimeSpan.FromSeconds(4)
            );
            */
        }
    }
}