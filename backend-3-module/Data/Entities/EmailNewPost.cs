namespace backend_3_module.Data.Entities;

public class EmailNewPost
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string To { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
    public bool Status { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
}