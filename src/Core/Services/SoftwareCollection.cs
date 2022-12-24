namespace AutoGame.Core.Services;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoGame.Core.Interfaces;

public class SoftwareCollection : Collection<ISoftwareManager>, ISoftwareCollection
{
    public SoftwareCollection(IEnumerable<ISoftwareManager> availableSoftware)
    {
        foreach (ISoftwareManager softwareManager in availableSoftware)
        {
            this.Add(softwareManager);
        }
    }

    public virtual ISoftwareManager? GetSoftwareByKeyOrNull(string? softwareKey) =>
        this.FirstOrDefault(s => s.Key == softwareKey);
}