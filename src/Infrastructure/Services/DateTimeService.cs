namespace AutoGame.Infrastructure.Services;

using System;
using AutoGame.Core.Interfaces;

public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
    
    public DateTimeOffset NowOffset => DateTimeOffset.Now;
}