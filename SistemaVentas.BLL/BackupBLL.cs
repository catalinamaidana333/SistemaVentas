using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.IO;
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

        public void GenerarRespaldo(string rutaDestinoFinal)
        {
            string nombreBD = "SistemaVentas"; // Nombre exacto de tu base de datos

            // Usamos una carpeta pública del sistema donde SQL Server tiene acceso garantizado
            string carpetaTemp = "C:\\Temp\\";
            if (!Directory.Exists(carpetaTemp))
            {
                Directory.CreateDirectory(carpetaTemp);
            }

            string archivoTemporal = Path.Combine(carpetaTemp, $"TempBackup_{DateTime.Now:yyyyMMdd_HHmmss}.bak");

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();

                // 1. SQL Server genera el backup en la carpeta C:\Temp\
                string query = $"BACKUP DATABASE [{nombreBD}] TO DISK = @Ruta WITH FORMAT, INIT, NAME = N'{nombreBD}-Full Database Backup';";

                using (SqlCommand comando = new SqlCommand(query, conexion))
                {
                    comando.CommandTimeout = 300;
                    comando.Parameters.AddWithValue("@Ruta", archivoTemporal);
                    comando.ExecuteNonQuery();
                }

                // 2. Copiamos el archivo desde C:\Temp\ a la ruta elegida por el usuario (Pendrive o Nube)
                if (File.Exists(archivoTemporal))
                {
                    File.Copy(archivoTemporal, rutaDestinoFinal, true);

                    // 3. Borramos el archivo temporal de C:\Temp
                    try
                    {
                        File.Delete(archivoTemporal);
                    }
                    catch
                    {
                        // Si queda retenido un momento, no afecta en nada
                    }
                }
                else
                {
                    throw new Exception("No se pudo generar el archivo de respaldo en el servidor.");
                }
            }
        }
    }
}