// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using Npgsql;
using System;
using System.Collections.Generic;
using UniversityGradesSystem.Models;

namespace UniversityGradesSystem.Services
{
    public class DisciplineService
    {
        private string _connectionString;

        public DisciplineService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Discipline> GetTeacherDisciplines(int teacherId)
        {
            var disciplines = new List<Discipline>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM get_teacher_disciplines(@teacherId)", conn))
                    {
                        cmd.Parameters.AddWithValue("teacherId", teacherId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                disciplines.Add(new Discipline
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки дисциплин: {ex.Message}");
            }
            return disciplines;
        }
    }
}