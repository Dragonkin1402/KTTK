using System.Text.RegularExpressions;
using MongoDB.Bson;

namespace StudentAppMongo;

// Tầng LOGIC: kiểm tra dữ liệu + điều phối
public class StudentService
{
    private readonly StudentRepository _repo;

    public StudentService(StudentRepository repo)
    {
        _repo = repo;
    }

    public Task<List<Student>> GetAllAsync() => _repo.GetAllAsync();

    public async Task<(bool Success, string Message)> AddAsync(
        string name, string email, string address, int age, double grade)
    {
        var error = Validate(name, email, address, age, grade);
        if (error != null) return (false, error);

        await _repo.AddAsync(new Student
        {
            Name = name.Trim(),
            Email = email.Trim(),
            Address = address.Trim(),
            Age = age,
            Grade = grade
        });
        return (true, "Thêm sinh viên thành công.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(
        string id, string name, string email, string address, int age, double grade)
    {
        if (!ObjectId.TryParse(id, out _)) return (false, "Id không hợp lệ.");

        var error = Validate(name, email, address, age, grade);
        if (error != null) return (false, error);

        bool ok = await _repo.UpdateAsync(new Student
        {
            Id = id,
            Name = name.Trim(),
            Email = email.Trim(),
            Address = address.Trim(),
            Age = age,
            Grade = grade
        });
        return ok ? (true, "Cập nhật thành công.") : (false, "Không tìm thấy sinh viên.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return (false, "Id không hợp lệ.");
        return await _repo.DeleteAsync(id)
            ? (true, "Xoá thành công.")
            : (false, "Không tìm thấy sinh viên.");
    }

    // ----- Tìm kiếm -----
    public async Task<Student?> FindByIdAsync(string id)
        => ObjectId.TryParse(id, out _) ? await _repo.GetByIdAsync(id) : null;

    public Task<List<Student>> SearchByNameAsync(string keyword) => _repo.SearchByNameAsync(keyword);
    public Task<List<Student>> SearchByAddressAsync(string keyword) => _repo.SearchByAddressAsync(keyword);
    public Task<List<Student>> SearchByGradeAsync(double grade) => _repo.SearchByGradeAsync(grade);

    // ----- Kiểm tra dữ liệu -----
    private static string? Validate(string name, string email, string address, int age, double grade)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Tên không được để trống.";
        if (string.IsNullOrWhiteSpace(address)) return "Địa chỉ không được để trống.";
        if (!Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) return "Email không hợp lệ.";
        if (age < 1 || age > 120) return "Tuổi phải trong khoảng 1 - 120.";
        if (grade < 0 || grade > 10) return "Điểm phải trong khoảng 0 - 10.";
        return null;
    }
}
