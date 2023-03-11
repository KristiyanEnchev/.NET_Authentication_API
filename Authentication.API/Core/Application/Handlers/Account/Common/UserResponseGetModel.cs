namespace Application.Handlers.Account.Common
{
    public class UserResponseGetModel
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public string? Role { get; set; }

        public string? CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }
    }
}
