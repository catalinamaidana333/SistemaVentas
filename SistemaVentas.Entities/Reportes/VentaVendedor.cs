using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities.Reportes
{
    public class VentaVendedor
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string NombreRol { get; set; } = string.Empty;
        public int TotalVentas { get; set; }
        public decimal TotalMonto { get; set; }
        public DateTime? UltimaVenta { get; set; }

        // Propiedades de compatibilidad (por si algún control viejo las utiliza)
        public string NombreVendedor
        {
            get => NombreCompleto;
            set => NombreCompleto = value;
        }
        public int CantidadVentas
        {
            get => TotalVentas;
            set => TotalVentas = value;
        }
        public decimal TotalRecaudado
        {
            get => TotalMonto;
            set => TotalMonto = value;
        }
    }
}