using System.Collections.Generic;
using SistemaVentas.DAL;
using SistemaVentas.Entities; // O SistemaVentas.Entidades según tu proyecto

namespace SistemaVentas.BLL
{
    public class CN_Producto
    {
        // Se reutiliza una única instancia a nivel de clase
        private CD_Producto objcd_producto = new CD_Producto();

        public List<Producto> Listar()
        {
            return objcd_producto.Listar();
        }

        public List<Categoria> ListarCategorias()
        {
            return objcd_producto.ListarCategorias();
        }

        public bool Registrar(Producto obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            // 1. Validación de campos vacíos
            if (string.IsNullOrWhiteSpace(obj.Nombre))
            {
                Mensaje = "El nombre del producto no puede estar vacío.";
                return false;
            }

            // 2. El precio de venta debe ser mayor al precio de costo
            if (obj.PrecioVenta <= obj.PrecioCosto)
            {
                Mensaje = "El precio de venta debe ser mayor al precio de costo.";
                return false;
            }

            // 3. Reutilización de la instancia privada
            return objcd_producto.Registrar(obj, out Mensaje);
        }

        public bool Editar(Producto obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrWhiteSpace(obj.Nombre))
            {
                Mensaje = "El nombre del producto no puede estar vacío.";
                return false;
            }

            // Misma regla de negocio aplicada para la edición
            if (obj.PrecioVenta <= obj.PrecioCosto)
            {
                Mensaje = "El precio de venta debe ser mayor al precio de costo.";
                return false;
            }

            return objcd_producto.Editar(obj, out Mensaje);
        }

        public bool Eliminar(Producto obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            return objcd_producto.Eliminar(obj, out Mensaje);
        }
    }
}