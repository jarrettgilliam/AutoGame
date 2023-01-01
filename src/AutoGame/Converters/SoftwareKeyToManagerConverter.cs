namespace AutoGame.Converters;

using System;
using System.Globalization;
using AutoGame.Core.Interfaces;
using Avalonia;
using Avalonia.Data.Converters;

public sealed class SoftwareKeyToManagerConverter : AvaloniaObject, IValueConverter
{
    public static readonly StyledProperty<ISoftwareCollection?> AvailableSoftwareProperty =
        AvaloniaProperty.Register<SoftwareKeyToManagerConverter, ISoftwareCollection?>(nameof(AvailableSoftware));

    public ISoftwareCollection? AvailableSoftware
    {
        get => this.GetValue(AvailableSoftwareProperty);
        set => this.SetValue(AvailableSoftwareProperty, value);
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        this.AvailableSoftware?.GetSoftwareByKeyOrNull(value as string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is ISoftwareManager software ? software.Key : null;
}