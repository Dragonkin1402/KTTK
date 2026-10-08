using TodoEntity = Todo.Domain.Todo;

namespace Todo.Application.Services;

public interface ITodoService
{
    Task<List<TodoEntity>> GetAll();
    Task<TodoEntity?> GetById(int id);
    Task AddTodo(TodoEntity todo);
    Task UpdateTodo(TodoEntity todo);
    Task DeleteTodo(int id);
}
