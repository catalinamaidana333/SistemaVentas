using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Microsoft.Data.SqlClient; 
using SistemaVentas.Entities; // Para poder usar la entidad CajaUsuario

namespace SistemaVentas.DAL
{
    public class CajaUsuarioDAL // Cambiamos internal por public para que la BLL pueda acceder
    {
        private string _cadenaConexion;

        public CajaUsuarioDAL()
        {
            // segun App.config 
            _cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;
        }

        // 1. Método para ABRIR la caja
        public int AbrirCaja(CajaUsuario caja)
        {
            int idGenerado = 0;
            // Usamos el estado 'Abierta' (o el booleano 'true'/'1' en SQL Server, asegúrate de cómo lo tienes en la base)
            string consulta = @"INSERT INTO CajaUsuario (IdCaja, IdUsuario, FechaApertura, MontoApertura, Estado) 
                                VALUES (@IdCaja, @IdUsuario, @FechaApertura, @MontoApertura, @Estado);
                                SELECT SCOPE_IDENTITY();"; // Para obtener el ID recién creado

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@IdCaja", caja.IdCaja);
                    comando.Parameters.AddWithValue("@IdUsuario", caja.IdUsuario);
                    comando.Parameters.AddWithValue("@FechaApertura", caja.FechaApertura);
                    comando.Parameters.AddWithValue("@MontoApertura", caja.MontoApertura);
                    comando.Parameters.AddWithValue("@Estado", caja.Estado); // true para abierta

                    conexion.Open();
                    // Ejecutamos y capturamos el ID insertado
                    idGenerado = Convert.ToInt32(comando.ExecuteScalar());
                }
            }
            return idGenerado;
        }

        // 2. Método para consultar si hay una caja abierta (y obtener su ID)
        public CajaUsuario ObtenerCajaAbiertaActiva()
        {
            CajaUsuario cajaActiva = null;
            // Busca la sesión que siga 'Abierta'
            string consulta = "SELECT TOP 1 IdCajaUsuario, IdCaja, IdUsuario, FechaApertura, MontoApertura, Estado FROM CajaUsuario WHERE Estado = @Estado ORDER BY IdCajaUsuario DESC";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@Estado", true); // true asumiendo que es bit en SQL para 'Abierta'

                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            cajaActiva = new CajaUsuario
                            {
                                IdCajaUsuario = Convert.ToInt32(lector["IdCajaUsuario"]),
                                IdCaja = Convert.ToInt32(lector["IdCaja"]),
                                IdUsuario = Convert.ToInt32(lector["IdUsuario"]),
                                FechaApertura = Convert.ToDateTime(lector["FechaApertura"]),
                                MontoApertura = Convert.ToDecimal(lector["MontoApertura"]),
                                Estado = Convert.ToBoolean(lector["Estado"])
                            };
                        }
                    }
                }
            }
            return cajaActiva;
        }

        // 3. Método para CERRAR la caja y guardar los totales
        public bool CerrarCaja(CajaUsuario caja)
        {
            bool respuesta = false;
            string consulta = @"UPDATE CajaUsuario 
                                SET FechaCierre = @FechaCierre, 
                                    MontoCierre = @MontoCierre, 
                                    MontoTotalVentasEfectivo = @VentasEfectivo, 
                                    MontoTotalVentasMp = @VentasMp, 
                                    MontoTotalGastosEfec = @Gastos,
                                    MontoSistema = @MontoSistema,
                                    Diferencia = @Diferencia,
                                    Estado = @Estado
                                WHERE IdCajaUsuario = @IdCajaUsuario";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@FechaCierre", caja.FechaCierre);
                    comando.Parameters.AddWithValue("@MontoCierre", caja.MontoCierre);
                    comando.Parameters.AddWithValue("@VentasEfectivo", caja.MontoTotalVentasEfectivo);
                    comando.Parameters.AddWithValue("@VentasMp", caja.MontoTotalVentasMp);
                    comando.Parameters.AddWithValue("@Gastos", caja.MontoTotalGastosEfec);
                    comando.Parameters.AddWithValue("@MontoSistema", caja.MontoSistema);
                    comando.Parameters.AddWithValue("@Diferencia", caja.Diferencia);
                    comando.Parameters.AddWithValue("@Estado", caja.Estado); // false para cerrada
                    comando.Parameters.AddWithValue("@IdCajaUsuario", caja.IdCajaUsuario);

                    conexion.Open();
                    respuesta = comando.ExecuteNonQuery() > 0;
                }
            }
            return respuesta;
        }
        // 4. Método para obtener los totales de ventas y compras en efectivo de una sesión
        // NO SE SI ESTA BIEN HECHO, REVISAR SI FUNCIONA
        public void ObtenerTotalesSistema(int idCajaUsuario, out decimal totalVentasEfectivo, out decimal totalComprasEfectivo)
        {
            totalVentasEfectivo = 0;
            totalComprasEfectivo = 0;

            // IMPORTANTE: Revisa que los nombres de las tablas (Venta, Compra) 
            // y columnas (Total, FormaPago) coincidan con los de tu base de datos.
            string consulta = @"
        DECLARE @Ventas DECIMAL(18,2) = 0;
        DECLARE @Compras DECIMAL(18,2) = 0;

        -- Sumar ventas en efectivo
        SELECT @Ventas = ISNULL(SUM(Total), 0) 
        FROM Venta 
        WHERE IdCajaUsuario = @IdCajaUsuario AND FormaPago = 'Efectivo';

        -- Sumar compras/gastos en efectivo
        SELECT @Compras = ISNULL(SUM(Total), 0) 
        FROM Compra 
        WHERE IdCajaUsuario = @IdCajaUsuario AND FormaPago = 'Efectivo';

        -- Devolver ambos resultados
        SELECT @Ventas AS TotalVentas, @Compras AS TotalCompras;";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@IdCajaUsuario", idCajaUsuario);

                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            totalVentasEfectivo = Convert.ToDecimal(lector["TotalVentas"]);
                            totalComprasEfectivo = Convert.ToDecimal(lector["TotalCompras"]);
                        }
                    }
                }
            }
        }
    }
}