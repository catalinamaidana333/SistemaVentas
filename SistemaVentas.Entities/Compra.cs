using System;

namespace SistemaVentas.Entities
{
    public class Compra
    {
        public int IdCompra { get; set; }
        public int IdUsuario { get; set; }
        public int? IdCajaUsuario { get; set; }
        public int IdProveedor { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Total { get; set; }

        public string NumeroFactura { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;

        // Origen del efectivo: "Caja" | "Externa" | null (Mercado Pago)
        public string? OrigenEfectivo { get; set; }

        public DateTime FechaHora { get; set; }

        public List<DetalleCompra> Detalles { get; set; } = new();

        // Campos de solo lectura para el historial (se completan con los JOINs del DAL)
        public string RazonSocialProveedor { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
    }
}
