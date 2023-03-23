namespace Application.Handlers.Account.Comands
{
    using MediatR;

    using Application.Interfaces;
    using Application.Handlers.Account.Common;
    using Application.Handlers.Identity.Common;

    using Shared;

    public class UpdateUserCommand : UserUpdateRequestModel, IRequest<Result<UserResponseModel>>
    {
        public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserResponseModel>>
        {
            private readonly IUserService _userService;

            public UpdateUserCommandHandler(IUserService userService)
            {
                _userService = userService;
            }

            public async Task<Result<UserResponseModel>> Handle(UpdateUserCommand request, CancellationToken cancellationToken) 
            {

            }
        }
    }
}
