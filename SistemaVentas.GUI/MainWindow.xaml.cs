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
            ConfigurarMenuPorRol();
        }

        private void ConfigurarMenuPorRol()
        {
            if (SesionGlobal.UsuarioActual == null) return;

            int idRolActual = SesionGlobal.UsuarioActual.IdRol;

            // Ocultamos todos los submenús al arrancar
            OcultarSubmenus();

            // Mapeo según tu Base de Datos:
            // 1: GERENTE | 2: VENDEDOR | 3: SUPERVISOR
            switch (idRolActual)
            {
                case 1: // GERENTE
                    btnUsuarios.Visibility = Visibility.Visible;
                    btnCompra.Visibility = Visibility.Visible;
                    btnProductos.Visibility = Visibility.Visible;
                    btnReportes.Visibility = Visibility.Visible;
                    break;

                case 2: // VENDEDOR (Sin acceso a Usuarios, Compras ni Productos)
                    btnUsuarios.Visibility = Visibility.Collapsed;
                    btnCompra.Visibility = Visibility.Collapsed;
                    btnProductos.Visibility = Visibility.Collapsed;
                    btnReportes.Visibility = Visibility.Visible;
                    break;

                case 3: // SUPERVISOR
                    btnUsuarios.Visibility = Visibility.Collapsed;
                    btnCompra.Visibility = Visibility.Visible;
                    btnProductos.Visibility = Visibility.Visible;
                    btnReportes.Visibility = Visibility.Visible;
                    break;
            }

            // Configurar acceso al módulo de Backups (solo Gerente)
            if (idRolActual != 1) // idRol == 1 es Gerente
            {
                btnBackup.IsEnabled = false;
            }

            if (SesionGlobal.HayUsuarioLogueado)
            {
                txtNombreUsuario.Text = $"Hola, {SesionGlobal.NombreUsuarioActual}";
            }
            else
            {
                txtNombreUsuario.Text = "Usuario no identificado";
            }
            CargarMenuSegunRol();
        }

        private void OcultarSubmenus()
        {
            btnSubRentabilidad.Visibility = Visibility.Collapsed;
            btnSubAuditoria.Visibility = Visibility.Collapsed;
            btnSubRendimiento.Visibility = Visibility.Collapsed;

            btnSubStockCritico.Visibility = Visibility.Collapsed;
            btnSubVentasCanceladas.Visibility = Visibility.Collapsed;
            btnSubVentasVendedor.Visibility = Visibility.Collapsed;
        }

        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            OcultarSubmenus();
            ContenedorPrincipal.Content = new Ventas();
        }

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            OcultarSubmenus();
            ContenedorPrincipal.Content = new ProductoControl();
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            OcultarSubmenus();
            ContenedorPrincipal.Content = new UsuarioControl();
        }

        private void btnCompra_Click(object sender, RoutedEventArgs e)
        {
            OcultarSubmenus();
            ContenedorPrincipal.Content = new Compra();
        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {
            if (SesionGlobal.UsuarioActual == null) return;

            int idRol = SesionGlobal.UsuarioActual.IdRol;
            int idUsuario = SesionGlobal.UsuarioActual.IdUsuario;

            OcultarSubmenus();

            if (idRol == 1) // 1 = GERENTE
            {
                btnSubRentabilidad.Visibility = Visibility.Visible;
                btnSubAuditoria.Visibility = Visibility.Visible;
                btnSubRendimiento.Visibility = Visibility.Visible;

                ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Gerente.RentabilidadView();
            }
            else if (idRol == 2) // 2 = VENDEDOR
            {
                // El Vendedor solo ve su Arqueo Diario directamente y sin submenús
                ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Vendedor.ArqueoDiarioView(idUsuario);
            }
            else if (idRol == 3) // 3 = SUPERVISOR
            {
                btnSubStockCritico.Visibility = Visibility.Visible;
                btnSubVentasCanceladas.Visibility = Visibility.Visible;
                btnSubVentasVendedor.Visibility = Visibility.Visible;

                ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Supervisor.StockCriticoView();
            }
        }

        // Submenús Gerente
        private void btnSubRentabilidad_Click(object sender, RoutedEventArgs e)
            => ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Gerente.RentabilidadView();

        private void btnSubAuditoria_Click(object sender, RoutedEventArgs e)
            => ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Gerente.AuditoriaView();

        private void btnSubRendimiento_Click(object sender, RoutedEventArgs e)
            => ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Gerente.RendimientoMensualView();

        // Submenús Supervisor
        private void btnSubStockCritico_Click(object sender, RoutedEventArgs e)
            => ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Supervisor.StockCriticoView();

        private void btnSubVentasCanceladas_Click(object sender, RoutedEventArgs e)
            => ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Supervisor.VentasCanceladasView();

        private void btnSubVentasVendedor_Click(object sender, RoutedEventArgs e)
            => ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Supervisor.VentasVendedorView();
    }
}