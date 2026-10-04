using SistemaVentas.BLL;
using SistemaVentas.Entities;
using SistemaVentas.GUI.Contexto;
using SistemaVentas.GUI.Dialogs;

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;



namespace SistemaVentas.GUI
{
    public partial class Ventas : UserControl
    {
        private readonly ObservableCollection<DetalleVenta> _carrito;
        private readonly ObservableCollection<Producto> _productos;
        private readonly CN_Producto _productoBLL;
        private readonly VentaBLL _ventaBLL;
        private ComboBox _cmbProductos = null!;
        

        // ELIMINADO: private bool _cajaAbierta = false;

        public Ventas()
        {
            InitializeComponent();
            
            
            _carrito = new ObservableCollection<DetalleVenta>();
            _productos = new ObservableCollection<Producto>();
            _productoBLL = new CN_Producto();
            _ventaBLL = new VentaBLL();
            dgVentas.ItemsSource = _carrito;
            cmbProductos.ItemsSource = _productos;

            CargarProductos();

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
        private void CargarProductos()
        {
            try
            {
                foreach (Producto producto in _productoBLL.Listar().Where(producto => producto.Activo))
                    _productos.Add(producto);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo cargar el catálogo de productos.\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProductos.SelectedItem is not Producto producto)
            {
                MessageBox.Show("Seleccione un producto del catálogo.", "Producto requerido",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1. Buscamos si el producto ya existe en el carrito
            var itemExistente = _carrito.FirstOrDefault(d => d.IdProducto == producto.IdProducto);

            if (itemExistente != null)
            {
                // 2. Si existe, incrementamos la cantidad
                itemExistente.Cantidad += 1;

                

                // 3. Forzamos a la grilla a redibujarse para mostrar la nueva cantidad y subtotal
                // Esto es necesario porque una ObservableCollection detecta cuando se agrega/quita una fila, 
                // pero NO detecta cuando cambia una propiedad interna de una fila existente.
                dgVentas.Items.Refresh();
            }
            else
            {
                // 4. Si no existe, lo agregamos como un nuevo registro
                _carrito.Add(new DetalleVenta
                {
                    IdProducto = producto.IdProducto,
                    Cantidad = 1,
                    PrecioUnitario = producto.PrecioVenta,
                    // Subtotal se calcula automáticamente en la entidad DetalleVenta
                });
            }

            // Limpiamos el combo y actualizamos el total general
            cmbProductos.SelectedIndex = -1;
            cmbProductos.Text = string.Empty;
            ActualizarTotal();
        }

        private void ActualizarTotal()
        {
            decimal total = _carrito.Sum(item => item.Subtotal);

            txtTotal.Text = $"$ {total:N2}";
            txtCantidadItems.Text = $"{_carrito.Sum(item => item.Cantidad)} items";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var itemSeleccionado = dgVentas.SelectedItem as DetalleVenta;

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
            // 1. Validaciones iniciales
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

            var metodoSeleccionado = _cmbMetodoPago.SelectedItem as ComboBoxItem;
            if (metodoSeleccionado == null || metodoSeleccionado.Content == null)
            {
                MessageBox.Show("Por favor, seleccione un método de pago válido.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string metodoPagoTag = metodoSeleccionado.Tag?.ToString() ?? string.Empty;

            // 2. Calcular montos
            decimal totalVenta = _carrito.Sum(item => item.Subtotal);
            decimal montoAbonado = 0;
            decimal vuelto = 0;

            // 3. Lógica según el método de pago
            if (metodoPagoTag == "Efectivo")
            {
                
                var dialogCobro = new CobroDialog(totalVenta);
                dialogCobro.Owner = Window.GetWindow(this);

                if (dialogCobro.ShowDialog() == true)
                {
                    montoAbonado = dialogCobro.MontoIngresado;
                    vuelto = montoAbonado - totalVenta;
                }
                else
                {
                    // Si el cajero cierra la ventana de cobro, cancelamos la operación
                    return; 
                }
                

               
            }
            else // Mercado Pago o Transferencia
            {
                montoAbonado = totalVenta;
                vuelto = 0; // En pago digital no hay vuelto físico
            }

            // 4. Armar la entidad Venta
            Venta nuevaVenta = new Venta
            {
                IdCajaUsuario = SesionGlobal.IdCajaUsuarioActual,
                MetodoPago = metodoPagoTag,
                Total = totalVenta,
                MontoAbonado = montoAbonado,
                Vuelto = vuelto,
                // Convertimos la ObservableCollection a List para la entidad
                Detalles = _carrito.ToList()
            };

            // 5. Enviar a la base de datos a través de la BLL
            try
            {
                // Llamamos al método Insertar (que a su vez llama a tu DAL transaccional)
                int idVentaGenerada = _ventaBLL.RegistrarVenta(nuevaVenta);

                // Si llegó hasta aquí, la transacción fue exitosa
                MessageBox.Show($"Venta #{idVentaGenerada} generada con éxito.\nTotal: ${totalVenta:N2}\nVuelto: ${vuelto:N2}",
                                "Venta Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                // 6. Limpiar la UI para el próximo cliente
                _carrito.Clear();
                ActualizarTotal();

                // Opcional: resetear el método de pago por defecto
                _cmbMetodoPago.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                // Si falló algo en la BD (ej. se cortó la conexión), el Rollback ya se hizo en la DAL
                MessageBox.Show($"Ocurrió un error al guardar la venta:\n{ex.Message}", "Error BD", MessageBoxButton.OK, MessageBoxImage.Error);
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

        

// --- 1. Eventos de los botones visuales (+ y -) ---
private void btnSumar_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button?.Tag is DetalleVenta item)
        {
            ModificarCantidad(item, 1); // Suma 1
        }
    }

    private void btnRestar_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button?.Tag is DetalleVenta item)
        {
            ModificarCantidad(item, -1); // Resta 1
        }
    }

    // --- 2. Evento que captura el teclado físico sobre la grilla ---
    private void dgVentas_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Obtenemos la fila que está seleccionada actualmente
        var itemSeleccionado = dgVentas.SelectedItem as DetalleVenta;

        // Si no hay nada seleccionado (o el carrito está vacío), no hacemos nada
        if (itemSeleccionado == null) return;

        // A. Si presiona la tecla '+' (en el teclado normal o en el numérico)
        if (e.Key == Key.Add || e.Key == Key.OemPlus)
        {
            ModificarCantidad(itemSeleccionado, 1);
            e.Handled = true; // Evita que la tecla haga otra acción no deseada
        }
        // B. Si presiona la tecla '-' 
        else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
        {
            ModificarCantidad(itemSeleccionado, -1);
            e.Handled = true;
        }
        // C. Si presiona un número del 1 al 9 (Teclado superior)
        else if (e.Key >= Key.D1 && e.Key <= Key.D9)
        {
            int numeroPresionado = e.Key - Key.D0;
            FijarCantidadExacta(itemSeleccionado, numeroPresionado);
            e.Handled = true;
        }
        // D. Si presiona un número del 1 al 9 (Teclado numérico lateral)
        else if (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad9)
        {
            int numeroPresionado = e.Key - Key.NumPad0;
            FijarCantidadExacta(itemSeleccionado, numeroPresionado);
            e.Handled = true;
        }
    }

    // --- 3. Métodos centralizados para actualizar la fila ---
    private void ModificarCantidad(DetalleVenta item, int variacion)
    {
        // Si al restar llega a 0, lo quitamos del carrito
        if (item.Cantidad + variacion <= 0)
        {
            _carrito.Remove(item);
        }
        else
        {
            item.Cantidad += variacion;
           
            dgVentas.Items.Refresh(); // Obliga a la grilla a actualizar los textos
        }
        ActualizarTotal(); // Tu método existente
    }

    private void FijarCantidadExacta(DetalleVenta item, int cantidadExacta)
    {
        item.Cantidad = cantidadExacta;
        
        dgVentas.Items.Refresh();
        ActualizarTotal();
    }

}
}
