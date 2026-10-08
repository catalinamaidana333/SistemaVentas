using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using SistemaVentas.Entities.Reportes;

namespace SistemaVentas.DAL.Reportes
{
    /// <summary>
    /// Clase de acceso a datos (DAL) para el reporte de Arqueo Diario.
    /// Utiliza SQL Server y la conexión definida en App.config.
    /// </summary>
    public class ArqueoRepository
    {
        private string _cadenaConexion;

        public ArqueoRepository()
        {
            // segun App.config
            _cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;
        }

        /// <summary>
        /// Obtiene el resumen de arqueo de una sesión de caja específica
        /// </summary>
        public ArqueoResumen ObtenerResumenPorCaja(int idUsuario)
        {
            ArqueoResumen resumen = null;

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();
                string consulta = @"
            SELECT TOP 1
                cu.fecha_apertura,
                cu.fecha_cierre,
                cu.monto_inicial,
                ISNULL(SUM(CASE WHEN v.metodo_pago = 'EFECTIVO' THEN v.total ELSE 0 END), 0) AS total_efectivo,
                ISNULL(SUM(CASE WHEN v.metodo_pago = 'MERCADOPAGO' THEN v.total ELSE 0 END), 0) AS total_mp
            FROM dbo.CajaUsuario cu
            LEFT JOIN dbo.Venta v ON cu.id_caja_usuario = v.id_caja_usuario
            WHERE cu.id_usuario = @idUsuario
            GROUP BY cu.id_caja_usuario, cu.fecha_apertura, cu.fecha_cierre, cu.monto_inicial
            ORDER BY cu.id_caja_usuario DESC;";

                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@idUsuario", idUsuario);
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            decimal inicial = Convert.ToDecimal(lector["monto_inicial"]);
                            decimal efec = Convert.ToDecimal(lector["total_efectivo"]);
                            decimal mp = Convert.ToDecimal(lector["total_mp"]);

                            resumen = new ArqueoResumen
                            {
                                FechaApertura = Convert.ToDateTime(lector["fecha_apertura"]),
                                FechaCierre = lector["fecha_cierre"] != DBNull.Value ? Convert.ToDateTime(lector["fecha_cierre"]) : (DateTime?)null,
                                MontoInicial = inicial,
                                TotalVentasEfectivo = efec,
                                TotalVentasMp = mp,
                                TotalComprasEfectivo = 0,
                                MontoSistema = inicial + efec
                            };
                        }
                    }
                }
            }
            return resumen;
        }
    }
}