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
    public partial class VentasCanceladasView : UserControl
    {
        private readonly SupervisorService _service = new SupervisorService();

        public VentasCanceladasView()
        {
            InitializeComponent();
            this.Loaded += VentasCanceladasView_Loaded;
        }

        private void VentasCanceladasView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgVentasCanceladas != null)
                {
                    DgVentasCanceladas.ItemsSource = _service.ObtenerVentasCanceladas();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar Ventas Canceladas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}