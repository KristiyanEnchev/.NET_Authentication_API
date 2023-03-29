namespace Domain.Events
{
    using Domain.Common;

    public class ApplicationUserDeletedEvent : BaseEvent
    {
        public ApplicationUserDeletedEvent(string userId)
        {
            UserId = userId;
        }

        public string UserId { get; }
    }
}
