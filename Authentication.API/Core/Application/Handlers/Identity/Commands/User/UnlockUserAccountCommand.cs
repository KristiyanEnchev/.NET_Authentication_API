﻿namespace Application.Handlers.Identity.Commands.User
{
    using MediatR;

    using Application.Interfaces;

    using Shared;

    public class UnlockUserAccountCommand : IRequest<Result<string>>
    {
        public string? Email { get; set; }

        public class UnlockUserAccountCommandHandler : IRequestHandler<UnlockUserAccountCommand, Result<string>>
        {
            private readonly IIdentity _identity;

            public UnlockUserAccountCommandHandler(IIdentity identity)
            {
                _identity = identity;
            }

            public async Task<Result<string>> Handle(UnlockUserAccountCommand request, CancellationToken cancellationToken) 
            {
                var result = await _identity.UnlockUserAccount(request.Email!);

                return result;
            }
        }
    }
}
