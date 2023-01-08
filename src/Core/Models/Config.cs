namespace AutoGame.Core.Models;

using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Config : ObservableObjectWithErrorInfo
{
    private bool isDirty;
    private bool enableTraceLogging;
    private bool startMinimized = true;
    private bool checkForUpdates = true;
    private string? softwareKey;
    private string? softwarePath;
    private string? softwareArguments;
    private bool launchWhenGameControllerConnected;
    private bool launchWhenParsecConnected;

    [JsonIgnore]
    public bool IsDirty
    {
        get => this.isDirty;
        set => this.SetProperty(ref this.isDirty, value);
    }

    public bool EnableTraceLogging
    {
        get => this.enableTraceLogging;
        set => this.SetProperty(ref this.enableTraceLogging, value);
    }

    public bool StartMinimized
    {
        get => this.startMinimized;
        set => this.SetProperty(ref this.startMinimized, value);
    }

    public bool CheckForUpdates
    {
        get => this.checkForUpdates;
        set => this.SetProperty(ref this.checkForUpdates, value);
    }

    public string? SoftwareKey
    {
        get => this.softwareKey;
        set => this.SetProperty(ref this.softwareKey, value);
    }

    public string? SoftwarePath
    {
        get => this.softwarePath;
        set => this.SetProperty(ref this.softwarePath, value);
    }

    public string? SoftwareArguments
    {
        get => this.softwareArguments;
        set => this.SetProperty(ref this.softwareArguments, value);
    }

    public bool LaunchWhenGameControllerConnected
    {
        get => this.launchWhenGameControllerConnected;
        set => this.SetProperty(ref this.launchWhenGameControllerConnected, value);
    }

    public bool LaunchWhenParsecConnected
    {
        get => this.launchWhenParsecConnected;
        set => this.SetProperty(ref this.launchWhenParsecConnected, value);
    }

    public int Version { get; set; }

    [JsonExtensionData] public IDictionary<string, JsonElement>? JsonExtensionData { get; set; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs args)
    {
        base.OnPropertyChanged(args);
        this.SetIsDirty(args);
        this.ClearPropertyErrors(args.PropertyName);
    }

    private void SetIsDirty(PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(this.IsDirty))
        {
            this.IsDirty = true;
        }
    }
}