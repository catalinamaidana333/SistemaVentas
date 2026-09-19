using System;
using System.Collections.Generic;
using System.Text;
using SistemaVentas.DAL.Reportes;
using SistemaVentas.Entities.Reportes;

namespace SistemaVentas.BLL.Reportes
{
    public class ArqueoService
    {
        private readonly ArqueoRepository _repository = new ArqueoRepository();

        public ArqueoResumen ObtenerResumen(int idCajaUsuario)
        {
            return _repository.ObtenerResumenPorCajaUsuario(idCajaUsuario);
        }
    }
}
