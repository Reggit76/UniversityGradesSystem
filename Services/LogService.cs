// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversityGradesSystem.Services
{
    public static class LogService
    {
        private static string connectionString = DatabaseManager.Instance.GetConnectionString();

        public static void LogAction(int? userId, string actionType, string description)
        {
            using (var conn = new NpgsqlConnection(connectionString))
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
    }
}