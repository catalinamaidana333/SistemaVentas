using SistemaVentas.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SistemaVentas.DAL
{
    /// <summary>
    /// Clase de acceso a datos (DAL) para la entidad Usuario.
    /// Utiliza SQL Server y la conexión definida en App.config.
    /// </summary>
    public class UsuarioDAL
    {
        private string _cadenaConexion;

        public UsuarioDAL()
        {
            // segun App.config 
            _cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;
        }

        /// <summary>
        /// Verifica si ya existe un usuario con el correo especificado
        /// </summary>
        public bool ExisteUsuarioPorCorreo(string correo, int? excluirIdUsuario = null)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return false;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();

                    string consulta = excluirIdUsuario.HasValue
                        ? "SELECT COUNT(*) FROM Usuario WHERE Correo = @correo AND id_usuario != @excluirIdUsuario"
                        : "SELECT COUNT(*) FROM Usuario WHERE Correo = @correo";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@correo", correo);
                        if (excluirIdUsuario.HasValue)
                        {
                            comando.Parameters.AddWithValue("@excluirIdUsuario", excluirIdUsuario.Value);
                        }
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
        /// Verifica si ya existe un usuario con el DNI especificado
        /// </summary>
        public bool ExisteUsuarioPorDNI(string dni, int? excluirIdUsuario = null)
        {
            if (string.IsNullOrWhiteSpace(dni))
                return false;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();

                    string consulta = excluirIdUsuario.HasValue
                        ? "SELECT COUNT(*) FROM Usuario WHERE dni = @dni AND id_usuario != @excluirIdUsuario"
                        : "SELECT COUNT(*) FROM Usuario WHERE dni = @dni";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@dni", dni);
                        if (excluirIdUsuario.HasValue)
                        {
                            comando.Parameters.AddWithValue("@excluirIdUsuario", excluirIdUsuario.Value);
                        }
                        int cantidad = (int)comando.ExecuteScalar();
                        return cantidad > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new UsuarioException($"Error al verificar DNI en base de datos: {ex.Message}", ex);
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

                    // ACTUALIZADO: Traemos los campos nuevos en vez de nombre_completo
                    string consulta = "SELECT id_usuario, nombre_usuario, nombre, apellido, dni, fecha_nacimiento, direccion, correo, password, id_rol FROM Usuario WHERE id_usuario = @idUsuario";
                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@idUsuario", idUsuario);
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            if (lector.Read())
                            {
                                return new Usuario
                                {
                                    IdUsuario = (int)lector["id_usuario"],
                                    NombreUsuario = lector["nombre_usuario"].ToString(),
                                    Nombree = lector["nombre"].ToString(),
                                    Apellido = lector["apellido"].ToString(),
                                    DNI = lector["dni"].ToString(),
                                    // Validamos si la fecha es NULL en la BD para que no explote
                                    FechaNacimiento = lector["fecha_nacimiento"] != DBNull.Value ? Convert.ToDateTime(lector["fecha_nacimiento"]) : DateTime.MinValue,
                                    Direccion = lector["direccion"].ToString(),
                                    Correo = lector["correo"].ToString(),
                                    Password = lector["password"].ToString(),
                                    IdRol = (int)lector["id_rol"]
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
                // ACTUALIZADO: Traemos los campos nuevos
                string query = "SELECT id_usuario, nombre_usuario, nombre, apellido, dni, fecha_nacimiento, direccion, correo, password, id_rol, activo FROM Usuario WHERE correo = @correo";

                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@correo", correo);

                conexion.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        usuario = new Usuario();

                        usuario.IdUsuario = Convert.ToInt32(reader["id_usuario"]);
                        usuario.NombreUsuario = reader["nombre_usuario"].ToString();
                        usuario.Nombree = reader["nombre"].ToString();
                        usuario.Apellido = reader["apellido"].ToString();
                        usuario.DNI = reader["dni"].ToString();
                        usuario.FechaNacimiento = reader["fecha_nacimiento"] != DBNull.Value ? Convert.ToDateTime(reader["fecha_nacimiento"]) : DateTime.MinValue;
                        usuario.Direccion = reader["direccion"].ToString();
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

                    // ACTUALIZADO: INSERT con todos los campos nuevos
                    string consulta = @"INSERT INTO Usuario (nombre_usuario, nombre, apellido, dni, fecha_nacimiento, direccion, correo, password, id_rol) 
                                       VALUES (@nombreUsuario, @nombre, @apellido, @dni, @fechaNacimiento, @direccion, @correo, @password, @idRol);
                                       SELECT SCOPE_IDENTITY();";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        // Usamos ?? DBNull.Value por si en algún momento llega un dato nulo, que se guarde en SQL correctamente
                        comando.Parameters.AddWithValue("@nombreUsuario", string.IsNullOrEmpty(usuario.NombreUsuario) ? (object)DBNull.Value : usuario.NombreUsuario);
                        comando.Parameters.AddWithValue("@nombre", string.IsNullOrEmpty(usuario.Nombree) ? (object)DBNull.Value : usuario.Nombree);
                        comando.Parameters.AddWithValue("@apellido", string.IsNullOrEmpty(usuario.Apellido) ? (object)DBNull.Value : usuario.Apellido);
                        comando.Parameters.AddWithValue("@dni", string.IsNullOrEmpty(usuario.DNI) ? (object)DBNull.Value : usuario.DNI);
                        comando.Parameters.AddWithValue("@fechaNacimiento", usuario.FechaNacimiento);
                        comando.Parameters.AddWithValue("@direccion", string.IsNullOrEmpty(usuario.Direccion) ? (object)DBNull.Value : usuario.Direccion);

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
        /// Actualiza un usuario existente en la base de datos (solo datos de perfil, no modifica la contraseña)
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
                            SET nombre_usuario = @nombreUsuario, 
                                nombre = @nombre, 
                                apellido = @apellido, 
                                dni = @dni, 
                                fecha_nacimiento = @fechaNacimiento, 
                                direccion = @direccion, 
                                correo = @correo, 
                                id_rol = @idRol
                            WHERE id_usuario = @idUsuario";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@nombreUsuario", string.IsNullOrEmpty(usuario.NombreUsuario) ? (object)DBNull.Value : usuario.NombreUsuario);
                        comando.Parameters.AddWithValue("@nombre", string.IsNullOrEmpty(usuario.Nombree) ? (object)DBNull.Value : usuario.Nombree);
                        comando.Parameters.AddWithValue("@apellido", string.IsNullOrEmpty(usuario.Apellido) ? (object)DBNull.Value : usuario.Apellido);
                        comando.Parameters.AddWithValue("@dni", string.IsNullOrEmpty(usuario.DNI) ? (object)DBNull.Value : usuario.DNI);
                        comando.Parameters.AddWithValue("@fechaNacimiento", usuario.FechaNacimiento);
                        comando.Parameters.AddWithValue("@direccion", string.IsNullOrEmpty(usuario.Direccion) ? (object)DBNull.Value : usuario.Direccion);
                        comando.Parameters.AddWithValue("@correo", usuario.Correo);
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
        /// Actualiza exclusivamente el hash de la contraseña de un usuario en la base de datos.
        /// </summary>
        public bool ActualizarPassword(int idUsuario, string passwordHasheada)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();

                    string consulta = @"UPDATE Usuario 
                            SET password = @password 
                            WHERE id_usuario = @idUsuario";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@password", passwordHasheada);
                        comando.Parameters.AddWithValue("@idUsuario", idUsuario);

                        int filasAfectadas = comando.ExecuteNonQuery();
                        return filasAfectadas > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new UsuarioException($"Error al actualizar contraseña del usuario: {ex.Message}", ex);
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

                    string consulta = "SELECT id_usuario, nombre_usuario, nombre, apellido, dni, fecha_nacimiento, direccion, correo, password, id_rol, activo FROM Usuario";
                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                usuarios.Add(new Usuario
                                {
                                    IdUsuario = (int)lector["id_usuario"],
                                    NombreUsuario = lector["nombre_usuario"].ToString(),
                                    Nombree = lector["nombre"].ToString(),
                                    Apellido = lector["apellido"].ToString(),
                                    DNI = lector["dni"].ToString(),
                                    FechaNacimiento = lector["fecha_nacimiento"] != DBNull.Value ? Convert.ToDateTime(lector["fecha_nacimiento"]) : DateTime.MinValue,
                                    Direccion = lector["direccion"].ToString(),
                                    Correo = lector["correo"].ToString(),
                                    Password = lector["password"].ToString(),
                                    IdRol = (int)lector["id_rol"],
                                    Estado = lector["activo"] != DBNull.Value ? Convert.ToBoolean(lector["activo"]) : true
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

        public List<Usuario> ObtenerPorRol(int idRolBuscado)
        {
            List<Usuario> usuarios = new List<Usuario>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    string consulta = "SELECT id_usuario, nombre_usuario, nombre, apellido, dni, fecha_nacimiento, direccion, correo, password, id_rol, activo FROM Usuario WHERE id_rol = @IdRol";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@IdRol", idRolBuscado);

                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                usuarios.Add(new Usuario
                                {
                                    IdUsuario = (int)lector["id_usuario"],
                                    NombreUsuario = lector["nombre_usuario"].ToString(),
                                    Nombree = lector["nombre"].ToString(),
                                    Apellido = lector["apellido"].ToString(),
                                    DNI = lector["dni"].ToString(),
                                    FechaNacimiento = lector["fecha_nacimiento"] != DBNull.Value ? Convert.ToDateTime(lector["fecha_nacimiento"]) : DateTime.MinValue,
                                    Direccion = lector["direccion"].ToString(),
                                    Correo = lector["correo"].ToString(),
                                    Password = lector["password"].ToString(),
                                    IdRol = (int)lector["id_rol"],
                                    Estado = lector["activo"] != DBNull.Value ? Convert.ToBoolean(lector["activo"]) : true
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new UsuarioException($"Error al filtrar usuarios por rol: {ex.Message}", ex);
            }

            return usuarios;
        }

        public bool DarDeBajaUsuario(int idUsuario)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                try
                {
                    string query = "UPDATE Usuario SET activo = 0 WHERE id_usuario = @IdUsuario";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                    conexion.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    respuesta = filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error en la base de datos al dar de baja: " + ex.Message);
                }
            }
            return respuesta;
        }
        public bool DarDeAltaUsuario(int idUsuario)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                try
                {
                    string query = "UPDATE Usuario SET activo = 1 WHERE id_usuario = @IdUsuario";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                    conexion.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    respuesta = filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error en la base de datos al dar de alta: " + ex.Message);
                }
            }
            return respuesta;
        }
        /// <summary>
        /// Obtiene usuarios con filtros opcionales por rol y estado
        /// </summary>
        /// <param name="idRol">ID del rol a filtrar (null = sin filtro de rol)</param>
        /// <param name="estado">Estado activo/inactivo (null = ambos estados)</param>
        /// <returns>Lista de usuarios que coinciden con los filtros</returns>
        public List<Usuario> ObtenerUsuariosConFiltros(int? idRol, bool? estado)
        {
            List<Usuario> usuarios = new List<Usuario>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();

                    // Consulta base - siempre incluye el campo activo
                    string consulta = "SELECT id_usuario, nombre_usuario, nombre, apellido, dni, fecha_nacimiento, direccion, correo, password, id_rol, activo FROM Usuario WHERE 1=1";

                    // Agregamos filtros según lo que se haya especificado
                    if (idRol.HasValue)
                    {
                        consulta += " AND id_rol = @IdRol";
                    }

                    if (estado.HasValue)
                    {
                        consulta += " AND activo = @Estado";
                    }

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        // Agregamos parámetros solo si tienen valor
                        if (idRol.HasValue)
                        {
                            comando.Parameters.AddWithValue("@IdRol", idRol.Value);
                        }

                        if (estado.HasValue)
                        {
                            comando.Parameters.AddWithValue("@Estado", estado.Value);
                        }

                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                usuarios.Add(new Usuario
                                {
                                    IdUsuario = (int)lector["id_usuario"],
                                    NombreUsuario = lector["nombre_usuario"].ToString(),
                                    Nombree = lector["nombre"].ToString(),
                                    Apellido = lector["apellido"].ToString(),
                                    DNI = lector["dni"].ToString(),
                                    FechaNacimiento = lector["fecha_nacimiento"] != DBNull.Value ? Convert.ToDateTime(lector["fecha_nacimiento"]) : DateTime.MinValue,
                                    Direccion = lector["direccion"].ToString(),
                                    Correo = lector["correo"].ToString(),
                                    Password = lector["password"].ToString(),
                                    IdRol = (int)lector["id_rol"],
                                    Estado = lector["activo"] != DBNull.Value ? Convert.ToBoolean(lector["activo"]) : true
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new UsuarioException($"Error al obtener usuarios con filtros: {ex.Message}", ex);
            }

            return usuarios;
        }

    }
}

