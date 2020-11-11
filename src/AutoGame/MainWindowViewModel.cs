using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.Services;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace AutoGame
{
    class MainWindowViewModel : IDisposable
    {
        private ISoftwareManager software;
        private IList<ILaunchCondition> launchConditions;

        public MainWindowViewModel()
        {
            ////this.software = new SteamBigPictureSoftwareManager();
            this.software = new PlayniteSoftwareManager();

            this.launchConditions = new ILaunchCondition[]
            {
                new ParsecConnectedLaunchCondition(),
                new GamepadConnectedLaunchCondition()
            };

            this.LoadedCommand = new DelegateCommand(this.OnLoaded);
        }

        public ICommand LoadedCommand { get; }

        private void OnLoaded()
        {
            foreach (ILaunchCondition condition in this.launchConditions)
            {
                condition.ConditionMet += this.OnLaunchConditionMet;
                condition.StartCheckingConditions();
            }
        }

        public void Dispose()
        {
            foreach (ILaunchCondition condition in this.launchConditions)
            {
                condition.ConditionMet -= this.OnLaunchConditionMet;
                condition.Dispose();
            }

            this.launchConditions = null;
            this.software = null;
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
