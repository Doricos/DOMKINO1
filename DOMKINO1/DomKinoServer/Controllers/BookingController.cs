using Microsoft.AspNetCore.Mvc;
using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Linq;
using DomKinoServer.Models;

namespace DomKinoServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly string _connectionString = "Server=localhost;Database=DomKino;User ID=root;Password=Trapper05/;";

        [HttpGet("active")]
        public IActionResult GetActiveBookings(int sessionId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();
                
                var query = @"
                    SELECT seat_id 
                    FROM Bookings 
                    WHERE session_id = @sessionId 
                    AND status IN ('active', 'paid')";

                var seatIds = connection.Query<int>(query, new { sessionId });
                return Ok(seatIds);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("create")]
        public IActionResult CreateBooking([FromBody] BookingRequest request)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                foreach (var seatId in request.SeatIds)
                {
                    var checkQuery = @"
                        SELECT COUNT(*) 
                        FROM Bookings 
                        WHERE seat_id = @seatId 
                        AND session_id = @sessionId 
                        AND status IN ('active', 'paid')";

                    var exists = connection.ExecuteScalar<int>(checkQuery, new {
                        seatId,
                        request.SessionId
                    });

                    if (exists > 0)
                        return BadRequest(new { 
                            success = false, 
                            message = $"Место {seatId} уже занято" 
                        });
                }

                var insertQuery = @"
                    INSERT INTO Bookings 
                        (seat_id, user_id, session_id, status, created_at)
                    VALUES 
                        (@seatId, @userId, @sessionId, 'active', NOW())";

                connection.Execute(insertQuery, request.SeatIds.Select(seatId => new {
                    seatId,
                    request.UserId,
                    request.SessionId
                }));

                return Ok(new { 
                    success = true, 
                    message = "Бронь успешно создана" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = ex.Message 
                });
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
                    SELECT 
                        s.session_id AS SessionId, 
                        s.show_date AS ShowDate,
                        m.movie_id AS MovieId,
                        m.title AS Title
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

        public class BookingRequest
        {
            public int[] SeatIds { get; set; }
            public int UserId { get; set; }
            public int SessionId { get; set; }
        }
    }
}