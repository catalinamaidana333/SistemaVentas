using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using SistemaVentas.Entities;

namespace SistemaVentas.DAL
{
    public class CD_Producto
    {
        private readonly string cadena = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

        public List<Producto> Listar()
        {
            List<Producto> lista = new List<Producto>();

            using (SqlConnection oconexion = new SqlConnection(cadena))
            {
                try
                {
                    string query = @"
                SELECT  p.id_producto,
                        p.id_categoria,
                        p.codigo_barras,
                        p.nombre,
                        p.precio_costo,
                        p.precio_venta,
                        p.stock_actual,
                        p.stock_minimo,
                        p.activo,
                        c.nombre AS NombreCategoria
                FROM    dbo.Producto p
                INNER JOIN dbo.Categoria c ON c.id_categoria = p.id_categoria
                ORDER BY p.id_producto DESC";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Producto()
                            {
                                IdProducto = Convert.ToInt32(dr["id_producto"]),
                                IdCategoria = Convert.ToInt32(dr["id_categoria"]),
                                CodigoBarras = dr["codigo_barras"] != DBNull.Value ? dr["codigo_barras"].ToString() : "",
                                Nombre = dr["nombre"].ToString(),
                                PrecioCosto = Convert.ToDecimal(dr["precio_costo"]),
                                PrecioVenta = Convert.ToDecimal(dr["precio_venta"]),
                                StockActual = Convert.ToInt32(dr["stock_actual"]),
                                StockMinimo = Convert.ToInt32(dr["stock_minimo"]),
                                Activo = Convert.ToBoolean(dr["activo"]),
                                oCategoria = new Categoria()
                                {
                                    IdCategoria = Convert.ToInt32(dr["id_categoria"]),
                                    Nombre = dr["NombreCategoria"].ToString()
                                }
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // CORRECCIÓN: Relanzar la excepción preservando el StackTrace original
                    throw new Exception("Error al consultar los datos en la base de datos: " + ex.Message, ex);
                }
            }
            return lista;
        }

        public bool Registrar(Producto obj, out string Mensaje)
        {
            bool respuesta = false;
            Mensaje = string.Empty;

            using (SqlConnection oconexion = new SqlConnection(cadena))
            {
                try
                {
                    oconexion.Open();

                    string query = @"INSERT INTO dbo.Producto
                             (id_categoria, codigo_barras, nombre, precio_costo, precio_venta, stock_actual, stock_minimo, activo)
                             VALUES
                             (@id_categoria, @codigo_barras, @nombre, @precio_costo, @precio_venta, @stock_actual, @stock_minimo, @activo)";

                    using (SqlCommand cmd = new SqlCommand(query, oconexion))
                    {
                        cmd.Parameters.AddWithValue("@id_categoria", obj.oCategoria.IdCategoria);
                        cmd.Parameters.AddWithValue("@codigo_barras", string.IsNullOrWhiteSpace(obj.CodigoBarras) ? (object)DBNull.Value : obj.CodigoBarras);
                        cmd.Parameters.AddWithValue("@nombre", obj.Nombre);
                        cmd.Parameters.AddWithValue("@precio_costo", obj.PrecioCosto);
                        cmd.Parameters.AddWithValue("@precio_venta", obj.PrecioVenta);
                        cmd.Parameters.AddWithValue("@stock_actual", obj.StockActual);
                        cmd.Parameters.AddWithValue("@stock_minimo", obj.StockMinimo > 0 ? obj.StockMinimo : 5);
                        cmd.Parameters.AddWithValue("@activo", obj.Activo ? 1 : 0);

                        respuesta = cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (SqlException ex)
                {
                    respuesta = false;
                    // Registrar el detalle técnico únicamente en la ventana de Salida / Logs para el desarrollador
                    System.Diagnostics.Debug.WriteLine($"Error SQL al registrar producto: {ex.Message}");

                    // Mensaje amigable y seguro para la interfaz de usuario
                    Mensaje = "No se pudo guardar el producto debido a un inconveniente con el servidor o los datos ingresados.";
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"Error inesperado al registrar producto: {ex.Message}");
                    Mensaje = "Ocurrió un error inesperado al procesar la solicitud.";
                }
            }

            return respuesta;
        }

        public bool Editar(Producto obj, out string Mensaje)
        {
            bool respuesta = false;
            Mensaje = string.Empty;

            using (SqlConnection oconexion = new SqlConnection(cadena))
            {
                try
                {
                    oconexion.Open();

                    string query = @"UPDATE dbo.Producto SET
                             id_categoria = @id_categoria,
                             codigo_barras = @codigo_barras,
                             nombre = @nombre,
                             precio_costo = @precio_costo,
                             precio_venta = @precio_venta,
                             stock_actual = @stock_actual,
                             stock_minimo = @stock_minimo,
                             activo = @activo
                             WHERE id_producto = @id_producto";

                    using (SqlCommand cmd = new SqlCommand(query, oconexion))
                    {
                        cmd.Parameters.AddWithValue("@id_producto", obj.IdProducto);
                        cmd.Parameters.AddWithValue("@id_categoria", obj.oCategoria.IdCategoria);
                        cmd.Parameters.AddWithValue("@codigo_barras", string.IsNullOrWhiteSpace(obj.CodigoBarras) ? (object)DBNull.Value : obj.CodigoBarras);
                        cmd.Parameters.AddWithValue("@nombre", obj.Nombre);
                        cmd.Parameters.AddWithValue("@precio_costo", obj.PrecioCosto);
                        cmd.Parameters.AddWithValue("@precio_venta", obj.PrecioVenta);
                        cmd.Parameters.AddWithValue("@stock_actual", obj.StockActual);
                        cmd.Parameters.AddWithValue("@stock_minimo", obj.StockMinimo > 0 ? obj.StockMinimo : 5);
                        cmd.Parameters.AddWithValue("@activo", obj.Activo ? 1 : 0);

                        respuesta = cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (SqlException ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"Error SQL al editar producto ID {obj.IdProducto}: {ex.Message}");
                    Mensaje = "No se pudo actualizar la información del producto.";
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"Error inesperado al editar producto: {ex.Message}");
                    Mensaje = "Ocurrió un error inesperado al procesar la solicitud.";
                }
            }

            return respuesta;
        }
        public bool Eliminar(Producto obj, out string Mensaje)
        {
            bool respuesta = false;
            Mensaje = string.Empty;

            using (SqlConnection oconexion = new SqlConnection(cadena))
            {
                try
                {
                    oconexion.Open();

                    // Borrado lógico deshabilitando el estado del producto
                    string query = "UPDATE dbo.Producto SET activo = 0 WHERE id_producto = @id_producto";

                    using (SqlCommand cmd = new SqlCommand(query, oconexion))
                    {
                        cmd.Parameters.AddWithValue("@id_producto", obj.IdProducto);
                        respuesta = cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    Mensaje = ex.Message;
                }
            }

            return respuesta;
        }

        public List<Categoria> ListarCategorias()
        {
            List<Categoria> lista = new List<Categoria>();

            using (SqlConnection oconexion = new SqlConnection(cadena))
            {
                try
                {
                    string query = "SELECT id_categoria, nombre FROM dbo.Categoria WHERE activa = 1 ORDER BY id_categoria";
                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Categoria()
                            {
                                IdCategoria = Convert.ToInt32(dr["id_categoria"]),
                                Nombre = dr["nombre"].ToString()
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    lista = new List<Categoria>();
                }
            }
            return lista;
        }
    }
}