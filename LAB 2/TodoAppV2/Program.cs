using System.Text;
using TodoAppV2;

public class Program
{
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        // Đọc chuỗi kết nối từ biến môi trường TODO_DB; nếu không có thì dùng mặc định (SQL Server chạy bằng Docker)
        string connectionString =
            Environment.GetEnvironmentVariable("TODO_DB")
            ?? "Server=localhost,1433;Database=TodoDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true";

        var repository = new TodoRepository(connectionString);

        try
        {
            await repository.InitializeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Không kết nối được SQL Server:");
            Console.WriteLine(ex.Message);
            Console.WriteLine("\nKiểm tra: container Docker đã chạy chưa? Mật khẩu/chuỗi kết nối đúng chưa? (xem README_LAB2.md)");
            return;
        }

        ITodoService service = new TodoService(repository);
        var ui = new TodoUI(service);
        await ui.Run();
    }
}
