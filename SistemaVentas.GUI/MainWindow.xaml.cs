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
            CargarMenuSegunRol();
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

        // Evento Vendedor
        private void btnSubArqueoDiario_Click(object sender, RoutedEventArgs e)
             => ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Vendedor.ArqueoDiarioView(1);
        

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
            else if (idRol == 3) // Supervisor
            {
                btnSubStockCritico.Visibility = Visibility.Visible;
                btnSubVentasCanceladas.Visibility = Visibility.Visible;
                btnSubVentasVendedor.Visibility = Visibility.Visible;

                ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Supervisor.StockCriticoView();
            }
            else if (idRol == 2) // Vendedor

                btnSubArqueoDiario.Visibility = Visibility.Visible;
            {
                ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Vendedor.ArqueoDiarioView(1);
            }
        }

        private void btnCaja_Click(object sender, RoutedEventArgs e)
        {
       
            // Reemplaza 'ContenedorPrincipal' por el x:Name del ContentControl/Grid de tu MainWindow
            ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.CajaView();
        }
        private void CargarMenuSegunRol()
        {
            int idRol = SesionGlobal.UsuarioActual.IdRol;

            // Solo habilitar el botón de Cajas si idRol es 1 (Gerente)
            if (idRol == 1)
            {
                btnCaja.Visibility = Visibility.Visible;
            }
            else
            {
                btnCaja.Visibility = Visibility.Collapsed;
            }
        }
    }
}
