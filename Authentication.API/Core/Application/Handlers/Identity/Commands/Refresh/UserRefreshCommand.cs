namespace Application.Handlers.Identity.Commands.Refresh
{
    using System.Threading;
    using System.Threading.Tasks;

    using MediatR;

    using Application.Interfaces;

    using Shared;
    using Application.Handlers.Identity.Common;

    public class UserRefreshCommand : UserRefreshModel, IRequest<Result<UserResponseModel>>
    {
        public UserRefreshCommand(string refreshToken)
            : base(refreshToken)
        {
        }

        public class UserRefreshCommandHandler : IRequestHandler<UserRefreshCommand, Result<UserResponseModel>>
        {
            private readonly IIdentity identity;

            public UserRefreshCommandHandler(IIdentity identity)
            {
                this.identity = identity;
            }

            public async Task<Result<UserResponseModel>> Handle(UserRefreshCommand request, CancellationToken cancellationToken)
            {
                var result = await identity.RefreshTokenAsync(request.RefreshToken);

                return result;
            }
        }
    }
}