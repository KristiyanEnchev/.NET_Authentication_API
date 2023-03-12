namespace Application.Interfaces
{
    using Application.Handlers.Account.Common;

    using Shared;

    public interface IUserService
    {
        Task<Result<List<UserResponseGetModel>>> GetListAsync(CancellationToken cancellationToken);
    }
}