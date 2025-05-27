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
            this.Size = new Size(450, 420);

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
                BackColor = Color.FromArgb(46, 204, 113),
                Padding = new Padding(15, 10, 15, 10)
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
                Padding = new Padding(20, 15, 20, 15)
            };
            formPanel.BorderStyle = BorderStyle.FixedSingle;

            // Используем TableLayoutPanel для полей формы
            TableLayoutPanel fieldsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                BackColor = Color.Transparent
            };

            // Настраиваем колонки
            fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Настраиваем строки
            for (int i = 0; i < 4; i++)
            {
                fieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            }

            // === Поле "Имя" ===
            Label lblFirstName = new Label
            {
                Text = "Имя:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            txtFirstName = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Margin = new Padding(0, 10, 0, 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 100
            };

            fieldsLayout.Controls.Add(lblFirstName, 0, 0);
            fieldsLayout.Controls.Add(txtFirstName, 1, 0);

            // === Поле "Отчество" ===
            Label lblMiddleName = new Label
            {
                Text = "Отчество:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            txtMiddleName = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Margin = new Padding(0, 10, 0, 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 100
            };

            fieldsLayout.Controls.Add(lblMiddleName, 0, 1);
            fieldsLayout.Controls.Add(txtMiddleName, 1, 1);

            // === Поле "Фамилия" ===
            Label lblLastName = new Label
            {
                Text = "Фамилия:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            txtLastName = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Margin = new Padding(0, 10, 0, 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 100
            };

            fieldsLayout.Controls.Add(lblLastName, 0, 2);
            fieldsLayout.Controls.Add(txtLastName, 1, 2);

            // === Поле "Группа" ===
            Label lblGroup = new Label
            {
                Text = "Группа:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            cmbGroup = new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 10, 0, 10),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            fieldsLayout.Controls.Add(lblGroup, 0, 3);
            fieldsLayout.Controls.Add(cmbGroup, 1, 3);

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
                BackColor = Color.FromArgb(46, 204, 113),
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