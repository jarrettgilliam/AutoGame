using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.Services;
using AutoGame.Models;
using AutoGame.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace AutoGame.ViewModels
{
    internal class MainWindowViewModel : BindableBase, IDisposable
    {
        private readonly ConfigService configService;

        private ISoftwareManager software;
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
            this.OKCommand = new DelegateCommand(this.OnOK);
            this.CancelCommand = new DelegateCommand(this.OnCancel);
            this.ApplyCommand = new DelegateCommand(this.OnApply);
        }

        public ICommand LoadedCommand { get; }

        public ICommand NotifyIconClickCommand { get; }

        public ICommand OKCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand ApplyCommand { get; }

        public Config Config
        {
            get => this.config;
            set => this.SetProperty(ref this.config, value);
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
            this.software = null;
        }

        private void OnLoaded()
        {
            this.WindowState = WindowState.Minimized;
            this.Config = this.configService.Load();
            this.ApplyConfiguration();
        }

        private void OnNotifyIconClick()
        {
            this.ShowWindow = true;
            this.WindowState = WindowState.Normal;
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
            this.Config = this.configService.Load();
            this.WindowState = WindowState.Minimized;
        }

        private void OnApply()
        {
            this.configService.Save(this.Config);
            this.ApplyConfiguration();
        }

        private void ApplyConfiguration()
        {
            if (this.Config.GameLauncher == Constants.Playnite)
            {
                this.software = new PlayniteFullscreenManager();
            }
            else
            {
                this.software = new SteamBigPictureManager();
            }

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
            if (!this.software.IsRunning)
            {
                this.software.Start();
            }
        }
    }
}
