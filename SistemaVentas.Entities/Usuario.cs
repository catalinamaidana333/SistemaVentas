using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Password { get; set; }
        public int IdRol { get; set; }
    }
}
