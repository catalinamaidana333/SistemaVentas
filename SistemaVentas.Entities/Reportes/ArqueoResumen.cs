using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities.Reportes
{
    public class ArqueoResumen
    {
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal TotalVentasEfectivo { get; set; }
        public decimal TotalVentasMp { get; set; }
        public decimal TotalComprasEfectivo { get; set; }
        public decimal MontoSistema { get; set; }
        public decimal? MontoDeclarado { get; set; }
        public decimal? Diferencia { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
