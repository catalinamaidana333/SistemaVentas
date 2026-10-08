using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.GUI
{
    internal class ValidadorGUI
    {
        // En tu ValidadorUI.cs (Capa de Presentación)
        public static bool EsNombreValido(string nombree, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(nombree))
            {
                mensajeError = "El nombre no puede estar vacío.";
                return false;
            }

            nombree = nombree.Trim();

            if (nombree.Length < 3 || nombree.Length > 100)
            {
                mensajeError = "El nombre debe tener entre 3 y 100 caracteres.";
                return false;
            }

            string patronNombre = @"^[a-zA-ZáéíóúñÁÉÍÓÚÑ\s]+$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(nombree, patronNombre))
            {
                mensajeError = "Solo puede contener letras y espacios.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        // Validador de Correo
        public static bool EsCorreoValido(string correo, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                mensajeError = "El correo no puede estar vacío.";
                return false;
            }

            correo = correo.Trim();

            if (correo.Length > 254)
            {
                mensajeError = "El correo no puede exceder 254 caracteres.";
                return false;
            }

            string patronCorreo = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(correo, patronCorreo))
            {
                mensajeError = "Formato inválido. Ejemplo: usuario@ejemplo.com";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        // Validador de Contraseña
        public static bool EsPasswordValido(string password, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                mensajeError = "La contraseña no puede estar vacía.";
                return false;
            }

            if (password.Length < 8)
            {
                mensajeError = "La contraseña debe tener mínimo 8 caracteres.";
                return false;
            }

            if (password.Length > 128)
            {
                mensajeError = "La contraseña no puede exceder 128 caracteres.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        // Validador de DNI
        public static bool EsDNIValido(string dni, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(dni))
            {
                mensajeError = "El DNI no puede estar vacío.";
                return false;
            }

            dni = dni.Trim();

            if (dni.Length > 8)
            {
                mensajeError = "El DNI no puede exceder 8 caracteres.";
                return false;
            }

            string patronDNI = @"^[0-9]+$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(dni, patronDNI))
            {
                mensajeError = "El DNI solo puede contener números.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        // Validador de Fecha de Nacimiento
        public static bool EsFechaNacimientoValida(DateTime? fecha, out string mensajeError)
        {
            if (!fecha.HasValue)
            {
                mensajeError = "La fecha de nacimiento no puede estar vacía.";
                return false;
            }

            if (fecha.Value > DateTime.Now)
            {
                mensajeError = "La fecha de nacimiento no puede ser futura.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        // Validador de Nombre de Usuario
        public static bool EsNombreUsuarioValido(string nombreUsuario, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                mensajeError = "El nombre de usuario no puede estar vacío.";
                return false;
            }

            nombreUsuario = nombreUsuario.Trim();

            if (nombreUsuario.Length < 3 || nombreUsuario.Length > 50)
            {
                mensajeError = "El nombre de usuario debe tener entre 3 y 50 caracteres.";
                return false;
            }

            string patronNombreUsuario = @"^[a-z0-9]+$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(nombreUsuario, patronNombreUsuario))
            {
                mensajeError = "El nombre de usuario solo puede contener minúsculas y números.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }
    }
}
