using System;
using System.Collections.Generic;
using System.Text;
using SistemaVentas.DAL.Reportes;
using SistemaVentas.Entities.Reportes;

namespace SistemaVentas.BLL.Reportes
{
    public class GerenteService
    {
        private readonly GerenteRepository _repo = new GerenteRepository();

        public RentabilidadResumen ObtenerRentabilidad() => _repo.ObtenerRentabilidad();
        public List<UsuarioAuditoria> ObtenerAuditoria() => _repo.ObtenerAuditoriaUsuarios();
        public List<RendimientoMensual> ObtenerRendimientoMensual() => _repo.ObtenerRendimientoMensual();
    }
}
