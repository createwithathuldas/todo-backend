namespace Todo_backend.DTOS
{
    public class TodoDto
    {
        public int Id {get;set;}
        public string Task { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CompletedAt {get;set;}
    }
}