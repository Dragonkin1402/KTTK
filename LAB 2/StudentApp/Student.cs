using System.Globalization;

namespace StudentApp;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";
    public int Age { get; set; }
    public double Grade { get; set; }

    public override string ToString()
        => $"{Id,-4} {Name,-22} {Email,-26} {Address,-18} {Age,-4} {Grade,-5:0.0#}";

    // Định dạng lưu file: Id|Name|Email|Address|Age|Grade
    public string ToFileString()
        => string.Join("|", Id, Name, Email, Address, Age,
                       Grade.ToString(CultureInfo.InvariantCulture));

    public static Student FromFileString(string line)
    {
        var p = line.Split('|');
        return new Student
        {
            Id = int.Parse(p[0]),
            Name = p[1],
            Email = p[2],
            Address = p[3],
            Age = int.Parse(p[4]),
            Grade = double.Parse(p[5], CultureInfo.InvariantCulture)
        };
    }
}
