using Npgsql;
using System;
using System.Collections.Generic;
using UniversityGradesSystem.Models;

namespace UniversityGradesSystem.Services
{
    public class StudentService
    {
        private string _connectionString;

        public StudentService(string connectionString)
        {
            this._connectionString = connectionString;
        }

        // === СУЩЕСТВУЮЩИЕ МЕТОДЫ ===

        // Получить всех студентов (старый метод)
        public List<Student> GetAllStudents()
        {
            var students = new List<Student>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id, first_name, middle_name, last_name, group_id FROM students", conn))
                {
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
            return students;
        }

        // Добавить студента
        public bool AddStudent(Student student, int adminUserId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO students (first_name, middle_name, last_name, group_id)
                        VALUES (@firstName, @middleName, @lastName, @groupId)", conn))
                    {
                        cmd.Parameters.AddWithValue("firstName", student.FirstName);
                        cmd.Parameters.AddWithValue("middleName", student.MiddleName);
                        cmd.Parameters.AddWithValue("lastName", student.LastName);
                        cmd.Parameters.AddWithValue("groupId", student.GroupId);
                        cmd.ExecuteNonQuery();
                        LogService.LogAction(adminUserId, "ADD_STUDENT", $"Добавлен студент: {student.FirstName} {student.LastName}");
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public List<Student> GetStudentsByDiscipline(int disciplineId)
        {
            var students = new List<Student>();
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
            return students;
        }

        public List<Student> GetStudentsByGroup(int groupId)
        {
            var students = new List<Student>();

            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand(@"
                SELECT id, first_name, middle_name, last_name, group_id 
                FROM students 
                WHERE group_id = @groupId", conn))
                    {
                        cmd.Parameters.AddWithValue("groupId", groupId);

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
                Console.WriteLine($"Ошибка при загрузке студентов: {ex.Message}");
            }

            return students;
        }

        // === НОВЫЕ МЕТОДЫ ===

        // Получить всех студентов с информацией о группах
        public List<StudentWithGroup> GetAllStudentsWithGroups()
        {
            var students = new List<StudentWithGroup>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM get_students_with_groups()", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                students.Add(new StudentWithGroup
                                {
                                    Id = reader.GetInt32(0),           // student_id
                                    FirstName = reader.GetString(1),   // first_name
                                    MiddleName = reader.GetString(2),  // middle_name
                                    LastName = reader.GetString(3),    // last_name
                                    GroupId = reader.GetInt32(4),      // group_id
                                    GroupName = reader.GetString(5),   // group_name
                                    SpecialtyName = reader.GetString(6), // specialty_name
                                    CourseNumber = reader.GetInt32(7), // course_number
                                    FullName = reader.GetString(8)     // full_name
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки студентов с группами: {ex.Message}");
                LogService.LogAction(null, "ERROR", $"Ошибка загрузки студентов с группами: {ex.Message}");
            }
            return students;
        }

        // Получить студентов конкретной группы с подробной информацией
        public List<StudentWithGroup> GetStudentsByGroupDetailed(int groupId)
        {
            var students = new List<StudentWithGroup>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM get_students_by_group_detailed(@groupId)", conn))
                    {
                        cmd.Parameters.AddWithValue("groupId", groupId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                students.Add(new StudentWithGroup
                                {
                                    Id = reader.GetInt32(0),           // student_id
                                    FirstName = reader.GetString(1),   // first_name
                                    MiddleName = reader.GetString(2),  // middle_name
                                    LastName = reader.GetString(3),    // last_name
                                    GroupId = reader.GetInt32(4),      // group_id
                                    GroupName = reader.GetString(5),   // group_name
                                    SpecialtyName = reader.GetString(6), // specialty_name
                                    CourseNumber = reader.GetInt32(7), // course_number
                                    FullName = reader.GetString(8)     // full_name
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки студентов группы {groupId}: {ex.Message}");
                LogService.LogAction(null, "ERROR", $"Ошибка загрузки студентов группы {groupId}: {ex.Message}");
            }
            return students;
        }

        // Получить статистику по студентам
        public Dictionary<string, int> GetStudentsStatistics()
        {
            var stats = new Dictionary<string, int>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();

                    // Общее количество студентов
                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM students", conn))
                    {
                        stats["TotalStudents"] = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Количество групп
                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM groups", conn))
                    {
                        stats["TotalGroups"] = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Количество специальностей
                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM specialties", conn))
                    {
                        stats["TotalSpecialties"] = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Студенты с оценками
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT COUNT(DISTINCT student_id) FROM grades", conn))
                    {
                        stats["StudentsWithGrades"] = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения статистики студентов: {ex.Message}");
                LogService.LogAction(null, "ERROR", $"Ошибка получения статистики студентов: {ex.Message}");

                // Возвращаем нулевые значения при ошибке
                stats["TotalStudents"] = 0;
                stats["TotalGroups"] = 0;
                stats["TotalSpecialties"] = 0;
                stats["StudentsWithGrades"] = 0;
            }
            return stats;
        }

        // Поиск студентов по имени
        public List<StudentWithGroup> SearchStudents(string searchTerm)
        {
            var students = new List<StudentWithGroup>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT * FROM get_students_with_groups()
                        WHERE LOWER(full_name) LIKE LOWER(@searchTerm)
                        ORDER BY full_name", conn))
                    {
                        cmd.Parameters.AddWithValue("searchTerm", $"%{searchTerm}%");
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                students.Add(new StudentWithGroup
                                {
                                    Id = reader.GetInt32(0),           // student_id
                                    FirstName = reader.GetString(1),   // first_name
                                    MiddleName = reader.GetString(2),  // middle_name
                                    LastName = reader.GetString(3),    // last_name
                                    GroupId = reader.GetInt32(4),      // group_id
                                    GroupName = reader.GetString(5),   // group_name
                                    SpecialtyName = reader.GetString(6), // specialty_name
                                    CourseNumber = reader.GetInt32(7), // course_number
                                    FullName = reader.GetString(8)     // full_name
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка поиска студентов: {ex.Message}");
                LogService.LogAction(null, "ERROR", $"Ошибка поиска студентов: {ex.Message}");
            }
            return students;
        }

        // Получить подробную информацию о студенте
        public StudentWithGroup GetStudentDetails(int studentId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT * FROM get_students_with_groups()
                        WHERE student_id = @studentId", conn))
                    {
                        cmd.Parameters.AddWithValue("studentId", studentId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new StudentWithGroup
                                {
                                    Id = reader.GetInt32(0),           // student_id
                                    FirstName = reader.GetString(1),   // first_name
                                    MiddleName = reader.GetString(2),  // middle_name
                                    LastName = reader.GetString(3),    // last_name
                                    GroupId = reader.GetInt32(4),      // group_id
                                    GroupName = reader.GetString(5),   // group_name
                                    SpecialtyName = reader.GetString(6), // specialty_name
                                    CourseNumber = reader.GetInt32(7), // course_number
                                    FullName = reader.GetString(8)     // full_name
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения данных студента {studentId}: {ex.Message}");
                LogService.LogAction(null, "ERROR", $"Ошибка получения данных студента {studentId}: {ex.Message}");
            }
            return null;
        }
    }
}