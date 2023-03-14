namespace Persistence.Contexts
{
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public interface IContext
    {
        DatabaseFacade Database { get; }
    }
}
