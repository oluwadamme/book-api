using System.ComponentModel.DataAnnotations;
namespace FirstApi.DTOs;

public class CreateBookRequest
{
    [Required, MinLength(2)] public string Title { get; set; }
    [Required, MinLength(2)] public string Author { get; set; }
    [Required, Range(1000, 2026)] public int YearPublished { get; set; }
}