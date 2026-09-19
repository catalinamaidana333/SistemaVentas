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
        public ArqueoResumen ObtenerResumenPorCajaUsuario(int idCajaUsuario)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();

                    string consulta = @"SELECT fecha_apertura, fecha_cierre, monto_inicial, 
                                       total_ventas_efectivo, total_ventas_mp, total_compras_efectivo, 
                                       monto_sys, monto_declarado, diferencia, estado
                                       FROM CajaUsuario
                                       WHERE id_caja_usuario = @idCajaUsuario";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@idCajaUsuario", idCajaUsuario);

                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            if (lector.Read())
                            {
                                return new ArqueoResumen
                                {
                                    FechaApertura = Convert.ToDateTime(lector["fecha_apertura"]),
                                    FechaCierre = lector["fecha_cierre"] != DBNull.Value ? Convert.ToDateTime(lector["fecha_cierre"]) : (DateTime?)null,
                                    MontoInicial = Convert.ToDecimal(lector["monto_inicial"]),
                                    TotalVentasEfectivo = Convert.ToDecimal(lector["total_ventas_efectivo"]),
                                    TotalVentasMp = Convert.ToDecimal(lector["total_ventas_mp"]),
                                    TotalComprasEfectivo = Convert.ToDecimal(lector["total_compras_efectivo"]),
                                    MontoSistema = lector["monto_sys"] != DBNull.Value ? Convert.ToDecimal(lector["monto_sys"]) : 0,
                                    MontoDeclarado = lector["monto_declarado"] != DBNull.Value ? Convert.ToDecimal(lector["monto_declarado"]) : (decimal?)null,
                                    Diferencia = lector["diferencia"] != DBNull.Value ? Convert.ToDecimal(lector["diferencia"]) : (decimal?)null,
                                    Estado = lector["estado"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el resumen de arqueo: {ex.Message}", ex);
            }

            return null;
        }
    }
}