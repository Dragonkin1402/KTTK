using System.Text;
using StudentAppMongo;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        // Lấy chuỗi kết nối MongoDB Atlas từ biến môi trường MONGODB_URI.
        // Không ghi mật khẩu thẳng vào code để tránh lộ khi nộp bài / đẩy lên GitHub.
        string? uri = Environment.GetEnvironmentVariable("MONGODB_URI");
        if (string.IsNullOrWhiteSpace(uri))
        {
            Console.WriteLine("Chưa có biến môi trường MONGODB_URI.");
            Console.Write("Dán chuỗi kết nối mongodb+srv://... vào đây: ");
            uri = Console.ReadLine()?.Trim();
        }

        if (string.IsNullOrWhiteSpace(uri))
        {
            Console.WriteLine("Không có chuỗi kết nối, thoát.");
            return;
        }

        try
        {
            var repository = new StudentRepository(uri);
            Console.WriteLine("Đang kết nối MongoDB...");
            await repository.PingAsync();
            Console.WriteLine("Kết nối thành công!");

            var service = new StudentService(repository);
            var ui = new StudentUI(service);
            await ui.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Không kết nối được MongoDB:");
            Console.WriteLine(ex.Message);
            Console.WriteLine("\nKiểm tra: mật khẩu, Network Access (IP) trên Atlas, mạng Internet (xem README_LAB2.md).");
        }
    }
}
