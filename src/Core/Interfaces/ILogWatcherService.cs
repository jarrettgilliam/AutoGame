namespace AutoGame.Core.Interfaces;

using System.Collections.Generic;
using AutoGame.Core.Delegates;

public interface ILogWatcherService
{
    event EventHandler<ILogWatcherService, IEnumerable<string>> LogEntriesAdded;
    void StartMonitoring(string filePath);
    void StopMonitoring();
}