using System;
using System.Windows.Forms;
using UniversityGradesSystem.Forms;

namespace UniversityGradesSystem
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}
