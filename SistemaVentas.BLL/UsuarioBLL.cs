using SistemaVentas.DAL;
using SistemaVentas.Entities;
using System;
using System.Configuration;
using System.Text.RegularExpressions;
using static SistemaVentas.Entities.Rol;
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
            
            // 2. Le inyectamos la conexión al DAL
            usuarioDAL = new UsuarioDAL();
        }

       


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
            // 1. Verificación de autorización: solo Gerente puede crear usuarios
            if (usuarioAutenticado == null || usuarioAutenticado.IdRol != (int)Roles.Gerente)
            {
                throw new AutorizacionException("No tiene permisos para crear usuarios. Solo los usuarios con rol Gerente pueden realizar esta acción.");
            }

            if (nuevoUsuario == null)
            {
                throw new ValidacionException("Los datos del nuevo usuario no pueden ser nulos.");
            }

            // 2. Verificamos que el correo no exista ya en la base de datos
            if (usuarioDAL.ExisteUsuarioPorCorreo(nuevoUsuario.Correo))
            {
                throw new UsuarioException("Ya existe un usuario registrado con este correo electrónico.");
            }

            // 3. Hashear la contraseña (transformación de seguridad)
            string passwordHasheada = HashearPassword(nuevoUsuario.Password);
            nuevoUsuario.Password = passwordHasheada;

            // 4. Mandamos a guardar a la Capa de Datos (DAL)
            return usuarioDAL.GuardarUsuario(nuevoUsuario);
        }
        public bool ValidarDatosNuevoUsuario(Usuario usuario, out string mensajeError)
        {
            mensajeError = string.Empty;

            try
            {
                // Llamamos a todos tus métodos de validación ya creados
                ValidarNombreUsuario(usuario.NombreUsuario);
                ValidarNombre(usuario.Nombree);
                ValidarAppellido(usuario.Apellido);
                ValidarDNI(usuario.DNI);
                ValidarFechaNacimiento(usuario.FechaNacimiento);
                ValidarDireccion(usuario.Direccion);

                // Opcional: Si en el formulario de creación también validamos correo y pass:
                ValidarCorreo(usuario.Correo);

                // Si el usuario es nuevo, validamos la contraseña obligatoriamente.
                if (string.IsNullOrWhiteSpace(usuario.Password))
                {
                    throw new ValidacionException("La contraseña es obligatoria para un nuevo usuario.");
                }
                ValidarPassword(usuario.Password);

                // Si todas las líneas de arriba se ejecutaron sin lanzar un "throw", 
                // significa que los datos están perfectos.
                return true;
            }
            catch (ValidacionException ex)
            {
                // Si CUALQUIERA de tus métodos privados lanza un error, cae automáticamente acá.
                // Agarramos tu mensaje personalizado y lo mandamos a la interfaz.
                mensajeError = ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                // Por si ocurre algún otro error inesperado
                mensajeError = "Error inesperado al validar: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Valida los datos de un usuario en edición. Permite omitir la contraseña si no fue modificada,
        /// y excluye el propio ID al comprobar duplicidad de DNI y Correo.
        /// </summary>
        public bool ValidarDatosEdicionUsuario(Usuario usuario, out string mensajeError)
        {
            mensajeError = string.Empty;

            try
            {
                ValidarNombreUsuario(usuario.NombreUsuario);
                ValidarNombre(usuario.Nombree);
                ValidarAppellido(usuario.Apellido);
                ValidarDNI(usuario.DNI, usuario.IdUsuario);
                ValidarFechaNacimiento(usuario.FechaNacimiento);
                ValidarDireccion(usuario.Direccion);
                ValidarCorreo(usuario.Correo, usuario.IdUsuario);

                return true;
            }
            catch (ValidacionException ex)
            {
                mensajeError = ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                mensajeError = "Error inesperado al validar: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Valida que el nombre tenga formato correcto.
        /// - No debe ser nulo o vacío
        /// - Debe contener solo letras, espacios y caracteres acentuados
        /// - Debe tener entre 3 y 100 caracteres
        /// </summary>
        private void ValidarNombre(string nombree)
        {
            if (string.IsNullOrWhiteSpace(nombree))
                throw new ValidacionException("El nombre no puede estar vacío.");

            nombree = nombree.Trim();

            if (nombree.Length < 3 || nombree.Length > 100)
                throw new ValidacionException("El nombre debe tener entre 3 y 100 caracteres.");

            // Regex para validar que contenga solo letras, espacios y caracteres acentuados
            // Permite: a-z, A-Z, acentos (á, é, í, ó, ú, ñ, etc.), espacios
            string patronNombre = @"^[a-zA-ZáéíóúñÁÉÍÓÚÑ\s]+$";
            if (!Regex.IsMatch(nombree, patronNombre))
                throw new ValidacionException(
                    "El nombre solo puede contener letras, espacios y caracteres acentuados.");
        }
        private void ValidarAppellido(string apellido)
        {
            if (string.IsNullOrWhiteSpace(apellido))
                throw new ValidacionException("El apellido no puede estar vacío.");

            apellido = apellido.Trim();

            if (apellido.Length < 3 || apellido.Length > 100)
                throw new ValidacionException("El apellido debe tener entre 3 y 100 caracteres.");

            // Regex para validar que contenga solo letras, espacios y caracteres acentuados
            // Permite: a-z, A-Z, acentos (á, é, í, ó, ú, ñ, etc.), espacios
            string patronApellido = @"^[a-zA-ZáéíóúñÁÉÍÓÚÑ\s]+$";
            if (!Regex.IsMatch(apellido, patronApellido))
                throw new ValidacionException(
                    "El apellido solo puede contener letras, espacios y caracteres acentuados.");
        }

        /// <summary>
        /// Valida que el nombre de usuario tenga un formato válido.
        /// - No debe ser nulo o vacío
        /// - Debe contener solo letras minúsculas y números (sin espacios)
        /// - Debe tener entre 3 y 50 caracteres
        /// </summary>
        private void ValidarNombreUsuario(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                throw new ValidacionException("El nombre de usuario no puede estar vacío.");

            nombreUsuario = nombreUsuario.Trim();

            if (nombreUsuario.Length < 3 || nombreUsuario.Length > 50)
                throw new ValidacionException("El nombre de usuario debe tener entre 3 y 50 caracteres.");

            // Regex para validar que contenga solo letras minúsculas y números
            string patronNombreUsuario = @"^[a-z0-9]+$";
            if (!Regex.IsMatch(nombreUsuario, patronNombreUsuario))
                throw new ValidacionException(
                    "El nombre de usuario solo puede contener letras minúsculas y números.");
        }

        /// <summary>
        /// Valida que el DNI tenga un formato válido.
        /// - No debe ser nulo o vacío
        /// - Debe contener solo números
        /// - Debe tener entre 7 y 8 dígitos
        /// <summary>
        /// Valida que el DNI tenga un formato válido.
        /// - No debe ser nulo o vacío
        /// - Debe contener solo números
        /// - Debe tener entre 7 y 8 dígitos
        /// - No puede estar duplicado (solo un usuario por DNI, excluyendo el usuario actual en edición)
        /// </summary>
        private void ValidarDNI(string dni, int? excluirIdUsuario = null)
        {
            if (string.IsNullOrWhiteSpace(dni))
                throw new ValidacionException("El DNI no puede estar vacío.");

            dni = dni.Trim();

            // Regex para validar que contenga solo números entre 7 y 8 dígitos
            string patronDNI = @"^\d{7,8}$";
            if (!Regex.IsMatch(dni, patronDNI))
                throw new ValidacionException(
                    "El DNI debe contener solo números y tener entre 7 y 8 dígitos.");

            // Verifica que el DNI no esté duplicado en la base de datos
            if (usuarioDAL.ExisteUsuarioPorDNI(dni, excluirIdUsuario))
            {
                throw new UsuarioException(
                    $"El DNI '{dni}' ya está registrado en el sistema. El DNI debe ser único para cada usuario.");
            }
        }

        /// <summary>
        /// Valida que la fecha de nacimiento sea válida.
        /// - Debe ser una fecha válida
        /// - El usuario debe tener más de 18 años
        /// - El usuario debe tener menos de 90 años
        /// </summary>
        private void ValidarFechaNacimiento(DateTime fechaNacimiento)
        {
            // Calcular la edad actual
            DateTime hoy = DateTime.Today;
            int edad = hoy.Year - fechaNacimiento.Year;

            // Ajustar si aún no ha cumplido años este año
            if (fechaNacimiento.Date > hoy.AddYears(-edad))
                edad--;

            // Validar que sea mayor de 18 años
            if (edad < 18)
                throw new ValidacionException(
                    "El usuario debe ser mayor de 18 años para registrarse.");

            // Validar que no sea mayor de 90 años
            if (edad > 90)
                throw new ValidacionException(
                    "La fecha de nacimiento no parece ser válida. El usuario no puede ser mayor de 90 años.");

            // Validar que la fecha no sea futura
            if (fechaNacimiento > hoy)
                throw new ValidacionException(
                    "La fecha de nacimiento no puede ser una fecha futura.");
        }

        /// <summary>
        /// Valida que la dirección tenga un formato válido.
        /// - No debe ser nulo o vacío
        /// - Puede contener letras y números
        /// - Debe tener entre 5 y 200 caracteres
        /// </summary>
        private void ValidarDireccion(string direccion)
        {
            if (string.IsNullOrWhiteSpace(direccion))
                throw new ValidacionException("La dirección no puede estar vacía.");

            direccion = direccion.Trim();

            if (direccion.Length < 5 || direccion.Length > 200)
                throw new ValidacionException("La dirección debe tener entre 5 y 200 caracteres.");

            // Regex para validar que contenga letras, números, espacios y caracteres comunes en direcciones
            // Permite: a-z, A-Z, 0-9, espacios, puntos, comas, guiones
            string patronDireccion = @"^[a-zA-Z0-9áéíóúñÁÉÍÓÚÑ\s.,#\-]+$";
            if (!Regex.IsMatch(direccion, patronDireccion))
                throw new ValidacionException(
                    "La dirección contiene caracteres no válidos. Solo se permiten letras, números y caracteres comunes como puntos, comas y guiones.");
        }

        /// <summary>
        /// Valida que el correo tenga un formato válido.
        /// Utiliza una expresión regular para verificar estructura de email estándar.
        /// </summary>
        private void ValidarCorreo(string correo, int? excluirIdUsuario = null)
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

            if (usuarioDAL.ExisteUsuarioPorCorreo(correo, excluirIdUsuario))
            {
                throw new UsuarioException(
                    $"El correo '{correo}' ya está registrado en el sistema.");
            }
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
        /// Determina si una cadena ya representa un hash de BCrypt válido.
        /// </summary>
        public bool EsBCryptHash(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length != 60)
                return false;

            return Regex.IsMatch(password, @"^\$2[abxy]\$\d{2}\$[./0-9A-Za-z]{53}$");
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

        public Usuario ObtenerUsuarioPorCorreo(string correo)
        {
            // La BLL es la única autorizada para llamar a la DAL
            return usuarioDAL.ObtenerUsuarioPorCorreo(correo);
        }

        public List<Usuario> ObtenerUsuariosParaVista(int idRolUsuarioActual)
        {
            // 1. Regla: El vendedor no tiene acceso
            if (idRolUsuarioActual == (int)Roles.Vendedor)
            {
                throw new UnauthorizedAccessException("No tienes permisos para ver el listado de usuarios.");
            }

            // 2. Regla: El supervisor solo ve vendedores (pedimos filtrado directo a la DB)
            if (idRolUsuarioActual == (int)Roles.Supervisor)
            {
                // En lugar de enviar un 3 duro a la DAL, también enviamos el enum casteado
                return usuarioDAL.ObtenerPorRol((int)Roles.Vendedor);
            }

            // 3. Regla: El gerente ve a todos
            // Si no es ni vendedor ni supervisor, asumimos que es Gerente y pasa de largo
            return usuarioDAL.ObtenerTodos();
        }

        /// <summary>
        /// Actualiza la información de perfil de un usuario en el sistema.
        /// Solo permitido para usuarios con rol Gerente.
        /// No modifica la contraseña del usuario.
        /// </summary>
        /// <param name="usuarioActualizado">Entidad con los datos de perfil modificados</param>
        /// <param name="usuarioAutenticado">Usuario que ejecuta la acción (debe ser Gerente)</param>
        /// <returns>True si la actualización fue exitosa</returns>
        /// <exception cref="AutorizacionException">Si el usuario ejecutor no es Gerente</exception>
        /// <exception cref="ValidacionException">Si los datos a actualizar son inválidos</exception>
        public bool ActualizarUsuario(Usuario usuarioActualizado, Usuario usuarioAutenticado)
        {
            if (usuarioAutenticado == null || usuarioAutenticado.IdRol != (int)Roles.Gerente)
            {
                throw new AutorizacionException("No tiene permisos para modificar usuarios. Solo los usuarios con rol Gerente pueden realizar esta acción.");
            }

            if (usuarioActualizado == null)
            {
                throw new ValidacionException("Los datos del usuario a actualizar no pueden ser nulos.");
            }

            if (!ValidarDatosEdicionUsuario(usuarioActualizado, out string mensajeError))
            {
                throw new ValidacionException(mensajeError);
            }

            bool exito = usuarioDAL.ActualizarUsuario(usuarioActualizado);

            if (!exito)
            {
                throw new Exception("No se pudo actualizar el usuario en la base de datos.");
            }
            return exito;
        }

        /// <summary>
        /// Cambia la contraseña de un usuario en el sistema.
        /// Valida la contraseña en texto plano y aplica hashing determinista con BCrypt.
        /// </summary>
        /// <param name="idUsuario">ID del usuario a modificar</param>
        /// <param name="passwordPlano">Nueva contraseña en texto plano</param>
        /// <param name="usuarioAutenticado">Usuario que ejecuta la acción (Gerente o el propio usuario)</param>
        /// <returns>True si el cambio fue exitoso</returns>
        /// <exception cref="AutorizacionException">Si el usuario no tiene permisos</exception>
        /// <exception cref="ValidacionException">Si la contraseña no cumple con los requisitos</exception>
        public bool CambiarPassword(int idUsuario, string passwordPlano, Usuario usuarioAutenticado)
        {
            if (usuarioAutenticado == null ||
                (usuarioAutenticado.IdRol != (int)Roles.Gerente && usuarioAutenticado.IdUsuario != idUsuario))
            {
                throw new AutorizacionException("No tiene permisos para cambiar la contraseña de este usuario.");
            }

            if (idUsuario <= 0)
            {
                throw new ValidacionException("ID de usuario no válido.");
            }

            // Validar la contraseña en texto plano
            ValidarPassword(passwordPlano);

            // Hashear siempre con BCrypt (workFactor: 12)
            string passwordHasheada = HashearPassword(passwordPlano);

            // Persistir en base de datos
            bool exito = usuarioDAL.ActualizarPassword(idUsuario, passwordHasheada);
            if (!exito)
            {
                throw new Exception("No se pudo actualizar la contraseña en la base de datos.");
            }
            return exito;
        }
        public Usuario AutenticarUsuario(string correo, string passwordPlano)
        {
            // 1. Buscamos al usuario
            Usuario usuario = ObtenerUsuarioPorCorreo(correo);
            if (usuario == null)
                throw new Exception("Usuario o contraseña incorrectos.");

            // 2. Verificamos la contraseña
            if (!VerificarPassword(passwordPlano, usuario.Password))
                throw new Exception("Usuario o contraseña incorrectos.");

            // 3. NUEVA REGLA: Verificamos si está activo 
            // (Ajusta la propiedad según cómo se llame en tu entidad Usuario, ej: Activo, Estado, etc.)
            if (!usuario.Estado) // 
                throw new Exception("El usuario se encuentra inactivo. Contacte al administrador.");

            return usuario; // Si pasa todo, devolvemos el usuario autenticado
        }

        /// <summary>
        /// Realiza la baja lógica de un usuario.
        /// Solo permitido para usuarios con rol Gerente.
        /// </summary>
        /// <param name="idUsuarioObjetivo">ID del usuario a dar de baja</param>
        /// <param name="usuarioAutenticado">Usuario que ejecuta la acción (debe ser Gerente)</param>
        /// <returns>True si la baja lógica fue exitosa</returns>
        /// <exception cref="AutorizacionException">Si el usuario ejecutor no es Gerente</exception>
        /// <exception cref="ValidacionException">Si el ID es inválido o se intenta dar de baja a sí mismo</exception>
        public bool DarDeBajaUsuario(int idUsuarioObjetivo, Usuario usuarioAutenticado)
        {
            if (usuarioAutenticado == null || usuarioAutenticado.IdRol != (int)Roles.Gerente)
            {
                throw new AutorizacionException("No tiene permisos para dar de baja usuarios. Solo los usuarios con rol Gerente pueden realizar esta acción.");
            }

            if (idUsuarioObjetivo <= 0)
            {
                throw new ValidacionException("ID de usuario no válido.");
            }

            if (usuarioAutenticado.IdUsuario == idUsuarioObjetivo)
            {
                throw new ValidacionException("No puede dar de baja su propia cuenta de usuario.");
            }

            // Llamamos a la DAL para el borrado lógico
            return usuarioDAL.DarDeBajaUsuario(idUsuarioObjetivo);
        }

        /// <summary>
        /// Realiza el alta lógica de un usuario previamente desactivado.
        /// Solo permitido para usuarios con rol Gerente.
        /// </summary>
        /// <param name="idUsuarioObjetivo">ID del usuario a dar de alta</param>
        /// <param name="usuarioAutenticado">Usuario que ejecuta la acción (debe ser Gerente)</param>
        /// <returns>True si el alta lógica fue exitosa</returns>
        /// <exception cref="AutorizacionException">Si el usuario ejecutor no es Gerente</exception>
        /// <exception cref="ValidacionException">Si el ID es inválido</exception>
        public bool DarDeAltaUsuario(int idUsuarioObjetivo, Usuario usuarioAutenticado)
        {
            if (usuarioAutenticado == null || usuarioAutenticado.IdRol != (int)Roles.Gerente)
            {
                throw new AutorizacionException("No tiene permisos para dar de alta usuarios. Solo los usuarios con rol Gerente pueden realizar esta acción.");
            }

            if (idUsuarioObjetivo <= 0)
            {
                throw new ValidacionException("ID de usuario no válido.");
            }

            // Llamamos a la DAL para el borrado lógico
            return usuarioDAL.DarDeAltaUsuario(idUsuarioObjetivo);
        }

        public bool PuedeAccederPantallaUsuarios(int idRol)
        {
            // Vendedor no entra. Gerente y Supervisor sí.
            return idRol != (int)Roles.Vendedor;
        }

        public bool PuedeCrearOEditarUsuarios(int idRol)
        {
            // Solo el gerente puede editar/crear
            return idRol == (int)Roles.Gerente;
        }

        /// <summary>
        /// Obtiene usuarios filtrados por rol y/o estado
        /// </summary>
        /// <param name="idRol">ID del rol (null para no filtrar por rol)</param>
        /// <param name="estado">estado activo/inactivo (null para ambos)</param>
        /// <returns>Lista de usuarios filtrada</returns>
        public List<Usuario> ObtenerUsuariosFiltrados(int? idRol, bool? estado, int idRolUsuarioActual)
        {
            try
            {
                // 1. Regla: El vendedor no puede filtrar usuarios
                if (idRolUsuarioActual == (int)Roles.Vendedor)
                {
                    throw new UnauthorizedAccessException("No tienes permisos para filtrar usuarios.");
                }

                // 2. Regla: El supervisor solo puede filtrar vendedores
                if (idRolUsuarioActual == (int)Roles.Supervisor)
                {
                    // Si intenta filtrar por un rol diferente a vendedor, denegar acceso
                    if (idRol.HasValue && idRol.Value != (int)Roles.Vendedor)
                    {
                        throw new UnauthorizedAccessException("Los supervisores solo pueden ver vendedores.");
                    }
                    // Forzar que siempre filtre por vendedores
                    idRol = (int)Roles.Vendedor;
                }

                // 3. Regla: El gerente puede ver todos los roles (no hay restricción)

                return usuarioDAL.ObtenerUsuariosConFiltros(idRol, estado);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener usuarios filtrados: {ex.Message}", ex);
            }
        }
    }
}
