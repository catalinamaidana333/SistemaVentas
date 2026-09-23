using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities
{
    public class Caja
    {
        public int IdCaja { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public bool Activa { get; set; } // Asegúrate de que se llame 'Activa' con A mayúscula
    }
}
