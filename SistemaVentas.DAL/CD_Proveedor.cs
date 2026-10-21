using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using SistemaVentas.Entities;

namespace SistemaVentas.DAL
{
    /// <summary>
    /// Clase de acceso a datos (DAL) para la entidad Proveedor.
    /// Solo ejecuta comandos SQL y mapea datos: no valida reglas de negocio
    /// ni redacta mensajes para la interfaz (eso es tarea de CN_Proveedor).
    /// </summary>
    public class CD_Proveedor
    {
        private readonly string cadena = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

        /// <summary>
        /// Lista únicamente los proveedores activos (activo = 1).
        /// </summary>
        public List<Proveedor> Listar()
        {
            List<Proveedor> lista = new List<Proveedor>();

            using (SqlConnection oconexion = new SqlConnection(cadena))
            {
                try
                {
                    string query = @"
                SELECT  pr.id_proveedor,
                        pr.razon_social,
                        pr.cuit,
                        pr.telefono,
                        pr.nombre_contacto,
                        pr.activo
                FROM    dbo.Proveedor pr
                WHERE   pr.activo = 1
                ORDER BY pr.razon_social ASC";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Proveedor()
                            {
                                IdProveedor = Convert.ToInt32(dr["id_proveedor"]),
                                RazonSocial = dr["razon_social"].ToString(),
                                Cuit = dr["cuit"].ToString(),
                                Telefono = dr["telefono"] != DBNull.Value ? dr["telefono"].ToString() : "",
                                NombreContacto = dr["nombre_contacto"] != DBNull.Value ? dr["nombre_contacto"].ToString() : "",
                                Activo = Convert.ToBoolean(dr["activo"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Detalle técnico solo para el log del desarrollador, la excepción sigue su curso
                    System.Diagnostics.Debug.WriteLine($"Error al consultar los proveedores: {ex.Message}");
                    throw;
                }
            }
            return lista;
        }

        /// <summary>
        /// Da de alta un proveedor. Devuelve true si la fila fue insertada.
        /// </summary>
        public bool Registrar(Proveedor obj)
        {
            using (SqlConnection oconexion = new SqlConnection(cadena))
            {
                try
                {
                    oconexion.Open();

                    string query = @"INSERT INTO dbo.Proveedor
                             (razon_social, cuit, telefono, nombre_contacto, activo)
                             VALUES
                             (@razon_social, @cuit, @telefono, @nombre_contacto, @activo)";

                    using (SqlCommand cmd = new SqlCommand(query, oconexion))
                    {
                        cmd.Parameters.AddWithValue("@razon_social", obj.RazonSocial);
                        cmd.Parameters.AddWithValue("@cuit", obj.Cuit);
                        cmd.Parameters.AddWithValue("@telefono", string.IsNullOrWhiteSpace(obj.Telefono) ? (object)DBNull.Value : obj.Telefono);
                        cmd.Parameters.AddWithValue("@nombre_contacto", string.IsNullOrWhiteSpace(obj.NombreContacto) ? (object)DBNull.Value : obj.NombreContacto);
                        cmd.Parameters.AddWithValue("@activo", obj.Activo ? 1 : 0);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    // Detalle técnico solo para el log del desarrollador, la excepción sigue su curso
                    System.Diagnostics.Debug.WriteLine($"Error SQL al registrar proveedor: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Modifica los datos de un proveedor existente. Devuelve true si se actualizó la fila.
        /// </summary>
        public bool Editar(Proveedor obj)
        {
            using (SqlConnection oconexion = new SqlConnection(cadena))
            {
                try
                {
                    oconexion.Open();

                    string query = @"UPDATE dbo.Proveedor SET
                             razon_social = @razon_social,
                             cuit = @cuit,
                             telefono = @telefono,
                             nombre_contacto = @nombre_contacto,
                             activo = @activo
                             WHERE id_proveedor = @id_proveedor";

                    using (SqlCommand cmd = new SqlCommand(query, oconexion))
                    {
                        cmd.Parameters.AddWithValue("@id_proveedor", obj.IdProveedor);
                        cmd.Parameters.AddWithValue("@razon_social", obj.RazonSocial);
                        cmd.Parameters.AddWithValue("@cuit", obj.Cuit);
                        cmd.Parameters.AddWithValue("@telefono", string.IsNullOrWhiteSpace(obj.Telefono) ? (object)DBNull.Value : obj.Telefono);
                        cmd.Parameters.AddWithValue("@nombre_contacto", string.IsNullOrWhiteSpace(obj.NombreContacto) ? (object)DBNull.Value : obj.NombreContacto);
                        cmd.Parameters.AddWithValue("@activo", obj.Activo ? 1 : 0);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    // Detalle técnico solo para el log del desarrollador, la excepción sigue su curso
                    System.Diagnostics.Debug.WriteLine($"Error SQL al editar proveedor ID {obj.IdProveedor}: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Obtiene un proveedor por su ID. Devuelve null si no existe.
        /// </summary>
        public Proveedor? ObtenerPorId(int idProveedor)
        {
            using (SqlConnection oconexion = new SqlConnection(cadena))
            {
                try
                {
                    string query = @"SELECT pr.id_proveedor,
                                           pr.razon_social,
                                           pr.cuit,
                                           pr.telefono,
                                           pr.nombre_contacto,
                                           pr.activo
                                    FROM dbo.Proveedor pr
                                    WHERE pr.id_proveedor = @id_proveedor";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@id_proveedor", idProveedor);

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new Proveedor()
                            {
                                IdProveedor = Convert.ToInt32(dr["id_proveedor"]),
                                RazonSocial = dr["razon_social"].ToString(),
                                Cuit = dr["cuit"].ToString(),
                                Telefono = dr["telefono"] != DBNull.Value ? dr["telefono"].ToString() : "",
                                NombreContacto = dr["nombre_contacto"] != DBNull.Value ? dr["nombre_contacto"].ToString() : "",
                                Activo = Convert.ToBoolean(dr["activo"])
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Detalle técnico solo para el log del desarrollador, la excepción sigue su curso
                    System.Diagnostics.Debug.WriteLine($"Error al consultar el proveedor ID {idProveedor}: {ex.Message}");
                    throw;
                }
            }

            return null;
        }
    }
}
