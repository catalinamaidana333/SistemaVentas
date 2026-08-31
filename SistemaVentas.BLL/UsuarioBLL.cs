using SistemaVentas.DAL;
using SistemaVentas.Entities;
using System;
using System.Configuration;
using System.Text.RegularExpressions;
using BC = BCrypt.Net.BCrypt;

namespace SistemaVentas.BLL
{
    /// <summary>
    /// Clase de lógica de negocio para la gestión de usuarios.
    /// Implementa validaciones, seguridad y reglas de negocio según arquitectura de capas.
    /// </summary>
    public class UsuarioBLL
    {
        private UsuarioDAL usuarioDAL;

        public UsuarioBLL()
        {
            // 1. Aquí SÍ leemos el App.config (estamos en una capa superior)
            string conexion = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

            // 2. Le inyectamos la conexión al DAL
            usuarioDAL = new UsuarioDAL(conexion);
        }

        // Constante del IdRol hardcodeada en desarrollo (debe configurarse según la BD)
        private const int ID_ROL_GERENTE = 1;

     

        /// <summary>
        /// Crea un nuevo usuario con validaciones completas de seguridad y reglas de negocio.
        /// 
        /// Validaciones implementadas:
        /// 1. Validación de formato (Nombre y Correo)
        /// 2. Hasheo de contraseña con BCrypt
        /// 3. Autorización (Solo el Gerente puede crear usuarios)
        /// 4. Verificación de existencia de correo duplicado
        /// </summary>
        /// <param name="nuevoUsuario">El usuario a crear con datos sin encriptar</param>
        /// <param name="usuarioAutenticado">El usuario que intenta realizar la creación (debe ser Gerente para autorización)</param>
        /// <returns>El ID del usuario creado</returns>
        /// <exception cref="ValidacionException">Si los datos no cumplen las validaciones de formato</exception>
        /// <exception cref="AutorizacionException">Si solo el Gerente puede crear usuarios</exception>
        /// <exception cref="UsuarioException">Para otros errores de usuario</exception>
        public int CrearUsuario(Usuario nuevoUsuario, Usuario usuarioAutenticado)
        {
            try
            {
                // Paso 1: Validar que los parámetros no sean nulos
                if (nuevoUsuario == null)
                    throw new ValidacionException("El usuario no puede ser nulo.");

                if (usuarioAutenticado == null)
                    throw new AutorizacionException("Debe estar autenticado para crear usuarios.");

                // Paso 2: Validar autorización - Solo el Gerente puede crear usuarios
                if (usuarioAutenticado.IdRol != ID_ROL_GERENTE)
                {
                    throw new AutorizacionException(
                        $"No tiene autorización para crear usuarios. Solo los Gerentes (IdRol: {ID_ROL_GERENTE}) pueden crear usuarios. Su IdRol es: {usuarioAutenticado.IdRol}");
                }

                // Paso 3: Validar formato del Nombre
                ValidarNombre(nuevoUsuario.Nombre);

                // Paso 4: Validar formato del Correo
                ValidarCorreo(nuevoUsuario.Correo);

                // Paso 5: Validar formato y seguridad de la Contraseña
                ValidarPassword(nuevoUsuario.Password);

                // Paso 6: Verificar que el correo no esté duplicado
                if (usuarioDAL.ExisteUsuarioPorCorreo(nuevoUsuario.Correo))
                {
                    throw new UsuarioException(
                        $"El correo '{nuevoUsuario.Correo}' ya está registrado en el sistema.");
                }

                // Paso 7: Hashear la contraseña (transformación de seguridad)
                string passwordHasheada = HashearPassword(nuevoUsuario.Password);
                nuevoUsuario.Password = passwordHasheada;

                // Paso 8: Guardar el usuario en la base de datos
                int idUsuarioCreado = usuarioDAL.GuardarUsuario(nuevoUsuario);

                return idUsuarioCreado;
            }
            catch (ValidacionException)
            {
                throw;
            }
            catch (AutorizacionException)
            {
                throw;
            }
            catch (UsuarioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UsuarioException(
                    $"Error inesperado al crear usuario: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Valida que el nombre tenga formato correcto.
        /// - No debe ser nulo o vacío
        /// - Debe contener solo letras, espacios y caracteres acentuados
        /// - Debe tener entre 3 y 100 caracteres
        /// </summary>
        private void ValidarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ValidacionException("El nombre no puede estar vacío.");

            nombre = nombre.Trim();

            if (nombre.Length < 3 || nombre.Length > 100)
                throw new ValidacionException("El nombre debe tener entre 3 y 100 caracteres.");

            // Regex para validar que contenga solo letras, espacios y caracteres acentuados
            // Permite: a-z, A-Z, acentos (á, é, í, ó, ú, ñ, etc.), espacios
            string patronNombre = @"^[a-zA-ZáéíóúñÁÉÍÓÚÑ\s]+$";
            if (!Regex.IsMatch(nombre, patronNombre))
                throw new ValidacionException(
                    "El nombre solo puede contener letras, espacios y caracteres acentuados.");
        }

        /// <summary>
        /// Valida que el correo tenga un formato válido.
        /// Utiliza una expresión regular para verificar estructura de email estándar.
        /// </summary>
        private void ValidarCorreo(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                throw new ValidacionException("El correo no puede estar vacío.");

            correo = correo.Trim();

            // Regex para validar formato básico de email
            // Cumple con RFC 5322 simplificado
            string patronCorreo = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(correo, patronCorreo))
                throw new ValidacionException(
                    $"El correo '{correo}' no tiene un formato válido. Ejemplo válido: usuario@ejemplo.com");

            // Validación adicional de longitud
            if (correo.Length > 254)
                throw new ValidacionException(
                    "El correo no puede exceder 254 caracteres.");
        }

        /// <summary>
        /// Valida que la contraseña cumpla con requisitos mínimos de seguridad.
        /// Requisitos:
        /// - Mínimo 8 caracteres
        /// </summary>
        private void ValidarPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ValidacionException("La contraseña no puede estar vacía.");

            if (password.Length < 8)
                throw new ValidacionException(
                    "La contraseña debe tener mínimo 8 caracteres.");

            if (password.Length > 128)
                throw new ValidacionException(
                    "La contraseña no puede exceder 128 caracteres.");
        }

        /// <summary>
        /// Hashea la contraseña usando BCrypt con un costo computacional de 12 iteraciones.
        /// BCrypt es más seguro que SHA-256 para almacenamiento de contraseñas ya que
        /// incluye salt automático y es resistente a ataques de fuerza bruta.
        /// </summary>
        private string HashearPassword(string passwordPlano)
        {
            try
            {
                // BCrypt con trabajo factor de 12 (mayor seguridad, más lento)
                // El salt se genera automáticamente e incluye en el hash resultante
                string passwordHasheada = BC.HashPassword(passwordPlano, workFactor: 12);
                return passwordHasheada;
            }
            catch (Exception ex)
            {
                throw new UsuarioException(
                    $"Error al hashear la contraseña: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica que una contraseña sin encriptar coincida con su hash almacenado.
        /// Útil para validar contraseñas durante el login.
        /// </summary>
        public bool VerificarPassword(string passwordPlano, string passwordHasheada)
        {
            try
            {
                return BC.Verify(passwordPlano, passwordHasheada);
            }
            catch (Exception ex)
            {
                throw new UsuarioException(
                    $"Error al verificar la contraseña: {ex.Message}", ex);
            }
        }
    }
}
