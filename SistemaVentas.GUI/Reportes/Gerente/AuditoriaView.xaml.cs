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

namespace SistemaVentas.GUI.Views.Reportes.Gerente
{
    public partial class AuditoriaView : UserControl
    {
        private readonly GerenteService _service = new GerenteService();

        public AuditoriaView()
        {
            InitializeComponent();
            this.Loaded += AuditoriaView_Loaded;
        }

        private void AuditoriaView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgAuditoria != null)
                {
                    DgAuditoria.ItemsSource = _service.ObtenerAuditoria();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el reporte de Auditoría: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}