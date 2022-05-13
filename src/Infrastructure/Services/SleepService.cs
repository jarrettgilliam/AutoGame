namespace AutoGame.Infrastructure.Services;

using System;
using System.Threading;
using AutoGame.Core.Interfaces;

public class SleepService : ISleepService
{
    public void Sleep(TimeSpan timeout) => Thread.Sleep(timeout);
}