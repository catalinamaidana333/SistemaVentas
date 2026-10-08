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
            try
            {
                // Simular generación de backup
                contadorBackups++;
                DateTime ahora = DateTime.Now;

                var nuevoBackup = new BackupInfo
                {
                    BackupId = contadorBackups,
                    NombreArchivo = $"SistemaVentas_Backup_{ahora:yyyyMMdd_HHmmss}.bak",
                    FechaHora = ahora,
                    Estado = "Completado"
                };

                backups.Insert(0, nuevoBackup); // Agregar al inicio

                // Mostrar mensaje de éxito
                MostrarMensajeExito();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
