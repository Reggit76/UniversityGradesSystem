using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace UniversityGradesSystem.Forms
{
    partial class AddGroupForm
    {
        private Label lblName;
        private TextBox txtName;
        private Label lblSpecialty;
        private TextBox txtSpecialty;
        private Label lblCourse;
        private TextBox txtCourse;
        private Button btnSave;

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AddGroupForm
            // 
            this.ClientSize = new System.Drawing.Size(400, 200);
            this.Text = "Добавить группу";

            // 
            // lblName
            // 
            this.lblName = new Label { Text = "Название:", Location = new System.Drawing.Point(50, 30) };

            // 
            // txtName
            // 
            this.txtName = new TextBox { Location = new System.Drawing.Point(150, 27) };

            // 
            // lblSpecialty
            // 
            this.lblSpecialty = new Label { Text = "ID специальности:", Location = new System.Drawing.Point(50, 60) };

            // 
            // txtSpecialty
            // 
            this.txtSpecialty = new TextBox { Location = new System.Drawing.Point(150, 57) };

            // 
            // lblCourse
            // 
            this.lblCourse = new Label { Text = "Курс ID:", Location = new System.Drawing.Point(50, 90) };

            // 
            // txtCourse
            // 
            this.txtCourse = new TextBox { Location = new System.Drawing.Point(150, 87) };

            // 
            // btnSave
            // 
            this.btnSave = new Button { Text = "Сохранить", Location = new System.Drawing.Point(150, 120) };
            this.btnSave.Click += new EventHandler(btnSave_Click);

            // 
            // Добавление элементов
            // 
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblSpecialty);
            this.Controls.Add(this.txtSpecialty);
            this.Controls.Add(this.lblCourse);
            this.Controls.Add(this.txtCourse);
            this.Controls.Add(this.btnSave);

            this.ResumeLayout(false);
        }
    }
}