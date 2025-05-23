using Npgsql;
using System;
using System.Collections.Generic;
using UniversityGradesSystem.Models;

namespace UniversityGradesSystem.Services
{
    public class DatabaseManager
    {
        private static DatabaseManager _instance;
        private string _connectionString;
        private DatabaseManager() { }
        public static DatabaseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DatabaseManager();
                }
                return _instance;
            }
        }

        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string GetConnectionString()
        {
            return _connectionString;
        }

        // === Аутентификация ===
        public (int? UserId, string Role) AuthenticateUser(string username, string password)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM authenticate_user(@username, @password)", conn))
                    {
                        cmd.Parameters.AddWithValue("username", username);
                        cmd.Parameters.AddWithValue("password", password);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return (reader.GetInt32(0), reader.GetString(1));
                            }
                            else
                            {
                                return (null, null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка аутентификации: {ex.Message}");
                return (null, null);
            }
        }

        // === Логирование ===
        public void LogAction(int? userId, string actionType, string description)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT log_action(@userId, @actionType, @description)", conn))
                    {
                        cmd.Parameters.AddWithValue("userId", userId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("actionType", actionType);
                        cmd.Parameters.AddWithValue("description", description);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOG_ERROR] {ex.Message}");
            }
        }
    }
}