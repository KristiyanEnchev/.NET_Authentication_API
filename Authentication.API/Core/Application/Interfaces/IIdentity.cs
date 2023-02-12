namespace Application.Interfaces
{
    using System.Threading.Tasks;

    using Application.Identity.Commands.Register;

    using Shared;

    public interface IIdentity
    {
        Task<Result<string>> Register(UserRegisterRequestModel userRequest);
    }
}