// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
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
        private StatusStrip statusStrip;

        public MainForm(int userId, string role)
        {
            InitializeComponent();
            this.userId = userId;
            this.role = role;
            this.Text = string.Format("Система учета успеваемости - {0}", role.ToUpper());
            this.WindowState = FormWindowState.Maximized;

            this.connString = DatabaseManager.Instance.GetConnectionString();

            // Логирование входа пользователя
            DatabaseManager.Instance.LogAction(userId, "LOGIN", string.Format("Пользователь вошел в систему. [ID, Роль]: [{0}, {1}]", userId, role));

            // Инициализация элементов интерфейса
            InitializeUI();
            InitializeRoleUI();
        }

        private void InitializeUI()
        {
            this.SuspendLayout();

            // === Настройка формы ===
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Icon = SystemIcons.Application;
            this.MinimumSize = new Size(1000, 600);

            // === Статус бар (создаем первым, чтобы он был внизу) ===
            CreateStatusBar();

            // === Главный TabControl ===
            CreateMainTabControl();

            // === Базовые вкладки ===
            if (role == "admin")
            {
                CreateStudentsTab();
            }
            else if (role == "teacher")
            {
                CreateDisciplinesTab();
            }

            CreateEnhancedAnalyticsTab();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void CreateStatusBar()
        {
            statusStrip = new StatusStrip();
            statusStrip.BackColor = Color.FromArgb(52, 73, 94);
            statusStrip.SizingGrip = false;

            ToolStripStatusLabel statusLabel = new ToolStripStatusLabel();
            statusLabel.Text = string.Format("Пользователь: {0} | Роль: {1} | {2:HH:mm:ss}", userId, role.ToUpper(), DateTime.Now);
            statusLabel.ForeColor = Color.White;
            statusLabel.Font = new Font("Segoe UI", 9F);

            ToolStripStatusLabel connectionStatus = new ToolStripStatusLabel();
            connectionStatus.Text = "🟢 Подключено к БД";
            connectionStatus.ForeColor = Color.LightGreen;
            connectionStatus.Spring = true;
            connectionStatus.TextAlign = ContentAlignment.MiddleRight;

            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, connectionStatus });
            this.Controls.Add(statusStrip);
        }

        private void CreateMainTabControl()
        {
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 10F);
            tabControl.Appearance = TabAppearance.FlatButtons;
            tabControl.Padding = new Point(15, 8);
            tabControl.ItemSize = new Size(120, 35);

            this.Controls.Add(tabControl);
        }

        private void CreateStudentsTab()
        {
            TabPage studentTab = new TabPage("👥 Студенты");
            studentTab.BackColor = Color.FromArgb(250, 250, 250);
            studentTab.UseVisualStyleBackColor = true;

            // Главный контейнер
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F)); // Заголовок и статистика
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Фильтры
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Таблица
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); // Статус

            // === Заголовочная панель ===
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(52, 152, 219),
                Padding = new Padding(15, 10, 15, 10)
            };

            TableLayoutPanel headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            Label titleLabel = new Label
            {
                Text = "👥 Управление студентами",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Панель статистики
            TableLayoutPanel statsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            statsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            statsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            // Элементы статистики (будут обновляться)
            Label lblTotalStudents = new Label
            {
                Text = "Всего студентов: загрузка...",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Tag = "TotalStudents"
            };

            Label lblTotalGroups = new Label
            {
                Text = "Всего групп: загрузка...",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Tag = "TotalGroups"
            };

            Label lblWithGrades = new Label
            {
                Text = "С оценками: загрузка...",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Tag = "WithGrades"
            };

            Label lblSpecialties = new Label
            {
                Text = "Специальностей: загрузка...",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Tag = "Specialties"
            };

            statsPanel.Controls.Add(lblTotalStudents, 0, 0);
            statsPanel.Controls.Add(lblTotalGroups, 1, 0);
            statsPanel.Controls.Add(lblWithGrades, 0, 1);
            statsPanel.Controls.Add(lblSpecialties, 1, 1);

            headerLayout.Controls.Add(titleLabel, 0, 0);
            headerLayout.Controls.Add(statsPanel, 1, 0);
            headerPanel.Controls.Add(headerLayout);
            mainLayout.Controls.Add(headerPanel, 0, 0);

            // === Панель фильтров ===
            Panel filtersPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };
            filtersPanel.BorderStyle = BorderStyle.FixedSingle;

            TableLayoutPanel filtersLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Лейбл
            filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F)); // Комбобокс групп
            filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // Кнопка фильтра
            filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // Кнопка сброса
            filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // Кнопка обновления
            filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Остальное

            Label lblFilterGroup = new Label
            {
                Text = "Фильтр по группе:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 10, 0)
            };

            ComboBox cmbFilterGroup = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 5, 10, 5),
                Tag = "GroupFilter"
            };

            Button btnFilter = new Button
            {
                Text = "🔍 Фильтр",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 5, 5, 5)
            };
            btnFilter.FlatAppearance.BorderSize = 0;

            Button btnClearFilter = new Button
            {
                Text = "❌ Сброс",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(5, 5, 5, 5)
            };
            btnClearFilter.FlatAppearance.BorderSize = 0;

            Button btnRefreshStudents = new Button
            {
                Text = "🔄 Обновить",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(5, 5, 10, 5)
            };
            btnRefreshStudents.FlatAppearance.BorderSize = 0;

            filtersLayout.Controls.Add(lblFilterGroup, 0, 0);
            filtersLayout.Controls.Add(cmbFilterGroup, 1, 0);
            filtersLayout.Controls.Add(btnFilter, 2, 0);
            filtersLayout.Controls.Add(btnClearFilter, 3, 0);
            filtersLayout.Controls.Add(btnRefreshStudents, 4, 0);

            filtersPanel.Controls.Add(filtersLayout);
            mainLayout.Controls.Add(filtersPanel, 0, 1);

            // === Таблица студентов ===
            studentGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,  // ВАЖНО: отключаем автогенерацию
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9F),
                GridColor = Color.FromArgb(230, 230, 230),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AllowUserToResizeRows = false,
                DataSource = null  // ВАЖНО: явно устанавливаем null
            };

            // Очищаем существующие колонки
            studentGrid.Columns.Clear();

            // Настройка колонок
            studentGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn {
                    Name = "Id",
                    HeaderText = "ID",
                    Width = 60,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn {
                    Name = "DisplayName",
                    HeaderText = "ФИО студента",
                    Width = 250,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn {
                    Name = "GroupName",
                    HeaderText = "Группа",
                    Width = 120,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn {
                    Name = "SpecialtyName",
                    HeaderText = "Специальность",
                    Width = 300,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn {
                    Name = "CourseNumber",
                    HeaderText = "Курс",
                    Width = 80,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn {
                    Name = "GroupInfo",
                    HeaderText = "Полная информация о группе",
                    Width = 400,
                    ReadOnly = true
                }
            });

            // Стилизация заголовков
            studentGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            studentGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            studentGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            studentGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            studentGrid.ColumnHeadersHeight = 35;

            // Альтернативные цвета строк
            studentGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);

            mainLayout.Controls.Add(studentGrid, 0, 2);

            // === Статусная строка ===
            Label lblStudentsStatus = new Label
            {
                Text = "Загрузка данных...",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(127, 140, 141),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 5, 0),
                Tag = "StudentsStatus"
            };
            mainLayout.Controls.Add(lblStudentsStatus, 0, 3);

            // === Обработчики событий ===

            // Загрузка групп для фильтра
            EventHandler loadGroupsForFilter = (s, e) =>
            {
                try
                {
                    var groupService = new GroupService(this.connString);
                    var groups = groupService.GetAllGroups();

                    cmbFilterGroup.Items.Clear();
                    cmbFilterGroup.Items.Add(new { Id = -1, Name = "Все группы" });

                    foreach (var group in groups)
                    {
                        cmbFilterGroup.Items.Add(group);
                    }

                    cmbFilterGroup.DisplayMember = "Name";
                    cmbFilterGroup.ValueMember = "Id";
                    cmbFilterGroup.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки групп: " + ex.Message, "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Применение фильтра
            btnFilter.Click += (s, e) =>
            {
                try
                {
                    var selectedItem = cmbFilterGroup.SelectedItem;
                    if (selectedItem == null) return;

                    dynamic group = selectedItem;
                    int groupId = (int)group.Id;

                    LoadStudentsFiltered(groupId, lblStudentsStatus);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка применения фильтра: " + ex.Message, "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Сброс фильтра
            btnClearFilter.Click += (s, e) =>
            {
                cmbFilterGroup.SelectedIndex = 0;
                LoadStudentsFiltered(-1, lblStudentsStatus);
            };

            // Обновление данных
            btnRefreshStudents.Click += (s, e) =>
            {
                var selectedItem = cmbFilterGroup.SelectedItem;
                int groupId = -1;
                if (selectedItem != null)
                {
                    dynamic group = selectedItem;
                    groupId = (int)group.Id;
                }

                LoadStudentsFiltered(groupId, lblStudentsStatus);
                loadGroupsForFilter(s, e);
                LoadStudentsStatistics(statsPanel);
            };

            // Инициализация данных при создании вкладки
            studentTab.VisibleChanged += (s, e) =>
            {
                if (studentTab.Visible)
                {
                    loadGroupsForFilter(s, e);
                    LoadStudentsFiltered(-1, lblStudentsStatus);
                    LoadStudentsStatistics(statsPanel);
                }
            };

            studentTab.Controls.Add(mainLayout);
            tabControl.TabPages.Add(studentTab);
        }

        private void LoadStudentsFiltered(int groupId, Label statusLabel)
        {
            try
            {
                statusLabel.Text = "Загрузка студентов...";
                statusLabel.Refresh();

                // ВАЖНО: Отключаем привязку данных и очищаем
                studentGrid.DataSource = null;
                studentGrid.Rows.Clear();

                var studentService = new StudentService(this.connString);
                List<StudentWithGroup> students;

                if (groupId == -1)
                {
                    students = studentService.GetAllStudentsWithGroups();
                    statusLabel.Text = string.Format("Показаны все студенты: {0}", students.Count);
                }
                else
                {
                    students = studentService.GetStudentsByGroupDetailed(groupId);
                    statusLabel.Text = string.Format("Показаны студенты группы: {0}", students.Count);
                }

                // Заполняем таблицу вручную
                foreach (var student in students)
                {
                    int rowIndex = studentGrid.Rows.Add();
                    var row = studentGrid.Rows[rowIndex];

                    row.Cells["Id"].Value = student.Id;
                    row.Cells["DisplayName"].Value = student.DisplayName;
                    row.Cells["GroupName"].Value = student.GroupName;
                    row.Cells["SpecialtyName"].Value = student.SpecialtyName;
                    row.Cells["CourseNumber"].Value = student.CourseNumber;
                    row.Cells["GroupInfo"].Value = student.GroupInfo;

                    // Сохраняем объект студента в Tag строки
                    row.Tag = student;
                }

                DatabaseManager.Instance.LogAction(userId, "DATA_LOADED",
                    string.Format("Загружен список студентов: {0} записей (фильтр: группа {1})", students.Count, groupId));
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Ошибка загрузки: " + ex.Message;
                MessageBox.Show("Ошибка загрузки студентов: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DatabaseManager.Instance.LogAction(userId, "ERROR",
                    "Ошибка загрузки студентов: " + ex.Message);
            }
        }

        // Метод для загрузки статистики студентов
        private void LoadStudentsStatistics(TableLayoutPanel statsPanel)
        {
            try
            {
                var studentService = new StudentService(this.connString);
                var stats = studentService.GetStudentsStatistics();

                foreach (Control control in statsPanel.Controls)
                {
                    if (control is Label label && label.Tag != null)
                    {
                        string tag = label.Tag.ToString();
                        switch (tag)
                        {
                            case "TotalStudents":
                                label.Text = string.Format("Всего студентов: {0}", GetStatValue(stats, "TotalStudents", 0));
                                break;
                            case "TotalGroups":
                                label.Text = string.Format("Всего групп: {0}", GetStatValue(stats, "TotalGroups", 0));
                                break;
                            case "WithGrades":
                                label.Text = string.Format("С оценками: {0}", GetStatValue(stats, "StudentsWithGrades", 0));
                                break;
                            case "Specialties":
                                label.Text = string.Format("Специальностей: {0}", GetStatValue(stats, "TotalSpecialties", 0));
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка загрузки статистики: " + ex.Message);
            }
        }

        // Вспомогательный метод для получения значения из Dictionary с дефолтным значением
        private int GetStatValue(Dictionary<string, int> stats, string key, int defaultValue)
        {
            if (stats != null && stats.ContainsKey(key))
            {
                return stats[key];
            }
            return defaultValue;
        }

        private void CreateDisciplinesTab()
        {
            TabPage disciplineTab = new TabPage("📚 Мои дисциплины");
            disciplineTab.BackColor = Color.FromArgb(250, 250, 250);
            disciplineTab.UseVisualStyleBackColor = true;

            // Главный контейнер
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Панель инструментов
            Panel toolPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };
            toolPanel.BorderStyle = BorderStyle.FixedSingle;

            Label titleLabel = new Label
            {
                Text = "Преподаваемые дисциплины",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Dock = DockStyle.Left,
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleLeft
            };

            toolPanel.Controls.Add(titleLabel);
            mainLayout.Controls.Add(toolPanel, 0, 0);

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
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false
            };

            // Стилизация заголовков
            disciplineGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            disciplineGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            disciplineGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            disciplineGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            disciplineGrid.ColumnHeadersHeight = 35;

            mainLayout.Controls.Add(disciplineGrid, 0, 1);
            disciplineTab.Controls.Add(mainLayout);
            tabControl.TabPages.Add(disciplineTab);
        }

        private void CreateEnhancedAnalyticsTab()
        {
            TabPage analyticsTab = new TabPage("📊 Аналитика");
            analyticsTab.BackColor = Color.FromArgb(240, 244, 247);
            analyticsTab.UseVisualStyleBackColor = true;

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
                    Text = string.Format("Ошибка загрузки аналитики: {0}", ex.Message),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = Color.Red,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };
                analyticsTab.Controls.Add(errorLabel);

                DatabaseManager.Instance.LogAction(userId, "ERROR", string.Format("Ошибка создания вкладки аналитики: {0}", ex.Message));
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
                    AddGradeEntryTab();
                }
                else if (role == "admin")
                {
                    // УБИРАЕМ вызов LoadStudents() - теперь загрузка происходит в самой вкладке
                    AddAdminTabs();
                    AddSpecialtyTabs(); // Добавляем новые вкладки для специальностей
                    AddTeacherManagementTab(); // НОВОЕ: Добавляем вкладку для управления преподавателями
                }

                DatabaseManager.Instance.LogAction(userId, "UI_INITIALIZED", string.Format("Интерфейс инициализирован для роли: {0}", role));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка инициализации интерфейса: {0}", ex.Message), "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DatabaseManager.Instance.LogAction(userId, "ERROR", string.Format("Ошибка инициализации UI: {0}", ex.Message));
            }
        }

        private void AddGradeEntryTab()
        {
            try
            {
                TabPage gradeTab = new TabPage("✏️ Выставить оценки");
                gradeTab.BackColor = Color.FromArgb(250, 250, 250);
                gradeTab.UseVisualStyleBackColor = true;

                // Создаем улучшенную форму выставления оценок
                EnhancedGradeEntryForm gradeForm = new EnhancedGradeEntryForm(userId);

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
                MessageBox.Show(string.Format("Ошибка создания вкладки выставления оценок: {0}", ex.Message), "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DatabaseManager.Instance.LogAction(userId, "ERROR", string.Format("Ошибка создания вкладки оценок: {0}", ex.Message));
            }
        }

        private void AddAdminTabs()
        {
            // === Вкладка управления ===
            TabPage managementTab = new TabPage("⚙️ Управление");
            managementTab.BackColor = Color.FromArgb(250, 250, 250);
            managementTab.UseVisualStyleBackColor = true;

            // Используем FlowLayoutPanel для автоматического размещения карточек
            FlowLayoutPanel flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(20),
                AutoScroll = true
            };

            // Карточка добавления студента
            Panel studentCard = CreateManagementCard(
                "📝 Добавить студента",
                "Добавление нового студента в систему",
                Color.FromArgb(46, 204, 113),
                (sender, e) =>
                {
                    try
                    {
                        EnhancedAddStudentForm form = new EnhancedAddStudentForm(userId);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            // Обновляем список только если вкладка студентов видима
                            // LoadStudents() больше не нужен
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Ошибка открытия формы добавления студента: {0}", ex.Message), "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            );

            // Карточка добавления группы
            Panel groupCard = CreateManagementCard(
                "👥 Добавить группу",
                "Создание новой учебной группы",
                Color.FromArgb(52, 152, 219),
                (sender, e) =>
                {
                    try
                    {
                        EnhancedAddGroupForm form = new EnhancedAddGroupForm(userId);
                        form.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Ошибка открытия формы добавления группы: {0}", ex.Message), "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            );

            // Карточка добавления дисциплины
            Panel disciplineCard = CreateManagementCard(
                "📚 Добавить дисциплину",
                "Создание новой дисциплины",
                Color.FromArgb(155, 89, 182),
                (sender, e) =>
                {
                    try
                    {
                        AddDisciplineForm form = new AddDisciplineForm(userId);
                        form.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Ошибка открытия формы добавления дисциплины: {0}", ex.Message), "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            );

            flowPanel.Controls.AddRange(new Control[] { studentCard, groupCard, disciplineCard });
            managementTab.Controls.Add(flowPanel);
            tabControl.TabPages.Add(managementTab);
        }

        // Новый метод для добавления вкладок управления специальностями
        private void AddSpecialtyTabs()
        {
            try
            {
                // === Вкладка управления специальностями ===
                TabPage specialtyTab = new TabPage("🎓 Специальности");
                specialtyTab.BackColor = Color.FromArgb(250, 250, 250);
                specialtyTab.UseVisualStyleBackColor = true;

                // Встраиваем форму управления специальностями
                SpecialtyManagementForm specialtyForm = new SpecialtyManagementForm(userId);

                specialtyForm.TopLevel = false;
                specialtyForm.FormBorderStyle = FormBorderStyle.None;
                specialtyForm.Dock = DockStyle.Fill;
                specialtyForm.Visible = true;

                specialtyTab.Controls.Add(specialtyForm);
                specialtyForm.Show();

                tabControl.TabPages.Add(specialtyTab);

                DatabaseManager.Instance.LogAction(userId, "TAB_CREATED", "Создана вкладка управления специальностями");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка создания вкладки специальностей: {0}", ex.Message), "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DatabaseManager.Instance.LogAction(userId, "ERROR", string.Format("Ошибка создания вкладки специальностей: {0}", ex.Message));

                // Создаем заглушку в случае ошибки
                CreateSpecialtyTabStub();
            }
        }

        // НОВЫЙ МЕТОД: Добавление вкладки управления преподавателями
        private void AddTeacherManagementTab()
        {
            try
            {
                // === Вкладка управления преподавателями ===
                TabPage teachersTab = new TabPage("👨‍🏫 Преподаватели");
                teachersTab.BackColor = Color.FromArgb(250, 250, 250);
                teachersTab.UseVisualStyleBackColor = true;

                // Встраиваем форму управления преподавателями
                TeacherManagementForm teachersForm = new TeacherManagementForm(userId);

                teachersForm.TopLevel = false;
                teachersForm.FormBorderStyle = FormBorderStyle.None;
                teachersForm.Dock = DockStyle.Fill;
                teachersForm.Visible = true;

                teachersTab.Controls.Add(teachersForm);
                teachersForm.Show();

                tabControl.TabPages.Add(teachersTab);

                DatabaseManager.Instance.LogAction(userId, "TAB_CREATED", "Создана вкладка управления преподавателями");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка создания вкладки преподавателей: {0}", ex.Message), "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DatabaseManager.Instance.LogAction(userId, "ERROR", string.Format("Ошибка создания вкладки преподавателей: {0}", ex.Message));

                // Создаем заглушку в случае ошибки
                CreateTeacherTabStub();
            }
        }

        // НОВЫЙ МЕТОД: Создание заглушки для вкладки преподавателей
        private void CreateTeacherTabStub()
        {
            TabPage teachersTab = new TabPage("👨‍🏫 Преподаватели");
            teachersTab.BackColor = Color.FromArgb(250, 250, 250);
            teachersTab.UseVisualStyleBackColor = true;

            // Простая карточка для управления преподавателями
            FlowLayoutPanel flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(20),
                AutoScroll = true
            };

            Panel teacherManagementCard = CreateManagementCard(
                "👨‍🏫 Управление преподавателями",
                "Добавление, редактирование и управление дисциплинами преподавателей",
                Color.FromArgb(52, 152, 219),
                (sender, e) =>
                {
                    try
                    {
                        TeacherManagementForm form = new TeacherManagementForm(userId);
                        form.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Ошибка открытия управления преподавателями: {0}", ex.Message), "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            );

            flowPanel.Controls.Add(teacherManagementCard);
            teachersTab.Controls.Add(flowPanel);
            tabControl.TabPages.Add(teachersTab);
        }

        private void CreateSpecialtyTabStub()
        {
            TabPage specialtyTab = new TabPage("🎓 Специальности");
            specialtyTab.BackColor = Color.FromArgb(250, 250, 250);
            specialtyTab.UseVisualStyleBackColor = true;

            // Простая карточка для добавления специальности
            FlowLayoutPanel flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(20),
                AutoScroll = true
            };

            Panel addSpecialtyCard = CreateManagementCard(
                "🎓 Добавить специальность",
                "Создание новой специальности обучения",
                Color.FromArgb(142, 68, 173),
                (sender, e) =>
                {
                    try
                    {
                        AddSpecialtyForm form = new AddSpecialtyForm(userId);
                        form.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Ошибка открытия формы добавления специальности: {0}", ex.Message), "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            );

            flowPanel.Controls.Add(addSpecialtyCard);
            specialtyTab.Controls.Add(flowPanel);
            tabControl.TabPages.Add(specialtyTab);
        }

        private Panel CreateManagementCard(string title, string description, Color color, EventHandler clickHandler)
        {
            Panel card = new Panel
            {
                Size = new Size(300, 150),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                Cursor = Cursors.Hand
            };

            Panel colorBar = new Panel
            {
                Size = new Size(5, 150),
                BackColor = color,
                Dock = DockStyle.Left
            };

            Label titleLabel = new Label
            {
                Text = title,
                Location = new Point(20, 20),
                Size = new Size(270, 30),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = color
            };

            Label descLabel = new Label
            {
                Text = description,
                Location = new Point(20, 55),
                Size = new Size(270, 40),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(127, 140, 141)
            };

            Button actionButton = new Button
            {
                Text = "Открыть",
                Location = new Point(20, 105),
                Size = new Size(100, 30),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            actionButton.FlatAppearance.BorderSize = 0;
            actionButton.Click += clickHandler;

            card.Controls.AddRange(new Control[] { colorBar, titleLabel, descLabel, actionButton });
            return card;
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

                    DatabaseManager.Instance.LogAction(userId, "DATA_LOADED", string.Format("Загружены дисциплины преподавателя: {0} записей", disciplines.Count));
                }
                else
                {
                    MessageBox.Show("Преподаватель не найден в системе", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка загрузки дисциплин: {0}", ex.Message), "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DatabaseManager.Instance.LogAction(userId, "ERROR", string.Format("Ошибка загрузки дисциплин: {0}", ex.Message));
            }
        }

        // Переопределяем закрытие формы для корректного завершения
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                DatabaseManager.Instance.LogAction(userId, "LOGOUT", "Пользователь завершил работу с системой");
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Ошибка логирования выхода: {0}", ex.Message));
            }

            base.OnFormClosing(e);
            Application.Exit(); // Полностью закрываем приложение
        }
    }
}