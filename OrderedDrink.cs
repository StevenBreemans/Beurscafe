using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beurscafe
{
    public class OrderedDrink
    {
        public Drinks Drink { get; set; }
        public int Orders { get; set; }

        public OrderedDrink(Drinks drink)
        {
            Drink = drink;
            Orders = 1; // Start with 1 order when first added
        }
    }

}
