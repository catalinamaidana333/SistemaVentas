namespace SistemaVentas.Entities
{
    public class DetalleCompra
    {
        public int IdDetalleCompra { get; set; }
        public int IdCompra { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }

        public string NombreProducto { get; set; } = string.Empty;
        public string CodigoBarras { get; set; } = string.Empty;

        // Propiedad calculada para que WPF la muestre en el DataGrid
        public decimal Subtotal => Cantidad * CostoUnitario;
    }
}
