namespace Application.Handlers.Account.Queries
{
    using MediatR;

    using Application.Interfaces;
    using Application.Handlers.Account.Common;

    using Shared;

    public class GetUsersQuery : UserRequestGetModel, IRequest<Result<List<UserResponseGetModel>>>
    {
        public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<List<UserResponseGetModel>>>
        {
            private readonly IUserService userService;

            public GetUsersQueryHandler(IUserService userService)
            {
                this.userService = userService;
            }

            public async Task<Result<List<UserResponseGetModel>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
            {
                var result = await userService.GetListAsync(cancellationToken);

                return result;
            }
        }
    }
}