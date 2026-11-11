using SistemaVentas.BLL;
using SistemaVentas.BLL.Tickets;
using SistemaVentas.Entities;
using SistemaVentas.Entities.Tickets;
using SistemaVentas.GUI.Contexto;
using SistemaVentas.GUI.Dialogs;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;



namespace SistemaVentas.GUI
{
    public partial class Ventas : UserControl
    {
        private const string ProductoRapido1 = "Coca-Cola";
        private const string ProductoRapido2 = "Coca-Cola Zero";

        private readonly ObservableCollection<DetalleVenta> _carrito;
        private readonly ObservableCollection<Producto> _productos;
        private readonly CN_Producto _productoBLL;
        private readonly VentaBLL _ventaBLL;
        private readonly TicketService _ticketService = new();
        private ComboBox _cmbProductos = null!;
        private Venta? _ventaParaTicket;
        private int _idVentaGenerada;
        private string? _rutaUltimoTicket;
        

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

            // Dejar el foco listo para el primer escaneo del lector
            Loaded += (_, _) => txtBuscador.Focus();
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
                _productos.Clear();
                foreach (Producto producto in _productoBLL.ListarDisponiblesParaVenta())
                    _productos.Add(producto);

                cmbProductos.SelectedIndex = -1;
                cmbProductos.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo cargar el catálogo de productos.\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RecargarProductos()
        {
            CargarProductos();
        }

        private Producto? ObtenerProductoDelCatalogo(int idProducto)
        {
            return _productos.FirstOrDefault(producto => producto.IdProducto == idProducto);
        }

        private void btnAgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProductos.SelectedItem is not Producto producto)
            {
                MessageBox.Show("Seleccione un producto del catálogo.", "Producto requerido",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AgregarAlCarrito(producto);
            txtBuscador.Focus();
        }

        private void AgregarAlCarrito(Producto producto)
        {
            // 1. Buscamos si el producto ya existe en el carrito
            var itemExistente = _carrito.FirstOrDefault(d => d.IdProducto == producto.IdProducto);
            int cantidadSolicitada = (itemExistente?.Cantidad ?? 0) + 1;

            // 2. Validamos el stock disponible antes de agregar o incrementar
            if (!_productoBLL.ValidarCantidadDisponible(producto, cantidadSolicitada, out string mensaje))
            {
                MessageBox.Show(mensaje, "Stock insuficiente",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (itemExistente != null)
            {
                // 3. Si existe, incrementamos la cantidad
                itemExistente.Cantidad = cantidadSolicitada;
            }
            else
            {
                // 4. Si no existe, lo agregamos como un nuevo registro
                _carrito.Add(new DetalleVenta
                {
                    IdProducto = producto.IdProducto,
                    NombreProducto = producto.Nombre,
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

        private void txtBuscador_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            e.Handled = true;

            string criterio = txtBuscador.Text;
            txtBuscador.Clear();
            txtBuscador.Focus();

            if (string.IsNullOrWhiteSpace(criterio))
                return;

            try
            {
                Producto? producto = _productoBLL.BuscarProducto(criterio, out string mensaje);

                if (producto is null)
                {
                    MessageBox.Show(mensaje, "Producto no encontrado",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                AgregarAlCarrito(producto);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo buscar el producto:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // El lector debe seguir teniendo el foco para el próximo escaneo
                txtBuscador.Focus();
            }
        }

        private void txtBuscador_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // El buscador solo acepta dígitos porque es exclusivo para códigos de barras
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void btnProductoRapido1_Click(object sender, RoutedEventArgs e)
        {
            AgregarProductoRapido(ProductoRapido1);
        }

        private void btnProductoRapido2_Click(object sender, RoutedEventArgs e)
        {
            AgregarProductoRapido(ProductoRapido2);
        }

        private void AgregarProductoRapido(string nombreProducto)
        {
            string buscado = Normalizar(nombreProducto);

            Producto? producto = _productos
                .Where(p => Normalizar(p.Nombre).Contains(buscado, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => Normalizar(p.Nombre).Length)
                .FirstOrDefault();

            if (producto is null)
            {
                MessageBox.Show($"No se encontró el producto '{nombreProducto}' en el catálogo.",
                    "Producto no encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AgregarAlCarrito(producto);
            txtBuscador.Focus();
        }

        private static string Normalizar(string texto)
        {
            return texto.Replace("-", "").Replace(" ", "").Trim();
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

        private void btnIngresoManual_Click(object sender, RoutedEventArgs e)
        {
            AbrirDialogoMovimientoManual("INGRESO");
        }

        private void btnEgresoManual_Click(object sender, RoutedEventArgs e)
        {
            AbrirDialogoMovimientoManual("EGRESO");
        }

        private void AbrirDialogoMovimientoManual(string tipo)
        {
            var dialog = new DialogIngresoEgreso(tipo)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                string operacion = tipo == "INGRESO" ? "Ingreso" : "Egreso";
                MessageBox.Show($"{operacion} registrado correctamente.", "Movimiento de caja",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
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

                // Guardamos la venta registrada para generar y reimprimir su ticket.
                _ventaParaTicket = nuevaVenta;
                _idVentaGenerada = idVentaGenerada;
                GenerarTicketSinVisualizar();

                MessageBox.Show(
                    "Venta registrada correctamente. Presione RECEIPT para ver el comprobante.",
                    "Venta exitosa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // 6. Limpiar la UI para el próximo cliente
                _carrito.Clear();
                ActualizarTotal();

                // 7. Recargar el catálogo para reflejar el nuevo stock
                //    (los productos agotados dejan de estar disponibles)
                RecargarProductos();

                // Opcional: resetear el método de pago por defecto
                _cmbMetodoPago.SelectedIndex = 0;
            }
            catch (StockInsuficienteException ex)
            {
                // La validación de la BLL o el SP detectaron que el stock ya no alcanza.
                MessageBox.Show(ex.Message, "Stock insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                // Si falló algo en la BD (ej. se cortó la conexión), el Rollback ya se hizo en la DAL
                MessageBox.Show($"Ocurrió un error al guardar la venta:\n{ex.Message}", "Error BD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerarTicketSinVisualizar()
        {
            try
            {
                _rutaUltimoTicket = GenerarTicketDesdeUltimaVenta();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"La venta se registró correctamente, pero no se pudo generar el ticket:\n{ex.Message}",
                    "Ticket", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            if (_ventaParaTicket is null)
            {
                MessageBox.Show("Primero debe registrarse una venta.", "Ticket",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                string rutaPdf = _rutaUltimoTicket ?? GenerarTicketDesdeUltimaVenta();
                if (!File.Exists(rutaPdf))
                    rutaPdf = GenerarTicketDesdeUltimaVenta();

                _rutaUltimoTicket = rutaPdf;
                AbrirPdf(_rutaUltimoTicket);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo generar el ticket:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerarTicketDesdeUltimaVenta()
        {
            if (_ventaParaTicket is null)
                throw new InvalidOperationException("No hay una venta registrada para generar el ticket.");

            var ticket = new TicketVenta
            {
                IdVenta = _idVentaGenerada,
                FechaHora = DateTime.Now,
                NombreMinimercado = TicketService.NombreMinimercado,
                NombreCajero = SesionGlobal.NombreCompletoActual,
                MetodoPago = _ventaParaTicket.MetodoPago,
                Total = _ventaParaTicket.Total,
                MontoAbonado = _ventaParaTicket.MontoAbonado,
                Vuelto = _ventaParaTicket.Vuelto,
                Detalles = _ventaParaTicket.Detalles.Select(d => new DetalleTicket
                {
                    NombreProducto = d.NombreProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList()
            };

            return _ticketService.GenerarTicket(ticket);
        }

        private static void AbrirPdf(string rutaPdf)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = rutaPdf,
                UseShellExecute = true
            });
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
        int nuevaCantidad = item.Cantidad + variacion;

        // Si al restar llega a 0, lo quitamos del carrito
        if (nuevaCantidad <= 0)
        {
            _carrito.Remove(item);
            ActualizarTotal();
            return;
        }

        // No permitimos superar el stock disponible del producto
        Producto? producto = ObtenerProductoDelCatalogo(item.IdProducto);
        if (producto != null &&
            !_productoBLL.ValidarCantidadDisponible(producto, nuevaCantidad, out string mensaje))
        {
            MessageBox.Show(mensaje, "Stock insuficiente",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        item.Cantidad = nuevaCantidad;
        ActualizarTotal(); // Tu método existente
    }

    private void FijarCantidadExacta(DetalleVenta item, int cantidadExacta)
    {
        Producto? producto = ObtenerProductoDelCatalogo(item.IdProducto);

        // Ajustamos la cantidad al stock disponible si se intenta superar
        if (producto != null && cantidadExacta > producto.StockActual)
        {
            cantidadExacta = producto.StockActual;
            MessageBox.Show($"La cantidad se ajustó al stock disponible ({producto.StockActual}).",
                "Stock insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        if (cantidadExacta <= 0)
        {
            _carrito.Remove(item);
            ActualizarTotal();
            return;
        }

        item.Cantidad = cantidadExacta;
        
        dgVentas.Items.Refresh();
        ActualizarTotal();
    }

}
}
