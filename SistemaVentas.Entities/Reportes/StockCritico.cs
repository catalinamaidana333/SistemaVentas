using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities.Reportes
{
    public class StockCritico
    {
        public string Nombre { get; set; } = string.Empty;
        public string CodigoBarras { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
    }
}
