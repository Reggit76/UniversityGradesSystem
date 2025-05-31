using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UniversityGradesSystem.Models;
using UniversityGradesSystem.Services;

namespace UniversityGradesSystem.Forms
{
    public partial class SpecialtyCurriculumForm : Form
    {
        private readonly Specialty specialty;
        private readonly SpecialtyService specialtyService;
        private readonly int adminUserId;

        // UI элементы
        private TableLayoutPanel mainLayout;
        private DataGridView dgvCurriculum;
        private ComboBox cmbDiscipline;
        private ComboBox cmbSemester;
        private Button btnAdd;
        private Button btnRemove;
        private Button btnClose;
        private Label lblStatus;

        public SpecialtyCurriculumForm(Specialty specialty, int adminUserId)
        {
            this.specialty = specialty;
            this.adminUserId = adminUserId;
            this.specialtyService = new SpecialtyService(DatabaseManager.Instance.GetConnectionString());

            InitializeComponent();
            InitializeEnhancedComponent();
            LoadData();
            LoadCurriculum();
        }

        private void InitializeEnhancedComponent()
        {
            this.SuspendLayout();

            // === Настройки формы ===
            this.Text = $"Учебный план: {specialty.Name}";
            this.BackColor = Color.FromArgb(240, 244, 247);
            this.MinimumSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            // === Главный контейнер ===
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(20)
            };

            // Настраиваем строки
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F)); // Заголовок
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F)); // Форма добавления
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Таблица
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // Кнопки
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); // Статус

            // === Заголовочная панель ===
            CreateHeaderPanel();

            // === Панель добавления дисциплин ===
            CreateAddPanel();

            // === Таблица учебного плана ===
            CreateCurriculumGrid();

            // === Панель кнопок ===
            CreateButtonsPanel();

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
            mainLayout.Controls.Add(lblStatus, 0, 4);

            // === Добавляем главный контейнер на форму ===
            this.Controls.Add(mainLayout);

            this.ResumeLayout(false);
        }

        private void CreateHeaderPanel()
        {
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(155, 89, 182),
                Padding = new Padding(20, 15, 20, 15)
            };

            Label titleLabel = new Label
            {
                Text = $"📚 Учебный план специальности",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label subtitleLabel = new Label
            {
                Text = specialty.Name,
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.White,
                Dock = DockStyle.Bottom,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(subtitleLabel);
            mainLayout.Controls.Add(headerPanel, 0, 0);
        }

        private void CreateAddPanel()
        {
            TableLayoutPanel addPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };
            addPanel.BorderStyle = BorderStyle.FixedSingle;

            // Настраиваем колонки
            addPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); // Дисциплина
            addPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F)); // Семестр
            addPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // Кнопка добавить
            addPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // Кнопка удалить

            // Настраиваем строки
            addPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F)); // Лейблы
            addPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60F)); // Элементы

            // === Лейблы ===
            var lblDiscipline = new Label
            {
                Text = "Дисциплина:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft
            };

            var lblSemester = new Label
            {
                Text = "Семестр:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft
            };

            addPanel.Controls.Add(lblDiscipline, 0, 0);
            addPanel.Controls.Add(lblSemester, 1, 0);

            // === Комбобоксы ===
            cmbDiscipline = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 5, 10, 5)
            };

            cmbSemester = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 5, 10, 5)
            };

            addPanel.Controls.Add(cmbDiscipline, 0, 1);
            addPanel.Controls.Add(cmbSemester, 1, 1);

            // === Кнопки ===
            btnAdd = new Button
            {
                Text = "➕ Добавить",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 5, 5, 5)
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            btnRemove = new Button
            {
                Text = "➖ Удалить",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false,
                Margin = new Padding(5, 5, 0, 5)
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += BtnRemove_Click;

            addPanel.Controls.Add(btnAdd, 2, 1);
            addPanel.Controls.Add(btnRemove, 3, 1);

            mainLayout.Controls.Add(addPanel, 0, 1);
        }

        private void CreateCurriculumGrid()
        {
            dgvCurriculum = new DataGridView
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
            dgvCurriculum.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn {
                    Name = "Id",
                    HeaderText = "ID",
                    Width = 60,
                    Visible = false
                },
                new DataGridViewTextBoxColumn {
                    Name = "CourseNumber",
                    HeaderText = "Курс",
                    Width = 80
                },
                new DataGridViewTextBoxColumn {
                    Name = "SemesterNumber",
                    HeaderText = "Семестр",
                    Width = 100
                },
                new DataGridViewTextBoxColumn {
                    Name = "DisciplineName",
                    HeaderText = "Дисциплина",
                    Width = 400
                },
                new DataGridViewTextBoxColumn {
                    Name = "SemesterDisplay",
                    HeaderText = "Период обучения",
                    Width = 200
                }
            });

            // Стилизация заголовков
            dgvCurriculum.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvCurriculum.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCurriculum.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvCurriculum.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCurriculum.ColumnHeadersHeight = 40;

            // Обработчик выбора строки
            dgvCurriculum.SelectionChanged += DgvCurriculum_SelectionChanged;

            mainLayout.Controls.Add(dgvCurriculum, 0, 2);
        }

        private void CreateButtonsPanel()
        {
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };
            buttonPanel.BorderStyle = BorderStyle.FixedSingle;

            btnClose = new Button
            {
                Text = "✅ Закрыть",
                Size = new Size(120, 30),
                Location = new Point(buttonPanel.Width - 135, 10),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            var lblInfo = new Label
            {
                Text = "💡 Выберите дисциплину и семестр, затем нажмите \"Добавить\" для создания записи в учебном плане",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(127, 140, 141),
                Location = new Point(0, 15),
                Size = new Size(buttonPanel.Width - 150, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            buttonPanel.Controls.Add(lblInfo);
            buttonPanel.Controls.Add(btnClose);

            mainLayout.Controls.Add(buttonPanel, 0, 3);
        }

        private void LoadData()
        {
            try
            {
                // Загружаем дисциплины
                var disciplines = specialtyService.GetAllDisciplines();
                if (disciplines.Any())
                {
                    cmbDiscipline.DisplayMember = "Name";
                    cmbDiscipline.ValueMember = "Id";
                    cmbDiscipline.DataSource = disciplines;
                    cmbDiscipline.SelectedIndex = -1;
                }

                // Загружаем семестры
                var semesters = specialtyService.GetAllSemesters();
                if (semesters.Any())
                {
                    cmbSemester.DisplayMember = "DisplayText";
                    cmbSemester.ValueMember = "Id";
                    cmbSemester.DataSource = semesters;
                    cmbSemester.SelectedIndex = -1;
                }

                UpdateStatus("Данные загружены");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки данных: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCurriculum()
        {
            try
            {
                UpdateStatus("Загрузка учебного плана...");

                var curriculum = specialtyService.GetSpecialtyCurriculum(specialty.Id);
                dgvCurriculum.Rows.Clear();

                foreach (var item in curriculum.OrderBy(c => c.CourseNumber).ThenBy(c => c.SemesterNumber).ThenBy(c => c.DisciplineName))
                {
                    int rowIndex = dgvCurriculum.Rows.Add();
                    var row = dgvCurriculum.Rows[rowIndex];

                    row.Cells["Id"].Value = item.Id;
                    row.Cells["CourseNumber"].Value = item.CourseNumber;
                    row.Cells["SemesterNumber"].Value = item.SemesterNumber;
                    row.Cells["DisciplineName"].Value = item.DisciplineName;
                    row.Cells["SemesterDisplay"].Value = item.SemesterDisplay;

                    // Сохраняем объект в Tag строки
                    row.Tag = item;
                }

                UpdateStatus($"Загружено записей в учебном плане: {curriculum.Count}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки учебного плана: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки учебного плана: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void DgvCurriculum_SelectionChanged(object sender, EventArgs e)
        {
            btnRemove.Enabled = dgvCurriculum.SelectedRows.Count > 0;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbDiscipline.SelectedItem == null)
            {
                MessageBox.Show("Выберите дисциплину!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbDiscipline.Focus();
                return;
            }

            if (cmbSemester.SelectedItem == null)
            {
                MessageBox.Show("Выберите семестр!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbSemester.Focus();
                return;
            }

            try
            {
                var selectedDiscipline = (Discipline)cmbDiscipline.SelectedItem;
                var selectedSemester = (SemesterDisplay)cmbSemester.SelectedItem;

                bool success = specialtyService.AddDisciplineToSpecialty(
                    specialty.Id,
                    selectedDiscipline.Id,
                    selectedSemester.Id,
                    adminUserId);

                if (success)
                {
                    MessageBox.Show("Дисциплина успешно добавлена в учебный план!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadCurriculum(); // Обновляем отображение

                    // Сбрасываем выбор
                    cmbDiscipline.SelectedIndex = -1;
                    cmbSemester.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show("Не удалось добавить дисциплину в учебный план.\nВозможно, такая запись уже существует.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления дисциплины: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (dgvCurriculum.SelectedRows.Count == 0) return;

            try
            {
                var selectedItem = (SpecialtyCurriculum)dgvCurriculum.SelectedRows[0].Tag;

                var result = MessageBox.Show(
                    $"Вы действительно хотите удалить дисциплину '{selectedItem.DisciplineName}' из {selectedItem.SemesterDisplay}?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (specialtyService.RemoveDisciplineFromSpecialty(selectedItem.Id, adminUserId))
                    {
                        MessageBox.Show("Дисциплина успешно удалена из учебного плана!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadCurriculum(); // Обновляем отображение
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить дисциплину из учебного плана!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления дисциплины: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработка нажатий клавиш
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter && btnAdd.Enabled)
            {
                BtnAdd_Click(btnAdd, EventArgs.Empty);
                return true;
            }
            else if (keyData == Keys.Delete && btnRemove.Enabled)
            {
                BtnRemove_Click(btnRemove, EventArgs.Empty);
                return true;
            }
            else if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }


}