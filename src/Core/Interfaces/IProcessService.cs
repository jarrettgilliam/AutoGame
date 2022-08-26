﻿namespace AutoGame.Core.Interfaces;

public interface IProcessService
{
    IProcess NewProcess();
    
    IProcess Start(string fileName, string? arguments);

    IDisposableList<IProcess> GetProcessesByName(string? processName);
}