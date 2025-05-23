using System;
using System.Windows.Forms;
using UniversityGradesSystem.Models;
using UniversityGradesSystem.Services;

namespace UniversityGradesSystem.Forms
{
    public partial class AddStudentForm : Form
    {
        private int adminUserId;
        private StudentService studentService;

        public AddStudentForm(int adminUserId)
        {
            InitializeComponent();
            this.adminUserId = adminUserId;
            this.studentService = new StudentService(DatabaseManager.Instance.GetConnectionString());
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var student = new Student
            {
                FirstName = txtFirstName.Text,
                MiddleName = txtMiddleName.Text,
                LastName = txtLastName.Text,
                GroupId = int.Parse(txtGroup.Text)
            };

            if (studentService.AddStudent(student, adminUserId))
            {
                MessageBox.Show("Студент добавлен!");
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка добавления студента!");
            }
        }
    }
}