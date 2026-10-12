using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace SistemaVentas.BLL
{
    public class BackupBLL
    {
        private readonly string _cadenaConexion;

        public BackupBLL()
        {
            _cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;
        }

        public void GenerarRespaldo(string rutaArchivo)
        {
            string nombreBD = "ConexionBD";

            string query = $"BACKUP DATABASE [{nombreBD}] TO DISK = @Ruta WITH FORMAT, INIT, NAME = N'{nombreBD}-Full Database Backup';";

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();
                using (SqlCommand comando = new SqlCommand(query, conexion))
                {
                    comando.CommandTimeout = 300; // 5 minutos por si la base pesa
                    comando.Parameters.AddWithValue("@Ruta", rutaArchivo);
                    comando.ExecuteNonQuery();
                }
            }
        }
    }
}