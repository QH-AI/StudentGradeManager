using System.Data;
using StudentGradeManager.BLL;
using StudentGradeManager.DAL;
using StudentGradeManager.Models;

namespace StudentGradeManager.Tests;

public class StudentServiceTests : IDisposable
{
    private readonly DatabaseHelper _db;
    private readonly StudentService _service;
    private readonly string _dbPath;

    public StudentServiceTests()
    {
        // 使用临时文件数据库进行测试（非内存模式，因为SQLite内存模式在连接关闭后丢失表）
        _dbPath = Path.Combine(Path.GetTempPath(), $"test_students_{Guid.NewGuid()}.db");
        _db = new DatabaseHelper(_dbPath);
        _db.Initialize();
        _service = new StudentService(_db);
    }

    public void Dispose()
    {
        try { File.Delete(_dbPath); } catch { }
    }

    // ============================================================
    // T01-T05: AddStudent 测试
    // ============================================================

    [Fact]
    public void AddStudent_WithValidData_ReturnsSuccess()
    {
        var student = new Student
        {
            StudentId = "20242401001",
            Name = "测试学生",
            Grade = "软件工程2401班",
            MathScore = 85,
            EnglishScore = 90,
            ProgrammingScore = 88
        };

        var (success, message) = _service.AddStudent(student);

        Assert.True(success);
        Assert.Equal("添加成功", message);
    }

    [Fact]
    public void AddStudent_WithEmptyStudentId_ReturnsError()
    {
        var student = new Student { StudentId = "", Name = "测试" };

        var (success, _) = _service.AddStudent(student);

        Assert.False(success);
    }

    [Fact]
    public void AddStudent_WithEmptyName_ReturnsError()
    {
        var student = new Student { StudentId = "20242401001", Name = "" };

        var (success, _) = _service.AddStudent(student);

        Assert.False(success);
    }

    [Fact]
    public void AddStudent_WithInvalidStudentIdFormat_ReturnsError()
    {
        var student = new Student { StudentId = "abc123", Name = "测试" };

        var (success, _) = _service.AddStudent(student);

        Assert.False(success);
    }

    [Fact]
    public void AddStudent_WithDuplicateStudentId_ReturnsError()
    {
        var student1 = new Student { StudentId = "20242401001", Name = "张三", Grade = "2401", MathScore = 80, EnglishScore = 80, ProgrammingScore = 80 };
        _service.AddStudent(student1);

        var student2 = new Student { StudentId = "20242401001", Name = "李四", Grade = "2401", MathScore = 90, EnglishScore = 90, ProgrammingScore = 90 };

        var (success, message) = _service.AddStudent(student2);

        Assert.False(success);
        Assert.Contains("已存在", message);
    }

    // ============================================================
    // T06: 搜索功能测试
    // ============================================================

    [Fact]
    public void SearchStudents_WithKeyword_ReturnsMatchingRecords()
    {
        _service.AddStudent(new Student { StudentId = "20242401001", Name = "张三丰", Grade = "2401", MathScore = 85, EnglishScore = 90, ProgrammingScore = 88 });
        _service.AddStudent(new Student { StudentId = "20242401002", Name = "李四", Grade = "2401", MathScore = 75, EnglishScore = 80, ProgrammingScore = 78 });
        _service.AddStudent(new Student { StudentId = "20242402001", Name = "张三", Grade = "2402", MathScore = 90, EnglishScore = 92, ProgrammingScore = 95 });

        var result = _service.SearchStudents("张三", "");

        Assert.Equal(2, result.Rows.Count); // 张三丰 和 张三
    }

    [Fact]
    public void SearchStudents_WithGradeFilter_ReturnsFilteredRecords()
    {
        _service.AddStudent(new Student { StudentId = "20242401001", Name = "测试1", Grade = "2401", MathScore = 80, EnglishScore = 80, ProgrammingScore = 80 });
        _service.AddStudent(new Student { StudentId = "20242402001", Name = "测试2", Grade = "2402", MathScore = 85, EnglishScore = 85, ProgrammingScore = 85 });

        var result = _service.SearchStudents("", "2401");

        Assert.Equal(1, result.Rows.Count);
    }

    // ============================================================
    // T07-T09: 成绩等级计算
    // ============================================================

    [Theory]
    [InlineData(100, 100, 100, "优秀")]
    [InlineData(95, 90, 88, "优秀")]
    [InlineData(88, 85, 82, "良好")]
    [InlineData(80, 80, 80, "良好")]
    [InlineData(75, 72, 78, "中等")]
    [InlineData(65, 68, 62, "及格")]
    [InlineData(60, 60, 60, "及格")]
    [InlineData(55, 50, 45, "不及格")]
    [InlineData(30, 40, 20, "不及格")]
    [InlineData(0, 0, 0, "不及格")]
    public void Student_GradeLevel_CalculatesCorrectly(float math, float eng, float prog, string expected)
    {
        var student = new Student
        {
            StudentId = "20242401001",
            Name = "测试",
            MathScore = math,
            EnglishScore = eng,
            ProgrammingScore = prog
        };

        Assert.Equal(expected, student.GradeLevel);
    }

    [Fact]
    public void Student_AverageScore_CalculatesCorrectly()
    {
        var student = new Student
        {
            MathScore = 90,
            EnglishScore = 80,
            ProgrammingScore = 70
        };

        Assert.Equal(80.0f, student.AverageScore, 1);
    }

    // ============================================================
    // T10: 成绩统计测试
    // ============================================================

    [Fact]
    public void GetGradeStatistics_WithMultipleGrades_ReturnsCorrectStats()
    {
        _service.AddStudent(new Student { StudentId = "20242401001", Name = "A", Grade = "2401", MathScore = 90, EnglishScore = 90, ProgrammingScore = 90 });
        _service.AddStudent(new Student { StudentId = "20242401002", Name = "B", Grade = "2401", MathScore = 60, EnglishScore = 60, ProgrammingScore = 60 });
        _service.AddStudent(new Student { StudentId = "20242402001", Name = "C", Grade = "2402", MathScore = 100, EnglishScore = 100, ProgrammingScore = 100 });

        var stats = _service.GetGradeStatistics();

        Assert.Equal(2, stats.Rows.Count); // 两个班级

        // 2401班: 平均分75, 最高分90, 及格率100%
        var row2401 = stats.Select("班级 = '2401'")[0];
        Assert.Equal(2, Convert.ToInt32(row2401["人数"]));
        Assert.Equal(75.0, Convert.ToDouble(row2401["平均分"]), 1);
        Assert.Equal(100.0, Convert.ToDouble(row2401["及格率"]), 1);

        // 2402班: 平均分100, 最高分100, 及格率100%
        var row2402 = stats.Select("班级 = '2402'")[0];
        Assert.Equal(1, Convert.ToInt32(row2402["人数"]));
        Assert.Equal(100.0, Convert.ToDouble(row2402["平均分"]), 1);
    }

    [Fact]
    public void GetGradeStatistics_WithEmptyDatabase_ReturnsEmptyTable()
    {
        var stats = _service.GetGradeStatistics();

        Assert.NotNull(stats);
        Assert.Equal(0, stats.Rows.Count);
    }

    // ============================================================
    // T11-T12: 更新学生
    // ============================================================

    [Fact]
    public void UpdateStudent_WithValidData_ReturnsSuccess()
    {
        _service.AddStudent(new Student { StudentId = "20242401001", Name = "原名", Grade = "2401", MathScore = 70, EnglishScore = 70, ProgrammingScore = 70 });

        // 获取已插入学生的ID
        var data = _service.SearchStudents("20242401001", "");
        int id = Convert.ToInt32(data.Rows[0]["Id"]);

        var updated = new Student
        {
            Id = id,
            StudentId = "20242401001",
            Name = "新名",
            Grade = "2402",
            MathScore = 90,
            EnglishScore = 90,
            ProgrammingScore = 90
        };

        var (success, message) = _service.UpdateStudent(updated);

        Assert.True(success);
        Assert.Equal("更新成功", message);

        // 验证更新后的数据
        var result = _service.SearchStudents("20242401001", "");
        Assert.Equal("新名", result.Rows[0]["Name"].ToString());
    }

    [Fact]
    public void UpdateStudent_WithInvalidId_ReturnsError()
    {
        var student = new Student
        {
            Id = 99999,
            StudentId = "20242401001",
            Name = "不存在"
        };

        var (success, _) = _service.UpdateStudent(student);

        Assert.False(success);
    }

    // ============================================================
    // T13-T14: 删除学生
    // ============================================================

    [Fact]
    public void DeleteStudent_WithValidId_ReturnsSuccess()
    {
        _service.AddStudent(new Student { StudentId = "20242401001", Name = "待删除", Grade = "2401", MathScore = 70, EnglishScore = 70, ProgrammingScore = 70 });
        var data = _service.SearchStudents("20242401001", "");
        int id = Convert.ToInt32(data.Rows[0]["Id"]);

        var (success, message) = _service.DeleteStudent(id);

        Assert.True(success);
        Assert.Equal("删除成功", message);

        // 验证已删除
        var result = _service.SearchStudents("20242401001", "");
        Assert.Equal(0, result.Rows.Count);
    }

    [Fact]
    public void DeleteStudent_WithInvalidId_ReturnsError()
    {
        var (success, _) = _service.DeleteStudent(99999);

        Assert.False(success);
    }

    // ============================================================
    // T15: 成绩分布统计
    // ============================================================

    [Fact]
    public void GetScoreDistribution_ReturnsCorrectDistribution()
    {
        _service.AddStudent(new Student { StudentId = "20242401001", Name = "优秀生", Grade = "2401", MathScore = 95, EnglishScore = 95, ProgrammingScore = 95 });
        _service.AddStudent(new Student { StudentId = "20242401002", Name = "良好生", Grade = "2401", MathScore = 85, EnglishScore = 85, ProgrammingScore = 85 });
        _service.AddStudent(new Student { StudentId = "20242401003", Name = "不及格", Grade = "2401", MathScore = 45, EnglishScore = 45, ProgrammingScore = 45 });

        var dist = _service.GetScoreDistribution();

        Assert.Equal(1, dist["优秀(>=90)"]);
        Assert.Equal(1, dist["良好(80-89)"]);
        Assert.Equal(0, dist["中等(70-79)"]);
        Assert.Equal(0, dist["及格(60-69)"]);
        Assert.Equal(1, dist["不及格(<60)"]);
    }

    // ============================================================
    // CSV导入测试
    // ============================================================

    [Fact]
    public void ImportCsv_WithValidFile_ImportsCorrectly()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"test_import_{Guid.NewGuid()}.csv");
        File.WriteAllText(tempFile, "学号,姓名,班级,数学,英语,程序设计\n20242401001,张三,2401,85,90,88\n20242401002,李四,2401,75,80,78\n", System.Text.Encoding.UTF8);

        try
        {
            var (imported, skipped) = _service.ImportCsv(tempFile);

            Assert.Equal(2, imported);
            Assert.Equal(0, skipped);

            var data = _service.SearchStudents("", "");
            Assert.Equal(2, data.Rows.Count);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ============================================================
    // 班级列表测试
    // ============================================================

    [Fact]
    public void GetGrades_ReturnsDistinctGrades()
    {
        _service.AddStudent(new Student { StudentId = "20242401001", Name = "A", Grade = "2401", MathScore = 80, EnglishScore = 80, ProgrammingScore = 80 });
        _service.AddStudent(new Student { StudentId = "20242401002", Name = "B", Grade = "2401", MathScore = 80, EnglishScore = 80, ProgrammingScore = 80 });
        _service.AddStudent(new Student { StudentId = "20242402001", Name = "C", Grade = "2402", MathScore = 80, EnglishScore = 80, ProgrammingScore = 80 });

        var grades = _service.GetGrades();

        Assert.Equal(2, grades.Count);
        Assert.Contains("2401", grades);
        Assert.Contains("2402", grades);
    }
}
