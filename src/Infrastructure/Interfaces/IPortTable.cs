namespace AutoGame.Infrastructure.Interfaces;

public interface IPortTable<out TRow>
{
    uint NumEntries { get; }
    TRow[] Table { get; }
}