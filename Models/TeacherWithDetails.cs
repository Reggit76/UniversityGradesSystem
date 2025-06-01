// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversityGradesSystem.Models
{
    // Расширенная модель преподавателя с дополнительной информацией
    public class TeacherWithDetails
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public int DisciplinesCount { get; set; }

        // Вычисляемые свойства для удобства отображения
        public string FullName => $"{LastName} {FirstName} {MiddleName}";
        public string ShortName => $"{LastName} {FirstName[0]}.{MiddleName[0]}.";
    }

    // Модель для управления назначениями дисциплин преподавателю
    public class TeacherDisciplineAssignment
    {
        public int DisciplineId { get; set; }
        public string DisciplineName { get; set; }
        public bool IsAssigned { get; set; }
    }

    // Модель для отображения информации о преподавателе в формах добавления
    public class TeacherFormData
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public List<int> SelectedDisciplineIds { get; set; } = new List<int>();

        // Валидация данных
        public bool IsValid(out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(FirstName))
            {
                errorMessage = "Введите имя преподавателя";
                return false;
            }

            if (string.IsNullOrWhiteSpace(MiddleName))
            {
                errorMessage = "Введите отчество преподавателя";
                return false;
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                errorMessage = "Введите фамилию преподавателя";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                errorMessage = "Введите логин для входа";
                return false;
            }

            if (Username.Length < 3)
            {
                errorMessage = "Логин должен содержать не менее 3 символов";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                errorMessage = "Введите пароль";
                return false;
            }

            if (Password.Length < 4)
            {
                errorMessage = "Пароль должен содержать не менее 4 символов";
                return false;
            }

            if (SelectedDisciplineIds.Count == 0)
            {
                errorMessage = "Выберите хотя бы одну дисциплину";
                return false;
            }

            return true;
        }

        // Полное имя для отображения
        public string FullName => $"{LastName} {FirstName} {MiddleName}";
    }
}