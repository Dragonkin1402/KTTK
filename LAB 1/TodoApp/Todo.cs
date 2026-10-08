namespace TodoApp;

public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsCompleted { get; set; }

    public override string ToString()
        => $"[{(IsCompleted ? "x" : " ")}] {Id}: {Title}";

    public string ToFileString()
        => $"{Id}|{IsCompleted}|{Title}";

    public static Todo FromFileString(string line)
    {
        var parts = line.Split('|', 3); // 3 để Title chứa dấu | vẫn không lỗi
        return new Todo
        {
            Id = int.Parse(parts[0]),
            IsCompleted = bool.Parse(parts[1]),
            Title = parts[2]
        };
    }
}
