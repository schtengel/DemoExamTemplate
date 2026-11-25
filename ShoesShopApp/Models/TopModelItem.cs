using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemEKZZ.Models
{
    public class TopModelItem
    {
        public string ModelName { get; set; } = "";
        public string ManufacturerName { get; set; } = "";
        public int TotalPairs { get; set; }
        public int ColumnWidth { get; set; }
    }
}
