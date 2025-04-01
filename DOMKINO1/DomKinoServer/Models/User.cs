namespace DomKinoServer.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal BonusBalance { get; set; }
        public bool AdminRights { get; set; }
    }
}