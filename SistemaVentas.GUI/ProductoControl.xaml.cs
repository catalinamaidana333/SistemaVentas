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

        public ProductoControl()
        {
            InitializeComponent();
            CargarCategorias();
            CargarProductos();
        }

        private void CargarProductos()
        {
            try
            {
                dgvProductos.ItemsSource = objCN_Producto.Listar();
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

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCategoria.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar una categoría.",
                    "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrecioCosto.Text, out decimal precioCosto) ||
                !decimal.TryParse(txtPrecioVenta.Text, out decimal precioVenta) ||
                !int.TryParse(txtStock.Text, out int stock))
            {
                MessageBox.Show("Revise los campos numéricos (precio costo, precio venta, stock).",
                    "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Producto objProducto = new Producto()
            {
                IdProducto = Convert.ToInt32(txtIdProducto.Text),
                Nombre = txtNombre.Text.Trim(),
                PrecioCosto = precioCosto,
                PrecioVenta = precioVenta,
                StockActual = stock,
                CodigoBarras = null,
                Activo = true,
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
            Button btn = sender as Button;
            Producto seleccionado = btn.DataContext as Producto;

            if (seleccionado != null)
            {
                txtIdProducto.Text = seleccionado.IdProducto.ToString();
                txtNombre.Text = seleccionado.Nombre;
                txtPrecioCosto.Text = seleccionado.PrecioCosto.ToString();
                txtPrecioVenta.Text = seleccionado.PrecioVenta.ToString();
                txtStock.Text = seleccionado.StockActual.ToString();
                cmbCategoria.SelectedValue = seleccionado.oCategoria.IdCategoria;

                btnGuardar.Content = "Actualizar Producto";
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Producto seleccionado = btn.DataContext as Producto;

            if (seleccionado != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"¿Está seguro de eliminar el producto '{seleccionado.Nombre}'?",
                    "Confirmar Eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    string mensaje = string.Empty;
                    bool respuesta = objCN_Producto.Eliminar(seleccionado, out mensaje);

                    if (respuesta)
                    {
                        MessageBox.Show("Producto eliminado correctamente.", "Éxito",
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
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            txtIdProducto.Text = "0";
            txtNombre.Clear();
            txtPrecioCosto.Clear();
            txtPrecioVenta.Clear();
            txtStock.Clear();
            cmbCategoria.SelectedIndex = -1;
            btnGuardar.Content = "Guardar Producto";
        }
    }
}