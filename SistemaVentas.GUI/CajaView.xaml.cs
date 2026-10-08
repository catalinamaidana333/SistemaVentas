using System;
using System.Collections.Generic;
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
using SistemaVentas.BLL;
using SistemaVentas.Entities; // O SistemaVentas.Entidades según tu proyecto

namespace SistemaVentas.GUI.Views
{
    public partial class CajaView : UserControl
    {
        private CN_Caja objCN_Caja = new CN_Caja();

        public CajaView()
        {
            InitializeComponent();
            CargarCajas();
        }

        private void CargarCajas()
        {
            dgvCajas.ItemsSource = objCN_Caja.Listar();
        }

        // Abrir el panel desplegable
        private void btnMostrarFormulario_Click(object sender, RoutedEventArgs e)
        {
            panelFormulario.Visibility = Visibility.Visible;
        }

        // Cerrar el panel desplegable
        private void btnCerrarFormulario_Click(object sender, RoutedEventArgs e)
        {
            panelFormulario.Visibility = Visibility.Collapsed;
        }

        // Registrar caja
        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            bool estaActiva = ((ComboBoxItem)cboEstado.SelectedItem).Content.ToString() == "Activa";

            Caja nuevaCaja = new Caja()
            {
                Descripcion = txtDescripcion.Text.Trim(),
                Activa = estaActiva
            };

            string mensaje;
            bool resultado = objCN_Caja.Registrar(nuevaCaja, out mensaje);

            if (resultado)
            {
                MessageBox.Show("Caja registrada exitosamente.", "Sistema", MessageBoxButton.OK, MessageBoxImage.Information);
                txtDescripcion.Clear();
                cboEstado.SelectedIndex = 0;
                panelFormulario.Visibility = Visibility.Collapsed; // Se oculta tras guardar
                CargarCajas();
            }
            else
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}