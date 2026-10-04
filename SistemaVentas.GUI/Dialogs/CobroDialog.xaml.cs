using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SistemaVentas.GUI.Dialogs
{
    public partial class CobroDialog : Window
    {
        private readonly decimal _totalVenta;

        // Estas propiedades son las que leeremos desde Ventas.xaml.cs
        public decimal MontoIngresado { get; private set; }
        public decimal VueltoCalculado { get; private set; }

        public CobroDialog(decimal totalVenta)
        {
            InitializeComponent();

            _totalVenta = totalVenta;
            txtTotal.Text = $"$ {_totalVenta:N2}";

            // Foco automático para que el cajero empiece a teclear inmediatamente
            txtMontoAbonado.Focus();
        }

        // EL SISTEMA CALCULA EL VUELTO EN TIEMPO REAL
        private void txtMontoAbonado_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtMontoAbonado.Text, out decimal montoAbonado))
            {
                decimal diferencia = montoAbonado - _totalVenta;

                if (diferencia >= 0)
                {
                    txtVuelto.Text = $"$ {diferencia:N2}";
                    txtVuelto.Foreground = Brushes.ForestGreen;
                }
                else
                {
                    // Si el monto es menor al total, le indicamos cuánto le falta cobrar
                    txtVuelto.Text = $"Faltan $ {Math.Abs(diferencia):N2}";
                    txtVuelto.Foreground = Brushes.Red;
                }
            }
            else
            {
                txtVuelto.Text = "$ 0.00";
                txtVuelto.Foreground = Brushes.Black;
            }
        }

        private void btnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            ConfirmarVenta();
        }

        // Soporte para que el cajero confirme con ENTER rápido
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmarVenta();
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false; // Cancela si presiona Escape
            }
        }

        private void ConfirmarVenta()
        {
            if (decimal.TryParse(txtMontoAbonado.Text, out decimal monto) && monto >= _totalVenta)
            {
                MontoIngresado = monto;
                VueltoCalculado = monto - _totalVenta;

                DialogResult = true; // Cierra y avisa que fue exitoso
            }
            else
            {
                MessageBox.Show("El monto abonado es menor al total de la venta.", "Dinero Insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMontoAbonado.Focus();
                txtMontoAbonado.SelectAll();
            }
        }
    }
}