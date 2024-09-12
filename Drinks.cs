using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beurscafe
{
    public class Drinks
    {
        public string Name { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public double? CurrentPrice { get; set; }
        public int Orders { get; set; }

        public Drinks(string name, double? minPrice, double? maxPrice, double? currentPrice)
        {
            Name = name;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            CurrentPrice = currentPrice;
            Orders = 0;
        }
    }
}
