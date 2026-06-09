using Microsoft.EntityFrameworkCore;
using Todo_backend.Models;

namespace Todo_backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<TodoList> TodoLists { get; set; }
    }
}