namespace SistemaVentas.Entities
{
    public class AlertaMargen
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public decimal CostoAnterior { get; set; }
        public decimal CostoNuevo { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal MargenAnterior { get; set; }
        public decimal MargenNuevo { get; set; }
        public decimal PrecioVentaSugerido { get; set; }
    }
}
