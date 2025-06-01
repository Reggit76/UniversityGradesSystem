// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
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
