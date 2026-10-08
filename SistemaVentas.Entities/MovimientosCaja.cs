using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities
{
    public class MovimientosCaja
    {
        public int IdMovimiento { get; set; }
        public int IdCaja { get; set; } // Nueva propiedad obligatoria
        public int? IdCajaUsuario { get; set; } // Ahora es Nullable
        public int IdUsuario { get; set; } // Quien hace el movimiento (Ej: el gerente)
        public string Tipo { get; set; } // "INGRESO" o "EGRESO"
        public decimal Monto { get; set; }
        public string Motivo { get; set; }
        public DateTime FechaHora { get; set; }
    }
}
