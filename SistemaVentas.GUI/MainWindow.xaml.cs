using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SistemaVentas.BLL;
using System;
using SistemaVentas.Entities;

namespace SistemaVentas.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CN_Producto objCN_Producto = new CN_Producto();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Carga la lista de productos traídos de SQL Server en la DataGrid
            dgvProductos.ItemsSource = objCN_Producto.Listar();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validar parsing de precios y stock...

            Producto objProducto = new Producto()
            {
                Nombre = txtNombre.Text.Trim(),
                PrecioCosto = Convert.ToDecimal(txtPrecioCosto.Text),
                PrecioVenta = Convert.ToDecimal(txtPrecioVenta.Text),
                StockActual = Convert.ToInt32(txtStock.Text),
                CodigoBarras = "", // Código de barras opcional/vacío
                oCategoria = new Categoria() { IdCategoria = 1 }
            };

            string mensaje = string.Empty;
            bool resultado = new CN_Producto().Registrar(objProducto, out mensaje);

            if (resultado)
            {
                MessageBox.Show("Producto guardado con éxito", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                // Limpiar campos o recargar grilla...
                dgvProductos.ItemsSource = new CN_Producto().Listar();
            }
            else
            {
                MessageBox.Show(mensaje, "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}