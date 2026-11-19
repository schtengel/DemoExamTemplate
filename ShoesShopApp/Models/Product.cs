namespace ShoesShopApp.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Article { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public decimal Price { get; set; }

    public int Discount { get; set; }

    public int Quantity { get; set; }

    public string? Description { get; set; }

    public string? PhotoPath { get; set; }

    public int CategoryId { get; set; }

    public int ManufacturerId { get; set; }

    public int SupplierId { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Supplier Supplier { get; set; } = null!;
}
