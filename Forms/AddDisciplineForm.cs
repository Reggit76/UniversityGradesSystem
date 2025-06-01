// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Windows.Forms;
using System.Drawing;
using UniversityGradesSystem.Models;
using UniversityGradesSystem.Services;
using Npgsql;

namespace UniversityGradesSystem.Forms
{
    public partial class AddDisciplineForm : Form
    {
        private int adminUserId;

        // UI элементы
        private TableLayoutPanel mainLayout;
        private TextBox txtName;
        private Button btnSave;
        private Button btnCancel;

        public AddDisciplineForm(int adminUserId)
        {
            this.adminUserId = adminUserId;
            InitializeEnhancedComponent();
        }

        private void InitializeEnhancedComponent()
        {
            this.SuspendLayout();

            // === Настройки формы ===
            this.Text = "Добавить дисциплину";
            this.BackColor = Color.FromArgb(240, 244, 247);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(500, 350); // Увеличили размер формы

            // === Главный контейнер ===
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(15)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F)); // Заголовок
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 200F)); // Форма - фиксированная высота
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Кнопки

            // === Заголовочная панель ===
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(155, 89, 182),
                Padding = new Padding(15, 15, 15, 15)
            };

            Label titleLabel = new Label
            {
                Text = "📚 Создание новой дисциплины",
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
                Padding = new Padding(25, 20, 25, 20)
            };
            formPanel.BorderStyle = BorderStyle.FixedSingle;

            // === Лейбл ===
            Label lblName = new Label
            {
                Text = "Название дисциплины:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(0, 15),
                Size = new Size(200, 25),
                TextAlign = ContentAlignment.BottomLeft
            };

            // === Поле ввода ===
            txtName = new TextBox
            {
                Font = new Font("Segoe UI", 11F),
                Location = new Point(0, 45),
                Size = new Size(400, 35),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 100
            };

            // === Описание ===
            Label lblDescription = new Label
            {
                Text = "💡 Введите полное название дисциплины.\nПример: «Основы программирования», «Математический анализ», «История России»",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(127, 140, 141),
                Location = new Point(0, 90),
                Size = new Size(400, 60),
                TextAlign = ContentAlignment.TopLeft
            };

            // Добавляем элементы в панель формы
            formPanel.Controls.AddRange(new Control[] { lblName, txtName, lblDescription });
            mainLayout.Controls.Add(formPanel, 0, 1);

            // === Панель кнопок ===
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
                Size = new Size(120, 40),
                Location = new Point(buttonPanel.Width - 250, 10),
                BackColor = Color.FromArgb(189, 195, 199), // Изначально неактивная
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false, // Изначально отключена
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

            // === Добавляем главный контейнер на форму ===
            this.Controls.Add(mainLayout);

            // Устанавливаем фокус на поле ввода
            this.ActiveControl = txtName;

            // Настраиваем обработку изменения текста
            txtName.TextChanged += (sender, args) =>
            {
                // Обновляем состояние кнопки сохранения
                btnSave.Enabled = !string.IsNullOrWhiteSpace(txtName.Text) && txtName.Text.Trim().Length >= 3;

                // Меняем цвет кнопки в зависимости от состояния
                if (btnSave.Enabled)
                {
                    btnSave.BackColor = Color.FromArgb(155, 89, 182);
                }
                else
                {
                    btnSave.BackColor = Color.FromArgb(189, 195, 199);
                }
            };

            this.ResumeLayout(false);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Валидация поля
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название дисциплины!", "Ошибка валидации",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            string disciplineName = txtName.Text.Trim();

            // Проверяем длину названия
            if (disciplineName.Length < 3)
            {
                MessageBox.Show("Название дисциплины должно содержать не менее 3 символов!", "Ошибка валидации",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            try
            {
                // Проверяем, не существует ли уже такая дисциплина
                if (DisciplineExists(disciplineName))
                {
                    MessageBox.Show($"Дисциплина с названием '{disciplineName}' уже существует!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtName.Focus();
                    return;
                }

                // Сохраняем дисциплину
                if (AddDiscipline(disciplineName))
                {
                    MessageBox.Show(
                        $"Дисциплина '{disciplineName}' успешно добавлена!",
                        "Успех",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    DatabaseManager.Instance.LogAction(adminUserId, "ADD_DISCIPLINE",
                        $"Добавлена дисциплина: {disciplineName}");

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось добавить дисциплину. Попробуйте еще раз.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении дисциплины: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                DatabaseManager.Instance.LogAction(adminUserId, "ERROR",
                    $"Ошибка добавления дисциплины: {ex.Message}");
            }
        }

        private bool DisciplineExists(string disciplineName)
        {
            try
            {
                using (var conn = new NpgsqlConnection(DatabaseManager.Instance.GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM disciplines WHERE LOWER(name) = LOWER(@name)", conn))
                    {
                        cmd.Parameters.AddWithValue("name", disciplineName);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка проверки существования дисциплины: {ex.Message}");
                return false; // В случае ошибки считаем, что дисциплины нет
            }
        }

        private bool AddDiscipline(string disciplineName)
        {
            try
            {
                using (var conn = new NpgsqlConnection(DatabaseManager.Instance.GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("INSERT INTO disciplines (name) VALUES (@name)", conn))
                    {
                        cmd.Parameters.AddWithValue("name", disciplineName);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка добавления дисциплины в БД: {ex.Message}");
                return false;
            }
        }

        // Дополнительная обработка нажатий клавиш
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter && btnSave.Enabled)
            {
                BtnSave_Click(btnSave, EventArgs.Empty);
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