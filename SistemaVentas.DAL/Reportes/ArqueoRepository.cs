using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using SistemaVentas.Entities.Reportes;

namespace SistemaVentas.DAL.Reportes
{
    /// <summary>
    /// Clase de acceso a datos (DAL) para el reporte de Arqueo Diario.
    /// Corregido según los hallazgos de auditoría (filtrado por sesión, formatos de pago y completitud de datos).
    /// </summary>
    public class ArqueoRepository
    {
        private string _cadenaConexion;

        public ArqueoRepository()
        {
            _cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;
        }

        /// <summary>
        /// Obtiene el resumen de arqueo filtrando estrictamente por la sesión de caja (id_caja_usuario)
        /// y completando los valores de ventas, compras, montos declarados y estado.
        /// </summary>
        public ArqueoResumen ObtenerResumenPorCaja(int idCajaUsuario)
        {
            ArqueoResumen resumen = null;

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();

                // Consulta optimizada que rescata datos de la caja, ventas agrupadas por tipo normalizado,
                // compras de la sesión y los campos de cierre (declarado, diferencia, estado).
                string consulta = @"
                    SELECT 
                        cu.fecha_apertura,
                        cu.fecha_cierre,
                        cu.monto_inicial,
                        ISNULL(cu.monto_declarado, 0) AS monto_declarado,
                        ISNULL(cu.diferencia, 0) AS diferencia,
                        ISNULL(cu.estado, 'Abierta') AS estado,
                        
                        -- Ventas en efectivo (normalizando formato y mayúsculas/minúsculas)
                        ISNULL((
                            SELECT SUM(v.total) 
                            FROM dbo.Venta v 
                            WHERE v.id_caja_usuario = cu.id_caja_usuario 
                              AND UPPER(LTRIM(RTRIM(v.metodo_pago))) IN ('EFECTIVO', 'CASH')
                        ), 0) AS total_efectivo,

                        -- Ventas en Mercado Pago (normalizando variaciones de espacio o nombres)
                        ISNULL((
                            SELECT SUM(v.total) 
                            FROM dbo.Venta v 
                            WHERE v.id_caja_usuario = cu.id_caja_usuario 
                              AND UPPER(LTRIM(RTRIM(v.metodo_pago))) IN ('MERCADOPAGO', 'MERCADO PAGO', 'MP')
                        ), 0) AS total_mp,

                        -- Compras en efectivo asociadas a la caja o periodo de la sesión
                        ISNULL((
                            SELECT SUM(c.total) 
                            FROM dbo.Compra c 
                            WHERE c.id_caja_usuario = cu.id_caja_usuario 
                              AND UPPER(LTRIM(RTRIM(c.metodo_pago))) IN ('EFECTIVO', 'CASH')
                        ), 0) AS total_compras_efectivo

                    FROM dbo.CajaUsuario cu
                    WHERE cu.id_caja_usuario = @idCajaUsuario;";

                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    // Corrección: Filtro por ID de Sesión de Caja exacto y no por ID de usuario general
                    comando.Parameters.AddWithValue("@idCajaUsuario", idCajaUsuario);

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            decimal inicial = Convert.ToDecimal(lector["monto_inicial"]);
                            decimal efec = Convert.ToDecimal(lector["total_efectivo"]);
                            decimal mp = Convert.ToDecimal(lector["total_mp"]);
                            decimal comprasEfec = Convert.ToDecimal(lector["total_compras_efectivo"]);

                            resumen = new ArqueoResumen
                            {
                                FechaApertura = Convert.ToDateTime(lector["fecha_apertura"]),
                                FechaCierre = lector["fecha_cierre"] != DBNull.Value ? Convert.ToDateTime(lector["fecha_cierre"]) : (DateTime?)null,
                                MontoInicial = inicial,
                                TotalVentasEfectivo = efec,
                                TotalVentasMp = mp,
                                TotalComprasEfectivo = comprasEfec, // Ya no devuelve cero fijo
                                MontoSistema = (inicial + efec) - comprasEfec,
                                MontoDeclarado = Convert.ToDecimal(lector["monto_declarado"]),
                                Diferencia = Convert.ToDecimal(lector["diferencia"]),
                                Estado = lector["estado"].ToString()
                            };
                        }
                    }
                }
            }
            return resumen;
        }
    }
}