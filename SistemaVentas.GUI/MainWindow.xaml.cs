using SistemaVentas.BLL;
using SistemaVentas.Entities;
using SistemaVentas.GUI.Contexto;
using SistemaVentas.GUI;
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

        // Eventos Gerente
        private void btnSubRentabilidad_Click(object sender, RoutedEventArgs e)
            => ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Gerente.RentabilidadView();

        private void btnSubAuditoria_Click(object sender, RoutedEventArgs e)
            => ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Gerente.AuditoriaView();

        private void btnSubRendimiento_Click(object sender, RoutedEventArgs e)
            => ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Gerente.RendimientoMensualView();

        // Eventos Supervisor
        private void btnSubStockCritico_Click(object sender, RoutedEventArgs e)
            => ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Supervisor.StockCriticoView();

        private void btnSubVentasCanceladas_Click(object sender, RoutedEventArgs e)
            => ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Supervisor.VentasCanceladasView();

        private void btnSubVentasVendedor_Click(object sender, RoutedEventArgs e)
            => ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Supervisor.VentasVendedorView();

        private void OcultarSubmenus()
        {
            btnSubRentabilidad.Visibility = Visibility.Collapsed;
            btnSubAuditoria.Visibility = Visibility.Collapsed;
            btnSubRendimiento.Visibility = Visibility.Collapsed;

            btnSubStockCritico.Visibility = Visibility.Collapsed;
            btnSubVentasCanceladas.Visibility = Visibility.Collapsed;
            btnSubVentasVendedor.Visibility = Visibility.Collapsed;
        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {
            int idRol = SesionGlobal.UsuarioActual.IdRol;

            OcultarSubmenus();

            if (idRol == 1) // Gerente
            {
                btnSubRentabilidad.Visibility = Visibility.Visible;
                btnSubAuditoria.Visibility = Visibility.Visible;
                btnSubRendimiento.Visibility = Visibility.Visible;

                ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Gerente.RentabilidadView();
            }
            else if (idRol == 2) // Supervisor
            {
                btnSubStockCritico.Visibility = Visibility.Visible;
                btnSubVentasCanceladas.Visibility = Visibility.Visible;
                btnSubVentasVendedor.Visibility = Visibility.Visible;

                ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Supervisor.StockCriticoView();
            }
            else if (idRol == 3) // Vendedor
            {
                ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Vendedor.ArqueoDiarioView(1);
            }
        }
    }
}
