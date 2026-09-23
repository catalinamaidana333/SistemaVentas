using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using SistemaVentas.Entities.Reportes;

namespace SistemaVentas.DAL.Reportes
{
    public class GerenteRepository
    {
        private readonly string _cadenaConexion;

        public GerenteRepository()
        {
            _cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;
        }

        // 1. Rentabilidad
        public RentabilidadResumen ObtenerRentabilidad()
        {
            var resumen = new RentabilidadResumen();
            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    string consulta = @"
                        SELECT 
                            (SELECT ISNULL(SUM(total), 0) FROM dbo.Venta) AS TotalVentas,
                            (SELECT ISNULL(SUM(total), 0) FROM dbo.Compra) AS TotalCompras;";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            resumen.TotalVentas = Convert.ToDecimal(lector["TotalVentas"]);
                            resumen.TotalCompras = Convert.ToDecimal(lector["TotalCompras"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener rentabilidad: {ex.Message}", ex);
            }
            return resumen;
        }

        // 2. Auditoría
        public List<UsuarioAuditoria> ObtenerAuditoriaUsuarios()
        {
            var lista = new List<UsuarioAuditoria>();

            string consulta = @"
        SELECT 
            (u.nombre + ' ' + u.apellido) AS nombre_completo,
            u.correo,
            r.nombre AS nombre_rol,
            COUNT(v.id_venta) AS total_ventas,
            ISNULL(SUM(v.total), 0) AS total_monto,
            MAX(v.fecha_hora) AS ultima_venta
        FROM Usuario u
        INNER JOIN Rol r ON u.id_rol = r.id_rol
        LEFT JOIN CajaUsuario cu ON u.id_usuario = cu.id_usuario
        LEFT JOIN Venta v ON cu.id_caja_usuario = v.id_caja_usuario
        GROUP BY (u.nombre + ' ' + u.apellido), u.correo, r.nombre";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            lista.Add(new UsuarioAuditoria
                            {
                                NombreCompleto = lector["nombre_completo"].ToString(),
                                Correo = lector["correo"].ToString(),
                                NombreRol = lector["nombre_rol"].ToString(),
                                TotalVentas = Convert.ToInt32(lector["total_ventas"]),
                                TotalMontoVendido = Convert.ToDecimal(lector["total_monto"]),
                                FechaUltimaVenta = lector["ultima_venta"] != DBNull.Value
                                    ? Convert.ToDateTime(lector["ultima_venta"])
                                    : (DateTime?)null
                            });
                        }
                    }
                }
            }

            return lista;
        }

        // 3. Rendimiento Mensual
        public List<RendimientoMensual> ObtenerRendimientoMensual()
        {
            var lista = new List<RendimientoMensual>();
            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    string consulta = @"
                        SELECT 
                            YEAR(fecha_hora) AS Anio,
                            MONTH(fecha_hora) AS Mes,
                            COUNT(id_venta) AS TotalTransacciones,
                            ISNULL(SUM(total), 0) AS TotalMonto
                        FROM dbo.Venta
                        GROUP BY YEAR(fecha_hora), MONTH(fecha_hora)
                        ORDER BY Anio DESC, Mes DESC;";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            lista.Add(new RendimientoMensual
                            {
                                Anio = Convert.ToInt32(lector["Anio"]),
                                Mes = Convert.ToInt32(lector["Mes"]),
                                TotalTransacciones = Convert.ToInt32(lector["TotalTransacciones"]),
                                TotalMonto = Convert.ToDecimal(lector["TotalMonto"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener rendimiento mensual: {ex.Message}", ex);
            }
            return lista;
        }
    }
}
