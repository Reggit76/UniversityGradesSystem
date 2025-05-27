using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using UniversityGradesSystem.Models;
using UniversityGradesSystem.Services;
using System.Drawing;
using Npgsql;

namespace UniversityGradesSystem.Forms
{
    public partial class EnhancedGradeEntryForm : Form
    {
        private readonly GradeService gradeService;
        private readonly DisciplineService disciplineService;
        private readonly StudentService studentService;
        private readonly TeacherService teacherService;
        private readonly GroupService groupService;
        private readonly int userId;
        private int? teacherId;

        // Простые элементы интерфейса
        private ComboBox cmbCourse;
        private ComboBox cmbSemester;
        private ComboBox cmbGroup;
        private ComboBox cmbDiscipline;
        private DataGridView dgvStudents;
        private Button btnSave;
        private Button btnRefresh;

        public EnhancedGradeEntryForm(int userId)
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
                    InitializeEnhancedComponent();
                    return;
                }

                InitializeComponent();
                InitializeDataGridView();

                // Загружаем данные
                LoadCourses();
                LoadSemesters();
                LoadGroups();
                LoadDisciplines();

                DatabaseManager.Instance.LogAction(userId, "FORM_OPENED", "Открыта улучшенная форма выставления оценок");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации формы: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                try
                {
                    InitializeEnhancedComponent();
                }
                catch
                {
                    this.Size = new Size(800, 600);
                    this.Text = "Выставление оценок (ошибка)";
                }
            }
        }

        private void InitializeEnhancedComponent()
        {
            this.SuspendLayout();

            // === Настройки формы ===
            this.Text = "Выставление оценок";
            this.BackColor = Color.WhiteSmoke;
            this.MinimumSize = new Size(1000, 700);

            // === Главный контейнер ===
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(20)
            };

            // Настраиваем строки
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // Заголовок
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F)); // Фильтры (увеличили высоту)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Таблица
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // Кнопки

            // === Заголовок ===
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(52, 73, 94),
                Padding = new Padding(20, 10, 20, 10)
            };

            Label titleLabel = new Label
            {
                Text = "📝 Выставление оценок студентам",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            headerPanel.Controls.Add(titleLabel);
            mainLayout.Controls.Add(headerPanel, 0, 0);

            // === Панель фильтров (расширенная) ===
            TableLayoutPanel filtersPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3, // Добавили еще одну строку
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };
            filtersPanel.BorderStyle = BorderStyle.FixedSingle;

            // Настраиваем колонки для фильтров
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            // Настраиваем строки
            filtersPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F)); // Лейблы
            filtersPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F)); // Верхняя строка комбобоксов
            filtersPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F)); // Нижняя строка комбобоксов

            // === Лейблы ===
            var lblCourse = new Label
            {
                Text = "Курс:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                Margin = new Padding(0, 0, 10, 0)
            };

            var lblSemester = new Label
            {
                Text = "Семестр:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                Margin = new Padding(0, 0, 10, 0)
            };

            var lblGroup = new Label
            {
                Text = "Группа:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                Margin = new Padding(0, 0, 10, 0)
            };

            var lblDiscipline = new Label
            {
                Text = "Дисциплина:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                Margin = new Padding(0, 0, 10, 0)
            };

            // === Комбобоксы ===
            this.cmbCourse = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 5, 15, 5)
            };
            this.cmbCourse.SelectedIndexChanged += CmbCourse_SelectedIndexChanged;

            this.cmbSemester = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 5, 15, 5)
            };
            this.cmbSemester.SelectedIndexChanged += CmbSemester_SelectedIndexChanged;

            this.cmbGroup = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 5, 15, 5)
            };
            this.cmbGroup.SelectedIndexChanged += CmbGroup_SelectedIndexChanged;

            this.cmbDiscipline = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 5, 15, 5)
            };
            this.cmbDiscipline.SelectedIndexChanged += CmbDiscipline_SelectedIndexChanged;

            // === Кнопка обновления ===
            btnRefresh = new Button
            {
                Text = "🔄 Обновить данные",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(5, 5, 5, 5)
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (sender, e) => LoadStudents();

            // Добавляем элементы фильтров
            filtersPanel.Controls.Add(lblCourse, 0, 0);
            filtersPanel.Controls.Add(lblSemester, 1, 0);
            filtersPanel.Controls.Add(lblGroup, 2, 0);
            filtersPanel.Controls.Add(lblDiscipline, 3, 0);

            filtersPanel.Controls.Add(this.cmbCourse, 0, 1);
            filtersPanel.Controls.Add(this.cmbSemester, 1, 1);
            filtersPanel.Controls.Add(this.cmbGroup, 2, 1);
            filtersPanel.Controls.Add(this.cmbDiscipline, 3, 1);

            filtersPanel.Controls.Add(btnRefresh, 0, 2);

            mainLayout.Controls.Add(filtersPanel, 0, 1);

            // === Таблица студентов ===
            this.dgvStudents = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9F),
                GridColor = Color.FromArgb(224, 224, 224),
                Margin = new Padding(0, 10, 0, 10)
            };

            // Стилизация заголовков таблицы
            this.dgvStudents.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            this.dgvStudents.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            this.dgvStudents.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.dgvStudents.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dgvStudents.ColumnHeadersHeight = 40;
            this.dgvStudents.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            mainLayout.Controls.Add(this.dgvStudents, 0, 2);

            // === Панель кнопок ===
            TableLayoutPanel buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };
            buttonPanel.BorderStyle = BorderStyle.FixedSingle;

            // Настраиваем колонки для кнопок
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F)); // Кнопка сохранения
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); // Информационный текст
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // Пустое место

            // === Кнопка сохранения ===
            this.btnSave = new Button
            {
                Text = "💾 Сохранить оценки",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 5, 15, 5)
            };
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.Click += BtnSave_Click;

            // === Информационная метка ===
            var lblInfo = new Label
            {
                Text = "💡 Выберите курс, семестр, группу и дисциплину, затем выставьте оценки студентам",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(127, 140, 141),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Добавляем элементы панели кнопок
            buttonPanel.Controls.Add(this.btnSave, 0, 0);
            buttonPanel.Controls.Add(lblInfo, 1, 0);

            mainLayout.Controls.Add(buttonPanel, 0, 3);

            // === Добавляем главный контейнер на форму ===
            this.Controls.Add(mainLayout);

            this.ResumeLayout(false);
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

            // Список оценок
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки курсов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSemesters()
        {
            try
            {
                var semesters = GetSemesters();

                if (semesters.Count > 0)
                {
                    cmbSemester.DisplayMember = "DisplayText";
                    cmbSemester.ValueMember = "Id";
                    cmbSemester.DataSource = semesters;
                    cmbSemester.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки семестров: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private List<SemesterDisplay> GetSemesters()
        {
            var semesters = new List<SemesterDisplay>();

            try
            {
                using (var conn = new NpgsqlConnection(DatabaseManager.Instance.GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT s.id, s.number, c.number as course_number 
                        FROM semesters s 
                        JOIN courses c ON s.course_id = c.id 
                        ORDER BY c.number, s.number", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                semesters.Add(new SemesterDisplay
                                {
                                    Id = reader.GetInt32(0),
                                    Number = reader.GetInt32(1),
                                    CourseNumber = reader.GetInt32(2),
                                    DisplayText = $"{reader.GetInt32(1)} семестр ({reader.GetInt32(2)} курс)"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения семестров: {ex.Message}");
            }

            return semesters;
        }

        private void LoadStudents()
        {
            if (cmbGroup.SelectedItem is Group selectedGroup &&
                cmbDiscipline.SelectedItem is Discipline selectedDiscipline)
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

        // Обработчики событий фильтров
        private void CmbCourse_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private void CmbSemester_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStudents();
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
                if (!(cmbDiscipline.SelectedItem is Discipline selectedDiscipline) ||
                    !(cmbGroup.SelectedItem is Group selectedGroup))
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