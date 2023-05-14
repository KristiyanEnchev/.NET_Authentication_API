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
    using Microsoft.AspNetCore.WebUtilities;
    using System.Text;

    internal class IdentityService : IIdentity
    {
        private const string InvalidErrorMessage = "Invalid credentials.";

        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IJwtGenerator jwtGenerator;
        private readonly ITransactionHelper transactionHelper;

        public IdentityService(UserManager<User> userManager, IJwtGenerator jwtGenerator, SignInManager<User> signInManager, ITransactionHelper transactionHelper)
        {
            this.userManager = userManager;
            this.jwtGenerator = jwtGenerator;
            this.signInManager = signInManager;
            this.transactionHelper = transactionHelper;
        }

        public async Task<Result<string>> Register(UserRegisterRequestModel userRequest)
        {
            var userExists = await userManager.FindByEmailAsync(userRequest.Email);

            if (userExists != null)
            {
                return Result<string>.Failure("Email already in use.");
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

            using (var transaction = await transactionHelper.BeginTransactionAsync())
            {
                var createUserResult = await userManager.CreateAsync(user, userRequest.Password);
                if (!createUserResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    var errors = createUserResult.Errors.Select(e => e.Description).ToList();
                    return Result<string>.Failure(errors);
                }

                var addToRoleResult = await userManager.AddToRoleAsync(user, "User");
                if (!addToRoleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    var errors = addToRoleResult.Errors.Select(e => e.Description).ToList();
                    return Result<string>.Failure(errors);
                }

                await transaction.CommitAsync();
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
                var errors = new List<string> { InvalidErrorMessage };
                return Result<UserResponseModel>.Failure(errors);
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

        public async Task<Result<string>> ConfirmEmail(string userEmail, string code)
        {
            using (var transaction = await transactionHelper.BeginTransactionAsync())
            {
                var user = await userManager.FindByEmailAsync(userEmail);
                if (user == null || code == null) 
                {
                    return Result<string>.Failure("User not found.");
                }

                var result = await userManager.ConfirmEmailAsync(user, code);
                if (result.Succeeded) 
                {
                    await transaction.RollbackAsync();
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return Result<string>.Failure(errors);
                }

                await transaction.CommitAsync();

                return Result<string>.SuccessResult($"Email confirmed :{userEmail}");
            }
        }

        public async Task<Result<string>> SendVerificationEmailAsync(string email, string origin)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Result<string>.Failure("User not found.");
            }

            string emailVerificationUri = await GetEmailVerificationUriAsync(user, origin);

            // here needs to go the Email sending logic

            return Result<string>.SuccessResult(emailVerificationUri);
            //return Result<string>.SuccessResult($"Email sent to {email}");
        }

        public async Task<string> GetEmailVerificationUriAsync(User user, string origin)
        {
            string code = await userManager.GenerateEmailConfirmationTokenAsync(user);

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            string route = "/confirmEmail";

            var endpointUri = new Uri(string.Concat($"{origin}/", route));

            string verificationUri = QueryHelpers.AddQueryString(endpointUri.ToString(), "email", user.Email!);

            verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);

            //verificationUri = QueryHelpers.AddQueryString(verificationUri, "otp", Otp);

            return verificationUri;
        }

        public async Task<Result<string>> UnlockUserAccount(string userEmail)
        {
            using (var transaction = await transactionHelper.BeginTransactionAsync())
            {
                var user = await userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    return Result<string>.Failure("User not found.");
                }

                var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return Result<string>.Failure(errors);
                }

                await transaction.CommitAsync();

                return Result<string>.SuccessResult($"User account unlocked :{userEmail}");
            }
        }

        public async Task<Result<string>> EnableTwoFactorAuthentication(string userEmail)
        {
            using (var transaction = await transactionHelper.BeginTransactionAsync())
            {
                var user = await userManager.FindByIdAsync(userEmail);
                if (user == null)
                {
                    return Result<string>.Failure("User not found.");
                }

                var result = await userManager.SetTwoFactorEnabledAsync(user, true);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return Result<string>.Failure(errors);
                }

                await transaction.CommitAsync();

                return Result<string>.SuccessResult($"2FA enabled for :{userEmail}");
            }
        }

        public async Task<Result<string>> DisableTwoFactorAuthentication(string userEmail)
        {
            using (var transaction = await transactionHelper.BeginTransactionAsync())
            {
                var user = await userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    return Result<string>.Failure("User not found.");
                }

                var result = await userManager.SetTwoFactorEnabledAsync(user, false);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return Result<string>.Failure(errors);
                }

                await transaction.CommitAsync();

                return Result<string>.SuccessResult($"2FA disabled for :{userEmail}");
            }
        }
    }
}