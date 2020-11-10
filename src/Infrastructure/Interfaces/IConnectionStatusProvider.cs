using Parscript.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Parscript.Infrastructure.Interfaces
{
    public interface IConnectionStatusProvider : IDisposable
    {
        Task<ConnectionStatus> ProvideStatus(ConnectionStatus currentStatus);
    }
}
