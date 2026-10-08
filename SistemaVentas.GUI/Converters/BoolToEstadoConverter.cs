using System;
using System.Globalization;
using System.Windows.Data;

namespace SistemaVentas.GUI.Converters
{
    /// <summary>
    /// Convierte un valor booleano a una cadena legible (Activo/Inactivo)
    /// </summary>
    public class BoolToEstadoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool estado)
            {
                return estado ? "Activo" : "Inactivo";
            }
            return "Desconocido";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string estado)
            {
                return estado.Equals("Activo", StringComparison.Ordinal);
            }
            return false;
        }
    }
}
