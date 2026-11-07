using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SistemaVentas.Entities
{
    public class DetalleVenta : INotifyPropertyChanged
    {
        private int _cantidad;
        private decimal _precioUnitario;

        public int IdDetalleVenta { get; set; }
        public int IdVenta { get; set; }
        public int IdProducto { get; set; }

        public int Cantidad
        {
            get => _cantidad;
            set
            {
                if (_cantidad == value) return;
                _cantidad = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public decimal PrecioUnitario
        {
            get => _precioUnitario;
            set
            {
                if (_precioUnitario == value) return;
                _precioUnitario = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        // Propiedad calculada para que WPF la muestre en el DataGrid
        public decimal Subtotal => Cantidad * PrecioUnitario;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
