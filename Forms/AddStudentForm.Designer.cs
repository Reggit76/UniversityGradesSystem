using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace UniversityGradesSystem.Forms
{
    partial class AddStudentForm
    {
        private Label lblFirstName;
        private TextBox txtFirstName;
        private Label lblMiddleName;
        private TextBox txtMiddleName;
        private Label lblLastName;
        private TextBox txtLastName;
        private Label lblGroup;
        private TextBox txtGroup;
        private Button btnSave;

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AddStudentForm
            // 
            this.ClientSize = new System.Drawing.Size(400, 200);
            this.Text = "Добавить студента";

            // 
            // lblFirstName
            // 
            this.lblFirstName = new Label { Text = "Имя:", Location = new System.Drawing.Point(50, 30) };

            // 
            // txtFirstName
            // 
            this.txtFirstName = new TextBox { Location = new System.Drawing.Point(150, 27) };

            // 
            // lblMiddleName
            // 
            this.lblMiddleName = new Label { Text = "Отчество:", Location = new System.Drawing.Point(50, 60) };

            // 
            // txtMiddleName
            // 
            this.txtMiddleName = new TextBox { Location = new System.Drawing.Point(150, 57) };

            // 
            // lblLastName
            // 
            this.lblLastName = new Label { Text = "Фамилия:", Location = new System.Drawing.Point(50, 90) };

            // 
            // txtLastName
            // 
            this.txtLastName = new TextBox { Location = new System.Drawing.Point(150, 87) };

            // 
            // lblGroup
            // 
            this.lblGroup = new Label { Text = "Группа ID:", Location = new System.Drawing.Point(50, 120) };

            // 
            // txtGroup
            // 
            this.txtGroup = new TextBox { Location = new System.Drawing.Point(150, 117) };

            // 
            // btnSave
            // 
            this.btnSave = new Button { Text = "Сохранить", Location = new System.Drawing.Point(150, 150) };
            this.btnSave.Click += new EventHandler(btnSave_Click);

            // 
            // Добавление элементов
            // 
            this.Controls.Add(this.lblFirstName);
            this.Controls.Add(this.txtFirstName);
            this.Controls.Add(this.lblMiddleName);
            this.Controls.Add(this.txtMiddleName);
            this.Controls.Add(this.lblLastName);
            this.Controls.Add(this.txtLastName);
            this.Controls.Add(this.lblGroup);
            this.Controls.Add(this.txtGroup);
            this.Controls.Add(this.btnSave);

            this.ResumeLayout(false);
        }
    }
}