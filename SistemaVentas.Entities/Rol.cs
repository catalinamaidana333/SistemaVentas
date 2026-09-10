using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities
{
    public class Rol
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; }

       
    }
    public enum Roles
    {
        Gerente = 1,
        Vendedor = 2,
        Supervisor = 3
    }
}
