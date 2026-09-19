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
    public partial class RentabilidadView : UserControl
    {
        private readonly GerenteService _service = new GerenteService();

        public RentabilidadView()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void CargarDatos()
        {
            var rentabilidad = _service.ObtenerRentabilidad();
            TxtTotalVentas.Text = rentabilidad.TotalVentas.ToString("C");
            TxtTotalCompras.Text = rentabilidad.TotalCompras.ToString("C");
            TxtGananciaNeta.Text = rentabilidad.GananciaNeta.ToString("C");
        }
    }
}
