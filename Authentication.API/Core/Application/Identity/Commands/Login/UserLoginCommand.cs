namespace Application.Identity.Commands.Login
{
    using System.Threading;
    using System.Threading.Tasks;

    using MediatR;

    using Application.Interfaces;
    using Application.Identity.Common;

    public class UserLoginCommand : UserRequestModel, IRequest<UserResponseModel>
    {
        public UserLoginCommand(string email, string password)
            : base(email, password)
        {
        }

        public class UserLoginCommandHandler : IRequestHandler<UserLoginCommand, UserResponseModel>
        {
            private readonly IIdentity identity;

            public UserLoginCommandHandler(IIdentity identity)
            {
                this.identity = identity;
            }

            public async Task<UserResponseModel> Handle(UserLoginCommand request, CancellationToken cancellationToken)
            {
                var result = await identity.Login(request);

                return result.Data;
            }
        }
    }
}