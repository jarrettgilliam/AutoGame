using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.LaunchConditions;
using AutoGame.Infrastructure.Models;
using AutoGame.Infrastructure.Services;
using AutoGame.Infrastructure.SoftwareManager;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AutoGame.ViewModels
{
    internal class MainWindowViewModel : BindableBase, IDisposable
    {
        private readonly ConfigService configService;

        private ISoftwareManager appliedSoftware;
        private IList<ILaunchCondition> launchConditions;
        private Config config;

        private WindowState windowState;
        private bool showWindow = true;
        private bool notifyIconVisible;

        public MainWindowViewModel()
        {
            this.configService = new ConfigService();
            this.LoadedCommand = new DelegateCommand(this.OnLoaded);
            this.NotifyIconClickCommand = new DelegateCommand(this.OnNotifyIconClick);
            this.BrowseSoftwarePathCommand = new DelegateCommand(this.OnBrowseSoftwarePath);
            this.OKCommand = new DelegateCommand(this.OnOK);
            this.CancelCommand = new DelegateCommand(this.OnCancel);
            this.ApplyCommand = new DelegateCommand(this.OnApply);
        }

        public ICommand LoadedCommand { get; }

        public ICommand NotifyIconClickCommand { get; }

        public ICommand BrowseSoftwarePathCommand { get; }

        public ICommand OKCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand ApplyCommand { get; }

        public IList<ISoftwareManager> AvailableSoftware { get; } = new ISoftwareManager[]
        {
            new SteamBigPictureManager(),
            new PlayniteFullscreenManager()
        };

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
            set => this.SetProperty(ref this.windowState, value, this.OnWindowStateChanged);
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
            this.DisposeLaunchConditions();
            this.appliedSoftware = null;
        }

        private void OnLoaded()
        {
            this.WindowState = WindowState.Minimized;
            this.Config = this.configService.Load(this.CreateDefaultConfig);
            this.ApplyConfiguration();
        }

        private void OnNotifyIconClick()
        {
            this.ShowWindow = true;
            this.WindowState = WindowState.Normal;
        }

        private void OnBrowseSoftwarePath()
        {
            ISoftwareManager software = this.GetSoftwareByKey(this.Config.SoftwareKey);
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

            this.WindowState = WindowState.Minimized;
        }

        private void OnCancel()
        {
            this.Config = this.configService.Load(this.CreateDefaultConfig);
            this.WindowState = WindowState.Minimized;
        }

        private void OnApply()
        {
            this.configService.Save(this.Config);
            this.ApplyConfiguration();
        }

        private Config CreateDefaultConfig()
        {
            ISoftwareManager s = this.AvailableSoftware.First();

            return new Config()
            {
                SoftwareKey = s.Key,
                SoftwarePath = s.FindSoftwarePathOrDefault(),
                LaunchWhenGamepadConnected = true,
                LaunchWhenParsecConnected = true
            };
        }

        private void OnConfigSoftwareKeyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Config.SoftwareKey) && sender is Config config)
            {
                ISoftwareManager s = this.GetSoftwareByKey(config.SoftwareKey);
                config.SoftwarePath = s.FindSoftwarePathOrDefault();
            }
        }

        private void ApplyConfiguration()
        {
            this.appliedSoftware = this.GetSoftwareByKey(this.Config.SoftwareKey);

            this.DisposeLaunchConditions();

            this.launchConditions = new List<ILaunchCondition>();

            if (this.Config.LaunchWhenGamepadConnected)
            {
                this.launchConditions.Add(new GamepadConnectedCondition());
            }

            if (this.Config.LaunchWhenParsecConnected)
            {
                this.launchConditions.Add(new ParsecConnectedCondition());
            }

            foreach (ILaunchCondition condition in this.launchConditions)
            {
                condition.ConditionMet += this.OnLaunchConditionMet;
                condition.StartCheckingConditions();
            }
        }

        private ISoftwareManager GetSoftwareByKey(string softwareKey)
        {
            return this.AvailableSoftware.FirstOrDefault(s => s.Key == softwareKey) ??
                   this.AvailableSoftware.FirstOrDefault();
        }

        private void OnWindowStateChanged()
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowWindow = false;
                this.NotifyIconVisible = true;
            }
            else
            {
                this.NotifyIconVisible = false;
            }
        }

        private void DisposeLaunchConditions()
        {
            if (this.launchConditions != null)
            {
                foreach (ILaunchCondition condition in this.launchConditions)
                {
                    condition.ConditionMet -= this.OnLaunchConditionMet;
                    condition.Dispose();
                }

                this.launchConditions = null;
            }
        }

        private void OnLaunchConditionMet(object sender, EventArgs e)
        {
            if (!this.appliedSoftware.IsRunning)
            {
                this.appliedSoftware.Start(this.Config.SoftwarePath);
            }
        }
    }
}
