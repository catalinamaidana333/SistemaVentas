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

        // 🔹 Nuevo: llena el ComboBox con las categorías de la BD
        private void CargarCategorias()
        {
            cmbCategoria.ItemsSource = objCN_Producto.ListarCategorias();
        }

        // 🔹 Nuevo: centraliza la carga de la grilla (lo llamas también al guardar)
        private void CargarProductos()
        {
            dgvProductos.ItemsSource = objCN_Producto.Listar();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // 🔹 Nuevo: validar que se haya elegido categoría
            if (cmbCategoria.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar una categoría.",
                    "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 🔹 Nuevo: validar parseo antes de convertir (evita FormatException)
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
                Nombre = txtNombre.Text.Trim(),
                PrecioCosto = precioCosto,
                PrecioVenta = precioVenta,
                StockActual = stock,
                CodigoBarras = null,
                oCategoria = new Categoria()
                {
                    // 🔹 Cambio clave: usa la categoría elegida, ya no el "1" fijo
                    IdCategoria = (int)cmbCategoria.SelectedValue
                }
            };

            string mensaje = string.Empty;
            bool resultado = objCN_Producto.Registrar(objProducto, out mensaje);

            if (resultado)
            {
                MessageBox.Show("Producto guardado con éxito", "Atención",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // 🔹 Refresca la grilla usando el mismo método
                CargarProductos();

                // 🔹 Limpia los campos (opcional, pero cómodo)
                txtNombre.Clear();
                txtPrecioCosto.Clear();
                txtPrecioVenta.Clear();
                txtStock.Clear();
                cmbCategoria.SelectedIndex = -1;
            }
            else
            {
                MessageBox.Show(mensaje, "Atención",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
