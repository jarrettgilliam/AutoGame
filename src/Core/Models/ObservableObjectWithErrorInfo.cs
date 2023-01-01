namespace AutoGame.Core.Models;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

public abstract class ObservableObjectWithErrorInfo : ObservableObject, INotifyDataErrorInfo
{
    private readonly ConcurrentDictionary<string, IList<string>> allErrors = new();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    [JsonIgnore] public bool HasErrors => this.allErrors.Any();

    public IEnumerable GetErrors(string? propertyName) =>
        this.allErrors.GetValueOrDefault(propertyName ?? "") ?? Array.Empty<string>();

    public void AddErrors(string propertyName, IEnumerable<string> errors)
    {
        IList<string> propErrors = this.allErrors.GetOrAdd(propertyName, new List<string>());

        foreach (string error in errors)
        {
            propErrors.Add(error);
        }

        this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    public void AddError(string propertyName, string error)
    {
        IList<string> propErrors = this.allErrors.GetOrAdd(propertyName, new List<string>());

        propErrors.Add(error);

        this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    public void ClearAllErrors()
    {
        string[] propertyNames = this.allErrors.Keys.ToArray();
        this.allErrors.Clear();

        foreach (string propertyName in propertyNames)
        {
            this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    public void ClearPropertyErrors(string? propertyName)
    {
        if (propertyName != null && this.allErrors.TryRemove(propertyName, out _))
        {
            this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}