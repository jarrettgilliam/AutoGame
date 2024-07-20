namespace AutoGame.Core.Tests.Models;

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AutoGame.Core.Models;

public class ObservableObjectWithErrorInfoTests
{
    private readonly List<string?> errorsChangedEvents = [];

    private readonly ObservableObjectWithErrorInfoImpl sut;

    public ObservableObjectWithErrorInfoTests()
    {
        this.sut = new ObservableObjectWithErrorInfoImpl();
        this.sut.ErrorsChanged += this.SutOnErrorsChanged;
    }

    [Fact]
    public void HasErrors_ReturnsTrue()
    {
        this.sut.AddError(nameof(this.sut.Property1), "test error");
        Assert.True(this.sut.HasErrors);
    }

    [Fact]
    public void HasErrors_ReturnsFalse()
    {
        Assert.False(this.sut.HasErrors);
    }

    [Fact]
    public void GetErrors_Returns_Empty_Collection_When_No_Errors()
    {
        var errors = this.sut.GetErrors(nameof(this.sut.Property1));

        Assert.NotNull(errors);
        Assert.Empty(errors);
    }

    [Fact]
    public void GetErrors_Returns_Added_Errors()
    {
        string expectedError = "test error";
        this.sut.AddError(nameof(this.sut.Property1), expectedError);

        var errors = this.sut.GetErrors(nameof(this.sut.Property1)).Cast<string>();

        Assert.Collection(errors,
            x => Assert.Equal(x, expectedError));
    }

    [Fact]
    public void AddErrors_Works()
    {
        var expectedErrors = new[]
        {
            "error 1",
            "error 2"
        };

        this.sut.AddErrors(nameof(this.sut.Property1), expectedErrors);
        var actualErrors = this.sut.GetErrors(nameof(this.sut.Property1)).Cast<string>();

        Assert.Collection(actualErrors,
            s => Assert.Equal(s, expectedErrors[0]),
            s => Assert.Equal(s, expectedErrors[1]));
    }

    [Fact]
    public void AddErrors_Fires_ErrorsChanged_Once()
    {
        this.sut.AddErrors(nameof(this.sut.Property1), new[]
        {
            "error 1",
            "error 2"
        });

        Assert.Single(this.errorsChangedEvents);
    }

    [Fact]
    public void AddError_Fires_ErrorsChanged()
    {
        this.sut.AddError(nameof(this.sut.Property1), "test error");
        Assert.Single(this.errorsChangedEvents);
    }

    [Fact]
    public void ClearAllErrors_Works()
    {
        this.sut.AddError(nameof(this.sut.Property1), "test error");
        this.sut.ClearAllErrors();
        Assert.False(this.sut.HasErrors);
    }

    [Fact]
    public void ClearAllErrors_Fires_ErrorsChanged()
    {
        this.sut.AddError(nameof(this.sut.Property1), "test error");
        this.sut.ClearAllErrors();
        Assert.Equal(2, this.errorsChangedEvents.Count);
    }

    [Fact]
    public void ClearPropertyErrors_Works()
    {
        this.sut.AddError(nameof(this.sut.Property1), "test error");
        this.sut.ClearPropertyErrors(nameof(this.sut.Property1));
        Assert.False(this.sut.GetErrors(nameof(this.sut.Property1)).Cast<string>().Any());
    }

    [Fact]
    public void ClearPropertyErrors_Fires_Errors_Changed()
    {
        this.sut.AddError(nameof(this.sut.Property1), "test error");
        this.sut.ClearPropertyErrors(nameof(this.sut.Property1));
        Assert.Equal(2, this.errorsChangedEvents.Count);
    }

    private void SutOnErrorsChanged(object? sender, DataErrorsChangedEventArgs e) =>
        this.errorsChangedEvents.Add(e.PropertyName);

    private class ObservableObjectWithErrorInfoImpl : ObservableObjectWithErrorInfo
    {
        private string? property1;

        public string? Property1
        {
            get => this.property1;
            set => this.SetProperty(ref this.property1, value);
        }
    }
}