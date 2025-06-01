// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UniversityGradesSystem.Services
{
    public class TeacherService
    {
        string _connectionString;
        public TeacherService(string connectionString) { this._connectionString = connectionString; }

        public int? GetTeacherId(int userId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(this._connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT id FROM teachers WHERE user_id = @userId", conn))
                    {
                        cmd.Parameters.AddWithValue("userId", userId);
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            return Convert.ToInt32(result);
                        }
                        else
                        {
                            DatabaseManager.Instance.LogAction(userId, "ERROR", "Преподаватель не найден по userId");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DatabaseManager.Instance.LogAction(userId, "ERROR", $"Ошибка получения teacherId: {ex.Message}");
                MessageBox.Show($"Ошибка получения данных преподавателя1: {ex.Message}");
                return null;
            }
        }
    }
}
