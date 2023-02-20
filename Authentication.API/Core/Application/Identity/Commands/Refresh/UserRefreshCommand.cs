namespace Application.Identity.Commands.Refresh
{
    using System.Threading;
    using System.Threading.Tasks;

    using MediatR;

    using Application.Interfaces;
    using Application.Identity.Commands.Common;

    public class UserRefreshCommand : UserRefreshModel, IRequest<UserResponseModel>
    {
        public UserRefreshCommand(string refreshToken)
            : base(refreshToken)
        {
        }

        public class UserRefreshCommandHandler : IRequestHandler<UserRefreshCommand, UserResponseModel>
        {
            private readonly IIdentity identity;

            public UserRefreshCommandHandler(IIdentity identity)
            {
                this.identity = identity;
            }

            public async Task<UserResponseModel> Handle(UserRefreshCommand request, CancellationToken cancellationToken)
            {
                var result = await identity.RefreshTokenAsync(request.RefreshToken);

                return result.Data;
            }
        }
    }
}