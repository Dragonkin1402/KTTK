using TodoEntity = Todo.Domain.Todo;

namespace Todo.Domain.Repositories;

public interface ITodoRepository : IRepository<TodoEntity>
{
}
