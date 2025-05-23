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

                    // Получаем discipline_semester_id
                    int disciplineSemesterId = GetOrCreateDisciplineSemesterId(disciplineId, conn);

                    using (var cmd = new NpgsqlCommand(@"
                INSERT INTO grades (student_id, discipline_semester_id, grade)
                VALUES (@studentId, @disciplineSemesterId, @grade)
                ON CONFLICT (student_id, discipline_semester_id)
                DO UPDATE SET grade = EXCLUDED.grade
                RETURNING id", conn))
                    {
                        cmd.Parameters.AddWithValue("studentId", studentId);
                        cmd.Parameters.AddWithValue("disciplineSemesterId", disciplineSemesterId);
                        cmd.Parameters.AddWithValue("grade", grade);

                        var result = cmd.ExecuteScalar();

                        DatabaseManager.Instance.LogAction(null, "INFO",
                            $"Оценка сохранена для студента {studentId} по дисциплине {disciplineId}: {grade}");

                        return result != null;
                    }
                }
            }
            catch (Exception ex)
            {
                DatabaseManager.Instance.LogAction(null, "ERROR",
                    $"Ошибка сохранения оценки: {ex.Message}");
                return false;
            }
        }

        // Получить или создать discipline_semester_id
        private int GetOrCreateDisciplineSemesterId(int disciplineId, NpgsqlConnection conn)
        {
            // Сначала ищем существующую запись
            using (var cmd = new NpgsqlCommand(@"
        SELECT ds.id 
        FROM discipline_semester ds 
        WHERE ds.discipline_id = @disciplineId 
        LIMIT 1", conn))
            {
                cmd.Parameters.AddWithValue("disciplineId", disciplineId);
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return (int)result;
                }
            }

            // Если не найдена, создаем новую (берем первый семестр)
            using (var cmd = new NpgsqlCommand(@"
        INSERT INTO discipline_semester (discipline_id, semester_id)
        SELECT @disciplineId, s.id 
        FROM semesters s 
        ORDER BY s.number 
        LIMIT 1
        RETURNING id", conn))
            {
                cmd.Parameters.AddWithValue("disciplineId", disciplineId);
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return (int)result;
                }
            }

            throw new Exception($"Не удалось найти или создать discipline_semester для дисциплины {disciplineId}");
        }

        public int? GetStudentGrade(int studentId, int disciplineId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT g.grade 
                        FROM grades g
                        JOIN discipline_semester ds ON g.discipline_semester_id = ds.id
                        WHERE g.student_id = @studentId AND ds.discipline_id = @disciplineId
                        ORDER BY g.id DESC
                        LIMIT 1", conn))
                    {
                        cmd.Parameters.AddWithValue("studentId", studentId);
                        cmd.Parameters.AddWithValue("disciplineId", disciplineId);

                        var result = cmd.ExecuteScalar();
                        return result != null ? (int?)result : null;
                    }
                }
            }
            catch (Exception ex)
            {
                DatabaseManager.Instance.LogAction(null, "ERROR", $"Ошибка получения оценки студента {studentId} по дисциплине {disciplineId}: {ex.Message}");
                return null;
            }
        }
    }
}