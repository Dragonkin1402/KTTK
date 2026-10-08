namespace TodoAppV2;

// Tầng UI
public class TodoUI
{
    private readonly ITodoService _todoService;

    public TodoUI(ITodoService todoService)
    {
        _todoService = todoService;
    }

    public async Task Run()
    {
        while (true)
        {
            Console.Clear();
            await ShowTodos();
            ShowMenu();

            string? choice = Console.ReadLine();
            switch (choice)
            {
                case "1": await AddTodo(); break;
                case "2": await DeleteTodo(); break;
                case "3": await ToggleTodo(); break;
                case "4": await EditTodo(); break;
                case "0": return;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ!");
                    break;
            }

            Console.WriteLine("Nhấn Enter để tiếp tục...");
            Console.ReadLine();
        }
    }

    private async Task ShowTodos()
    {
        var todos = await _todoService.GetAllAsync();
        Console.WriteLine("=== DANH SÁCH CÔNG VIỆC ===");
        foreach (var todo in todos)
            Console.WriteLine(todo);

        if (todos.Count == 0)
            Console.WriteLine("Chưa có công việc nào.");
    }

    private void ShowMenu()
    {
        Console.WriteLine("\nChức năng:");
        Console.WriteLine("1. Thêm Todo");
        Console.WriteLine("2. Xoá Todo");
        Console.WriteLine("3. Đánh dấu hoàn thành");
        Console.WriteLine("4. Sửa nội dung");
        Console.WriteLine("0. Thoát");
        Console.Write("Chọn: ");
    }

    private async Task AddTodo()
    {
        Console.Write("Nhập nội dung công việc: ");
        string? title = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(title))
            await _todoService.AddAsync(title);
    }

    private async Task DeleteTodo()
    {
        Console.Write("Nhập ID công việc cần xoá: ");
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            if (!await _todoService.DeleteAsync(id))
                Console.WriteLine("Không tìm thấy công việc.");
        }
    }

    private async Task ToggleTodo()
    {
        Console.Write("Nhập ID cần đánh dấu hoàn thành: ");
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            if (!await _todoService.ToggleTodoAsync(id))
                Console.WriteLine("Không tìm thấy công việc.");
        }
    }

    private async Task EditTodo()
    {
        Console.Write("Nhập ID cần sửa: ");
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            Console.Write("Nhập nội dung mới: ");
            var newTitle = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newTitle))
            {
                if (!await _todoService.UpdateAsync(id, newTitle))
                    Console.WriteLine("Không tìm thấy công việc.");
            }
        }
    }
}
