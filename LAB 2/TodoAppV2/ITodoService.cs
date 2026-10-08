namespace TodoAppV2;

public interface ITodoService
{
    Task<List<Todo>> GetAllAsync();
    Task<Todo?> GetAsync(int id);
    Task AddAsync(string title);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateAsync(int id, string title);
    Task<bool> ToggleTodoAsync(int id);
}
