using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UniversityGradesSystem.Models;
using UniversityGradesSystem.Services;
using System.Drawing;

namespace UniversityGradesSystem.Forms
{
    public partial class GradeEntryForm : Form
    {
        private readonly GradeService gradeService;
        private readonly DisciplineService disciplineService;
        private readonly StudentService studentService;
        private readonly TeacherService teacherService;
        private readonly GroupService groupService;
        private readonly int userId;
        private int? teacherId;

        // Простые элементы интерфейса
        private ComboBox cmbGroup;
        private ComboBox cmbDiscipline;
        private DataGridView dgvStudents;
        private Button btnSave;

        public GradeEntryForm(int userId)
        {
            this.userId = userId;

            try
            {
                string connString = DatabaseManager.Instance.GetConnectionString();

                // Инициализация сервисов
                gradeService = new GradeService(connString);
                disciplineService = new DisciplineService(connString);
                studentService = new StudentService(connString);
                teacherService = new TeacherService(connString);
                groupService = new GroupService(connString);

                // Получение teacherId
                teacherId = teacherService.GetTeacherId(userId);
                if (!teacherId.HasValue)
                {
                    MessageBox.Show("Преподаватель не найден. Обратитесь к администратору.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    InitializeComponent(); // Всё равно создаем интерфейс
                    return;
                }

                InitializeComponent();
                InitializeDataGridView();

                // Загружаем данные
                LoadGroups();
                LoadDisciplines();

                DatabaseManager.Instance.LogAction(userId, "FORM_OPENED", "Открыта форма выставления оценок");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации формы: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Создаем базовый интерфейс даже при ошибке
                try
                {
                    InitializeComponent();
                }
                catch
                {
                    this.Size = new Size(800, 600);
                    this.Text = "Выставление оценок (ошибка)";
                }
            }
        }

        private void InitializeDataGridView()
        {
            dgvStudents.Columns.Clear();

            // ID колонка (скрытая)
            dgvStudents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                ReadOnly = true,
                Visible = false
            });

            // ФИО колонка
            dgvStudents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FullName",
                HeaderText = "ФИО студента",
                Width = 400,
                ReadOnly = true
            });

            // Оценка колонка
            var gradeColumn = new DataGridViewComboBoxColumn
            {
                Name = "Grade",
                HeaderText = "Оценка",
                Width = 100
            };

            // Простой список оценок
            gradeColumn.Items.AddRange(new object[] { "Не выбрано", "2", "3", "4", "5" });
            dgvStudents.Columns.Add(gradeColumn);

            // Настройка внешнего вида
            dgvStudents.RowHeadersVisible = false;
            dgvStudents.AllowUserToAddRows = false;
            dgvStudents.AllowUserToDeleteRows = false;
            dgvStudents.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvStudents.MultiSelect = false;

            // Обработчик ошибок
            dgvStudents.DataError += (sender, e) => {
                e.ThrowException = false;
            };
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
                    MessageBox.Show("Группы не найдены в базе данных", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки групп: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDisciplines()
        {
            try
            {
                if (!teacherId.HasValue)
                {
                    MessageBox.Show("ID преподавателя не определен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var disciplines = disciplineService.GetTeacherDisciplines(teacherId.Value);

                if (disciplines != null && disciplines.Count > 0)
                {
                    cmbDiscipline.DisplayMember = "Name";
                    cmbDiscipline.ValueMember = "Id";
                    cmbDiscipline.DataSource = disciplines;
                    cmbDiscipline.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show($"У преподавателя нет назначенных дисциплин", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки дисциплин: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStudents()
        {
            if (cmbGroup.SelectedItem is Group selectedGroup)
            {
                try
                {
                    var students = studentService.GetStudentsByGroup(selectedGroup.Id);

                    dgvStudents.Rows.Clear();

                    if (students == null || students.Count == 0)
                    {
                        MessageBox.Show("В выбранной группе нет студентов", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    foreach (var student in students)
                    {
                        int rowIndex = dgvStudents.Rows.Add();
                        var row = dgvStudents.Rows[rowIndex];

                        row.Cells["Id"].Value = student.Id;
                        row.Cells["FullName"].Value = $"{student.LastName} {student.FirstName} {student.MiddleName}";

                        // Загружаем существующую оценку
                        if (cmbDiscipline.SelectedItem is Discipline selectedDiscipline)
                        {
                            var existingGrade = gradeService.GetStudentGrade(student.Id, selectedDiscipline.Id);
                            if (existingGrade.HasValue)
                            {
                                row.Cells["Grade"].Value = existingGrade.Value.ToString();
                            }
                            else
                            {
                                row.Cells["Grade"].Value = "Не выбрано";
                            }
                        }
                        else
                        {
                            row.Cells["Grade"].Value = "Не выбрано";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки студентов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                dgvStudents.Rows.Clear();
            }
        }

        private void CmbGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private void CmbDiscipline_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(cmbDiscipline.SelectedItem is Discipline selectedDiscipline) || !(cmbGroup.SelectedItem is Group selectedGroup))
                {
                    MessageBox.Show("Выберите группу и дисциплину", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Console.WriteLine($"Начинаем сохранение. Группа: {selectedGroup.Name}, Дисциплина: {selectedDiscipline.Name}");

                int savedCount = 0;
                int errorCount = 0;
                int processedCount = 0;

                foreach (DataGridViewRow row in dgvStudents.Rows)
                {
                    if (!row.IsNewRow && row.Cells["Grade"].Value != null && row.Cells["Id"].Value != null)
                    {
                        string gradeText = row.Cells["Grade"].Value.ToString();
                        Console.WriteLine($"Обрабатываем строку: ID={row.Cells["Id"].Value}, Grade={gradeText}");

                        // Пропускаем "Не выбрано"
                        if (gradeText == "Не выбрано")
                        {
                            Console.WriteLine("Пропускаем - не выбрано");
                            continue;
                        }

                        if (int.TryParse(gradeText, out int grade) && grade >= 2 && grade <= 5)
                        {
                            processedCount++;
                            int studentId = (int)row.Cells["Id"].Value;
                            int disciplineId = selectedDiscipline.Id;

                            Console.WriteLine($"Попытка сохранить: StudentId={studentId}, DisciplineId={disciplineId}, Grade={grade}");

                            if (gradeService.SaveGrade(studentId, disciplineId, grade))
                            {
                                savedCount++;
                                Console.WriteLine("Оценка успешно сохранена");
                            }
                            else
                            {
                                errorCount++;
                                Console.WriteLine("Ошибка сохранения оценки");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Неверное значение оценки: {gradeText}");
                        }
                    }
                }

                Console.WriteLine($"Результат: обработано={processedCount}, сохранено={savedCount}, ошибок={errorCount}");

                // Показываем результат
                if (savedCount > 0)
                {
                    string message = $"Успешно сохранено оценок: {savedCount}";
                    if (errorCount > 0)
                    {
                        message += $"\nОшибок при сохранении: {errorCount}";
                    }
                    MessageBox.Show(message, "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadStudents(); // Обновляем отображение
                }
                else if (errorCount > 0)
                {
                    MessageBox.Show($"Не удалось сохранить {errorCount} оценок. Проверьте консоль для деталей.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Не было выставлено ни одной оценки (обработано строк: {processedCount})", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения оценок: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"Исключение в BtnSave_Click: {ex}");
            }
        }
    }
}