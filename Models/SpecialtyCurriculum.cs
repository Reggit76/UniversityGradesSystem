using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversityGradesSystem.Models
{
    // Модель для учебного плана специальности (связка специальность-дисциплина-семестр)
    public class SpecialtyCurriculum
    {
        public int Id { get; set; }
        public int SpecialtyId { get; set; }
        public int DisciplineId { get; set; }
        public int SemesterId { get; set; }

        // Дополнительные поля для отображения
        public string DisciplineName { get; set; }
        public int SemesterNumber { get; set; }
        public int CourseNumber { get; set; }

        // Форматированное отображение
        public string SemesterDisplay => $"{SemesterNumber} семестр ({CourseNumber} курс)";
    }
}