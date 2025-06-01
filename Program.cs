// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
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

            // Устанавливаем обработчик необработанных исключений
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                // Запускаем форму входа
                Application.Run(new LoginForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка приложения: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show($"Произошла ошибка: {e.Exception.Message}\n\nПриложение будет продолжать работу.",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Критическая ошибка: {ex.Message}\n\nПриложение будет закрыто.",
                    "Критическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}