using System;
using System.Drawing;
using System.Windows.Forms;

namespace UniversityGradesSystem.Forms
{
    partial class LoginForm : Form
    {
        private System.ComponentModel.IContainer components = null;

        // UI элементы
        private Panel headerPanel;
        private Panel formPanel;
        private Panel infoPanel;
        private Panel buttonPanel;

        private Label titleLabel;
        private Label subtitleLabel;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblInfo;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();

            // === НАСТРОЙКИ ФОРМЫ ===
            this.Text = "Вход в систему учета успеваемости";
            this.Size = new Size(500, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 244, 247);
            this.Icon = SystemIcons.Shield;

            // === ЗАГОЛОВОЧНАЯ ПАНЕЛЬ ===
            this.headerPanel = new Panel();
            this.headerPanel.Size = new Size(500, 120);
            this.headerPanel.Location = new Point(0, 0);
            this.headerPanel.BackColor = Color.FromArgb(52, 73, 94);
            this.headerPanel.Dock = DockStyle.Top;

            // Заголовок
            this.titleLabel = new Label();
            this.titleLabel.Text = "🎓 СИСТЕМА УЧЕТА УСПЕВАЕМОСТИ";
            this.titleLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.titleLabel.ForeColor = Color.White;
            this.titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.titleLabel.Location = new Point(20, 25);
            this.titleLabel.Size = new Size(460, 35);

            // Подзаголовок
            this.subtitleLabel = new Label();
            this.subtitleLabel.Text = "Университетская информационная система";
            this.subtitleLabel.Font = new Font("Segoe UI", 10F);
            this.subtitleLabel.ForeColor = Color.FromArgb(189, 195, 199);
            this.subtitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.subtitleLabel.Location = new Point(20, 65);
            this.subtitleLabel.Size = new Size(460, 25);

            this.headerPanel.Controls.Add(this.titleLabel);
            this.headerPanel.Controls.Add(this.subtitleLabel);

            // === ПАНЕЛЬ ФОРМЫ ===
            this.formPanel = new Panel();
            this.formPanel.Size = new Size(400, 180);
            this.formPanel.Location = new Point(50, 150);
            this.formPanel.BackColor = Color.White;
            this.formPanel.BorderStyle = BorderStyle.FixedSingle;

            // === ЭЛЕМЕНТЫ ФОРМЫ ===

            // Лейбл "Логин"
            this.lblUsername = new Label();
            this.lblUsername.Text = "Логин:";
            this.lblUsername.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblUsername.ForeColor = Color.FromArgb(52, 73, 94);
            this.lblUsername.Location = new Point(25, 20);
            this.lblUsername.Size = new Size(350, 20);
            this.lblUsername.TextAlign = ContentAlignment.BottomLeft;

            // Поле "Логин"
            this.txtUsername = new TextBox();
            this.txtUsername.Font = new Font("Segoe UI", 11F);
            this.txtUsername.Location = new Point(25, 45);
            this.txtUsername.Size = new Size(350, 25);
            this.txtUsername.BorderStyle = BorderStyle.FixedSingle;
            this.txtUsername.BackColor = Color.White;
            this.txtUsername.ForeColor = Color.FromArgb(44, 62, 80);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.TabIndex = 0;

            // Лейбл "Пароль"
            this.lblPassword = new Label();
            this.lblPassword.Text = "Пароль:";
            this.lblPassword.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblPassword.ForeColor = Color.FromArgb(52, 73, 94);
            this.lblPassword.Location = new Point(25, 85);
            this.lblPassword.Size = new Size(350, 20);
            this.lblPassword.TextAlign = ContentAlignment.BottomLeft;

            // Поле "Пароль"
            this.txtPassword = new TextBox();
            this.txtPassword.Font = new Font("Segoe UI", 11F);
            this.txtPassword.Location = new Point(25, 110);
            this.txtPassword.Size = new Size(350, 25);
            this.txtPassword.BorderStyle = BorderStyle.FixedSingle;
            this.txtPassword.BackColor = Color.White;
            this.txtPassword.ForeColor = Color.FromArgb(44, 62, 80);
            this.txtPassword.PasswordChar = '●';
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.TabIndex = 1;

            // Добавляем элементы в панель формы
            this.formPanel.Controls.Add(this.lblUsername);
            this.formPanel.Controls.Add(this.txtUsername);
            this.formPanel.Controls.Add(this.lblPassword);
            this.formPanel.Controls.Add(this.txtPassword);

            // === ПАНЕЛЬ КНОПКИ ===
            this.buttonPanel = new Panel();
            this.buttonPanel.Size = new Size(400, 60);
            this.buttonPanel.Location = new Point(50, 350);
            this.buttonPanel.BackColor = Color.Transparent;

            // Кнопка "Войти"
            this.btnLogin = new Button();
            this.btnLogin.Text = "🔐 Войти в систему";
            this.btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.btnLogin.Size = new Size(200, 45);
            this.btnLogin.Location = new Point(100, 10);
            this.btnLogin.BackColor = Color.FromArgb(52, 152, 219);
            this.btnLogin.ForeColor = Color.White;
            this.btnLogin.FlatStyle = FlatStyle.Flat;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.Cursor = Cursors.Hand;
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.TabIndex = 2;
            this.btnLogin.Click += new EventHandler(this.btnLogin_Click);

            // Эффект наведения для кнопки
            this.btnLogin.MouseEnter += (s, e) => {
                this.btnLogin.BackColor = Color.FromArgb(41, 128, 185);
            };
            this.btnLogin.MouseLeave += (s, e) => {
                this.btnLogin.BackColor = Color.FromArgb(52, 152, 219);
            };

            this.buttonPanel.Controls.Add(this.btnLogin);

            // === ИНФОРМАЦИОННАЯ ПАНЕЛЬ ===
            this.infoPanel = new Panel();
            this.infoPanel.Size = new Size(400, 130);
            this.infoPanel.Location = new Point(50, 430);
            this.infoPanel.BackColor = Color.FromArgb(236, 240, 241);
            this.infoPanel.BorderStyle = BorderStyle.FixedSingle;

            // Информационный текст
            this.lblInfo = new Label();
            this.lblInfo.Text = "По поводу данных входа обращайтесь в тех отдел!";
            this.lblInfo.Font = new Font("Segoe UI", 9F);
            this.lblInfo.ForeColor = Color.FromArgb(52, 73, 94);
            this.lblInfo.Location = new Point(15, 10);
            this.lblInfo.Size = new Size(370, 30);
            this.lblInfo.TextAlign = ContentAlignment.TopLeft;

            this.infoPanel.Controls.Add(this.lblInfo);

            // === ДОБАВЛЯЕМ ВСЕ ПАНЕЛИ НА ФОРМУ ===
            this.Controls.Add(this.headerPanel);
            this.Controls.Add(this.formPanel);
            this.Controls.Add(this.buttonPanel);
            this.Controls.Add(this.infoPanel);

            // === ОБРАБОТЧИКИ СОБЫТИЙ ===

            // Enter для перехода между полями и входа
            this.txtUsername.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    this.txtPassword.Focus();
                    e.SuppressKeyPress = true;
                }
            };

            this.txtPassword.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    this.btnLogin_Click(this.btnLogin, EventArgs.Empty);
                    e.SuppressKeyPress = true;
                }
            };

            // Escape для закрытия
            this.KeyPreview = true;
            this.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                }
            };

            // Устанавливаем фокус на поле логина
            this.ActiveControl = this.txtUsername;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}