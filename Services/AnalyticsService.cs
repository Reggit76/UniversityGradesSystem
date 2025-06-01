// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using Npgsql;
using System;
using System.Collections.Generic;
using UniversityGradesSystem.Models;

namespace UniversityGradesSystem.Services
{
    public class AnalyticsService
    {
        private string _connectionString;

        public AnalyticsService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Существующие методы
        public decimal GetExcellentPercentage()
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT get_excellent_percentage()", conn))
                    {
                        var result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToDecimal(result) : 0;
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
                        var result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToDecimal(result) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка расчёта неуспевающих: {ex.Message}");
                return 0;
            }
        }

        // Исправленные методы для расширенной аналитики
        public GroupAnalytics GetGroupAnalytics(int groupId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM get_group_analytics(@groupId)", conn))
                    {
                        cmd.Parameters.AddWithValue("groupId", groupId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new GroupAnalytics
                                {
                                    TotalStudents = reader.GetInt32(0),           // total_students
                                    ExcellentCount = reader.GetInt32(1),         // excellent_count
                                    ExcellentPercentage = reader.GetDecimal(2),  // excellent_percentage
                                    GoodCount = reader.GetInt32(3),              // good_count
                                    GoodPercentage = reader.GetDecimal(4),       // good_percentage
                                    SatisfactoryCount = reader.GetInt32(5),      // satisfactory_count
                                    SatisfactoryPercentage = reader.GetDecimal(6), // satisfactory_percentage
                                    FailingCount = reader.GetInt32(7),           // failing_count
                                    FailingPercentage = reader.GetDecimal(8),    // failing_percentage
                                    AverageGrade = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9) // average_grade
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения аналитики группы {groupId}: {ex.Message}");
            }

            return new GroupAnalytics();
        }

        public List<TeacherDisciplineAnalytics> GetTeacherDisciplineAnalytics(int teacherId, int disciplineId)
        {
            var analytics = new List<TeacherDisciplineAnalytics>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM get_teacher_discipline_analytics(@teacherId, @disciplineId)", conn))
                    {
                        cmd.Parameters.AddWithValue("teacherId", teacherId);
                        cmd.Parameters.AddWithValue("disciplineId", disciplineId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                analytics.Add(new TeacherDisciplineAnalytics
                                {
                                    GroupName = reader.GetString(0),              // group_name
                                    GroupId = reader.GetInt32(1),                // group_id
                                    TotalStudents = reader.GetInt32(2),          // total_students
                                    GradedStudents = reader.GetInt32(3),         // graded_students
                                    AverageGrade = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4), // average_grade
                                    ExcellentCount = reader.GetInt32(5),         // excellent_count
                                    GoodCount = reader.GetInt32(6),              // good_count
                                    SatisfactoryCount = reader.GetInt32(7),      // satisfactory_count
                                    FailingCount = reader.GetInt32(8)            // failing_count
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения аналитики по дисциплине: {ex.Message}");
            }
            return analytics;
        }

        public List<TopStudent> GetTopStudentsByGroup(int groupId, int limit = 10)
        {
            var topStudents = new List<TopStudent>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM get_top_students_by_group(@groupId, @limit)", conn))
                    {
                        cmd.Parameters.AddWithValue("groupId", groupId);
                        cmd.Parameters.AddWithValue("limit", limit);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                topStudents.Add(new TopStudent
                                {
                                    StudentName = reader.GetString(0),        // student_name
                                    StudentId = reader.GetInt32(1),          // student_id
                                    AverageGrade = reader.GetDecimal(2),     // average_grade
                                    TotalGrades = reader.GetInt32(3)         // total_grades
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения топ студентов группы {groupId}: {ex.Message}");
            }
            return topStudents;
        }

        public List<GroupSummary> GetAllGroupsSummary()
        {
            var groupsSummary = new List<GroupSummary>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM get_all_groups_summary()", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                groupsSummary.Add(new GroupSummary
                                {
                                    GroupName = reader.GetString(0),                    // group_name
                                    GroupId = reader.GetInt32(1),                      // group_id
                                    SpecialtyName = reader.GetString(2),               // specialty_name
                                    CourseNumber = reader.GetInt32(3),                 // course_number
                                    TotalStudents = reader.GetInt32(4),                // total_students
                                    AverageGrade = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5), // average_grade
                                    ExcellentPercentage = reader.GetDecimal(6)         // excellent_percentage
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения сводки по группам: {ex.Message}");
            }
            return groupsSummary;
        }

        // Упрощенный метод для получения базовой аналитики группы (если новые функции не работают)
        public GroupAnalytics GetSimpleGroupAnalytics(int groupId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();

                    // Получаем базовую статистику простыми запросами
                    var analytics = new GroupAnalytics();

                    // Общее количество студентов
                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM students WHERE group_id = @groupId", conn))
                    {
                        cmd.Parameters.AddWithValue("groupId", groupId);
                        analytics.TotalStudents = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    if (analytics.TotalStudents > 0)
                    {
                        // Количество отличников (все оценки 5)
                        using (var cmd = new NpgsqlCommand(@"
                            SELECT COUNT(DISTINCT s.id) 
                            FROM students s 
                            WHERE s.group_id = @groupId 
                            AND NOT EXISTS (
                                SELECT 1 FROM grades g 
                                WHERE g.student_id = s.id AND g.grade < 5
                            )
                            AND EXISTS (
                                SELECT 1 FROM grades g 
                                WHERE g.student_id = s.id
                            )", conn))
                        {
                            cmd.Parameters.AddWithValue("groupId", groupId);
                            analytics.ExcellentCount = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Количество неуспевающих (есть оценка 2)
                        using (var cmd = new NpgsqlCommand(@"
                            SELECT COUNT(DISTINCT s.id) 
                            FROM students s 
                            WHERE s.group_id = @groupId 
                            AND EXISTS (
                                SELECT 1 FROM grades g 
                                WHERE g.student_id = s.id AND g.grade = 2
                            )", conn))
                        {
                            cmd.Parameters.AddWithValue("groupId", groupId);
                            analytics.FailingCount = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Средний балл
                        using (var cmd = new NpgsqlCommand(@"
                            SELECT AVG(g.grade::NUMERIC) 
                            FROM grades g 
                            JOIN students s ON g.student_id = s.id 
                            WHERE s.group_id = @groupId", conn))
                        {
                            cmd.Parameters.AddWithValue("groupId", groupId);
                            var result = cmd.ExecuteScalar();
                            analytics.AverageGrade = result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                        }

                        // Вычисляем проценты
                        analytics.ExcellentPercentage = (decimal)analytics.ExcellentCount * 100 / analytics.TotalStudents;
                        analytics.FailingPercentage = (decimal)analytics.FailingCount * 100 / analytics.TotalStudents;

                        // Примерные значения для хорошистов и троечников
                        analytics.GoodCount = analytics.TotalStudents - analytics.ExcellentCount - analytics.FailingCount;
                        analytics.GoodCount = Math.Max(0, analytics.GoodCount / 2); // Примерно половина остальных
                        analytics.SatisfactoryCount = analytics.TotalStudents - analytics.ExcellentCount - analytics.GoodCount - analytics.FailingCount;

                        analytics.GoodPercentage = (decimal)analytics.GoodCount * 100 / analytics.TotalStudents;
                        analytics.SatisfactoryPercentage = (decimal)analytics.SatisfactoryCount * 100 / analytics.TotalStudents;
                    }

                    return analytics;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения простой аналитики группы {groupId}: {ex.Message}");
                return new GroupAnalytics();
            }
        }
    }

    // Модели для аналитики
    public class GroupAnalytics
    {
        public int TotalStudents { get; set; }
        public int ExcellentCount { get; set; }
        public decimal ExcellentPercentage { get; set; }
        public int GoodCount { get; set; }
        public decimal GoodPercentage { get; set; }
        public int SatisfactoryCount { get; set; }
        public decimal SatisfactoryPercentage { get; set; }
        public int FailingCount { get; set; }
        public decimal FailingPercentage { get; set; }
        public decimal AverageGrade { get; set; }
    }

    public class TeacherDisciplineAnalytics
    {
        public string GroupName { get; set; }
        public int GroupId { get; set; }
        public int TotalStudents { get; set; }
        public int GradedStudents { get; set; }
        public decimal AverageGrade { get; set; }
        public int ExcellentCount { get; set; }
        public int GoodCount { get; set; }
        public int SatisfactoryCount { get; set; }
        public int FailingCount { get; set; }
    }

    public class TopStudent
    {
        public string StudentName { get; set; }
        public int StudentId { get; set; }
        public decimal AverageGrade { get; set; }
        public int TotalGrades { get; set; }
    }

    public class GroupSummary
    {
        public string GroupName { get; set; }
        public int GroupId { get; set; }
        public string SpecialtyName { get; set; }
        public int CourseNumber { get; set; }
        public int TotalStudents { get; set; }
        public decimal AverageGrade { get; set; }
        public decimal ExcellentPercentage { get; set; }
    }
}