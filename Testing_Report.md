# 测试报告：学生成绩管理系统 (Student Grade Manager)

## 测试环境

| 项目 | 详情 |
|------|------|
| 操作系统 | Windows 11 |
| .NET 版本 | .NET 9.0 |
| 测试框架 | xUnit 2.x |
| 数据库 | SQLite（内存模式测试 / 文件模式集成测试） |
| 开发工具 | VS Code / Claude Code AI辅助 |

## TDD 实践过程

### 红阶段（Red）—— 先写失败的测试

#### Test 1: `AddStudent_WithValidData_ShouldReturnSuccess`
- **输入**：合法的学生数据对象
- **预期**：返回 `(success=true, message="添加成功")`
- **结果（红）**：方法未实现，编译失败 ✓

#### Test 2: `AddStudent_WithEmptyStudentId_ShouldReturnError`
- **输入**：`StudentId=""` 的学生对象
- **预期**：返回 `(success=false)`
- **结果（红）**：方法未实现，编译失败 ✓

### 绿阶段（Green）—— 写最少代码让测试通过

实现了 `StudentService.AddStudent()` 和 `DatabaseHelper.ExecuteNonQuery()` 方法。

两个测试均通过 ✓

### 重构阶段（Refactor）

- 提取 `CalcAverage()` 辅助方法，消除重复计算
- 使用 switch 表达式替代 if-else 链（`CalcGradeLevel`）
- 添加参数化查询替代字符串拼接（防止SQL注入）
- 添加正则表达式验证学号格式

## 测试用例与结果

| 编号 | 测试用例 | 测试方法 | 输入 | 预期输出 | 实际结果 |
|------|----------|----------|------|----------|----------|
| T01 | 正常添加学生 | AddStudent | 合法学生对象 | success=true | ✓ 通过 |
| T02 | 空学号验证 | AddStudent | StudentId="" | success=false | ✓ 通过 |
| T03 | 学号格式验证 | AddStudent | StudentId="abc" | success=false | ✓ 通过 |
| T04 | 重复学号处理 | AddStudent | 已存在的学号 | success=false | ✓ 通过 |
| T05 | 空姓名验证 | AddStudent | Name="" | success=false | ✓ 通过 |
| T06 | 成绩统计计算 | GetGradeStatistics | 3个班级各5人 | 正确统计数据 | ✓ 通过 |
| T07 | 搜索功能 | SearchStudents | keyword="张三" | 返回匹配记录 | ✓ 通过 |
| T08 | CSV导入 | ImportCsv | 10条合法记录 | imported=10 | ✓ 通过 |
| T09 | 成绩等级计算 | CalcGradeLevel | avg=85 | "良好" | ✓ 通过 |
| T10 | 边界值—满分 | CalcGradeLevel | avg=100 | "优秀" | ✓ 通过 |
| T11 | 边界值—零分 | CalcGradeLevel | avg=0 | "不及格" | ✓ 通过 |
| T12 | 空列表统计 | GetGradeStatistics | 数据库无记录 | 返回空DataTable | ✓ 通过 |
| T13 | 更新学生 | UpdateStudent | 修改后的学生对象 | success=true | ✓ 通过 |
| T14 | 删除学生 | DeleteStudent | 有效ID | success=true | ✓ 通过 |
| T15 | 删除不存在学生 | DeleteStudent | 无效ID | success=false | ✓ 通过 |

**测试通过率：15/15 = 100%**

## 关键测试代码示例

```csharp
[Fact]
public void AddStudent_WithValidData_ShouldReturnSuccess()
{
    // Arrange
    var db = new DatabaseHelper("Data Source=:memory:;Version=3;");
    db.Initialize();
    var service = new StudentService(db);
    var student = new Student
    {
        StudentId = "20242401001",
        Name = "测试学生",
        Grade = "软件工程2401班",
        MathScore = 85,
        EnglishScore = 90,
        ProgrammingScore = 88
    };

    // Act
    var (success, message) = service.AddStudent(student);

    // Assert
    Assert.True(success);
    Assert.Equal("添加成功", message);
}

[Fact]
public void AddStudent_WithEmptyStudentId_ShouldReturnError()
{
    // Arrange
    var db = new DatabaseHelper("Data Source=:memory:;Version=3;");
    db.Initialize();
    var service = new StudentService(db);
    var student = new Student { StudentId = "", Name = "测试" };

    // Act
    var (success, message) = service.AddStudent(student);

    // Assert
    Assert.False(success);
}

[Theory]
[InlineData(100, "优秀")]
[InlineData(85, "良好")]
[InlineData(75, "中等")]
[InlineData(65, "及格")]
[InlineData(45, "不及格")]
[InlineData(0, "不及格")]
public void CalcGradeLevel_WithVariousScores_ReturnsCorrectLevel(float avg, string expected)
{
    var student = new Student
    {
        MathScore = avg,
        EnglishScore = avg,
        ProgrammingScore = avg
    };
    
    Assert.Equal(expected, student.GradeLevel);
}
```

## 测试总结

1. **TDD让代码设计更清晰**：先定义接口期望行为再编码，避免了"写完再改"的返工
2. **单元测试提供安全网**：重构阶段可以放心修改代码，测试会立即反馈是否破坏已有功能
3. **参数化测试减少重复**：使用 `[Theory]` + `[InlineData]` 覆盖边界值，代码量少覆盖面广
4. **AI辅助测试编写**：Claude Code能快速生成测试骨架，但测试场景设计和边界值识别仍需人工主导
