using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.GUI
{
    internal class ValidadorGUI
    {
        // En tu ValidadorUI.cs (Capa de Presentación)
        public static bool EsNombreValido(string nombre, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                mensajeError = "El nombre no puede estar vacío.";
                return false;
            }

            nombre = nombre.Trim();

            if (nombre.Length < 3 || nombre.Length > 100)
            {
                mensajeError = "El nombre debe tener entre 3 y 100 caracteres.";
                return false;
            }

            string patronNombre = @"^[a-zA-ZáéíóúñÁÉÍÓÚÑ\s]+$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(nombre, patronNombre))
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
    }
}
