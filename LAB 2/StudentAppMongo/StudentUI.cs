using System.Globalization;

namespace StudentAppMongo;

// Tầng UI
public class StudentUI
{
    private readonly StudentService _service;

    public StudentUI(StudentService service)
    {
        _service = service;
    }

    public async Task Run()
    {
        while (true)
        {
            Console.Clear();
            ShowMenu();

            string? choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1": PrintTable(await _service.GetAllAsync()); break;
                case "2": await AddStudent(); break;
                case "3": await EditStudent(); break;
                case "4": await DeleteStudent(); break;
                case "5": await SearchById(); break;
                case "6": PrintTable(await _service.SearchByNameAsync(Prompt("Nhập tên cần tìm: "))); break;
                case "7": PrintTable(await _service.SearchByAddressAsync(Prompt("Nhập địa chỉ cần tìm: "))); break;
                case "8": await SearchByGrade(); break;
                case "0": return;
                default: Console.WriteLine("Lựa chọn không hợp lệ!"); break;
            }

            Console.WriteLine("\nNhấn Enter để tiếp tục...");
            Console.ReadLine();
        }
    }

    private static void ShowMenu()
    {
        Console.WriteLine("===== QUẢN LÝ SINH VIÊN (MongoDB) =====");
        Console.WriteLine("1. Hiển thị danh sách");
        Console.WriteLine("2. Thêm sinh viên");
        Console.WriteLine("3. Sửa sinh viên");
        Console.WriteLine("4. Xoá sinh viên");
        Console.WriteLine("5. Tìm theo Id");
        Console.WriteLine("6. Tìm theo Tên");
        Console.WriteLine("7. Tìm theo Địa chỉ");
        Console.WriteLine("8. Tìm theo Điểm (Grade)");
        Console.WriteLine("0. Thoát");
        Console.Write("Chọn: ");
    }

    private async Task AddStudent()
    {
        Console.WriteLine("--- Thêm sinh viên ---");
        string name = Prompt("Họ tên: ");
        string email = Prompt("Email: ");
        string address = Prompt("Địa chỉ: ");

        if (!TryReadInt("Tuổi: ", out int age)) return;
        if (!TryReadDouble("Điểm (0-10): ", out double grade)) return;

        var (_, message) = await _service.AddAsync(name, email, address, age, grade);
        Console.WriteLine(message);
    }

    private async Task EditStudent()
    {
        Console.WriteLine("--- Sửa sinh viên (Enter để giữ nguyên giá trị cũ) ---");
        string id = Prompt("Nhập Id cần sửa: ");

        var s = await _service.FindByIdAsync(id);
        if (s == null)
        {
            Console.WriteLine("Không tìm thấy sinh viên.");
            return;
        }

        string name = PromptKeep($"Họ tên [{s.Name}]: ", s.Name);
        string email = PromptKeep($"Email [{s.Email}]: ", s.Email);
        string address = PromptKeep($"Địa chỉ [{s.Address}]: ", s.Address);

        string ageText = Prompt($"Tuổi [{s.Age}]: ");
        int age = s.Age;
        if (ageText != "" && !int.TryParse(ageText, out age))
        {
            Console.WriteLine("Tuổi không hợp lệ.");
            return;
        }

        string gradeText = Prompt($"Điểm [{s.Grade}]: ");
        double grade = s.Grade;
        if (gradeText != "" && !TryParseDouble(gradeText, out grade))
        {
            Console.WriteLine("Điểm không hợp lệ.");
            return;
        }

        var (_, message) = await _service.UpdateAsync(id, name, email, address, age, grade);
        Console.WriteLine(message);
    }

    private async Task DeleteStudent()
    {
        string id = Prompt("Nhập Id cần xoá: ");

        var s = await _service.FindByIdAsync(id);
        if (s == null)
        {
            Console.WriteLine("Không tìm thấy sinh viên.");
            return;
        }

        Console.Write($"Xoá '{s.Name}'? (y/n): ");
        if (Console.ReadLine()?.Trim().ToLower() != "y")
        {
            Console.WriteLine("Đã huỷ.");
            return;
        }

        var (_, message) = await _service.DeleteAsync(id);
        Console.WriteLine(message);
    }

    private async Task SearchById()
    {
        var s = await _service.FindByIdAsync(Prompt("Nhập Id: "));
        PrintTable(s == null ? new List<Student>() : new List<Student> { s });
    }

    private async Task SearchByGrade()
    {
        if (!TryReadDouble("Nhập điểm cần tìm: ", out double grade)) return;
        PrintTable(await _service.SearchByGradeAsync(grade));
    }

    // ----- Hàm hỗ trợ nhập/xuất -----
    private static void PrintTable(List<Student> list)
    {
        Console.WriteLine($"{"Id",-25}{"Họ tên",-22} {"Email",-26} {"Địa chỉ",-16} {"Tuổi",-4} {"Điểm",-5}");
        Console.WriteLine(new string('-', 104));

        if (list.Count == 0)
        {
            Console.WriteLine("(Không có dữ liệu)");
            return;
        }

        foreach (var s in list)
            Console.WriteLine(s);

        Console.WriteLine($"\nTổng: {list.Count} sinh viên");
    }

    private static string Prompt(string label)
    {
        Console.Write(label);
        return Console.ReadLine()?.Trim() ?? "";
    }

    private static string PromptKeep(string label, string oldValue)
    {
        string input = Prompt(label);
        return input == "" ? oldValue : input;
    }

    private static bool TryReadInt(string label, out int value)
    {
        if (int.TryParse(Prompt(label), out value)) return true;
        Console.WriteLine("Giá trị không hợp lệ.");
        return false;
    }

    private static bool TryReadDouble(string label, out double value)
    {
        if (TryParseDouble(Prompt(label), out value)) return true;
        Console.WriteLine("Giá trị không hợp lệ.");
        return false;
    }

    // Chấp nhận cả "8.5" và "8,5"
    private static bool TryParseDouble(string text, out double value)
        => double.TryParse(text.Replace(',', '.'), NumberStyles.Float,
                           CultureInfo.InvariantCulture, out value);
}
