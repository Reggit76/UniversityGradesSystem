using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UniversityGradesSystem.Models;
using UniversityGradesSystem.Services;
using MaterialSkin;
using MaterialSkin.Controls;
using System.Drawing;

namespace UniversityGradesSystem.Forms
{
    public partial class GradeEntryForm : MaterialForm
    {
        private readonly GradeService gradeService;
        private readonly DisciplineService disciplineService;
        private readonly StudentService studentService;
        private readonly TeacherService teacherService;
        private readonly GroupService groupService;
        private readonly int userId;
        private int? teacherId;

        // Используем Material-компоненты
        private MaterialComboBox cmbGroup;
        private MaterialComboBox cmbDiscipline;
        private DataGridView dgvStudents;
        private MaterialButton btnSave;

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
            MessageBox.Show("Преподаватель не найден. Обратитесь к администратору.");
            // НЕ закрываем форму, просто показываем сообщение
            return;
        }

        InitializeComponent(); // Вызов метода инициализации из Designer.cs
        ApplyMaterialSkin();
        InitializeDataGridView(); // Инициализируем DataGridView после создания компонентов
        
        // Загружаем данные после инициализации компонентов
        LoadGroups();
        LoadDisciplines();
        
        DatabaseManager.Instance.LogAction(userId, "FORM_OPENED", "Открыта форма выставления оценок");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Ошибка инициализации формы выставления оценок: {ex.Message}");
        DatabaseManager.Instance.LogAction(userId, "ERROR", $"Ошибка инициализации GradeEntryForm: {ex.Message}");
        
        // Даже при ошибке пытаемся создать минимальный интерфейс
        try
        {
            InitializeComponent();
        }
        catch
        {
            // Если даже базовая инициализация не работает, создаем простую форму
            this.Size = new System.Drawing.Size(800, 600);
            this.Text = "Выставление оценок (ошибка загрузки)";
            var errorLabel = new Label 
            { 
                Text = "Ошибка загрузки данных. Проверьте подключение к базе данных.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(errorLabel);
        }
    }
}

        private void ApplyMaterialSkin()
        {
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new MaterialSkin.ColorScheme(
                MaterialSkin.Primary.BlueGrey800,
                MaterialSkin.Primary.BlueGrey900,
                MaterialSkin.Primary.BlueGrey500,
                MaterialSkin.Accent.LightBlue200,
                MaterialSkin.TextShade.WHITE);
        }

        private void InitializeDataGridView()
        {
            dgvStudents.Columns.Clear();

            dgvStudents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id",
                ReadOnly = true,
                Visible = false
            });

            dgvStudents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FullName",
                HeaderText = "ФИО студента",
                Width = 400,
                ReadOnly = true
            });

            var gradeColumn = new DataGridViewComboBoxColumn
            {
                Name = "Grade",
                HeaderText = "Оценка",
                DataSource = new List<int?> { null, 2, 3, 4, 5 },
                ValueType = typeof(int?),
                Width = 150,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
            };
            dgvStudents.Columns.Add(gradeColumn);
        }

        private void LoadGroups()
        {
            try
            {
                var groups = groupService.GetAllGroups();
                Console.WriteLine($"Загружено групп: {groups?.Count ?? 0}");

                if (groups != null && groups.Count > 0)
                {
                    cmbGroup.DataSource = groups;
                    cmbGroup.DisplayMember = "Name";
                    cmbGroup.ValueMember = "Id";
                    cmbGroup.SelectedIndex = -1; // Снимаем выделение

                    Console.WriteLine("Группы успешно загружены в ComboBox");
                }
                else
                {
                    MessageBox.Show("Не найдено ни одной группы в базе данных");
                    Console.WriteLine("Группы не найдены");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки групп: {ex.Message}");
                Console.WriteLine($"Ошибка загрузки групп: {ex.Message}");
                DatabaseManager.Instance.LogAction(userId, "ERROR", $"Ошибка загрузки групп: {ex.Message}");
            }
        }

        private void LoadDisciplines()
        {
            try
            {
                if (!teacherId.HasValue)
                {
                    MessageBox.Show("ID преподавателя не определен");
                    return;
                }

                var disciplines = disciplineService.GetTeacherDisciplines(teacherId.Value);
                Console.WriteLine($"Загружено дисциплин для преподавателя {teacherId.Value}: {disciplines?.Count ?? 0}");

                if (disciplines != null && disciplines.Count > 0)
                {
                    cmbDiscipline.DataSource = disciplines;
                    cmbDiscipline.DisplayMember = "Name";
                    cmbDiscipline.ValueMember = "Id";
                    cmbDiscipline.SelectedIndex = -1; // Снимаем выделение

                    Console.WriteLine("Дисциплины успешно загружены в ComboBox");

                    // Выводим список дисциплин для отладки
                    foreach (var disc in disciplines)
                    {
                        Console.WriteLine($"Дисциплина: ID={disc.Id}, Name={disc.Name}");
                    }
                }
                else
                {
                    MessageBox.Show($"У преподавателя с ID {teacherId.Value} нет назначенных дисциплин");
                    Console.WriteLine($"Дисциплины не найдены для преподавателя {teacherId.Value}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки дисциплин: {ex.Message}");
                Console.WriteLine($"Ошибка загрузки дисциплин: {ex.Message}");
                DatabaseManager.Instance.LogAction(userId, "ERROR", $"Ошибка загрузки дисциплин: {ex.Message}");
            }
        }

        private void LoadStudents()
        {
            if (cmbGroup.SelectedItem is Group selectedGroup)
            {
                try
                {
                    var students = studentService.GetStudentsByGroup(selectedGroup.Id);

                    // Очищаем DataGridView
                    dgvStudents.Rows.Clear();

                    // Добавляем студентов
                    foreach (var student in students)
                    {
                        int rowIndex = dgvStudents.Rows.Add();
                        var row = dgvStudents.Rows[rowIndex];

                        row.Cells["Id"].Value = student.Id;
                        row.Cells["FullName"].Value = $"{student.LastName} {student.FirstName} {student.MiddleName}";

                        // Загружаем существующую оценку, если есть
                        if (cmbDiscipline.SelectedItem is Discipline selectedDiscipline)
                        {
                            var existingGrade = gradeService.GetStudentGrade(student.Id, selectedDiscipline.Id);
                            row.Cells["Grade"].Value = existingGrade;
                        }
                        else
                        {
                            row.Cells["Grade"].Value = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки студентов: {ex.Message}");
                }
            }
        }

        private void CmbGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private void CmbDiscipline_SelectedIndexChanged(object sender, EventArgs e)
        {
            // При смене дисциплины перезагружаем студентов с их оценками
            LoadStudents();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbDiscipline.SelectedItem is Discipline selectedDiscipline && cmbGroup.SelectedItem is Group selectedGroup)
                {
                    int savedCount = 0;
                    int errorCount = 0;

                    foreach (DataGridViewRow row in dgvStudents.Rows)
                    {
                        if (!row.IsNewRow && row.Cells["Grade"].Value != null && row.Cells["Id"].Value != null)
                        {
                            int studentId = (int)row.Cells["Id"].Value;
                            int grade = (int)row.Cells["Grade"].Value;
                            int disciplineId = selectedDiscipline.Id;

                            // Используем корректный метод сервиса
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

                    if (savedCount > 0)
                    {
                        string message = $"Успешно сохранено оценок: {savedCount}";
                        if (errorCount > 0)
                        {
                            message += $"\nОшибок при сохранении: {errorCount}";
                        }
                        MessageBox.Show(message);

                        // Обновляем отображение после сохранения
                        LoadStudents();
                    }
                    else if (errorCount > 0)
                    {
                        MessageBox.Show($"Все оценки ({errorCount}) не удалось сохранить. Проверьте логи.");
                    }
                    else
                    {
                        MessageBox.Show("Не было выставлено ни одной оценки");
                    }
                }
                else
                {
                    MessageBox.Show("Выберите группу и дисциплину");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения оценок: {ex.Message}");
            }
        }
    }
}