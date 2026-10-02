using SistemaVentas.BLL;
using SistemaVentas.Entities;
using SistemaVentas.GUI.Contexto;
using System;
using System.Windows;

namespace SistemaVentas.GUI.Dialogs
{
    public partial class CerrarCajaDialog : Window
    {
        private CajaUsuarioBLL _cajaBLL;

        public CerrarCajaDialog()
        {
            InitializeComponent();
            _cajaBLL = new CajaUsuarioBLL();
        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtMontoCierre.Text, out decimal montoCierre) && montoCierre >= 0)
            {
                try
                {
                    // 1. Preparamos la entidad con el ID activo y el dinero declarado
                    var cajaCierre = new CajaUsuario
                    {
                        IdCajaUsuario = SesionGlobal.IdCajaUsuarioActual,
                        MontoCierre = montoCierre
                    };

                    // 2. Ejecutamos la lógica (calcula MontoSistema, Diferencia y hace el UPDATE)
                    _cajaBLL.CerrarCaja(cajaCierre);

                    // 3. Mostramos el resultado del Arqueo usando los datos que la BLL procesó
                    string estadoDiferencia = cajaCierre.Diferencia == 0 ? "¡Excelente! La caja cuadra perfectamente." :
                                             (cajaCierre.Diferencia > 0 ? "ATENCIÓN: Sobró dinero en caja." : "ATENCIÓN: Faltó dinero en caja.");

                    string mensajeArqueo = $"--- RESUMEN DE ARQUEO ---\n\n" +
                                           $"Sistema Esperaba: ${cajaCierre.MontoSistema:N2}\n" +
                                           $"Efectivo Declarado: ${cajaCierre.MontoCierre:N2}\n" +
                                           $"Diferencia: ${cajaCierre.Diferencia:N2}\n\n" +
                                           estadoDiferencia;

                    MessageBox.Show(mensajeArqueo, "Arqueo Finalizado", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 4. Limpiamos el estado global
                    SesionGlobal.IdCajaUsuarioActual = 0;

                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error al cerrar caja", MessageBoxButton.OK, MessageBoxImage.Error);
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