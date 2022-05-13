﻿namespace AutoGame.Core.Models;

using Prism.Mvvm;
using System.Collections;
using System.ComponentModel;
using Newtonsoft.Json;

public class Config : BindableBase, INotifyDataErrorInfo
{
    private bool isDirty;
    private readonly Dictionary<string, IEnumerable<string>> allErrors;
    private bool enableTraceLogging;
    private string? softwareKey;
    private string? softwarePath;
    private bool launchWhenGamepadConnected;
    private bool launchWhenParsecConnected;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public Config()
    {
        this.PropertyChanged += this.SetIsDirty;
        this.allErrors = new Dictionary<string, IEnumerable<string>>();
    }

    [JsonIgnore]
    public bool IsDirty
    {
        get => this.isDirty;
        set => this.SetProperty(ref this.isDirty, value);
    }

    [JsonIgnore]
    public bool HasErrors => this.allErrors.Any();

    public bool EnableTraceLogging
    {
        get => this.enableTraceLogging;
        set => this.SetProperty(ref this.enableTraceLogging, value);
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
        this.ClearPropertyErrors(args.PropertyName);
    }

    public void ClearPropertyErrors(string? propertyName = null)
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

    private void SetIsDirty(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(this.IsDirty))
        {
            this.IsDirty = true;
        }
    }
}