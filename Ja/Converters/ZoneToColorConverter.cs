using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Ja.Converters
{
    /// <summary>
    /// Konwerter strefy treningowej na kolor tła
    /// </summary>
    public class ZoneToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int zone)
            {
                return zone switch
                {
                    1 => new SolidColorBrush(Color.FromRgb(211, 211, 211)), // LightGray - Recovery
                    2 => new SolidColorBrush(Color.FromRgb(173, 216, 230)), // LightBlue - Endurance
                    3 => new SolidColorBrush(Color.FromRgb(144, 238, 144)), // LightGreen - Tempo
                    4 => new SolidColorBrush(Color.FromRgb(255, 255, 153)), // LightYellow - Threshold
                    5 => new SolidColorBrush(Color.FromRgb(255, 200, 124)), // LightOrange - VO2max
                    6 => new SolidColorBrush(Color.FromRgb(255, 160, 122)), // LightCoral - Anaerobic
                    7 => new SolidColorBrush(Color.FromRgb(255, 99, 71)),   // Tomato - Neuromuscular
                    _ => Brushes.White
                };
            }

            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
