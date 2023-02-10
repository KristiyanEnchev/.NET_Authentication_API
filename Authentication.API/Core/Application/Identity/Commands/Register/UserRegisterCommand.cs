namespace Application.Identity.Commands.Register
{
    using System.Threading;
    using System.Threading.Tasks;

    using MediatR;

    using Application.Interfaces;

    public class UserRegisterCommand : UserRegisterRequestModel, IRequest<string>
    {
        public UserRegisterCommand(string firstName, string lastName, string email, string password, string confirmPassword)
            : base(firstName, lastName, email, password) 
        {
            ConfirmPassword = confirmPassword;
        }

        public string ConfirmPassword { get; }

        public class UserRegisterCommandHandler : IRequestHandler<UserRegisterCommand, string>
        {
            private readonly IIdentity identity;

            public UserRegisterCommandHandler(IIdentity identity)
            {
                this.identity = identity;
            }

            public async Task<string> Handle(UserRegisterCommand request, CancellationToken cancellationToken)
            {
                var result = await identity.Register(request);

                return result;
            }
        }
    }
}