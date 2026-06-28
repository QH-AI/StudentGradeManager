# 项目提案：学生成绩管理系统 (Student Grade Manager)

## 1. 项目概述

开发一个基于 **C# WinForm + SQLite** 的桌面端学生成绩管理系统，实现学生信息的增删改查、成绩录入统计、数据导入导出等功能，帮助教师高效管理班级学生成绩数据。

## 2. 选题理由

- **贴近实际教学场景**：功能需求明确，有真实的业务逻辑
- **技术栈覆盖课程核心知识点**：WinForm控件、数据库(SQLite)、文件操作(CSV)、GDI+图表绘制、三层架构
- **难度适中**：有明确的迭代开发路径，既能体现技术深度，又能在有限时间内完成
- **现代UI实践**：参考掘金管理后台风格，实现深色侧边栏 + 白色卡片化内容区的现代桌面UI

## 3. 系统架构设计

采用**三层架构**（表现层 / 业务逻辑层 / 数据访问层）：

```
表示层 (WinForm UI)
    ↕
业务逻辑层 (BLL - StudentService)
    ↕
数据访问层 (DAL - DatabaseHelper)
    ↕
SQLite 数据库 (students.db)
```

### 项目结构

```
StudentGradeManager/
├── Models/                 # 数据模型
│   └── Student.cs          # 学生实体类
├── DAL/                    # 数据访问层
│   └── DatabaseHelper.cs   # SQLite数据库操作封装
├── BLL/                    # 业务逻辑层
│   └── StudentService.cs   # 学生业务逻辑（CRUD、统计、导入导出）
├── Forms/                  # 窗体
│   ├── MainForm.cs         # 主窗体（侧边栏导航+数据表格+统计卡片）
│   ├── StudentEditDialog.cs # 新增/编辑学生对话框
│   └── ChartForm.cs        # 成绩统计图表窗体（GDI+自绘）
├── Data/                   # 数据库文件目录
│   └── students.db         # SQLite数据库（运行时自动创建）
└── Program.cs              # 程序入口
```

## 4. 核心功能

- ✅ 学生信息CRUD（学号、姓名、班级、数学/英语/程序设计成绩）
- ✅ 多条件搜索与筛选（按学号/姓名模糊搜索 + 按班级筛选）
- ✅ 成绩统计分析（平均分、最高分、及格率，按班级分组）
- ✅ 顶部统计卡片（学生总数、平均成绩、及格率，实时联动筛选）
- ✅ 成绩分布图表（饼图、班级柱状图、等级分布柱状图，GDI+自绘）
- ✅ CSV数据导入/导出（BOM编码处理，兼容Excel）
- ✅ 输入验证（学号格式验证、参数化查询防SQL注入）
- ✅ Toast操作反馈（成功/失败底部通知）
- ✅ 响应式布局（窗口缩放自适应）

## 5. 开发方法

采用 **RDD（README Driven Development）+ TDD（Test Driven Development）** 模式：

- **RDD**：先编写本提案文档，明确需求、接口和架构
- **TDD**：对核心业务逻辑（StudentService）编写单元测试，遵循红-绿-重构循环

## 6. 技术选型说明

| 层次 | 技术 | 理由 |
|------|------|------|
| UI框架 | WinForm .NET 9.0 | 课程核心内容，成熟稳定，拖拽式开发效率高 |
| 数据库 | SQLite (System.Data.SQLite) | 免安装、轻量级、适合桌面单机应用 |
| 数据访问 | ADO.NET + 参数化查询 | 防SQL注入，直接高效的数据库操作 |
| 图表 | GDI+ Graphics自绘 | 课程核心知识点，无需额外依赖，灵活定制 |
| UI风格 | 自绘圆角控件 + 现代管理后台 | 参考掘金后台风格，深色侧边栏 + 白色卡片 |
| 测试 | xUnit | .NET主流测试框架，与Visual Studio深度集成 |

## 7. Git仓库地址

https://github.com/[username]/StudentGradeManager

## 8. 迭代计划

| Sprint | 内容 | 预计时间 |
|--------|------|----------|
| Sprint 1 | 搭建项目骨架，实现学生信息CRUD | 1天 |
| Sprint 2 | 添加搜索筛选、数据导入导出 | 1天 |
| Sprint 3 | 实现图表统计、UI美化、测试完善 | 1天 |
