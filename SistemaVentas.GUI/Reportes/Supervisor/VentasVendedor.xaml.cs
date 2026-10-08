using System;
using System.Collections.Generic;
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
using SistemaVentas.BLL.Reportes;

namespace SistemaVentas.GUI.Views.Reportes.Supervisor
{
    public partial class VentasVendedorView : UserControl
    {
        private readonly SupervisorService _service = new SupervisorService();

        public VentasVendedorView()
        {
            InitializeComponent();
            this.Loaded += VentasVendedorView_Loaded;
        }

        private void VentasVendedorView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgVentasVendedor != null)
                {
                    DgVentasVendedor.ItemsSource = _service.ObtenerVentasPorVendedor();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar Ventas por Vendedor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}