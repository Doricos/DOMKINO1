namespace DomKinoServer.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty; // Инициализация пустой строкой
        public string Genre { get; set; } = string.Empty; // Инициализация пустой строкой
        public int Duration { get; set; }
        public int AgeRating { get; set; } // Инициализация пустой строкой
    }
}