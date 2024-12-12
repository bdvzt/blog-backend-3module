namespace backend_3_module.Data.DTO.Author;

public class AuthorDTO
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public DateTime BirthDate { get; set; }
    public Gender Gender { get; set; }
    public int Posts { get; set; }
    public int Likes { get; set; }
    public DateTime CreateTime { get; set; }
}