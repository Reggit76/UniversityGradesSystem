using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversityGradesSystem.Models
{
    // Вспомогательный класс для отображения курсов в комбобоксах
    public class CourseDisplay
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string DisplayText { get; set; }
    }

    // Вспомогательный класс для отображения семестров в комбобоксах
    public class SemesterDisplay
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int CourseNumber { get; set; }
        public string DisplayText { get; set; }
    }
}