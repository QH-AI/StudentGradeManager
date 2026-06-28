using System.Data;
using System.Data.SQLite;

namespace StudentGradeManager.DAL
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string dbPath)
        {
            _connectionString = $"Data Source={dbPath};Version=3;";
        }

        /// <summary>
        /// 初始化数据库，创建学生表
        /// </summary>
        public void Initialize()
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Students (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId TEXT NOT NULL UNIQUE,
                    Name TEXT NOT NULL,
                    Grade TEXT DEFAULT '',
                    MathScore REAL DEFAULT 0,
                    EnglishScore REAL DEFAULT 0,
                    ProgrammingScore REAL DEFAULT 0,
                    CreatedAt TEXT DEFAULT (datetime('now','localtime')),
                    UpdatedAt TEXT DEFAULT (datetime('now','localtime'))
                );";
            cmd.ExecuteNonQuery();

            // 启用 WAL 模式提升并发性能
            cmd.CommandText = "PRAGMA journal_mode=WAL;";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 执行查询，返回DataTable（用于DataGridView绑定）
        /// </summary>
        public DataTable ExecuteQuery(string sql, params SQLiteParameter[] parameters)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            using var adapter = new SQLiteDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        /// <summary>
        /// 执行非查询SQL（INSERT/UPDATE/DELETE），返回影响行数
        /// </summary>
        public int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 执行标量查询（COUNT/MAX等）
        /// </summary>
        public object ExecuteScalar(string sql, params SQLiteParameter[] parameters)
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }
    }
}
