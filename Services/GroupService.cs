// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using Npgsql;
using System;
using System.Collections.Generic;
using UniversityGradesSystem.Models;

namespace UniversityGradesSystem.Services
{
    public class GroupService
    {
        private readonly string _connectionString;

        public GroupService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Group> GetAllGroups()
        {
            var groups = new List<Group>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT id, name, specialty_id, course_id FROM groups", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                groups.Add(new Group
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    SpecialtyId = reader.GetInt32(2),
                                    CourseId = reader.GetInt32(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DatabaseManager.Instance.LogAction(null, "ERROR", $"Ошибка загрузки групп: {ex.Message}");
            }
            return groups;
        }

        // НОВЫЙ МЕТОД: Получение групп, связанных с преподавателем через дисциплины
        public List<Group> GetGroupsByTeacher(int teacherId)
        {
            var groups = new List<Group>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT DISTINCT g.id, g.name, g.specialty_id, g.course_id
                        FROM groups g
                        JOIN specialty_curriculum sc ON g.specialty_id = sc.specialty_id
                        JOIN teacher_discipline td ON sc.discipline_id = td.discipline_id
                        WHERE td.teacher_id = @teacherId
                        ORDER BY g.name", conn))
                    {
                        cmd.Parameters.AddWithValue("teacherId", teacherId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                groups.Add(new Group
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    SpecialtyId = reader.GetInt32(2),
                                    CourseId = reader.GetInt32(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DatabaseManager.Instance.LogAction(null, "ERROR", $"Ошибка загрузки групп преподавателя {teacherId}: {ex.Message}");
                Console.WriteLine($"Ошибка получения групп преподавателя: {ex.Message}");
            }
            return groups;
        }

        // НОВЫЙ МЕТОД: Проверка, связана ли группа с преподавателем
        public bool IsGroupRelatedToTeacher(int groupId, int teacherId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT COUNT(*)
                        FROM groups g
                        JOIN specialty_curriculum sc ON g.specialty_id = sc.specialty_id
                        JOIN teacher_discipline td ON sc.discipline_id = td.discipline_id
                        WHERE g.id = @groupId AND td.teacher_id = @teacherId", conn))
                    {
                        cmd.Parameters.AddWithValue("groupId", groupId);
                        cmd.Parameters.AddWithValue("teacherId", teacherId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка проверки связи группы {groupId} с преподавателем {teacherId}: {ex.Message}");
                return false;
            }
        }

        public bool AddGroup(Group group, int adminUserId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO groups (name, specialty_id, course_id)
                        VALUES (@name, @specialtyId, @courseId)", conn))
                    {
                        cmd.Parameters.AddWithValue("name", group.Name);
                        cmd.Parameters.AddWithValue("specialtyId", group.SpecialtyId);
                        cmd.Parameters.AddWithValue("courseId", group.CourseId);
                        cmd.ExecuteNonQuery();
                        DatabaseManager.Instance.LogAction(adminUserId, "ADD_GROUP", $"Добавлена группа: {group.Name}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                DatabaseManager.Instance.LogAction(adminUserId, "ERROR", $"Ошибка добавления группы: {ex.Message}");
                return false;
            }
        }
    }
}