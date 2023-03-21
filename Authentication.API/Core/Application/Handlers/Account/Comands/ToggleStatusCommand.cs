namespace Application.Handlers.Account.Comands
{
    using MediatR;

    using Models.Enums;

    using Application.Interfaces;

    using Shared;

    public class ToggleStatusCommand : IRequest<Result<string>>
    {
        public string Value { get; set; }
        public ToggleUserValue ToggleValue { get; set; }
        public bool ToggleTo { get; set; }

        public ToggleStatusCommand(string value, ToggleUserValue toggleUserValue, bool toggleTo)
        {
            this.Value = value;
            this.ToggleValue = toggleUserValue;
            this.ToggleTo = toggleTo;
        }

        public class ToggleStatusCommandHandler : IRequestHandler<ToggleStatusCommand, Result<string>>
        {
            private readonly IUserService userService;

            public ToggleStatusCommandHandler(IUserService userService)
            {
                this.userService = userService;
            }

            public async Task<Result<string>> Handle(ToggleStatusCommand request, CancellationToken cancellationToken)
            {
                var result = await userService.ToggleStatusAsync(request.Value, request.ToggleValue, request.ToggleTo, cancellationToken);

                return result;
            }
        }
    }
}