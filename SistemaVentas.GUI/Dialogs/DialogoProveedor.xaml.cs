using SistemaVentas.BLL;
using SistemaVentas.Entities;
using System.Windows;

namespace SistemaVentas.GUI.Dialogs
{
    public partial class DialogoProveedor : Window
    {
        private readonly CN_Proveedor _cnProveedor = new CN_Proveedor();
        public Proveedor? ProveedorCreado { get; private set; }

        public DialogoProveedor()
        {
            InitializeComponent();
            txtRazonSocial.Focus();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            Proveedor proveedor = new Proveedor
            {
                RazonSocial = txtRazonSocial.Text,
                Cuit = txtCuit.Text,
                Telefono = txtTelefono.Text,
                NombreContacto = txtContacto.Text,
                Activo = true
            };

            if (_cnProveedor.Registrar(proveedor, out int idProveedor, out string mensaje))
            {
                proveedor.IdProveedor = idProveedor;
                ProveedorCreado = proveedor;
                MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            }
            else
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
