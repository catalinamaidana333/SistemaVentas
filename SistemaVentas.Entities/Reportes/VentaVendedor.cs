using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities.Reportes
{
    public class VentaVendedor
    {
        public string NombreVendedor { get; set; } = string.Empty;
        public int CantidadVentas { get; set; }
        public decimal TotalRecaudado { get; set; }
    }
}
