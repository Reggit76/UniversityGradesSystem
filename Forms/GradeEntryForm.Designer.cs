using System.Windows.Forms;
using System.Drawing;

namespace UniversityGradesSystem.Forms
{
    partial class GradeEntryForm : Form
    {
        private void InitializeComponent()
        {
            this.SuspendLayout();

            // === Лейбл для группы ===
            var lblGroup = new Label
            {
                Text = "Группа:",
                Location = new Point(20, 20),
                Size = new Size(80, 23),
                Font = new Font("Microsoft Sans Serif", 10F)
            };

            // === Комбобокс групп ===
            this.cmbGroup = new ComboBox
            {
                Location = new Point(110, 17),
                Size = new Size(200, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.cmbGroup.SelectedIndexChanged += CmbGroup_SelectedIndexChanged;

            // === Лейбл для дисциплины ===
            var lblDiscipline = new Label
            {
                Text = "Дисциплина:",
                Location = new Point(330, 20),
                Size = new Size(90, 23),
                Font = new Font("Microsoft Sans Serif", 10F)
            };

            // === Комбобокс дисциплин ===
            this.cmbDiscipline = new ComboBox
            {
                Location = new Point(430, 17),
                Size = new Size(250, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.cmbDiscipline.SelectedIndexChanged += CmbDiscipline_SelectedIndexChanged;

            // === Кнопка обновления ===
            var btnRefresh = new Button
            {
                Text = "Обновить",
                Location = new Point(700, 17),
                Size = new Size(80, 30),
                BackColor = Color.LightBlue
            };
            btnRefresh.Click += (sender, e) => LoadStudents();

            // === Таблица студентов ===
            this.dgvStudents = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(760, 350),
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };

            // === Кнопка сохранения ===
            this.btnSave = new Button
            {
                Text = "Сохранить оценки",
                Location = new Point(20, 440),
                Size = new Size(150, 35),
                BackColor = Color.LightGreen,
                Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold)
            };
            this.btnSave.Click += BtnSave_Click;

            // === Информационная метка ===
            var lblInfo = new Label
            {
                Text = "Выберите группу и дисциплину, затем выставьте оценки студентам",
                Location = new Point(190, 450),
                Size = new Size(400, 20),
                ForeColor = Color.Gray,
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Italic)
            };

            // === Добавление всех элементов на форму ===
            this.Controls.AddRange(new Control[] {
                lblGroup, this.cmbGroup,
                lblDiscipline, this.cmbDiscipline,
                btnRefresh, this.dgvStudents,
                this.btnSave, lblInfo
            });

            // === Настройки формы ===
            this.Text = "Выставление оценок";
            this.Size = new Size(820, 520);
            this.MinimumSize = new Size(820, 520);
            this.MaximumSize = new Size(820, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.WhiteSmoke;

            this.ResumeLayout(false);
        }
    }
}