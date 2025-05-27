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
            this.Text = $"Система учета успеваемости - {role.ToUpper()}";
            this.WindowState = FormWindowState.Maximized;

            this.connString = DatabaseManager.Instance.GetConnectionString();

            // Логирование входа пользователя
            DatabaseManager.Instance.LogAction(userId, "LOGIN", $"Пользователь вошел в систему. [ID, Роль]: [{userId}, {role}]");

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
                Text = "Список студентов",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Dock = DockStyle.Left,
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleLeft
            };

            toolPanel.Controls.Add(titleLabel);
            mainLayout.Controls.Add(toolPanel, 0, 0);

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
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false
            };

            // Стилизация заголовков
            studentGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            studentGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            studentGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            studentGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            studentGrid.ColumnHeadersHeight = 35;

            mainLayout.Controls.Add(studentGrid, 0, 1);
            studentTab.Controls.Add(mainLayout);
            tabControl.TabPages.Add(studentTab);
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
                    Text = $"Ошибка загрузки аналитики: {ex.Message}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = Color.Red,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
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
                MessageBox.Show($"Ошибка создания вкладки выставления оценок: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DatabaseManager.Instance.LogAction(userId, "ERROR", $"Ошибка создания вкладки оценок: {ex.Message}");
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
                            LoadStudents(); // Обновляем список после добавления
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка открытия формы добавления студента: {ex.Message}", "Ошибка",
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
                        MessageBox.Show($"Ошибка открытия формы добавления группы: {ex.Message}", "Ошибка",
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
                        MessageBox.Show($"Ошибка открытия формы добавления дисциплины: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            );

            flowPanel.Controls.AddRange(new Control[] { studentCard, groupCard, disciplineCard });
            managementTab.Controls.Add(flowPanel);
            tabControl.TabPages.Add(managementTab);
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