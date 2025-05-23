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

            // === Комбобокс дисциплин ===
            this.cmbDiscipline = new MaterialComboBox
            {
                Location = new System.Drawing.Point(50, 30),
                Width = 200
            };

            // === Кнопка загрузки студентов ===
            var btnLoad = new MaterialButton
            {
                Text = "Загрузить студентов",
                Location = new System.Drawing.Point(260, 28)
            };
            btnLoad.Click += (sender, e) => LoadStudents();

            // === Таблица студентов ===
            this.dgvStudents = new DataGridView
            {
                Dock = DockStyle.Fill,
                Location = new System.Drawing.Point(50, 60),
                AutoGenerateColumns = true
            };

            // === Поле ввода оценки ===
            var txtGrade = new TextBox
            {
                Location = new System.Drawing.Point(50, 310),
                Width = 50
            };

            // === Кнопка сохранения ===
            this.btnSave = new MaterialButton
            {
                Text = "Сохранить оценку",
                Location = new System.Drawing.Point(110, 310)
            };
            this.btnSave.Click += BtnSave_Click;

            // === Добавление элементов ===
            this.Controls.AddRange(new Control[] {
                this.cmbDiscipline,
                btnLoad,
                this.dgvStudents,
                txtGrade,
                this.btnSave
            });

            this.ClientSize = new System.Drawing.Size(800, 400);
            this.Text = "Выставление оценок"; // Теперь доступно
            this.ResumeLayout(false); // Теперь доступно
        }
    }
}