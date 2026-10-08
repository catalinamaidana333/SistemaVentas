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
        public RentabilidadResumen ObtenerRentabilidad(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var resultado = new RentabilidadResumen();

            try
            {
                string consulta = @"
            SELECT 
                ISNULL(SUM(dv.precio_venta * dv.cantidad), 0) AS total_ingresos,
                ISNULL(SUM(dv.precio_costo * dv.cantidad), 0) AS total_costos
            FROM DetalleVenta dv
            INNER JOIN Venta v ON dv.id_venta = v.id_venta
            WHERE 1=1";

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
                            if (lector.Read())
                            {
                                // Asignamos a las propiedades de RentabilidadResumen
                                resultado.TotalVentas = Convert.ToDecimal(lector["total_ingresos"]);
                                resultado.TotalCostos = Convert.ToDecimal(lector["total_costos"]);
                                // GananciaNeta no se asigna porque ya se calcula sola mediante '=> TotalVentas - TotalCostos'
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener rentabilidad: {ex.Message}");
            }

            return resultado;
        }

        // 2. Auditoría
        public List<UsuarioAuditoria> ObtenerAuditoriaUsuarios(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var lista = new List<UsuarioAuditoria>();

            try
            {
                string consulta = @"
            SELECT 
                (RTRIM(ISNULL(u.nombre, '')) + ' ' + ISNULL(u.apellido, '')) AS nombre_completo,
                u.correo,
                r.nombre AS nombre_rol,
                COUNT(v.id_venta) AS total_ventas,
                ISNULL(SUM(v.total), 0) AS total_monto_vendido,
                MAX(v.fecha_hora) AS fecha_ultima_venta
            FROM dbo.Usuario u
            INNER JOIN dbo.Rol r ON u.id_rol = r.id_rol
            LEFT JOIN dbo.CajaUsuario cu ON u.id_usuario = cu.id_usuario
            LEFT JOIN dbo.Venta v ON cu.id_caja_usuario = v.id_caja_usuario
            WHERE 1=1";

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
            ORDER BY total_ventas DESC";

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
                                lista.Add(new UsuarioAuditoria()
                                {
                                    NombreCompleto = lector["nombre_completo"].ToString().Trim(),
                                    Correo = lector["correo"].ToString(),
                                    NombreRol = lector["nombre_rol"].ToString(),
                                    TotalVentas = Convert.ToInt32(lector["total_ventas"]),
                                    TotalMontoVendido = Convert.ToDecimal(lector["total_monto_vendido"]),
                                    FechaUltimaVenta = lector["fecha_ultima_venta"] != DBNull.Value
                                        ? Convert.ToDateTime(lector["fecha_ultima_venta"])
                                        : (DateTime?)null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener auditoría de usuarios: {ex.Message}");
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

                    // 1. Consulta SQL ajustada a los campos que necesitas
                    string consulta = @"
        SELECT 
            YEAR(fecha_hora) AS Anio,
            MONTH(fecha_hora) AS Mes,
            COUNT(id_venta) AS TotalVentas,
            ISNULL(SUM(total), 0) AS TotalMonto,
            ISNULL(AVG(total), 0) AS TicketPromedio
        FROM dbo.Venta
        GROUP BY YEAR(fecha_hora), MONTH(fecha_hora)
        ORDER BY Anio DESC, Mes DESC;";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                // 2. Mapeo directo a la entidad RendimientoMensual
                                lista.Add(new RendimientoMensual()
                                {
                                    Anio = Convert.ToInt32(lector["Anio"]),
                                    Mes = Convert.ToInt32(lector["Mes"]),
                                    TotalVentas = Convert.ToInt32(lector["TotalVentas"]),
                                    TotalMonto = Convert.ToDecimal(lector["TotalMonto"]),
                                    TicketPromedio = Convert.ToDecimal(lector["TicketPromedio"])
                                });
                            }
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
