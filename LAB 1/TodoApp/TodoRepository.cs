namespace TodoApp;

// Tầng DATA
public class TodoRepository
{
    private readonly List<Todo> _todos = new();
    private int _nextId = 1;
    private readonly string filePath = "todos.txt";

    public TodoRepository() => LoadFromFile();

    public List<Todo> GetAll() => _todos;

    public Todo Add(string title)
    {
        var item = new Todo { Id = _nextId++, Title = title, IsCompleted = false };
        _todos.Add(item);
        SaveToFile();
        return item;
    }

    public bool Delete(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item == null) return false;
        _todos.Remove(item);
        SaveToFile();
        return true;
    }

    public bool ToggleComplete(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item == null) return false;
        item.IsCompleted = !item.IsCompleted;
        SaveToFile();
        return true;
    }

    public bool Update(int id, string newTitle)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item == null) return false;
        item.Title = newTitle;
        SaveToFile();
        return true;
    }

    private void LoadFromFile()
    {
        if (!File.Exists(filePath)) return;
        foreach (var line in File.ReadAllLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var item = Todo.FromFileString(line);
            _todos.Add(item);
            if (item.Id >= _nextId) _nextId = item.Id + 1;
        }
    }

    private void SaveToFile()
        => File.WriteAllLines(filePath, _todos.Select(t => t.ToFileString()));
}
