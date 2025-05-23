using Npgsql;
using System;
using System.Windows.Forms;
using UniversityGradesSystem.Services;

namespace UniversityGradesSystem.Forms
{
    public partial class AddGroupForm : Form
    {
        private int adminUserId;

        public AddGroupForm(int adminUserId)
        {
            InitializeComponent();
            this.adminUserId = adminUserId;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = new NpgsqlConnection(DatabaseManager.Instance.GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO groups (name, specialty_id, course_id)
                        VALUES (@name, @specialty, @course)", conn))
                    {
                        cmd.Parameters.AddWithValue("name", txtName.Text);
                        cmd.Parameters.AddWithValue("specialty", int.Parse(txtSpecialty.Text));
                        cmd.Parameters.AddWithValue("course", int.Parse(txtCourse.Text));
                        cmd.ExecuteNonQuery();
                        DatabaseManager.Instance.LogAction(adminUserId, "ADD_GROUP", $"Добавлена группа: {txtName.Text}");
                        MessageBox.Show("Группа добавлена!");
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления группы: {ex.Message}");
            }
        }
    }
}