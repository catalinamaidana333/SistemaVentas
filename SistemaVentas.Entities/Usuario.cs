using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
       
        public string Correo { get; set; }
        public string Password { get; set; }
        public int IdRol { get; set; }
        public bool Estado { get; set; }

        // --- LOS CAMPOS NUEVOS ---
        public string NombreUsuario { get; set; }
        public string Nombree { get; set; }
        public string Apellido { get; set; }
        public string DNI { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Direccion { get; set; }
    }
}
