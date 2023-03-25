namespace Domain.Events
{
    using Domain.Common;

    public class ApplicationUserUpdatedEvent : BaseEvent
    {
        public ApplicationUserUpdatedEvent(string userId, List<string> changedProperties)
        {
            UserId = userId;
            ChangedProperties = changedProperties;
        }

        public string UserId { get; }
        public List<string> ChangedProperties { get; }
    }
}
