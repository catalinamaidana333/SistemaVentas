using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Microsoft.Data.SqlClient;
using SistemaVentas.Entities;

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

        public void RestaurarBaseDeDatos(string rutaArchivoOrigen)
        {
            string nombreBD = "SistemaVentas";
            string cadenaConexionMaster = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

            // 1. Asegurar que C:\Temp exista
            string carpetaTemp = @"C:\Temp";
            if (!System.IO.Directory.Exists(carpetaTemp))
            {
                System.IO.Directory.CreateDirectory(carpetaTemp);
            }

            // 2. Copiar el archivo .bak seleccionado a C:\Temp para evitar bloqueos de OneDrive
            string nombreArchivo = System.IO.Path.GetFileName(rutaArchivoOrigen);
            string rutaTempBackup = System.IO.Path.Combine(carpetaTemp, nombreArchivo);

            System.IO.File.Copy(rutaArchivoOrigen, rutaTempBackup, true);

            Microsoft.Data.SqlClient.SqlConnectionStringBuilder builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(cadenaConexionMaster);
            builder.InitialCatalog = "master";

            using (Microsoft.Data.SqlClient.SqlConnection conexion = new Microsoft.Data.SqlClient.SqlConnection(builder.ConnectionString))
            {
                conexion.Open();

                string queryPrep = $"ALTER DATABASE [{nombreBD}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                string queryRestaurar = $"RESTORE DATABASE [{nombreBD}] FROM DISK = @Ruta WITH REPLACE;";
                string queryPost = $"ALTER DATABASE [{nombreBD}] SET MULTI_USER;";

                using (Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand($"{queryPrep} {queryRestaurar} {queryPost}", conexion))
                {
                    cmd.CommandTimeout = 600;
                    // ¡IMPORTANTE: Usar la ruta temporal de C:\Temp, no la de OneDrive!
                    cmd.Parameters.AddWithValue("@Ruta", rutaTempBackup);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void RegistrarHistorialBackup(string nombreArchivo, string tipoOperacion)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = "INSERT INTO HistorialBackups (NombreArchivo, FechaHora, TipoOperacion, Estado) VALUES (@Nombre, GETDATE(), @Tipo, 'Completado')";

                using (SqlCommand cmd = new SqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombreArchivo);
                    cmd.Parameters.AddWithValue("@Tipo", tipoOperacion); // Ej: "BACKUP" o "RESTAURACION"
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<BackupInfo> ObtenerHistorialBackups()
        {
            var lista = new List<BackupInfo>();
            string cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = "SELECT IdBackup, NombreArchivo, FechaHora, TipoOperacion FROM HistorialBackups ORDER BY FechaHora DESC";

                using (SqlCommand cmd = new SqlCommand(query, conexion))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int contador = 1;
                        while (reader.Read())
                        {
                            lista.Add(new BackupInfo
                            {
                                BackupId = contador++,
                                NombreArchivo = reader["NombreArchivo"].ToString(),
                                FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                                Estado = reader["TipoOperacion"].ToString()
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}