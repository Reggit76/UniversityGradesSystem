using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversityGradesSystem.Models
{
    public class DisciplineSemester
    {
        public int Id { get; set; }
        public int DisciplineId { get; set; }
        public int SemesterId { get; set; }
    }
}
