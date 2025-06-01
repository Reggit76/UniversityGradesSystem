// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversityGradesSystem.Models
{
    public class Grade
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int DisciplineSemesterId { get; set; }
        public int Value { get; set; }
    }
}
