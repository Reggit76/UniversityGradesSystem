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

        // Получить всех студентов
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
                //LogService.LogAction(adminUserId, "ADD_STUDENT", $"Добавлен студент: {student.FirstName} {student.LastName}");
                Console.WriteLine($"Ошибка при загрузке студентов: {ex.Message}");
            }

            return students;
        }
    }
}