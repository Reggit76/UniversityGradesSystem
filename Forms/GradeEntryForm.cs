using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UniversityGradesSystem.Models;
using UniversityGradesSystem.Services;
using MaterialSkin;
using MaterialSkin.Controls;

namespace UniversityGradesSystem.Forms
{
    public partial class GradeEntryForm : MaterialForm
    {
        // Удалены дублирующие объявления - они уже в Designer.cs
        private readonly GradeService gradeService;
        private readonly DisciplineService disciplineService;
        private readonly StudentService studentService;
        private readonly TeacherService teacherService;
        private readonly GroupService groupService;
        private readonly int userId;
        private int? teacherId;

        // Используем Material-компоненты
        private MaterialComboBox cmbGroup;
        private MaterialComboBox cmbDiscipline;
        private DataGridView dgvStudents;
        private MaterialButton btnSave;

        public GradeEntryForm(int userId)
        {
            this.userId = userId;
            string connString = DatabaseManager.Instance.GetConnectionString();

            // Инициализация сервисов
            gradeService = new GradeService(connString);
            disciplineService = new DisciplineService(connString);
            studentService = new StudentService(connString);
            teacherService = new TeacherService(connString);
            groupService = new GroupService(connString);

            // Получение teacherId
            teacherId = teacherService.GetTeacherId(userId);
            if (!teacherId.HasValue)
            {
                MessageBox.Show("Преподаватель не найден");
                this.Close();
                return;
            }

            InitializeComponent(); // Вызов метода инициализации из Designer.cs
            ApplyMaterialSkin();
            LoadGroups();
            LoadDisciplines();
        }

        private void ApplyMaterialSkin()
        {
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new MaterialSkin.ColorScheme(
                MaterialSkin.Primary.BlueGrey800,
                MaterialSkin.Primary.BlueGrey900,
                MaterialSkin.Primary.BlueGrey500,
                MaterialSkin.Accent.LightBlue200,
                MaterialSkin.TextShade.WHITE);
        }

        private void InitializeDataGridView()
        {
            dgvStudents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id",
                ReadOnly = true
            });
            dgvStudents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FullName",
                HeaderText = "ФИО",
                DataPropertyName = "FullName",
                ReadOnly = true
            });
            var gradeColumn = new DataGridViewComboBoxColumn
            {
                Name = "Grade",
                HeaderText = "Оценка",
                DataSource = new List<int> { 2, 3, 4, 5 },
                ValueType = typeof(int)
            };
            dgvStudents.Columns.Add(gradeColumn);
        }

        private void LoadGroups()
        {
            try
            {
                var groups = groupService.GetAllGroups();
                cmbGroup.DataSource = groups;
                cmbGroup.DisplayMember = "Name";
                cmbGroup.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки групп: {ex.Message}");
            }
        }

        private void LoadDisciplines()
        {
            try
            {
                var disciplines = disciplineService.GetTeacherDisciplines(teacherId.Value);
                cmbDiscipline.DataSource = disciplines;
                cmbDiscipline.DisplayMember = "Name";
                cmbDiscipline.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки дисциплин: {ex.Message}");
            }
        }

        private void LoadStudents()
        {
            if (cmbGroup.SelectedItem is Group selectedGroup)
            {
                try
                {
                    var students = studentService.GetStudentsByGroup(selectedGroup.Id);
                    dgvStudents.DataSource = students;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки студентов: {ex.Message}");
                }
            }
        }

        private void CmbGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private void CmbDiscipline_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbDiscipline.SelectedItem is Discipline selectedDiscipline)
                {
                    foreach (DataGridViewRow row in dgvStudents.Rows)
                    {
                        if (row.Cells["Grade"].Value != null)
                        {
                            int studentId = (int)row.Cells["Id"].Value;
                            int grade = (int)row.Cells["Grade"].Value;
                            int disciplineId = selectedDiscipline.Id;

                            // Используем корректный метод сервиса
                            gradeService.SaveGrade(studentId, disciplineId, grade);
                        }
                    }
                    MessageBox.Show("Оценки успешно сохранены!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения оценок: {ex.Message}");
            }
        }
    }
}