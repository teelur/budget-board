namespace BudgetBoard.Database.Interfaces;

public interface ILargeObjectStore
{
    Task<long> WriteLargeObjectAsync(long objectId, byte[] data);

    Task<byte[]?> ReadLargeObjectAsync(long objectId);
}
