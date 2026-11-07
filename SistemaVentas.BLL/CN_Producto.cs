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

        public string GenerarCodigoBarrasUnico(int longitud = 13)
        {
            if (longitud <= 0)
                throw new ArgumentOutOfRangeException(nameof(longitud),
                    "La longitud del código debe ser mayor a cero.");

            const int maxIntentos = 1000;

            for (int intento = 0; intento < maxIntentos; intento++)
            {
                string codigo = GenerarCadenaNumerica(longitud);

                if (!objcd_producto.ExisteCodigoBarras(codigo))
                    return codigo;
            }

            throw new InvalidOperationException(
                "No se pudo generar un código de barras único después de varios intentos.");
        }

        private string GenerarCadenaNumerica(int longitud)
        {
            char[] digitos = new char[longitud];

            for (int i = 0; i < longitud; i++)
            {
                int min = (i == 0 && longitud > 1) ? 1 : 0;
                digitos[i] = (char)('0' + Random.Shared.Next(min, 10));
            }

            return new string(digitos);
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

        public bool ActualizarPrecioVenta(int idProducto, decimal nuevoPrecioVenta, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (idProducto <= 0)
            {
                Mensaje = "El producto indicado no es válido.";
                return false;
            }

            if (nuevoPrecioVenta <= 0)
            {
                Mensaje = "El nuevo precio de venta debe ser mayor a 0.";
                return false;
            }

            return objcd_producto.ActualizarPrecioVenta(idProducto, nuevoPrecioVenta, out Mensaje);
        }

        public bool Eliminar(Producto obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            return objcd_producto.Eliminar(obj, out Mensaje);
        }
    }
}