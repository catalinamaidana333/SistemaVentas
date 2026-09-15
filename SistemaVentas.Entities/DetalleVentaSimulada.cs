using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities
{
    public class DetalleVentaSimulada
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        // El subtotal se calcula solo en base a la cantidad y el precio
        public decimal Subtotal
        {
            get { return Cantidad * PrecioUnitario; }
        }
    }
}
