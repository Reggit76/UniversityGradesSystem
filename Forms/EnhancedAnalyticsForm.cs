using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using UniversityGradesSystem.Models;
using UniversityGradesSystem.Services;

namespace UniversityGradesSystem.Forms
{
    public partial class EnhancedAnalyticsForm : Form
    {
        private readonly AnalyticsService analyticsService;
        private readonly GroupService groupService;
        private readonly DisciplineService disciplineService;
        private readonly TeacherService teacherService;
        private readonly int userId;
        private readonly string userRole;
        private int? teacherId;

        // UI элементы
        private Panel headerPanel;
        private ComboBox cmbGroup;
        private ComboBox cmbDiscipline;
        private Button btnRefresh;
        private TabControl tabControl;

        // Вкладки
        private TabPage overviewTab;
        private TabPage detailsTab;
        private TabPage topStudentsTab;

        // Графики
        private Chart performanceChart;
        private Chart gradesDistributionChart;

        // Панели данных
        private Panel statsPanel;
        private DataGridView topStudentsGrid;
        private DataGridView groupDetailsGrid;

        public EnhancedAnalyticsForm(int userId, string role)
        {
            this.userId = userId;
            this.userRole = role;

            try
            {
                string connString = DatabaseManager.Instance.GetConnectionString();
                analyticsService = new AnalyticsService(connString);
                groupService = new GroupService(connString);
                disciplineService = new DisciplineService(connString);
                teacherService = new TeacherService(connString);

                if (role == "teacher")
                {
                    teacherId = teacherService.GetTeacherId(userId);
                }

                InitializeComponent();
                InitializeCharts();
                LoadData();
                LoadAnalytics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации аналитики: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // === Настройки формы ===
            this.Text = "Аналитика успеваемости";
            this.Size = new Size(1200, 800);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 244, 247);

            // === Заголовочная панель ===
            CreateHeaderPanel();

            // === Основной TabControl ===
            CreateTabControl();

            // === Финальная настройка ===
            this.ResumeLayout(false);
        }

        private void CreateHeaderPanel()
        {
            headerPanel = new Panel
            {
                Height = 80,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(52, 73, 94),
                Padding = new Padding(20, 10, 20, 10)
            };

            // Заголовок
            var titleLabel = new Label
            {
                Text = "📊 Аналитика успеваемости",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };

            // Панель фильтров
            var filtersPanel = new Panel
            {
                Height = 40,
                Dock = DockStyle.Bottom,
                BackColor = Color.Transparent
            };

            // Группа
            var lblGroup = new Label
            {
                Text = "Группа:",
                Location = new Point(20, 10),
                Size = new Size(60, 20),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F)
            };

            cmbGroup = new ComboBox
            {
                Location = new Point(90, 8),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F)
            };
            cmbGroup.SelectedIndexChanged += CmbGroup_SelectedIndexChanged;

            // Дисциплина (для преподавателей)
            var lblDiscipline = new Label
            {
                Text = "Дисциплина:",
                Location = new Point(260, 10),
                Size = new Size(80, 20),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Visible = userRole == "teacher"
            };

            cmbDiscipline = new ComboBox
            {
                Location = new Point(350, 8),
                Size = new Size(180, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Visible = userRole == "teacher"
            };
            cmbDiscipline.SelectedIndexChanged += CmbDiscipline_SelectedIndexChanged;

            // Кнопка обновления
            btnRefresh = new Button
            {
                Text = "🔄 Обновить",
                Location = new Point(userRole == "teacher" ? 550 : 260, 8),
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += BtnRefresh_Click;

            filtersPanel.Controls.AddRange(new Control[] {
                lblGroup, cmbGroup, lblDiscipline, cmbDiscipline, btnRefresh
            });

            headerPanel.Controls.AddRange(new Control[] { titleLabel, filtersPanel });
            this.Controls.Add(headerPanel);
        }

        private void CreateTabControl()
        {
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Appearance = TabAppearance.FlatButtons,
                Padding = new Point(20, 5)
            };

            // === Вкладка "Обзор" ===
            overviewTab = new TabPage("📈 Обзор")
            {
                BackColor = Color.FromArgb(240, 244, 247),
                Padding = new Padding(15)
            };
            CreateOverviewTab();

            // === Вкладка "Детали" ===
            detailsTab = new TabPage("📋 Детали")
            {
                BackColor = Color.FromArgb(240, 244, 247),
                Padding = new Padding(15)
            };
            CreateDetailsTab();

            // === Вкладка "Топ студенты" ===
            topStudentsTab = new TabPage("🏆 Лучшие студенты")
            {
                BackColor = Color.FromArgb(240, 244, 247),
                Padding = new Padding(15)
            };
            CreateTopStudentsTab();

            tabControl.TabPages.AddRange(new TabPage[] { overviewTab, detailsTab, topStudentsTab });
            this.Controls.Add(tabControl);
        }

        private void CreateOverviewTab()
        {
            // Панель статистики сверху
            statsPanel = new Panel
            {
                Height = 120,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 0, 15)
            };

            // Панель графиков
            var chartsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // График успеваемости (круговой)
            performanceChart = new Chart
            {
                Width = 400,
                Height = 350,
                Location = new Point(15, 15),
                BackColor = Color.White,
                BorderlineColor = Color.LightGray,
                BorderlineWidth = 1
            };

            // График распределения оценок (столбчатый)
            gradesDistributionChart = new Chart
            {
                Width = 400,
                Height = 350,
                Location = new Point(430, 15),
                BackColor = Color.White,
                BorderlineColor = Color.LightGray,
                BorderlineWidth = 1
            };

            chartsPanel.Controls.AddRange(new Control[] { performanceChart, gradesDistributionChart });
            overviewTab.Controls.AddRange(new Control[] { statsPanel, chartsPanel });
        }

        private void CreateDetailsTab()
        {
            groupDetailsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Font = new Font("Segoe UI", 9F),
                GridColor = Color.FromArgb(224, 224, 224),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // Стилизация заголовков
            groupDetailsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            groupDetailsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            groupDetailsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            detailsTab.Controls.Add(groupDetailsGrid);
        }

        private void CreateTopStudentsTab()
        {
            topStudentsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Font = new Font("Segoe UI", 9F),
                GridColor = Color.FromArgb(224, 224, 224),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // Колонки для топ студентов
            topStudentsGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Rank", HeaderText = "№", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "StudentName", HeaderText = "ФИО студента", Width = 300 },
                new DataGridViewTextBoxColumn { Name = "AverageGrade", HeaderText = "Средний балл", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "TotalGrades", HeaderText = "Всего оценок", Width = 120 }
            });

            // Стилизация
            topStudentsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            topStudentsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            topStudentsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            topStudentsTab.Controls.Add(topStudentsGrid);
        }

        private void InitializeCharts()
        {
            // === График успеваемости (круговой) ===
            performanceChart.ChartAreas.Add(new ChartArea("Performance"));
            performanceChart.Series.Add(new Series("Performance")
            {
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = true,
                LabelFormat = "{0:F1}%"
            });
            performanceChart.Titles.Add(new Title("Распределение по успеваемости"));

            // === График оценок (столбчатый) ===
            gradesDistributionChart.ChartAreas.Add(new ChartArea("Grades"));
            gradesDistributionChart.Series.Add(new Series("Grades")
            {
                ChartType = SeriesChartType.Column,
                IsValueShownAsLabel = true
            });
            gradesDistributionChart.Titles.Add(new Title("Распределение оценок"));
            gradesDistributionChart.ChartAreas["Grades"].AxisX.Title = "Оценки";
            gradesDistributionChart.ChartAreas["Grades"].AxisY.Title = "Количество";
        }

        private void LoadData()
        {
            try
            {
                // Загрузка групп
                var groups = groupService.GetAllGroups();
                if (groups.Any())
                {
                    cmbGroup.DisplayMember = "Name";
                    cmbGroup.ValueMember = "Id";
                    cmbGroup.DataSource = new List<Group> { new Group { Id = -1, Name = "Все группы" } }
                        .Concat(groups).ToList();
                    cmbGroup.SelectedIndex = 0;
                }

                // Загрузка дисциплин для преподавателей
                if (userRole == "teacher" && teacherId.HasValue)
                {
                    var disciplines = disciplineService.GetTeacherDisciplines(teacherId.Value);
                    if (disciplines.Any())
                    {
                        cmbDiscipline.DisplayMember = "Name";
                        cmbDiscipline.ValueMember = "Id";
                        cmbDiscipline.DataSource = disciplines;
                        cmbDiscipline.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadAnalytics()
        {
            try
            {
                if (cmbGroup.SelectedItem is Group selectedGroup)
                {
                    if (selectedGroup.Id == -1)
                    {
                        LoadOverallAnalytics();
                    }
                    else
                    {
                        LoadGroupAnalytics(selectedGroup.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки аналитики: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadGroupAnalytics(int groupId)
        {
            try
            {
                // Сначала пробуем расширенную аналитику
                var analytics = analyticsService.GetGroupAnalytics(groupId);

                // Если данных нет, пробуем простую аналитику
                if (analytics.TotalStudents == 0)
                {
                    analytics = analyticsService.GetSimpleGroupAnalytics(groupId);
                }

                // Обновляем статистические панели
                UpdateStatsPanel(analytics);

                // Обновляем графики
                UpdatePerformanceChart(analytics);
                UpdateGradesChart(analytics);

                // Обновляем топ студентов
                LoadTopStudents(groupId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки аналитики группы: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Console.WriteLine($"Детали ошибки LoadGroupAnalytics: {ex}");
            }
        }

        private void LoadOverallAnalytics()
        {
            try
            {
                var groupsSummary = analyticsService.GetAllGroupsSummary();

                // Если нет данных из расширенной функции, получаем простую аналитику
                if (!groupsSummary.Any())
                {
                    // Получаем список групп и собираем аналитику для каждой
                    var groups = groupService.GetAllGroups();
                    var totalAnalytics = new GroupAnalytics();

                    foreach (var group in groups)
                    {
                        var groupAnalytics = analyticsService.GetSimpleGroupAnalytics(group.Id);
                        totalAnalytics.TotalStudents += groupAnalytics.TotalStudents;
                        totalAnalytics.ExcellentCount += groupAnalytics.ExcellentCount;
                        totalAnalytics.FailingCount += groupAnalytics.FailingCount;
                        totalAnalytics.GoodCount += groupAnalytics.GoodCount;
                        totalAnalytics.SatisfactoryCount += groupAnalytics.SatisfactoryCount;
                    }

                    // Пересчитываем проценты
                    if (totalAnalytics.TotalStudents > 0)
                    {
                        totalAnalytics.ExcellentPercentage = (decimal)totalAnalytics.ExcellentCount * 100 / totalAnalytics.TotalStudents;
                        totalAnalytics.FailingPercentage = (decimal)totalAnalytics.FailingCount * 100 / totalAnalytics.TotalStudents;
                        totalAnalytics.GoodPercentage = (decimal)totalAnalytics.GoodCount * 100 / totalAnalytics.TotalStudents;
                        totalAnalytics.SatisfactoryPercentage = (decimal)totalAnalytics.SatisfactoryCount * 100 / totalAnalytics.TotalStudents;
                    }

                    UpdateStatsPanel(totalAnalytics);
                    UpdatePerformanceChart(totalAnalytics);
                    UpdateGradesChart(totalAnalytics);

                    // Создаем простую таблицу групп
                    var simpleSummary = groups.Select(g => new GroupSummary
                    {
                        GroupName = g.Name,
                        GroupId = g.Id,
                        SpecialtyName = "Неизвестно",
                        CourseNumber = 1,
                        TotalStudents = 0,
                        AverageGrade = 0,
                        ExcellentPercentage = 0
                    }).ToList();

                    UpdateGroupDetailsGrid(simpleSummary);
                }
                else
                {
                    // Для общей аналитики используем суммарные данные
                    var totalAnalytics = CalculateOverallAnalytics(groupsSummary);

                    UpdateStatsPanel(totalAnalytics);
                    UpdatePerformanceChart(totalAnalytics);
                    UpdateGradesChart(totalAnalytics);

                    // Обновляем таблицу групп
                    UpdateGroupDetailsGrid(groupsSummary);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки общей аналитики: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Console.WriteLine($"Детали ошибки LoadOverallAnalytics: {ex}");

                // В случае полной ошибки показываем базовые данные
                var basicAnalytics = new GroupAnalytics
                {
                    TotalStudents = 0,
                    ExcellentCount = 0,
                    ExcellentPercentage = 0,
                    GoodCount = 0,
                    GoodPercentage = 0,
                    SatisfactoryCount = 0,
                    SatisfactoryPercentage = 0,
                    FailingCount = 0,
                    FailingPercentage = 0,
                    AverageGrade = 0
                };

                UpdateStatsPanel(basicAnalytics);
                UpdatePerformanceChart(basicAnalytics);
                UpdateGradesChart(basicAnalytics);
            }
        }

        private GroupAnalytics CalculateOverallAnalytics(List<GroupSummary> groupsSummary)
        {
            if (!groupsSummary.Any()) return new GroupAnalytics();

            return new GroupAnalytics
            {
                TotalStudents = groupsSummary.Sum(g => g.TotalStudents),
                AverageGrade = groupsSummary.Where(g => g.AverageGrade > 0).Average(g => g.AverageGrade),
                ExcellentPercentage = groupsSummary.Where(g => g.TotalStudents > 0)
                    .Average(g => g.ExcellentPercentage)
                // Для общей статистики используем средние значения
            };
        }

        private void UpdateStatsPanel(GroupAnalytics analytics)
        {
            statsPanel.Controls.Clear();

            var cards = new[]
            {
                CreateStatCard("👥 Всего студентов", analytics.TotalStudents.ToString(), Color.FromArgb(52, 152, 219)),
                CreateStatCard("⭐ Отличники", $"{analytics.ExcellentCount} ({analytics.ExcellentPercentage:F1}%)", Color.FromArgb(46, 204, 113)),
                CreateStatCard("📊 Средний балл", analytics.AverageGrade.ToString("F2"), Color.FromArgb(155, 89, 182)),
                CreateStatCard("⚠️ Неуспевающие", $"{analytics.FailingCount} ({analytics.FailingPercentage:F1}%)", Color.FromArgb(231, 76, 60))
            };

            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].Location = new Point(i * 200 + 15, 15);
                statsPanel.Controls.Add(cards[i]);
            }
        }

        private Panel CreateStatCard(string title, string value, Color color)
        {
            var card = new Panel
            {
                Size = new Size(180, 90),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var colorBar = new Panel
            {
                Size = new Size(4, 90),
                BackColor = color,
                Dock = DockStyle.Left
            };

            var titleLabel = new Label
            {
                Text = title,
                Location = new Point(15, 15),
                Size = new Size(160, 20),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(127, 140, 141)
            };

            var valueLabel = new Label
            {
                Text = value,
                Location = new Point(15, 40),
                Size = new Size(160, 30),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = color
            };

            card.Controls.AddRange(new Control[] { colorBar, titleLabel, valueLabel });
            return card;
        }

        private void UpdatePerformanceChart(GroupAnalytics analytics)
        {
            performanceChart.Series["Performance"].Points.Clear();

            if (analytics.TotalStudents > 0)
            {
                performanceChart.Series["Performance"].Points.AddXY("Отличники", analytics.ExcellentPercentage);
                performanceChart.Series["Performance"].Points.AddXY("Хорошисты", analytics.GoodPercentage);
                performanceChart.Series["Performance"].Points.AddXY("Удовлетв.", analytics.SatisfactoryPercentage);
                performanceChart.Series["Performance"].Points.AddXY("Неуспев.", analytics.FailingPercentage);

                // Цвета
                performanceChart.Series["Performance"].Points[0].Color = Color.FromArgb(46, 204, 113);
                performanceChart.Series["Performance"].Points[1].Color = Color.FromArgb(52, 152, 219);
                performanceChart.Series["Performance"].Points[2].Color = Color.FromArgb(241, 196, 15);
                performanceChart.Series["Performance"].Points[3].Color = Color.FromArgb(231, 76, 60);
            }
        }

        private void UpdateGradesChart(GroupAnalytics analytics)
        {
            gradesDistributionChart.Series["Grades"].Points.Clear();

            gradesDistributionChart.Series["Grades"].Points.AddXY("5", analytics.ExcellentCount);
            gradesDistributionChart.Series["Grades"].Points.AddXY("4", analytics.GoodCount);
            gradesDistributionChart.Series["Grades"].Points.AddXY("3", analytics.SatisfactoryCount);
            gradesDistributionChart.Series["Grades"].Points.AddXY("2", analytics.FailingCount);

            // Цвета столбцов
            gradesDistributionChart.Series["Grades"].Points[0].Color = Color.FromArgb(46, 204, 113);
            gradesDistributionChart.Series["Grades"].Points[1].Color = Color.FromArgb(52, 152, 219);
            gradesDistributionChart.Series["Grades"].Points[2].Color = Color.FromArgb(241, 196, 15);
            gradesDistributionChart.Series["Grades"].Points[3].Color = Color.FromArgb(231, 76, 60);
        }

        private void LoadTopStudents(int groupId)
        {
            try
            {
                var topStudents = analyticsService.GetTopStudentsByGroup(groupId, 10);

                topStudentsGrid.Rows.Clear();

                if (topStudents == null || !topStudents.Any())
                {
                    // Если нет данных, показываем сообщение
                    var rowIndex = topStudentsGrid.Rows.Add();
                    var row = topStudentsGrid.Rows[rowIndex];
                    row.Cells["Rank"].Value = "-";
                    row.Cells["StudentName"].Value = "Нет данных о студентах";
                    row.Cells["AverageGrade"].Value = "-";
                    row.Cells["TotalGrades"].Value = "-";
                    row.DefaultCellStyle.ForeColor = Color.Gray;
                    return;
                }

                for (int i = 0; i < topStudents.Count; i++)
                {
                    var student = topStudents[i];
                    var rowIndex = topStudentsGrid.Rows.Add();
                    var row = topStudentsGrid.Rows[rowIndex];

                    row.Cells["Rank"].Value = i + 1;
                    row.Cells["StudentName"].Value = student.StudentName ?? "Неизвестно";
                    row.Cells["AverageGrade"].Value = student.AverageGrade.ToString("F2");
                    row.Cells["TotalGrades"].Value = student.TotalGrades;

                    // Выделяем топ-3 цветом
                    if (i < 3)
                    {
                        var colors = new[] { Color.Gold, Color.Silver, Color.FromArgb(205, 127, 50) };
                        row.DefaultCellStyle.BackColor = Color.FromArgb(20, colors[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки топ студентов для группы {groupId}: {ex.Message}");

                // В случае ошибки показываем сообщение об ошибке
                topStudentsGrid.Rows.Clear();
                var rowIndex = topStudentsGrid.Rows.Add();
                var row = topStudentsGrid.Rows[rowIndex];
                row.Cells["Rank"].Value = "!";
                row.Cells["StudentName"].Value = "Ошибка загрузки данных";
                row.Cells["AverageGrade"].Value = "-";
                row.Cells["TotalGrades"].Value = "-";
                row.DefaultCellStyle.ForeColor = Color.Red;
            }
        }

        private void UpdateGroupDetailsGrid(List<GroupSummary> groupsSummary)
        {
            groupDetailsGrid.Columns.Clear();
            groupDetailsGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "GroupName", HeaderText = "Группа", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "SpecialtyName", HeaderText = "Специальность", Width = 250 },
                new DataGridViewTextBoxColumn { Name = "CourseNumber", HeaderText = "Курс", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "TotalStudents", HeaderText = "Студентов", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "AverageGrade", HeaderText = "Ср. балл", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "ExcellentPercentage", HeaderText = "% отличников", Width = 100 }
            });

            groupDetailsGrid.Rows.Clear();
            foreach (var group in groupsSummary.OrderBy(g => g.CourseNumber).ThenBy(g => g.GroupName))
            {
                groupDetailsGrid.Rows.Add(
                    group.GroupName,
                    group.SpecialtyName,
                    group.CourseNumber,
                    group.TotalStudents,
                    group.AverageGrade.ToString("F2"),
                    group.ExcellentPercentage.ToString("F1") + "%"
                );
            }
        }

        // Обработчики событий
        private void CmbGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadAnalytics();
        }

        private void CmbDiscipline_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadAnalytics();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadAnalytics();
        }
    }
}