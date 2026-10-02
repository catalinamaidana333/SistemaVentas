namespace SistemaVentas.Entities
{
    public class DetalleVenta
    {
        public int IdProducto { get; set; }
        //falta id_detalleventa? o no hace falta? y id_venta
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
