using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities.Reportes
{
    public class RentabilidadResumen
    {
        public decimal TotalVentas { get; set; }
        public decimal TotalCompras { get; set; }
        public decimal GananciaNeta => TotalVentas - TotalCompras;
    }
}
