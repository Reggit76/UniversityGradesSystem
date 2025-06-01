// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversityGradesSystem.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string ActionType { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
    }
}