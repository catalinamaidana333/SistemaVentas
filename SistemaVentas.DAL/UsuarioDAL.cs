using SistemaVentas.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace SistemaVentas.DAL
{
    /// <summary>
    /// Clase de acceso a datos (DAL) para la entidad Usuario.
    /// Utiliza SQL Server y la conexión definida en App.config.
    /// </summary>
    public class UsuarioDAL
    {
        private string _cadenaConexion;
        // Constructor vacío (ya no pide parámetros)
        public UsuarioDAL()
        {
            // Revisa tu App.config y reemplaza "NombreDeTuConexion" por el valor exacto del atributo name="..."
            _cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;
        }

        /// <summary>
        /// Verifica si ya existe un usuario con el correo especificado
        /// </summary>
        public bool ExisteUsuarioPorCorreo(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return false;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();

                    string consulta = "SELECT COUNT(*) FROM Usuario WHERE Correo = @correo";
                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@correo", correo);
                        int cantidad = (int)comando.ExecuteScalar();
                        return cantidad > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new UsuarioException($"Error al verificar correo en base de datos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        public Usuario ObtenerUsuarioPorId(int idUsuario)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();

                    string consulta = "SELECT IdUsuario, Nombre, Correo, Password, IdRol FROM Usuario WHERE IdUsuario = @idUsuario";
                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@idUsuario", idUsuario);
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            if (lector.Read())
                            {
                                return new Usuario
                                {
                                    IdUsuario = (int)lector["IdUsuario"],
                                    Nombre = lector["Nombre"].ToString(),
                                    Correo = lector["Correo"].ToString(),
                                    Password = lector["Password"].ToString(),
                                    IdRol = (int)lector["IdRol"]
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new UsuarioException($"Error al obtener usuario por ID: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Obtiene un usuario por su correo
        /// </summary>
        public Usuario ObtenerUsuarioPorCorreo(string correo)
        {
            Usuario usuario = null;

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                // 1. El string SQL usa estrictamente los nombres de la base de datos
                string query = "SELECT id_usuario, nombre_completo, correo, password, id_rol, activo FROM Usuario WHERE correo = @correo";

                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@correo", correo);

                conexion.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        usuario = new Usuario();

                        // 2. Las propiedades de C# (izquierda) reciben los datos de las columnas SQL (derecha)
                        usuario.IdUsuario = Convert.ToInt32(reader["id_usuario"]);
                        usuario.Nombre = reader["nombre_completo"].ToString();
                        usuario.Correo = reader["correo"].ToString();
                        usuario.Password = reader["password"].ToString();
                        usuario.IdRol = Convert.ToInt32(reader["id_rol"]);
                        usuario.Estado = Convert.ToBoolean(reader["activo"]);
                    }
                }
            }
            return usuario;
        }

        /// <summary>
        /// Guarda un nuevo usuario en la base de datos SQL Server
        /// </summary>
        public int GuardarUsuario(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();

                    string consulta = @"INSERT INTO Usuario (Nombre, Correo, Password, IdRol) 
                                       VALUES (@nombre, @correo, @password, @idRol);
                                       SELECT SCOPE_IDENTITY();";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@nombre", usuario.Nombre);
                        comando.Parameters.AddWithValue("@correo", usuario.Correo);
                        comando.Parameters.AddWithValue("@password", usuario.Password);
                        comando.Parameters.AddWithValue("@idRol", usuario.IdRol);

                        int idUsuarioCreado = Convert.ToInt32(comando.ExecuteScalar());
                        return idUsuarioCreado;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new UsuarioException($"Error al guardar usuario en base de datos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza un usuario existente en la base de datos
        /// </summary>
        public bool ActualizarUsuario(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();

                    string consulta = @"UPDATE Usuario 
                                       SET Nombre = @nombre, Correo = @correo, Password = @password, IdRol = @idRol
                                       WHERE IdUsuario = @idUsuario";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@nombre", usuario.Nombre);
                        comando.Parameters.AddWithValue("@correo", usuario.Correo);
                        comando.Parameters.AddWithValue("@password", usuario.Password);
                        comando.Parameters.AddWithValue("@idRol", usuario.IdRol);
                        comando.Parameters.AddWithValue("@idUsuario", usuario.IdUsuario);

                        int filasAfectadas = comando.ExecuteNonQuery();
                        return filasAfectadas > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new UsuarioException($"Error al actualizar usuario: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        public List<Usuario> ObtenerTodos()
        {
            List<Usuario> usuarios = new List<Usuario>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();

                    string consulta = "SELECT IdUsuario, Nombre, Correo, Password, IdRol FROM Usuario";
                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                usuarios.Add(new Usuario
                                {
                                    IdUsuario = (int)lector["IdUsuario"],
                                    Nombre = lector["Nombre"].ToString(),
                                    Correo = lector["Correo"].ToString(),
                                    Password = lector["Password"].ToString(),
                                    IdRol = (int)lector["IdRol"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new UsuarioException($"Error al obtener usuarios: {ex.Message}", ex);
            }

            return usuarios;
        }
    }
}
