namespace AutoGame.Core.Interfaces;

using System.Collections.Generic;

public interface ISoftwareCollection : IEnumerable<ISoftwareManager>
{
    ISoftwareManager? GetSoftwareByKeyOrNull(string? softwareKey);
}