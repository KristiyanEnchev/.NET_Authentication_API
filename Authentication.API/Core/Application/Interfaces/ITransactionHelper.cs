namespace Application.Interfaces
{
    using Microsoft.EntityFrameworkCore.Storage;

    public interface ITransactionHelper
    {
        ValueTask<IDbContextTransaction> BeginTransactionAsync();
        IDbContextTransaction BeginTransaction();
    }
}