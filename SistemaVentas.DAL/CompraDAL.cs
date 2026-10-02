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
            if (compra == null)
                throw new ArgumentNullException(nameof(compra));
            if (compra.IdCajaUsuario <= 0)
                throw new ArgumentException("La compra debe pertenecer a una sesión de caja válida.", nameof(compra));

            const string consultaCompra = @"INSERT INTO dbo.Compra
                                            (id_usuario, id_caja_usuario, id_proveedor, fecha_hora, metodo_pago, total)
                                            OUTPUT INSERTED.id_compra
                                            VALUES (@id_usuario, @id_caja_usuario, @id_proveedor, GETDATE(), @metodo_pago, @total);";
            const string consultaDetalle = @"INSERT INTO dbo.DetalleCompra
                                             (id_compra, id_producto, cantidad, costo_unitario_hist, subtotal)
                                             VALUES (@id_compra, @id_producto, @cantidad, @costo_unitario, @subtotal);";

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
                            comando.Parameters.AddWithValue("@id_caja_usuario", compra.IdCajaUsuario);
                            comando.Parameters.AddWithValue("@id_proveedor", compra.IdProveedor);
                            comando.Parameters.AddWithValue("@metodo_pago", compra.MetodoPago);
                            comando.Parameters.AddWithValue("@total", compra.Total);
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
    }
}
