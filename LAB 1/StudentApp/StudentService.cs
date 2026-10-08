using System.Text.RegularExpressions;

namespace StudentApp;

// Tầng LOGIC: kiểm tra dữ liệu hợp lệ + tìm kiếm
public class StudentService
{
    private readonly StudentRepository _repo = new();

    public List<Student> GetAll() => _repo.GetAll();

    public (bool Success, string Message) Add(
        string name, string email, string address, int age, double grade)
    {
        var error = Validate(name, email, address, age, grade);
        if (error != null) return (false, error);

        _repo.Add(new Student
        {
            Name = name.Trim(),
            Email = email.Trim(),
            Address = address.Trim(),
            Age = age,
            Grade = grade
        });
        return (true, "Thêm sinh viên thành công.");
    }

    public (bool Success, string Message) Update(
        int id, string name, string email, string address, int age, double grade)
    {
        var error = Validate(name, email, address, age, grade);
        if (error != null) return (false, error);

        bool ok = _repo.Update(new Student
        {
            Id = id,
            Name = name.Trim(),
            Email = email.Trim(),
            Address = address.Trim(),
            Age = age,
            Grade = grade
        });
        return ok ? (true, "Cập nhật thành công.")
                  : (false, "Không tìm thấy sinh viên.");
    }

    public (bool Success, string Message) Delete(int id)
        => _repo.Delete(id) ? (true, "Xoá thành công.")
                            : (false, "Không tìm thấy sinh viên.");

    // ----- Tìm kiếm -----
    public Student? FindById(int id) => _repo.GetById(id);

    public List<Student> SearchByName(string keyword)
        => _repo.GetAll()
                .Where(s => s.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

    public List<Student> SearchByAddress(string keyword)
        => _repo.GetAll()
                .Where(s => s.Address.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

    public List<Student> SearchByGrade(double grade)
        => _repo.GetAll()
                .Where(s => Math.Abs(s.Grade - grade) < 0.0001)
                .ToList();

    // ----- Kiểm tra dữ liệu -----
    private static string? Validate(string name, string email, string address, int age, double grade)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Tên không được để trống.";
        if (string.IsNullOrWhiteSpace(address)) return "Địa chỉ không được để trống.";
        if (!Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) return "Email không hợp lệ.";
        if (age < 1 || age > 120) return "Tuổi phải trong khoảng 1 - 120.";
        if (grade < 0 || grade > 10) return "Điểm phải trong khoảng 0 - 10.";
        if (name.Contains('|') || email.Contains('|') || address.Contains('|'))
            return "Không được dùng ký tự '|' (dùng làm dấu phân cách khi lưu file).";
        return null;
    }
}
