using Todo.Domain.Repositories;
using TodoEntity = Todo.Domain.Todo;

namespace Todo.Application.Services;

public class TodoService : ITodoService
{
    private readonly ITodoRepository _repository;

    public TodoService(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task AddTodo(TodoEntity todo)
    {
        await _repository.AddAsync(todo);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteTodo(int id)
    {
        var todo = await _repository.GetByIdAsync(id);
        if (todo != null)
        {
            _repository.DeleteAsync(todo);
            await _repository.SaveChangesAsync();
        }
    }

    public async Task<List<TodoEntity>> GetAll()
    {
        var todos = await _repository.GetAllAsync();
        return todos.ToList();
    }

    public async Task<TodoEntity?> GetById(int id) => await _repository.GetByIdAsync(id);

    public async Task UpdateTodo(TodoEntity todo)
    {
        var item = await _repository.GetByIdAsync(todo.Id);
        if (item != null)
        {
            item.Title = todo.Title;
            item.IsCompleted = todo.IsCompleted;
            _repository.UpdateAsync(item);
            await _repository.SaveChangesAsync();
        }
    }
}
