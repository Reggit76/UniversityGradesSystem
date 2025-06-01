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

                // Сначала создаем пользователя PostgreSQL (вне транзакции)
                var safeUsername = EscapeIdentifier(username);
                if (!CreatePostgreSQLUser(safeUsername, password, conn))
                {
                    throw new Exception("Не удалось создать пользователя PostgreSQL");
                }

                // Затем выполняем остальные операции в транзакции
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Создаем пользователя в таблице users
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
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        // При ошибке удаляем созданного пользователя PostgreSQL
                        try
                        {
                            DeletePostgreSQLUser(safeUsername, conn);
                        }
                        catch (Exception cleanupEx)
                        {
                            Console.WriteLine($"Ошибка очистки пользователя PostgreSQL: {cleanupEx.Message}");
                        }

                        Console.WriteLine($"Ошибка добавления преподавателя: {ex.Message}");
                        DatabaseManager.Instance.LogAction(adminUserId, "ERROR",
                            $"Ошибка добавления преподавателя: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        // НОВЫЙ МЕТОД: Создание пользователя PostgreSQL (без транзакции)
        private bool CreatePostgreSQLUser(string safeUsername, string password, NpgsqlConnection conn)
        {
            try
            {
                // Проверяем, не существует ли уже пользователь
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM pg_user WHERE usename = @username", conn))
                {
                    cmd.Parameters.AddWithValue("username", safeUsername);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count > 0)
                    {
                        Console.WriteLine($"Пользователь PostgreSQL '{safeUsername}' уже существует");
                        return false;
                    }
                }

                // 1. Создаем пользователя PostgreSQL (используем строковую интерполяцию для безопасного имени)
                var createUserSql = $"CREATE USER \"{safeUsername}\" WITH LOGIN PASSWORD '{EscapePassword(password)}'";
                using (var cmd = new NpgsqlCommand(createUserSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // 2. Назначаем роль teacher
                using (var cmd = new NpgsqlCommand($"GRANT teacher TO \"{safeUsername}\"", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // 3. Даем базовые права преподавателя
                var grantCommands = new[]
                {
                    $"GRANT CONNECT ON DATABASE \"UniversityDB\" TO \"{safeUsername}\"",
                    $"GRANT USAGE ON SCHEMA public TO \"{safeUsername}\"",
                    $"GRANT SELECT ON ALL TABLES IN SCHEMA public TO \"{safeUsername}\"",
                    $"GRANT INSERT, UPDATE ON grades TO \"{safeUsername}\"",
                    $"GRANT INSERT ON audit_log TO \"{safeUsername}\"",
                    $"GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO \"{safeUsername}\"",
                    $"GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO \"{safeUsername}\"",
                    $"ALTER USER \"{safeUsername}\" SET search_path TO public, \"$user\", pg_temp"
                };

                foreach (var grantCommand in grantCommands)
                {
                    using (var cmd = new NpgsqlCommand(grantCommand, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                Console.WriteLine($"Пользователь PostgreSQL '{safeUsername}' успешно создан с правами преподавателя");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания пользователя PostgreSQL '{safeUsername}': {ex.Message}");
                return false;
            }
        }

        // НОВЫЙ МЕТОД: Удаление пользователя PostgreSQL
        private void DeletePostgreSQLUser(string safeUsername, NpgsqlConnection conn)
        {
            try
            {
                // Завершаем активные соединения пользователя
                using (var cmd = new NpgsqlCommand(@"
                    SELECT pg_terminate_backend(pid) 
                    FROM pg_stat_activity 
                    WHERE usename = @username AND pid <> pg_backend_pid()", conn))
                {
                    cmd.Parameters.AddWithValue("username", safeUsername);
                    cmd.ExecuteNonQuery();
                }

                // Удаляем пользователя
                using (var cmd = new NpgsqlCommand($"DROP USER IF EXISTS \"{safeUsername}\"", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                Console.WriteLine($"Пользователь PostgreSQL '{safeUsername}' удален");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления пользователя PostgreSQL '{safeUsername}': {ex.Message}");
            }
        }

        // НОВЫЙ МЕТОД: Безопасное экранирование пароля
        private string EscapePassword(string password)
        {
            // Экранируем одинарные кавычки в пароле
            return password.Replace("'", "''");
        }

        // НОВЫЙ МЕТОД: Безопасное экранирование идентификаторов PostgreSQL
        private string EscapeIdentifier(string identifier)
        {
            // Простая проверка на допустимые символы (буквы, цифры, подчеркивание)
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Идентификатор не может быть пустым");

            // Удаляем недопустимые символы и приводим к нижнему регистру
            var cleaned = System.Text.RegularExpressions.Regex.Replace(identifier.ToLower(), @"[^a-z0-9_]", "");

            if (string.IsNullOrWhiteSpace(cleaned))
                throw new ArgumentException("Идентификатор содержит только недопустимые символы");

            // Если начинается с цифры, добавляем префикс
            if (char.IsDigit(cleaned[0]))
                cleaned = "u_" + cleaned;

            // Ограничиваем длину (максимум 63 символа для PostgreSQL)
            if (cleaned.Length > 63)
                cleaned = cleaned.Substring(0, 63);

            return cleaned;
        }

        // Смена пароля преподавателя (ИСПРАВЛЕНО)
        public bool ChangeTeacherPassword(int userId, string newPassword, int adminUserId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                try
                {
                    // Получаем username для обновления пароля PostgreSQL
                    string username;
                    using (var cmd = new NpgsqlCommand("SELECT username FROM users WHERE id = @userId AND role = 'teacher'", conn))
                    {
                        cmd.Parameters.AddWithValue("userId", userId);
                        var result = cmd.ExecuteScalar();
                        if (result == null)
                            return false;
                        username = result.ToString();
                    }

                    var safeUsername = EscapeIdentifier(username);

                    // 1. Сначала обновляем пароль пользователя PostgreSQL (вне транзакции)
                    var alterUserSql = $"ALTER USER \"{safeUsername}\" WITH PASSWORD '{EscapePassword(newPassword)}'";
                    using (var cmd = new NpgsqlCommand(alterUserSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Затем обновляем пароль в таблице users (в транзакции)
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            using (var cmd = new NpgsqlCommand(@"
                                UPDATE users 
                                SET password_hash = crypt(@password, gen_salt('bf')) 
                                WHERE id = @userId AND role = 'teacher'", conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.Parameters.AddWithValue("password", newPassword);
                                cmd.Parameters.AddWithValue("userId", userId);
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected == 0)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }

                    DatabaseManager.Instance.LogAction(adminUserId, "CHANGE_PASSWORD",
                        $"Изменен пароль для пользователя ID: {userId} (username: {username})");

                    Console.WriteLine($"Пароль для пользователя '{username}' успешно изменен");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка смены пароля: {ex.Message}");
                    DatabaseManager.Instance.LogAction(adminUserId, "ERROR",
                        $"Ошибка смены пароля для пользователя {userId}: {ex.Message}");
                    return false;
                }
            }
        }

        // Удаление преподавателя (ИСПРАВЛЕНО)
        public bool DeleteTeacher(int teacherId, int adminUserId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                try
                {
                    // Получаем информацию о преподавателе
                    int userId;
                    string teacherInfo;
                    string username;
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT t.user_id, t.last_name || ' ' || t.first_name || ' ' || t.middle_name, u.username
                        FROM teachers t 
                        JOIN users u ON t.user_id = u.id
                        WHERE t.id = @teacherId", conn))
                    {
                        cmd.Parameters.AddWithValue("teacherId", teacherId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                return false; // Преподаватель не найден
                            }
                            userId = reader.GetInt32(0);
                            teacherInfo = reader.GetString(1);
                            username = reader.GetString(2);
                        }
                    }

                    var safeUsername = EscapeIdentifier(username);

                    // 1. Сначала удаляем данные из таблиц (в транзакции)
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Удаляем связи с дисциплинами
                            using (var cmd = new NpgsqlCommand("DELETE FROM teacher_discipline WHERE teacher_id = @teacherId", conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.Parameters.AddWithValue("teacherId", teacherId);
                                cmd.ExecuteNonQuery();
                            }

                            // Удаляем преподавателя
                            using (var cmd = new NpgsqlCommand("DELETE FROM teachers WHERE id = @teacherId", conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.Parameters.AddWithValue("teacherId", teacherId);
                                cmd.ExecuteNonQuery();
                            }

                            // Удаляем пользователя из таблицы users
                            using (var cmd = new NpgsqlCommand("DELETE FROM users WHERE id = @userId", conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.Parameters.AddWithValue("userId", userId);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }

                    // 2. Затем удаляем пользователя PostgreSQL (вне транзакции)
                    try
                    {
                        // Завершаем активные соединения пользователя (если есть)
                        using (var cmd = new NpgsqlCommand(@"
                            SELECT pg_terminate_backend(pid) 
                            FROM pg_stat_activity 
                            WHERE usename = @username AND pid <> pg_backend_pid()", conn))
                        {
                            cmd.Parameters.AddWithValue("username", username);
                            cmd.ExecuteNonQuery();
                        }

                        // Отзываем права
                        var revokeCommands = new[]
                        {
                            $"REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA public FROM \"{safeUsername}\"",
                            $"REVOKE ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public FROM \"{safeUsername}\"",
                            $"REVOKE ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public FROM \"{safeUsername}\"",
                            $"REVOKE teacher FROM \"{safeUsername}\""
                        };

                        foreach (var revokeCommand in revokeCommands)
                        {
                            try
                            {
                                using (var cmd = new NpgsqlCommand(revokeCommand, conn))
                                {
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Предупреждение при отзыве прав: {ex.Message}");
                            }
                        }

                        // Удаляем пользователя
                        using (var cmd = new NpgsqlCommand($"DROP USER IF EXISTS \"{safeUsername}\"", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        Console.WriteLine($"Пользователь PostgreSQL '{username}' успешно удален");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Предупреждение: не удалось удалить пользователя PostgreSQL '{username}': {ex.Message}");
                        // Не прерываем операцию из-за этой ошибки
                    }

                    DatabaseManager.Instance.LogAction(adminUserId, "DELETE_TEACHER",
                        $"Удален преподаватель: {teacherInfo} (username: {username})");

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка удаления преподавателя: {ex.Message}");
                    DatabaseManager.Instance.LogAction(adminUserId, "ERROR",
                        $"Ошибка удаления преподавателя {teacherId}: {ex.Message}");
                    throw;
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