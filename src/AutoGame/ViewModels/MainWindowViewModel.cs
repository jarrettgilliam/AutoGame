using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.Models;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;

namespace AutoGame.ViewModels
{
    internal class MainWindowViewModel : BindableBase, IDisposable
    {
        private Config config;
        private WindowState windowState;
        private bool showWindow = true;
        private bool notifyIconVisible;

        public MainWindowViewModel(
            ILoggingService loggingService,
            IConfigService configService,
            IAutoGameService autoGameService)
        {
            this.LoggingService = loggingService;
            this.ConfigService = configService;
            this.AutoGameService = autoGameService;

            this.LoadedCommand = new DelegateCommand(this.OnLoaded);
            this.NotifyIconClickCommand = new DelegateCommand(this.OnNotifyIconClick);
            this.BrowseSoftwarePathCommand = new DelegateCommand(this.OnBrowseSoftwarePath);
            this.OKCommand = new DelegateCommand(this.OnOK);
            this.CancelCommand = new DelegateCommand(this.OnCancel);
            this.ApplyCommand = new DelegateCommand(this.OnApply);
        }

        private ILoggingService LoggingService { get; }

        private IConfigService ConfigService { get; }

        public IAutoGameService AutoGameService { get; }

        public ICommand LoadedCommand { get; }

        public ICommand NotifyIconClickCommand { get; }

        public ICommand BrowseSoftwarePathCommand { get; }

        public ICommand OKCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand ApplyCommand { get; }

        public Config Config
        {
            get => this.config;

            set
            {
                var oldValue = this.config;
                if (this.SetProperty(ref this.config, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.PropertyChanged -= this.OnConfigSoftwareKeyChanged;
                    }

                    if (value != null)
                    {
                        value.PropertyChanged += this.OnConfigSoftwareKeyChanged;
                    }
                }
            }
        }

        public WindowState WindowState
        {
            get => this.windowState;
            set => this.SetProperty(ref this.windowState, value);
        }

        public bool ShowWindow
        {
            get => this.showWindow;
            set => this.SetProperty(ref this.showWindow, value);
        }

        public bool NotifyIconVisible
        {
            get => this.notifyIconVisible;
            set => this.SetProperty(ref this.notifyIconVisible, value);
        }

        public void Dispose()
        {
            try
            {
                this.AutoGameService.Dispose();
            }
            catch (Exception ex)
            {
                this.LoggingService.LogException("disposing main window view model", ex);
            }

            this.LoggingService.Dispose();
        }

        private void OnLoaded()
        {
            try
            {
                this.SetWindowState(WindowState.Minimized);
                this.Config = this.LoadConfig();
                this.AutoGameService.ApplyConfiguration(this.Config);
            }
            catch (Exception ex)
            {
                this.LoggingService.LogException("handling application loaded", ex);
            }
        }

        private void OnNotifyIconClick()
        {
            try
            {
                this.SetWindowState(WindowState.Normal);
            }
            catch (Exception ex)
            {
                this.LoggingService.LogException("handling tray icon click", ex);
            }
        }

        private void OnBrowseSoftwarePath()
        {
            try
            {
                ISoftwareManager software = this.AutoGameService.GetSoftwareByKey(this.Config.SoftwareKey);
                string defaultPath = software.FindSoftwarePathOrDefault();
                string executable = Path.GetFileName(defaultPath);

                var dialog = new OpenFileDialog()
                {
                    FileName = executable,
                    InitialDirectory = Path.GetDirectoryName(this.Config.SoftwarePath)
                };

                if (string.IsNullOrEmpty(dialog.InitialDirectory))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(defaultPath);
                }

                dialog.Filter = $"{software.Description}|{executable}";

                if (dialog.ShowDialog() == true)
                {
                    this.Config.SoftwarePath = dialog.FileName;
                }
            }
            catch (Exception ex)
            {
                this.LoggingService.LogException("browsing for a software path", ex);
            }
        }

        private void OnOK()
        {
            try
            {
                if (this.Config.IsDirty)
                {
                    this.OnApply();
                }

                this.SetWindowState(WindowState.Minimized);
            }
            catch (Exception ex)
            {
                this.LoggingService.LogException("handling OK", ex);
            }
        }

        private void OnCancel()
        {
            try
            {
                this.Config = this.LoadConfig();
                this.SetWindowState(WindowState.Minimized);
            }
            catch (Exception ex)
            {
                this.LoggingService.LogException("handling Cancel", ex);
            }
        }

        private Config LoadConfig()
        {
            Config config = this.ConfigService.Load(this.AutoGameService.CreateDefaultConfiguration);
            this.LoggingService.EnableTraceLogging = config.EnableTraceLogging;
            return config;
        }

        private void OnApply()
        {
            try
            {
                this.ConfigService.Save(this.Config);
                this.AutoGameService.ApplyConfiguration(this.Config);
            }
            catch (Exception ex)
            {
                this.LoggingService.LogException("handling Apply", ex);
            }
        }

        private void OnConfigSoftwareKeyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == nameof(this.Config.SoftwareKey) && sender is Config config)
                {
                    ISoftwareManager s = this.AutoGameService.GetSoftwareByKey(config.SoftwareKey);
                    config.SoftwarePath = s.FindSoftwarePathOrDefault();
                }
            }
            catch (Exception ex)
            {
                this.LoggingService.LogException("handling config software key changed", ex);
            }
        }

        private void SetWindowState(WindowState windowState)
        {
            if (this.WindowState == windowState)
            {
                return;
            }

            if (windowState == WindowState.Minimized)
            {
                this.WindowState = windowState;
                this.ShowWindow = false;
                this.NotifyIconVisible = true;
            }
            else
            {
                this.NotifyIconVisible = false;
                this.ShowWindow = true;
                this.WindowState = windowState;
            }
        }
    }
}
