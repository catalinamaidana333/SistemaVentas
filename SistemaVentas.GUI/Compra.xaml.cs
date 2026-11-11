using SistemaVentas.BLL;
using SistemaVentas.Entities;
using SistemaVentas.GUI.Contexto;
using SistemaVentas.GUI.Dialogs;

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SistemaVentas.GUI
{
    public partial class Compra : UserControl
    {
        private readonly ObservableCollection<DetalleCompra> _carrito;
        private readonly ObservableCollection<Producto> _productos;
        private readonly ObservableCollection<Proveedor> _proveedores;
        private readonly CompraBLL _compraBLL;
        private readonly CN_Producto _productoBLL;
        private readonly CN_Proveedor _proveedorBLL;

        public Compra()
        {
            InitializeComponent();

            _carrito = new ObservableCollection<DetalleCompra>();
            _productos = new ObservableCollection<Producto>();
            _proveedores = new ObservableCollection<Proveedor>();
            _compraBLL = new CompraBLL();
            _productoBLL = new CN_Producto();
            _proveedorBLL = new CN_Proveedor();

            dgCompras.ItemsSource = _carrito;
            cmbProductos.ItemsSource = _productos;
            cmbProveedor.ItemsSource = _proveedores;
            txtUsuarioActual.Text = $"⚙️ {SesionGlobal.NombreUsuarioActual}";

            CargarProductos();
            CargarProveedores();
            ActualizarPanelOrigen();
            ActualizarTotal();
        }

        private void CargarProductos()
        {
            try
            {
                _productos.Clear();
                foreach (Producto producto in _productoBLL.Listar().Where(producto => producto.Activo))
                    _productos.Add(producto);

                cmbProductos.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo cargar el catálogo de productos.\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarProveedores(int idSeleccionar = 0)
        {
            try
            {
                _proveedores.Clear();

                var proveedores = _proveedorBLL.Listar();

                if (proveedores.Count == 0)
                {
                    SembrarProveedoresDemo();
                    proveedores = _proveedorBLL.Listar();
                }

                foreach (Proveedor proveedor in proveedores)
                    _proveedores.Add(proveedor);

                if (_proveedores.Count == 0)
                {
                    foreach (Proveedor proveedor in ObtenerProveedoresDemo())
                        _proveedores.Add(proveedor);
                }

                if (idSeleccionar > 0)
                    cmbProveedor.SelectedItem = _proveedores.FirstOrDefault(p => p.IdProveedor == idSeleccionar);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo cargar el listado de proveedores.\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SembrarProveedoresDemo()
        {
            foreach (Proveedor proveedor in ObtenerProveedoresDemo())
                _proveedorBLL.Registrar(proveedor, out _);
        }

        private static Proveedor[] ObtenerProveedoresDemo()
        {
            return new[]
            {
                new Proveedor
                {
                    RazonSocial = "Distribuidora Central S.A.",
                    Cuit = "30-71234567-1",
                    Telefono = "11-4000-1000",
                    NombreContacto = "Compras",
                    Activo = true
                },
                new Proveedor
                {
                    RazonSocial = "Almacén del Sur S.R.L.",
                    Cuit = "30-69876543-3",
                    Telefono = "11-4000-2000",
                    NombreContacto = "Ventas",
                    Activo = true
                }
            };
        }

        private void cmbProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProductos.SelectedItem is not Producto producto)
                return;

            txtCostoUnitario.Text = producto.PrecioCosto.ToString("0.00", CultureInfo.CurrentCulture);

            if (string.IsNullOrWhiteSpace(txtCantidad.Text))
                txtCantidad.Text = "1";
        }

        private void btnAgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProductos.SelectedItem is not Producto producto)
            {
                MessageBox.Show("Seleccione un producto del catálogo.", "Producto requerido",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtCantidad.Text.Trim(), out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingrese una cantidad válida mayor a 0.", "Cantidad inválida",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryParseDecimal(txtCostoUnitario.Text, out decimal costo) || costo <= 0)
            {
                MessageBox.Show("Ingrese un costo unitario válido mayor a 0.", "Costo inválido",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DetalleCompra? itemExistente = _carrito.FirstOrDefault(d => d.IdProducto == producto.IdProducto);

            if (itemExistente != null)
            {
                itemExistente.Cantidad += cantidad;
                dgCompras.Items.Refresh();
            }
            else
            {
                _carrito.Add(new DetalleCompra
                {
                    IdProducto = producto.IdProducto,
                    NombreProducto = producto.Nombre,
                    Cantidad = cantidad,
                    CostoUnitario = costo,
                    StockActual = producto.StockActual
                });
            }

            cmbProductos.SelectedIndex = -1;
            cmbProductos.Text = string.Empty;
            txtCantidad.Text = "1";
            txtCostoUnitario.Text = string.Empty;

            ActualizarTotal();
        }

        private void btnSumar_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is DetalleCompra item)
                ModificarCantidad(item, 1);
        }

        private void btnRestar_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is DetalleCompra item)
                ModificarCantidad(item, -1);
        }

        private void dgCompras_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
                return;

            if (dgCompras.SelectedItem is not DetalleCompra item)
                return;

            if (e.Key == Key.Add || e.Key == Key.OemPlus)
            {
                ModificarCantidad(item, 1);
                e.Handled = true;
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                ModificarCantidad(item, -1);
                e.Handled = true;
            }
            else if (e.Key >= Key.D1 && e.Key <= Key.D9)
            {
                FijarCantidadExacta(item, e.Key - Key.D0);
                e.Handled = true;
            }
            else if (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad9)
            {
                FijarCantidadExacta(item, e.Key - Key.NumPad0);
                e.Handled = true;
            }
        }

        private void dgCompras_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit || e.Row.Item is not DetalleCompra item)
                return;

            if (e.EditingElement is TextBox editor)
            {
                string texto = editor.Text?.Trim() ?? string.Empty;
                string encabezado = e.Column.Header?.ToString() ?? string.Empty;

                if (encabezado == "QTY IN")
                {
                    if (!int.TryParse(texto, out int cantidad) || cantidad <= 0)
                    {
                        MessageBox.Show("La cantidad debe ser un número mayor a 0.", "Cantidad inválida",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        e.Cancel = true;
                        return;
                    }

                    item.Cantidad = cantidad;
                }
                else if (encabezado == "UNIT COST")
                {
                    if (!TryParseDecimal(texto, out decimal costo) || costo <= 0)
                    {
                        MessageBox.Show("El costo unitario debe ser un número mayor a 0.", "Costo inválido",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        e.Cancel = true;
                        return;
                    }

                    item.CostoUnitario = costo;
                }
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                dgCompras.Items.Refresh();
                ActualizarTotal();
            }), DispatcherPriority.Background);
        }

        private void ModificarCantidad(DetalleCompra item, int variacion)
        {
            if (item.Cantidad + variacion <= 0)
                _carrito.Remove(item);
            else
            {
                item.Cantidad += variacion;
                dgCompras.Items.Refresh();
            }

            ActualizarTotal();
        }

        private void FijarCantidadExacta(DetalleCompra item, int cantidadExacta)
        {
            if (cantidadExacta <= 0)
                _carrito.Remove(item);
            else
            {
                item.Cantidad = cantidadExacta;
                dgCompras.Items.Refresh();
            }

            ActualizarTotal();
        }

        private void btnEliminarLinea_Click(object sender, RoutedEventArgs e)
        {
            if (dgCompras.SelectedItem is DetalleCompra item)
            {
                _carrito.Remove(item);
                ActualizarTotal();
            }
        }

        private void btnSubir_Click(object sender, RoutedEventArgs e)
        {
            if (dgCompras.SelectedItem is not DetalleCompra item)
                return;

            int indice = _carrito.IndexOf(item);
            if (indice <= 0)
                return;

            _carrito.Move(indice, indice - 1);
            dgCompras.SelectedItem = item;
            dgCompras.ScrollIntoView(item);
        }

        private void btnBajar_Click(object sender, RoutedEventArgs e)
        {
            if (dgCompras.SelectedItem is not DetalleCompra item)
                return;

            int indice = _carrito.IndexOf(item);
            if (indice < 0 || indice >= _carrito.Count - 1)
                return;

            _carrito.Move(indice, indice + 1);
            dgCompras.SelectedItem = item;
            dgCompras.ScrollIntoView(item);
        }

        private void rbEfectivo_Checked(object sender, RoutedEventArgs e)
        {
            if (panelOrigenEfectivo != null)
                panelOrigenEfectivo.Visibility = Visibility.Visible;
        }

        private void rbMercadoPago_Checked(object sender, RoutedEventArgs e)
        {
            if (panelOrigenEfectivo != null)
                panelOrigenEfectivo.Visibility = Visibility.Collapsed;
        }

        private void ActualizarPanelOrigen()
        {
            panelOrigenEfectivo.Visibility = rbEfectivo.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void ActualizarTotal()
        {
            decimal total = _carrito.Sum(item => item.Subtotal);
            int cantidadItems = _carrito.Sum(item => item.Cantidad);

            txtTotal.Text = $"$ {total:N2}";
            txtCantidadItems.Text = cantidadItems == 1 ? "1 item" : $"{cantidadItems} items";
        }

        private void btnProcesarCompra_Click(object sender, RoutedEventArgs e)
        {
            bool esEfectivo = rbEfectivo.IsChecked == true;
            bool esMercadoPago = rbMercadoPago.IsChecked == true;
            bool esCaja = esEfectivo && rbCaja.IsChecked == true;

            if (esCaja && SesionGlobal.IdCajaUsuarioActual <= 0)
            {
                MessageBox.Show("Debe abrir un turno de caja antes de registrar una compra en efectivo desde la caja.",
                    "Caja cerrada", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Proveedor? proveedor = ObtenerProveedorSeleccionado();
            if (proveedor == null)
            {
                MessageBox.Show("Seleccione un proveedor o registre uno nuevo.", "Proveedor requerido",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_carrito.Count == 0)
            {
                MessageBox.Show("Agregue al menos un producto antes de procesar la compra.", "Compra vacía",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!esEfectivo && !esMercadoPago)
            {
                MessageBox.Show("Seleccione un método de pago válido.", "Método de pago requerido",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (proveedor.IdProveedor <= 0)
            {
                MessageBox.Show("El proveedor seleccionado no está registrado en la base de datos.",
                    "Proveedor inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SesionGlobal.IdUsuarioActual <= 0)
            {
                MessageBox.Show("No hay un usuario autenticado para registrar la compra.", "Sesión inválida",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string metodoPago;
            string? origenEfectivo = null;
            int? idCajaUsuario = null;

            if (esEfectivo)
            {
                metodoPago = "Efectivo";

                if (rbCaja.IsChecked == true)
                {
                    origenEfectivo = "Caja";
                    idCajaUsuario = SesionGlobal.IdCajaUsuarioActual;
                }
                else if (rbExterna.IsChecked == true)
                {
                    origenEfectivo = "Externa";
                }
                else
                {
                    MessageBox.Show("Seleccione el origen del efectivo.", "Origen requerido",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                metodoPago = "Mercado Pago";
            }

            SistemaVentas.Entities.Compra compra = new SistemaVentas.Entities.Compra
            {
                IdUsuario = SesionGlobal.IdUsuarioActual,
                IdCajaUsuario = idCajaUsuario,
                IdProveedor = proveedor.IdProveedor,
                MetodoPago = metodoPago,
                Total = _carrito.Sum(item => item.Subtotal),
                NumeroFactura = txtNumeroFactura.Text.Trim(),
                Observaciones = txtObservaciones.Text.Trim(),
                OrigenEfectivo = origenEfectivo,
                Detalles = _carrito.ToList()
            };

            try
            {
                int idCompra = _compraBLL.RegistrarCompra(compra, out System.Collections.Generic.List<AlertaMargen> alertas);

                MessageBox.Show($"Compra #{idCompra} registrada correctamente.\nTotal: ${compra.Total:N2}",
                    "Compra registrada", MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarFormulario();
                CargarProductos();

                if (alertas.Count > 0)
                {
                    DialogoAlertaMargen dialogo = new DialogoAlertaMargen(alertas)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    dialogo.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo registrar la compra:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnNuevoProveedor_Click(object sender, RoutedEventArgs e)
        {
            DialogoProveedor dialogo = new DialogoProveedor
            {
                Owner = Window.GetWindow(this)
            };

            if (dialogo.ShowDialog() != true)
                return;

            int idCreado = dialogo.ProveedorCreado?.IdProveedor ?? 0;
            CargarProveedores(idCreado);
        }

        private void LimpiarFormulario()
        {
            _carrito.Clear();
            txtNumeroFactura.Clear();
            txtObservaciones.Clear();
            txtCantidad.Text = "1";
            txtCostoUnitario.Clear();
            cmbProductos.SelectedIndex = -1;
            cmbProductos.Text = string.Empty;
            ActualizarTotal();
        }

        private Proveedor? ObtenerProveedorSeleccionado()
        {
            string texto = cmbProveedor.Text?.Trim() ?? string.Empty;

            if (texto.Length > 0)
            {
                Proveedor? porTexto = _proveedores.FirstOrDefault(p => p.IdProveedor > 0 &&
                    string.Equals(p.RazonSocial, texto, StringComparison.OrdinalIgnoreCase));

                if (porTexto != null)
                    return porTexto;

                return null;
            }

            if (cmbProveedor.SelectedItem is Proveedor seleccionado && seleccionado.IdProveedor > 0)
                return seleccionado;

            return null;
        }

        private static bool TryParseDecimal(string? texto, out decimal valor)
        {
            return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out valor)
                || decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
        }
    }
}
