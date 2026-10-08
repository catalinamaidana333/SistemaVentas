namespace SistemaVentas.Entities
{
    public class DetalleVenta
    {
        public int IdDetalleVenta { get; set; }
        public int IdVenta { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        // Propiedad calculada para que WPF la muestre en el DataGrid
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
