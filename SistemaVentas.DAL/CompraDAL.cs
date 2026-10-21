using SistemaVentas.Entities;
using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SistemaVentas.DAL
{
    public class CompraDAL
    {
        private readonly string _cadenaConexion;

        public CompraDAL()
        {
            _cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;
        }

        public decimal ObtenerTotalComprasEfectivoPorSesion(int idCajaUsuario)
        {
            const string consulta = @"SELECT ISNULL(SUM(total), 0)
                                      FROM dbo.Compra
                                      WHERE id_caja_usuario = @id_caja_usuario
                                        AND metodo_pago = @metodo_pago;";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            using (SqlCommand comando = new SqlCommand(consulta, conexion))
            {
                comando.Parameters.AddWithValue("@id_caja_usuario", idCajaUsuario);
                comando.Parameters.AddWithValue("@metodo_pago", "Efectivo");
                conexion.Open();
                return Convert.ToDecimal(comando.ExecuteScalar());
            }
        }

        public int Insertar(Compra compra)
        {
            // Solo validamos nulidad extrema del objeto, el resto de validaciones se borran porque ya las hizo la BLL.
            if (compra == null)
                throw new ArgumentNullException(nameof(compra));

            // La sesión de caja es opcional: si no hay, id_caja_usuario va como NULL (no como 0, rompería la FK).
            // Consulta actualizada con los nuevos campos de tu migración
            const string consultaCompra = @"INSERT INTO dbo.Compra
                                    (id_usuario, id_caja_usuario, id_proveedor, fecha_hora, metodo_pago, total, numero_factura, observaciones, origen_efectivo)
                                    OUTPUT INSERTED.id_compra
                                    VALUES (@id_usuario, @id_caja_usuario, @id_proveedor, GETDATE(), @metodo_pago, @total, @numero_factura, @observaciones, @origen_efectivo);";

            const string consultaDetalle = @"INSERT INTO dbo.DetalleCompra
                                     (id_compra, id_producto, cantidad, costo_unitario_hist, subtotal)
                                     VALUES (@id_compra, @id_producto, @cantidad, @costo_unitario, @subtotal);";

            // La compra suma existencias y actualiza el costo del producto con el último costo de compra.
            const string consultaActualizaProducto = @"UPDATE dbo.Producto
                                            SET stock_actual = stock_actual + @cantidad,
                                                precio_costo = @costo
                                            WHERE id_producto = @id_producto;";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        int idCompra;
                        using (SqlCommand comando = new SqlCommand(consultaCompra, conexion, transaccion))
                        {
                            comando.Parameters.AddWithValue("@id_usuario", compra.IdUsuario);

                            // La sesión de caja es opcional, se bindea con tipo explícito para poder enviar NULL.
                            comando.Parameters.Add("@id_caja_usuario", SqlDbType.Int).Value = compra.IdCajaUsuario ?? (object)DBNull.Value;

                            comando.Parameters.AddWithValue("@id_proveedor", compra.IdProveedor);
                            comando.Parameters.AddWithValue("@metodo_pago", compra.MetodoPago);
                            comando.Parameters.AddWithValue("@total", compra.Total);

                            // Mapeo de los nuevos campos, gestionando strings nulos/vacíos a DBNull
                            comando.Parameters.AddWithValue("@numero_factura", string.IsNullOrWhiteSpace(compra.NumeroFactura) ? (object)DBNull.Value : compra.NumeroFactura);
                            comando.Parameters.AddWithValue("@observaciones", string.IsNullOrWhiteSpace(compra.Observaciones) ? (object)DBNull.Value : compra.Observaciones);
                            comando.Parameters.AddWithValue("@origen_efectivo", string.IsNullOrWhiteSpace(compra.OrigenEfectivo) ? (object)DBNull.Value : compra.OrigenEfectivo);

                            idCompra = Convert.ToInt32(comando.ExecuteScalar());
                        }

                        foreach (DetalleCompra detalle in compra.Detalles)
                        {
                            using SqlCommand comandoDetalle = new SqlCommand(consultaDetalle, conexion, transaccion);
                            comandoDetalle.Parameters.AddWithValue("@id_compra", idCompra);
                            comandoDetalle.Parameters.AddWithValue("@id_producto", detalle.IdProducto);
                            comandoDetalle.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                            comandoDetalle.Parameters.AddWithValue("@costo_unitario", detalle.CostoUnitario);
                            comandoDetalle.Parameters.AddWithValue("@subtotal", detalle.Subtotal);
                            comandoDetalle.ExecuteNonQuery();

                            // Actualización del producto dentro de la misma transacción de la compra
                            using SqlCommand comandoProducto = new SqlCommand(consultaActualizaProducto, conexion, transaccion);
                            comandoProducto.Parameters.AddWithValue("@id_producto", detalle.IdProducto);
                            comandoProducto.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                            comandoProducto.Parameters.AddWithValue("@costo", detalle.CostoUnitario);
                            comandoProducto.ExecuteNonQuery();
                        }

                        transaccion.Commit();
                        return idCompra;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Lista el historial de compras con los datos del proveedor y del usuario que la realizó.
        /// </summary>
        public List<Compra> Listar()
        {
            List<Compra> lista = new List<Compra>();

            const string consulta = @"SELECT  c.id_compra,
                                             c.id_usuario,
                                             c.id_caja_usuario,
                                             c.id_proveedor,
                                             c.fecha_hora,
                                             c.metodo_pago,
                                             c.total,
                                             c.numero_factura,
                                             c.observaciones,
                                             c.origen_efectivo,
                                             pr.razon_social AS razon_social_proveedor,
                                             ISNULL(NULLIF(LTRIM(RTRIM(CONCAT(u.nombre, ' ', u.apellido))), ''), ISNULL(u.nombre_usuario, '')) AS nombre_usuario
                                      FROM    dbo.Compra c
                                      INNER JOIN dbo.Proveedor pr ON pr.id_proveedor = c.id_proveedor
                                      LEFT JOIN dbo.Usuario u ON u.id_usuario = c.id_usuario
                                      ORDER BY c.fecha_hora DESC, c.id_compra DESC;";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            using (SqlCommand comando = new SqlCommand(consulta, conexion))
            {
                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        lista.Add(MapearCompra(lector));
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Obtiene una compra por su ID, con el detalle de productos y los datos del proveedor.
        /// Devuelve null si la compra no existe.
        /// </summary>
        public Compra? ObtenerPorId(int idCompra)
        {
            if (idCompra <= 0)
                return null;

            const string consultaCompra = @"SELECT  c.id_compra,
                                                  c.id_usuario,
                                                  c.id_caja_usuario,
                                                  c.id_proveedor,
                                                  c.fecha_hora,
                                                  c.metodo_pago,
                                                  c.total,
                                                  c.numero_factura,
                                                  c.observaciones,
                                                  c.origen_efectivo,
                                                  pr.razon_social AS razon_social_proveedor,
                                             ISNULL(NULLIF(LTRIM(RTRIM(CONCAT(u.nombre, ' ', u.apellido))), ''), ISNULL(u.nombre_usuario, '')) AS nombre_usuario
                                           FROM    dbo.Compra c
                                           INNER JOIN dbo.Proveedor pr ON pr.id_proveedor = c.id_proveedor
                                           LEFT JOIN dbo.Usuario u ON u.id_usuario = c.id_usuario
                                           WHERE c.id_compra = @id_compra;";

            const string consultaDetalle = @"SELECT  dc.id_detalle_compra,
                                                     dc.id_compra,
                                                     dc.id_producto,
                                                     dc.cantidad,
                                                     dc.costo_unitario_hist,
                                                     dc.subtotal,
                                                     p.nombre AS nombre_producto,
                                                     p.codigo_barras
                                              FROM    dbo.DetalleCompra dc
                                              INNER JOIN dbo.Producto p ON p.id_producto = dc.id_producto
                                              WHERE dc.id_compra = @id_compra
                                              ORDER BY dc.id_detalle_compra;";

            Compra? compra = null;

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();

                using (SqlCommand comando = new SqlCommand(consultaCompra, conexion))
                {
                    comando.Parameters.AddWithValue("@id_compra", idCompra);

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            compra = MapearCompra(lector);
                        }
                    }
                }

                if (compra == null)
                    return null;

                using (SqlCommand comando = new SqlCommand(consultaDetalle, conexion))
                {
                    comando.Parameters.AddWithValue("@id_compra", idCompra);

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            compra.Detalles.Add(new DetalleCompra()
                            {
                                IdDetalleCompra = Convert.ToInt32(lector["id_detalle_compra"]),
                                IdCompra = Convert.ToInt32(lector["id_compra"]),
                                IdProducto = Convert.ToInt32(lector["id_producto"]),
                                Cantidad = Convert.ToInt32(lector["cantidad"]),
                                CostoUnitario = Convert.ToDecimal(lector["costo_unitario_hist"]),
                                NombreProducto = lector["nombre_producto"] != DBNull.Value ? lector["nombre_producto"].ToString() : string.Empty,
                                CodigoBarras = lector["codigo_barras"] != DBNull.Value ? lector["codigo_barras"].ToString() : string.Empty
                            });
                        }
                    }
                }
            }

            return compra;
        }

        /// <summary>
        /// Mapea la fila actual del lector a una entidad Compra.
        /// </summary>
        private static Compra MapearCompra(SqlDataReader lector)
        {
            return new Compra()
            {
                IdCompra = Convert.ToInt32(lector["id_compra"]),
                IdUsuario = Convert.ToInt32(lector["id_usuario"]),
                IdCajaUsuario = lector["id_caja_usuario"] != DBNull.Value ? Convert.ToInt32(lector["id_caja_usuario"]) : null,
                IdProveedor = Convert.ToInt32(lector["id_proveedor"]),
                FechaHora = Convert.ToDateTime(lector["fecha_hora"]),
                MetodoPago = lector["metodo_pago"].ToString(),
                Total = Convert.ToDecimal(lector["total"]),
                NumeroFactura = lector["numero_factura"] != DBNull.Value ? lector["numero_factura"].ToString() : string.Empty,
                Observaciones = lector["observaciones"] != DBNull.Value ? lector["observaciones"].ToString() : string.Empty,
                OrigenEfectivo = lector["origen_efectivo"] != DBNull.Value ? lector["origen_efectivo"].ToString() : string.Empty,
                RazonSocialProveedor = lector["razon_social_proveedor"] != DBNull.Value ? lector["razon_social_proveedor"].ToString() : string.Empty,
                NombreUsuario = lector["nombre_usuario"] != DBNull.Value ? lector["nombre_usuario"].ToString() : string.Empty
            };
        }
    }
}

