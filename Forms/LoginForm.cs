using System;
using System.Windows.Forms;
using UniversityGradesSystem.Services;
using UniversityGradesSystem.Models;

namespace UniversityGradesSystem.Forms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            // Установка начальной строки подключения
            string initialConn = "Server=localhost;Port=5432;Database=UniversityDB;User Id=app_user;Password=app_password;";
            DatabaseManager.Instance.SetConnectionString(initialConn);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!");
                return;
            }

            var (userId, role) = DatabaseManager.Instance.AuthenticateUser(username, password);
            if (userId.HasValue)
            {
                // Обновляем строку подключения на основе учетных данных пользователя
                string userConnection = $"Server=localhost;Port=5432;Database=UniversityDB;User Id={username};Password={password};";
                DatabaseManager.Instance.SetConnectionString(userConnection);

                // Логируем вход
                DatabaseManager.Instance.LogAction(userId.Value, "LOGIN", $"Пользователь {username} вошел в систему");

                MainForm mainForm = new MainForm(userId.Value, role);
                mainForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!", "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}