using SistemaVentas.Entities;
using SistemaVentas.BLL;
using System;

namespace SistemaVentas.Ejemplos
{
    /// <summary>
    /// Ejemplo de uso de la capa BLL para crear usuarios con validaciones
    /// y seguridad implementadas en la arquitectura de capas.
    /// </summary>
    public class EjemploCrearUsuario
    {
        public static void Main()
        {
            // Instanciar la BLL de Usuario
            var usuarioBLL = new UsuarioBLL();

            // Crear el usuario autenticado (Gerente - IdRol = 1)
            var usuarioGerente = new Usuario
            {
                IdUsuario = 1,
                Nombre = "Juan Gerente",
                Correo = "juan.gerente@empresa.com",
                Password = "contraseña hasheada...",
                IdRol = 1 // Gerente
            };

            // Crear un nuevo usuario para registrar
            var nuevoUsuario = new Usuario
            {
                Nombre = "Carlos Empleado",
                Correo = "carlos@empresa.com",
                Password = "Abc123456", // Será hasheada por la BLL
                IdRol = 2 // Empleado
            };

            try
            {
                // PASO 1: Crear usuario (requiere autorización de Gerente)
                int idNuevoUsuario = usuarioBLL.CrearUsuario(nuevoUsuario, usuarioGerente);
                Console.WriteLine($"✓ Usuario creado exitosamente con ID: {idNuevoUsuario}");
                Console.WriteLine($"  Contraseña hasheada y almacenada de forma segura en BD");
            }
            catch (AutorizacionException ex)
            {
                // Si quien intenta crear no es Gerente
                Console.WriteLine($"❌ Error de autorización: {ex.Message}");
            }
            catch (ValidacionException ex)
            {
                // Si los datos no cumplen con validaciones
                Console.WriteLine($"❌ Error de validación: {ex.Message}");
            }
            catch (UsuarioException ex)
            {
                // Si hay error general
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        /// <summary>
        /// CASOS DE USO Y EJEMPLOS DE EXCEPCIONES
        /// </summary>
        public static void MostrarEjemplosCasosFallo()
        {
            var usuarioBLL = new UsuarioBLL();

            // Usuario NO gerente (IdRol != 1)
            var usuarioNoGerente = new Usuario { IdRol = 2 };

            var usuario = new Usuario
            {
                Nombre = "Test",
                Correo = "test@test.com",
                Password = "Test1234"
            };

            // CASO 1: Falta de autorización
            try
            {
                usuarioBLL.CrearUsuario(usuario, usuarioNoGerente);
            }
            catch (AutorizacionException ex)
            {
                Console.WriteLine($"CASO 1 - Autorización rechazada: {ex.Message}");
                // "No tiene autorización para crear usuarios. Solo los Gerentes (IdRol: 1) pueden..."
            }

            // CASO 2: Nombre inválido (contiene números)
            try
            {
                var usuarioGerente = new Usuario { IdRol = 1 };
                var usuarioInvalido = new Usuario
                {
                    Nombre = "Juan123", // INVÁLIDO
                    Correo = "juan@test.com",
                    Password = "Test1234"
                };
                usuarioBLL.CrearUsuario(usuarioInvalido, usuarioGerente);
            }
            catch (ValidacionException ex)
            {
                Console.WriteLine($"CASO 2 - Nombre inválido: {ex.Message}");
                // "El nombre solo puede contener letras, espacios y caracteres acentuados."
            }

            // CASO 3: Correo inválido
            try
            {
                var usuarioGerente = new Usuario { IdRol = 1 };
                var usuarioInvalido = new Usuario
                {
                    Nombre = "Juan García",
                    Correo = "correoinvalido", // INVÁLIDO
                    Password = "Test1234"
                };
                usuarioBLL.CrearUsuario(usuarioInvalido, usuarioGerente);
            }
            catch (ValidacionException ex)
            {
                Console.WriteLine($"CASO 3 - Correo inválido: {ex.Message}");
                // "El correo 'correoinvalido' no tiene un formato válido..."
            }

            // CASO 4: Contraseña demasiado corta
            try
            {
                var usuarioGerente = new Usuario { IdRol = 1 };
                var usuarioInvalido = new Usuario
                {
                    Nombre = "Juan García",
                    Correo = "juan@test.com",
                    Password = "Test" // INVÁLIDO - menos de 8 caracteres
                };
                usuarioBLL.CrearUsuario(usuarioInvalido, usuarioGerente);
            }
            catch (ValidacionException ex)
            {
                Console.WriteLine($"CASO 4 - Contraseña inválida: {ex.Message}");
                // "La contraseña debe tener mínimo 8 caracteres."
            }

            // CASO 5: Correo duplicado
            try
            {
                var usuarioGerente = new Usuario { IdRol = 1 };
                var usuarioPrimero = new Usuario
                {
                    Nombre = "Juan García",
                    Correo = "juan@test.com",
                    Password = "Test12345"
                };

                var usuarioSegundo = new Usuario
                {
                    Nombre = "Otra Persona",
                    Correo = "juan@test.com", // DUPLICADO
                    Password = "Contraseña123"
                };

                // Si el primer usuario ya existe en BD
                usuarioBLL.CrearUsuario(usuarioSegundo, usuarioGerente);
            }
            catch (UsuarioException ex)
            {
                Console.WriteLine($"CASO 5 - Correo duplicado: {ex.Message}");
                // "El correo 'juan@test.com' ya está registrado en el sistema."
            }
        }
    }
}
