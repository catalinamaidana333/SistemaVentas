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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Pantalla inicial al abrir la aplicación
            ContenedorPrincipal.Content = new UsuarioControl();
        }

        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            ContenedorPrincipal.Content = new Ventas();
        }

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            ContenedorPrincipal.Content = new ProductoControl();
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            ContenedorPrincipal.Content = new UsuarioControl();
        }

       
    }
}