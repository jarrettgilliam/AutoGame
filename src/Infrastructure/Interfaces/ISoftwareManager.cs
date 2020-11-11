using System;
using System.Collections.Generic;
using System.Text;

namespace AutoGame.Infrastructure.Interfaces
{
    public interface ISoftwareManager
    {
        bool IsRunning { get; }

        void Start();
    }
}
