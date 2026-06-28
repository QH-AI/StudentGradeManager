using System.Data;
using System.Drawing.Drawing2D;
using StudentGradeManager.BLL;
using StudentGradeManager.DAL;
using StudentGradeManager.Models;

namespace StudentGradeManager.Forms
{
    public partial class MainForm : Form
    {
        // 颜色常量 - 现代管理后台配色
        private static readonly Color SidebarBg = Color.FromArgb(60, 90, 130);       // 深蓝灰侧边栏
        private static readonly Color SidebarHover = Color.FromArgb(75, 110, 155);
        private static readonly Color SidebarActive = Color.FromArgb(45, 75, 110);
        private static readonly Color ContentBg = Color.FromArgb(240, 242, 245);
        private static readonly Color CardBg = Color.White;
        private static readonly Color PrimaryBlue = Color.FromArgb(70, 105, 150);
        private static readonly Color TextDark = Color.FromArgb(48, 49, 51);
        private static readonly Color TextGray = Color.FromArgb(144, 147, 153);
        private static readonly Color BorderColor = Color.FromArgb(228, 231, 237);
        private static readonly Color SuccessGreen = Color.FromArgb(103, 194, 58);
        private static readonly Color WarningOrange = Color.FromArgb(230, 162, 60);
        private static readonly Color DangerRed = Color.FromArgb(245, 108, 108);

        private readonly StudentService _service;
        private DataTable _currentData = null!;
        private DataGridView dgvStudents = null!;
        private TextBox txtSearch = null!;
        private ComboBox cmbGradeFilter = null!;
        private Label lblRecordCount = null!;
        private Panel sidebarPanel = null!;
        private Panel contentPanel = null!;
        private Panel topBarPanel = null!;
        private Panel cardPanel = null!;
        private Panel statsCardsPanel = null!;
        private Label lblAvgScore = null!;
        private Label lblPassRate = null!;
        private Label lblTotalStudents = null!;
        private Button? activeNavBtn = null;
        private bool _columnsConfigured = false;

        public MainForm()
        {
            InitializeComponent();
            SetupSidebar();
            SetupContent();
            BindEvents();

            // 初始化数据库和服务
            string dbDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dbDir);
            var db = new DatabaseHelper(Path.Combine(dbDir, "students.db"));
            db.Initialize();
            _service = new StudentService(db);

            SeedSampleData();
            RefreshGrid();
            RefreshGradeFilter();
            RefreshStatsCards();

            // 默认选中第一个导航按钮 (在 navPanel 中)
            var navPanel = sidebarPanel.Controls[2] as Panel;
            if (navPanel != null && navPanel.Controls.Count > 0 && navPanel.Controls[0] is Button firstBtn)
                SelectNavButton(firstBtn);
        }

        private void InitializeComponent()
        {
            this.Text = "学生成绩管理系统";
            this.Size = new Size(1260, 760);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(960, 600);
            this.BackColor = ContentBg;
            this.Font = new Font("Microsoft YaHei", 9);
        }

        // ============================================================
        //  侧边栏
        // ============================================================
        private void SetupSidebar()
        {
            sidebarPanel = new Panel
            {
                Width = 220,
                Dock = DockStyle.Left,
                BackColor = SidebarBg,
                Padding = new Padding(0)
            };

            // Logo 区域
            var logoPanel = new Panel
            {
                Height = 64,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(50, 78, 115)
            };
            var logoLabel = new Label
            {
                Text = "  📚 成绩管理",
                Font = new Font("Microsoft YaHei", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 18)
            };
            logoPanel.Controls.Add(logoLabel);

            // 分割线
            var divider = new Panel
            {
                Height = 1,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(80, 115, 160),
                Margin = new Padding(0)
            };

            // 导航按钮区域
            var navPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 8, 0, 0)
            };

            var navButtons = new[]
            {
                ("📋  学生管理", "manage"),
                ("📊  成绩统计", "stats"),
                ("📥  数据导入", "import"),
                ("📤  数据导出", "export"),
            };

            int y = 4;
            foreach (var (text, tag) in navButtons)
            {
                var btn = CreateNavButton(text, tag, y);
                navPanel.Controls.Add(btn);
                y += 52;
            }

            // 底部用户信息
            var userPanel = new Panel
            {
                Height = 56,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(50, 78, 115)
            };
            var userLabel = new Label
            {
                Text = "  👤  软件工程2401班",
                Font = new Font("Microsoft YaHei", 9),
                ForeColor = Color.FromArgb(180, 195, 215),
                AutoSize = true,
                Location = new Point(16, 18)
            };
            userPanel.Controls.Add(userLabel);
            navPanel.Controls.Add(userPanel);

            sidebarPanel.Controls.Add(navPanel);
            sidebarPanel.Controls.Add(divider);
            sidebarPanel.Controls.Add(logoPanel);

            this.Controls.Add(sidebarPanel);
        }

        private Button CreateNavButton(string text, string tag, int y)
        {
            var btn = new Button
            {
                Text = text,
                Tag = tag,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = SidebarBg,
                ForeColor = Color.FromArgb(200, 210, 225),
                Font = new Font("Microsoft YaHei", 11, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Height = 46,
                Width = 204,
                Location = new Point(8, y),
                Cursor = Cursors.Hand,
                Padding = new Padding(16, 0, 0, 0)
            };

            btn.MouseEnter += (s, e) =>
            {
                if (btn != activeNavBtn)
                    btn.BackColor = SidebarHover;
            };
            btn.MouseLeave += (s, e) =>
            {
                if (btn != activeNavBtn)
                    btn.BackColor = SidebarBg;
            };
            btn.Click += (s, e) => SelectNavButton(btn);

            return btn;
        }

        private void SelectNavButton(Button btn)
        {
            if (activeNavBtn != null)
            {
                activeNavBtn.BackColor = SidebarBg;
                activeNavBtn.ForeColor = Color.FromArgb(200, 210, 225);
            }

            activeNavBtn = btn;
            btn.BackColor = SidebarActive;
            btn.ForeColor = Color.White;

            // 左侧选中指示条
            btn.Paint += (s, e) =>
            {
                if (s == activeNavBtn)
                {
                    using var brush = new SolidBrush(Color.FromArgb(100, 180, 255));
                    e.Graphics.FillRectangle(brush, 0, 0, 3, btn.Height);
                }
            };
        }

        // ============================================================
        //  主内容区域
        // ============================================================
        private void SetupContent()
        {
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ContentBg,
                Padding = new Padding(24, 24, 24, 18)
            };

            // 顶部栏
            SetupTopBar();

            // 统计卡片行
            SetupStatsCards();

            // 数据表格卡片
            SetupDataCard();

            // 底部状态
            SetupStatusBar();

            this.Controls.Add(contentPanel);
        }

        private void SetupTopBar()
        {
            topBarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = ContentBg
            };

            // 标题
            var titleLabel = new Label
            {
                Text = "学生管理",
                Font = new Font("Microsoft YaHei", 18, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(0, 8)
            };

            // 搜索区域（右侧）
            var searchBox = new Panel
            {
                Width = 480,
                Height = 36,
                BackColor = CardBg,
                Location = new Point(0, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            // 圆角效果（通过调整位置实现）
            searchBox.Paint += (s, e) =>
            {
                var rect = new Rectangle(0, 0, searchBox.Width - 1, searchBox.Height - 1);
                using var pen = new Pen(BorderColor);
                e.Graphics.DrawRoundedRectangle(pen, rect, 6);
            };

            txtSearch = new TextBox
            {
                Font = new Font("Microsoft YaHei", 10),
                Width = 180,
                Height = 28,
                Location = new Point(10, 4),
                BorderStyle = BorderStyle.None,
                PlaceholderText = "搜索学号或姓名..."
            };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) RefreshGrid(); };

            cmbGradeFilter = new ComboBox
            {
                Font = new Font("Microsoft YaHei", 10),
                Width = 140,
                Height = 28,
                Location = new Point(200, 4),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cmbGradeFilter.SelectedIndexChanged += (s, e) => { RefreshGrid(); RefreshStatsCards(); };

            var btnSearch = CreateFlatButton("搜索", PrimaryBlue, Color.White, 72, 28);
            btnSearch.Location = new Point(348, 4);
            btnSearch.Click += (s, e) => RefreshGrid();

            var btnClear = CreateFlatButton("清除", Color.White, TextGray, 60, 28);
            btnClear.Location = new Point(424, 4);
            btnClear.Click += (s, e) =>
            {
                txtSearch.Text = "";
                cmbGradeFilter.SelectedIndex = -1;
                RefreshGrid();
                RefreshStatsCards();
            };

            searchBox.Controls.AddRange(new Control[] { txtSearch, cmbGradeFilter, btnSearch, btnClear });

            // 右侧定位
            searchBox.Location = new Point(
                contentPanel.ClientSize.Width - 24 - 480,
                (56 - 36) / 2);

            // 操作按钮
            var btnAdd = CreateFlatButton("+ 新增学生", PrimaryBlue, Color.White, 110, 32);
            btnAdd.Location = new Point(searchBox.Left - 120, 10);
            btnAdd.Click += (s, e) => AddStudent();

            topBarPanel.Controls.Add(titleLabel);
            topBarPanel.Controls.Add(btnAdd);
            topBarPanel.Controls.Add(searchBox);

            contentPanel.Controls.Add(topBarPanel);
        }

        private void SetupStatsCards()
        {
            statsCardsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = ContentBg,
                Padding = new Padding(0, 12, 0, 12)
            };

            // 3 个统计卡片
            var cards = new[]
            {
                ("学生总数", "0", "👥", Color.FromArgb(64, 158, 255)),
                ("平均成绩", "0.0", "📈", Color.FromArgb(103, 194, 58)),
                ("及格率", "0%", "✅", Color.FromArgb(230, 162, 60)),
            };

            int cardW = (contentPanel.ClientSize.Width - 48 - 24) / 3;
            for (int i = 0; i < 3; i++)
            {
                var card = new Panel
                {
                    Width = cardW,
                    Height = 76,
                    Location = new Point(i * (cardW + 12), 0),
                    BackColor = CardBg
                };
                card.Paint += (s, e) =>
                {
                    var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                    using var pen = new Pen(BorderColor);
                    e.Graphics.DrawRoundedRectangle(pen, rect, 8);
                };

                // 图标
                var iconLabel = new Label
                {
                    Text = cards[i].Item3,
                    Font = new Font("Microsoft YaHei", 22),
                    AutoSize = true,
                    Location = new Point(16, 12),
                    BackColor = Color.Transparent
                };

                // 数值
                var valueLabel = new Label
                {
                    Text = cards[i].Item2,
                    Font = new Font("Microsoft YaHei", 20, FontStyle.Bold),
                    ForeColor = cards[i].Item4,
                    AutoSize = true,
                    Location = new Point(70, 10),
                    BackColor = Color.Transparent
                };
                // 保存引用以便更新
                switch (i)
                {
                    case 0: lblTotalStudents = valueLabel; break;
                    case 1: lblAvgScore = valueLabel; break;
                    case 2: lblPassRate = valueLabel; break;
                }

                // 标签
                var titleLbl = new Label
                {
                    Text = cards[i].Item1,
                    Font = new Font("Microsoft YaHei", 9),
                    ForeColor = TextGray,
                    AutoSize = true,
                    Location = new Point(70, 40),
                    BackColor = Color.Transparent
                };

                card.Controls.AddRange(new Control[] { iconLabel, valueLabel, titleLbl });
                statsCardsPanel.Controls.Add(card);
            }

            contentPanel.Controls.Add(statsCardsPanel);
        }

        private void SetupDataCard()
        {
            cardPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                Padding = new Padding(0)
            };
            cardPanel.Paint += (s, e) =>
            {
                var rect = new Rectangle(0, 0, cardPanel.Width - 1, cardPanel.Height - 1);
                using var pen = new Pen(BorderColor);
                e.Graphics.DrawRoundedRectangle(pen, rect, 8);
            };

            // 表格标题栏
            var tableHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.FromArgb(250, 251, 252),
                Padding = new Padding(20, 10, 16, 10)
            };
            var tableTitle = new Label
            {
                Text = "学生成绩列表",
                Font = new Font("Microsoft YaHei", 12, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(16, 12)
            };

            // 操作按钮组
            var btnEdit = CreateFlatButton("编辑", Color.White, PrimaryBlue, 64, 28);
            btnEdit.Click += (s, e) => EditStudent();
            btnEdit.Location = new Point(0, 10);
            btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            var btnDel = CreateFlatButton("删除", Color.White, DangerRed, 64, 28);
            btnDel.Click += (s, e) => DeleteStudent();
            btnDel.Location = new Point(-72, 10);
            btnDel.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            var btnRefresh = CreateFlatButton("刷新", Color.White, TextGray, 64, 28);
            btnRefresh.Click += (s, e) => RefreshGrid();
            btnRefresh.Location = new Point(-144, 10);
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            var btnActions = new Panel
            {
                Width = 230,
                Height = 48,
                Location = new Point(cardPanel.Width - 250, 0),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnActions.Controls.AddRange(new Control[] { btnEdit, btnDel, btnRefresh });

            // 定位编辑按钮
            btnEdit.Location = new Point(btnActions.Width - 74, 10);
            btnDel.Location = new Point(btnActions.Width - 148, 10);
            btnRefresh.Location = new Point(btnActions.Width - 222, 10);

            tableHeader.Controls.Add(tableTitle);
            tableHeader.Controls.Add(btnActions);

            // DataGridView
            dgvStudents = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                Font = new Font("Microsoft YaHei", 9),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(245, 247, 250)
            };

            dgvStudents.EnableHeadersVisualStyles = false;
            dgvStudents.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(250, 251, 252),
                ForeColor = TextGray,
                Font = new Font("Microsoft YaHei", 9, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(0, 8, 0, 8),
                SelectionBackColor = Color.FromArgb(250, 251, 252)
            };
            dgvStudents.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = TextDark,
                SelectionBackColor = Color.FromArgb(236, 245, 255),
                SelectionForeColor = TextDark,
                Padding = new Padding(8, 4, 8, 4)
            };
            dgvStudents.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 250, 252)
            };
            dgvStudents.RowTemplate.Height = 38;
            dgvStudents.ColumnHeadersHeight = 40;
            dgvStudents.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditStudent(); };

            cardPanel.Controls.Add(dgvStudents);
            cardPanel.Controls.Add(tableHeader);
            contentPanel.Controls.Add(cardPanel);
        }

        private void SetupStatusBar()
        {
            var statusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                BackColor = ContentBg,
                Padding = new Padding(0, 6, 0, 0)
            };

            lblRecordCount = new Label
            {
                Text = "共 0 条记录",
                Font = new Font("Microsoft YaHei", 9),
                ForeColor = TextGray,
                AutoSize = true
            };
            statusBar.Controls.Add(lblRecordCount);
            contentPanel.Controls.Add(statusBar);
        }

        // ============================================================
        //  辅助方法
        // ============================================================
        private Button CreateFlatButton(string text, Color bg, Color fg, int w, int h)
        {
            var btn = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = bg,
                ForeColor = fg,
                Font = new Font("Microsoft YaHei", 9),
                Size = new Size(w, h),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.Paint += (s, e) =>
            {
                var rect = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using var brush = new SolidBrush(btn.BackColor);
                using var pen = new Pen(btn.BackColor == Color.White ? BorderColor : btn.BackColor);
                // 简单圆角
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = GetRoundedRect(rect, 4);
                e.Graphics.FillPath(brush, path);
                // 文字
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var textBrush = new SolidBrush(fg);
                e.Graphics.DrawString(text, btn.Font, textBrush, rect, sf);
            };
            btn.MouseEnter += (s, e) =>
            {
                if (bg == PrimaryBlue) btn.BackColor = Color.FromArgb(90, 130, 180);
                else if (bg == Color.White) btn.BackColor = Color.FromArgb(245, 247, 250);
                else btn.BackColor = ControlPaint.Dark(bg, 0.1f);
            };
            btn.MouseLeave += (s, e) => { btn.BackColor = bg; };
            return btn;
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void BindEvents()
        {
            this.Resize += (s, e) =>
            {
                if (cardPanel != null) cardPanel.Invalidate();
                if (statsCardsPanel != null) statsCardsPanel.Invalidate();
            };
        }

        // ============================================================
        //  数据刷新
        // ============================================================
        private void RefreshGrid()
        {
            string keyword = txtSearch.Text?.Trim() ?? "";
            string grade = cmbGradeFilter.SelectedItem?.ToString() ?? "";
            if (grade == "全部班级") grade = "";

            _currentData = _service.SearchStudents(keyword, grade);

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(RefreshGrid));
                return;
            }

            dgvStudents.DataSource = null;
            dgvStudents.DataSource = _currentData;

            if (_currentData.Rows.Count > 0)
            {
                ConfigColumns();
                lblRecordCount.Text = $"共 {_currentData.Rows.Count} 条记录";
            }
            else
            {
                lblRecordCount.Text = "共 0 条记录";
            }

            RefreshStatsCards();
        }

        private void RefreshStatsCards()
        {
            if (_currentData == null || _currentData.Rows.Count == 0)
            {
                lblTotalStudents.Text = "0";
                lblAvgScore.Text = "0.0";
                lblPassRate.Text = "0%";
                return;
            }

            int total = _currentData.Rows.Count;
            double avgScore = 0;
            int passCount = 0;

            foreach (DataRow row in _currentData.Rows)
            {
                double score = Convert.ToDouble(row["AverageScore"]);
                avgScore += score;
                if (score >= 60) passCount++;
            }
            avgScore /= total;

            lblTotalStudents.Text = total.ToString();
            lblAvgScore.Text = avgScore.ToString("F1");
            lblPassRate.Text = $"{(double)passCount / total * 100:F1}%";
        }

        private void ConfigColumns()
        {
            if (dgvStudents.Columns.Count == 0) return;

            dgvStudents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            if (dgvStudents.Columns.Contains("Id"))
                dgvStudents.Columns["Id"]!.Visible = false;

            var headerMap = new Dictionary<string, string>
            {
                { "StudentId", "学号" },
                { "Name", "姓名" },
                { "Grade", "班级" },
                { "MathScore", "数学" },
                { "EnglishScore", "英语" },
                { "ProgrammingScore", "程序设计" },
                { "AverageScore", "平均分" },
                { "GradeLevel", "等级" },
                { "CreatedAt", "创建时间" },
                { "UpdatedAt", "更新时间" }
            };

            foreach (DataGridViewColumn col in dgvStudents.Columns)
            {
                if (headerMap.TryGetValue(col.Name, out string? header))
                    col.HeaderText = header;

                switch (col.Name)
                {
                    case "StudentId": col.Width = 110; break;
                    case "Name": col.Width = 80; break;
                    case "Grade": col.Width = 140; break;
                    case "MathScore":
                    case "EnglishScore":
                    case "ProgrammingScore":
                        col.Width = 80;
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        break;
                    case "AverageScore":
                        col.Width = 75;
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        col.DefaultCellStyle.Font = new Font("Microsoft YaHei", 9, FontStyle.Bold);
                        break;
                    case "GradeLevel":
                        col.Width = 70;
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        break;
                    case "CreatedAt":
                    case "UpdatedAt":
                        col.Visible = false;
                        break;
                }
            }

            dgvStudents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            if (!_columnsConfigured && dgvStudents.Columns.Contains("GradeLevel"))
            {
                dgvStudents.CellFormatting += DgvStudents_CellFormatting;
                _columnsConfigured = true;
            }
        }

        private void DgvStudents_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvStudents.Columns.Contains("GradeLevel") &&
                e.ColumnIndex == dgvStudents.Columns["GradeLevel"]!.Index &&
                e.RowIndex >= 0)
            {
                var val = dgvStudents.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
                e.CellStyle!.ForeColor = val switch
                {
                    "优秀" => Color.FromArgb(46, 125, 50),
                    "良好" => Color.FromArgb(21, 101, 192),
                    "中等" => Color.FromArgb(245, 124, 0),
                    "及格" => Color.FromArgb(230, 81, 0),
                    "不及格" => Color.FromArgb(198, 40, 40),
                    _ => TextDark
                };
                e.CellStyle.Font = new Font("Microsoft YaHei", 9, FontStyle.Bold);
            }
        }

        private void RefreshGradeFilter()
        {
            var grades = _service.GetGrades();
            cmbGradeFilter.Items.Clear();
            cmbGradeFilter.Items.Add("全部班级");
            foreach (var g in grades)
                cmbGradeFilter.Items.Add(g);
            cmbGradeFilter.SelectedIndex = 0;
        }

        // ============================================================
        //  业务操作（与之前相同）
        // ============================================================
        private void AddStudent()
        {
            using var dialog = new StudentEditDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var (success, msg) = _service.AddStudent(dialog.StudentData);
                ShowToast(msg, success);
                if (success) { RefreshGrid(); RefreshGradeFilter(); }
            }
        }

        private void EditStudent()
        {
            if (dgvStudents.SelectedRows.Count == 0)
            {
                ShowToast("请先选择要编辑的学生", false);
                return;
            }

            var row = dgvStudents.SelectedRows[0];
            var student = new Student
            {
                Id = Convert.ToInt32(row.Cells["Id"].Value),
                StudentId = row.Cells["StudentId"].Value?.ToString()!,
                Name = row.Cells["Name"].Value?.ToString()!,
                Grade = row.Cells["Grade"].Value?.ToString()!,
                MathScore = Convert.ToSingle(row.Cells["MathScore"].Value),
                EnglishScore = Convert.ToSingle(row.Cells["EnglishScore"].Value),
                ProgrammingScore = Convert.ToSingle(row.Cells["ProgrammingScore"].Value)
            };

            using var dialog = new StudentEditDialog(student);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                dialog.StudentData.Id = student.Id;
                var (success, msg) = _service.UpdateStudent(dialog.StudentData);
                ShowToast(msg, success);
                if (success) { RefreshGrid(); RefreshGradeFilter(); }
            }
        }

        private void DeleteStudent()
        {
            if (dgvStudents.SelectedRows.Count == 0)
            {
                ShowToast("请先选择要删除的学生", false);
                return;
            }

            var count = dgvStudents.SelectedRows.Count;
            var result = MessageBox.Show($"确定要删除选中的 {count} 名学生吗？\n此操作不可恢复。",
                "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            int deleted = 0;
            foreach (DataGridViewRow row in dgvStudents.SelectedRows)
            {
                int id = Convert.ToInt32(row.Cells["Id"].Value);
                var (success, _) = _service.DeleteStudent(id);
                if (success) deleted++;
            }

            ShowToast($"已删除 {deleted} 条记录", true);
            RefreshGrid();
            RefreshGradeFilter();
        }

        private void ImportCsv()
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "CSV文件|*.csv|所有文件|*.*",
                Title = "选择CSV文件导入"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            Cursor = Cursors.WaitCursor;
            var (imported, skipped) = _service.ImportCsv(ofd.FileName);
            Cursor = Cursors.Default;

            ShowToast($"导入完成！成功 {imported} 条，跳过 {skipped} 条", true);
            RefreshGrid();
            RefreshGradeFilter();
        }

        private void ExportCsv()
        {
            if (_currentData == null || _currentData.Rows.Count == 0)
            {
                ShowToast("没有数据可导出", false);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "CSV文件|*.csv",
                Title = "导出CSV文件",
                FileName = $"学生成绩_导出_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            _service.ExportCsv(sfd.FileName, _currentData);
            ShowToast($"成功导出 {_currentData.Rows.Count} 条记录", true);
        }

        private void ShowChart()
        {
            using var chartForm = new ChartForm(_service);
            chartForm.ShowDialog(this);
        }

        private void ShowReport()
        {
            var stats = _service.GetGradeStatistics();
            if (stats.Rows.Count == 0)
            {
                ShowToast("暂无统计数据", false);
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("═══════════ 班级统计报表 ═══════════\n");
            foreach (DataRow row in stats.Rows)
            {
                sb.AppendLine($"📋 {row["班级"]}");
                sb.AppendLine($"   人数: {row["人数"]}  |  平均分: {row["平均分"]}  |  最高分: {row["最高分"]}  |  及格率: {row["及格率"]}%\n");
            }

            MessageBox.Show(sb.ToString(), "统计报表", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowToast(string message, bool isSuccess)
        {
            // 简单Toast: 底部居中提示
            var toast = new Form
            {
                Size = new Size(400, 40),
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                BackColor = isSuccess ? Color.FromArgb(103, 194, 58) : Color.FromArgb(245, 108, 108),
                ShowInTaskbar = false,
                TopMost = true,
                Opacity = 0.95
            };

            var label = new Label
            {
                Text = message,
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            toast.Controls.Add(label);

            // 定位到父窗口底部居中
            toast.Location = new Point(
                this.Location.X + (this.Width - toast.Width) / 2,
                this.Location.Y + this.Height - toast.Height - 60);

            toast.Show(this);
            var timer = new System.Windows.Forms.Timer { Interval = 2000 };
            timer.Tick += (s, e) => { timer.Stop(); toast.Close(); };
            timer.Start();
        }

        private void SeedSampleData()
        {
            var dt = _service.SearchStudents("", "");
            if (dt.Rows.Count > 0) return;

            var samples = new[]
            {
                new Student { StudentId = "20242401001", Name = "张三", Grade = "软件工程2401班", MathScore = 92, EnglishScore = 88, ProgrammingScore = 95 },
                new Student { StudentId = "20242401002", Name = "李四", Grade = "软件工程2401班", MathScore = 78, EnglishScore = 85, ProgrammingScore = 80 },
                new Student { StudentId = "20242401003", Name = "王五", Grade = "软件工程2401班", MathScore = 65, EnglishScore = 60, ProgrammingScore = 72 },
                new Student { StudentId = "20242401004", Name = "赵六", Grade = "软件工程2401班", MathScore = 45, EnglishScore = 52, ProgrammingScore = 48 },
                new Student { StudentId = "20242401005", Name = "孙七", Grade = "软件工程2401班", MathScore = 88, EnglishScore = 91, ProgrammingScore = 86 },
                new Student { StudentId = "20242402001", Name = "周八", Grade = "软件工程2402班", MathScore = 95, EnglishScore = 93, ProgrammingScore = 97 },
                new Student { StudentId = "20242402002", Name = "吴九", Grade = "软件工程2402班", MathScore = 72, EnglishScore = 68, ProgrammingScore = 75 },
                new Student { StudentId = "20242402003", Name = "郑十", Grade = "软件工程2402班", MathScore = 58, EnglishScore = 62, ProgrammingScore = 55 },
                new Student { StudentId = "20242402004", Name = "陈一", Grade = "软件工程2402班", MathScore = 90, EnglishScore = 87, ProgrammingScore = 92 },
                new Student { StudentId = "20242402005", Name = "刘二", Grade = "软件工程2402班", MathScore = 82, EnglishScore = 79, ProgrammingScore = 85 },
            };

            foreach (var s in samples)
                _service.AddStudent(s);
        }
    }

    // 扩展方法：圆角矩形
    public static class GraphicsExtensions
    {
        public static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle rect, int radius)
        {
            using var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            g.DrawPath(pen, path);
        }
    }
}
