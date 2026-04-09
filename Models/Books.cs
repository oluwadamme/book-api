namespace FirstApi.Models;

public class Book
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public int YearPublished { get; set; }
}