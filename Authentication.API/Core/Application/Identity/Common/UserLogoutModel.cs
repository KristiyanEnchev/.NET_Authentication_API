namespace Application.Identity.Commands.Common
{
    public abstract class UserLogoutModel
    {
        protected internal UserLogoutModel(string email)
        {
            this.Email = email;
        }

        public string Email { get; set; }
    }
}