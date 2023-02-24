namespace Application.Identity.Commands.Logout
{
    using System.Threading;
    using System.Threading.Tasks;

    using MediatR;

    using Application.Interfaces;
    using Application.Identity.Common;

    public class UserLogoutCommand : UserLogoutModel, IRequest<bool>
    {
        public UserLogoutCommand(string email)
            : base(email)
        {
        }

        public class UserLogoutCommandHandler : IRequestHandler<UserLogoutCommand, bool>
        {
            private readonly IIdentity identity;

            public UserLogoutCommandHandler(IIdentity identity)
            {
                this.identity = identity;
            }

            public async Task<bool> Handle(UserLogoutCommand request, CancellationToken cancellationToken)
            {
                var result = await identity.LogoutAsync(request.Email);

                return result.Succeeded ? true : false;
            }
        }
    }
}