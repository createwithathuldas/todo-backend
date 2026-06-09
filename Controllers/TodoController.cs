//todo controller to add,update,soft delete and get all todos
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Todo_backend.DTOS;

namespace Todo_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodoController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }
            return int.Parse(userIdClaim);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodo([FromBody] TodoDto todoDto)
        {
            var userId = GetCurrentUserId();
            var createdTodo = await _todoService.CreateTodoAsync(userId, todoDto);
            return CreatedAtAction(nameof(GetTodoById), new { id = createdTodo.Id }, createdTodo);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodoById(int id)
        {
            var userId = GetCurrentUserId();
            var todo = await _todoService.GetTodoByIdAsync(userId, id);
            if (todo == null)
                return NotFound();
            return Ok(todo);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTodos()
        {
            var userId = GetCurrentUserId();
            var todos = await _todoService.GetAllTodosAsync(userId);
            return Ok(todos);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, [FromBody] TodoDto todoDto)
        {
            var userId = GetCurrentUserId();
            var updatedTodo = await _todoService.UpdateTodoAsync(userId, id, todoDto);
            if (updatedTodo == null)
                return NotFound();
            return Ok(updatedTodo);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteTodo(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _todoService.SoftDeleteTodoAsync(userId, id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }

    public interface ITodoService
    {
        Task<TodoDto> CreateTodoAsync(int userId, TodoDto todoDto);
        Task<TodoDto> GetTodoByIdAsync(int userId, int id);
        Task<IEnumerable<TodoDto>> GetAllTodosAsync(int userId);
        Task<TodoDto> UpdateTodoAsync(int userId, int id, TodoDto todoDto);
        Task<bool> SoftDeleteTodoAsync(int userId, int id);
    }
}