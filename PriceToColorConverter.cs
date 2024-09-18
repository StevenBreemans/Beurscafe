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

                // Colors from a unified, softer color palette
                Brush softGreen = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#81C784")); // Softer Green
                Brush softOrange = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB74D")); // Softer Orange
                Brush softRed = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC143C"));   // Softer Red
                Brush defaultGray = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B0BEC5")); // Soft Gray as fallback

                if (currentPrice <= lowerThreshold)
                {
                    return softGreen;  // Below 25% -> Softer Green
                }
                else if (currentPrice > lowerThreshold && currentPrice < upperThreshold)
                {
                    return softOrange;  // Between 25% and 75% -> Softer Orange
                }
                else
                {
                    return softRed;  // Above 75% -> Softer Red
                }
            }

            // Default color (if something goes wrong)
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B0BEC5"));  // Light Gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
