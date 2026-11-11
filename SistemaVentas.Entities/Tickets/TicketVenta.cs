namespace SistemaVentas.Entities.Tickets
{
    public class TicketVenta
    {
        public int IdVenta { get; set; }
        public DateTime FechaHora { get; set; }
        public string NombreMinimercado { get; set; } = string.Empty;
        public string NombreCajero { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal MontoAbonado { get; set; }
        public decimal Vuelto { get; set; }
        public List<DetalleTicket> Detalles { get; set; } = new();
    }
}
