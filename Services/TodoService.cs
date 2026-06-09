using Microsoft.EntityFrameworkCore;
using Todo_backend.Data;
using Todo_backend.DTOS;
using Todo_backend.Models;
using Todo_backend.Controllers;

namespace Todo_backend.Services
{
    public class TodoService : ITodoService
    {
        private readonly ApplicationDbContext _context;

        public TodoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TodoDto> CreateTodoAsync(int userId, TodoDto todoDto)
        {
            var user = await _context.Users.FindAsync(userId);
            var todo = new TodoList
            {
                Task = todoDto.Task,
                UserId = userId,
                User = user!,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = false,
                Isdeleted = false
            };

            _context.TodoLists.Add(todo);
            await _context.SaveChangesAsync();

            return MapToDto(todo);
        }

        public async Task<TodoDto> GetTodoByIdAsync(int userId, int id)
        {
            var todo = await _context.TodoLists
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && !t.Isdeleted);

            if (todo == null)
                return null!;

            return MapToDto(todo);
        }

        public async Task<IEnumerable<TodoDto>> GetAllTodosAsync(int userId)
        {
            var todos = await _context.TodoLists
                .Include(t => t.User)
                .Where(t => t.UserId == userId && !t.Isdeleted)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return todos.Select(MapToDto).ToList();
        }

        public async Task<TodoDto> UpdateTodoAsync(int userId, int id, TodoDto todoDto)
        {
            var todo = await _context.TodoLists
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && !t.Isdeleted);

            if (todo == null)
                return null!;

            todo.Task = todoDto.Task;
            todo.IsCompleted = todoDto.IsCompleted;

            if (todoDto.IsCompleted && todo.CompletedAt == default)
            {
                todo.CompletedAt = DateTime.UtcNow;
            }
            else if (!todoDto.IsCompleted)
            {
                todo.CompletedAt = default;
            }

            _context.TodoLists.Update(todo);
            await _context.SaveChangesAsync();

            return MapToDto(todo);
        }

        public async Task<bool> SoftDeleteTodoAsync(int userId, int id)
        {
            var todo = await _context.TodoLists
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && !t.Isdeleted);

            if (todo == null)
                return false;

            todo.Isdeleted = true;
            todo.DeletedAt = DateTime.UtcNow;

            _context.TodoLists.Update(todo);
            await _context.SaveChangesAsync();

            return true;
        }

        private TodoDto MapToDto(TodoList todo)
        {
            return new TodoDto
            {
                Id = todo.Id,
                Task = todo.Task,
                UserName = todo.User?.Name ?? "Guest",
                CreatedAt = todo.CreatedAt,
                IsCompleted = todo.IsCompleted,
                CompletedAt = todo.CompletedAt
            };
        }
    }
}
