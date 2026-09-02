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

namespace SistemaVentas.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // pantalla inicial al abrir MainWindow
            ContenedorPrincipal.Content = new UsuarioControl();
        }
        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            // Reemplaza el contenido con la vista de Usuarios
            ContenedorPrincipal.Content = new UsuarioControl();
        }

        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            // Reemplaza el contenido con la vista de Ventas
            ContenedorPrincipal.Content = new Ventas();
        }
    }
}