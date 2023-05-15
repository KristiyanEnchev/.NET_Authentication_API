namespace Application.Interfaces
{
    using System.Threading.Tasks;

    using Application.Handlers.Identity.Common;
    using Application.Handlers.Identity.Commands.Register;

    using Shared;

    public interface IIdentity
    {
        Task<Result<string>> Register(UserRegisterRequestModel userRequest);
        Task<Result<UserResponseModel>> Login(UserRequestModel userRequest);
        Task<Result<UserResponseModel>> RefreshTokenAsync(string refreshToken);
        Task<Result<string>> LogoutAsync(string userEmail);

        Task<Result<string>> ConfirmEmail(string userEmail, string code, string otp);
        Task<Result<string>> UnlockUserAccount(string userEmail);
        Task<Result<string>> EnableTwoFactorAuthentication(string userEmail);
        Task<Result<string>> DisableTwoFactorAuthentication(string userEmail);
    }
}