using System.ComponentModel.DataAnnotations;

public class Category
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    public string? Description { get; set; }

    public string? Slug { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Product>? Products { get; set; }
}