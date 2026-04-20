using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Order
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;

    public string Status { get; set; } = "Pending";

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public string? ShippingAddress { get; set; }

    public string? Note { get; set; }

    // Navigation
    public User? User { get; set; }
    public ICollection<OrderDetail>? OrderDetails { get; set; }
}