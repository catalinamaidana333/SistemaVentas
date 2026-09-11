using System.Collections.Generic;
using SistemaVentas.DAL;
using SistemaVentas.Entities;

namespace SistemaVentas.BLL
{
	public class CN_Categoria
	{
		private CD_Categoria objcd_categoria = new CD_Categoria();

		public List<Categoria> Listar()
		{
			return objcd_categoria.Listar();
		}
	}
}