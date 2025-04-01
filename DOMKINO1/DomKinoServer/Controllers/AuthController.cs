using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using DomKinoServer.Models;

namespace DomKinoServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string _connectionString = "Server=localhost;Database=DomKino;User ID=root;Password=Trapper05/;";

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();
                
                var query = "SELECT * FROM Users WHERE phone_number = @phoneNumber AND password = @password";
                using var command = new MySqlCommand(query, connection);
                
                command.Parameters.AddWithValue("@phoneNumber", request.Phone);
                command.Parameters.AddWithValue("@password", request.Password);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var user = new User
                    {
                        UserId = reader.GetInt32("user_id"), // Убедитесь, что userId включен
                        FirstName = reader.GetString("first_name"),
                        LastName = reader.GetString("last_name"),
                        PhoneNumber = reader.GetString("phone_number"),
                        BonusBalance = reader.GetDecimal("bonus_balance"),
                        AdminRights = reader.GetBoolean("AdminRights")
                    };

                    return Ok(new { 
                        success = true, 
                        user = new {
                            user.UserId, // Возвращаем userId
                            user.FirstName,
                            user.LastName,
                            user.PhoneNumber,
                            user.BonusBalance,
                            adminRights = user.AdminRights
                        }
                    });
                }
                
                return BadRequest(new { success = false, message = "Неверный номер или пароль" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("grant-admin")]
        public IActionResult GrantAdminRights([FromBody] GrantAdminRequest request)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();
                
                // Проверка пароля администратора
                if (request.AdminPassword != "Admin123")
                    return BadRequest(new { success = false, message = "Неверный пароль администратора" });

                // Обновление прав пользователя
                var updateQuery = "UPDATE Users SET AdminRights = TRUE WHERE phone_number = @phoneNumber";
                using var command = new MySqlCommand(updateQuery, connection);
                
                command.Parameters.AddWithValue("@phoneNumber", request.PhoneNumber);
                int affectedRows = command.ExecuteNonQuery();

                if (affectedRows == 0)
                    return BadRequest(new { success = false, message = "Пользователь не найден" });

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.FirstName) ||
                string.IsNullOrEmpty(request.LastName) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { success = false, message = "Неправильно заполнены данные" });
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // Проверка на уникальность номера телефона
                var checkQuery = "SELECT COUNT(*) FROM Users WHERE phone_number = @phoneNumber";
                using (var checkCommand = new MySqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@phoneNumber", request.PhoneNumber);
                    int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                    if (count > 0)
                    {
                        return BadRequest(new { success = false, message = "Пользователь с таким номером уже существует" });
                    }
                }

                // Создание пользователя
                var insertQuery = "INSERT INTO Users (first_name, last_name, phone_number, password) VALUES (@firstName, @lastName, @phoneNumber, @password)";
                using (var insertCommand = new MySqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@firstName", request.FirstName);
                    insertCommand.Parameters.AddWithValue("@lastName", request.LastName);
                    insertCommand.Parameters.AddWithValue("@phoneNumber", request.PhoneNumber);
                    insertCommand.Parameters.AddWithValue("@password", request.Password);

                    try
                    {
                        insertCommand.ExecuteNonQuery();
                        return Ok(new { success = true });
                    }
                    catch (MySqlException ex)
                    {
                        return BadRequest(new { success = false, message = ex.Message });
                    }
                }
            }
        }
    }
}