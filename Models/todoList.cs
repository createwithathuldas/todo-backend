namespace Todo_backend.Models
{
    public class TodoList
    {
        public int Id { get; set; }
        public string Task { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CompletedAt { get; set; }
        public bool Isdeleted{get;set;}
        public DateTime? DeletedAt { get; set; }
        public User User { get; set; }
    }
}