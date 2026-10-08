namespace SistemaVentas.Entities
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public int IdCajaUsuario { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal MontoAbonado { get; set; }
        public decimal Vuelto { get; set; }
        public decimal Total { get; set; }
        public List<DetalleVenta> Detalles { get; set; } = new();
    }
}
