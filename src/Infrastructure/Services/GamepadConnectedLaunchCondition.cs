using AutoGame.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Windows.Gaming.Input;

namespace AutoGame.Infrastructure.Services
{
    public class GamepadConnectedLaunchCondition : ILaunchCondition
    {
        private readonly object checkConditionLock = new object();

        public GamepadConnectedLaunchCondition()
        {
        }

        public event EventHandler ConditionMet;

        public void StartCheckingConditions()
        {
            Gamepad.GamepadAdded += this.Gamepad_GamepadAdded;
            this.CheckConditionMet();
        }

        private void Gamepad_GamepadAdded(object sender, Windows.Gaming.Input.Gamepad e)
        {
            this.CheckConditionMet();
        }

        public void Dispose()
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
