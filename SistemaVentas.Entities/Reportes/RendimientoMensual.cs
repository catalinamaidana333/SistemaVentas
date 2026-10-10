using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities.Reportes
{
    public class RendimientoMensual
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int TotalVentas { get; set; }
        public decimal TotalMonto { get; set; }
        public decimal TicketPromedio { get; set; }
    }
}
