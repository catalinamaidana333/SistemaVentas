using SistemaVentas.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SistemaVentas.DAL
{
    public class MovimientosCajaDAL
    {
        private string _cadenaConexion;

        public MovimientosCajaDAL()
        {
            // segun App.config 
            _cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;
        }

        public bool InsertarMovimiento(MovimientosCaja mov)
        {
            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                string query = @"INSERT INTO MovimientosCaja 
                         (id_caja, id_caja_usuario, id_usuario, tipo, monto, motivo, fecha_hora) 
                         VALUES (@id_caja, @id_caja_usuario, @id_usuario, @tipo, @monto, @motivo, @fecha_hora)";

                SqlCommand cmd = new SqlCommand(query, conexion);

                cmd.Parameters.AddWithValue("@id_caja", mov.IdCaja);

                // Manejo del valor nulo para la sesión de caja
                if (mov.IdCajaUsuario.HasValue)
                    cmd.Parameters.AddWithValue("@id_caja_usuario", mov.IdCajaUsuario.Value);
                else
                    cmd.Parameters.AddWithValue("@id_caja_usuario", DBNull.Value);

                cmd.Parameters.AddWithValue("@id_usuario", mov.IdUsuario);
                cmd.Parameters.AddWithValue("@tipo", mov.Tipo);
                cmd.Parameters.AddWithValue("@monto", mov.Monto);
                cmd.Parameters.AddWithValue("@motivo", mov.Motivo);
                cmd.Parameters.AddWithValue("@fecha_hora", mov.FechaHora);

                conexion.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public decimal ObtenerSaldoNetoPorSesion(int idCajaUsuario)
        {
            const string query = @"SELECT ISNULL(SUM(CASE
                                        WHEN tipo = 'INGRESO' THEN monto
                                        WHEN tipo = 'EGRESO' THEN -monto
                                        ELSE 0
                                    END), 0)
                                  FROM MovimientosCaja
                                  WHERE id_caja_usuario = @id_caja_usuario";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            using (SqlCommand cmd = new SqlCommand(query, conexion))
            {
                cmd.Parameters.AddWithValue("@id_caja_usuario", idCajaUsuario);
                conexion.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }
    }
}
