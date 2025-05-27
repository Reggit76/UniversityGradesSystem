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

        public MainForm(int userId, string role)
        {
            InitializeComponent();
            this.userId = userId;
            this.role = role;
            this.Text = $"Система учета успеваемости - {role.ToUpper()}";
            this.WindowState = FormWindowState.Maximized; // Развернуть окно

            this.connString = DatabaseManager.Instance.GetConnectionString();

            // Логирование входа пользователя
            DatabaseManager.Instance.LogAction(userId, "LOGIN", $"Пользователь вошел в систему. [ID, Роль]: [{userId}, {role}]");

            // Инициализация элементов интерфейса
            InitializeUI();
            InitializeRoleUI();
        }

        private void InitializeUI()
        {
            // === Настройка формы ===
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Icon = SystemIcons.Application; // Добавляем иконку

            // === Статус бар ===
            StatusStrip statusStrip = new StatusStrip();
            statusStrip.BackColor = Color.FromArgb(52, 73, 94);

            ToolStripStatusLabel statusLabel = new ToolStripStatusLabel();
            statusLabel.Text = $"Пользователь: {userId} | Роль: {role.ToUpper()} | {DateTime.Now:HH:mm:ss}";
            statusLabel.ForeColor = Color.White;
            statusLabel.Font = new Font("Segoe UI", 9F);

            ToolStripStatusLabel connectionStatus = new ToolStripStatusLabel();
            connectionStatus.Text = "🟢 Подключено к БД";
            connectionStatus.ForeColor = Color.LightGreen;
            connectionStatus.Spring = true;
            connectionStatus.TextAlign = ContentAlignment.MiddleRight;

            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, connectionStatus });
            this.Controls.Add(statusStrip);

            // === Главный TabControl ===
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 10F);
            tabControl.Appearance = TabAppearance.FlatButtons;
            tabControl.Padding = new Point(15, 8);
            tabControl.ItemSize = new Size(120, 35);

            // === Базовые вкладки ===
            CreateStudentsTab();
            CreateDisciplinesTab();
            CreateEnhancedAnalyticsTab(); // Новая улучшенная аналитика

            this.Controls.Add(tabControl);
        }

        private void CreateStudentsTab()
        {
            TabPage studentTab = new TabPage("👥 Студенты");
            studentTab.BackColor = Color.FromArgb(250, 250, 250);
            studentTab.Padding = new Padding(10);

            // Панель инструментов
            Panel toolPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Top,
                BackColor = Color.White,
                Padding = new Padding(10, 10, 10, 10)
            };
            toolPanel.BorderStyle = BorderStyle.FixedSingle;

            Label titleLabel = new Label
            {
                Text = "Список студентов",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(10, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            toolPanel.Controls.Add(titleLabel);

            // Таблица студентов
            studentGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Font = new Font("Segoe UI", 9F),
                GridColor = Color.FromArgb(230, 230, 230),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };

            // Стилизация заголовков
            studentGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            studentGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            studentGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            studentGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            studentTab.Controls.AddRange(new Control[] { toolPanel, studentGrid });
            tabControl.TabPages.Add(studentTab);
        }

        private void CreateDisciplinesTab()
        {
            TabPage disciplineTab = new TabPage("📚 Мои дисциплины");
            disciplineTab.BackColor = Color.FromArgb(250, 250, 250);
            disciplineTab.Padding = new Padding(10);

            // Панель инструментов
            Panel toolPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Top,
                BackColor = Color.White,
                Padding = new Padding(10, 10, 10, 10)
            };
            toolPanel.BorderStyle = BorderStyle.FixedSingle;

            Label titleLabel = new Label
            {
                Text = "Преподаваемые дисциплины",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(10, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            toolPanel.Controls.Add(titleLabel);

            // Таблица дисциплин
            disciplineGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Font = new Font("Segoe UI", 9F),
                GridColor = Color.FromArgb(230, 230, 230),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };

            // Стилизация заголовков
            disciplineGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            disciplineGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            disciplineGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            disciplineGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            disciplineTab.Controls.AddRange(new Control[] { toolPanel, disciplineGrid });
            tabControl.TabPages.Add(disciplineTab);
        }

        private void CreateEnhancedAnalyticsTab()
        {
            TabPage analyticsTab = new TabPage("📊 Аналитика");
            analyticsTab.BackColor = Color.FromArgb(240, 244, 247);

            try
            {
                // Встраиваем новую форму аналитики
                EnhancedAnalyticsForm analyticsForm = new EnhancedAnalyticsForm(userId, role);

                analyticsForm.TopLevel = false;
                analyticsForm.FormBorderStyle = FormBorderStyle.None;
                analyticsForm.Dock = DockStyle.Fill;
                analyticsForm.Visible = true;

                analyticsTab.Controls.Add(analyticsForm);
                analyticsForm.Show();

                DatabaseManager.Instance.LogAction(userId, "TAB_CREATED", "Создана улучшенная вкладка аналитики");
            }
            catch (Exception ex)
            {
                // В случае ошибки создаем простую заглушку
                Label errorLabel = new Label
                {
                    Text = $"Ошибка загрузки аналитики: {ex.Message}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = Color.Red
                };
                analyticsTab.Controls.Add(errorLabel);

                DatabaseManager.Instance.LogAction(userId, "ERROR", $"Ошибка создания вкладки аналитики: {ex.Message}");
            }

            tabControl.TabPages.Add(analyticsTab);
        }

        private void InitializeRoleUI()
        {
            try
            {
                if (role == "teacher")
                {
                    LoadTeacherDisciplines();
                    HideTabPage("👥 Студенты");
                    AddGradeEntryTab();
                }
                else if (role == "admin")
                {
                    LoadStudents();
                    AddAdminTabs();
                }

                DatabaseManager.Instance.LogAction(userId, "UI_INITIALIZED", $"Интерфейс инициализирован для роли: {role}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации интерфейса: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DatabaseManager.Instance.LogAction(userId, "ERROR", $"Ошибка инициализации UI: {ex.Message}");
            }
        }

        private void AddGradeEntryTab()
        {
            try
            {
                TabPage gradeTab = new TabPage("✏️ Выставить оценки");
                gradeTab.BackColor = Color.FromArgb(250, 250, 250);

                // Создаем форму выставления оценок
                GradeEntryForm gradeForm = new GradeEntryForm(userId);

                // Настраиваем форму для встраивания
                gradeForm.TopLevel = false;
                gradeForm.FormBorderStyle = FormBorderStyle.None;
                gradeForm.Dock = DockStyle.Fill;
                gradeForm.Visible = true;

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
                MessageBox.Show($"Ошибка создания вкладки выставления оценок: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DatabaseManager.Instance.LogAction(userId, "ERROR", $"Ошибка создания вкладки оценок: {ex.Message}");
            }
        }

        private void AddAdminTabs()
        {
            // === Вкладка добавления студента ===
            TabPage addStudentTab = new TabPage("➕ Добавить студента");
            addStudentTab.BackColor = Color.FromArgb(250, 250, 250);
            addStudentTab.Padding = new Padding(20);

            Panel studentPanel = new Panel
            {
                Size = new Size(400, 200),
                Location = new Point(50, 50),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label studentTitle = new Label
            {
                Text = "Добавление нового студента",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(360, 30),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            Button btnAddStudent = new Button
            {
                Text = "📝 Добавить студента",
                Location = new Point(20, 70),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddStudent.FlatAppearance.BorderSize = 0;
            btnAddStudent.Click += (sender, e) =>
            {
                try
                {
                    AddStudentForm form = new AddStudentForm(userId);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadStudents(); // Обновляем список после добавления
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка открытия формы добавления студента: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            studentPanel.Controls.AddRange(new Control[] { studentTitle, btnAddStudent });
            addStudentTab.Controls.Add(studentPanel);
            tabControl.TabPages.Add(addStudentTab);

            // === Вкладка добавления группы ===
            TabPage addGroupTab = new TabPage("➕ Добавить группу");
            addGroupTab.BackColor = Color.FromArgb(250, 250, 250);
            addGroupTab.Padding = new Padding(20);

            Panel groupPanel = new Panel
            {
                Size = new Size(400, 200),
                Location = new Point(50, 50),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label groupTitle = new Label
            {
                Text = "Добавление новой группы",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(360, 30),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            Button btnAddGroup = new Button
            {
                Text = "👥 Добавить группу",
                Location = new Point(20, 70),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddGroup.FlatAppearance.BorderSize = 0;
            btnAddGroup.Click += (sender, e) =>
            {
                try
                {
                    new AddGroupForm(userId).ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка открытия формы добавления группы: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            groupPanel.Controls.AddRange(new Control[] { groupTitle, btnAddGroup });
            addGroupTab.Controls.Add(groupPanel);
            tabControl.TabPages.Add(addGroupTab);
        }

        private void LoadStudents()
        {
            try
            {
                var studentService = new StudentService(this.connString);
                var students = studentService.GetAllStudents();
                studentGrid.DataSource = students;

                // Улучшаем отображение колонок
                if (studentGrid.Columns.Count > 0)
                {
                    studentGrid.Columns["Id"].HeaderText = "ID";
                    studentGrid.Columns["FirstName"].HeaderText = "Имя";
                    studentGrid.Columns["MiddleName"].HeaderText = "Отчество";
                    studentGrid.Columns["LastName"].HeaderText = "Фамилия";
                    studentGrid.Columns["GroupId"].HeaderText = "ID группы";

                    // Настраиваем ширину колонок
                    studentGrid.Columns["Id"].Width = 60;
                    studentGrid.Columns["FirstName"].Width = 150;
                    studentGrid.Columns["MiddleName"].Width = 150;
                    studentGrid.Columns["LastName"].Width = 150;
                    studentGrid.Columns["GroupId"].Width = 100;
                }

                DatabaseManager.Instance.LogAction(userId, "DATA_LOADED", $"Загружен список студентов: {students.Count} записей");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки студентов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DatabaseManager.Instance.LogAction(userId, "ERROR", $"Ошибка загрузки студентов: {ex.Message}");
            }
        }

        private void LoadTeacherDisciplines()
        {
            try
            {
                var disciplineService = new DisciplineService(this.connString);
                var teacherService = new TeacherService(this.connString);

                var teacherId = teacherService.GetTeacherId(userId);
                if (teacherId.HasValue)
                {
                    var disciplines = disciplineService.GetTeacherDisciplines(teacherId.Value);
                    disciplineGrid.DataSource = disciplines;

                    // Улучшаем отображение колонок
                    if (disciplineGrid.Columns.Count > 0)
                    {
                        disciplineGrid.Columns["Id"].HeaderText = "ID";
                        disciplineGrid.Columns["Name"].HeaderText = "Название дисциплины";

                        disciplineGrid.Columns["Id"].Width = 80;
                        disciplineGrid.Columns["Name"].Width = 300;
                    }

                    DatabaseManager.Instance.LogAction(userId, "DATA_LOADED", $"Загружены дисциплины преподавателя: {disciplines.Count} записей");
                }
                else
                {
                    MessageBox.Show("Преподаватель не найден в системе", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки дисциплин: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DatabaseManager.Instance.LogAction(userId, "ERROR", $"Ошибка загрузки дисциплин: {ex.Message}");
            }
        }

        private void HideTabPage(string tabName)
        {
            for (int i = tabControl.TabPages.Count - 1; i >= 0; i--)
            {
                if (tabControl.TabPages[i].Text == tabName)
                {
                    tabControl.TabPages.RemoveAt(i);
                    DatabaseManager.Instance.LogAction(userId, "TAB_HIDDEN", $"Скрыта вкладка: {tabName}");
                    break;
                }
            }
        }

        // Переопределяем закрытие формы для корректного завершения
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                DatabaseManager.Instance.LogAction(userId, "LOGOUT", $"Пользователь завершил работу с системой");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка логирования выхода: {ex.Message}");
            }

            base.OnFormClosing(e);
            Application.Exit(); // Полностью закрываем приложение
        }
    }
}