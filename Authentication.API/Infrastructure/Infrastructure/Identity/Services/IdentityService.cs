namespace Infrastructure.Identity.Services
{
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;

    using Domain.Events;
    using Domain.Entities.Identity;

    using Shared.Exceptions;

    using Application.Interfaces;
    using Application.Identity.Commands.Register;

    internal class IdentityService : IIdentity
    {
        private readonly UserManager<User> userManager;

        public IdentityService(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<string> Register(UserRegisterRequestModel userRequest)
        {
            var checkForEmailExist = await userManager.FindByEmailAsync(userRequest.Email);

            if (checkForEmailExist != null)
            {
                throw new CustomException("Email already in use!", null, System.Net.HttpStatusCode.Conflict);
            }

            var user = new User()
            {
                FirstName = userRequest.FirstName,
                LastName = userRequest.LastName,
                Email = userRequest.Email,
                UserName = userRequest.Email,
                IsActive = true,
            };

            var identityResult = await userManager.CreateAsync(
                user,
                userRequest.Password);

            if (!identityResult.Succeeded)
            {
                var errors = identityResult.Errors.Select(e => e.Description);

                throw new CustomException("Something went wrond when trying to create the user !", errors.ToList(), System.Net.HttpStatusCode.BadRequest);
            }

            var userRegisteredEvent = new UserRegisteredEvent(
                user.Id,
                userRequest.FirstName,
                user.LastName);

            user.AddDomainEvent(userRegisteredEvent);

            return "Succesfull Registration !";
        }
    }
}