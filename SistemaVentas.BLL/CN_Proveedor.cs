using System;
using System.Collections.Generic;
using System.Diagnostics;
using SistemaVentas.DAL;
using SistemaVentas.Entities;

namespace SistemaVentas.BLL
{
    /// <summary>
    /// Capa de negocio del Proveedor.
    /// Es el único lugar donde se validan las reglas del dominio y se redactan
    /// los mensajes que se muestran al usuario.
    /// </summary>
    public class CN_Proveedor
    {
        // Se reutiliza una única instancia a nivel de clase
        private readonly CD_Proveedor objcd_proveedor = new CD_Proveedor();

        public List<Proveedor> Listar()
        {
            return objcd_proveedor.Listar();
        }

        public Proveedor? ObtenerPorId(int idProveedor)
        {
            return objcd_proveedor.ObtenerPorId(idProveedor);
        }

        public bool Registrar(Proveedor obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            // 1. Validación de campos obligatorios
            if (obj == null)
            {
                Mensaje = "El proveedor a registrar no puede ser nulo.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(obj.RazonSocial))
            {
                Mensaje = "La razón social del proveedor no puede estar vacía.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(obj.Cuit))
            {
                Mensaje = "El CUIT del proveedor no puede estar vacío.";
                return false;
            }

            // 2. La capa de datos solo ejecuta el SQL y devuelve el resultado
            try
            {
                if (!objcd_proveedor.Registrar(Normalizar(obj)))
                {
                    Mensaje = "No se pudo guardar el proveedor.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Detalle técnico solo para el log del desarrollador
                Debug.WriteLine($"Error inesperado al registrar proveedor: {ex.Message}");
                Mensaje = "No se pudo guardar el proveedor debido a un inconveniente con el servidor o los datos ingresados.";
                return false;
            }

            Mensaje = "Proveedor registrado correctamente.";
            return true;
        }

        public bool Editar(Proveedor obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            // 1. Validación de campos obligatorios
            if (obj == null)
            {
                Mensaje = "El proveedor a editar no puede ser nulo.";
                return false;
            }

            if (obj.IdProveedor <= 0)
            {
                Mensaje = "El proveedor a editar no es válido.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(obj.RazonSocial))
            {
                Mensaje = "La razón social del proveedor no puede estar vacía.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(obj.Cuit))
            {
                Mensaje = "El CUIT del proveedor no puede estar vacío.";
                return false;
            }

            // 2. La capa de datos solo ejecuta el SQL y devuelve el resultado
            try
            {
                if (!objcd_proveedor.Editar(Normalizar(obj)))
                {
                    Mensaje = "No se pudo actualizar la información del proveedor.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Detalle técnico solo para el log del desarrollador
                Debug.WriteLine($"Error inesperado al editar proveedor: {ex.Message}");
                Mensaje = "No se pudo actualizar la información del proveedor debido a un inconveniente con el servidor o los datos ingresados.";
                return false;
            }

            Mensaje = "Proveedor actualizado correctamente.";
            return true;
        }

        /// <summary>
        /// Normaliza los textos del proveedor (recorta espacios) antes de enviarlos a la capa de datos.
        /// </summary>
        private static Proveedor Normalizar(Proveedor obj)
        {
            return new Proveedor()
            {
                IdProveedor = obj.IdProveedor,
                RazonSocial = obj.RazonSocial.Trim(),
                Cuit = obj.Cuit.Trim(),
                Telefono = obj.Telefono?.Trim() ?? string.Empty,
                NombreContacto = obj.NombreContacto?.Trim() ?? string.Empty,
                Activo = obj.Activo
            };
        }
    }
}
