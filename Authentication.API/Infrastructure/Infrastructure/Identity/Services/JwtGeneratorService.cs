namespace Infrastructure.Identity.Services
{
    using System.Text;
    using System.Threading.Tasks;
    using System.Security.Claims;
    using System.IdentityModel.Tokens.Jwt;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    using Domain.Entities.Identity;

    using Application.Interfaces;
    using Application.Identity.Common;

    using Persistence.Constants;

    using Models;

    using Shared.Exceptions;

    internal class JwtGeneratorService : IJwtGenerator
    {
        private const string InvalidToken = "Invalit refresh token !";

        private readonly IDateTimeService dateTime;
        private readonly UserManager<User> userManager;
        private readonly ApplicationSettings applicationSettings;

        public JwtGeneratorService(IDateTimeService dateTime, UserManager<User> userManager, IOptions<ApplicationSettings> applicationSettings)
        {
            this.dateTime = dateTime;
            this.userManager = userManager;
            this.applicationSettings = applicationSettings.Value;
        }

        public async Task<UserResponseModel> GenerateToken(User user)
        {
            var token = await GenerateAccessToken(user);

            var newRefreshToken = await GenerateRefreshToken(user);
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(applicationSettings.RefreshTokenExpirationInDays);

            await userManager.UpdateAsync(user);

            var tokenResult = new UserResponseModel(token, user.RefreshTokenExpiryTime, newRefreshToken);

            return tokenResult;
        }

        public async Task<string> GenerateRefreshToken(User user)
        {
            await RemoveAuthenticationToken(user);
            string newRefreshToken = await GenerateRefreshTokenWithSecret(user);
            IdentityResult result = await userManager.SetAuthenticationTokenAsync(user, applicationSettings.LoginProvider, applicationSettings.TokenNames.RefreshToken, newRefreshToken);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);

                throw new CustomException(InvalidToken, errors.ToList(), System.Net.HttpStatusCode.Unauthorized);
            }

            return newRefreshToken;
        }

        public async Task<User> ValidateRefreshToken(string refreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken validatedToken;
            try
            {
                var _tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(applicationSettings.RefreshSecret!)),
                    ValidateAudience = false,
                    ValidateIssuer = false,
                };

                var principal = tokenHandler.ValidateToken(refreshToken, _tokenValidationParameters, out validatedToken);
            }
            catch (Exception ex)
            {
                throw new CustomException(InvalidToken, null, System.Net.HttpStatusCode.Unauthorized);
            }

            var jwtToken = validatedToken as JwtSecurityToken;
            if (jwtToken == null)
            {
                throw new CustomException(InvalidToken, null, System.Net.HttpStatusCode.Unauthorized);
            }

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new CustomException(InvalidToken, null, System.Net.HttpStatusCode.Unauthorized);
            }

            var user = await userManager.FindByIdAsync(userIdClaim);
            if (user == null)
            {
                throw new CustomException(InvalidToken, null, System.Net.HttpStatusCode.Unauthorized);
            }

            string oldRefreshToken = await userManager.GetAuthenticationTokenAsync(user, applicationSettings.LoginProvider, applicationSettings.TokenNames.RefreshToken!) ?? "";

            if (oldRefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new CustomException(InvalidToken, null, System.Net.HttpStatusCode.Unauthorized);
            }

            return user;
        }

        public async Task<string> GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(applicationSettings.Secret!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Email!)
                }),
                Expires = dateTime.NowUtc.AddSeconds(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var isAdministrator = await userManager.IsInRoleAsync(user, Roles.Administrator);

            if (isAdministrator)
            {
                tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, Roles.Administrator));
            }

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encryptedToken = tokenHandler.WriteToken(token);

            return encryptedToken;
        }

        public async Task<string> GenerateRefreshTokenWithSecret(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(applicationSettings.RefreshSecret!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Email!)
                }),
                Expires = dateTime.NowUtc.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encryptedToken = tokenHandler.WriteToken(token);

            return encryptedToken;
        }

        public async Task RemoveAuthenticationToken(User user)
        {
            await userManager.RemoveAuthenticationTokenAsync(user, applicationSettings.LoginProvider, applicationSettings.TokenNames.RefreshToken!);
        }
    }
}