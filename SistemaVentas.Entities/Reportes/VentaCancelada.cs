using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities.Reportes
{
    public class VentaCancelada
    {
        public int IdVenta { get; set; }
        public DateTime FechaHora { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}
