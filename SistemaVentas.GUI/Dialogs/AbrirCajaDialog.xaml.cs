using SistemaVentas.BLL;
using SistemaVentas.Entities;
using SistemaVentas.GUI.Contexto;
// Donde creamos SesionGlobal
using System;
using System.Windows;

namespace SistemaVentas.GUI.Dialogs
{
    public partial class AbrirCajaDialog : Window
    {
        private CajaUsuarioBLL _cajaBLL;
        public decimal MontoIngresado { get; private set; }

        public AbrirCajaDialog()
        {
            InitializeComponent();
            _cajaBLL = new CajaUsuarioBLL();
        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtMontoCaja.Text, out decimal monto) && monto >= 0)
            {
                try
                {
                    // 1. Preparamos la entidad
                    var nuevaCaja = new CajaUsuario
                    {
                        IdUsuario = SesionGlobal.IdUsuarioLogueado,
                        IdCaja = 1, // Asumiendo que el vendedor usa la "Caja Física 1". Esto podría ser un ComboBox en el futuro.
                        MontoInicial = monto
                    };

                    // 2. Llamamos a la BLL
                    int idGenerado = _cajaBLL.AbrirCaja(nuevaCaja, SesionGlobal.IdRolUsuarioLogueado);

                    // 3. Guardamos el ID real en la sesión global
                    SesionGlobal.IdCajaUsuarioActual = idGenerado;
                    MontoIngresado = monto;

                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    // Capturamos las validaciones de la BLL (ej. "Ya tienes una caja abierta")
                    MessageBox.Show(ex.Message, "Error al abrir caja", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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