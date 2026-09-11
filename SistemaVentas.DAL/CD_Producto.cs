using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
                IF NOT EXISTS (SELECT 1 FROM dbo.Categoria)
                BEGIN
                    INSERT INTO dbo.Categoria (nombre, activa) VALUES ('Gral', 1);
                END

                INSERT INTO dbo.Producto (id_categoria, codigo_barras, nombre, precio_costo, precio_venta, stock_actual, stock_minimo, activo)
                VALUES ((SELECT TOP 1 id_categoria FROM dbo.Categoria), @codigo_barras, @nombre, @precio_costo, @precio_venta, @stock_actual, 5, 1)";

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

                    // 1. Crear categoría por defecto si la tabla está vacía
                    string sqlCategoria = @"
                IF NOT EXISTS (SELECT 1 FROM Categoria)
                BEGIN
                    INSERT INTO Categoria (nombre, activa) VALUES ('Gral', 1);
                END";

                    using (SqlCommand cmdCat = new SqlCommand(sqlCategoria, oconexion))
                    {
                        cmdCat.ExecuteNonQuery();
                    }

                    // 2. Obtener el ID de la categoría recién creada o existente
                    int idCategoria = 0;
                    string sqlGetId = "SELECT TOP 1 id_categoria FROM Categoria";
                    using (SqlCommand cmdId = new SqlCommand(sqlGetId, oconexion))
                    {
                        idCategoria = Convert.ToInt32(cmdId.ExecuteScalar());
                    }

                    // 3. Insertar el producto asignando ese ID válido
                    string query = @"INSERT INTO Producto (id_categoria, codigo_barras, nombre, precio_costo, precio_venta, stock_actual, stock_minimo, activo)
                             VALUES (@id_categoria, @codigo_barras, @nombre, @precio_costo, @precio_venta, @stock_actual, 5, 1)";

                    using (SqlCommand cmd = new SqlCommand(query, oconexion))
                    {
                        cmd.Parameters.AddWithValue("@id_categoria", idCategoria);
                        cmd.Parameters.AddWithValue("@codigo_barras", obj.CodigoBarras ?? "");
                        cmd.Parameters.AddWithValue("@nombre", obj.Nombre);
                        cmd.Parameters.AddWithValue("@precio_costo", obj.PrecioCosto);
                        cmd.Parameters.AddWithValue("@precio_venta", obj.PrecioVenta);
                        cmd.Parameters.AddWithValue("@stock_actual", obj.StockActual);

                        int filasAfectadas = cmd.ExecuteNonQuery();
                        if (filasAfectadas > 0)
                        {
                            respuesta = true;
                        }
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
    }
}