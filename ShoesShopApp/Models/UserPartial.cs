using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoesShopApp.Models
{
    public partial class User
    {
        public bool IsAdmin => Role.RoleId == 1;
        public bool IsStaff => Role.RoleId == 2;
    }
}
