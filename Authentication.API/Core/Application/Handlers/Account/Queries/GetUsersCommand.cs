namespace Application.Handlers.Account.Queries
{
    using MediatR;

    using Application.Handlers.Account.Common;

    using Shared;

    public class GetUsersQuery : UserRequestGetModel, IRequest<Result<UserResponseGetModel>>
    {
        public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<UserResponseGetModel>>
        {
            public GetUsersQueryHandler()
            {
            }

            public async Task<Result<UserResponseGetModel>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
            {
                return null;
            }
        }
    }
}