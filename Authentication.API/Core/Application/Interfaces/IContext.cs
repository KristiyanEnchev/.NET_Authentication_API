namespace Application.Interfaces
{
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public interface IContext
    {
        DatabaseFacade Database { get; }
    }
}
