namespace AutoGame.Infrastructure.Services;

using System;
using AutoGame.Core.Interfaces;

internal sealed class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
    
    public DateTimeOffset NowOffset => DateTimeOffset.Now;
}