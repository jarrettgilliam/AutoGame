namespace AutoGame.Infrastructure.Windows.Interfaces;

public interface IPortTable<out TRow>
{
    uint NumEntries { get; }
    TRow[] Table { get; }
}