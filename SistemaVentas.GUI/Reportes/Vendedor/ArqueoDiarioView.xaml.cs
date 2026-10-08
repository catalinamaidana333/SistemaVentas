using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SistemaVentas.BLL.Reportes;
using SistemaVentas.DAL.Reportes;

namespace SistemaVentas.GUI.Views.Reportes.Vendedor
{
    public partial class ArqueoDiarioView : UserControl
    {
        private readonly ArqueoService _service = new ArqueoService();

        public ArqueoDiarioView(int idCajaUsuario)
        {
            InitializeComponent();
            CargarArqueo(idCajaUsuario);
        }

        private void CargarArqueo(int idCajaUsuario)
        {
            var resumen = _service.ObtenerResumen(idCajaUsuario);

            if (resumen == null)
            {
                TxtFecha.Text = "No se encontró información de caja para esta sesión.";
                return;
            }

            TxtFecha.Text = $"Apertura: {resumen.FechaApertura:dd/MM/yyyy HH:mm}" +
                             (resumen.FechaCierre.HasValue ? $"  |  Cierre: {resumen.FechaCierre:dd/MM/yyyy HH:mm}" : "  |  Caja abierta");

            TxtMontoInicial.Text = $"$ {resumen.MontoInicial:N2}";
            TxtVentasEfectivo.Text = $"$ {resumen.TotalVentasEfectivo:N2}";
            TxtVentasMp.Text = $"$ {resumen.TotalVentasMp:N2}";
            TxtComprasEfectivo.Text = $"$ {resumen.TotalComprasEfectivo:N2}";
            TxtMontoSistema.Text = $"$ {resumen.MontoSistema:N2}";
            TxtMontoDeclarado.Text = resumen.MontoDeclarado.HasValue ? $"$ {resumen.MontoDeclarado:N2}" : "Sin declarar aún";

            if (resumen.Diferencia.HasValue)
            {
                TxtDiferencia.Text = $"$ {resumen.Diferencia:N2}";
                TxtDiferencia.Foreground = resumen.Diferencia == 0
                    ? new SolidColorBrush(Colors.LightGreen)
                    : new SolidColorBrush(Colors.OrangeRed);
            }
            else
            {
                TxtDiferencia.Text = "—";
                TxtDiferencia.Foreground = new SolidColorBrush(Colors.White);
            }

            TxtEstado.Text = resumen.Estado;
        }
    }
}
