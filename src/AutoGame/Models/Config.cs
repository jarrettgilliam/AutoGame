using Newtonsoft.Json;
using Prism.Mvvm;
using System.ComponentModel;
using System.Linq;

namespace AutoGame.Models
{
    public class Config : BindableBase
    {
        private bool isDirty;
        private string gameLauncher;
        private bool launchWhenGamepadConnected;
        private bool launchWhenParsecConnected;

        public Config()
        {
            this.GameLauncher = Constants.GameLaunchers.FirstOrDefault().Key;
            this.LaunchWhenGamepadConnected = true;
            this.LaunchWhenParsecConnected = true;

            this.PropertyChanged += this.SetIsDirty;
        }

        [JsonIgnore]
        public bool IsDirty
        {
            get => this.isDirty;
            set => this.SetProperty(ref this.isDirty, value);
        }

        public string GameLauncher
        {
            get => this.gameLauncher;
            set => this.SetProperty(ref this.gameLauncher, value);
        }

        public bool LaunchWhenGamepadConnected
        {
            get => this.launchWhenGamepadConnected;
            set => this.SetProperty(ref this.launchWhenGamepadConnected, value);
        }

        public bool LaunchWhenParsecConnected
        {
            get => this.launchWhenParsecConnected;
            set => this.SetProperty(ref this.launchWhenParsecConnected, value);
        }

        private void SetIsDirty(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(this.IsDirty))
            {
                this.IsDirty = true;
            }
        }
    }
}
