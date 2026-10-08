namespace TodoAppV2;

public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsCompleted { get; set; }

    public override string ToString()
        => $"[{(IsCompleted ? "x" : " ")}] {Id}: {Title}";
}
