using System;
using System.Windows;
using System.Windows.Controls;
using SistemaVentas.BLL;
using SistemaVentas.Entities;

namespace SistemaVentas.GUI
{
    public partial class ProductoControl : UserControl
    {
        private CN_Producto objCN_Producto = new CN_Producto();
        private Producto? _productoEnEdicion;

        public ProductoControl()
        {
            InitializeComponent();
            CargarCategorias();
            CargarProductos();
            GenerarCodigoBarras();
        }

        private void CargarProductos()
        {
            try
            {
                bool incluirInactivos = chkMostrarInactivos?.IsChecked == true;
                dgvProductos.ItemsSource = objCN_Producto.Listar(incluirInactivos);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al cargar la lista de productos:\n{ex.Message}",
                                "Error de Conexión",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void CargarCategorias()
        {
            try
            {
                cmbCategoria.ItemsSource = objCN_Producto.ListarCategorias();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al cargar las categorías:\n{ex.Message}",
                                "Error de Conexión",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void GenerarCodigoBarras()
        {
            try
            {
                txtCodigoBarras.Text = objCN_Producto.GenerarCodigoBarrasUnico();
            }
            catch (Exception ex)
            {
                txtCodigoBarras.Text = string.Empty;
                MessageBox.Show($"No se pudo generar el código de barras:\n{ex.Message}",
                                "Atención",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCategoria.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar una categoría.",
                    "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal precioCosto;
            if (_productoEnEdicion != null)
            {
                precioCosto = _productoEnEdicion.PrecioCosto;
            }
            else if (!decimal.TryParse(txtPrecioCosto.Text, out precioCosto))
            {
                MessageBox.Show("Revise el campo numérico del precio de costo.",
                    "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrecioVenta.Text, out decimal precioVenta) ||
                !int.TryParse(txtStock.Text, out int stock))
            {
                MessageBox.Show("Revise los campos numéricos (precio venta, stock).",
                    "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Producto objProducto = new Producto()
            {
                IdProducto = _productoEnEdicion?.IdProducto ?? 0,
                Nombre = txtNombre.Text.Trim(),
                PrecioCosto = precioCosto,
                PrecioVenta = precioVenta,
                StockActual = stock,
                CodigoBarras = _productoEnEdicion != null
                    ? _productoEnEdicion.CodigoBarras
                    : txtCodigoBarras.Text,
                Activo = _productoEnEdicion?.Activo ?? true,
                StockMinimo = _productoEnEdicion?.StockMinimo ?? 0,
                oCategoria = new Categoria()
                {
                    IdCategoria = (int)cmbCategoria.SelectedValue
                }
            };

            string mensaje = string.Empty;
            bool resultado = false;

            // Si IdProducto es 0 REGISTRA, si es distinto de 0 EDITA
            if (objProducto.IdProducto == 0)
            {
                resultado = objCN_Producto.Registrar(objProducto, out mensaje);
            }
            else
            {
                resultado = objCN_Producto.Editar(objProducto, out mensaje);
            }

            if (resultado)
            {
                MessageBox.Show("Operación realizada con éxito", "Atención",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                CargarProductos();
                LimpiarFormulario();
            }
            else
            {
                MessageBox.Show(mensaje, "Atención",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not Producto seleccionado)
                return;

            _productoEnEdicion = seleccionado;
            txtIdProducto.Text = seleccionado.IdProducto.ToString();
            txtCodigoBarras.Text = seleccionado.CodigoBarras;
            txtNombre.Text = seleccionado.Nombre;
            txtPrecioCosto.Text = seleccionado.PrecioCosto.ToString();
            txtPrecioCosto.IsReadOnly = true;
            txtPrecioVenta.Text = seleccionado.PrecioVenta.ToString();
            txtStock.Text = seleccionado.StockActual.ToString();
            cmbCategoria.SelectedValue = seleccionado.oCategoria.IdCategoria;

            btnGuardar.Content = "Actualizar Producto";
        }

        private void chkMostrarInactivos_Checked(object sender, RoutedEventArgs e)
        {
            CargarProductos();
        }

        private void chkMostrarInactivos_Unchecked(object sender, RoutedEventArgs e)
        {
            CargarProductos();
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not Producto seleccionado)
                return;

            if (!seleccionado.Activo)
            {
                MessageBox.Show("El producto ya se encuentra dado de baja.",
                    "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"¿Dar de baja el producto '{seleccionado.Nombre}'? Dejará de estar disponible para la venta.",
                "Confirmar Baja",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            string mensaje = string.Empty;
            bool respuesta = objCN_Producto.Eliminar(seleccionado, out mensaje);

            if (respuesta)
            {
                MessageBox.Show("Producto dado de baja correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                CargarProductos();
                LimpiarFormulario();
            }
            else
            {
                MessageBox.Show(mensaje, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnReactivar_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not Producto seleccionado)
                return;

            MessageBoxResult result = MessageBox.Show(
                $"¿Reactivar el producto '{seleccionado.Nombre}'? Volverá a estar disponible para la venta.",
                "Confirmar Reactivación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            bool estadoAnterior = seleccionado.Activo;
            seleccionado.Activo = true;

            string mensaje = string.Empty;
            bool respuesta = objCN_Producto.Editar(seleccionado, out mensaje);

            if (respuesta)
            {
                MessageBox.Show("Producto reactivado correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                CargarProductos();
                LimpiarFormulario();
            }
            else
            {
                seleccionado.Activo = estadoAnterior;
                MessageBox.Show(mensaje, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            _productoEnEdicion = null;
            txtIdProducto.Text = "0";
            txtCodigoBarras.Clear();
            GenerarCodigoBarras();
            txtNombre.Clear();
            txtPrecioCosto.Clear();
            txtPrecioCosto.IsReadOnly = false;
            txtPrecioVenta.Clear();
            txtStock.Clear();
            cmbCategoria.SelectedIndex = -1;
            btnGuardar.Content = "Guardar Producto";
        }
    }
}