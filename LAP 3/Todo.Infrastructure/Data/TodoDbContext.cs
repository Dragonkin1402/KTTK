using Microsoft.EntityFrameworkCore;
using TodoEntity = Todo.Domain.Todo;

namespace Todo.Infrastructure.Data;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

    public DbSet<TodoEntity> Todos => Set<TodoEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoEntity>(e =>
        {
            e.ToTable("Todos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(50);
        });
    }
}
