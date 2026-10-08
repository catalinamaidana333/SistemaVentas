namespace SistemaVentas.Entities
{
    public class Compra
    {
        public int IdCompra { get; set; }
        public int IdUsuario { get; set; }
        public int IdCajaUsuario { get; set; }
        public int IdProveedor { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public List<DetalleCompra> Detalles { get; set; } = new();
    }
}
