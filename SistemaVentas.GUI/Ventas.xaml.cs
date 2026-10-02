using SistemaVentas.Entities;
using SistemaVentas.GUI.Contexto;
using SistemaVentas.GUI.Dialogs;

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SistemaVentas.GUI
{
    public partial class Ventas : UserControl
    {
        private ObservableCollection<DetalleVentaSimulada> _carrito;

        // ELIMINADO: private bool _cajaAbierta = false;

        public Ventas()
        {
            InitializeComponent();
            _carrito = new ObservableCollection<DetalleVentaSimulada>();
            dgVentas.ItemsSource = _carrito;

            // Validar estado visual del botón al cargar el control
            ActualizarBotonCaja();
        }

        private void ActualizarBotonCaja()
        {
            if (SesionGlobal.IdCajaUsuarioActual > 0)
            {
                btnAbrirCerrarCaja.Content = "📁 CERRAR CAJA";
            }
            else
            {
                btnAbrirCerrarCaja.Content = "📂 ABRIR CAJA";
            }
        }
        // 4. Evento del botón para simular una carga
        private void btnAgregarPrueba_Click(object sender, RoutedEventArgs e)
            {
                // Creamos un producto hardcodeado (simulando que lo leemos de unos TextBox)
                var nuevoItem = new DetalleVentaSimulada
                {
                    IdProducto = 101,
                    NombreProducto = "Yerba Mate 1kg (Simulado)",
                    Cantidad = 2,
                    PrecioUnitario = 1500m
                };

                // Al agregarlo, la grilla dgCarrito se actualiza sola
                _carrito.Add(nuevoItem);

                // Recalculamos el total
                ActualizarTotal();
            }

            // 5. Método para sumar todo
            private void ActualizarTotal()
            {
                decimal total = 0;
                foreach (var item in _carrito)
                {
                    total += item.Subtotal;
                }

                txtTotal.Text = $"$ {total:N2}";
                txtCantidadItems.Text = $"{_carrito.Count} items";
            }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //esto funcionaria si el btn delete se encontrara dentro del datagrid
            //al estar físicamente fuera de la tabla, ese botón no hereda el contexto de una fila individual
            //dgVentas.SelectedItem = (DetalleVentaSimulada)((Button)sender).DataContext;
            //var itemSeleccionado = (DetalleVentaSimulada)dgVentas.SelectedItem;
            //_carrito.Remove(itemSeleccionado);
            //ActualizarTotal();

            var itemSeleccionado = dgVentas.SelectedItem as DetalleVentaSimulada;

            if (itemSeleccionado != null)
            {
                _carrito.Remove(itemSeleccionado);
                ActualizarTotal();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _carrito.Clear();
            ActualizarTotal();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // NUEVO: Validar que la caja esté abierta antes de intentar vender
            if (SesionGlobal.IdCajaUsuarioActual == 0)
            {
                MessageBox.Show("Debe abrir un turno de caja antes de procesar una venta.", "Caja Cerrada", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_carrito.Count == 0)
            {
                MessageBox.Show("El carrito está vacío. No se puede generar la venta.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
                // solo mostramos un mensaje
                MessageBox.Show("Venta generada con éxito (simulado).");
                // Limpiamos el carrito después de generar la venta
                _carrito.Clear();
                ActualizarTotal();
            }
        }

        private void btnAbrirCerrarCaja_Click(object sender, RoutedEventArgs e)
        {
            if (SesionGlobal.IdCajaUsuarioActual == 0)
            {
                // Abrir caja
                var dialog = new AbrirCajaDialog();

                // Obtenemos la ventana padre para que el diálogo sea modal
                Window parentWindow = Window.GetWindow(this);
                dialog.Owner = parentWindow; // Buena práctica

                if (dialog.ShowDialog() == true)
                {
                    
                    
                    ActualizarBotonCaja();

                    MessageBox.Show($"Caja abierta con éxito.\nMonto inicial: ${dialog.MontoIngresado:N2}",
                                  "Caja Abierta", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                // Cerrar caja
                var dialog = new CerrarCajaDialog();
                dialog.Owner = Window.GetWindow(this);

                if (dialog.ShowDialog() == true)
                {
                    // El diálogo ya se encargó de poner SesionGlobal.IdCajaUsuarioActual en 0
                    ActualizarBotonCaja();
                }
            }
        }

    }
}
