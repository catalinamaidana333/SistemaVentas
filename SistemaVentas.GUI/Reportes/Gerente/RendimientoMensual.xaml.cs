using SistemaVentas.BLL.Reportes;
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

namespace SistemaVentas.GUI.Views.Reportes.Gerente
{
    public partial class RendimientoMensualView : UserControl
    {
        private readonly GerenteService _service = new GerenteService();

        public RendimientoMensualView()
        {
            InitializeComponent();
            DgRendimiento.ItemsSource = _service.ObtenerRendimientoMensual();
        }
    }
}
