using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities
{
    public class CajaUsuario
    {
        public int IdCajaUsuario { get; set; }
        public int IdCaja { get; set; }
        public int IdUsuario { get; set; }
        public bool Estado { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime FechaCierre { get; set; }
        //conviene ponerlo como int y despues calcularlo en la logica de negocio?
        public decimal MontoApertura { get; set; }
        public decimal MontoCierre { get; set; }
        public decimal MontoTotalVentasEfectivo { get; set; }
        public decimal MontoTotalVentasMp { get; set; }
        public decimal MontoTotalGastosEfec { get; set; }

        public decimal MontoSistema { get; set; }
        public decimal Diferencia { get; set; }
    }
}
