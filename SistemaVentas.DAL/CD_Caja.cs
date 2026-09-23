using SistemaVentas.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SistemaVentas.DAL
{
    public static class CD_Conexion
    {
        public static SqlConnection ObtenerConexion()
        {
            string cadena = ConfigurationManager.ConnectionStrings["ConexionBD"]?.ConnectionString;

            if (string.IsNullOrEmpty(cadena))
            {
                throw new InvalidOperationException("La cadena de conexión 'ConexionBD' no está configurada en App.config.");
            }

            return new SqlConnection(cadena);
        }
    }
    public class CD_Caja
    {
        public List<Caja> Listar()
        {
            List<Caja> lista = new List<Caja>();

            using (SqlConnection oconexion = CD_Conexion.ObtenerConexion())
            {
                try
                {
                    string query = "SELECT id_caja, descripcion, activa FROM dbo.Caja";
                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Caja()
                            {
                                IdCaja = Convert.ToInt32(dr["id_caja"]),
                                Descripcion = dr["descripcion"].ToString(),
                                Activa = Convert.ToBoolean(dr["activa"]) // Se usa 'activa' y 'Activa'
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    lista = new List<Caja>();
                }
            }

            return lista;
        }

        public bool Registrar(Caja obj, out string Mensaje)
        {
            bool respuesta = false;
            Mensaje = string.Empty;

            using (SqlConnection oconexion = CD_Conexion.ObtenerConexion())
            {
                try
                {
                    string query = "INSERT INTO dbo.Caja (descripcion, activa) VALUES (@descripcion, @activa)";
                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@descripcion", obj.Descripcion);
                    cmd.Parameters.AddWithValue("@activa", obj.Activa); // Se usa obj.Activa en lugar de obj.Estado
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();
                    int filas = cmd.ExecuteNonQuery();
                    if (filas > 0) respuesta = true;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    Mensaje = ex.Message;
                }
            }

            return respuesta;
        }
    }
}
