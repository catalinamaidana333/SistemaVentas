using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using SistemaVentas.Entities;
using Microsoft.Data.SqlClient;

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
                catch (Exception)
                {
                    lista = new List<Producto>();
                }
            }
            return lista;
        }

        // METODO AGREGADO PARA REGISTRAR PRODUCTOS EN LA BASE DE DATOS
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
                             (@id_categoria, @codigo_barras, @nombre, @precio_costo, @precio_venta, @stock_actual, 5, 1)";

                    using (SqlCommand cmd = new SqlCommand(query, oconexion))
                    {
                        cmd.Parameters.AddWithValue("@id_categoria", obj.oCategoria.IdCategoria);
                        cmd.Parameters.AddWithValue("@codigo_barras",
    string.IsNullOrWhiteSpace(obj.CodigoBarras) ? (object)DBNull.Value : obj.CodigoBarras);
                        cmd.Parameters.AddWithValue("@nombre", obj.Nombre);
                        cmd.Parameters.AddWithValue("@precio_costo", obj.PrecioCosto);
                        cmd.Parameters.AddWithValue("@precio_venta", obj.PrecioVenta);
                        cmd.Parameters.AddWithValue("@stock_actual", obj.StockActual);

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