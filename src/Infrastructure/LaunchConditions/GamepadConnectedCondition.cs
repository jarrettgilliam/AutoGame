using System;
using System.Linq;
using AutoGame.Infrastructure.Interfaces;
using Windows.Gaming.Input;

namespace AutoGame.Infrastructure.LaunchConditions
{
    public class GamepadConnectedCondition : ILaunchCondition
    {
        private readonly object checkConditionLock = new object();

        public GamepadConnectedCondition()
        {
        }

        public event EventHandler ConditionMet;

        public void StartMonitoring()
        {
            Gamepad.GamepadAdded += this.Gamepad_GamepadAdded;
            this.CheckConditionMet();
        }

        private void Gamepad_GamepadAdded(object sender, Gamepad e)
        {
            this.CheckConditionMet();
        }

        public void StopMonitoring()
        {
            Gamepad.GamepadAdded -= this.Gamepad_GamepadAdded;
        }

        private void CheckConditionMet()
        {
            lock (this.checkConditionLock)
            {
                if (Gamepad.Gamepads.Any())
                {
                    this.ConditionMet?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
