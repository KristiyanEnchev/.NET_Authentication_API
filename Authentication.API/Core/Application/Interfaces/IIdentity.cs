namespace Application.Interfaces
{
    using System.Threading.Tasks;

    using Application.Identity.Commands.Register;

    public interface IIdentity
    {
        Task<string> Register(UserRegisterRequestModel userRequest);
    }
}