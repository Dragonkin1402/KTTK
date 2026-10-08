namespace StudentApp;

// Tầng DATA: chỉ lo đọc/ghi file và thao tác trên danh sách
public class StudentRepository
{
    private readonly List<Student> _students = new();
    private readonly string _filePath;
    private int _nextId = 1;

    public StudentRepository(string filePath = "students.txt")
    {
        _filePath = filePath;
        LoadFromFile();
    }

    public List<Student> GetAll() => _students;

    public Student? GetById(int id) => _students.FirstOrDefault(s => s.Id == id);

    public Student Add(Student student)
    {
        student.Id = _nextId++;
        _students.Add(student);
        SaveToFile();
        return student;
    }

    public bool Update(Student updated)
    {
        var s = GetById(updated.Id);
        if (s == null) return false;

        s.Name = updated.Name;
        s.Email = updated.Email;
        s.Address = updated.Address;
        s.Age = updated.Age;
        s.Grade = updated.Grade;
        SaveToFile();
        return true;
    }

    public bool Delete(int id)
    {
        var s = GetById(id);
        if (s == null) return false;

        _students.Remove(s);
        SaveToFile();
        return true;
    }

    private void LoadFromFile()
    {
        if (!File.Exists(_filePath)) return;

        foreach (var line in File.ReadAllLines(_filePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                var s = Student.FromFileString(line);
                _students.Add(s);
                if (s.Id >= _nextId) _nextId = s.Id + 1;
            }
            catch (Exception)
            {
                // Bỏ qua dòng bị lỗi định dạng để chương trình không crash
            }
        }
    }

    private void SaveToFile()
        => File.WriteAllLines(_filePath, _students.Select(s => s.ToFileString()));
}
