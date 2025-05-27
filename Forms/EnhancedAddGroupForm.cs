using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using UniversityGradesSystem.Models;
using UniversityGradesSystem.Services;
using Npgsql;

namespace UniversityGradesSystem.Forms
{
    public partial class EnhancedAddGroupForm : Form
    {
        private int adminUserId;
        private GroupService groupService;

        // UI элементы
        private TableLayoutPanel mainLayout;
        private TextBox txtName;
        private ComboBox cmbSpecialty;
        private ComboBox cmbCourse;
        private Button btnSave;
        private Button btnCancel;

        public EnhancedAddGroupForm(int adminUserId)
        {
            this.adminUserId = adminUserId;
            this.groupService = new GroupService(DatabaseManager.Instance.GetConnectionString());

            InitializeEnhancedComponent();
            LoadSpecialties();
            LoadCourses();
        }

        private void InitializeEnhancedComponent()
        {
            this.SuspendLayout();

            // === Настройки формы ===
            this.Text = "Добавить группу";
            this.BackColor = Color.FromArgb(240, 244, 247);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(450, 350);

            // === Главный контейнер ===
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(20)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Заголовок
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Форма
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Кнопки

            // === Заголовочная панель ===
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(52, 152, 219),
                Padding = new Padding(15, 10, 15, 10)
            };

            Label titleLabel = new Label
            {
                Text = "👥 Создание новой группы",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            headerPanel.Controls.Add(titleLabel);
            mainLayout.Controls.Add(headerPanel, 0, 0);

            // === Панель формы ===
            Panel formPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20, 15, 20, 15)
            };
            formPanel.BorderStyle = BorderStyle.FixedSingle;

            // Используем TableLayoutPanel для полей формы
            TableLayoutPanel fieldsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = Color.Transparent
            };

            // Настраиваем колонки
            fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Настраиваем строки
            for (int i = 0; i < 3; i++)
            {
                fieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            }

            // === Поле "Название группы" ===
            Label lblName = new Label
            {
                Text = "Название:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            txtName = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Margin = new Padding(0, 10, 0, 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 20
            };

            fieldsLayout.Controls.Add(lblName, 0, 0);
            fieldsLayout.Controls.Add(txtName, 1, 0);

            // === Поле "Специальность" ===
            Label lblSpecialty = new Label
            {
                Text = "Специальность:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            cmbSpecialty = new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 10, 0, 10),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            fieldsLayout.Controls.Add(lblSpecialty, 0, 1);
            fieldsLayout.Controls.Add(cmbSpecialty, 1, 1);

            // === Поле "Курс" ===
            Label lblCourse = new Label
            {
                Text = "Курс:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            cmbCourse = new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 10, 0, 10),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            fieldsLayout.Controls.Add(lblCourse, 0, 2);
            fieldsLayout.Controls.Add(cmbCourse, 1, 2);

            formPanel.Controls.Add(fieldsLayout);
            mainLayout.Controls.Add(formPanel, 0, 1);

            // === Панель кнопок ===
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };
            buttonPanel.BorderStyle = BorderStyle.FixedSingle;

            TableLayoutPanel buttonLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Пустое место
            buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F)); // Кнопка "Сохранить"
            buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F)); // Кнопка "Отмена"

            // Кнопка "Сохранить"
            btnSave = new Button
            {
                Text = "💾 Сохранить",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(5, 5, 2, 5)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            // Кнопка "Отмена"
            btnCancel = new Button
            {
                Text = "❌ Отмена",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(3, 5, 5, 5),
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();

            buttonLayout.Controls.Add(new Panel(), 0, 0); // Пустое место
            buttonLayout.Controls.Add(btnSave, 1, 0);
            buttonLayout.Controls.Add(btnCancel, 2, 0);

            buttonPanel.Controls.Add(buttonLayout);
            mainLayout.Controls.Add(buttonPanel, 0, 2);

            // === Добавляем главный контейнер на форму ===
            this.Controls.Add(mainLayout);

            // Устанавливаем фокус на первое поле
            this.ActiveControl = txtName;

            this.ResumeLayout(false);
        }

        private void LoadSpecialties()
        {
            try
            {
                var specialties = GetSpecialties();

                if (specialties.Count > 0)
                {
                    cmbSpecialty.DisplayMember = "Name";
                    cmbSpecialty.ValueMember = "Id";
                    cmbSpecialty.DataSource = specialties;
                    cmbSpecialty.SelectedIndex = -1;
                }
                else
                {
                    // Если специальностей нет, создаем дефолтные
                    CreateDefaultSpecialties();
                    specialties = GetSpecialties();

                    cmbSpecialty.DisplayMember = "Name";
                    cmbSpecialty.ValueMember = "Id";
                    cmbSpecialty.DataSource = specialties;
                    cmbSpecialty.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки специальностей: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCourses()
        {
            try
            {
                var courses = GetCourses();

                if (courses.Count > 0)
                {
                    cmbCourse.DisplayMember = "DisplayText";
                    cmbCourse.ValueMember = "Id";
                    cmbCourse.DataSource = courses;
                    cmbCourse.SelectedIndex = -1;
                }
                else
                {
                    // Если курсов нет, создаем дефолтные
                    CreateDefaultCourses();
                    courses = GetCourses();

                    cmbCourse.DisplayMember = "DisplayText";
                    cmbCourse.ValueMember = "Id";
                    cmbCourse.DataSource = courses;
                    cmbCourse.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки курсов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<Specialty> GetSpecialties()
        {
            var specialties = new List<Specialty>();

            try
            {
                using (var conn = new NpgsqlConnection(DatabaseManager.Instance.GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT id, name FROM specialties ORDER BY name", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                specialties.Add(new Specialty
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
                Console.WriteLine($"Ошибка получения специальностей: {ex.Message}");
            }

            return specialties;
        }

        private List<CourseDisplay> GetCourses()
        {
            var courses = new List<CourseDisplay>();

            try
            {
                using (var conn = new NpgsqlConnection(DatabaseManager.Instance.GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT id, number FROM courses ORDER BY number", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                courses.Add(new CourseDisplay
                                {
                                    Id = reader.GetInt32(0),
                                    Number = reader.GetInt32(1),
                                    DisplayText = $"{reader.GetInt32(1)} курс"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения курсов: {ex.Message}");
            }

            return courses;
        }

        private void CreateDefaultSpecialties()
        {
            try
            {
                using (var conn = new NpgsqlConnection(DatabaseManager.Instance.GetConnectionString()))
                {
                    conn.Open();

                    var defaultSpecialties = new[]
                    {
                        "Информатика и вычислительная техника",
                        "Программная инженерия",
                        "Информационные системы и технологии",
                        "Прикладная математика и информатика",
                        "Математическое обеспечение и администрирование информационных систем"
                    };

                    foreach (var specialty in defaultSpecialties)
                    {
                        using (var cmd = new NpgsqlCommand(
                            "INSERT INTO specialties (name) VALUES (@name) ON CONFLICT (name) DO NOTHING", conn))
                        {
                            cmd.Parameters.AddWithValue("name", specialty);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания дефолтных специальностей: {ex.Message}");
            }
        }

        private void CreateDefaultCourses()
        {
            try
            {
                using (var conn = new NpgsqlConnection(DatabaseManager.Instance.GetConnectionString()))
                {
                    conn.Open();

                    for (int i = 1; i <= 4; i++)
                    {
                        using (var cmd = new NpgsqlCommand(
                            "INSERT INTO courses (number) VALUES (@number) ON CONFLICT (number) DO NOTHING", conn))
                        {
                            cmd.Parameters.AddWithValue("number", i);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания дефолтных курсов: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Валидация полей
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название группы!", "Ошибка валидации",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (cmbSpecialty.SelectedItem == null)
            {
                MessageBox.Show("Выберите специальность!", "Ошибка валидации",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbSpecialty.Focus();
                return;
            }

            if (cmbCourse.SelectedItem == null)
            {
                MessageBox.Show("Выберите курс!", "Ошибка валидации",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCourse.Focus();
                return;
            }

            try
            {
                // Создаем объект группы
                var group = new Group
                {
                    Name = txtName.Text.Trim(),
                    SpecialtyId = ((Specialty)cmbSpecialty.SelectedItem).Id,
                    CourseId = ((CourseDisplay)cmbCourse.SelectedItem).Id
                };

                // Сохраняем группу
                if (groupService.AddGroup(group, adminUserId))
                {
                    MessageBox.Show(
                        $"Группа '{group.Name}' успешно создана!",
                        "Успех",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось создать группу. Возможно, группа с таким названием уже существует.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании группы: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                DatabaseManager.Instance.LogAction(adminUserId, "ERROR",
                    $"Ошибка создания группы: {ex.Message}");
            }
        }

        // Дополнительная валидация при нажатии Enter
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                if (this.ActiveControl == txtName)
                {
                    cmbSpecialty.Focus();
                    return true;
                }
                else if (this.ActiveControl == cmbSpecialty)
                {
                    cmbCourse.Focus();
                    return true;
                }
                else if (this.ActiveControl == cmbCourse)
                {
                    BtnSave_Click(btnSave, EventArgs.Empty);
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