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
            // Estado = 0 (False/Cerrada)
            string query = @"UPDATE CajaUsuario 
                             SET fecha_cierre = GETDATE(), 
                                 monto_cierre = @monto_cierre,
                                 monto_total_ventas_efectivo = @monto_total_ventas_efectivo,
                                 monto_total_ventas_mp = @monto_total_ventas_mp,
                                 monto_total_gastos_efec = @monto_total_gastos_efec,
                                 monto_sistema = @monto_sistema, 
                                 diferencia = @diferencia, 
                                 estado = 0 
                             WHERE id_caja_usuario = @id_caja_usuario";

            using (SqlConnection con = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@monto_cierre", cajaUsuario.MontoCierre);
                    cmd.Parameters.AddWithValue("@monto_total_ventas_efectivo", cajaUsuario.MontoTotalVentasEfectivo);
                    cmd.Parameters.AddWithValue("@monto_total_ventas_mp", cajaUsuario.MontoTotalVentasMp);
                    cmd.Parameters.AddWithValue("@monto_total_gastos_efec", cajaUsuario.MontoTotalGastosEfec);
                    cmd.Parameters.AddWithValue("@monto_sistema", cajaUsuario.MontoSistema);
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