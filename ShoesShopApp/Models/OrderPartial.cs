using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoesShopApp.Models
{
    public partial class Order
    {
        public string Articles => string.Join(", ", OrderItems.Select(i => $"{i.Product.Article}, {i.Product.Quantity}"));
    }
}
