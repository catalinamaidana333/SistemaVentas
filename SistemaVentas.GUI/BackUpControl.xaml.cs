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
    }

    /// <summary>
    /// Modelo para representar la información de un backup
    /// </summary>
    public class BackupInfo
    {
        public int BackupId { get; set; }
        public string NombreArchivo { get; set; }
        public DateTime FechaHora { get; set; }
        public string Estado { get; set; }
    }
}