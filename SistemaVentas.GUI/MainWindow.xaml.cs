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
            if (SesionGlobal.HayUsuarioLogueado)
            {
                txtNombreUsuario.Text = $"Hola, {SesionGlobal.NombreUsuarioActual}";
            }
            else
            {
                txtNombreUsuario.Text = "Usuario no identificado";
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

        private void btnCompra_Click(object sender, RoutedEventArgs e)
        {
            ContenedorPrincipal.Content = new Compra();
        }
        // Nuevo método para Cerrar Sesión
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            // 1. Limpiamos la variable global
            SesionGlobal.CerrarSesion();

            // 2. Abrimos la ventana de Login (Asegúrate de poner el nombre correcto de tu ventana de Login)
            Login ventanaLogin = new Login();
            ventanaLogin.Show();

            // 3. Cerramos el MainWindow actual
            this.Close();
        }
    }
}