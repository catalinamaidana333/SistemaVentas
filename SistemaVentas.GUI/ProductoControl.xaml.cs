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
            dgvProductos.ItemsSource = objCN_Producto.Listar();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            Producto objProducto = new Producto()
            {
                Nombre = txtNombre.Text.Trim(),
                PrecioCosto = Convert.ToDecimal(txtPrecioCosto.Text),
                PrecioVenta = Convert.ToDecimal(txtPrecioVenta.Text),
                StockActual = Convert.ToInt32(txtStock.Text),
                CodigoBarras = "",
                oCategoria = new Categoria() { IdCategoria = 1 }
            };

            string mensaje = string.Empty;
            bool resultado = new CN_Producto().Registrar(objProducto, out mensaje);

            if (resultado)
            {
                MessageBox.Show("Producto guardado con éxito", "Atención",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                dgvProductos.ItemsSource = new CN_Producto().Listar();
            }
            else
            {
                MessageBox.Show(mensaje, "Atención",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
