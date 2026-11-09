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
            if (venta.Detalles == null || venta.Detalles.Count == 0)
                throw new ArgumentException("La venta debe contener al menos un producto.", nameof(venta));

            // La transacción, el alta de Venta/DetalleVenta y el descuento atómico
            // de stock viven dentro del SP. La DAL solo envía el carrito como TVP.
            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            using (SqlCommand comando = new SqlCommand("dbo.usp_RegistrarVenta", conexion))
            {
                comando.CommandType = CommandType.StoredProcedure;

                comando.Parameters.AddWithValue("@id_caja_usuario", venta.IdCajaUsuario);
                comando.Parameters.AddWithValue("@metodo_pago", venta.MetodoPago);
                comando.Parameters.AddWithValue("@total", venta.Total);
                comando.Parameters.AddWithValue("@monto_abonado", venta.MontoAbonado);
                comando.Parameters.AddWithValue("@vuelto", venta.Vuelto);

                SqlParameter parametroDetalle = comando.Parameters.Add("@detalle", SqlDbType.Structured);
                parametroDetalle.TypeName = "dbo.DetalleVentaTipo";
                parametroDetalle.Value = CrearTablaDetalle(venta.Detalles);

                SqlParameter parametroIdVenta = comando.Parameters.Add("@id_venta", SqlDbType.Int);
                parametroIdVenta.Direction = ParameterDirection.Output;

                try
                {
                    conexion.Open();
                    comando.ExecuteNonQuery();
                }
                catch (SqlException ex) when (ex.Number == 50001)
                {
                    // El SP lanza 50001 cuando el UPDATE condicionado no pudo
                    // descontar el stock de todos los productos pedidos.
                    throw new StockInsuficienteException(ex.Message, ex);
                }

                return Convert.ToInt32(parametroIdVenta.Value);
            }
        }

        private static DataTable CrearTablaDetalle(IEnumerable<DetalleVenta> detalles)
        {
            DataTable tabla = new DataTable();
            tabla.Columns.Add("id_producto", typeof(int));
            tabla.Columns.Add("cantidad", typeof(int));
            tabla.Columns.Add("precio_unitario_historico", typeof(decimal));
            tabla.Columns.Add("subtotal", typeof(decimal));

            foreach (DetalleVenta detalle in detalles)
            {
                tabla.Rows.Add(
                    detalle.IdProducto,
                    detalle.Cantidad,
                    detalle.PrecioUnitario,
                    detalle.Subtotal);
            }

            return tabla;
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
