namespace Application.Identity.Common
{
    public class UserResponseModel
    {
        public UserResponseModel(string token, DateTime refreshTokenExpiryTime, string refreshToken)
        {
            AccesToken = token;
            RefreshTokenExpiryTime = refreshTokenExpiryTime;
            RefreshToken = refreshToken;
        }

        public string AccesToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public string RefreshToken { get; set; }
    }
}