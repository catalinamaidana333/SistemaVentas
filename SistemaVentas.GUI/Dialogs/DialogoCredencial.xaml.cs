using System.Windows;
using SistemaVentas.BLL;
using SistemaVentas.Entities;

namespace SistemaVentas.GUI
{
    public partial class DialogoCredencial : Window
    {
        private UsuarioBLL usuarioBLL;
        public Usuario UsuarioValidado { get; private set; }

        public DialogoCredencial(string mensajeTitulo)
        {
            InitializeComponent();
            usuarioBLL = new UsuarioBLL();
            this.Title = mensajeTitulo; // Para que muestre "Autorización: Gerente" o "Supervisor"
        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            string correo = txtUsuario.Text.Trim();
            string password = txtPassword.Password;

            try
            {
                // Valida contra la base de datos usando tu BLL existente
                Usuario usuario = usuarioBLL.AutenticarUsuario(correo, password);

                if (usuario != null)
                {
                    UsuarioValidado = usuario;
                    this.DialogResult = true; // Cierra la ventana con éxito
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Credenciales incorrectas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
