namespace Models
{
    public class ApplicationSettings
    {
        public string? Secret { get; set; }
        public string? RefreshSecret { get; set; }
        public int RefreshTokenExpirationInDays { get; set; }
        public string? LoginProvider { get; set; }
        public TokenNames? TokenNames { get; set; }
    }

    public class TokenNames
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}