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

        // Adjust the price based on orders
        public void AdjustPrice()
        {
            if (Orders > 0)
            {
                // Increase the price based on the number of orders
                CurrentPrice += Orders * 0.10;  // Increase by 0.10 per order
                if (CurrentPrice > MaxPrice) CurrentPrice = MaxPrice;  // Ensure it doesn't exceed the max price
            }
            else
            {
                // Decrease the price by a fixed amount if no orders were made
                CurrentPrice -= 0.10;
                if (CurrentPrice < MinPrice) CurrentPrice = MinPrice;  // Ensure it doesn't go below the min price
            }

            // Round CurrentPrice to 1 decimal place
            CurrentPrice = Math.Round(CurrentPrice.Value, 1);
        }


    }
}
