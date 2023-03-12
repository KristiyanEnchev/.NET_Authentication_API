namespace Infrastructure.Identity.Services
{
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;

    using Domain.Events;
    using Domain.Entities.Identity;

    using Shared;
    using Shared.Exceptions;

    using Application.Interfaces;
    using Application.Handlers.Identity.Common;
    using Application.Handlers.Identity.Commands.Register;

    internal class IdentityService : IIdentity
    {
        private const string InvalidErrorMessage = "Invalid credentials.";

        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IJwtGenerator jwtGenerator;

        public IdentityService(UserManager<User> userManager, IJwtGenerator jwtGenerator, SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.jwtGenerator = jwtGenerator;
            this.signInManager = signInManager;
        }

        public async Task<Result<string>> Register(UserRegisterRequestModel userRequest)
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
                CreatedBy = "Registration",
                CreatedDate = DateTime.UtcNow,
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

            return Result<string>.SuccessResult("Succesfull Registration !");
        }

        public async Task<Result<UserResponseModel>> Login(UserRequestModel userRequest)
        {
            if (await userManager.FindByEmailAsync(userRequest.Email.Trim().Normalize()) is not { } user
                || !await userManager.CheckPasswordAsync(user, userRequest.Password)
                || !user.IsActive)
            {
                throw new CustomException(InvalidErrorMessage, null, System.Net.HttpStatusCode.Unauthorized);
            }

            SignInResult signInResult = await signInManager.PasswordSignInAsync(user, userRequest.Password, false, lockoutOnFailure: false);

            if (!signInResult.Succeeded)
            {
                throw new CustomException(InvalidErrorMessage, null, System.Net.HttpStatusCode.Unauthorized);
            }

            var tokenResult = await jwtGenerator.GenerateToken(user);

            return Result<UserResponseModel>.SuccessResult(tokenResult);
        }

        public async Task<Result<UserResponseModel>> RefreshTokenAsync(string refreshToken)
        {
            var user = await jwtGenerator.ValidateRefreshToken(refreshToken);

            var tokenResult = await jwtGenerator.GenerateToken(user);

            return Result<UserResponseModel>.SuccessResult(tokenResult);
        }

        public async Task<Result<string>> LogoutAsync(string userEmail)
        {
            var user = await userManager.FindByEmailAsync(userEmail);

            if (user != null)
            {
                await jwtGenerator.RemoveAuthenticationToken(user);
            }

            await signInManager.SignOutAsync();

            return Result<string>.SuccessResult("Succesfull Logout !");
        }
    }
}