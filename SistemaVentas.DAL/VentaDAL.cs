using SistemaVentas.Entities;
using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SistemaVentas.DAL
{
    public class VentaDAL
    {
        private readonly string _cadenaConexion;

        public VentaDAL()
        {
            _cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;
        }

        public decimal ObtenerTotalVentasEfectivoPorSesion(int idCajaUsuario)
        {
            return ObtenerTotalPorMetodoPago(idCajaUsuario, "Efectivo");
        }

        public decimal ObtenerTotalVentasMercadoPagoPorSesion(int idCajaUsuario)
        {
            return ObtenerTotalPorMetodoPago(idCajaUsuario, "Mercado Pago");
        }

        public int Insertar(Venta venta)
        {
            if (venta == null)
                throw new ArgumentNullException(nameof(venta));
            if (venta.IdCajaUsuario <= 0)
                throw new ArgumentException("La venta debe pertenecer a una sesión de caja válida.", nameof(venta));

            // 1. Agregamos monto_abonado y vuelto al INSERT y a los VALUES
            const string consultaVenta = @"INSERT INTO dbo.Venta 
                                   (id_caja_usuario, fecha_hora, metodo_pago, total, monto_abonado, vuelto)
                                   OUTPUT INSERTED.id_venta
                                   VALUES (@id_caja_usuario, GETDATE(), @metodo_pago, @total, @monto_abonado, @vuelto);";

            const string consultaDetalle = @"INSERT INTO dbo.DetalleVenta
                                     (id_venta, id_producto, cantidad, precio_unitario_historico, subtotal)
                                     VALUES (@id_venta, @id_producto, @cantidad, @precio_unitario, @subtotal);";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        int idVenta;
                        using (SqlCommand comando = new SqlCommand(consultaVenta, conexion, transaccion))
                        {
                            comando.Parameters.AddWithValue("@id_caja_usuario", venta.IdCajaUsuario);
                            comando.Parameters.AddWithValue("@metodo_pago", venta.MetodoPago);
                            comando.Parameters.AddWithValue("@total", venta.Total);

                            // 2. Pasamos los valores al comando SQL
                            comando.Parameters.AddWithValue("@monto_abonado", venta.MontoAbonado);
                            comando.Parameters.AddWithValue("@vuelto", venta.Vuelto);

                            idVenta = Convert.ToInt32(comando.ExecuteScalar());
                        }

                        foreach (DetalleVenta detalle in venta.Detalles)
                        {
                            using SqlCommand comandoDetalle = new SqlCommand(consultaDetalle, conexion, transaccion);
                            comandoDetalle.Parameters.AddWithValue("@id_venta", idVenta);
                            comandoDetalle.Parameters.AddWithValue("@id_producto", detalle.IdProducto);
                            comandoDetalle.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                            comandoDetalle.Parameters.AddWithValue("@precio_unitario", detalle.PrecioUnitario);
                            comandoDetalle.Parameters.AddWithValue("@subtotal", detalle.Subtotal);
                            comandoDetalle.ExecuteNonQuery();
                        }

                        // Aquí podrías agregar el UPDATE para descontar el stock si lo necesitas

                        transaccion.Commit();
                        return idVenta;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        private decimal ObtenerTotalPorMetodoPago(int idCajaUsuario, string metodoPago)
        {
            const string consulta = @"SELECT ISNULL(SUM(total), 0)
                                      FROM dbo.Venta
                                      WHERE id_caja_usuario = @id_caja_usuario
                                        AND metodo_pago = @metodo_pago;";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            using (SqlCommand comando = new SqlCommand(consulta, conexion))
            {
                comando.Parameters.AddWithValue("@id_caja_usuario", idCajaUsuario);
                comando.Parameters.AddWithValue("@metodo_pago", metodoPago);
                conexion.Open();
                return Convert.ToDecimal(comando.ExecuteScalar());
            }
        }
    }
}
