using SistemaVentas.Entities;
using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SistemaVentas.DAL
{
    public class CajaUsuarioDAL
    {
        private string _cadenaConexion;

        public CajaUsuarioDAL()
        {
            _cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;
        }

        public int AbrirCaja(CajaUsuario cajaUsuario)
        {
            int idGenerado = 0;
            // Nombres de columna en snake_case. Estado = 1 (True/Abierta)
            string query = @"INSERT INTO CajaUsuario 
                             (id_caja, id_usuario, fecha_apertura, monto_inicial, estado) 
                             VALUES (@id_caja, @id_usuario, GETDATE(), @monto_inicial, 1);
                             SELECT SCOPE_IDENTITY();";

            using (SqlConnection con = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id_caja", cajaUsuario.IdCaja);
                    cmd.Parameters.AddWithValue("@id_usuario", cajaUsuario.IdUsuario);
                    cmd.Parameters.AddWithValue("@monto_inicial", cajaUsuario.MontoInicial);

                    con.Open();
                    idGenerado = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            return idGenerado;
        }

        public void CerrarCaja(CajaUsuario cajaUsuario)
        {
            // Corregimos los nombres de las columnas a la izquierda del '=' 
            // para que coincidan exactamente con tu tabla SQL Server.
            string query = @"UPDATE CajaUsuario 
                     SET fecha_cierre = GETDATE(), 
                         monto_declarado = @monto_declarado,
                         total_ventas_efectivo = @total_ventas_efectivo,
                         total_ventas_mp = @total_ventas_mp,
                         total_compras_efectivo = @monto_compras_efectivo,
                         monto_sys = @monto_sys, 
                         diferencia = @diferencia, 
                         estado = 0 
                     WHERE id_caja_usuario = @id_caja_usuario";

            using (SqlConnection con = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@monto_declarado", cajaUsuario.MontoCierre);
                    cmd.Parameters.AddWithValue("@total_ventas_efectivo", cajaUsuario.MontoTotalVentasEfectivo);
                    cmd.Parameters.AddWithValue("@total_ventas_mp", cajaUsuario.MontoTotalVentasMp);
                    cmd.Parameters.AddWithValue("@monto_compras_efectivo", cajaUsuario.MontoTotalGastosEfec);
                    cmd.Parameters.AddWithValue("@monto_sys", cajaUsuario.MontoSistema);
                    cmd.Parameters.AddWithValue("@diferencia", cajaUsuario.Diferencia);
                    cmd.Parameters.AddWithValue("@id_caja_usuario", cajaUsuario.IdCajaUsuario);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public CajaUsuario ObtenerSesionAbiertaPorUsuario(int idUsuario)
        {
            CajaUsuario sesion = null;
            // Filtramos por estado = 1 (Abierta)
            string query = @"SELECT id_caja_usuario, id_caja, id_usuario, fecha_apertura, monto_inicial, estado 
                             FROM CajaUsuario 
                             WHERE id_usuario = @id_usuario AND estado = 1";

            using (SqlConnection con = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            sesion = new CajaUsuario
                            {
                                // El DataReader lee snake_case y lo mapeamos al PascalCase de la Entidad
                                IdCajaUsuario = Convert.ToInt32(reader["id_caja_usuario"]),
                                IdCaja = Convert.ToInt32(reader["id_caja"]),
                                IdUsuario = Convert.ToInt32(reader["id_usuario"]),
                                FechaApertura = Convert.ToDateTime(reader["fecha_apertura"]),
                                MontoInicial = Convert.ToDecimal(reader["monto_inicial"]),
                                Estado = Convert.ToBoolean(reader["estado"])
                            };
                        }
                    }
                }
            }
            return sesion;
        }

        public CajaUsuario ObtenerSesionAbiertaPorId(int idCajaUsuario)
        {
            CajaUsuario sesion = null;
            const string query = @"SELECT id_caja_usuario, id_caja, id_usuario, fecha_apertura, monto_inicial, estado
                                   FROM CajaUsuario
                                   WHERE id_caja_usuario = @id_caja_usuario AND estado = 1";

            using (SqlConnection con = new SqlConnection(_cadenaConexion))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@id_caja_usuario", idCajaUsuario);
                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        sesion = new CajaUsuario
                        {
                            IdCajaUsuario = Convert.ToInt32(reader["id_caja_usuario"]),
                            IdCaja = Convert.ToInt32(reader["id_caja"]),
                            IdUsuario = Convert.ToInt32(reader["id_usuario"]),
                            FechaApertura = Convert.ToDateTime(reader["fecha_apertura"]),
                            MontoInicial = Convert.ToDecimal(reader["monto_inicial"]),
                            Estado = Convert.ToBoolean(reader["estado"])
                        };
                    }
                }
            }

            return sesion;
        }

        public CajaUsuario ObtenerSesionAbiertaPorCaja(int idCaja)
        {
            CajaUsuario sesion = null;
            string query = @"SELECT id_caja_usuario, id_caja, id_usuario, fecha_apertura, monto_inicial, estado
                             FROM CajaUsuario
                             WHERE id_caja = @id_caja AND estado = 1";

            using (SqlConnection con = new SqlConnection(_cadenaConexion))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@id_caja", idCaja);
                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        sesion = new CajaUsuario
                        {
                            IdCajaUsuario = Convert.ToInt32(reader["id_caja_usuario"]),
                            IdCaja = Convert.ToInt32(reader["id_caja"]),
                            IdUsuario = Convert.ToInt32(reader["id_usuario"]),
                            FechaApertura = Convert.ToDateTime(reader["fecha_apertura"]),
                            MontoInicial = Convert.ToDecimal(reader["monto_inicial"]),
                            Estado = Convert.ToBoolean(reader["estado"])
                        };
                    }
                }
            }

            return sesion;
        }

        public decimal? ObtenerSaldoEsperadoApertura(int idCaja)
        {
            string query = @"SELECT TOP (1)
                                cierre.monto_declarado + ISNULL((
                                    SELECT SUM(CASE
                                        WHEN movimiento.tipo = 'INGRESO' THEN movimiento.monto
                                        WHEN movimiento.tipo = 'EGRESO' THEN -movimiento.monto
                                        ELSE 0
                                    END)
                                    FROM MovimientosCaja movimiento
                                    WHERE movimiento.id_caja = cierre.id_caja
                                      AND movimiento.fecha_hora > cierre.fecha_cierre
                                      AND movimiento.fecha_hora <= GETDATE()
                                ), 0)
                             FROM CajaUsuario cierre
                             WHERE cierre.id_caja = @id_caja
                               AND cierre.estado = 0
                               AND cierre.fecha_cierre IS NOT NULL
                               AND cierre.monto_declarado IS NOT NULL
                             ORDER BY cierre.fecha_cierre DESC, cierre.id_caja_usuario DESC";

            using (SqlConnection con = new SqlConnection(_cadenaConexion))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@id_caja", idCaja);
                con.Open();

                object resultado = cmd.ExecuteScalar();
                return resultado == null || resultado == DBNull.Value
                    ? null
                    : Convert.ToDecimal(resultado);
            }
        }

        public bool VerificarCajaFisicaEnUso(int idCaja)
        {
            bool enUso = false;
            string query = @"SELECT COUNT(1) FROM CajaUsuario 
                             WHERE id_caja = @id_caja AND estado = 1";

            using (SqlConnection con = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id_caja", idCaja);
                    con.Open();

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    enUso = count > 0;
                }
            }
            return enUso;
        }
    }
}