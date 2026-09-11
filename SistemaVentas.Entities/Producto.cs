namespace SistemaVentas.Entities
{
	public class Producto
	{
		public int IdProducto { get; set; }
		public int IdCategoria { get; set; }
		public string CodigoBarras { get; set; }
		public string Nombre { get; set; }
		public decimal PrecioCosto { get; set; }
		public decimal PrecioVenta { get; set; }
		public int StockActual { get; set; }
		public int StockMinimo { get; set; }
		public bool Activo { get; set; }

		public Categoria oCategoria { get; set; }
	}
}