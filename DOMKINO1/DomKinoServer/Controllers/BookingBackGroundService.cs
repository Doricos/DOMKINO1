using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Threading;
using System.Threading.Tasks;

public class BookingBackgroundService : BackgroundService
{
    private readonly ILogger<BookingBackgroundService> _logger;
    private readonly string _connectionString = "Server=localhost;Database=DomKino;User ID=root;Password=Trapper05/;";

    public BookingBackgroundService(ILogger<BookingBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновая задача для отмены брони запущена.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Проверяем брони каждую минуту
                await CheckAndCancelExpiredBookings();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Интервал проверки
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в фоновой задаче.");
            }
        }

        _logger.LogInformation("Фоновая задача для отмены брони остановлена.");
    }

    private async Task CheckAndCancelExpiredBookings()
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Находим брони, которые активны более 10 минут
            var query = @"
                UPDATE Bookings
                SET status = 'canceled'
                WHERE status = 'active' AND created_at < NOW() - INTERVAL 10 MINUTE";

            using (var command = new MySqlCommand(query, connection))
            {
                int affectedRows = await command.ExecuteNonQueryAsync();
                if (affectedRows > 0)
                {
                    _logger.LogInformation($"Отменено {affectedRows} брони(й).");
                }
            }
        }
    }
}