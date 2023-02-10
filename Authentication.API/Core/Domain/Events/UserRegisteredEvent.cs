namespace Domain.Events
{
    using Domain.Common;

    public class UserRegisteredEvent : BaseEvent
    {
        public UserRegisteredEvent(string userId, string firstName, string lastName)
        {
            this.UserId = userId;
            this.FirstName = firstName;
            this.LastName = lastName;
        }

        public string UserId { get; }
        public string FirstName { get; }
        public string LastName { get; }
    }
}
