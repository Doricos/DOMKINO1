using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using DomKinoServer.Models;

namespace DomKinoServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly string _connectionString = "Server=localhost;Database=DomKino;User ID=root;Password=Trapper05/;";

        [HttpGet("search")]
        public IActionResult SearchMovies(string genre = "", int ageRating = 0, int maxDuration = 240, string searchQuery = "")
        {
            var movies = new List<Movie>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT * FROM movies WHERE 1=1";

                if (!string.IsNullOrEmpty(genre))
                    query += " AND genre = @genre";
                if (ageRating > 0)
                    query += " AND age_restriction = @ageRating";
                if (maxDuration > 0)
                    query += " AND duration <= @maxDuration";
                if (!string.IsNullOrEmpty(searchQuery))
                    query += " AND title LIKE @searchQuery";

                using (var command = new MySqlCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(genre))
                        command.Parameters.AddWithValue("@genre", genre);
                    if (ageRating > 0)
                        command.Parameters.AddWithValue("@ageRating", ageRating);
                    if (maxDuration > 0)
                        command.Parameters.AddWithValue("@maxDuration", maxDuration);
                    if (!string.IsNullOrEmpty(searchQuery))
                        command.Parameters.AddWithValue("@searchQuery", $"%{searchQuery}%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            movies.Add(new Movie
                            {
                                Id = reader.GetInt32("movie_id"),
                                Title = reader.GetString("title"),
                                Genre = reader.GetString("genre"),
                                Duration = reader.GetInt32("duration"),
                                AgeRating = reader.GetInt32("age_restriction")
                            });
                        }
                    }
                }
            }

            return Ok(movies);
        }
    }
}