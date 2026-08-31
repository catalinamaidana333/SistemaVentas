using System;

namespace SistemaVentas.Entities
{
    /// <summary>
    /// Excepción personalizada para errores relacionados con validaciones de usuario
    /// </summary>
    public class ValidacionException : Exception
    {
        public ValidacionException(string mensaje) : base(mensaje)
        {
        }

        public ValidacionException(string mensaje, Exception innerException) 
            : base(mensaje, innerException)
        {
        }
    }

    /// <summary>
    /// Excepción personalizada para errores de autorización
    /// </summary>
    public class AutorizacionException : Exception
    {
        public AutorizacionException(string mensaje) : base(mensaje)
        {
        }

        public AutorizacionException(string mensaje, Exception innerException) 
            : base(mensaje, innerException)
        {
        }
    }

    /// <summary>
    /// Excepción personalizada para errores generales de usuario
    /// </summary>
    public class UsuarioException : Exception
    {
        public UsuarioException(string mensaje) : base(mensaje)
        {
        }

        public UsuarioException(string mensaje, Exception innerException) 
            : base(mensaje, innerException)
        {
        }
    }
}
