using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoGame.Infrastructure.Interfaces;
using AutoGame.Infrastructure.Models;

namespace AutoGame.Infrastructure.Services
{
    public sealed class AutoGameService : IAutoGameService
    {
        private ISoftwareManager appliedSoftware;
        private string appliedSoftwarePath;
        private IList<ILaunchCondition> appliedLaunchConditions;

        public AutoGameService(
            IList<ISoftwareManager> availableSoftware,
            ILaunchCondition gamepadConnectedCondition,
            ILaunchCondition parsecConnectedCondition)
        {
            this.AvailableSoftware = availableSoftware;
            this.GamepadConnectedCondition = gamepadConnectedCondition;
            this.ParsecConnectedCondition = parsecConnectedCondition;
        }

        public IList<ISoftwareManager> AvailableSoftware { get; }

        private ILaunchCondition GamepadConnectedCondition { get; }

        private ILaunchCondition ParsecConnectedCondition { get; }

        public void ApplyConfiguration(Config config)
        {
            this.ValidateConfig(config);

            this.appliedSoftware = this.GetSoftwareByKey(config.SoftwareKey);
            this.appliedSoftwarePath = config.SoftwarePath;

            this.StopMonitoringAllLaunchConditions();

            this.appliedLaunchConditions = new List<ILaunchCondition>();

            if (config.LaunchWhenGamepadConnected)
            {
                this.appliedLaunchConditions.Add(this.GamepadConnectedCondition);
            }

            if (config.LaunchWhenParsecConnected)
            {
                this.appliedLaunchConditions.Add(this.ParsecConnectedCondition);
            }

            foreach (ILaunchCondition condition in this.appliedLaunchConditions)
            {
                condition.ConditionMet += this.OnLaunchConditionMet;
                condition.StartMonitoring();
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
            this.StopMonitoringAllLaunchConditions();
            this.appliedSoftware = null;
            this.appliedSoftwarePath = null;
        }

        private void StopMonitoringAllLaunchConditions()
        {
            if (this.appliedLaunchConditions != null)
            {
                foreach (ILaunchCondition condition in this.appliedLaunchConditions)
                {
                    condition.ConditionMet -= this.OnLaunchConditionMet;
                    condition.StopMonitoring();
                }

                this.appliedLaunchConditions = null;
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
