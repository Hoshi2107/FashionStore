using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Product
{
    public int Id { get; set; }

    //[Required(ErrorMessage = "Vui lòng chọn danh mục")]
    //[Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục")]
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục")]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; }

    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int Stock { get; set; } = 0;

    public string? ImageUrl { get; set; }

    public string? Size { get; set; }

    public string? Color { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Category? Category { get; set; }
    public ICollection<OrderDetail>? OrderDetails { get; set; }
}