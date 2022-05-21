namespace AutoGame.Core.Tests.Models;

using System.ComponentModel;
using System.Reflection;
using AutoGame.Core.Models;

public class ConfigTests
{
    private Config sut;
    private int propertyChangedFireCount;
    private int errorsChangedFireCount;

    public ConfigTests()
    {
        this.sut = new Config();
        this.sut.PropertyChanged += this.ConfigOnPropertyChanged;
        this.sut.ErrorsChanged += this.OnErrorsChanged;
    }

    [Theory]
    [InlineData(nameof(Config.EnableTraceLogging), true)]
    [InlineData(nameof(Config.SoftwareKey), "TheKey")]
    [InlineData(nameof(Config.SoftwarePath), "ThePath")]
    [InlineData(nameof(Config.LaunchWhenGamepadConnected), true)]
    [InlineData(nameof(Config.LaunchWhenParsecConnected), true)]
    public void Property_Works(string propertyName, object newValue)
    {
        PropertyInfo propertyInfo =
            typeof(Config).GetProperty(propertyName)
            ?? throw new ArgumentException($"Invalid property '{propertyName}'");

        this.sut.AddError(propertyName, "Error");

        propertyInfo.SetValue(this.sut, propertyInfo.GetValue(this.sut));

        Assert.NotEqual(newValue, propertyInfo.GetValue(this.sut));
        Assert.False(this.sut.IsDirty);
        Assert.Equal(0, this.propertyChangedFireCount);
        Assert.Equal(1, this.errorsChangedFireCount);
        Assert.True(this.sut.GetErrors(propertyName).Cast<string>().Any());

        propertyInfo.SetValue(this.sut, newValue);

        Assert.Equal(newValue, propertyInfo.GetValue(this.sut));
        Assert.True(this.sut.IsDirty);
        Assert.Equal(2, this.propertyChangedFireCount);
        Assert.Equal(2, this.errorsChangedFireCount);
        Assert.False(this.sut.GetErrors(propertyName).Cast<string>().Any());
    }

    [Fact]
    public void HasErrors_ReturnsTrue()
    {
        this.sut.AddError("", "");
        Assert.True(this.sut.HasErrors);
    }

    [Fact]
    public void HasErrors_ReturnsFalse()
    {
        Assert.False(this.sut.HasErrors);
    }

    [Fact]
    public void ClearAllErrors_Works()
    {
        this.sut.AddError("", "");
        this.sut.ClearAllErrors();
        Assert.False(this.sut.HasErrors);
    }

    private void ConfigOnPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        this.propertyChangedFireCount++;

    private void OnErrorsChanged(object? sender, DataErrorsChangedEventArgs e) =>
        this.errorsChangedFireCount++;
}