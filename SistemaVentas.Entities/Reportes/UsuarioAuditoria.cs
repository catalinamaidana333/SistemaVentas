using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities.Reportes
{
    public class UsuarioAuditoria
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string NombreRol { get; set; } = string.Empty;
        public int TotalVentas { get; set; }
        public decimal TotalMontoVendido { get; set; }
        public DateTime? FechaUltimaVenta { get; set; }
    }
}
