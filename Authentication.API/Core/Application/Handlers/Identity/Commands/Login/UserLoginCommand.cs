namespace Application.Handlers.Identity.Commands.Login
{
    using System.Threading;
    using System.Threading.Tasks;

    using MediatR;

    using Application.Interfaces;

    using Shared;
    using Application.Handlers.Identity.Common;

    public class UserLoginCommand : UserRequestModel, IRequest<Result<UserResponseModel>>
    {
        public UserLoginCommand(string email, string password)
            : base(email, password)
        {
        }

        public class UserLoginCommandHandler : IRequestHandler<UserLoginCommand, Result<UserResponseModel>>
        {
            private readonly IIdentity identity;

            public UserLoginCommandHandler(IIdentity identity)
            {
                this.identity = identity;
            }

            public async Task<Result<UserResponseModel>> Handle(UserLoginCommand request, CancellationToken cancellationToken)
            {
                var result = await identity.Login(request);

                return result;
            }
        }
    }
}