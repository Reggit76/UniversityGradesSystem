using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UniversityGradesSystem.Models;
using UniversityGradesSystem.Services;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace UniversityGradesSystem.Forms
{
    public partial class MainForm : Form
    {
        private int userId;
        private string role;
        private string connString;

        private TabControl tabControl;
        private DataGridView studentGrid;
        private DataGridView disciplineGrid;
        private Chart chart;

        public MainForm(int userId, string role)
        {
            InitializeComponent();
            this.userId = userId;
            this.role = role;
            this.Text = $"Учет успеваемости - {role.ToUpper()}";

            this.connString = DatabaseManager.Instance.GetConnectionString();

            // Логирование входа пользователя
            DatabaseManager.Instance.LogAction(userId, "LOGIN", $"Пользователь вошел в систему. [ID, Роль]: [{userId}, {role}]");

            // Инициализация элементов интерфейса
            InitializeUI();
            InitializeRoleUI();
        }

        private void InitializeUI()
        {
            StatusStrip statusStrip = new StatusStrip();
            ToolStripStatusLabel statusLabel = new ToolStripStatusLabel();
            statusLabel.Text = $"Пользователь: userid = {userId}, role = {role}";

            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);

            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            TabPage studentTab = new TabPage("Студенты");
            studentGrid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true };
            studentTab.Controls.Add(studentGrid);
            tabControl.TabPages.Add(studentTab);

            TabPage disciplineTab = new TabPage("Мои дисциплины");
            disciplineGrid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true };
            disciplineTab.Controls.Add(disciplineGrid);
            tabControl.TabPages.Add(disciplineTab);

            TabPage reportTab = new TabPage("Аналитика");
            chart = new Chart();
            chart.Dock = DockStyle.Fill;
            chart.ChartAreas.Add(new ChartArea("Performance"));
            chart.Series.Add(new Series("Performance") { ChartType = SeriesChartType.Pie });
            reportTab.Controls.Add(chart);
            tabControl.TabPages.Add(reportTab);

            this.Controls.Add(tabControl);
        }

        private void InitializeRoleUI()
        {
            try
            {
                if (role == "teacher")
                {
                    LoadTeacherDisciplines();
                    HideTabPage("Студенты");
                    AddGradeEntryTab();
                }
                else if (role == "admin")
                {
                    LoadStudents();
                    AddAdminTabs();
                }

                LoadAnalytics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}");
            }
        }

        private void AddGradeEntryTab()
        {
            try
            {
                TabPage gradeTab = new TabPage("Выставить оценки");

                // Создаем форму выставления оценок
                GradeEntryForm gradeForm = new GradeEntryForm(userId);

                // Настраиваем форму для встраивания
                gradeForm.TopLevel = false;
                gradeForm.FormBorderStyle = FormBorderStyle.None;
                gradeForm.Dock = DockStyle.Fill;
                gradeForm.Visible = true; // ВАЖНО: делаем форму видимой

                // Добавляем форму на вкладку
                gradeTab.Controls.Add(gradeForm);

                // Показываем форму
                gradeForm.Show();

                // Добавляем вкладку в TabControl
                tabControl.TabPages.Add(gradeTab);

                DatabaseManager.Instance.LogAction(userId, "TAB_CREATED", "Создана вкладка выставления оценок");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания вкладки выставления оценок: {ex.Message}");
                DatabaseManager.Instance.LogAction(userId, "ERROR", $"Ошибка создания вкладки оценок: {ex.Message}");
            }
        }

        private void AddAdminTabs()
        {
            TabPage addStudentTab = new TabPage("Добавить студента");
            Button btnAddStudent = new Button { Text = "Добавить студента", Dock = DockStyle.Top };
            btnAddStudent.Click += (sender, e) =>
            {
                AddStudentForm form = new AddStudentForm(userId);
                form.ShowDialog();
            };
            addStudentTab.Controls.Add(btnAddStudent);
            tabControl.TabPages.Add(addStudentTab);

            TabPage addGroupTab = new TabPage("Добавить группу");
            Button btnAddGroup = new Button { Text = "Добавить группу", Dock = DockStyle.Top };
            btnAddGroup.Click += (sender, e) => new AddGroupForm(userId).ShowDialog();
            addGroupTab.Controls.Add(btnAddGroup);
            tabControl.TabPages.Add(addGroupTab);
        }

        private void LoadStudents()
        {
            var studentService = new StudentService(this.connString);
            var students = studentService.GetAllStudents();
            studentGrid.DataSource = students;
        }

        private void LoadTeacherDisciplines()
        {
            var disciplineService = new DisciplineService(this.connString);
            var teacherService = new TeacherService(this.connString);

            int teacherId = teacherService.GetTeacherId(userId).Value;

            var disciplines = disciplineService.GetTeacherDisciplines(teacherId);
            disciplineGrid.DataSource = disciplines;
        }

        private void LoadAnalytics()
        {
            var analyticsService = new AnalyticsService(this.connString);
            decimal excellent = analyticsService.GetExcellentPercentage();
            decimal failing = analyticsService.GetFailingPercentage();

            chart.Series["Performance"].Points.Clear();
            chart.Series["Performance"].Points.AddXY("Отличники", excellent);
            chart.Series["Performance"].Points.AddXY("Неуспевающие", failing);
        }

        private void HideTabPage(string tabName)
        {
            foreach (TabPage page in tabControl.TabPages)
            {
                if (page.Text == tabName)
                {
                    tabControl.TabPages.Remove(page);
                    DatabaseManager.Instance.LogAction(userId, "TAB_HIDDEN", $"Скрыта вкладка: {tabName}");
                    break;
                }
            }
        }
    }
}