namespace TodoAppV2;

// Tầng LOGIC
public class TodoService : ITodoService
{
    private readonly TodoRepository _repository;

    public TodoService(TodoRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Todo>> GetAllAsync() => _repository.GetAllAsync();

    public Task<Todo?> GetAsync(int id) => _repository.GetAsync(id);

    public async Task AddAsync(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return;
        await _repository.AddAsync(title.Trim());
    }

    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);

    public async Task<bool> ToggleTodoAsync(int id)
    {
        var todo = await _repository.GetAsync(id);
        if (todo == null) return false;

        todo.IsCompleted = !todo.IsCompleted;
        return await _repository.UpdateAsync(todo);
    }

    public async Task<bool> UpdateAsync(int id, string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return false;

        var todo = await _repository.GetAsync(id);
        if (todo == null) return false;

        todo.Title = title.Trim();
        return await _repository.UpdateAsync(todo);
    }
}
