namespace AutoGame.Infrastructure.Windows.Tests.Services;

using System;
using System.Collections.Generic;
using AutoGame.Core.Interfaces;
using AutoGame.Infrastructure.Windows.Interfaces;
using AutoGame.Infrastructure.Windows.Services;
using Moq;
using Xunit;

public class WindowServiceTests
{
    private readonly WindowService sut;
    private readonly Mock<IUser32Service> user32Service = new();
    private readonly Mock<IDateTimeService> dateTimeService = new();
    private readonly Mock<ISleepService> sleepService = new();

    private readonly IntPtr currentForegroundWindow = new(1);
    private readonly IntPtr targetForegroundWindow = new(2);

    private readonly Queue<IntPtr> findWindowQueue = new();
    private readonly Queue<IntPtr> getForegroundWindowQueue = new();
    private readonly Queue<DateTime> utcNowQueue = new();

    public WindowServiceTests()
    {
        this.user32Service
            .Setup(x => x.FindWindow(
                It.IsAny<string?>(),
                It.IsAny<string>()))
            .Returns(() =>
                this.findWindowQueue.TryDequeue(out IntPtr r)
                    ? r
                    : IntPtr.Zero);

        this.user32Service
            .Setup(x => x.GetForegroundWindow())
            .Returns(() =>
                this.getForegroundWindowQueue.TryDequeue(out IntPtr r)
                    ? r
                    : IntPtr.Zero);

        this.user32Service
            .Setup(x => x.GetWindowThreadProcessId(
                It.IsAny<IntPtr>(),
                It.IsAny<IntPtr>()))
            .Returns<IntPtr, IntPtr>((hWnd, _) => (uint)hWnd);

        this.dateTimeService
            .SetupGet(x => x.UtcNow)
            .Returns(() =>
                this.utcNowQueue.TryDequeue(out DateTime r)
                    ? r
                    : DateTime.MinValue);

        this.sut = new WindowService(
            this.user32Service.Object,
            this.dateTimeService.Object,
            this.sleepService.Object);
    }

    [Fact]
    public void RepeatTryForceForegroundWindowByTitle_WindowFound_SetsForegroundWindow()
    {
        this.findWindowQueue.Enqueue(this.targetForegroundWindow);
        this.getForegroundWindowQueue.Enqueue(this.currentForegroundWindow);
        this.getForegroundWindowQueue.Enqueue(this.targetForegroundWindow);

        this.sut.RepeatTryForceForegroundWindowByTitle(
            "Title", TimeSpan.FromSeconds(5));

        this.user32Service.Verify(
            x => x.SetForegroundWindow(this.targetForegroundWindow),
            Times.Once);
    }

    [Fact]
    public void RepeatTryForceForegroundWindowByTitle_WindowNotFound_RetrySetForegroundWindow()
    {
        this.findWindowQueue.Enqueue(IntPtr.Zero);
        this.findWindowQueue.Enqueue(this.targetForegroundWindow);
        this.getForegroundWindowQueue.Enqueue(this.currentForegroundWindow);
        this.getForegroundWindowQueue.Enqueue(this.targetForegroundWindow);

        this.sut.RepeatTryForceForegroundWindowByTitle(
            "Title", TimeSpan.FromSeconds(5));

        this.user32Service.Verify(
            x => x.FindWindow(It.IsAny<string?>(), It.IsAny<string>()),
            Times.Exactly(2));

        this.user32Service.Verify(
            x => x.SetForegroundWindow(this.targetForegroundWindow),
            Times.Once);
    }

    [Fact]
    public void RepeatTryForceForegroundWindowByTitle_WindowNeverFound_DontSetForegroundWindow()
    {
        TimeSpan timeout = TimeSpan.FromSeconds(5);

        this.utcNowQueue.Enqueue(DateTime.MinValue);
        this.utcNowQueue.Enqueue(DateTime.MinValue + timeout);

        this.sut.RepeatTryForceForegroundWindowByTitle("Title", timeout);

        this.user32Service.Verify(x => x.SetForegroundWindow(It.IsAny<IntPtr>()), Times.Never);
    }

    [Fact]
    public void RepeatTryForceForegroundWindowByTitle_DoesntStick_RetrySetForegroundWindow()
    {
        this.findWindowQueue.Enqueue(this.targetForegroundWindow);
        this.findWindowQueue.Enqueue(this.targetForegroundWindow);
        this.getForegroundWindowQueue.Enqueue(this.currentForegroundWindow);
        this.getForegroundWindowQueue.Enqueue(this.currentForegroundWindow);
        this.getForegroundWindowQueue.Enqueue(this.currentForegroundWindow);
        this.getForegroundWindowQueue.Enqueue(this.targetForegroundWindow);

        this.sut.RepeatTryForceForegroundWindowByTitle(
            "Title", TimeSpan.FromSeconds(5));

        this.user32Service.Verify(
            x => x.SetForegroundWindow(this.targetForegroundWindow),
            Times.Exactly(2));
    }

    [Fact]
    public void ForceForegroundWindow_ThreadsDontMatch_AttachThreadInput()
    {
        this.findWindowQueue.Enqueue(this.targetForegroundWindow);
        this.getForegroundWindowQueue.Enqueue(this.currentForegroundWindow);
        this.getForegroundWindowQueue.Enqueue(this.targetForegroundWindow);

        this.sut.RepeatTryForceForegroundWindowByTitle(
            "Title", TimeSpan.FromSeconds(5));

        this.user32Service.Verify(
            x => x.AttachThreadInput(It.IsAny<uint>(), It.IsAny<uint>(), true),
            Times.Once);

        this.user32Service.Verify(
            x => x.AttachThreadInput(It.IsAny<uint>(), It.IsAny<uint>(), false),
            Times.Once);
    }

    [Fact]
    public void ForceForegroundWindow_ThreadsMatch_DontAttachThreadInput()
    {
        this.findWindowQueue.Enqueue(this.targetForegroundWindow);
        this.getForegroundWindowQueue.Enqueue(this.targetForegroundWindow);
        this.getForegroundWindowQueue.Enqueue(this.targetForegroundWindow);

        this.sut.RepeatTryForceForegroundWindowByTitle(
            "Title", TimeSpan.FromSeconds(5));

        this.user32Service.Verify(
            x => x.AttachThreadInput(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<bool>()),
            Times.Never);

        this.user32Service.Verify(
            x => x.SetForegroundWindow(It.IsAny<IntPtr>()),
            Times.Once);
    }
}