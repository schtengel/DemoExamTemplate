using System.ComponentModel.DataAnnotations.Schema;

namespace ShoesShopApp.Models;

public partial class Manufacturer
{
    public int ManufacturerId { get; set; }

    public string ManufacturerName { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [NotMapped]
    public virtual int ColumnWidth { get; set; }

    [NotMapped]
    public int FilteredCount { get; set; }
}
