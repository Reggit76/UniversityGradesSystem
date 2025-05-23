using Npgsql;
using System;
using System.Collections.Generic;
using UniversityGradesSystem.Models;

namespace UniversityGradesSystem.Services
{
    public class GradeService
    {
        private string _connectionString;

        public GradeService(string connectionString)
        {
            this._connectionString = connectionString;
        }

        // Получить студентов по дисциплине
        public List<Student> GetStudentsByDiscipline(int disciplineId)
        {
            var students = new List<Student>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT s.id, s.first_name, s.middle_name, s.last_name, s.group_id 
                        FROM students s
                        JOIN groups g ON s.group_id = g.id
                        WHERE g.id = @groupId", conn))
                    {
                        cmd.Parameters.AddWithValue("groupId", disciplineId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                students.Add(new Student
                                {
                                    Id = reader.GetInt32(0),
                                    FirstName = reader.GetString(1),
                                    MiddleName = reader.GetString(2),
                                    LastName = reader.GetString(3),
                                    GroupId = reader.GetInt32(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DatabaseManager.Instance.LogAction(null, "ERROR", $"Ошибка загрузки студентов: {ex.Message}");
            }
            return students;
        }

        // Обновить оценку
        public bool UpdateGrade(int gradeId, int newGrade, int userId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT update_grade_safe(@gradeId, @newGrade, @userId)", conn))
                    {
                        cmd.Parameters.AddWithValue("gradeId", gradeId);
                        cmd.Parameters.AddWithValue("newGrade", newGrade);
                        cmd.Parameters.AddWithValue("userId", userId);
                        return (bool)cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                DatabaseManager.Instance.LogAction(userId, "ERROR", $"Ошибка обновления оценки: {ex.Message}");
                return false;
            }
        }

        public bool SaveGrade(int studentId, int disciplineId, int grade)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO grades (student_id, discipline_id, grade, created_at)
                        VALUES (@studentId, @disciplineId, @grade, NOW())
                        ON CONFLICT (student_id, discipline_id)
                        DO UPDATE SET
                            grade = EXCLUDED.grade,
                            updated_at = NOW()
                        RETURNING id", conn))
                    {
                        // Добавляем параметры для предотвращения SQL-инъекций
                        cmd.Parameters.AddWithValue("studentId", studentId);
                        cmd.Parameters.AddWithValue("disciplineId", disciplineId);
                        cmd.Parameters.AddWithValue("grade", grade);

                        // Выполняем команду и проверяем результат
                        var result = cmd.ExecuteScalar();

                        // Логируем успешную операцию
                        DatabaseManager.Instance.LogAction(null, "INFO",
                            $"Оценка сохранена для студента {studentId} по дисциплине {disciplineId}: {grade}");

                        return result != null;
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                DatabaseManager.Instance.LogAction(null, "ERROR",
                    $"Ошибка сохранения оценки: {ex.Message}");

                return false;
            }
        }
    }
}