using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.LaunchConditions;
using AutoGame.Infrastructure.Models;
using AutoGame.Infrastructure.SoftwareManager;

namespace AutoGame.Infrastructure.Services
{
    public sealed class AutoGameService : IAutoGameService
    {
        private IList<ILaunchCondition> launchConditions;
        private ISoftwareManager appliedSoftware;
        private string appliedSoftwarePath;

        public AutoGameService()
        {
            this.AvailableSoftware = new ISoftwareManager[]
            {
                new SteamBigPictureManager(),
                new PlayniteFullscreenManager()
            };
        }

        public IList<ISoftwareManager> AvailableSoftware { get; }

        public void ApplyConfiguration(Config config)
        {
            this.ValidateConfig(config);

            this.appliedSoftware = this.GetSoftwareByKey(config.SoftwareKey);
            this.appliedSoftwarePath = config.SoftwarePath;

            this.DisposeLaunchConditions();

            this.launchConditions = new List<ILaunchCondition>();

            if (config.LaunchWhenGamepadConnected)
            {
                this.launchConditions.Add(new GamepadConnectedCondition());
            }

            if (config.LaunchWhenParsecConnected)
            {
                this.launchConditions.Add(new ParsecConnectedCondition());
            }

            foreach (ILaunchCondition condition in this.launchConditions)
            {
                condition.ConditionMet += this.OnLaunchConditionMet;
                condition.StartCheckingConditions();
            }
        }

        public Config CreateDefaultConfiguration()
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

        public ISoftwareManager GetSoftwareByKey(string softwareKey)
        {
            return this.AvailableSoftware.FirstOrDefault(s => s.Key == softwareKey) ??
                   this.AvailableSoftware.FirstOrDefault();
        }

        public void Dispose()
        {
            this.DisposeLaunchConditions();
            this.appliedSoftware = null;
            this.appliedSoftwarePath = null;
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
                this.appliedSoftware.Start(this.appliedSoftwarePath);
            }
        }

        private void ValidateConfig(Config config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (string.IsNullOrEmpty(config.SoftwarePath) ||
                !File.Exists(config.SoftwarePath))
            {
                throw new FileNotFoundException("The software path is invalid", config.SoftwarePath);
            }
        }
    }
}
