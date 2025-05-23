using Npgsql;
using System;

namespace UniversityGradesSystem.Services
{
    public class AnalyticsService
    {
        private string _connectionString;

        public AnalyticsService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public decimal GetExcellentPercentage()
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT get_excellent_percentage()", conn))
                    {
                        return (decimal)cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка расчёта отличников: {ex.Message}");
                return 0;
            }
        }

        public decimal GetFailingPercentage()
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT get_failing_percentage()", conn))
                    {
                        return (decimal)cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка расчёта неуспевающих: {ex.Message}");
                return 0;
            }
        }
    }
}