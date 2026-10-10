using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities.Reportes
{
    public class RentabilidadResumen
    {
        public decimal TotalVentas { get; set; }
        public decimal TotalCostos { get; set; }

        // Propiedad calculada automáticamente: Ventas menos Costos de lo vendido
        public decimal GananciaNeta => TotalVentas - TotalCostos;
    }
}
