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
    public partial class TeacherDisciplinesForm : Form
    {
        private readonly TeacherWithDetails teacher;
        private readonly EnhancedTeacherService teacherService;
        private readonly int adminUserId;

        // UI элементы
        private TableLayoutPanel mainLayout;
        private CheckedListBox clbDisciplines;
        private Button btnSave;
        private Button btnCancel;
        private Button btnSelectAll;
        private Button btnClearAll;
        private Label lblInfo;
        private Label lblStatus;

        private List<TeacherDisciplineAssignment> disciplineAssignments;

        public TeacherDisciplinesForm(TeacherWithDetails teacher, int adminUserId)
        {
            this.teacher = teacher;
            this.adminUserId = adminUserId;
            this.teacherService = new EnhancedTeacherService(DatabaseManager.Instance.GetConnectionString());

            InitializeComponent();
            InitializeEnhancedComponent();
            LoadDisciplineAssignments();
        }

        private void InitializeEnhancedComponent()
        {
            this.SuspendLayout();

            // === Настройки формы ===
            this.Text = $"Управление дисциплинами: {teacher.FullName}";
            this.BackColor = Color.FromArgb(240, 244, 247);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(500, 600);

            // === Главный контейнер ===
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(15)
            };

            // Настраиваем строки
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F)); // Заголовок
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // Информация
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); // Кнопки управления
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Список дисциплин
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Кнопки действий
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); // Статус

            // === Заголовочная панель ===
            CreateHeaderPanel();

            // === Информационная панель ===
            CreateInfoPanel();

            // === Панель кнопок управления выбором ===
            CreateSelectionButtonsPanel();

            // === Список дисциплин ===
            CreateDisciplinesList();

            // === Панель кнопок действий ===
            CreateActionButtonsPanel();

            // === Статусная строка ===
            CreateStatusPanel();

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
                Text = "📚 Управление дисциплинами преподавателя",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            headerPanel.Controls.Add(titleLabel);
            mainLayout.Controls.Add(headerPanel, 0, 0);
        }

        private void CreateInfoPanel()
        {
            Panel infoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(52, 152, 219),
                Padding = new Padding(15, 10, 15, 10)
            };

            lblInfo = new Label
            {
                Text = $"Преподаватель: {teacher.FullName} (логин: {teacher.Username})",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            infoPanel.Controls.Add(lblInfo);
            mainLayout.Controls.Add(infoPanel, 0, 1);
        }

        private void CreateSelectionButtonsPanel()
        {
            TableLayoutPanel selectionPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 5, 0, 5)
            };

            // Настраиваем колонки
            selectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // Выбрать все
            selectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // Очистить все
            selectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Остальное

            btnSelectAll = new Button
            {
                Text = "✓ Выбрать все",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 10, 0)
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
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 10, 0)
            };
            btnClearAll.FlatAppearance.BorderSize = 0;
            btnClearAll.Click += BtnClearAll_Click;

            selectionPanel.Controls.Add(btnSelectAll, 0, 0);
            selectionPanel.Controls.Add(btnClearAll, 1, 0);

            mainLayout.Controls.Add(selectionPanel, 0, 2);
        }

        private void CreateDisciplinesList()
        {
            Panel listPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(5)
            };
            listPanel.BorderStyle = BorderStyle.FixedSingle;

            clbDisciplines = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                ScrollAlwaysVisible = true,
                BorderStyle = BorderStyle.None
            };

            // Цветовое выделение назначенных дисциплин
            clbDisciplines.DrawMode = DrawMode.OwnerDrawFixed;
            clbDisciplines.DrawItem += ClbDisciplines_DrawItem;

            listPanel.Controls.Add(clbDisciplines);
            mainLayout.Controls.Add(listPanel, 0, 3);
        }

        private void CreateActionButtonsPanel()
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
                Text = "💾 Сохранить изменения",
                Size = new Size(180, 40),
                Location = new Point(buttonPanel.Width - 305, 10),
                BackColor = Color.FromArgb(155, 89, 182),
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
                BackColor = Color.FromArgb(149, 165, 166),
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
            mainLayout.Controls.Add(buttonPanel, 0, 4);
        }

        private void CreateStatusPanel()
        {
            lblStatus = new Label
            {
                Text = "Загрузка данных...",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(127, 140, 141),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 5, 0)
            };

            mainLayout.Controls.Add(lblStatus, 0, 5);
        }

        private void LoadDisciplineAssignments()
        {
            try
            {
                UpdateStatus("Загрузка дисциплин...");

                disciplineAssignments = teacherService.GetTeacherDisciplineAssignments(teacher.Id);
                clbDisciplines.Items.Clear();

                foreach (var assignment in disciplineAssignments.OrderBy(a => a.DisciplineName))
                {
                    int index = clbDisciplines.Items.Add(assignment);
                    clbDisciplines.SetItemChecked(index, assignment.IsAssigned);
                }

                clbDisciplines.DisplayMember = "DisciplineName";

                int assignedCount = disciplineAssignments.Count(a => a.IsAssigned);
                UpdateStatus($"Загружено {disciplineAssignments.Count} дисциплин, назначено: {assignedCount}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки дисциплин: {ex.Message}", "Ошибка",
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

        private void ClbDisciplines_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            var assignment = (TeacherDisciplineAssignment)clbDisciplines.Items[e.Index];
            bool isChecked = clbDisciplines.GetItemChecked(e.Index);

            // Определяем цвета
            Color backColor = Color.White;
            Color textColor = Color.Black;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                backColor = Color.FromArgb(52, 152, 219);
                textColor = Color.White;
            }
            else if (isChecked)
            {
                backColor = Color.FromArgb(230, 255, 230); // Светло-зеленый для назначенных
                textColor = Color.FromArgb(27, 94, 32);
            }

            // Рисуем фон
            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            // Рисуем чекбокс
            CheckBoxRenderer.DrawCheckBox(e.Graphics,
                new Point(e.Bounds.Left + 2, e.Bounds.Top + 2),
                isChecked ? System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal :
                           System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);

            // Рисуем текст
            using (var brush = new SolidBrush(textColor))
            {
                var textRect = new Rectangle(e.Bounds.Left + 20, e.Bounds.Top,
                                           e.Bounds.Width - 20, e.Bounds.Height);
                e.Graphics.DrawString(assignment.DisciplineName, e.Font, brush, textRect,
                                    StringFormat.GenericDefault);
            }

            // Рисуем рамку фокуса
            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds);
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
                UpdateStatus("Сохранение изменений...");

                // Собираем ID выбранных дисциплин
                var selectedDisciplineIds = new List<int>();
                for (int i = 0; i < clbDisciplines.Items.Count; i++)
                {
                    if (clbDisciplines.GetItemChecked(i))
                    {
                        var assignment = (TeacherDisciplineAssignment)clbDisciplines.Items[i];
                        selectedDisciplineIds.Add(assignment.DisciplineId);
                    }
                }

                // Проверяем, есть ли изменения
                var originallyAssigned = disciplineAssignments.Where(a => a.IsAssigned).Select(a => a.DisciplineId).ToList();

                bool hasChanges = selectedDisciplineIds.Count != originallyAssigned.Count ||
                                selectedDisciplineIds.Except(originallyAssigned).Any() ||
                                originallyAssigned.Except(selectedDisciplineIds).Any();

                if (!hasChanges)
                {
                    MessageBox.Show("Изменений не обнаружено.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Сохраняем изменения
                if (teacherService.UpdateTeacherDisciplines(teacher.Id, selectedDisciplineIds, adminUserId))
                {
                    string message = $"Изменения успешно сохранены!\n\n" +
                                   $"Преподавателю {teacher.FullName} назначено дисциплин: {selectedDisciplineIds.Count}";

                    if (selectedDisciplineIds.Count > 0)
                    {
                        var disciplineNames = clbDisciplines.CheckedItems.Cast<TeacherDisciplineAssignment>()
                                                                       .Select(a => a.DisciplineName)
                                                                       .OrderBy(name => name);
                        message += "\n\nСписок дисциплин:\n• " + string.Join("\n• ", disciplineNames);
                    }

                    MessageBox.Show(message, "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось сохранить изменения!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatus("Ошибка сохранения");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка: {ex.Message}");
                MessageBox.Show($"Ошибка сохранения изменений: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                DatabaseManager.Instance.LogAction(adminUserId, "ERROR",
                    $"Ошибка обновления дисциплин преподавателя {teacher.Id}: {ex.Message}");
            }
        }

        // Дополнительная обработка нажатий клавиш
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                BtnSave_Click(btnSave, EventArgs.Empty);
                return true;
            }
            else if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            else if (keyData == Keys.F1) // Выбрать все
            {
                BtnSelectAll_Click(btnSelectAll, EventArgs.Empty);
                return true;
            }
            else if (keyData == Keys.F2) // Очистить все
            {
                BtnClearAll_Click(btnClearAll, EventArgs.Empty);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}