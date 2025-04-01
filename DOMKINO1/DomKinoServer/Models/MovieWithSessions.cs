using System;
using System.Collections.Generic;

namespace DomKinoServer.Models
{
    public class MovieWithSessions
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int AgeRestriction { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Poster { get; set; } = string.Empty;
        public List<SessionDto> Sessions { get; set; } = new List<SessionDto>();
    }
}