using System;
using System.Windows.Forms;

namespace UniversityGradesSystem.Forms
{
    partial class LoginForm : Form
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnLogin;

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
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 250);
            this.Text = "Вход в систему";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // === Элементы интерфейса ===
            this.lblUsername = new Label { Text = "Логин:", Location = new System.Drawing.Point(50, 50), Width = 100 };
            this.txtUsername = new TextBox { Location = new System.Drawing.Point(150, 50), Width = 200 };
            this.lblPassword = new Label { Text = "Пароль:", Location = new System.Drawing.Point(50, 90), Width = 100 };
            this.txtPassword = new TextBox { Location = new System.Drawing.Point(150, 90), Width = 200, PasswordChar = '*' };
            this.btnLogin = new Button { Text = "Войти", Location = new System.Drawing.Point(150, 130), Width = 100 };

            // === Обработчик события ===
            this.btnLogin.Click += new EventHandler(btnLogin_Click);

            // === Добавление элементов на форму ===
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.btnLogin);
        }

        #endregion
    }
}