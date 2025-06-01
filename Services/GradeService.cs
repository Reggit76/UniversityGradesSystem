// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
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

        // Улучшенная версия SaveGrade с лучшей обработкой ошибок
        public bool SaveGrade(int studentId, int disciplineId, int grade)
        {
            try
            {
                Console.WriteLine($"SaveGrade вызван: studentId={studentId}, disciplineId={disciplineId}, grade={grade}");

                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    Console.WriteLine("Соединение с БД открыто");

                    // Используем транзакцию для атомарности операции
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Сначала найдем discipline_semester_id
                            int disciplineSemesterId = GetOrCreateDisciplineSemesterId(disciplineId, conn);
                            Console.WriteLine($"Используем discipline_semester_id: {disciplineSemesterId}");

                            // Проверяем, существует ли уже оценка для этого студента и дисциплины
                            bool gradeExists = false;
                            using (var checkCmd = new NpgsqlCommand(@"
                                SELECT COUNT(*) 
                                FROM grades 
                                WHERE student_id = @studentId AND discipline_semester_id = @disciplineSemesterId", conn))
                            {
                                checkCmd.Parameters.AddWithValue("studentId", studentId);
                                checkCmd.Parameters.AddWithValue("disciplineSemesterId", disciplineSemesterId);
                                checkCmd.Transaction = transaction;

                                gradeExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                            }

                            // Вставляем или обновляем оценку
                            string sql = gradeExists ?
                                @"UPDATE grades 
                                  SET grade = @grade 
                                  WHERE student_id = @studentId AND discipline_semester_id = @disciplineSemesterId" :
                                @"INSERT INTO grades (student_id, discipline_semester_id, grade) 
                                  VALUES (@studentId, @disciplineSemesterId, @grade)";

                            using (var cmd = new NpgsqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("studentId", studentId);
                                cmd.Parameters.AddWithValue("disciplineSemesterId", disciplineSemesterId);
                                cmd.Parameters.AddWithValue("grade", grade);
                                cmd.Transaction = transaction;

                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    Console.WriteLine($"Оценка успешно {(gradeExists ? "обновлена" : "добавлена")}");

                                    // Коммитим транзакцию
                                    transaction.Commit();

                                    // Логируем успешную операцию (опционально, без прерывания основной операции)
                                    try
                                    {
                                        LogGradeOperation(studentId, disciplineId, grade, gradeExists ? "UPDATE" : "INSERT");
                                    }
                                    catch (Exception logEx)
                                    {
                                        Console.WriteLine($"Предупреждение: не удалось залогировать операцию: {logEx.Message}");
                                        // Не прерываем выполнение из-за ошибки логирования
                                    }

                                    return true;
                                }
                                else
                                {
                                    Console.WriteLine("Не удалось сохранить оценку - нет затронутых строк");
                                    transaction.Rollback();
                                    return false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в SaveGrade: {ex.Message}");
                Console.WriteLine($"Детали исключения: {ex}");
                return false;
            }
        }

        // Вспомогательный метод для логирования операций с оценками
        private void LogGradeOperation(int studentId, int disciplineId, int grade, string operation)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT log_action(@userId, @actionType, @description)", conn))
                    {
                        // Пытаемся получить userId из контекста (если возможно)
                        // или используем NULL если не удается
                        cmd.Parameters.AddWithValue("userId", DBNull.Value);
                        cmd.Parameters.AddWithValue("actionType", $"GRADE_{operation}");
                        cmd.Parameters.AddWithValue("description",
                            $"Оценка {operation.ToLower()}: StudentId={studentId}, DisciplineId={disciplineId}, Grade={grade}");
                        cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось залогировать операцию: {ex.Message}");
                // Не выбрасываем исключение, чтобы не прервать основную операцию
            }
        }

        // Улучшенная версия GetOrCreateDisciplineSemesterId
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
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
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
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
            }

            throw new Exception($"Не удалось найти или создать discipline_semester для дисциплины {disciplineId}");
        }

        // Остальные методы остаются без изменений
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
                Console.WriteLine($"Ошибка загрузки студентов: {ex.Message}");
            }
            return students;
        }

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
                        var result = cmd.ExecuteScalar();
                        return result != null && Convert.ToBoolean(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления оценки: {ex.Message}");
                return false;
            }
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
                        return result != null && result != DBNull.Value ? Convert.ToInt32(result) : (int?)null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения оценки студента {studentId} по дисциплине {disciplineId}: {ex.Message}");
                return null;
            }
        }
    }
}