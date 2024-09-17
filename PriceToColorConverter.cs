using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Beurscafe
{
    public class PriceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Drinks drink && drink.MinPrice.HasValue && drink.MaxPrice.HasValue && drink.CurrentPrice.HasValue)
            {
                double minPrice = drink.MinPrice.Value;
                double maxPrice = drink.MaxPrice.Value;
                double currentPrice = drink.CurrentPrice.Value;

                // Calculate the 25% and 75% thresholds
                double lowerThreshold = minPrice + 0.25 * (maxPrice - minPrice);
                double upperThreshold = minPrice + 0.75 * (maxPrice - minPrice);

                if (currentPrice <= lowerThreshold)
                {
                    return Brushes.Green;  // Below 25% -> Green
                }
                else if (currentPrice > lowerThreshold && currentPrice < upperThreshold)
                {
                    return Brushes.Orange;  // Between 25% and 75% -> Orange
                }
                else
                {
                    return Brushes.Red;  // Above 75% -> Red
                }
            }

            // Default color (if something goes wrong)
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
