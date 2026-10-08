namespace TodoApp;

// Tầng LOGIC
public class TodoService
{
    private readonly TodoRepository _repo = new();

    public List<Todo> GetTodos() => _repo.GetAll();
    public Todo AddTodo(string title) => _repo.Add(title);
    public bool RemoveTodo(int id) => _repo.Delete(id);
    public bool ToggleTodo(int id) => _repo.ToggleComplete(id);
    public bool EditTodo(int id, string newTitle) => _repo.Update(id, newTitle);
}
