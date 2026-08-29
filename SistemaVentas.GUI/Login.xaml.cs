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
using System.Windows.Shapes;

namespace SistemaVentas.GUI
{
    /// <summary>
    /// Lógica de interacción para Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }
        private void btnIngresar_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text;
            string password = txtPassword.Password;

            // Aquí irá tu conexión a la BLL para validar en SQL Server
            bool esValido = true; // Simulación

            if (esValido)
            {
                MainWindow ventanaPrincipal = new MainWindow();
                ventanaPrincipal.Show();
                this.Close(); // Cierra la ventana de Login
            }
            else
            {
                lblError.Visibility = Visibility.Visible;
            }
        }
    }
}
