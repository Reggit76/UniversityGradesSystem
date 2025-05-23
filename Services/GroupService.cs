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