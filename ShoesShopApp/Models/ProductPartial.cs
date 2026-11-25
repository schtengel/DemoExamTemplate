using System.IO;

namespace ShoesShopApp.Models
{
    public partial class Product
    {
        public string Title => $"{Category?.CategoryName} | {ProductName}";
        public bool HasDiscount => Discount > 0;
        public decimal FinalPrice => Price * (100 - Discount) / 100;
        public string RowColor => Discount > 15 ? "#2E8B57" : Quantity == 0 ? "LightBlue" : "White";

        public string Photo
        {
            get
            {
                var path = Path.Combine(AppContext.BaseDirectory, PhotoPath ?? "");
                return File.Exists(path) ? path : Path.Combine(AppContext.BaseDirectory, "Images", "picture.png");
            }
        }

        public string SearchText => string.Join(" ", Article, ProductName, Unit, Description, Category.CategoryName, Manufacturer.ManufacturerName, Supplier.SupplierName);
    }
}
