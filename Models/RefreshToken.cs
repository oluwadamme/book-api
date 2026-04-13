namespace FirstApi.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; }
    public string FamilyId { get; set; } // The ID grouping all rotated tokens

    public DateTime CreatedOn { get; set; }
    public DateTime ExpiresOn { get; set; }

    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; } // The Kill Switch
}