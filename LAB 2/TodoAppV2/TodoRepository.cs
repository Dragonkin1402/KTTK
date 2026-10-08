using Dapper;
using Microsoft.Data.SqlClient;

namespace TodoAppV2;

// Tầng DATA: làm việc trực tiếp với SQL Server bằng Dapper
public class TodoRepository
{
    private readonly string _connectionString;

    public TodoRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqlConnection ConnectDB() => new SqlConnection(_connectionString);

    // Tự tạo database TodoDB và bảng Todos nếu chưa có
    public async Task InitializeAsync()
    {
        var builder = new SqlConnectionStringBuilder(_connectionString);
        string dbName = builder.InitialCatalog;
        builder.InitialCatalog = "master";

        using (var master = new SqlConnection(builder.ConnectionString))
        {
            await master.ExecuteAsync(@"
                DECLARE @sql NVARCHAR(300) = N'CREATE DATABASE ' + QUOTENAME(@name);
                IF DB_ID(@name) IS NULL EXEC(@sql);", new { name = dbName });
        }

        using (var database = ConnectDB())
        {
            await database.ExecuteAsync(@"
                IF OBJECT_ID(N'dbo.Todos', N'U') IS NULL
                CREATE TABLE dbo.Todos (
                    Id          INT IDENTITY(1,1) PRIMARY KEY,
                    Title       NVARCHAR(200) NOT NULL,
                    IsCompleted BIT NOT NULL DEFAULT 0
                );");
        }
    }

    public async Task<List<Todo>> GetAllAsync()
    {
        using (var database = ConnectDB())
        {
            var result = await database.QueryAsync<Todo>("SELECT * FROM Todos ORDER BY Id");
            return result.ToList();
        }
    }

    public async Task<Todo?> GetAsync(int id)
    {
        using (var database = ConnectDB())
        {
            return await database.QueryFirstOrDefaultAsync<Todo>(
                "SELECT * FROM Todos WHERE Id = @id", new { id });
        }
    }

    // Trả về Id vừa được tạo
    public async Task<int> AddAsync(string title)
    {
        using (var database = ConnectDB())
        {
            string query = @"INSERT INTO Todos (Title) OUTPUT INSERTED.Id VALUES (@Title)";
            return await database.ExecuteScalarAsync<int>(query, new { Title = title });
        }
    }

    public async Task<bool> UpdateAsync(Todo todo)
    {
        using (var database = ConnectDB())
        {
            string query = @"UPDATE Todos
                             SET Title = @Title,
                                 IsCompleted = @IsCompleted
                             WHERE Id = @Id";
            int rowsAffected = await database.ExecuteAsync(query, todo);
            return rowsAffected > 0;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using (var database = ConnectDB())
        {
            string query = @"DELETE FROM Todos WHERE Id = @Id";
            int rowsAffected = await database.ExecuteAsync(query, new { Id = id });
            return rowsAffected > 0;
        }
    }
}
