using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; }

    [Required]
    [StringLength(100)]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; } // m đã đổi từ PasswordHash

    [Required]
    public string Role { get; set; } = "Customer";

    [StringLength(15)]
    public string? Phone { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<Order>? Orders { get; set; }
}