using Newtonsoft.Json;
using Prism.Mvvm;
using System.ComponentModel;

namespace AutoGame.Models
{
    public class Config : BindableBase
    {
        private bool isDirty;
        private string softwareKey;
        private string softwarePath;
        private bool launchWhenGamepadConnected;
        private bool launchWhenParsecConnected;

        public Config()
        {
            this.PropertyChanged += this.SetIsDirty;
        }

        [JsonIgnore]
        public bool IsDirty
        {
            get => this.isDirty;
            set => this.SetProperty(ref this.isDirty, value);
        }

        public string SoftwareKey
        {
            get => this.softwareKey;
            set => this.SetProperty(ref this.softwareKey, value);
        }

        public string SoftwarePath
        {
            get => this.softwarePath;
            set => this.SetProperty(ref this.softwarePath, value);
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
