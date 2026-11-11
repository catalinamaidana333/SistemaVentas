using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaVentas.Entities
{
    public class BackupInfo
    {
        public int BackupId { get; set; }
        public string NombreArchivo { get; set; }
        public DateTime FechaHora { get; set; }
        public string Estado { get; set; }
    }
}
