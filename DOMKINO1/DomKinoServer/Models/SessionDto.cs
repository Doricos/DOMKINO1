namespace DomKinoServer.Models
{
    public class SessionDto
    {
        public int SessionId { get; set; }
        public DateTime ShowDate { get; set; }
        public int MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}