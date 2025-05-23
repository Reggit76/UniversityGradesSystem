using MaterialSkin.Controls;
using System.Windows.Forms;
using System.Drawing;

namespace UniversityGradesSystem.Forms
{
    partial class GradeEntryForm : MaterialForm
    {
        private void InitializeComponent()
        {
            this.SuspendLayout();

            // === Панель верхних элементов управления ===
            var topPanel = new Panel
            {
                Height = 80,
                Dock = DockStyle.Top,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            // === Лейбл для группы ===
            var lblGroup = new Label
            {
                Text = "Группа:",
                Location = new Point(10, 15),
                Size = new Size(80, 23),
                Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            // === Комбобокс групп ===
            this.cmbGroup = new MaterialComboBox
            {
                Location = new Point(100, 10),
                Size = new Size(200, 30)
            };
            this.cmbGroup.SelectedIndexChanged += CmbGroup_SelectedIndexChanged;

            // === Лейбл для дисциплины ===
            var lblDiscipline = new Label
            {
                Text = "Дисциплина:",
                Location = new Point(320, 15),
                Size = new Size(90, 23),
                Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            // === Комбобокс дисциплин ===
            this.cmbDiscipline = new MaterialComboBox
            {
                Location = new Point(420, 10),
                Size = new Size(250, 30)
            };
            this.cmbDiscipline.SelectedIndexChanged += CmbDiscipline_SelectedIndexChanged;

            // === Кнопка загрузки студентов ===
            var btnLoad = new MaterialButton
            {
                Text = "ОБНОВИТЬ",
                Location = new Point(690, 10),
                Size = new Size(100, 35),
                UseAccentColor = true
            };
            btnLoad.Click += (sender, e) => LoadStudents();

            // Добавляем элементы в верхнюю панель
            topPanel.Controls.AddRange(new Control[] {
                lblGroup, this.cmbGroup, lblDiscipline, this.cmbDiscipline, btnLoad
            });

            // === Панель для DataGridView ===
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // === Таблица студентов ===
            this.dgvStudents = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 35 }
            };

            gridPanel.Controls.Add(this.dgvStudents);

            // === Панель для кнопки сохранения ===
            var bottomPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Bottom,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            // === Информационная метка ===
            var infoLabel = new Label
            {
                Text = "Выберите группу и дисциплину, затем выставьте оценки студентам",
                Location = new Point(10, 15),
                Size = new Size(400, 23),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            // === Кнопка сохранения ===
            this.btnSave = new MaterialButton
            {
                Text = "СОХРАНИТЬ ОЦЕНКИ",
                Location = new Point(650, 10),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                UseAccentColor = false
            };
            this.btnSave.Click += BtnSave_Click;

            bottomPanel.Controls.AddRange(new Control[] { infoLabel, this.btnSave });

            // === Добавление панелей на форму ===
            this.Controls.AddRange(new Control[] {
                topPanel,
                gridPanel,
                bottomPanel
            });

            // === Настройки формы ===
            this.Size = new Size(900, 600);
            this.MinimumSize = new Size(800, 500);
            this.Text = "Выставление оценок";
            this.StartPosition = FormStartPosition.CenterParent;

            this.ResumeLayout(false);
        }
    }
}