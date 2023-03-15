namespace Infrastructure.Identity.Services
{
    using System.Threading.Tasks;

    using Application.Handlers.Identity.Common;

    using Domain.Entities.Identity;

    public interface IJwtGenerator
    {
        Task<UserResponseModel> GenerateToken(User user);
        Task<User> ValidateRefreshToken(string refreshToken);
        Task RemoveAuthenticationToken(User user);
    }
}