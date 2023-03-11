namespace Application.Handlers.Identity.Commands.Register
{
    using Application.Handlers.Identity.Common;

    public class UserRegisterRequestModel : UserRequestModel
    {
        internal UserRegisterRequestModel(string firstName, string lastName, string email, string password)
            : base(email, password)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}