namespace DomKinoServer.Models
{
    public class GrantAdminRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string AdminPassword { get; set; } = string.Empty;
    }
}