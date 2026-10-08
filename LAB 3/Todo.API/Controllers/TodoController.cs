using Microsoft.AspNetCore.Mvc;
using Todo.Application.Services;
using TodoEntity = Todo.Domain.Todo;

namespace Todo.API.Controllers;

[Route("api/v1/todos")]
[ApiController]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodoController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAll() => Ok(await _todoService.GetAll());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var todo = await _todoService.GetById(id);
        return todo == null ? NotFound() : Ok(todo);
    }

    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] TodoEntity todo)
    {
        todo.Id = 0;
        await _todoService.AddTodo(todo);
        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TodoEntity todo)
    {
        if (await _todoService.GetById(id) == null) return NotFound();
        todo.Id = id;
        await _todoService.UpdateTodo(todo);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _todoService.GetById(id) == null) return NotFound();
        await _todoService.DeleteTodo(id);
        return NoContent();
    }
}
