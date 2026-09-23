using SistemaVentas.Entities;
using SistemaVentas.GUI.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SistemaVentas.GUI
{
    public partial class Ventas : UserControl
    {
        // 1. Declaramos nuestro carrito simulado
        private ObservableCollection<DetalleVentaSimulada> _carrito;

        // Estado de la caja
        private bool _cajaAbierta = false;

        public Ventas()
        {
            InitializeComponent();

            // 2. Inicializamos el carrito vacio
            _carrito = new ObservableCollection<DetalleVentaSimulada>();

            // 3. Enlazamos la grilla al carrito
            dgVentas.ItemsSource = _carrito;
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
            if(_carrito.Count == 0)
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
            if (!_cajaAbierta)
            {
                // Abrir caja
                var dialog = new AbrirCajaDialog();

                // Obtenemos la ventana padre para que el diálogo sea modal
                Window parentWindow = Window.GetWindow(this);

                if (dialog.ShowDialog() == true)
                {
                    decimal montoIngresado = dialog.MontoIngresado;

                    _cajaAbierta = true;
                    btnAbrirCerrarCaja.Content = "📁 CERRAR CAJA";

                    MessageBox.Show($"Caja abierta simulada con éxito.\nMonto inicial: ${montoIngresado:N2}", 
                                  "Caja Abierta", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                // Cerrar caja
                _cajaAbierta = false;
                btnAbrirCerrarCaja.Content = "📂 ABRIR CAJA";

                MessageBox.Show("Caja cerrada simulada con éxito.", 
                              "Caja Cerrada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
