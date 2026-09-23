using System.Windows;

namespace SistemaVentas.GUI.Dialogs
{
    public partial class AbrirCajaDialog : Window
    {
        public decimal MontoIngresado { get; private set; }

        public AbrirCajaDialog()
        {
            InitializeComponent();
        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtMontoCaja.Text, out decimal monto) && monto >= 0)
            {
                MontoIngresado = monto;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Por favor ingrese un monto válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
