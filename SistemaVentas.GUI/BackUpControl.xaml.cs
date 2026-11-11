using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using SistemaVentas.BLL;
using SistemaVentas.Entities;

namespace SistemaVentas.GUI
{
    /// <summary>
    /// Lógica de interacción para BackUpControl.xaml
    /// </summary>
    public partial class BackUpControl : UserControl
    {
        private ObservableCollection<BackupInfo> backups;
        private int contadorBackups = 0;

        public BackUpControl()
        {
            InitializeComponent();
            backups = new ObservableCollection<BackupInfo>();
            DgBackups.ItemsSource = backups;
        }

        private void BtnGenerarBackup_Click(object sender, RoutedEventArgs e)
        {
            // 1. Abrir la ventana para elegir dónde guardarlo (Pendrive o carpeta de la Nube)
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"SistemaVentas_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak",
                DefaultExt = ".bak",
                Filter = "Backup Files (*.bak)|*.bak|All Files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string rutaArchivo = saveFileDialog.FileName;

                    // 2. Ejecutar el backup real en SQL Server mediante la BLL
                    SistemaVentas.BLL.BackupBLL backupBll = new SistemaVentas.BLL.BackupBLL();
                    backupBll.GenerarRespaldo(rutaArchivo);

                    // 3. Registrar en la tabla visual
                    contadorBackups++;
                    DateTime ahora = DateTime.Now;

                    var nuevoBackup = new BackupInfo
                    {
                        BackupId = contadorBackups,
                        NombreArchivo = System.IO.Path.GetFileName(rutaArchivo), // Muestra el nombre del archivo
                        FechaHora = ahora,
                        Estado = "Completado"
                    };

                    backups.Insert(0, nuevoBackup); // Agregar al inicio de la tabla

                    // 4. Mostrar mensaje de éxito con la ruta exacta de guardado
                    MostrarMensajeExito();

                    MessageBox.Show(
                        $"¡Copia de seguridad realizada con éxito!\n\nGuardado en:\n{rutaArchivo}",
                        "Respaldo Exitoso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    // 5. Abrir automáticamente la carpeta en el Explorador de Windows
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{rutaArchivo}\"");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al generar backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MostrarMensajeExito()
        {
            TbMensajeExito.Visibility = Visibility.Visible;

            // Crear un temporizador para ocultar el mensaje después de 3 segundos
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += (s, e) =>
            {
                TbMensajeExito.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }

        // =========================================================================
        // MÉTODO DE DOBLE AUTORIZACIÓN (Gerente -> Supervisor) - DENTRO DE LA CLASE CORRECTA
        // =========================================================================
        private bool ValidarDobleAutorizacionJerarquica()
        {
            // --- NIVEL 1: AUTORIZACIÓN DEL GERENTE ---
            DialogoCredencial ventanaGerente = new DialogoCredencial("Autorización 1/2: Ingrese Gerente");
            if (ventanaGerente.ShowDialog() == true)
            {
                var gerente = ventanaGerente.UsuarioValidado;
                if (gerente == null || gerente.IdRol != 1) // 1 = Rol de Gerente
                {
                    MessageBox.Show("Acceso denegado: El usuario no posee permisos de Gerente.", "Seguridad", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // --- NIVEL 2: AUTORIZACIÓN DEL SUPERVISOR ---
                DialogoCredencial ventanaSupervisor = new DialogoCredencial("Autorización 2/2: Ingrese Supervisor");
                if (ventanaSupervisor.ShowDialog() == true)
                {
                    var supervisor = ventanaSupervisor.UsuarioValidado;
                    if (supervisor == null || supervisor.IdRol != 3) // 3 = Rol de Supervisor 
                    {
                        MessageBox.Show("Acceso denegado: El usuario no posee permisos de Supervisor.", "Seguridad", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }

                    return true; // ¡Ambos pasaron con éxito!
                }
            }
            return false;
        }

        // =========================================================================
        // EVENTO DEL BOTÓN RESTAURAR - DENTRO DE LA CLASE CORRECTA
        // =========================================================================
        private void BtnRestaurarBackup_Click(object sender, RoutedEventArgs e)
        {
            // 1. Abrir explorador para elegir el archivo .bak
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Backup Files (*.bak)|*.bak|All Files (*.*)|*.*",
                Title = "Seleccionar respaldo para restaurar"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string archivoSeleccionado = openFileDialog.FileName;

                // 2. Ejecutar la doble validación jerárquica
                if (!ValidarDobleAutorizacionJerarquica())
                {
                    return; // Si cancelan o no tienen los roles, se cancela todo
                }

                // 3. Confirmación de seguridad
                var confirmacion = MessageBox.Show(
                    "ADVERTENCIA: La restauración sobrescribirá los datos actuales con los del backup seleccionado.\n\n¿Deseas continuar?",
                    "Confirmar Restauración",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (confirmacion == MessageBoxResult.Yes)
                {
                    try
                    {
                        // 4. Llamada a la BLL para ejecutar el RESTORE en SQL Server
                        BackupBLL backupBll = new BackupBLL();
                        backupBll.RestaurarBaseDeDatos(archivoSeleccionado);

                        MessageBox.Show(
                            "¡Base de datos restaurada con éxito! Por seguridad, la aplicación se cerrará.",
                            "Restauración Exitosa",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );

                        Application.Current.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al restaurar la base de datos: {ex.Message}", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CargarHistorialDesdeBD()
        {
            try
            {
                BackupBLL backupBll = new BackupBLL();
                var listaDb = backupBll.ObtenerHistorialBackups();

                backups = new ObservableCollection<BackupInfo>(listaDb);
                DgBackups.ItemsSource = backups;
            }
            catch (Exception ex)
            {
                // Esto te mostrará si hay un error con la tabla o la conexión
                MessageBox.Show($"Error al cargar el historial desde la base de datos: {ex.Message}", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);

                backups = new ObservableCollection<BackupInfo>();
                DgBackups.ItemsSource = backups;
            }
        }
    }
}