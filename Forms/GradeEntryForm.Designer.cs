using System.Windows.Forms;
using System.Drawing;

namespace UniversityGradesSystem.Forms
{
    partial class GradeEntryForm : Form
    {
        private void InitializeComponent()
        {
            this.SuspendLayout();

            // === Настройки формы ===
            this.Text = "Выставление оценок";
            this.BackColor = Color.WhiteSmoke;
            this.MinimumSize = new Size(900, 600);

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
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // Фильтры
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
            TableLayoutPanel filtersPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };
            filtersPanel.BorderStyle = BorderStyle.FixedSingle;

            // Настраиваем колонки для фильтров
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Лейбл группы
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F)); // Комбобокс группы
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Лейбл дисциплины
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); // Комбобокс дисциплины
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // Кнопка обновления
            filtersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F)); // Пустое место

            // === Лейбл группы ===
            var lblGroup = new Label
            {
                Text = "Группа:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 10, 0)
            };

            // === Комбобокс групп ===
            this.cmbGroup = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 5, 15, 5)
            };
            this.cmbGroup.SelectedIndexChanged += CmbGroup_SelectedIndexChanged;

            // === Лейбл дисциплины ===
            var lblDiscipline = new Label
            {
                Text = "Дисциплина:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 10, 0)
            };

            // === Комбобокс дисциплин ===
            this.cmbDiscipline = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 5, 15, 5)
            };
            this.cmbDiscipline.SelectedIndexChanged += CmbDiscipline_SelectedIndexChanged;

            // === Кнопка обновления ===
            var btnRefresh = new Button
            {
                Text = "🔄 Обновить",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 5, 0, 5)
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (sender, e) => LoadStudents();

            // Добавляем элементы фильтров
            filtersPanel.Controls.Add(lblGroup, 0, 0);
            filtersPanel.Controls.Add(this.cmbGroup, 1, 0);
            filtersPanel.Controls.Add(lblDiscipline, 2, 0);
            filtersPanel.Controls.Add(this.cmbDiscipline, 3, 0);
            filtersPanel.Controls.Add(btnRefresh, 4, 0);

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
                Text = "💡 Выберите группу и дисциплину, затем выставьте оценки студентам",
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
    }
}