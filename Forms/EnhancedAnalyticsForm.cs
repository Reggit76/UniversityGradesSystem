// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
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
        private TableLayoutPanel mainLayout;
        private Panel headerPanel;
        private ComboBox cmbGroup;
        private ComboBox cmbDiscipline;
        private Button btnRefresh;
        private TabControl tabControl;

        // Вкладки
        private TabPage overviewTab;
        private TabPage detailsTab;

        // Графики
        private Chart performanceChart;
        private Chart gradesDistributionChart;

        // Панели данных
        private Panel statsPanel;
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
            this.BackColor = Color.FromArgb(240, 244, 247);
            this.MinimumSize = new Size(1000, 600);

            // === Главный контейнер ===
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F)); // Заголовок
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Содержимое

            // === Заголовочная панель ===
            CreateHeaderPanel();

            // === Основной TabControl ===
            CreateTabControl();

            // Добавляем элементы в главный контейнер
            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(tabControl, 0, 1);

            // Добавляем главный контейнер на форму
            this.Controls.Add(mainLayout);

            this.ResumeLayout(false);
        }

        private void CreateHeaderPanel()
        {
            headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(52, 73, 94),
                Padding = new Padding(20, 10, 20, 10)
            };

            // Используем TableLayoutPanel для заголовка
            TableLayoutPanel headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            // Заголовок
            var titleLabel = new Label
            {
                Text = "📊 Аналитика успеваемости",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Панель фильтров
            var filtersPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            // Настраиваем колонки фильтров
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Лейбл группы
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F)); // Комбобокс группы
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Лейбл дисциплины
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F)); // Комбобокс дисциплины
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F)); // Кнопка обновления
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Остальное пространство

            // Группа
            var lblGroup = new Label
            {
                Text = "Группа:",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 10, 0)
            };

            cmbGroup = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 5, 15, 5)
            };
            cmbGroup.SelectedIndexChanged += CmbGroup_SelectedIndexChanged;

            // Дисциплина (для преподавателей)
            var lblDiscipline = new Label
            {
                Text = "Дисциплина:",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft,
                Visible = userRole == "teacher",
                Margin = new Padding(0, 0, 10, 0)
            };

            cmbDiscipline = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Visible = userRole == "teacher",
                Margin = new Padding(0, 5, 15, 5)
            };
            cmbDiscipline.SelectedIndexChanged += CmbDiscipline_SelectedIndexChanged;

            // Кнопка обновления
            btnRefresh = new Button
            {
                Text = "🔄 Обновить",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 5, 0, 5)
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += BtnRefresh_Click;

            // Добавляем элементы фильтров
            filtersPanel.Controls.Add(lblGroup, 0, 0);
            filtersPanel.Controls.Add(cmbGroup, 1, 0);
            filtersPanel.Controls.Add(lblDiscipline, 2, 0);
            filtersPanel.Controls.Add(cmbDiscipline, 3, 0);
            filtersPanel.Controls.Add(btnRefresh, 4, 0);

            // Добавляем в заголовок
            headerLayout.Controls.Add(titleLabel, 0, 0);
            headerLayout.Controls.Add(filtersPanel, 0, 1);

            headerPanel.Controls.Add(headerLayout);
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
                UseVisualStyleBackColor = true
            };
            CreateOverviewTab();

            // === Вкладка "Детали" ===
            detailsTab = new TabPage("📋 Детали")
            {
                BackColor = Color.FromArgb(240, 244, 247),
                UseVisualStyleBackColor = true
            };
            CreateDetailsTab();

            tabControl.TabPages.AddRange(new TabPage[] { overviewTab, detailsTab });
        }

        private void CreateOverviewTab()
        {
            // Используем TableLayoutPanel для обзорной вкладки
            TableLayoutPanel overviewLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(15)
            };
            overviewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F)); // Статистика
            overviewLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Графики

            // Панель статистики
            statsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // Панель графиков
            TableLayoutPanel chartsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 15, 0, 0)
            };
            chartsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            chartsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // График успеваемости (круговой)
            performanceChart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderlineColor = Color.LightGray,
                BorderlineWidth = 1,
                Margin = new Padding(0, 0, 7, 0)
            };

            // График распределения оценок (столбчатый)
            gradesDistributionChart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderlineColor = Color.LightGray,
                BorderlineWidth = 1,
                Margin = new Padding(8, 0, 0, 0)
            };

            chartsLayout.Controls.Add(performanceChart, 0, 0);
            chartsLayout.Controls.Add(gradesDistributionChart, 1, 0);

            overviewLayout.Controls.Add(statsPanel, 0, 0);
            overviewLayout.Controls.Add(chartsLayout, 0, 1);

            overviewTab.Controls.Add(overviewLayout);
        }

        private void CreateDetailsTab()
        {
            TableLayoutPanel detailsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 1,
                Padding = new Padding(15)
            };

            groupDetailsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Font = new Font("Segoe UI", 9F),
                GridColor = Color.FromArgb(224, 224, 224),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };

            // Стилизация заголовков
            groupDetailsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            groupDetailsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            groupDetailsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            groupDetailsGrid.ColumnHeadersHeight = 35;

            detailsLayout.Controls.Add(groupDetailsGrid, 0, 0);
            detailsTab.Controls.Add(detailsLayout);
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
                // Загрузка групп - ИСПРАВЛЕННАЯ ЛОГИКА
                List<Group> groups;

                if (userRole == "teacher" && teacherId.HasValue)
                {
                    // Для преподавателей загружаем только связанные группы
                    groups = groupService.GetGroupsByTeacher(teacherId.Value);

                    if (!groups.Any())
                    {
                        MessageBox.Show("У вас нет назначенных дисциплин для групп.", "Информация",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Добавляем пустую группу для отображения
                        groups.Add(new Group { Id = -2, Name = "Нет доступных групп" });
                    }
                }
                else
                {
                    // Для администраторов загружаем все группы
                    groups = groupService.GetAllGroups();
                }

                if (groups.Any())
                {
                    cmbGroup.DisplayMember = "Name";
                    cmbGroup.ValueMember = "Id";

                    var groupList = new List<Group>();

                    // Добавляем "Все группы" только если есть реальные группы
                    if (groups.Any(g => g.Id > 0))
                    {
                        groupList.Add(new Group { Id = -1, Name = "Все группы" });
                    }

                    groupList.AddRange(groups);

                    cmbGroup.DataSource = groupList;
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
                    else if (selectedGroup.Id == -2)
                    {
                        // Нет доступных групп - показываем пустую аналитику
                        var emptyAnalytics = new GroupAnalytics();
                        UpdateStatsPanel(emptyAnalytics);
                        UpdatePerformanceChart(emptyAnalytics);
                        UpdateGradesChart(emptyAnalytics);
                        UpdateGroupDetailsGrid(new List<GroupSummary>());
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
                // ДОПОЛНИТЕЛЬНАЯ ПРОВЕРКА для преподавателей
                if (userRole == "teacher" && teacherId.HasValue)
                {
                    if (!groupService.IsGroupRelatedToTeacher(groupId, teacherId.Value))
                    {
                        MessageBox.Show("У вас нет прав для просмотра аналитики этой группы.", "Доступ запрещен",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

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

                // Обновляем детали только для конкретной группы
                if (groupId > 0)
                {
                    UpdateGroupDetailsForSingleGroup(groupId);
                }
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
                    List<Group> groups;
                    if (userRole == "teacher" && teacherId.HasValue)
                    {
                        groups = groupService.GetGroupsByTeacher(teacherId.Value);
                    }
                    else
                    {
                        groups = groupService.GetAllGroups();
                    }

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
                    // Фильтруем группы для преподавателей
                    if (userRole == "teacher" && teacherId.HasValue)
                    {
                        var teacherGroups = groupService.GetGroupsByTeacher(teacherId.Value);
                        var teacherGroupIds = teacherGroups.Select(g => g.Id).ToHashSet();
                        groupsSummary = groupsSummary.Where(gs => teacherGroupIds.Contains(gs.GroupId)).ToList();
                    }

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
                AverageGrade = groupsSummary.Where(g => g.AverageGrade > 0).Any() ?
                    groupsSummary.Where(g => g.AverageGrade > 0).Average(g => g.AverageGrade) : 0,
                ExcellentPercentage = groupsSummary.Where(g => g.TotalStudents > 0).Any() ?
                    groupsSummary.Where(g => g.TotalStudents > 0).Average(g => g.ExcellentPercentage) : 0
                // Для общей статистики используем средние значения
            };
        }

        private void UpdateStatsPanel(GroupAnalytics analytics)
        {
            statsPanel.Controls.Clear();

            // Используем FlowLayoutPanel для автоматического размещения карточек
            FlowLayoutPanel cardFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0)
            };

            var cards = new[]
            {
                CreateStatCard("👥 Всего студентов", analytics.TotalStudents.ToString(), Color.FromArgb(52, 152, 219)),
                CreateStatCard("⭐ Отличники", $"{analytics.ExcellentCount} ({analytics.ExcellentPercentage:F1}%)", Color.FromArgb(46, 204, 113)),
                CreateStatCard("📊 Средний балл", analytics.AverageGrade.ToString("F2"), Color.FromArgb(155, 89, 182)),
                CreateStatCard("⚠️ Неуспевающие", $"{analytics.FailingCount} ({analytics.FailingPercentage:F1}%)", Color.FromArgb(231, 76, 60))
            };

            foreach (var card in cards)
            {
                cardFlow.Controls.Add(card);
            }

            statsPanel.Controls.Add(cardFlow);
        }

        private Panel CreateStatCard(string title, string value, Color color)
        {
            var card = new Panel
            {
                Size = new Size(200, 100),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5)
            };

            var colorBar = new Panel
            {
                Size = new Size(4, 100),
                BackColor = color,
                Dock = DockStyle.Left
            };

            var titleLabel = new Label
            {
                Text = title,
                Location = new Point(15, 15),
                Size = new Size(180, 20),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(127, 140, 141)
            };

            var valueLabel = new Label
            {
                Text = value,
                Location = new Point(15, 40),
                Size = new Size(180, 40),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
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

        private void UpdateGroupDetailsForSingleGroup(int groupId)
        {
            try
            {
                // Получаем информацию о конкретной группе
                var allGroups = groupService.GetAllGroups();
                var group = allGroups.FirstOrDefault(g => g.Id == groupId);

                if (group != null)
                {
                    var analytics = analyticsService.GetGroupAnalytics(groupId);
                    if (analytics.TotalStudents == 0)
                    {
                        analytics = analyticsService.GetSimpleGroupAnalytics(groupId);
                    }

                    var singleGroupSummary = new List<GroupSummary>
                    {
                        new GroupSummary
                        {
                            GroupName = group.Name,
                            GroupId = group.Id,
                            SpecialtyName = "Не определена",
                            CourseNumber = 1,
                            TotalStudents = analytics.TotalStudents,
                            AverageGrade = analytics.AverageGrade,
                            ExcellentPercentage = analytics.ExcellentPercentage
                        }
                    };

                    UpdateGroupDetailsGrid(singleGroupSummary);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления деталей для группы {groupId}: {ex.Message}");
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
            LoadData();
            LoadAnalytics();
        }
    }
}