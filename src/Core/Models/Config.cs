namespace AutoGame.Core.Models;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

public class Config : ObservableObject, INotifyDataErrorInfo
{
    private readonly Dictionary<string, IEnumerable<string>> allErrors = new();
    private bool isDirty;
    private bool enableTraceLogging;
    private bool startMinimized = true;
    private string? softwareKey;
    private string? softwarePath;
    private string? softwareArguments;
    private bool launchWhenGameControllerConnected;
    private bool launchWhenParsecConnected;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    [JsonIgnore]
    public bool IsDirty
    {
        get => this.isDirty;
        set => this.SetProperty(ref this.isDirty, value);
    }

    [JsonIgnore] public bool HasErrors => this.allErrors.Any();

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

    public IEnumerable GetErrors(string? propertyName) =>
        this.allErrors.GetValueOrDefault(propertyName ?? "") ?? Array.Empty<string>();

    public void AddError(string propertyName, string error)
    {
        List<string> propErrors = this.GetErrors(propertyName).Cast<string>().ToList();

        propErrors.Add(error);

        this.allErrors[propertyName] = propErrors;

        this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs args)
    {
        base.OnPropertyChanged(args);
        this.SetIsDirty(args);
        this.ClearPropertyErrors(args.PropertyName);
    }

    private void ClearPropertyErrors(string? propertyName = null)
    {
        if (propertyName != null && this.allErrors.Remove(propertyName))
        {
            this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    public void ClearAllErrors()
    {
        List<string> propertyNames = this.allErrors.Keys.ToList();
        this.allErrors.Clear();

        foreach (string propertyName in propertyNames)
        {
            this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    private void SetIsDirty(PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(this.IsDirty))
        {
            this.IsDirty = true;
        }
    }
}