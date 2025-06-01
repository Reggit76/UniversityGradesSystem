// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
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
    public partial class EnhancedAddStudentForm : Form
    {
        private int adminUserId;
        private StudentService studentService;
        private GroupService groupService;

        // UI элементы
        private TableLayoutPanel mainLayout;
        private TextBox txtFirstName;
        private TextBox txtMiddleName;
        private TextBox txtLastName;
        private ComboBox cmbGroup;
        private Button btnSave;
        private Button btnCancel;

        public EnhancedAddStudentForm(int adminUserId)
        {
            this.adminUserId = adminUserId;
            this.studentService = new StudentService(DatabaseManager.Instance.GetConnectionString());
            this.groupService = new GroupService(DatabaseManager.Instance.GetConnectionString());

            InitializeEnhancedComponent();
            LoadGroups();
        }

        private void InitializeEnhancedComponent()
        {
            this.SuspendLayout();

            // === Настройки формы ===
            this.Text = "Добавить студента";
            this.BackColor = Color.FromArgb(240, 244, 247);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(500, 480); // Увеличили размер формы

            // === Главный контейнер ===
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(15)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F)); // Заголовок
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 320F)); // Форма - фиксированная высота
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Кнопки

            // === Заголовочная панель ===
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(46, 204, 113),
                Padding = new Padding(15, 15, 15, 15)
            };

            Label titleLabel = new Label
            {
                Text = "📝 Добавление нового студента",
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
                Padding = new Padding(25, 20, 25, 20) // Увеличили отступы
            };
            formPanel.BorderStyle = BorderStyle.FixedSingle;

            // Используем обычный подход с абсолютным позиционированием для более точного контроля
            int startY = 15;
            int labelHeight = 25;
            int textBoxHeight = 35;
            int spacing = 55; // Расстояние между полями

            // === Поле "Имя" ===
            Label lblFirstName = new Label
            {
                Text = "Имя:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(0, startY),
                Size = new Size(120, labelHeight),
                TextAlign = ContentAlignment.BottomLeft
            };

            txtFirstName = new TextBox
            {
                Font = new Font("Segoe UI", 11F),
                Location = new Point(0, startY + labelHeight + 3),
                Size = new Size(400, textBoxHeight),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 100
            };

            // === Поле "Отчество" ===
            Label lblMiddleName = new Label
            {
                Text = "Отчество:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(0, startY + spacing),
                Size = new Size(120, labelHeight),
                TextAlign = ContentAlignment.BottomLeft
            };

            txtMiddleName = new TextBox
            {
                Font = new Font("Segoe UI", 11F),
                Location = new Point(0, startY + spacing + labelHeight + 3),
                Size = new Size(400, textBoxHeight),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 100
            };

            // === Поле "Фамилия" ===
            Label lblLastName = new Label
            {
                Text = "Фамилия:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(0, startY + spacing * 2),
                Size = new Size(120, labelHeight),
                TextAlign = ContentAlignment.BottomLeft
            };

            txtLastName = new TextBox
            {
                Font = new Font("Segoe UI", 11F),
                Location = new Point(0, startY + spacing * 2 + labelHeight + 3),
                Size = new Size(400, textBoxHeight),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 100
            };

            // === Поле "Группа" ===
            Label lblGroup = new Label
            {
                Text = "Группа:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(0, startY + spacing * 3),
                Size = new Size(120, labelHeight),
                TextAlign = ContentAlignment.BottomLeft
            };

            cmbGroup = new ComboBox
            {
                Font = new Font("Segoe UI", 11F),
                Location = new Point(0, startY + spacing * 3 + labelHeight + 3),
                Size = new Size(400, textBoxHeight),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Popup
            };

            // Добавляем все элементы в панель формы
            formPanel.Controls.AddRange(new Control[] {
                lblFirstName, txtFirstName,
                lblMiddleName, txtMiddleName,
                lblLastName, txtLastName,
                lblGroup, cmbGroup
            });

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
                BackColor = Color.FromArgb(46, 204, 113),
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

            // === Добавляем главный контейнер на форму ===
            this.Controls.Add(mainLayout);

            // Устанавливаем фокус на первое поле
            this.ActiveControl = txtFirstName;

            this.ResumeLayout(false);
        }

        private void LoadGroups()
        {
            try
            {
                var groups = groupService.GetAllGroups();

                if (groups != null && groups.Count > 0)
                {
                    cmbGroup.DisplayMember = "Name";
                    cmbGroup.ValueMember = "Id";
                    cmbGroup.DataSource = groups;
                    cmbGroup.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show("В системе нет групп. Сначала создайте группы.", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки групп: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Валидация полей
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Введите имя студента!", "Ошибка валидации",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirstName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMiddleName.Text))
            {
                MessageBox.Show("Введите отчество студента!", "Ошибка валидации",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMiddleName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Введите фамилию студента!", "Ошибка валидации",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLastName.Focus();
                return;
            }

            if (cmbGroup.SelectedItem == null)
            {
                MessageBox.Show("Выберите группу!", "Ошибка валидации",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbGroup.Focus();
                return;
            }

            try
            {
                // Создаем объект студента
                var student = new Student
                {
                    FirstName = txtFirstName.Text.Trim(),
                    MiddleName = txtMiddleName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    GroupId = ((Group)cmbGroup.SelectedItem).Id
                };

                // Сохраняем студента
                if (studentService.AddStudent(student, adminUserId))
                {
                    MessageBox.Show(
                        $"Студент '{student.LastName} {student.FirstName} {student.MiddleName}' успешно добавлен!",
                        "Успех",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось добавить студента. Попробуйте еще раз.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении студента: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                DatabaseManager.Instance.LogAction(adminUserId, "ERROR",
                    $"Ошибка добавления студента: {ex.Message}");
            }
        }

        // Дополнительная валидация при нажатии Enter
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                if (this.ActiveControl == txtFirstName)
                {
                    txtMiddleName.Focus();
                    return true;
                }
                else if (this.ActiveControl == txtMiddleName)
                {
                    txtLastName.Focus();
                    return true;
                }
                else if (this.ActiveControl == txtLastName)
                {
                    cmbGroup.Focus();
                    return true;
                }
                else if (this.ActiveControl == cmbGroup)
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