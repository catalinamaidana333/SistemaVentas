using SistemaVentas.BLL;
using SistemaVentas.Entities;
using SistemaVentas.GUI.Contexto;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SistemaVentas.GUI.Dialogs
{
    public partial class DialogIngresoEgreso : Window
    {
        private readonly string _tipo;
        private readonly MovimientosCajaBLL _movimientosCajaBLL = new();
        private readonly List<Caja> _cajas;
        private readonly CajaUsuario? _sesionAbierta;

        public DialogIngresoEgreso(string tipo)
        {
            InitializeComponent();

            if (!string.Equals(tipo, "INGRESO", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(tipo, "EGRESO", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("El tipo de movimiento debe ser INGRESO o EGRESO.", nameof(tipo));

            _tipo = tipo.Trim().ToUpperInvariant();
            txtTitulo.Text = _tipo == "INGRESO" ? "Registrar ingreso de efectivo" : "Registrar egreso de efectivo";

            var cajaBLL = new CN_Caja();
            var cajaUsuarioBLL = new CajaUsuarioBLL();
            _cajas = cajaBLL.Listar().Where(caja => caja.Activa).ToList();
            _sesionAbierta = cajaUsuarioBLL.RecuperarSesionAbierta(SesionGlobal.IdUsuarioActual);

            if (_sesionAbierta != null && !_cajas.Any(caja => caja.IdCaja == _sesionAbierta.IdCaja))
            {
                _cajas.Add(new Caja
                {
                    IdCaja = _sesionAbierta.IdCaja,
                    Descripcion = $"Caja actual ({_sesionAbierta.IdCaja})",
                    Activa = true
                });
            }

            cmbCaja.ItemsSource = _cajas;
            cmbCaja.IsEnabled = _sesionAbierta == null || SesionGlobal.EsGerente;
            if (_sesionAbierta != null)
                cmbCaja.SelectedValue = _sesionAbierta.IdCaja;

            txtMonto.Focus();
        }

        private void txtMonto_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string textoPropuesto = txtMonto.Text.Remove(txtMonto.SelectionStart, txtMonto.SelectionLength)
                .Insert(txtMonto.SelectionStart, e.Text);
            e.Handled = !decimal.TryParse(textoPropuesto, NumberStyles.AllowDecimalPoint,
                CultureInfo.CurrentCulture, out _);
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCaja.SelectedItem is not Caja caja)
            {
                MessageBox.Show("Seleccione una caja.", "Caja requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtMonto.Text, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out decimal monto))
            {
                MessageBox.Show("Introduzca un monto válido.", "Monto inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMonto.Focus();
                txtMonto.SelectAll();
                return;
            }

            try
            {
                _movimientosCajaBLL.RegistrarMovimientoManual(new MovimientosCaja
                {
                    IdCaja = caja.IdCaja,
                    IdUsuario = SesionGlobal.IdUsuarioActual,
                    Tipo = _tipo,
                    Monto = monto,
                    Motivo = txtMotivo.Text,
                    FechaHora = DateTime.Now
                });

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo registrar el movimiento", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
