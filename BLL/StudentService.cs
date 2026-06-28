using System.Data;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using StudentGradeManager.DAL;
using StudentGradeManager.Models;

namespace StudentGradeManager.BLL
{
    public class StudentService
    {
        private readonly DatabaseHelper _db;

        public StudentService(DatabaseHelper db)
        {
            _db = db;
        }

        /// <summary>
        /// 添加学生——带参数化查询和输入验证
        /// </summary>
        public (bool success, string message) AddStudent(Student s)
        {
            if (string.IsNullOrWhiteSpace(s.StudentId))
                return (false, "学号不能为空");
            if (string.IsNullOrWhiteSpace(s.Name))
                return (false, "姓名不能为空");
            if (!Regex.IsMatch(s.StudentId, @"^\d{8,12}$"))
                return (false, "学号格式不正确（需为8-12位数字）");

            try
            {
                _db.ExecuteNonQuery(
                    @"INSERT INTO Students (StudentId, Name, Grade, MathScore,
                      EnglishScore, ProgrammingScore)
                      VALUES (@sid, @name, @grade, @math, @eng, @prog)",
                    new SQLiteParameter("@sid", s.StudentId),
                    new SQLiteParameter("@name", s.Name),
                    new SQLiteParameter("@grade", s.Grade ?? ""),
                    new SQLiteParameter("@math", s.MathScore),
                    new SQLiteParameter("@eng", s.EnglishScore),
                    new SQLiteParameter("@prog", s.ProgrammingScore));
                return (true, "添加成功");
            }
            catch (SQLiteException ex) when (ex.Message.Contains("UNIQUE"))
            {
                return (false, "学号已存在，请勿重复添加");
            }
        }

        /// <summary>
        /// 更新学生信息
        /// </summary>
        public (bool success, string message) UpdateStudent(Student s)
        {
            if (s.Id <= 0)
                return (false, "无效的学生ID");
            if (string.IsNullOrWhiteSpace(s.StudentId))
                return (false, "学号不能为空");
            if (string.IsNullOrWhiteSpace(s.Name))
                return (false, "姓名不能为空");

            try
            {
                int rows = _db.ExecuteNonQuery(
                    @"UPDATE Students SET StudentId=@sid, Name=@name, Grade=@grade,
                      MathScore=@math, EnglishScore=@eng, ProgrammingScore=@prog,
                      UpdatedAt=datetime('now','localtime')
                      WHERE Id=@id",
                    new SQLiteParameter("@sid", s.StudentId),
                    new SQLiteParameter("@name", s.Name),
                    new SQLiteParameter("@grade", s.Grade ?? ""),
                    new SQLiteParameter("@math", s.MathScore),
                    new SQLiteParameter("@eng", s.EnglishScore),
                    new SQLiteParameter("@prog", s.ProgrammingScore),
                    new SQLiteParameter("@id", s.Id));
                return rows > 0
                    ? (true, "更新成功")
                    : (false, "未找到该学生记录");
            }
            catch (SQLiteException ex) when (ex.Message.Contains("UNIQUE"))
            {
                return (false, "学号与其他学生冲突");
            }
        }

        /// <summary>
        /// 删除学生
        /// </summary>
        public (bool success, string message) DeleteStudent(int id)
        {
            int rows = _db.ExecuteNonQuery(
                "DELETE FROM Students WHERE Id=@id",
                new SQLiteParameter("@id", id));
            return rows > 0
                ? (true, "删除成功")
                : (false, "未找到该学生记录");
        }

        /// <summary>
        /// 多条件搜索（参数化查询防SQL注入）
        /// </summary>
        public DataTable SearchStudents(string keyword, string grade)
        {
            string sql = "SELECT Id, StudentId, Name, Grade, MathScore, EnglishScore, ProgrammingScore, (MathScore+EnglishScore+ProgrammingScore)/3.0 AS AverageScore, CASE WHEN (MathScore+EnglishScore+ProgrammingScore)/3.0 >= 90 THEN '优秀' WHEN (MathScore+EnglishScore+ProgrammingScore)/3.0 >= 80 THEN '良好' WHEN (MathScore+EnglishScore+ProgrammingScore)/3.0 >= 70 THEN '中等' WHEN (MathScore+EnglishScore+ProgrammingScore)/3.0 >= 60 THEN '及格' ELSE '不及格' END AS GradeLevel, CreatedAt, UpdatedAt FROM Students WHERE 1=1";
            var parameters = new List<SQLiteParameter>();

            if (!string.IsNullOrEmpty(keyword))
            {
                sql += " AND (StudentId LIKE @kw OR Name LIKE @kw)";
                parameters.Add(new SQLiteParameter("@kw", $"%{keyword}%"));
            }
            if (!string.IsNullOrEmpty(grade))
            {
                sql += " AND Grade = @grade";
                parameters.Add(new SQLiteParameter("@grade", grade));
            }

            sql += " ORDER BY StudentId";
            return _db.ExecuteQuery(sql, parameters.ToArray());
        }

        /// <summary>
        /// 获取所有不重复的班级列表（用于筛选下拉框）
        /// </summary>
        public List<string> GetGrades()
        {
            var dt = _db.ExecuteQuery("SELECT DISTINCT Grade FROM Students WHERE Grade != '' ORDER BY Grade");
            return dt.AsEnumerable().Select(r => r["Grade"].ToString()!).ToList();
        }

        /// <summary>
        /// 统计分析：按班级分组
        /// </summary>
        public DataTable GetGradeStatistics()
        {
            return _db.ExecuteQuery(
                @"SELECT Grade AS 班级, COUNT(*) AS 人数,
                  ROUND(AVG((MathScore+EnglishScore+ProgrammingScore)/3.0),1) AS 平均分,
                  MAX((MathScore+EnglishScore+ProgrammingScore)/3.0) AS 最高分,
                  ROUND(100.0*SUM(CASE WHEN (MathScore+EnglishScore+ProgrammingScore)/3.0>=60 THEN 1 ELSE 0 END)/COUNT(*),1) AS 及格率
                  FROM Students GROUP BY Grade ORDER BY Grade");
        }

        /// <summary>
        /// 获取成绩分布数据（用于图表）
        /// </summary>
        public Dictionary<string, int> GetScoreDistribution()
        {
            var levels = new Dictionary<string, int>
            {
                { "优秀(>=90)", 0 },
                { "良好(80-89)", 0 },
                { "中等(70-79)", 0 },
                { "及格(60-69)", 0 },
                { "不及格(<60)", 0 }
            };

            var dt = _db.ExecuteQuery("SELECT (MathScore+EnglishScore+ProgrammingScore)/3.0 AS Avg FROM Students");
            foreach (DataRow row in dt.Rows)
            {
                double avg = Convert.ToDouble(row["Avg"]);
                if (avg >= 90) levels["优秀(>=90)"]++;
                else if (avg >= 80) levels["良好(80-89)"]++;
                else if (avg >= 70) levels["中等(70-79)"]++;
                else if (avg >= 60) levels["及格(60-69)"]++;
                else levels["不及格(<60)"]++;
            }
            return levels;
        }

        /// <summary>
        /// CSV批量导入
        /// </summary>
        public (int imported, int skipped) ImportCsv(string filePath)
        {
            int imported = 0, skipped = 0;
            foreach (var line in File.ReadAllLines(filePath, System.Text.Encoding.UTF8).Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length < 6) { skipped++; continue; }

                var student = new Student
                {
                    StudentId = parts[0].Trim(),
                    Name = parts[1].Trim(),
                    Grade = parts[2].Trim(),
                    MathScore = float.TryParse(parts[3], out float m) ? m : 0,
                    EnglishScore = float.TryParse(parts[4], out float e) ? e : 0,
                    ProgrammingScore = float.TryParse(parts[5], out float p) ? p : 0
                };

                var (success, _) = AddStudent(student);
                if (success) imported++; else skipped++;
            }
            return (imported, skipped);
        }

        /// <summary>
        /// 导出为CSV
        /// </summary>
        public void ExportCsv(string filePath, DataTable data)
        {
            using var sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
            // 写入 BOM 确保 Excel 正确识别编码
            sw.BaseStream.Write(new byte[] { 0xEF, 0xBB, 0xBF }, 0, 3);

            // 写入表头
            var headers = new[] { "学号", "姓名", "班级", "数学成绩", "英语成绩", "程序设计成绩", "平均分", "等级" };
            sw.WriteLine(string.Join(",", headers));

            // 写入数据
            foreach (DataRow row in data.Rows)
            {
                var values = new[]
                {
                    row["StudentId"].ToString(),
                    row["Name"].ToString(),
                    row["Grade"].ToString(),
                    row["MathScore"].ToString(),
                    row["EnglishScore"].ToString(),
                    row["ProgrammingScore"].ToString(),
                    string.Format("{0:F1}", row["AverageScore"]),
                    row["GradeLevel"].ToString()
                };
                sw.WriteLine(string.Join(",", values));
            }
        }
    }
}
