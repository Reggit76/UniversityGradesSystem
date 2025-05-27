using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversityGradesSystem.Models
{
    public class Semester
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int CourseId { get; set; }
    }
}