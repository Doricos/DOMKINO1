using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Dapper;
using System;
using System.Collections.Generic;
using DomKinoServer.Models;

namespace DomKinoServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController : ControllerBase
    {
        private readonly string _connectionString = "Server=localhost;Database=DomKino;User ID=root;Password=Trapper05/;";

        [HttpGet("active")]
        public IActionResult GetActiveMovies(string genre = "", int ageRating = 0, int maxDuration = 240)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                var query = @"
                    SELECT 
                        m.movie_id AS MovieId, 
                        m.title AS Title, 
                        m.genre AS Genre, 
                        m.duration AS Duration,
                        m.age_restriction AS AgeRestriction,
                        m.description AS Description,
                        m.poster AS Poster,
                        s.session_id AS SessionId,
                        s.show_date AS ShowDate
                    FROM Movies m
                    JOIN Sessions s ON m.movie_id = s.movie_id
                    WHERE s.show_date >= NOW()
                    AND (@genre = '' OR m.genre = @genre)
                    AND (@ageRating = 0 OR m.age_restriction = @ageRating)
                    AND (@maxDuration = 0 OR m.duration <= @maxDuration)
                    ORDER BY m.movie_id, s.show_date;";

                var moviesDict = new Dictionary<int, MovieWithSessions>();
                
var results = connection.Query<MovieWithSessions, SessionDto, MovieWithSessions>(
    query,
    (movie, session) => 
    {
        if (!moviesDict.TryGetValue(movie.MovieId, out var movieEntry))
        {
            movieEntry = movie;
            movieEntry.Sessions = new List<SessionDto>();
            moviesDict.Add(movie.MovieId, movieEntry);
        }
        movieEntry.Sessions.Add(session);
        return movieEntry;
    },
    new { genre, ageRating, maxDuration },
    splitOn: "SessionId" // Убедитесь, что это правильное поле для разделения
);

                return Ok(moviesDict.Values);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpGet("{sessionId}")]
        public IActionResult GetSession(int sessionId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                var query = @"
                    SELECT s.session_id, s.show_date, 
                           m.title, m.genre, m.duration 
                    FROM Sessions s
                    JOIN Movies m ON s.movie_id = m.movie_id
                    WHERE s.session_id = @sessionId";

                var session = connection.QueryFirstOrDefault<SessionDto>(query, new { sessionId });
                
                return session != null 
                    ? Ok(session) 
                    : NotFound(new { success = false, message = "Сеанс не найден" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}