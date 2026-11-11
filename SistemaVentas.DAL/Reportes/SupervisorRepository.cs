using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Data.SqlClient;
using SistemaVentas.Entities.Reportes;

namespace SistemaVentas.DAL.Reportes
{
    /// <summary>
    /// Clase de acceso a datos (DAL) para los reportes de Supervisor.
    /// Corregido según hallazgos de auditoría (filtro de canceladas por estado y manejo de excepciones).
    /// </summary>
    public class SupervisorRepository
    {
        private readonly string _cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

        public SupervisorRepository()
        {
        }

        public SupervisorRepository(string cadenaConexion)
        {
            if (!string.IsNullOrEmpty(cadenaConexion))
            {
                _cadenaConexion = cadenaConexion;
            }
        }

        // 1. Stock Crítico
        public List<StockCritico> ObtenerStockCritico()
        {
            var lista = new List<StockCritico>();

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();
                string consulta = @"
                    SELECT nombre, codigo_barras, stock_actual, stock_minimo
                    FROM dbo.Producto
                    WHERE stock_actual <= stock_minimo AND activo = 1
                    ORDER BY stock_actual ASC;";

                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            lista.Add(new StockCritico
                            {
                                Nombre = lector["nombre"].ToString(),
                                CodigoBarras = lector["codigo_barras"].ToString(),
                                StockActual = Convert.ToInt32(lector["stock_actual"]),
                                StockMinimo = Convert.ToInt32(lector["stock_minimo"])
                            });
                        }
                    }
                }
            }

            return lista;
        }

        // 2. Ventas Canceladas del Día (Corregido: Filtro por estado real en lugar de total <= 0)
        public List<VentaCancelada> ObtenerVentasCanceladas(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var lista = new List<VentaCancelada>();

            string consulta = @"
                SELECT 
                    v.id_venta,
                    v.fecha_hora,
                    v.total,
                    (RTRIM(ISNULL(u.nombre, '')) + ' ' + ISNULL(u.apellido, '')) AS vendedor
                FROM dbo.Venta v
                INNER JOIN dbo.CajaUsuario cu ON v.id_caja_usuario = cu.id_caja_usuario
                INNER JOIN dbo.Usuario u ON cu.id_usuario = u.id_usuario
                WHERE UPPER(LTRIM(RTRIM(v.estado))) = 'CANCELADA'";

            if (fechaInicio.HasValue)
            {
                consulta += " AND v.fecha_hora >= @fechaInicio";
            }
            if (fechaFin.HasValue)
            {
                consulta += " AND v.fecha_hora <= @fechaFin";
            }

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    if (fechaInicio.HasValue)
                        comando.Parameters.AddWithValue("@fechaInicio", fechaInicio.Value);

                    if (fechaFin.HasValue)
                        comando.Parameters.AddWithValue("@fechaFin", fechaFin.Value);

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            lista.Add(new VentaCancelada()
                            {
                                IdVenta = Convert.ToInt32(lector["id_venta"]),
                                FechaHora = Convert.ToDateTime(lector["fecha_hora"]),
                                Total = Convert.ToDecimal(lector["total"]),
                                Vendedor = lector["vendedor"].ToString().Trim()
                            });
                        }
                    }
                }
            }

            return lista;
        }

        // 3. Ventas por Vendedor (Propagación de errores limpia)
        public List<VentaVendedor> ObtenerVentasPorVendedor(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var lista = new List<VentaVendedor>();

            string consulta = @"
                SELECT 
                    (RTRIM(ISNULL(u.nombre, '')) + ' ' + ISNULL(u.apellido, '')) AS nombre_completo,
                    u.correo,
                    r.nombre AS nombre_rol,
                    COUNT(v.id_venta) AS total_ventas,
                    ISNULL(SUM(v.total), 0) AS total_monto,
                    MAX(v.fecha_hora) AS ultima_venta
                FROM dbo.Usuario u
                INNER JOIN dbo.Rol r ON u.id_rol = r.id_rol
                LEFT JOIN dbo.CajaUsuario cu ON u.id_usuario = cu.id_usuario
                LEFT JOIN dbo.Venta v ON cu.id_caja_usuario = v.id_caja_usuario
                WHERE r.nombre = 'Vendedor'";

            if (fechaInicio.HasValue)
            {
                consulta += " AND v.fecha_hora >= @fechaInicio";
            }
            if (fechaFin.HasValue)
            {
                consulta += " AND v.fecha_hora <= @fechaFin";
            }

            consulta += @" 
                GROUP BY u.nombre, u.apellido, u.correo, r.nombre
                ORDER BY total_monto DESC";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    if (fechaInicio.HasValue)
                        comando.Parameters.AddWithValue("@fechaInicio", fechaInicio.Value);

                    if (fechaFin.HasValue)
                        comando.Parameters.AddWithValue("@fechaFin", fechaFin.Value);

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            lista.Add(new VentaVendedor()
                            {
                                NombreCompleto = lector["nombre_completo"].ToString().Trim(),
                                Correo = lector["correo"].ToString(),
                                NombreRol = lector["nombre_rol"].ToString(),
                                TotalVentas = Convert.ToInt32(lector["total_ventas"]),
                                TotalMonto = Convert.ToDecimal(lector["total_monto"]),
                                UltimaVenta = lector["ultima_venta"] != DBNull.Value
                                    ? Convert.ToDateTime(lector["ultima_venta"])
                                    : (DateTime?)null
                            });
                        }
                    }
                }
            }

            return lista;
        }
    }
}