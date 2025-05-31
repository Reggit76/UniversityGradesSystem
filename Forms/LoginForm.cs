using System;
using System.Windows.Forms;
using UniversityGradesSystem.Services;
using UniversityGradesSystem.Models;
using System.Drawing;

namespace UniversityGradesSystem.Forms
{
    public partial class LoginForm : Form
    {
        private bool isReturningFromMainForm = false;

        public LoginForm(bool returningFromMainForm = false)
        {
            this.isReturningFromMainForm = returningFromMainForm;

            InitializeComponent();

            // Установка начальной строки подключения
            string initialConn = "Server=localhost;Port=5432;Database=UniversityDB;User Id=app_user;Password=app_password;";
            DatabaseManager.Instance.SetConnectionString(initialConn);

            if (returningFromMainForm)
            {
                ShowReturnMessage();
            }
        }

        // Добавляем конструктор по умолчанию для совместимости
        public LoginForm() : this(false)
        {
        }

        private void ShowReturnMessage()
        {
            // Показываем сообщение о выходе из системы
            Label returnMessage = new Label
            {
                Text = "✅ Вы успешно вышли из системы",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113),
                BackColor = Color.FromArgb(212, 237, 218),
                Location = new Point(50, 580),
                Size = new Size(400, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };

            this.Controls.Add(returnMessage);
            returnMessage.BringToFront();

            // Удаляем сообщение через 3 секунды
            Timer timer = new Timer();
            timer.Interval = 3000;
            timer.Tick += (s, e) =>
            {
                if (this.Controls.Contains(returnMessage))
                {
                    this.Controls.Remove(returnMessage);
                }
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text.Trim();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ShowErrorMessage("Введите логин и пароль!");
                    return;
                }

                // Показываем индикатор загрузки
                ShowLoadingIndicator(true);

                var (userId, role) = DatabaseManager.Instance.AuthenticateUser(username, password);

                if (userId.HasValue)
                {
                    // Обновляем строку подключения на основе учетных данных пользователя
                    string userConnection = $"Server=localhost;Port=5432;Database=UniversityDB;User Id={username};Password={password};";
                    DatabaseManager.Instance.SetConnectionString(userConnection);

                    // Логируем вход
                    DatabaseManager.Instance.LogAction(userId.Value, "LOGIN", $"Пользователь {username} вошел в систему");

                    // Скрываем форму входа
                    this.Hide();

                    try
                    {
                        // Создаем и показываем главную форму
                        MainForm mainForm = new MainForm(userId.Value, role);

                        // Подписываемся на событие закрытия главной формы
                        mainForm.FormClosed += (s, args) =>
                        {
                            // Показываем форму входа снова
                            this.Show();
                            this.WindowState = FormWindowState.Normal;
                            this.BringToFront();
                            this.Focus();

                            // Очищаем поля
                            ClearFields();

                            // Показываем сообщение о выходе
                            ShowReturnMessage();
                        };

                        mainForm.Show();
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage($"Ошибка открытия главной формы: {ex.Message}");
                        this.Show(); // Показываем форму входа обратно при ошибке
                    }
                }
                else
                {
                    ShowErrorMessage("Неверный логин или пароль!");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка входа в систему: {ex.Message}");
            }
            finally
            {
                ShowLoadingIndicator(false);
            }
        }

        private void ShowLoadingIndicator(bool show)
        {
            if (show)
            {
                btnLogin.Text = "⏳ Вход в систему...";
                btnLogin.Enabled = false;
                btnLogin.BackColor = Color.FromArgb(149, 165, 166);
            }
            else
            {
                btnLogin.Text = "🔐 Войти в систему";
                btnLogin.Enabled = true;
                btnLogin.BackColor = Color.FromArgb(52, 152, 219);
            }
        }

        private void ShowErrorMessage(string message)
        {
            // Удаляем предыдущее сообщение об ошибке, если есть
            Control existingError = null;
            foreach (Control control in this.Controls)
            {
                if (control.Tag?.ToString() == "ErrorMessage")
                {
                    existingError = control;
                    break;
                }
            }

            if (existingError != null)
            {
                this.Controls.Remove(existingError);
            }

            // Создаем новое сообщение об ошибке
            Label errorLabel = new Label
            {
                Text = "❌ " + message,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(231, 76, 60),
                BackColor = Color.FromArgb(248, 215, 218),
                Location = new Point(50, 580),
                Size = new Size(400, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                Tag = "ErrorMessage",
                BorderStyle = BorderStyle.FixedSingle
            };

            this.Controls.Add(errorLabel);
            errorLabel.BringToFront();

            // Удаляем сообщение об ошибке через 5 секунд
            Timer timer = new Timer();
            timer.Interval = 5000;
            timer.Tick += (s, e) =>
            {
                if (this.Controls.Contains(errorLabel))
                {
                    this.Controls.Remove(errorLabel);
                }
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        private void ClearFields()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtUsername.Focus();
        }

        // Переопределяем метод закрытия формы
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Если это основное закрытие приложения (не возврат из MainForm)
            if (!isReturningFromMainForm)
            {
                var result = MessageBox.Show(
                    "Вы действительно хотите выйти из приложения?",
                    "Подтверждение выхода",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            base.OnFormClosing(e);
        }

        // Обработка нажатий клавиш
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                btnLogin_Click(this, EventArgs.Empty);
                return true;
            }
            else if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Статический метод для создания новой формы входа после выхода
        public static void ShowLoginFormAfterLogout()
        {
            Application.Run(new LoginForm(true));
        }
    }
}