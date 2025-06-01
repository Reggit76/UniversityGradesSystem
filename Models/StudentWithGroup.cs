// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversityGradesSystem.Models
{
    // Расширенная модель студента с информацией о группе
    public class StudentWithGroup
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string SpecialtyName { get; set; }
        public int CourseNumber { get; set; }
        public string FullName { get; set; }

        // Вычисляемые свойства для удобства отображения
        public string DisplayName => $"{LastName} {FirstName} {MiddleName}";
        public string GroupInfo => $"{GroupName} ({SpecialtyName}, {CourseNumber} курс)";
    }
}