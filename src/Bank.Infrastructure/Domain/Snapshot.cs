namespace Bank.Infrastructure.Domain
{
    public interface ISnapshot
    {
        long SnapshotStreamVersion { get; set; }
    }
}
