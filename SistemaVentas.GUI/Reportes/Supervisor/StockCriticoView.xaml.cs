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
    public partial class StockCriticoView : UserControl
    {
        private readonly SupervisorService _service = new SupervisorService();

        public StockCriticoView()
        {
            InitializeComponent();
            DgStockCritico.ItemsSource = _service.ObtenerStockCritico();
        }
    }
}
