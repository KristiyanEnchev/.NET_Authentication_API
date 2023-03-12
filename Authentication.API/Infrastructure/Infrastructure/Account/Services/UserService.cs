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
    using Persistence.Constants;

    public class UserService : IUserService
    {
        private readonly UserManager<User> userManager;
        private readonly IMapper _mapper;


        public UserService(UserManager<User> userManager, IMapper mapper)
        {
            this.userManager = userManager;
            _mapper = mapper;
        }

        public async Task<Result<List<UserResponseGetModel>>> GetListAsync(CancellationToken cancellationToken)
        {
            var users = await userManager.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var userResponses = new List<UserResponseGetModel>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();

                var userResponse = _mapper.Map<UserResponseGetModel>(user);
                userResponse.Role = role; 

                userResponses.Add(userResponse);
            }

            return Result<List<UserResponseGetModel>>.SuccessResult(userResponses);
        }
    }
}
