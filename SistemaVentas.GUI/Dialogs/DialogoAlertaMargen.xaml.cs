using SistemaVentas.BLL;
using SistemaVentas.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SistemaVentas.GUI.Dialogs
{
    public partial class DialogoAlertaMargen : Window
    {
        private readonly CN_Producto _cnProducto = new CN_Producto();
        private readonly ObservableCollection<AlertaMargenItem> _items = new ObservableCollection<AlertaMargenItem>();

        public DialogoAlertaMargen(List<AlertaMargen> alertas)
        {
            InitializeComponent();

            if (alertas == null)
                return;

            foreach (var alerta in alertas)
            {
                _items.Add(new AlertaMargenItem
                {
                    IdProducto = alerta.IdProducto,
                    NombreProducto = alerta.NombreProducto,
                    CostoAnterior = alerta.CostoAnterior,
                    CostoNuevo = alerta.CostoNuevo,
                    PrecioVenta = alerta.PrecioVenta,
                    MargenAnterior = alerta.MargenAnterior,
                    MargenNuevo = alerta.MargenNuevo,
                    PrecioVentaSugerido = alerta.PrecioVentaSugerido,
                    NuevoPrecioVenta = alerta.PrecioVentaSugerido
                });
            }

            dgAlertas.ItemsSource = _items;
        }

        private void btnAplicar_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not AlertaMargenItem item)
                return;

            if (item.NuevoPrecioVenta <= 0)
            {
                MessageBox.Show("El nuevo precio de venta debe ser mayor a 0.", "Precio inválido", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Obtener producto actual
            var productos = _cnProducto.Listar();
            var producto = productos.FirstOrDefault(p => p.IdProducto == item.IdProducto);
            if (producto == null)
            {
                MessageBox.Show("No se encontró el producto.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Actualizar precio de venta
            producto.PrecioVenta = item.NuevoPrecioVenta;
            // Mantener costo actual (ya actualizado por la compra) - el costo ya fue actualizado en CompraDAL
            // pero necesitamos asegurarnos de tener el costo correcto
            producto.PrecioCosto = item.CostoNuevo;

            if (_cnProducto.Editar(producto, out string mensaje))
            {
                MessageBox.Show($"Precio de venta actualizado para {item.NombreProducto}.", "Éxito", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                // Remover el item aplicado
                _items.Remove(item);
            }
            else
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public class AlertaMargenItem : INotifyPropertyChanged
        {
            private decimal _nuevoPrecioVenta;
            
            public int IdProducto { get; set; }
            public string NombreProducto { get; set; } = string.Empty;
            public decimal CostoAnterior { get; set; }
            public decimal CostoNuevo { get; set; }
            public decimal PrecioVenta { get; set; }
            public decimal MargenAnterior { get; set; }
            public decimal MargenNuevo { get; set; }
            public decimal PrecioVentaSugerido { get; set; }
            
            public decimal NuevoPrecioVenta 
            { 
                get => _nuevoPrecioVenta; 
                set 
                { 
                    _nuevoPrecioVenta = value; 
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NuevoPrecioVenta)));
                } 
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}
