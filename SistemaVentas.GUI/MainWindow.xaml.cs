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

            // 1. CARGAR POR DEFECTO EL PUNTO DE VENTA AL ABRIR EL MAINWINDOW
            OcultarSubmenus();
            ContenedorPrincipal.Content = new Ventas();
        }

        private void ConfigurarMenuPorRol()
        {
            if (SesionGlobal.UsuarioActual == null) return;

            int idRolActual = SesionGlobal.UsuarioActual.IdRol;

            // Ocultamos submenús al arrancar
            OcultarSubmenus();

            // 2. CONFIGURACIÓN DE ACCESOS (Visible/Habilitado vs En Gris Abajo)
            // 1: GERENTE | 2: VENDEDOR | 3: SUPERVISOR
            switch (idRolActual)
            {
                case 1: // GERENTE (Tiene acceso a todo)
                    EstablecerAccesoBoton(btnUsuarios, true);
                    EstablecerAccesoBoton(btnCompra, true);
                    EstablecerAccesoBoton(btnProductos, true);
                    EstablecerAccesoBoton(btnReportes, true);
                    break;

                case 2: // VENDEDOR (Sin acceso a Usuarios, Compras ni Productos -> Se muestran abajo en gris)
                    EstablecerAccesoBoton(btnUsuarios, false);
                    EstablecerAccesoBoton(btnCompra, false);
                    EstablecerAccesoBoton(btnProductos, false);
                    EstablecerAccesoBoton(btnReportes, true);
                    break;

                case 3: // SUPERVISOR (Sin acceso a Usuarios -> Se muestra abajo en gris)
                    EstablecerAccesoBoton(btnUsuarios, false);
                    EstablecerAccesoBoton(btnCompra, true);
                    EstablecerAccesoBoton(btnProductos, true);
                    EstablecerAccesoBoton(btnReportes, true);
                    break;
            }

            // Configurar acceso al módulo de Backups (solo Gerente, el resto en gris abajo)
            if (idRolActual != 1)
            {
                EstablecerAccesoBoton(btnBackup, false);
            }
            else
            {
                EstablecerAccesoBoton(btnBackup, true);
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

        /// <summary>
        /// Manda los botones sin permisos al fondo visualmente (si están en un StackPanel) y los pone en gris (Opacity = 0.4 e IsEnabled = false)
        /// </summary>
        private void EstablecerAccesoBoton(Button boton, bool tieneAcceso)
        {
            if (boton == null) return;

            if (tieneAcceso)
            {
                boton.Visibility = Visibility.Visible;
                boton.IsEnabled = true;
                boton.Opacity = 1.0;
            }
            else
            {
                // En vez de ocultarlo (Collapsed), lo dejamos visible pero inactivo y translúcido
                boton.Visibility = Visibility.Visible;
                boton.IsEnabled = false;
                boton.Opacity = 0.4; // Apariencia en gris / deshabilitado

                // Si están dentro de un StackPanel, esto los desplaza dinámicamente hacia el final del contenedor
                if (boton.Parent is Panel panelPadre)
                {
                    panelPadre.Children.Remove(boton);
                    panelPadre.Children.Add(boton); // Lo manda al último lugar (abajo de todo)
                }
            }
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
            if (SesionGlobal.UsuarioActual == null || SesionGlobal.UsuarioActual.IdRol is not (1 or 2))
            {
                MessageBox.Show("No tienes permisos para acceder al módulo de compras.", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OcultarSubmenus();
            ContenedorPrincipal.Content = new Compra();
        }

        private void btnCaja_Click(object sender, RoutedEventArgs e)
        {
            OcultarSubmenus();
            // ContenedorPrincipal.Content = new CajaView(); 
        }

        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            OcultarSubmenus();

            if (SesionGlobal.UsuarioActual == null || SesionGlobal.UsuarioActual.IdRol != 1)
            {
                MessageBox.Show("No tienes permisos para realizar copias de seguridad.", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ContenedorPrincipal.Content = new SistemaVentas.GUI.BackUpControl();
        }

        private void btnManual_Click(object sender, RoutedEventArgs e)
        {
            OcultarSubmenus();
            ContenedorPrincipal.Content = new ManualUsuarioView();
        }

        private void btnSubArqueoDiario_Click(object sender, RoutedEventArgs e)
        {
            if (SesionGlobal.UsuarioActual == null) return;
            OcultarSubmenus();
            ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Vendedor.ArqueoDiarioView(SesionGlobal.UsuarioActual.IdUsuario);
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            SesionGlobal.UsuarioActual = null;
            Login login = new Login();
            login.Show();
            this.Close();
        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {
            if (SesionGlobal.UsuarioActual == null) return;

            int idRol = SesionGlobal.UsuarioActual.IdRol;
            int idUsuario = SesionGlobal.UsuarioActual.IdUsuario;

            OcultarSubmenus();

            if (idRol == 1) // GERENTE
            {
                btnSubRentabilidad.Visibility = Visibility.Visible;
                btnSubAuditoria.Visibility = Visibility.Visible;
                btnSubRendimiento.Visibility = Visibility.Visible;

                ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Gerente.RentabilidadView();
            }
            else if (idRol == 2) // VENDEDOR
            {
                ContenedorPrincipal.Content = new SistemaVentas.GUI.Views.Reportes.Vendedor.ArqueoDiarioView(idUsuario);
            }
            else if (idRol == 3) // SUPERVISOR
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