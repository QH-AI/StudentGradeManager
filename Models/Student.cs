namespace StudentGradeManager.Models
{
    public class Student
    {
        public int Id { get; set; }                      // 主键，自增
        public string StudentId { get; set; } = "";       // 学号
        public string Name { get; set; } = "";            // 姓名
        public string Grade { get; set; } = "";           // 年级/班级
        public float MathScore { get; set; }              // 数学成绩
        public float EnglishScore { get; set; }           // 英语成绩
        public float ProgrammingScore { get; set; }       // 程序设计成绩

        // 计算属性：平均分
        public float AverageScore => (MathScore + EnglishScore + ProgrammingScore) / 3f;

        // 计算属性：等级
        public string GradeLevel => AverageScore switch
        {
            >= 90 => "优秀",
            >= 80 => "良好",
            >= 70 => "中等",
            >= 60 => "及格",
            _ => "不及格"
        };

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
