using SistemaVentas.BLL;
using SistemaVentas.Entities;
using SistemaVentas.GUI.Contexto;
using System;
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
    public partial class MainWindow : Window
    { 

        private UsuarioBLL _usuarioLogica = new UsuarioBLL();

        public MainWindow()
        {
            InitializeComponent();

            // Llamás a este método apenas arranca la ventana principal
            ConfigurarMenu();
        }

        private void ConfigurarMenu()
        {
            // Traemos el rol de la sesión global
            int idRolActual = SesionGlobal.UsuarioActual.IdRol;

            // Le preguntamos a la BLL si tiene acceso
            if (!_usuarioLogica.PuedeAccederPantallaUsuarios(idRolActual))
            {
               
                btnUsuarios.IsEnabled = false; 
            }
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