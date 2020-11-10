using System;
using System.Collections.Generic;
using System.Text;

namespace Parscript.Infrastructure.Interfaces
{
    public interface ISoftwareManager
    {
        bool IsRunning { get; }

        void Start();
        
        void Stop();
    }
}
