namespace Infrastructure.Account.Services
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    using AutoMapper;
    using AutoMapper.QueryableExtensions;

    using Domain.Entities.Identity;

    using Shared;

    using Application.Handlers.Account.Common;
    using Application.Interfaces;
    using Application.Common;
    using Application.Extensions;

    public class UserService : IUserService
    {
        private readonly UserManager<User> userManager;
        private readonly IMapper _mapper;
        private readonly ITransactionHelper _transactionHelper;

        public UserService(UserManager<User> userManager, IMapper mapper, ITransactionHelper transactionHelper)
        {
            this.userManager = userManager;
            _mapper = mapper;
            _transactionHelper = transactionHelper;
        }

        public async Task<Result<List<UserResponseGetModel>>> GetListAsync(CancellationToken cancellationToken)
        {
            var users = await userManager.Users
                .AsNoTracking()
                .ProjectTo<UserResponseGetModel>(_mapper.ConfigurationProvider) 
                .ToListAsync(cancellationToken);

            foreach (var userResponse in users)
            {
                var user = await userManager.FindByIdAsync(userResponse.Id);
                var roles = await userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();
                userResponse.Role = role; 
            }

            return Result<List<UserResponseGetModel>>.SuccessResult(users);
        }

        public async Task<PaginatedResult<UserResponseGetModel>> GetPagedListAsync(
            int pageNumber,
            int pageSize,
            string sortBy,
            string order,
            CancellationToken cancellationToken)
        {
            var sortOrder = new UserSortOrder(sortBy, order);

            var paginatedAndSortedUsers = await userManager.Users
                .AsNoTracking()
                .Sort(sortOrder)
                .ProjectTo<UserResponseGetModel>(_mapper.ConfigurationProvider)
                .ToPaginatedListAsync(pageNumber, pageSize, cancellationToken);

            foreach (var userResponse in paginatedAndSortedUsers.Data)
            {
                var user = await userManager.FindByIdAsync(userResponse.Id);
                var roles = await userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();
                userResponse.Role = role;
            }

            return paginatedAndSortedUsers;
        }
    }
}
