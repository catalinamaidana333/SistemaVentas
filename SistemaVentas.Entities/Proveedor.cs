namespace SistemaVentas.Entities
{
    public class Proveedor
    {
        public int IdProveedor { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string Cuit { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string NombreContacto { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
