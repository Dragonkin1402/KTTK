using System.Globalization;

namespace StudentApp;

// Tầng UI: chỉ nhập/xuất, không chứa logic nghiệp vụ
public class StudentUI
{
    private readonly StudentService _service = new();

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            ShowMenu();

            string? choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1": ShowAll(); break;
                case "2": AddStudent(); break;
                case "3": EditStudent(); break;
                case "4": DeleteStudent(); break;
                case "5": SearchById(); break;
                case "6": SearchByName(); break;
                case "7": SearchByAddress(); break;
                case "8": SearchByGrade(); break;
                case "0": return;
                default: Console.WriteLine("Lựa chọn không hợp lệ!"); break;
            }

            Console.WriteLine("\nNhấn Enter để tiếp tục...");
            Console.ReadLine();
        }
    }

    private static void ShowMenu()
    {
        Console.WriteLine("===== QUẢN LÝ SINH VIÊN =====");
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

    private void ShowAll() => PrintTable(_service.GetAll());

    private void AddStudent()
    {
        Console.WriteLine("--- Thêm sinh viên ---");
        string name = Prompt("Họ tên: ");
        string email = Prompt("Email: ");
        string address = Prompt("Địa chỉ: ");

        if (!TryReadInt("Tuổi: ", out int age)) return;
        if (!TryReadDouble("Điểm (0-10): ", out double grade)) return;

        var (_, message) = _service.Add(name, email, address, age, grade);
        Console.WriteLine(message);
    }

    private void EditStudent()
    {
        Console.WriteLine("--- Sửa sinh viên (Enter để giữ nguyên giá trị cũ) ---");
        if (!TryReadInt("Nhập Id cần sửa: ", out int id)) return;

        var s = _service.FindById(id);
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

        var (_, message) = _service.Update(id, name, email, address, age, grade);
        Console.WriteLine(message);
    }

    private void DeleteStudent()
    {
        if (!TryReadInt("Nhập Id cần xoá: ", out int id)) return;

        var s = _service.FindById(id);
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

        var (_, message) = _service.Delete(id);
        Console.WriteLine(message);
    }

    private void SearchById()
    {
        if (!TryReadInt("Nhập Id: ", out int id)) return;

        var s = _service.FindById(id);
        PrintTable(s == null ? new List<Student>() : new List<Student> { s });
    }

    private void SearchByName()
        => PrintTable(_service.SearchByName(Prompt("Nhập tên cần tìm: ")));

    private void SearchByAddress()
        => PrintTable(_service.SearchByAddress(Prompt("Nhập địa chỉ cần tìm: ")));

    private void SearchByGrade()
    {
        if (!TryReadDouble("Nhập điểm cần tìm: ", out double grade)) return;
        PrintTable(_service.SearchByGrade(grade));
    }

    // ----- Hàm hỗ trợ nhập/xuất -----
    private static void PrintTable(List<Student> list)
    {
        Console.WriteLine($"{"Id",-4} {"Họ tên",-22} {"Email",-26} {"Địa chỉ",-18} {"Tuổi",-4} {"Điểm",-5}");
        Console.WriteLine(new string('-', 84));

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
