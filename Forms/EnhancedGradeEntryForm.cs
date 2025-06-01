// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
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

        // UI элементы
        private ComboBox cmbCourse;
        private ComboBox cmbSemester;
        private ComboBox cmbGroup;
        private ComboBox cmbDiscipline;
        private DataGridView dgvStudents;
        private Button btnSave;
        private Button btnRefresh;
        private Label lblStatus;

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
                    MessageBox.Show("Преподаватель не найден. Обратитесь к администратору.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                InitializeEnhancedComponent();

                // Создание базовых данных если их нет
                EnsureBasicDataExists();

                // Загружаем данные
                LoadInitialData();

                DatabaseManager.Instance.LogAction(userId, "FORM_OPENED", "Открыта форма выставления оценок");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации формы: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DatabaseManager.Instance.LogAction(userId, "ERROR", $"Ошибка инициализации формы оценок: {ex.Message}");

                // Создаем базовую форму даже при ошибке
                InitializeBasicForm();
            }
        }

        private void InitializeBasicForm()
        {
            this.Size = new Size(800, 600);
            this.Text = "Выставление оценок (ошибка инициализации)";
            this.BackColor = Color.WhiteSmoke;

            Label errorLabel = new Label
            {
                Text = "Произошла ошибка при инициализации формы.\nОбратитесь к администратору.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.Red
            };

            this.Controls.Add(errorLabel);
        }

        private void EnsureBasicDataExists()
        {
            try
            {
                using (var conn = new NpgsqlConnection(DatabaseManager.Instance.GetConnectionString()))
                {
                    conn.Open();

                    // Создаем курсы если их нет
                    for (int i = 1; i <= 4; i++)
                    {
                        using (var cmd = new NpgsqlCommand(
                            "INSERT INTO courses (number) VALUES (@number) ON CONFLICT (number) DO NOTHING", conn))
                        {
                            cmd.Parameters.AddWithValue("number", i);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Создаем семестры для каждого курса если их нет
                    for (int courseNum = 1; courseNum <= 4; courseNum++)
                    {
                        // Получаем ID курса
                        int courseId;
                        using (var cmd = new NpgsqlCommand("SELECT id FROM courses WHERE number = @number", conn))
                        {
                            cmd.Parameters.AddWithValue("number", courseNum);
                            courseId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Создаем 2 семестра для каждого курса
                        for (int semNum = 1; semNum <= 2; semNum++)
                        {
                            using (var cmd = new NpgsqlCommand(@"
                                INSERT INTO semesters (number, course_id) 
                                VALUES (@number, @courseId)
                                ON CONFLICT DO NOTHING", conn))
                            {
                                cmd.Parameters.AddWithValue("number", semNum);
                                cmd.Parameters.AddWithValue("courseId", courseId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                DatabaseManager.Instance.LogAction(userId, "DATA_INIT", "Созданы базовые данные (курсы, семестры)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания базовых данных: {ex.Message}");
                DatabaseManager.Instance.LogAction(userId, "ERROR", $"Ошибка создания базовых данных: {ex.Message}");
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
                RowCount = 5,
                Padding = new Padding(20)
            };

            // Настраиваем строки
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // Заголовок
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));  // Фильтры
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));  // Статус
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

            // === Панель фильтров ===
            CreateFiltersPanel(mainLayout);

            // === Статусная строка ===
            lblStatus = new Label
            {
                Text = "Выберите параметры для загрузки списка студентов",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(127, 140, 141),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 5, 0)
            };
            mainLayout.Controls.Add(lblStatus, 0, 2);

            // === Таблица студентов ===
            CreateStudentsGrid(mainLayout);

            // === Панель кнопок ===
            CreateButtonsPanel(mainLayout);

            // === Добавляем главный контейнер на форму ===
            this.Controls.Add(mainLayout);

            this.ResumeLayout(false);
        }

        private void CreateFiltersPanel(TableLayoutPanel mainLayout)
        {
            TableLayoutPanel filtersPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };
            filtersPanel.BorderStyle = BorderStyle.FixedSingle;

            // Настраиваем колонки
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));

            // Настраиваем строки
            filtersPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F)); // Лейблы
            filtersPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60F)); // Комбобоксы

            // === Лейблы ===
            var labels = new[]
            {
                new Label { Text = "Курс:", Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                           ForeColor = Color.FromArgb(52, 73, 94), Dock = DockStyle.Fill,
                           TextAlign = ContentAlignment.BottomLeft },
                new Label { Text = "Семестр:", Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                           ForeColor = Color.FromArgb(52, 73, 94), Dock = DockStyle.Fill,
                           TextAlign = ContentAlignment.BottomLeft },
                new Label { Text = "Группа:", Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                           ForeColor = Color.FromArgb(52, 73, 94), Dock = DockStyle.Fill,
                           TextAlign = ContentAlignment.BottomLeft },
                new Label { Text = "Дисциплина:", Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                           ForeColor = Color.FromArgb(52, 73, 94), Dock = DockStyle.Fill,
                           TextAlign = ContentAlignment.BottomLeft }
            };

            for (int i = 0; i < labels.Length; i++)
            {
                filtersPanel.Controls.Add(labels[i], i, 0);
            }

            // === Комбобоксы ===
            this.cmbCourse = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 5, 10, 5)
            };
            this.cmbCourse.SelectedIndexChanged += CmbCourse_SelectedIndexChanged;

            this.cmbSemester = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 5, 10, 5)
            };
            this.cmbSemester.SelectedIndexChanged += CmbSemester_SelectedIndexChanged;

            this.cmbGroup = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 5, 10, 5)
            };
            this.cmbGroup.SelectedIndexChanged += CmbGroup_SelectedIndexChanged;

            this.cmbDiscipline = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 5, 10, 5)
            };
            this.cmbDiscipline.SelectedIndexChanged += CmbDiscipline_SelectedIndexChanged;

            btnRefresh = new Button
            {
                Text = "🔄",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(5, 5, 5, 5)
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (sender, e) => LoadStudents();

            filtersPanel.Controls.Add(this.cmbCourse, 0, 1);
            filtersPanel.Controls.Add(this.cmbSemester, 1, 1);
            filtersPanel.Controls.Add(this.cmbGroup, 2, 1);
            filtersPanel.Controls.Add(this.cmbDiscipline, 3, 1);
            filtersPanel.Controls.Add(btnRefresh, 4, 1);

            mainLayout.Controls.Add(filtersPanel, 0, 1);
        }

        private void CreateStudentsGrid(TableLayoutPanel mainLayout)
        {
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

            // Настройка колонок
            InitializeDataGridView();

            // Стилизация заголовков
            this.dgvStudents.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            this.dgvStudents.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            this.dgvStudents.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.dgvStudents.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dgvStudents.ColumnHeadersHeight = 40;

            mainLayout.Controls.Add(this.dgvStudents, 0, 3);
        }

        private void CreateButtonsPanel(TableLayoutPanel mainLayout)
        {
            TableLayoutPanel buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };
            buttonPanel.BorderStyle = BorderStyle.FixedSingle;

            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

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

            var lblInfo = new Label
            {
                Text = "💡 Выберите все параметры фильтрации, затем выставьте оценки студентам в таблице",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(127, 140, 141),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft
            };

            buttonPanel.Controls.Add(this.btnSave, 0, 0);
            buttonPanel.Controls.Add(lblInfo, 1, 0);

            mainLayout.Controls.Add(buttonPanel, 0, 4);
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
                Width = 120
            };

            gradeColumn.Items.AddRange(new object[] { "Не выбрано", "2", "3", "4", "5" });
            dgvStudents.Columns.Add(gradeColumn);

            // Обработчик ошибок
            dgvStudents.DataError += (sender, e) => {
                e.ThrowException = false;
            };
        }

        private void LoadInitialData()
        {
            try
            {
                UpdateStatus("Загрузка данных...");

                LoadCourses();
                LoadSemesters();
                LoadGroups();
                LoadDisciplines();

                UpdateStatus("Данные загружены. Выберите параметры для фильтрации.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки данных: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
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
                    UpdateStatus("Курсы не найдены в базе данных");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки курсов: {ex.Message}");
                Console.WriteLine($"Ошибка загрузки курсов: {ex.Message}");
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
                else
                {
                    UpdateStatus("Семестры не найдены в базе данных");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки семестров: {ex.Message}");
                Console.WriteLine($"Ошибка загрузки семестров: {ex.Message}");
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
                    UpdateStatus("Группы не найдены в базе данных");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки групп: {ex.Message}");
                Console.WriteLine($"Ошибка загрузки групп: {ex.Message}");
            }
        }

        private void LoadDisciplines()
        {
            try
            {
                if (!teacherId.HasValue)
                {
                    UpdateStatus("ID преподавателя не определен");
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
                    UpdateStatus("У преподавателя нет назначенных дисциплин");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки дисциплин: {ex.Message}");
                Console.WriteLine($"Ошибка загрузки дисциплин: {ex.Message}");
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
            if (!(cmbGroup.SelectedItem is Group selectedGroup) ||
                !(cmbDiscipline.SelectedItem is Discipline selectedDiscipline))
            {
                dgvStudents.Rows.Clear();
                UpdateStatus("Выберите группу и дисциплину для загрузки студентов");
                return;
            }

            try
            {
                UpdateStatus("Загрузка списка студентов...");

                var students = studentService.GetStudentsByGroup(selectedGroup.Id);
                dgvStudents.Rows.Clear();

                if (students == null || students.Count == 0)
                {
                    UpdateStatus("В выбранной группе нет студентов");
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

                UpdateStatus($"Загружено студентов: {students.Count}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки студентов: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки студентов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("Выберите группу и дисциплину", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                UpdateStatus("Сохранение оценок...");

                int savedCount = 0;
                int errorCount = 0;
                int processedCount = 0;

                foreach (DataGridViewRow row in dgvStudents.Rows)
                {
                    if (!row.IsNewRow && row.Cells["Grade"].Value != null && row.Cells["Id"].Value != null)
                    {
                        string gradeText = row.Cells["Grade"].Value.ToString();

                        if (gradeText == "Не выбрано")
                        {
                            continue;
                        }

                        if (int.TryParse(gradeText, out int grade) && grade >= 2 && grade <= 5)
                        {
                            processedCount++;
                            int studentId = (int)row.Cells["Id"].Value;
                            int disciplineId = selectedDiscipline.Id;

                            if (gradeService.SaveGrade(studentId, disciplineId, grade))
                            {
                                savedCount++;
                            }
                            else
                            {
                                errorCount++;
                            }
                        }
                    }
                }

                // Показываем результат
                if (savedCount > 0)
                {
                    string message = $"Успешно сохранено оценок: {savedCount}";
                    if (errorCount > 0)
                    {
                        message += $"\nОшибок при сохранении: {errorCount}";
                    }

                    MessageBox.Show(message, "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateStatus($"Сохранено: {savedCount}, ошибок: {errorCount}");
                    LoadStudents(); // Обновляем отображение
                }
                else if (errorCount > 0)
                {
                    MessageBox.Show($"Не удалось сохранить {errorCount} оценок", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatus($"Ошибка сохранения оценок");
                }
                else
                {
                    MessageBox.Show("Не было выставлено ни одной оценки", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateStatus("Нет оценок для сохранения");
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Ошибка сохранения оценок: {ex.Message}";
                UpdateStatus(errorMsg);
                MessageBox.Show(errorMsg, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DatabaseManager.Instance.LogAction(userId, "ERROR", errorMsg);
            }
        }
    }
}