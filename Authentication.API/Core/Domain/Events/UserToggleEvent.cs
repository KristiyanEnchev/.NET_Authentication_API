namespace Domain.Events
{
    using Domain.Common;

    public class UserToggleEvent : BaseEvent
    {
        public UserToggleEvent(bool isActive, bool isEmailConfirmed, bool isLockedOut, IEnumerable<string> changedProperties)
        {
            this.IsActive = isActive;
            this.IsEmailConfirmed = isEmailConfirmed;
            this.IsLockedOut = isLockedOut;
            ChangedProperties = changedProperties ?? new List<string>();
        }

        public bool IsActive { get; }
        public bool IsEmailConfirmed { get; }
        public bool IsLockedOut { get; }
        public IEnumerable<string> ChangedProperties { get; }
    }
}