using System;
using System.Collections.Generic;
using System.Text;
using SistemaVentas.DAL;
using SistemaVentas.Entities;

namespace SistemaVentas.BLL
{
    public class CN_Caja
    {
        private CD_Caja objcd_caja = new CD_Caja();

        public List<Caja> Listar()
        {
            return objcd_caja.Listar();
        }

        public bool Registrar(Caja obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrWhiteSpace(obj.Descripcion))
            {
                Mensaje = "Debe ingresar el número de caja y la dirección en la descripción.";
                return false;
            }

            return objcd_caja.Registrar(obj, out Mensaje);
        }
    }
}
