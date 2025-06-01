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
    public partial class AddTeacherForm : Form
    {
        private readonly EnhancedTeacherService teacherService;
        private readonly DisciplineService disciplineService;
        private readonly int adminUserId;

        // UI элементы
        private TableLayoutPanel mainLayout;
        private TextBox txtFirstName;
        private TextBox txtMiddleName;
        private TextBox txtLastName;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private CheckedListBox clbDisciplines;
        private Button btnSave;
        private Button btnCancel;
        private Button btnSelectAll;
        private Button btnClearAll;

        public AddTeacherForm(int adminUserId)
        {
            this.adminUserId = adminUserId;
            this.teacherService = new EnhancedTeacherService(DatabaseManager.Instance.GetConnectionString());
            this.disciplineService = new DisciplineService(DatabaseManager.Instance.GetConnectionString());

            InitializeComponent();
            InitializeEnhancedComponent();
            LoadDisciplines();
        }

        private void InitializeEnhancedComponent()
        {
            this.SuspendLayout();

            // === Настройки формы ===
            this.Text = "Добавить преподавателя";
            this.BackColor = Color.FromArgb(240, 244, 247);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(600, 700);

            // === Главный контейнер ===
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(15)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F)); // Заголовок
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Форма
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Кнопки

            // === Заголовочная панель ===
            CreateHeaderPanel();

            // === Панель формы ===
            CreateFormPanel();

            // === Панель кнопок ===
            CreateButtonsPanel();

            // === Добавляем главный контейнер на форму ===
            this.Controls.Add(mainLayout);

            // Устанавливаем фокус на первое поле
            this.ActiveControl = txtFirstName;

            this.ResumeLayout(false);
        }

        private void CreateHeaderPanel()
        {
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(52, 152, 219),
                Padding = new Padding(20, 15, 20, 15)
            };

            Label titleLabel = new Label
            {
                Text = "👨‍🏫 Добавление нового преподавателя",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            headerPanel.Controls.Add(titleLabel);
            mainLayout.Controls.Add(headerPanel, 0, 0);
        }

        private void CreateFormPanel()
        {
            Panel formPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(25, 20, 25, 20)
            };
            formPanel.BorderStyle = BorderStyle.FixedSingle;

            // Используем TableLayoutPanel для лучшего контроля макета
            TableLayoutPanel formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                BackColor = Color.Transparent
            };

            // Настраиваем колонки (50% каждая)
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // Настраиваем строки
            for (int i = 0; i < 7; i++)
            {
                formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            }
            formLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Для списка дисциплин

            int currentRow = 0;

            // === Поля ФИО ===
            // Фамилия
            var lblLastName = CreateLabel("Фамилия:");
            txtLastName = CreateTextBox();
            formLayout.Controls.Add(lblLastName, 0, currentRow);
            formLayout.Controls.Add(txtLastName, 1, currentRow++);

            // Имя
            var lblFirstName = CreateLabel("Имя:");
            txtFirstName = CreateTextBox();
            formLayout.Controls.Add(lblFirstName, 0, currentRow);
            formLayout.Controls.Add(txtFirstName, 1, currentRow++);

            // Отчество
            var lblMiddleName = CreateLabel("Отчество:");
            txtMiddleName = CreateTextBox();
            formLayout.Controls.Add(lblMiddleName, 0, currentRow);
            formLayout.Controls.Add(txtMiddleName, 1, currentRow++);

            // === Данные для входа ===
            // Логин
            var lblUsername = CreateLabel("Логин:");
            txtUsername = CreateTextBox();
            txtUsername.TextChanged += TxtUsername_TextChanged;
            formLayout.Controls.Add(lblUsername, 0, currentRow);
            formLayout.Controls.Add(txtUsername, 1, currentRow++);

            // Пароль
            var lblPassword = CreateLabel("Пароль:");
            txtPassword = CreateTextBox();
            txtPassword.PasswordChar = '●';
            formLayout.Controls.Add(lblPassword, 0, currentRow);
            formLayout.Controls.Add(txtPassword, 1, currentRow++);

            // === Дисциплины ===
            var lblDisciplines = CreateLabel("Дисциплины:");
            formLayout.Controls.Add(lblDisciplines, 0, currentRow);

            // Кнопки управления выбором дисциплин
            TableLayoutPanel disciplineButtonsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 10, 0, 5)
            };
            disciplineButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            disciplineButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            btnSelectAll = new Button
            {
                Text = "✓ Выбрать все",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8F),
                Margin = new Padding(0, 0, 5, 0)
            };
            btnSelectAll.FlatAppearance.BorderSize = 0;
            btnSelectAll.Click += BtnSelectAll_Click;

            btnClearAll = new Button
            {
                Text = "✗ Очистить все",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8F),
                Margin = new Padding(5, 0, 0, 0)
            };
            btnClearAll.FlatAppearance.BorderSize = 0;
            btnClearAll.Click += BtnClearAll_Click;

            disciplineButtonsPanel.Controls.Add(btnSelectAll, 0, 0);
            disciplineButtonsPanel.Controls.Add(btnClearAll, 1, 0);

            formLayout.Controls.Add(disciplineButtonsPanel, 1, currentRow++);

            // Список дисциплин
            var disciplinesContainer = new Panel
            {
                Dock = DockStyle.Fill
            };

            clbDisciplines = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9F),
                ScrollAlwaysVisible = true
            };

            disciplinesContainer.Controls.Add(clbDisciplines);
            formLayout.Controls.Add(disciplinesContainer, 0, currentRow);
            formLayout.SetColumnSpan(disciplinesContainer, 2); // Занимает обе колонки

            formPanel.Controls.Add(formLayout);
            mainLayout.Controls.Add(formPanel, 0, 1);
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

            // Кнопка "Сохранить"
            btnSave = new Button
            {
                Text = "💾 Сохранить",
                Size = new Size(150, 40),
                Location = new Point(buttonPanel.Width - 275, 10),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            // Кнопка "Отмена"
            btnCancel = new Button
            {
                Text = "❌ Отмена",
                Size = new Size(120, 40),
                Location = new Point(buttonPanel.Width - 125, 10),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();

            buttonPanel.Controls.AddRange(new Control[] { btnSave, btnCancel });
            mainLayout.Controls.Add(buttonPanel, 0, 2);
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 10, 0)
            };
        }

        private TextBox CreateTextBox()
        {
            return new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 15, 0, 5)
            };
        }

        private void LoadDisciplines()
        {
            try
            {
                var allDisciplines = GetAllDisciplines();
                clbDisciplines.Items.Clear();

                foreach (var discipline in allDisciplines.OrderBy(d => d.Name))
                {
                    clbDisciplines.Items.Add(discipline, false);
                }

                clbDisciplines.DisplayMember = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки дисциплин: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<Discipline> GetAllDisciplines()
        {
            var disciplines = new List<Discipline>();
            try
            {
                using (var conn = new Npgsql.NpgsqlConnection(DatabaseManager.Instance.GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand("SELECT id, name FROM disciplines ORDER BY name", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                disciplines.Add(new Discipline
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения дисциплин: {ex.Message}");
            }
            return disciplines;
        }

        private void TxtUsername_TextChanged(object sender, EventArgs e)
        {
            // Автоматическая проверка доступности логина
            string username = txtUsername.Text.Trim();
            if (username.Length >= 3)
            {
                if (teacherService.UsernameExists(username))
                {
                    txtUsername.BackColor = Color.FromArgb(255, 230, 230); // Светло-красный
                }
                else
                {
                    txtUsername.BackColor = Color.FromArgb(230, 255, 230); // Светло-зеленый
                }
            }
            else
            {
                txtUsername.BackColor = Color.White;
            }
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbDisciplines.Items.Count; i++)
            {
                clbDisciplines.SetItemChecked(i, true);
            }
        }

        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbDisciplines.Items.Count; i++)
            {
                clbDisciplines.SetItemChecked(i, false);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Собираем данные из формы
                var formData = new TeacherFormData
                {
                    FirstName = txtFirstName.Text.Trim(),
                    MiddleName = txtMiddleName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    Username = txtUsername.Text.Trim(),
                    Password = txtPassword.Text.Trim()
                };

                // Собираем выбранные дисциплины
                foreach (Discipline discipline in clbDisciplines.CheckedItems)
                {
                    formData.SelectedDisciplineIds.Add(discipline.Id);
                }

                // Валидация
                if (!formData.IsValid(out string errorMessage))
                {
                    MessageBox.Show(errorMessage, "Ошибка валидации",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Дополнительная проверка логина
                if (teacherService.UsernameExists(formData.Username))
                {
                    MessageBox.Show($"Пользователь с логином '{formData.Username}' уже существует!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUsername.Focus();
                    return;
                }

                // Сохранение
                if (teacherService.AddTeacher(
                    formData.FirstName,
                    formData.MiddleName,
                    formData.LastName,
                    formData.Username,
                    formData.Password,
                    formData.SelectedDisciplineIds,
                    adminUserId))
                {
                    MessageBox.Show(
                        $"Преподаватель '{formData.FullName}' успешно добавлен!\n\nЛогин: {formData.Username}\nПароль: {formData.Password}\nДисциплин назначено: {formData.SelectedDisciplineIds.Count}",
                        "Успех",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось добавить преподавателя. Попробуйте еще раз.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении преподавателя: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                DatabaseManager.Instance.LogAction(adminUserId, "ERROR",
                    $"Ошибка добавления преподавателя: {ex.Message}");
            }
        }

        // Дополнительная обработка нажатий клавиш
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                if (this.ActiveControl == txtLastName)
                {
                    txtFirstName.Focus();
                    return true;
                }
                else if (this.ActiveControl == txtFirstName)
                {
                    txtMiddleName.Focus();
                    return true;
                }
                else if (this.ActiveControl == txtMiddleName)
                {
                    txtUsername.Focus();
                    return true;
                }
                else if (this.ActiveControl == txtUsername)
                {
                    txtPassword.Focus();
                    return true;
                }
                else if (this.ActiveControl == txtPassword)
                {
                    clbDisciplines.Focus();
                    return true;
                }
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