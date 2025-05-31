using Npgsql;
using System;
using System.Collections.Generic;
using UniversityGradesSystem.Models;

namespace UniversityGradesSystem.Services
{
    public class SpecialtyService
    {
        private readonly string _connectionString;

        public SpecialtyService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Получение всех специальностей
        public List<Specialty> GetAllSpecialties()
        {
            var specialties = new List<Specialty>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT id, name FROM specialties ORDER BY name", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                specialties.Add(new Specialty
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DatabaseManager.Instance.LogAction(null, "ERROR", $"Ошибка загрузки специальностей: {ex.Message}");
            }
            return specialties;
        }

        // Добавление новой специальности
        public bool AddSpecialty(Specialty specialty, int adminUserId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO specialties (name)
                        VALUES (@name)", conn))
                    {
                        cmd.Parameters.AddWithValue("name", specialty.Name);
                        cmd.ExecuteNonQuery();
                        DatabaseManager.Instance.LogAction(adminUserId, "ADD_SPECIALTY", $"Добавлена специальность: {specialty.Name}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                DatabaseManager.Instance.LogAction(adminUserId, "ERROR", $"Ошибка добавления специальности: {ex.Message}");
                return false;
            }
        }

        // Проверка существования специальности по названию
        public bool SpecialtyExists(string name)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM specialties WHERE LOWER(name) = LOWER(@name)", conn))
                    {
                        cmd.Parameters.AddWithValue("name", name);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка проверки существования специальности: {ex.Message}");
                return false;
            }
        }

        // Получение дисциплин специальности по семестрам
        public List<SpecialtyCurriculum> GetSpecialtyCurriculum(int specialtyId)
        {
            var curriculum = new List<SpecialtyCurriculum>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT sc.id, sc.specialty_id, sc.discipline_id, sc.semester_id,
                               d.name as discipline_name, s.number as semester_number, c.number as course_number
                        FROM specialty_curriculum sc
                        JOIN disciplines d ON sc.discipline_id = d.id
                        JOIN semesters s ON sc.semester_id = s.id
                        JOIN courses c ON s.course_id = c.id
                        WHERE sc.specialty_id = @specialtyId
                        ORDER BY c.number, s.number, d.name", conn))
                    {
                        cmd.Parameters.AddWithValue("specialtyId", specialtyId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                curriculum.Add(new SpecialtyCurriculum
                                {
                                    Id = reader.GetInt32(0),
                                    SpecialtyId = reader.GetInt32(1),
                                    DisciplineId = reader.GetInt32(2),
                                    SemesterId = reader.GetInt32(3),
                                    DisciplineName = reader.GetString(4),
                                    SemesterNumber = reader.GetInt32(5),
                                    CourseNumber = reader.GetInt32(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки учебного плана: {ex.Message}");
            }
            return curriculum;
        }

        // Добавление дисциплины в учебный план специальности
        public bool AddDisciplineToSpecialty(int specialtyId, int disciplineId, int semesterId, int adminUserId)
        {
            try
            {
                // Проверяем, не существует ли уже такая связка
                if (DisciplineExistsInSpecialty(specialtyId, disciplineId, semesterId))
                {
                    return false; // Уже существует
                }

                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO specialty_curriculum (specialty_id, discipline_id, semester_id)
                        VALUES (@specialtyId, @disciplineId, @semesterId)", conn))
                    {
                        cmd.Parameters.AddWithValue("specialtyId", specialtyId);
                        cmd.Parameters.AddWithValue("disciplineId", disciplineId);
                        cmd.Parameters.AddWithValue("semesterId", semesterId);
                        cmd.ExecuteNonQuery();

                        DatabaseManager.Instance.LogAction(adminUserId, "ADD_CURRICULUM",
                            $"Добавлена дисциплина в учебный план: специальность={specialtyId}, дисциплина={disciplineId}, семестр={semesterId}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                DatabaseManager.Instance.LogAction(adminUserId, "ERROR", $"Ошибка добавления дисциплины в учебный план: {ex.Message}");
                return false;
            }
        }

        // Удаление дисциплины из учебного плана специальности
        public bool RemoveDisciplineFromSpecialty(int curriculumId, int adminUserId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("DELETE FROM specialty_curriculum WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("id", curriculumId);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            DatabaseManager.Instance.LogAction(adminUserId, "REMOVE_CURRICULUM",
                                $"Удалена дисциплина из учебного плана: id={curriculumId}");
                        }

                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                DatabaseManager.Instance.LogAction(adminUserId, "ERROR", $"Ошибка удаления дисциплины из учебного плана: {ex.Message}");
                return false;
            }
        }

        // Проверка существования дисциплины в учебном плане
        private bool DisciplineExistsInSpecialty(int specialtyId, int disciplineId, int semesterId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT COUNT(*) FROM specialty_curriculum 
                        WHERE specialty_id = @specialtyId AND discipline_id = @disciplineId AND semester_id = @semesterId", conn))
                    {
                        cmd.Parameters.AddWithValue("specialtyId", specialtyId);
                        cmd.Parameters.AddWithValue("disciplineId", disciplineId);
                        cmd.Parameters.AddWithValue("semesterId", semesterId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка проверки существования дисциплины в учебном плане: {ex.Message}");
                return false;
            }
        }

        // Получение всех дисциплин
        public List<Discipline> GetAllDisciplines()
        {
            var disciplines = new List<Discipline>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT id, name FROM disciplines ORDER BY name", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                disciplines.Add(new Discipline
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки дисциплин: {ex.Message}");
            }
            return disciplines;
        }

        // Получение всех семестров
        public List<SemesterDisplay> GetAllSemesters()
        {
            var semesters = new List<SemesterDisplay>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT s.id, s.number, c.number as course_number 
                        FROM semesters s 
                        JOIN courses c ON s.course_id = c.id 
                        ORDER BY c.number, s.number", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                semesters.Add(new SemesterDisplay
                                {
                                    Id = reader.GetInt32(0),
                                    Number = reader.GetInt32(1),
                                    CourseNumber = reader.GetInt32(2),
                                    DisplayText = $"{reader.GetInt32(1)} семестр ({reader.GetInt32(2)} курс)"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки семестров: {ex.Message}");
            }
            return semesters;
        }

        // Удаление специальности
        public bool DeleteSpecialty(int specialtyId, int adminUserId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Сначала удаляем связанные записи из учебного плана
                            using (var cmd = new NpgsqlCommand("DELETE FROM specialty_curriculum WHERE specialty_id = @specialtyId", conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.Parameters.AddWithValue("specialtyId", specialtyId);
                                cmd.ExecuteNonQuery();
                            }

                            // Затем удаляем саму специальность
                            using (var cmd = new NpgsqlCommand("DELETE FROM specialties WHERE id = @specialtyId", conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.Parameters.AddWithValue("specialtyId", specialtyId);
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    transaction.Commit();
                                    DatabaseManager.Instance.LogAction(adminUserId, "DELETE_SPECIALTY",
                                        $"Удалена специальность: id={specialtyId}");
                                    return true;
                                }
                                else
                                {
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
                DatabaseManager.Instance.LogAction(adminUserId, "ERROR", $"Ошибка удаления специальности: {ex.Message}");
                return false;
            }
        }

        // Обновление специальности
        public bool UpdateSpecialty(Specialty specialty, int adminUserId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("UPDATE specialties SET name = @name WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("name", specialty.Name);
                        cmd.Parameters.AddWithValue("id", specialty.Id);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            DatabaseManager.Instance.LogAction(adminUserId, "UPDATE_SPECIALTY",
                                $"Обновлена специальность: {specialty.Name}");
                        }

                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                DatabaseManager.Instance.LogAction(adminUserId, "ERROR", $"Ошибка обновления специальности: {ex.Message}");
                return false;
            }
        }
    }
}