using SistemaVentas.Entities;

namespace SistemaVentas.GUI.Contexto
{
    /// <summary>
    /// Contexto global para mantener la sesión del usuario logueado.
    /// Accesible desde cualquier ventana de la aplicación para obtener
    /// información del usuario autenticado, su rol y permisos.
    /// </summary>
    public static class SesionGlobal
    {
        /// <summary>
        /// Usuario actualmente logueado en la aplicación.
        /// Se establece en Login.xaml.cs tras validación exitosa.
        /// </summary>
        public static Usuario UsuarioActual { get; set; }

        /// <summary>
        /// Verifica si el usuario actual tiene rol de Gerente (IdRol = 1).
        /// Utilizado para habilitación/deshabilitación de botones y funcionalidades.
        /// </summary>
        public static bool EsGerente => UsuarioActual?.IdRol == 1;

        /// <summary>
        /// Verifica si hay un usuario logueado en la sesión actual.
        /// Útil para validar acceso a ventanas que requieren autenticación.
        /// </summary>
        public static bool HayUsuarioLogueado => UsuarioActual != null;

        /// <summary>
        /// Obtiene el ID del usuario actualmente logueado.
        /// Retorna -1 si no hay usuario logueado.
        /// </summary>
        public static int IdUsuarioActual => UsuarioActual?.IdUsuario ?? -1;

        /// <summary>
        /// Obtiene el nombre del usuario actualmente logueado.
        /// Retorna "Invitado" si no hay usuario logueado.
        /// </summary>
        public static string NombreUsuarioActual => UsuarioActual?.Nombre ?? "Invitado";

        /// <summary>
        /// Obtiene el rol del usuario actualmente logueado.
        /// Retorna -1 si no hay usuario logueado.
        /// </summary>
        public static int IdRolActual => UsuarioActual?.IdRol ?? -1;

        /// <summary>
        /// Cierra la sesión actual limpiando el usuario logueado.
        /// Se ejecuta al hacer logout o al cerrar la aplicación.
        /// </summary>
        public static void CerrarSesion()
        {
            UsuarioActual = null;
        }
    }
}
