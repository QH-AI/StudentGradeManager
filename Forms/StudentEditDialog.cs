using StudentGradeManager.Models;

namespace StudentGradeManager.Forms
{
    public partial class StudentEditDialog : Form
    {
        public Student StudentData { get; private set; } = null!;
        private readonly bool _isEditMode;
        private TextBox txtStudentId = null!, txtName = null!, txtGrade = null!;
        private NumericUpDown numMath = null!, numEnglish = null!, numProgramming = null!;
        private Button btnSave = null!, btnCancel = null!;
        private TableLayoutPanel table = null!;

        /// <summary>
        /// 新增模式
        /// </summary>
        public StudentEditDialog()
        {
            _isEditMode = false;
            InitializeComponent();
            this.Text = "新增学生";
        }

        /// <summary>
        /// 编辑模式
        /// </summary>
        public StudentEditDialog(Student existing) : this()
        {
            _isEditMode = true;
            this.Text = "编辑学生";
            // 预填充数据
            txtStudentId.Text = existing.StudentId;
            txtName.Text = existing.Name;
            txtGrade.Text = existing.Grade;
            numMath.Value = (decimal)existing.MathScore;
            numEnglish.Value = (decimal)existing.EnglishScore;
            numProgramming.Value = (decimal)existing.ProgrammingScore;
        }

        private void InitializeComponent()
        {
            this.Size = new Size(420, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            var primaryBlue = Color.FromArgb(70, 105, 150);
            var textDark = Color.FromArgb(48, 49, 51);
            var bgColor = Color.FromArgb(240, 242, 245);

            this.BackColor = bgColor;
            this.Padding = new Padding(24);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;

            var titleFont = new Font("Microsoft YaHei", 14, FontStyle.Bold);
            var labelFont = new Font("Microsoft YaHei", 10);
            var inputFont = new Font("Microsoft YaHei", 10);

            // Title
            var lblTitle = new Label
            {
                Text = _isEditMode ? "编辑学生信息" : "新增学生",
                Font = titleFont,
                ForeColor = textDark,
                Dock = DockStyle.Top,
                Height = 36
            };

            // 输入表单
            table = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 6,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 0),
                ColumnStyles = { new ColumnStyle(SizeType.Absolute, 120), new ColumnStyle(SizeType.Percent, 100) }
            };

            // 学号
            table.Controls.Add(new Label { Text = "学号：", Font = labelFont, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
            txtStudentId = new TextBox { Font = inputFont, Dock = DockStyle.Fill, Height = 30, PlaceholderText = "输入8-12位数字学号" };
            table.Controls.Add(txtStudentId, 1, 0);

            // 姓名
            table.Controls.Add(new Label { Text = "姓名：", Font = labelFont, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
            txtName = new TextBox { Font = inputFont, Dock = DockStyle.Fill, Height = 30, PlaceholderText = "输入学生姓名" };
            table.Controls.Add(txtName, 1, 1);

            // 班级
            table.Controls.Add(new Label { Text = "班级：", Font = labelFont, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 2);
            txtGrade = new TextBox { Font = inputFont, Dock = DockStyle.Fill, Height = 30, PlaceholderText = "如：软件工程2401班" };
            table.Controls.Add(txtGrade, 1, 2);

            // 数学成绩
            table.Controls.Add(new Label { Text = "数学成绩：", Font = labelFont, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 3);
            numMath = new NumericUpDown { Font = inputFont, Minimum = 0, Maximum = 100, DecimalPlaces = 1, Dock = DockStyle.Fill, Height = 30 };
            table.Controls.Add(numMath, 1, 3);

            // 英语成绩
            table.Controls.Add(new Label { Text = "英语成绩：", Font = labelFont, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 4);
            numEnglish = new NumericUpDown { Font = inputFont, Minimum = 0, Maximum = 100, DecimalPlaces = 1, Dock = DockStyle.Fill, Height = 30 };
            table.Controls.Add(numEnglish, 1, 4);

            // 程序设计成绩
            table.Controls.Add(new Label { Text = "程序设计成绩：", Font = labelFont, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 5);
            numProgramming = new NumericUpDown { Font = inputFont, Minimum = 0, Maximum = 100, DecimalPlaces = 1, Dock = DockStyle.Fill, Height = 30 };
            table.Controls.Add(numProgramming, 1, 5);

            // 底部按钮
            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0)
            };

            btnCancel = new Button
            {
                Text = "取消",
                Font = labelFont,
                Size = new Size(90, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(224, 224, 224),
                FlatAppearance = { BorderSize = 0 }
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            btnSave = new Button
            {
                Text = "保存",
                Font = labelFont,
                Size = new Size(90, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(25, 118, 210),
                ForeColor = Color.White,
                FlatAppearance = { BorderSize = 0 }
            };
            btnSave.Click += BtnSave_Click;

            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnSave);

            this.Controls.Add(table);
            this.Controls.Add(btnPanel);
            this.Controls.Add(lblTitle);
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // 输入验证
            if (string.IsNullOrWhiteSpace(txtStudentId.Text))
            {
                MessageBox.Show("学号不能为空", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStudentId.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("姓名不能为空", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            StudentData = new Student
            {
                StudentId = txtStudentId.Text.Trim(),
                Name = txtName.Text.Trim(),
                Grade = txtGrade.Text.Trim(),
                MathScore = (float)numMath.Value,
                EnglishScore = (float)numEnglish.Value,
                ProgrammingScore = (float)numProgramming.Value
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
