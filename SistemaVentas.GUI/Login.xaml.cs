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

using SistemaVentas.BLL;
using SistemaVentas.DAL;
using SistemaVentas.Entities;
using SistemaVentas.GUI.Contexto;

namespace SistemaVentas.GUI
{
    /// <summary>
    /// Lógica de interacción para Login.xaml
    /// Integrada con BLL para validación de credenciales contra BD SQL Server
    /// </summary>
    public partial class Login : Window
    {
        private UsuarioBLL usuarioBLL;
        

        public Login()
        {
            InitializeComponent();
            
            usuarioBLL = new UsuarioBLL();
           
        }

        /// <summary>
        /// Manejador del botón "INICIAR SESIÓN".
        /// Valida credenciales mediante la capa BLL contra BD SQL Server.
        /// </summary>
        private void btnIngresar_Click(object sender, RoutedEventArgs e)
        {
            // PASO 1: Obtener valores de inputs
            string correoIngresado = txtUsuario.Text?.Trim();
            string passwordIngresada = txtPassword.Password;

            // PASO 2: Validaciones básicas de UI (rápidas, antes de BD)
            if (string.IsNullOrWhiteSpace(correoIngresado))
            {
                MostrarError("El correo no puede estar vacío.");
                return;
            }

            if (string.IsNullOrWhiteSpace(passwordIngresada))
            {
                MostrarError("La contraseña no puede estar vacía.");
                return;
            }

            // PASO 3: Desactivar botón durante validación (evitar clicks múltiples)
            btnIngresar.IsEnabled = false;
            btnIngresar.Content = "Validando...";
            lblError.Visibility = Visibility.Collapsed;

            try
            {
                // PASO 4: Obtener usuario de BD por correo
                Usuario usuarioEnBD = usuarioBLL.ObtenerUsuarioPorCorreo(correoIngresado);

                if (usuarioEnBD == null)
                {
                    // Usuario no existe en BD
                    MostrarError("Correo no se encontro");
                    return;
                }

                // PASO 5: Verificar que la contraseña ingresada coincida con el hash en BD
                // Usa BCrypt.Verify (seguro para contraseñas hasheadas)
                if (!usuarioBLL.VerificarPassword(passwordIngresada, usuarioEnBD.Password))
                {
                    // Contraseña incorrecta
                    MostrarError("Contraseña incorrecta");
                    return;
                }

                // ✅ PASO 6: LOGIN EXITOSO
                // Guardar usuario en contexto global (accesible desde toda la app)
                SesionGlobal.UsuarioActual = usuarioEnBD;

                // PASO 7: Abrir ventana principal
                MainWindow ventanaPrincipal = new MainWindow();
                ventanaPrincipal.Show();

                // PASO 8: Cerrar ventana de login
                this.Close();
            }
            catch (UsuarioException ex)
            {
                // Error en lógica de negocio
                MostrarError($"Error en sistema: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Error inesperado
                MostrarError($"Error inesperado: {ex.Message}");
            }
            finally
            {
                // PASO 9: Restaurar estado del botón
                btnIngresar.IsEnabled = true;
                btnIngresar.Content = "INICIAR SESIÓN";
            }
        }

        /// <summary>
        /// Muestra mensaje de error en la etiqueta lblError
        /// </summary>
        private void MostrarError(string mensaje)
        {
            lblError.Text = mensaje;
            lblError.Visibility = Visibility.Visible;
        }
    }
}

