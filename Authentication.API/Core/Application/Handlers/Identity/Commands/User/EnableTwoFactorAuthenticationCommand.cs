namespace Application.Handlers.Identity.Commands.User
{
    using MediatR;

    using Application.Interfaces;

    using Shared;

    public class EnableTwoFactorAuthenticationCommand : IRequest<Result<string>>
    {
        public string? Email { get; set; }

        public class EnableTwoFactorAuthenticationCommandHandler : IRequestHandler<EnableTwoFactorAuthenticationCommand, Result<string>>
        {
            private readonly IIdentity _identity;

            public EnableTwoFactorAuthenticationCommandHandler(IIdentity identity)
            {
                _identity = identity;
            }

            public async Task<Result<string>> Handle(EnableTwoFactorAuthenticationCommand request, CancellationToken cancellationToken)
            {
                var result = await _identity.EnableTwoFactorAuthentication(request.Email!);

                return result;
            }
        }
    {
    }
}
