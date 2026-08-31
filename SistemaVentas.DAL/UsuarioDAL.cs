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

        public UsuarioDAL(string cadenaConexion)
        {
            if (string.IsNullOrWhiteSpace(cadenaConexion))
                throw new ArgumentException("La cadena de conexión no puede estar vacía");

            _cadenaConexion = cadenaConexion;
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
            if (string.IsNullOrWhiteSpace(correo))
                return null;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();

                    string consulta = "SELECT IdUsuario, Nombre, Correo, Password, IdRol FROM Usuario WHERE Correo = @correo";
                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@correo", correo);
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
                throw new UsuarioException($"Error al obtener usuario por correo: {ex.Message}", ex);
            }

            return null;
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
