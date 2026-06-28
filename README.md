# 学生成绩管理系统 (Student Grade Manager)

基于 **C# WinForm .NET 9.0 + SQLite** 的桌面端学生成绩管理系统。

## 功能特性

- 学生信息 CRUD（学号、姓名、班级、成绩）
- 多条件搜索与班级筛选
- 顶部统计卡片（总数/均分/及格率，实时联动）
- GDI+ 成绩分布图表（饼图 + 柱状图，3种切换）
- CSV 导入/导出（BOM 编码，兼容 Excel）
- 现代管理后台 UI（深色侧边栏 + 白色卡片化内容区）
- Toast 操作反馈、输入验证、防 SQL 注入

## 技术栈

| 层次 | 技术 |
|------|------|
| UI | WinForm .NET 9.0，自绘圆角控件 |
| 数据库 | SQLite (System.Data.SQLite) |
| 架构 | 三层架构 (Models / BLL / DAL) |
| 图表 | GDI+ Graphics 自绘 |
| 测试 | xUnit |

## 项目结构

```
StudentGradeManager/
├── Models/          Student.cs
├── DAL/             DatabaseHelper.cs
├── BLL/             StudentService.cs
├── Forms/
│   ├── MainForm.cs          主窗体
│   ├── StudentEditDialog.cs 编辑对话框
│   └── ChartForm.cs         图表窗体
├── Data/            数据库文件（运行时生成）
├── Project_Proposal.md   项目提案
├── Testing_Report.md     测试报告
└── Program.cs            入口
```

## 运行

```bash
dotnet run
```

## 文档

- [项目提案](./Project_Proposal.md)
- [测试报告](./Testing_Report.md)
