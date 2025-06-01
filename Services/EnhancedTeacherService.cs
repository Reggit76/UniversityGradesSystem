// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using Npgsql;
using System;
using System.Collections.Generic;
using UniversityGradesSystem.Models;

namespace UniversityGradesSystem.Services
{
    public class EnhancedTeacherService : TeacherService
    {
        protected readonly string _connectionString;

        public EnhancedTeacherService(string connectionString) : base(connectionString)
        {
            _connectionString = connectionString;
        }

        // Получение всех преподавателей с подробной информацией
        public List<TeacherWithDetails> GetAllTeachersWithDetails()
        {
            var teachers = new List<TeacherWithDetails>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT 
                            t.id,
                            t.first_name,
                            t.middle_name,
                            t.last_name,
                            t.user_id,
                            u.username,
                            COALESCE(COUNT(td.discipline_id), 0) as disciplines_count
                        FROM teachers t
                        JOIN users u ON t.user_id = u.id
                        LEFT JOIN teacher_discipline td ON t.id = td.teacher_id
                        GROUP BY t.id, t.first_name, t.middle_name, t.last_name, t.user_id, u.username
                        ORDER BY t.last_name, t.first_name", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                teachers.Add(new TeacherWithDetails
                                {
                                    Id = reader.GetInt32(0),
                                    FirstName = reader.GetString(1),
                                    MiddleName = reader.GetString(2),
                                    LastName = reader.GetString(3),
                                    UserId = reader.GetInt32(4),
                                    Username = reader.GetString(5),
                                    DisciplinesCount = reader.GetInt32(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DatabaseManager.Instance.LogAction(null, "ERROR", $"Ошибка загрузки преподавателей: {ex.Message}");
                Console.WriteLine($"Ошибка получения преподавателей: {ex.Message}");
            }
            return teachers;
        }

        // Добавление нового преподавателя
        public bool AddTeacher(string firstName, string middleName, string lastName, string username, string password, List<int> disciplineIds, int adminUserId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Создаем пользователя
                        int userId;
                        using (var cmd = new NpgsqlCommand(@"
                            INSERT INTO users (username, password_hash, role) 
                            VALUES (@username, crypt(@password, gen_salt('bf')), 'teacher') 
                            RETURNING id", conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("username", username);
                            cmd.Parameters.AddWithValue("password", password);
                            userId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // 2. Создаем преподавателя
                        int teacherId;
                        using (var cmd = new NpgsqlCommand(@"
                            INSERT INTO teachers (first_name, middle_name, last_name, user_id) 
                            VALUES (@firstName, @middleName, @lastName, @userId) 
                            RETURNING id", conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("firstName", firstName);
                            cmd.Parameters.AddWithValue("middleName", middleName);
                            cmd.Parameters.AddWithValue("lastName", lastName);
                            cmd.Parameters.AddWithValue("userId", userId);
                            teacherId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // 3. Назначаем дисциплины
                        foreach (var disciplineId in disciplineIds)
                        {
                            using (var cmd = new NpgsqlCommand(@"
                                INSERT INTO teacher_discipline (teacher_id, discipline_id) 
                                VALUES (@teacherId, @disciplineId)", conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.Parameters.AddWithValue("teacherId", teacherId);
                                cmd.Parameters.AddWithValue("disciplineId", disciplineId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();

                        DatabaseManager.Instance.LogAction(adminUserId, "ADD_TEACHER",
                            $"Добавлен преподаватель: {lastName} {firstName} {middleName} (логин: {username})");

                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Смена пароля преподавателя
        public bool ChangeTeacherPassword(int userId, string newPassword, int adminUserId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        UPDATE users 
                        SET password_hash = crypt(@password, gen_salt('bf')) 
                        WHERE id = @userId AND role = 'teacher'", conn))
                    {
                        cmd.Parameters.AddWithValue("password", newPassword);
                        cmd.Parameters.AddWithValue("userId", userId);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            DatabaseManager.Instance.LogAction(adminUserId, "CHANGE_PASSWORD",
                                $"Изменен пароль для пользователя ID: {userId}");
                        }

                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                DatabaseManager.Instance.LogAction(adminUserId, "ERROR",
                    $"Ошибка смены пароля для пользователя {userId}: {ex.Message}");
                return false;
            }
        }

        // Удаление преподавателя
        public bool DeleteTeacher(int teacherId, int adminUserId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Получаем информацию о преподавателе
                        int userId;
                        string teacherInfo;
                        using (var cmd = new NpgsqlCommand(@"
                            SELECT t.user_id, t.last_name || ' ' || t.first_name || ' ' || t.middle_name 
                            FROM teachers t 
                            WHERE t.id = @teacherId", conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("teacherId", teacherId);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    return false; // Преподаватель не найден
                                }
                                userId = reader.GetInt32(0);
                                teacherInfo = reader.GetString(1);
                            }
                        }

                        // 1. Удаляем связи с дисциплинами
                        using (var cmd = new NpgsqlCommand("DELETE FROM teacher_discipline WHERE teacher_id = @teacherId", conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("teacherId", teacherId);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Удаляем преподавателя
                        using (var cmd = new NpgsqlCommand("DELETE FROM teachers WHERE id = @teacherId", conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("teacherId", teacherId);
                            cmd.ExecuteNonQuery();
                        }

                        // 3. Удаляем пользователя
                        using (var cmd = new NpgsqlCommand("DELETE FROM users WHERE id = @userId", conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("userId", userId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        DatabaseManager.Instance.LogAction(adminUserId, "DELETE_TEACHER",
                            $"Удален преподаватель: {teacherInfo}");

                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Проверка существования логина
        public bool UsernameExists(string username)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE LOWER(username) = LOWER(@username)", conn))
                    {
                        cmd.Parameters.AddWithValue("username", username);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка проверки существования логина: {ex.Message}");
                return true; // В случае ошибки считаем, что логин существует
            }
        }

        // Получение дисциплин преподавателя для управления
        public List<TeacherDisciplineAssignment> GetTeacherDisciplineAssignments(int teacherId)
        {
            var assignments = new List<TeacherDisciplineAssignment>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT 
                            d.id,
                            d.name,
                            CASE WHEN td.teacher_id IS NOT NULL THEN TRUE ELSE FALSE END as is_assigned
                        FROM disciplines d
                        LEFT JOIN teacher_discipline td ON d.id = td.discipline_id AND td.teacher_id = @teacherId
                        ORDER BY d.name", conn))
                    {
                        cmd.Parameters.AddWithValue("teacherId", teacherId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                assignments.Add(new TeacherDisciplineAssignment
                                {
                                    DisciplineId = reader.GetInt32(0),
                                    DisciplineName = reader.GetString(1),
                                    IsAssigned = reader.GetBoolean(2)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения назначений дисциплин: {ex.Message}");
            }
            return assignments;
        }

        // Обновление назначений дисциплин преподавателя
        public bool UpdateTeacherDisciplines(int teacherId, List<int> disciplineIds, int adminUserId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Удаляем все существующие назначения
                        using (var cmd = new NpgsqlCommand("DELETE FROM teacher_discipline WHERE teacher_id = @teacherId", conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("teacherId", teacherId);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Добавляем новые назначения
                        foreach (var disciplineId in disciplineIds)
                        {
                            using (var cmd = new NpgsqlCommand(@"
                                INSERT INTO teacher_discipline (teacher_id, discipline_id) 
                                VALUES (@teacherId, @disciplineId)", conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.Parameters.AddWithValue("teacherId", teacherId);
                                cmd.Parameters.AddWithValue("disciplineId", disciplineId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();

                        DatabaseManager.Instance.LogAction(adminUserId, "UPDATE_TEACHER_DISCIPLINES",
                            $"Обновлены дисциплины преподавателя ID: {teacherId}, назначено дисциплин: {disciplineIds.Count}");

                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}