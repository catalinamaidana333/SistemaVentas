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

            var cajaBLL = new CN_Caja();
            var cajasActivas = cajaBLL.Listar().Where(caja => caja.Activa).ToList();
            cmbCaja.ItemsSource = cajasActivas;
            if (cajasActivas.Count == 1)
                cmbCaja.SelectedIndex = 0;
        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCaja.SelectedItem is not Caja caja)
            {
                MessageBox.Show("Seleccione una caja activa.", "Caja requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (decimal.TryParse(txtMontoCaja.Text, out decimal monto) && monto >= 0)
            {
                try
                {
                    decimal? saldoEsperado = _cajaBLL.ObtenerSaldoEsperadoApertura(caja.IdCaja);
                    if (saldoEsperado.HasValue && monto != saldoEsperado.Value)
                    {
                        decimal diferencia = monto - saldoEsperado.Value;
                        MessageBoxResult respuesta = MessageBox.Show(
                            $"El monto contado no coincide con el saldo esperado de la caja.\n\n" +
                            $"Saldo esperado: {saldoEsperado.Value:C2}\n" +
                            $"Monto contado: {monto:C2}\n" +
                            $"Diferencia: {diferencia:C2}\n\n" +
                            "¿Desea continuar con la apertura?",
                            "Diferencia en apertura",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning,
                            MessageBoxResult.No);

                        if (respuesta != MessageBoxResult.Yes)
                            return;
                    }

                    var nuevaCaja = new CajaUsuario
                    {
                        IdUsuario = SesionGlobal.IdUsuarioActual,
                        IdCaja = caja.IdCaja,
                        MontoInicial = monto
                    };

                    int idGenerado = _cajaBLL.AbrirCaja(nuevaCaja, SesionGlobal.IdRolActual);

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