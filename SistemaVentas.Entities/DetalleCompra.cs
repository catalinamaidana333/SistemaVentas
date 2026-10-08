namespace SistemaVentas.Entities
{
    public class DetalleCompra
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
