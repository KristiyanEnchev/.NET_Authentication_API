namespace Application.Identity.Commands.Common
{
    public abstract class UserRefreshModel
    {
        protected internal UserRefreshModel(string refreshToken)
        {
            this.RefreshToken = refreshToken;
        }

        public string RefreshToken { get; set; }
    }
}