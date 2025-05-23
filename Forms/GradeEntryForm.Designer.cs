// Добавьте пространство имен, чтобы partial-классы объединились
using MaterialSkin.Controls;
using System.Windows.Forms;

namespace UniversityGradesSystem.Forms
{
    partial class GradeEntryForm : MaterialForm // Укажите наследование
    {
        private void InitializeComponent()
        {
            this.SuspendLayout(); // Теперь доступно

            // === Лейбл для группы ===
            var lblGroup = new Label
            {
                Text = "Группа:",
                Location = new System.Drawing.Point(50, 10),
                Width = 80
            };

            // === Комбобокс групп ===
            this.cmbGroup = new MaterialComboBox
            {
                Location = new System.Drawing.Point(140, 5),
                Width = 200
            };
            this.cmbGroup.SelectedIndexChanged += CmbGroup_SelectedIndexChanged;

            // === Лейбл для дисциплины ===
            var lblDiscipline = new Label
            {
                Text = "Дисциплина:",
                Location = new System.Drawing.Point(50, 40),
                Width = 80
            };

            // === Комбобокс дисциплин ===
            this.cmbDiscipline = new MaterialComboBox
            {
                Location = new System.Drawing.Point(140, 35),
                Width = 200
            };
            this.cmbDiscipline.SelectedIndexChanged += CmbDiscipline_SelectedIndexChanged;

            // === Кнопка загрузки студентов ===
            var btnLoad = new MaterialButton
            {
                Text = "Загрузить студентов",
                Location = new System.Drawing.Point(350, 30),
                Width = 150
            };
            btnLoad.Click += (sender, e) => LoadStudents();

            // === Таблица студентов ===
            this.dgvStudents = new DataGridView
            {
                Location = new System.Drawing.Point(50, 70),
                Size = new System.Drawing.Size(700, 300),
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            InitializeDataGridView();

            // === Кнопка сохранения ===
            this.btnSave = new MaterialButton
            {
                Text = "Сохранить оценки",
                Location = new System.Drawing.Point(50, 380),
                Width = 150
            };
            this.btnSave.Click += BtnSave_Click;

            // === Добавление элементов ===
            this.Controls.AddRange(new Control[] {
                lblGroup,
                this.cmbGroup,
                lblDiscipline,
                this.cmbDiscipline,
                btnLoad,
                this.dgvStudents,
                this.btnSave
            });

            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "Выставление оценок"; // Теперь доступно
            this.ResumeLayout(false); // Теперь доступно
        }
    }
}