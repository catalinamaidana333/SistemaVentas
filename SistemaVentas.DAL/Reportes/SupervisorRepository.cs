using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Data.SqlClient;
using SistemaVentas.Entities.Reportes;

namespace SistemaVentas.DAL.Reportes
{
    public class SupervisorRepository
    {
        // Asignación directa al declarar el campo
        private readonly string _cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

        public SupervisorRepository()
        {
        }

        // Si tenés otro constructor con parámetros, dejalo así:
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

            try
            {
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
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener stock crítico: {ex.Message}", ex);
            }

            return lista;
        }

        // 2. Ventas Canceladas del Día
        public List<VentaCancelada> ObtenerVentasCanceladas()
        {
            var lista = new List<VentaCancelada>();
            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    string consulta = @"
                        SELECT id_venta, fecha_hora, metodo_pago, total
                        FROM dbo.Venta
                        WHERE CAST(fecha_hora AS DATE) = CAST(GETDATE() AS DATE)
                          AND total <= 0
                        ORDER BY fecha_hora DESC;";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            lista.Add(new VentaCancelada
                            {
                                IdVenta = Convert.ToInt32(lector["id_venta"]),
                                FechaHora = Convert.ToDateTime(lector["fecha_hora"]),
                                MetodoPago = lector["metodo_pago"].ToString(),
                                Total = Convert.ToDecimal(lector["total"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener ventas canceladas: {ex.Message}", ex);
            }
            return lista;
        }

        // 3. Ventas por Vendedor
        public List<VentaVendedor> ObtenerVentasPorVendedor()
        {
            var lista = new List<VentaVendedor>();

            try
            {
                string consulta = @"
                    SELECT 
                        u.nombre_completo AS NombreVendedor,
                        COUNT(v.id_venta) AS CantidadVentas
                    FROM Usuario u
                    INNER JOIN Rol r ON u.id_rol = r.id_rol
                    LEFT JOIN CajaUsuario cu ON u.id_usuario = cu.id_usuario
                    LEFT JOIN Venta v ON cu.id_caja_usuario = v.id_caja_usuario
                    WHERE r.nombre = 'Vendedor'
                    GROUP BY u.nombre_completo";

                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                lista.Add(new VentaVendedor
                                {
                                    NombreVendedor = lector["NombreVendedor"].ToString(),
                                    CantidadVentas = Convert.ToInt32(lector["CantidadVentas"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener ventas por vendedor: {ex.Message}", ex);
            }

            return lista;
        }
    }
}