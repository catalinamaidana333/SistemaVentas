using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using SistemaVentas.Entities;

namespace SistemaVentas.DAL
{
	public class CD_Categoria
	{
		private readonly string cadena = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

		public List<Categoria> Listar()
		{
			List<Categoria> lista = new List<Categoria>();

			using (SqlConnection oconexion = new SqlConnection(cadena))
			{
				try
				{
					string query = "SELECT id_categoria, nombre, descripcion, activa FROM Categoria";
					SqlCommand cmd = new SqlCommand(query, oconexion);
					cmd.CommandType = CommandType.Text;

					oconexion.Open();

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							lista.Add(new Categoria()
							{
								IdCategoria = Convert.ToInt32(dr["id_categoria"]),
								Nombre = dr["nombre"].ToString(),
								Descripcion = dr["descripcion"] != DBNull.Value ? dr["descripcion"].ToString() : "",
								Activa = Convert.ToBoolean(dr["activa"])
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