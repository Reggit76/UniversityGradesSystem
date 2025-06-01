// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UniversityGradesSystem.Models;
using UniversityGradesSystem.Services;

namespace UniversityGradesSystem.Forms
{
    public partial class TeacherManagementForm : Form
    {
        private readonly EnhancedTeacherService teacherService;
        private readonly DisciplineService disciplineService;
        private readonly int adminUserId;

        // UI элементы
        private TableLayoutPanel mainLayout;
        private DataGridView dgvTeachers;
        private DataGridView dgvTeacherDisciplines;
        private Button btnAdd;
        private Button btnChangePassword;
        private Button btnDelete;
        private Button btnManageDisciplines;
        private Button btnRefresh;
        private Label lblStatus;
        private Label lblDisciplinesStatus;

        public TeacherManagementForm(int adminUserId)
        {
            this.adminUserId = adminUserId;
            this.teacherService = new EnhancedTeacherService(DatabaseManager.Instance.GetConnectionString());
            this.disciplineService = new DisciplineService(DatabaseManager.Instance.GetConnectionString());

            InitializeComponent();
            InitializeEnhancedComponent();
            LoadTeachers();
        }

        private void InitializeEnhancedComponent()
        {
            this.SuspendLayout();

            // === Настройки формы ===
            this.Text = "Управление преподавателями";
            this.BackColor = Color.FromArgb(240, 244, 247);
            this.MinimumSize = new Size(1200, 700);

            // === Главный контейнер ===
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(20)
            };

            // Настраиваем колонки (левая - преподаватели, правая - дисциплины)
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            // Настраиваем строки
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F)); // Заголовки
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Кнопки
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Таблицы
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); // Статус

            // === Заголовочные панели ===
            CreateHeaderPanels();

            // === Панели кнопок ===
            CreateButtonsPanels();

            // === Таблицы данных ===
            CreateDataGrids();

            // === Статусные строки ===
            CreateStatusLabels();

            // === Добавляем главный контейнер на форму ===
            this.Controls.Add(mainLayout);

            this.ResumeLayout(false);
        }

        private void CreateHeaderPanels()
        {
            // Заголовок для преподавателей
            Panel teachersHeaderPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(52, 152, 219),
                Padding = new Padding(20, 15, 20, 15),
                Margin = new Padding(0, 0, 5, 0)
            };

            Label teachersTitle = new Label
            {
                Text = "👨‍🏫 Преподаватели",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            teachersHeaderPanel.Controls.Add(teachersTitle);
            mainLayout.Controls.Add(teachersHeaderPanel, 0, 0);

            // Заголовок для дисциплин
            Panel disciplinesHeaderPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(155, 89, 182),
                Padding = new Padding(20, 15, 20, 15),
                Margin = new Padding(5, 0, 0, 0)
            };

            Label disciplinesTitle = new Label
            {
                Text = "📚 Дисциплины преподавателя",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            disciplinesHeaderPanel.Controls.Add(disciplinesTitle);
            mainLayout.Controls.Add(disciplinesHeaderPanel, 1, 0);
        }

        private void CreateButtonsPanels()
        {
            // Панель кнопок для преподавателей
            TableLayoutPanel teachersButtonsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10),
                Margin = new Padding(0, 0, 5, 0)
            };
            teachersButtonsPanel.BorderStyle = BorderStyle.FixedSingle;

            // Настраиваем колонки
            teachersButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F)); // Добавить
            teachersButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F)); // Сменить пароль
            teachersButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // Удалить
            teachersButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F)); // Обновить
            teachersButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Остальное

            // Кнопка добавления
            btnAdd = new Button
            {
                Text = "➕ Добавить",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 5, 10, 5)
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            // Кнопка смены пароля
            btnChangePassword = new Button
            {
                Text = "🔑 Сменить пароль",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false,
                Margin = new Padding(0, 5, 10, 5)
            };
            btnChangePassword.FlatAppearance.BorderSize = 0;
            btnChangePassword.Click += BtnChangePassword_Click;

            // Кнопка удаления
            btnDelete = new Button
            {
                Text = "🗑️ Удалить",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false,
                Margin = new Padding(0, 5, 10, 5)
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;

            // Кнопка обновления
            btnRefresh = new Button
            {
                Text = "🔄 Обновить",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 5, 10, 5)
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadTeachers();

            teachersButtonsPanel.Controls.Add(btnAdd, 0, 0);
            teachersButtonsPanel.Controls.Add(btnChangePassword, 1, 0);
            teachersButtonsPanel.Controls.Add(btnDelete, 2, 0);
            teachersButtonsPanel.Controls.Add(btnRefresh, 3, 0);

            mainLayout.Controls.Add(teachersButtonsPanel, 0, 1);

            // Панель кнопок для дисциплин
            TableLayoutPanel disciplinesButtonsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10),
                Margin = new Padding(5, 0, 0, 0)
            };
            disciplinesButtonsPanel.BorderStyle = BorderStyle.FixedSingle;

            disciplinesButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
            disciplinesButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Кнопка управления дисциплинами
            btnManageDisciplines = new Button
            {
                Text = "⚙️ Управлять дисциплинами",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false,
                Margin = new Padding(0, 5, 10, 5)
            };
            btnManageDisciplines.FlatAppearance.BorderSize = 0;
            btnManageDisciplines.Click += BtnManageDisciplines_Click;

            disciplinesButtonsPanel.Controls.Add(btnManageDisciplines, 0, 0);

            mainLayout.Controls.Add(disciplinesButtonsPanel, 1, 1);
        }

        private void CreateDataGrids()
        {
            // Таблица преподавателей
            dgvTeachers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9F),
                GridColor = Color.FromArgb(224, 224, 224),
                Margin = new Padding(0, 10, 5, 10)
            };

            // Настройка колонок преподавателей
            dgvTeachers.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn {
                    Name = "Id",
                    HeaderText = "ID",
                    Width = 50,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn {
                    Name = "FullName",
                    HeaderText = "ФИО",
                    Width = 250,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn {
                    Name = "Username",
                    HeaderText = "Логин",
                    Width = 120,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn {
                    Name = "DisciplinesCount",
                    HeaderText = "Дисциплин",
                    Width = 80,
                    ReadOnly = true
                }
            });

            // Стилизация заголовков преподавателей
            dgvTeachers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvTeachers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvTeachers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvTeachers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvTeachers.ColumnHeadersHeight = 40;

            // Обработчик выбора преподавателя
            dgvTeachers.SelectionChanged += DgvTeachers_SelectionChanged;

            mainLayout.Controls.Add(dgvTeachers, 0, 2);

            // Таблица дисциплин преподавателя
            dgvTeacherDisciplines = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9F),
                GridColor = Color.FromArgb(224, 224, 224),
                Margin = new Padding(5, 10, 0, 10)
            };

            // Настройка колонок дисциплин
            dgvTeacherDisciplines.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn {
                    Name = "Id",
                    HeaderText = "ID",
                    Width = 50,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn {
                    Name = "Name",
                    HeaderText = "Название дисциплины",
                    Width = 300,
                    ReadOnly = true
                }
            });

            // Стилизация заголовков дисциплин
            dgvTeacherDisciplines.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvTeacherDisciplines.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvTeacherDisciplines.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvTeacherDisciplines.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvTeacherDisciplines.ColumnHeadersHeight = 40;

            mainLayout.Controls.Add(dgvTeacherDisciplines, 1, 2);
        }

        private void CreateStatusLabels()
        {
            // Статус для преподавателей
            lblStatus = new Label
            {
                Text = "Готово к работе",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(127, 140, 141),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 5, 0),
                Margin = new Padding(0, 0, 5, 0)
            };
            mainLayout.Controls.Add(lblStatus, 0, 3);

            // Статус для дисциплин
            lblDisciplinesStatus = new Label
            {
                Text = "Выберите преподавателя",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(127, 140, 141),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 5, 0),
                Margin = new Padding(5, 0, 0, 0)
            };
            mainLayout.Controls.Add(lblDisciplinesStatus, 1, 3);
        }

        private void LoadTeachers()
        {
            try
            {
                UpdateStatus("Загрузка списка преподавателей...");

                var teachers = teacherService.GetAllTeachersWithDetails();
                dgvTeachers.Rows.Clear();

                foreach (var teacher in teachers)
                {
                    int rowIndex = dgvTeachers.Rows.Add();
                    var row = dgvTeachers.Rows[rowIndex];

                    row.Cells["Id"].Value = teacher.Id;
                    row.Cells["FullName"].Value = teacher.FullName;
                    row.Cells["Username"].Value = teacher.Username;
                    row.Cells["DisciplinesCount"].Value = teacher.DisciplinesCount;

                    // Сохраняем объект преподавателя в Tag строки
                    row.Tag = teacher;
                }

                UpdateStatus($"Загружено преподавателей: {teachers.Count}");
                DatabaseManager.Instance.LogAction(adminUserId, "DATA_LOADED", $"Загружен список преподавателей: {teachers.Count} записей");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки преподавателей: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTeacherDisciplines(int teacherId)
        {
            try
            {
                UpdateDisciplinesStatus("Загрузка дисциплин...");

                var disciplines = disciplineService.GetTeacherDisciplines(teacherId);
                dgvTeacherDisciplines.Rows.Clear();

                foreach (var discipline in disciplines)
                {
                    int rowIndex = dgvTeacherDisciplines.Rows.Add();
                    var row = dgvTeacherDisciplines.Rows[rowIndex];

                    row.Cells["Id"].Value = discipline.Id;
                    row.Cells["Name"].Value = discipline.Name;

                    // Сохраняем объект дисциплины в Tag строки
                    row.Tag = discipline;
                }

                UpdateDisciplinesStatus($"Дисциплин: {disciplines.Count}");
            }
            catch (Exception ex)
            {
                UpdateDisciplinesStatus($"Ошибка: {ex.Message}");
                Console.WriteLine($"Ошибка загрузки дисциплин преподавателя: {ex.Message}");
            }
        }

        private void UpdateStatus(string message)
        {
            if (lblStatus != null)
            {
                lblStatus.Text = message;
                lblStatus.Refresh();
            }
        }

        private void UpdateDisciplinesStatus(string message)
        {
            if (lblDisciplinesStatus != null)
            {
                lblDisciplinesStatus.Text = message;
                lblDisciplinesStatus.Refresh();
            }
        }

        private void DgvTeachers_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dgvTeachers.SelectedRows.Count > 0;
            btnChangePassword.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
            btnManageDisciplines.Enabled = hasSelection;

            if (hasSelection && dgvTeachers.SelectedRows[0].Tag is TeacherWithDetails teacher)
            {
                LoadTeacherDisciplines(teacher.Id);
            }
            else
            {
                dgvTeacherDisciplines.Rows.Clear();
                UpdateDisciplinesStatus("Выберите преподавателя");
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var addForm = new AddTeacherForm(adminUserId);
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    LoadTeachers(); // Обновляем список
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы добавления: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnChangePassword_Click(object sender, EventArgs e)
        {
            if (dgvTeachers.SelectedRows.Count == 0) return;

            try
            {
                var selectedTeacher = (TeacherWithDetails)dgvTeachers.SelectedRows[0].Tag;

                string newPassword = ShowPasswordInputDialog($"Введите новый пароль для преподавателя {selectedTeacher.FullName}:");

                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    if (teacherService.ChangeTeacherPassword(selectedTeacher.UserId, newPassword.Trim(), adminUserId))
                    {
                        MessageBox.Show("Пароль успешно изменен!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось изменить пароль!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка смены пароля: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvTeachers.SelectedRows.Count == 0) return;

            try
            {
                var selectedTeacher = (TeacherWithDetails)dgvTeachers.SelectedRows[0].Tag;

                string message = $"Вы действительно хотите удалить преподавателя '{selectedTeacher.FullName}'?";
                if (selectedTeacher.DisciplinesCount > 0)
                {
                    message += $"\n\nВНИМАНИЕ: У этого преподавателя назначено {selectedTeacher.DisciplinesCount} дисциплин. При удалении все связи будут потеряны!";
                }

                var result = MessageBox.Show(message, "Подтверждение удаления",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (teacherService.DeleteTeacher(selectedTeacher.Id, adminUserId))
                    {
                        MessageBox.Show("Преподаватель успешно удален!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadTeachers();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить преподавателя!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnManageDisciplines_Click(object sender, EventArgs e)
        {
            if (dgvTeachers.SelectedRows.Count == 0) return;

            try
            {
                var selectedTeacher = (TeacherWithDetails)dgvTeachers.SelectedRows[0].Tag;
                var disciplinesForm = new TeacherDisciplinesForm(selectedTeacher, adminUserId);
                if (disciplinesForm.ShowDialog() == DialogResult.OK)
                {
                    LoadTeachers(); // Обновляем список преподавателей
                    LoadTeacherDisciplines(selectedTeacher.Id); // Обновляем дисциплины
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия управления дисциплинами: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для отображения диалога ввода пароля
        private string ShowPasswordInputDialog(string prompt)
        {
            Form inputForm = new Form()
            {
                Width = 400,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Смена пароля",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label textLabel = new Label() { Left = 20, Top = 20, Width = 340, Text = prompt };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340, PasswordChar = '●' };
            Button confirmation = new Button() { Text = "OK", Left = 235, Width = 100, Top = 80, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "Отмена", Left = 130, Width = 100, Top = 80, DialogResult = DialogResult.Cancel };

            confirmation.Click += (sender, e) => { inputForm.Close(); };
            cancel.Click += (sender, e) => { inputForm.Close(); };

            inputForm.Controls.Add(textLabel);
            inputForm.Controls.Add(textBox);
            inputForm.Controls.Add(confirmation);
            inputForm.Controls.Add(cancel);
            inputForm.AcceptButton = confirmation;
            inputForm.CancelButton = cancel;

            return inputForm.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}