using System.Collections.Generic;
using SistemaVentas.DAL;
using SistemaVentas.Entities;

namespace SistemaVentas.BLL
{
	public class CN_Producto
	{
		private CD_Producto objcd_producto = new CD_Producto();

		public List<Producto> Listar()
		{
			return objcd_producto.Listar();
		}

        // Regla de negocio para asegurar ganancias
        public bool Registrar(Producto obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            // Validación solicitada en la consigna
            if (obj.PrecioVenta <= obj.PrecioCosto)
            {
                Mensaje = "El precio de venta debe ser mayor al precio de costo.";
                return false;
            }

            // Si pasa la validación, llama a la capa DAL
            return new CD_Producto().Registrar(obj, out Mensaje);
        }
    }
}