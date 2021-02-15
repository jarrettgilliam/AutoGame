using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.Models;
using AutoGame.Infrastructure.Services;
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

        public MainWindowViewModel()
        {
            this.ConfigService = new ConfigService();
            this.AutoGameService = new AutoGameService();
            this.LoadedCommand = new DelegateCommand(this.OnLoaded);
            this.NotifyIconClickCommand = new DelegateCommand(this.OnNotifyIconClick);
            this.BrowseSoftwarePathCommand = new DelegateCommand(this.OnBrowseSoftwarePath);
            this.OKCommand = new DelegateCommand(this.OnOK);
            this.CancelCommand = new DelegateCommand(this.OnCancel);
            this.ApplyCommand = new DelegateCommand(this.OnApply);
        }

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
            this.AutoGameService.Dispose();
        }

        private void OnLoaded()
        {
            this.SetWindowState(WindowState.Minimized);
            this.Config = this.ConfigService.Load(this.AutoGameService.CreateDefaultConfiguration);
            this.AutoGameService.ApplyConfiguration(this.Config);
        }

        private void OnNotifyIconClick()
        {
            this.SetWindowState(WindowState.Normal);
        }

        private void OnBrowseSoftwarePath()
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

        private void OnOK()
        {
            if (this.Config.IsDirty)
            {
                this.OnApply();
            }

            this.SetWindowState(WindowState.Minimized);
        }

        private void OnCancel()
        {
            this.Config = this.ConfigService.Load(this.AutoGameService.CreateDefaultConfiguration);
            this.SetWindowState(WindowState.Minimized);
        }

        private void OnApply()
        {
            this.ConfigService.Save(this.Config);
            this.AutoGameService.ApplyConfiguration(this.Config);
        }

        private void OnConfigSoftwareKeyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Config.SoftwareKey) && sender is Config config)
            {
                ISoftwareManager s = this.AutoGameService.GetSoftwareByKey(config.SoftwareKey);
                config.SoftwarePath = s.FindSoftwarePathOrDefault();
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
