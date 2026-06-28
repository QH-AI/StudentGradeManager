using StudentGradeManager.BLL;
using StudentGradeManager.DAL;
using System.Data;

namespace StudentGradeManager.Forms
{
    public partial class ChartForm : Form
    {
        private readonly StudentService _service;
        private ComboBox cmbChartType = null!;
        private Panel drawPanel = null!;

        public ChartForm(StudentService service)
        {
            _service = service;
            InitializeComponent();
            DrawChart();
        }

        private void InitializeComponent()
        {
            this.Text = "成绩统计分析图表";
            this.Size = new Size(750, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(600, 400);
            this.BackColor = Color.White;

            // 顶部工具栏
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(25, 118, 210),
                Padding = new Padding(10, 8, 10, 8)
            };

            var lblTitle = new Label
            {
                Text = "📊  成绩统计分析",
                Font = new Font("Microsoft YaHei", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 10)
            };
            topPanel.Controls.Add(lblTitle);

            cmbChartType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Microsoft YaHei", 10),
                Width = 160,
                Height = 30,
                Location = new Point(topPanel.Width - 180, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            cmbChartType.Items.AddRange(new[] { "成绩分布饼图", "班级统计柱状图", "等级分布柱状图" });
            cmbChartType.SelectedIndex = 0;
            cmbChartType.SelectedIndexChanged += (s, e) => DrawChart();
            topPanel.Controls.Add(cmbChartType);

            drawPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            drawPanel.Paint += DrawPanel_Paint;

            this.Controls.Add(drawPanel);
            this.Controls.Add(topPanel);
        }

        private void DrawChart()
        {
            drawPanel.Invalidate();
        }

        private void DrawPanel_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            switch (cmbChartType.SelectedIndex)
            {
                case 0: DrawPieChart(g, drawPanel.ClientRectangle); break;
                case 1: DrawBarChart(g, drawPanel.ClientRectangle); break;
                case 2: DrawDistributionChart(g, drawPanel.ClientRectangle); break;
            }
        }

        private void DrawPieChart(Graphics g, Rectangle rect)
        {
            var data = _service.GetScoreDistribution();
            var colors = new[] { Color.FromArgb(76, 175, 80), Color.FromArgb(33, 150, 243), Color.FromArgb(255, 193, 7), Color.FromArgb(255, 152, 0), Color.FromArgb(244, 67, 54) };
            var total = data.Values.Sum();
            if (total == 0) { DrawEmpty(g, rect); return; }

            int cx = rect.X + rect.Width / 2 - 80, cy = rect.Y + rect.Height / 2 - 20;
            int radius = Math.Min(rect.Width - 350, rect.Height - 60) / 2;
            float startAngle = -90f;

            int i = 0;
            var font = new Font("Microsoft YaHei", 10);
            int legendX = cx + radius + 40;
            int legendY = cy - radius + 20;

            foreach (var kv in data)
            {
                if (kv.Value == 0) { i++; continue; }
                float sweep = (float)kv.Value / total * 360f;
                using var brush = new SolidBrush(colors[i]);
                g.FillPie(brush, cx - radius, cy - radius, radius * 2, radius * 2, startAngle, sweep);
                g.DrawPie(Pens.White, cx - radius, cy - radius, radius * 2, radius * 2, startAngle, sweep);

                // Legend
                g.FillRectangle(brush, legendX, legendY, 16, 16);
                g.DrawRectangle(Pens.Gray, legendX, legendY, 16, 16);
                g.DrawString($"{kv.Key}: {kv.Value}人 ({kv.Value * 100.0 / total:F1}%)", font, Brushes.Black, legendX + 24, legendY - 1);
                legendY += 30;
                startAngle += sweep;
                i++;
            }

            font.Dispose();
        }

        private void DrawBarChart(Graphics g, Rectangle rect)
        {
            var dt = _service.GetGradeStatistics();
            if (dt.Rows.Count == 0) { DrawEmpty(g, rect); return; }

            var font = new Font("Microsoft YaHei", 9);
            var titleFont = new Font("Microsoft YaHei", 12, FontStyle.Bold);

            int marginLeft = 80, marginRight = 40, marginTop = 40, marginBottom = 80;
            int chartW = rect.Width - marginLeft - marginRight;
            int chartH = rect.Height - marginTop - marginBottom;

            // Find max value
            double maxVal = 0;
            foreach (DataRow row in dt.Rows)
            {
                maxVal = Math.Max(maxVal, Convert.ToDouble(row["平均分"]));
            }
            maxVal = Math.Ceiling(maxVal / 10) * 10;
            if (maxVal <= 0) maxVal = 100;

            var barColors = new[] { Color.FromArgb(33, 150, 243), Color.FromArgb(76, 175, 80), Color.FromArgb(255, 193, 7), Color.FromArgb(156, 39, 176), Color.FromArgb(255, 87, 34) };

            int barCount = dt.Rows.Count;
            int barWidth = Math.Min(80, (chartW / barCount) - 20);
            int gap = (chartW - barWidth * barCount) / (barCount + 1);

            // Y axis
            g.DrawLine(Pens.Gray, marginLeft, marginTop, marginLeft, marginTop + chartH);
            // X axis
            g.DrawLine(Pens.Gray, marginLeft, marginTop + chartH, marginLeft + chartW, marginTop + chartH);

            // Y axis labels
            for (int v = 0; v <= maxVal; v += (int)(maxVal / 5 > 0 ? maxVal / 5 : 10))
            {
                int y = marginTop + chartH - (int)(v / maxVal * chartH);
                g.DrawString(v.ToString(), font, Brushes.Gray, marginLeft - 50, y - 8);
                g.DrawLine(Pens.LightGray, marginLeft, y, marginLeft + chartW, y);
            }

            for (int i = 0; i < barCount; i++)
            {
                var row = dt.Rows[i];
                double avg = Convert.ToDouble(row["平均分"]);
                int barH = (int)(avg / maxVal * chartH);
                int x = marginLeft + gap + i * (barWidth + gap);
                int y = marginTop + chartH - barH;

                using var brush = new SolidBrush(barColors[i % barColors.Length]);
                g.FillRectangle(brush, x, y, barWidth, barH);
                g.DrawRectangle(Pens.White, x, y, barWidth, barH);

                // Value on top
                string valText = avg.ToString("F1");
                var sz = g.MeasureString(valText, font);
                g.DrawString(valText, font, Brushes.Black, x + (barWidth - sz.Width) / 2, y - sz.Height);

                // Label below
                string label = row["班级"].ToString()!;
                var lsz = g.MeasureString(label, font);
                g.DrawString(label, font, Brushes.Black, x + (barWidth - lsz.Width) / 2, marginTop + chartH + 5);
            }

            g.DrawString("班级平均分统计", titleFont, Brushes.Black, marginLeft, 5);
            font.Dispose();
            titleFont.Dispose();
        }

        private void DrawDistributionChart(Graphics g, Rectangle rect)
        {
            var data = _service.GetScoreDistribution();
            var font = new Font("Microsoft YaHei", 9);
            var titleFont = new Font("Microsoft YaHei", 12, FontStyle.Bold);

            int marginLeft = 100, marginRight = 40, marginTop = 40, marginBottom = 80;
            int chartW = rect.Width - marginLeft - marginRight;
            int chartH = rect.Height - marginTop - marginBottom;

            int maxVal = Math.Max(data.Values.Max(), 1);
            maxVal = (int)Math.Ceiling(maxVal / 5.0) * 5;
            if (maxVal <= 0) maxVal = 10;

            var keys = data.Keys.ToArray();
            var values = data.Values.ToArray();
            var colors = new[] { Color.FromArgb(76, 175, 80), Color.FromArgb(33, 150, 243), Color.FromArgb(255, 193, 7), Color.FromArgb(255, 152, 0), Color.FromArgb(244, 67, 54) };

            int barCount = keys.Length;
            int barWidth = Math.Min(80, (chartW / barCount) - 20);
            int gap = (chartW - barWidth * barCount) / (barCount + 1);

            g.DrawLine(Pens.Gray, marginLeft, marginTop, marginLeft, marginTop + chartH);
            g.DrawLine(Pens.Gray, marginLeft, marginTop + chartH, marginLeft + chartW, marginTop + chartH);

            for (int v = 0; v <= maxVal; v += Math.Max(1, maxVal / 5))
            {
                int y = marginTop + chartH - (int)((double)v / maxVal * chartH);
                g.DrawString(v.ToString(), font, Brushes.Gray, marginLeft - 40, y - 8);
                g.DrawLine(Pens.LightGray, marginLeft, y, marginLeft + chartW, y);
            }

            for (int i = 0; i < barCount; i++)
            {
                int barH = (int)((double)values[i] / maxVal * chartH);
                int x = marginLeft + gap + i * (barWidth + gap);
                int y = marginTop + chartH - barH;

                using var brush = new SolidBrush(colors[i]);
                g.FillRectangle(brush, x, y, barWidth, barH);
                g.DrawRectangle(Pens.White, x, y, barWidth, barH);

                string valText = values[i].ToString();
                var sz = g.MeasureString(valText, font);
                g.DrawString(valText, font, Brushes.Black, x + (barWidth - sz.Width) / 2, y - sz.Height);

                // 标签——短化显示
                string label = keys[i].Replace("优秀(>=90)", "优秀").Replace("良好(80-89)", "良好").Replace("中等(70-79)", "中等").Replace("及格(60-69)", "及格").Replace("不及格(<60)", "不及格");
                var lsz = g.MeasureString(label, font);
                g.DrawString(label, font, Brushes.Black, x + (barWidth - lsz.Width) / 2, marginTop + chartH + 5);
            }

            g.DrawString("成绩等级分布", titleFont, Brushes.Black, marginLeft, 5);
            font.Dispose();
            titleFont.Dispose();
        }

        private void DrawEmpty(Graphics g, Rectangle rect)
        {
            var font = new Font("Microsoft YaHei", 14);
            var text = "暂无数据，请先添加学生记录";
            var sz = g.MeasureString(text, font);
            g.DrawString(text, font, Brushes.Gray,
                rect.X + (rect.Width - sz.Width) / 2,
                rect.Y + (rect.Height - sz.Height) / 2);
            font.Dispose();
        }
    }
}
