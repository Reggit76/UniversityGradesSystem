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
    public partial class SpecialtyManagementForm : Form
    {
        private readonly SpecialtyService specialtyService;
        private readonly GroupService groupService;
        private readonly int adminUserId;

        // UI элементы
        private TableLayoutPanel mainLayout;
        private DataGridView dgvSpecialties;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnManageCurriculum;
        private Button btnRefresh;
        private Label lblStatus;

        public SpecialtyManagementForm(int adminUserId)
        {
            this.adminUserId = adminUserId;
            this.specialtyService = new SpecialtyService(DatabaseManager.Instance.GetConnectionString());
            this.groupService = new GroupService(DatabaseManager.Instance.GetConnectionString());

            InitializeComponent();
            InitializeEnhancedComponent();
            LoadSpecialties();
        }

        private void InitializeEnhancedComponent()
        {
            this.SuspendLayout();

            // === Настройки формы ===
            this.Text = "Управление специальностями";
            this.BackColor = Color.FromArgb(240, 244, 247);
            this.MinimumSize = new Size(1000, 600);

            // === Главный контейнер ===
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(20)
            };

            // Настраиваем строки
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F)); // Заголовок
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Кнопки
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Таблица
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); // Статус

            // === Заголовочная панель ===
            CreateHeaderPanel();

            // === Панель кнопок ===
            CreateButtonsPanel();

            // === Таблица специальностей ===
            CreateSpecialtiesGrid();

            // === Статусная строка ===
            lblStatus = new Label
            {
                Text = "Готово к работе",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(127, 140, 141),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 5, 0)
            };
            mainLayout.Controls.Add(lblStatus, 0, 3);

            // === Добавляем главный контейнер на форму ===
            this.Controls.Add(mainLayout);

            this.ResumeLayout(false);
        }

        private void CreateHeaderPanel()
        {
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(142, 68, 173),
                Padding = new Padding(20, 15, 20, 15)
            };

            Label titleLabel = new Label
            {
                Text = "🎓 Управление специальностями",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            headerPanel.Controls.Add(titleLabel);
            mainLayout.Controls.Add(headerPanel, 0, 0);
        }

        private void CreateButtonsPanel()
        {
            TableLayoutPanel buttonsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };
            buttonsPanel.BorderStyle = BorderStyle.FixedSingle;

            // Настраиваем колонки
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F)); // Добавить
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // Изменить
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // Удалить
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F)); // Учебный план
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F)); // Обновить
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Остальное пространство

            // Кнопка добавления
            btnAdd = new Button
            {
                Text = "📝 Добавить",
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

            // Кнопка редактирования
            btnEdit = new Button
            {
                Text = "✏️ Изменить",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false,
                Margin = new Padding(0, 5, 10, 5)
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += BtnEdit_Click;

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

            // Кнопка управления учебным планом
            btnManageCurriculum = new Button
            {
                Text = "📚 Учебный план",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false,
                Margin = new Padding(0, 5, 10, 5)
            };
            btnManageCurriculum.FlatAppearance.BorderSize = 0;
            btnManageCurriculum.Click += BtnManageCurriculum_Click;

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
            btnRefresh.Click += (s, e) => LoadSpecialties();

            buttonsPanel.Controls.Add(btnAdd, 0, 0);
            buttonsPanel.Controls.Add(btnEdit, 1, 0);
            buttonsPanel.Controls.Add(btnDelete, 2, 0);
            buttonsPanel.Controls.Add(btnManageCurriculum, 3, 0);
            buttonsPanel.Controls.Add(btnRefresh, 4, 0);

            mainLayout.Controls.Add(buttonsPanel, 0, 1);
        }

        private void CreateSpecialtiesGrid()
        {
            dgvSpecialties = new DataGridView
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
                Margin = new Padding(0, 10, 0, 10)
            };

            // Настройка колонок
            dgvSpecialties.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn {
                    Name = "Id",
                    HeaderText = "ID",
                    Width = 60,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn {
                    Name = "Name",
                    HeaderText = "Название специальности",
                    Width = 500,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn {
                    Name = "GroupsCount",
                    HeaderText = "Количество групп",
                    Width = 150,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn {
                    Name = "StudentsCount",
                    HeaderText = "Количество студентов",
                    Width = 150,
                    ReadOnly = true
                }
            });

            // Стилизация заголовков
            dgvSpecialties.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvSpecialties.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvSpecialties.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvSpecialties.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvSpecialties.ColumnHeadersHeight = 40;

            // Обработчик выбора строки
            dgvSpecialties.SelectionChanged += DgvSpecialties_SelectionChanged;

            // Обработчик двойного клика
            dgvSpecialties.DoubleClick += (s, e) => {
                if (btnManageCurriculum.Enabled)
                {
                    BtnManageCurriculum_Click(s, e);
                }
            };

            mainLayout.Controls.Add(dgvSpecialties, 0, 2);
        }

        private void LoadSpecialties()
        {
            try
            {
                UpdateStatus("Загрузка специальностей...");

                var specialties = specialtyService.GetAllSpecialties();
                dgvSpecialties.Rows.Clear();

                foreach (var specialty in specialties)
                {
                    // Получаем количество групп и студентов для специальности
                    var groupsCount = GetGroupsCount(specialty.Id);
                    var studentsCount = GetStudentsCount(specialty.Id);

                    int rowIndex = dgvSpecialties.Rows.Add();
                    var row = dgvSpecialties.Rows[rowIndex];

                    row.Cells["Id"].Value = specialty.Id;
                    row.Cells["Name"].Value = specialty.Name;
                    row.Cells["GroupsCount"].Value = groupsCount;
                    row.Cells["StudentsCount"].Value = studentsCount;

                    // Сохраняем объект специальности в Tag строки
                    row.Tag = specialty;
                }

                UpdateStatus($"Загружено специальностей: {specialties.Count}");
                DatabaseManager.Instance.LogAction(adminUserId, "DATA_LOADED", $"Загружен список специальностей: {specialties.Count} записей");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки специальностей: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetGroupsCount(int specialtyId)
        {
            try
            {
                var allGroups = groupService.GetAllGroups();
                return allGroups.Count(g => g.SpecialtyId == specialtyId);
            }
            catch
            {
                return 0;
            }
        }

        private int GetStudentsCount(int specialtyId)
        {
            try
            {
                var allGroups = groupService.GetAllGroups();
                var specialtyGroups = allGroups.Where(g => g.SpecialtyId == specialtyId).ToList();

                int totalStudents = 0;
                var studentService = new StudentService(DatabaseManager.Instance.GetConnectionString());

                foreach (var group in specialtyGroups)
                {
                    var students = studentService.GetStudentsByGroup(group.Id);
                    totalStudents += students.Count;
                }

                return totalStudents;
            }
            catch
            {
                return 0;
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

        private void DgvSpecialties_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dgvSpecialties.SelectedRows.Count > 0;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
            btnManageCurriculum.Enabled = hasSelection;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var addForm = new AddSpecialtyForm(adminUserId);
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    LoadSpecialties(); // Обновляем список
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы добавления: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для отображения диалога ввода текста (замена для InputBox)
        private string ShowInputDialog(string prompt, string title, string defaultValue = "")
        {
            Form inputForm = new Form()
            {
                Width = 400,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label textLabel = new Label() { Left = 20, Top = 20, Width = 340, Text = prompt };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340, Text = defaultValue };
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

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvSpecialties.SelectedRows.Count == 0) return;

            try
            {
                var selectedSpecialty = (Specialty)dgvSpecialties.SelectedRows[0].Tag;

                string newName = ShowInputDialog("Введите новое название специальности:", "Редактирование специальности", selectedSpecialty.Name);

                if (!string.IsNullOrWhiteSpace(newName) && newName.Trim() != selectedSpecialty.Name)
                {
                    newName = newName.Trim();

                    // Проверяем уникальность
                    if (specialtyService.SpecialtyExists(newName))
                    {
                        MessageBox.Show("Специальность с таким названием уже существует!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    selectedSpecialty.Name = newName;
                    if (specialtyService.UpdateSpecialty(selectedSpecialty, adminUserId))
                    {
                        MessageBox.Show("Специальность успешно обновлена!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadSpecialties();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить специальность!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка редактирования: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvSpecialties.SelectedRows.Count == 0) return;

            try
            {
                var selectedSpecialty = (Specialty)dgvSpecialties.SelectedRows[0].Tag;
                var groupsCount = GetGroupsCount(selectedSpecialty.Id);

                string message = $"Вы действительно хотите удалить специальность '{selectedSpecialty.Name}'?";
                if (groupsCount > 0)
                {
                    message += $"\n\nВНИМАНИЕ: У этой специальности есть {groupsCount} групп. При удалении специальности все связанные данные будут потеряны!";
                }

                var result = MessageBox.Show(message, "Подтверждение удаления",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (specialtyService.DeleteSpecialty(selectedSpecialty.Id, adminUserId))
                    {
                        MessageBox.Show("Специальность успешно удалена!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadSpecialties();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить специальность!", "Ошибка",
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

        private void BtnManageCurriculum_Click(object sender, EventArgs e)
        {
            if (dgvSpecialties.SelectedRows.Count == 0) return;

            try
            {
                var selectedSpecialty = (Specialty)dgvSpecialties.SelectedRows[0].Tag;
                var curriculumForm = new SpecialtyCurriculumForm(selectedSpecialty, adminUserId);
                curriculumForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия учебного плана: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }


}