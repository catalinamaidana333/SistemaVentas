using SistemaVentas.DAL.Reportes;
using SistemaVentas.Entities.Reportes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.BLL.Reportes
{
    public class SupervisorService
    {
        private readonly SupervisorRepository _repo = new SupervisorRepository();

        public List<StockCritico> ObtenerStockCritico() => _repo.ObtenerStockCritico();
        public List<VentaCancelada> ObtenerVentasCanceladas() => _repo.ObtenerVentasCanceladas();
        public List<VentaVendedor> ObtenerVentasPorVendedor() => _repo.ObtenerVentasPorVendedor();
    }
}
