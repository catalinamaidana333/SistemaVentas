using SistemaVentas.BLL;
using SistemaVentas.Entities;
using SistemaVentas.GUI.Contexto;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SistemaVentas.GUI
{
    /// <summary>
    /// Lógica de interacción para UsuarioControl.xaml
    /// </summary>
    public partial class UsuarioControl : UserControl
    {
        public UsuarioControl()
        {
            InitializeComponent();
        }
        // 1. Tu botón original que está arriba de la tabla ahora solo abre el modal
        private void btnCrearUsuario_Click(object sender, RoutedEventArgs e)
        {
            ModalCrearUsuario.Visibility = Visibility.Visible;
        }

        // 2. Botón para cerrar el modal sin guardar
        private void btnCerrarModal_Click(object sender, RoutedEventArgs e)
        {
            ModalCrearUsuario.Visibility = Visibility.Collapsed;
            LimpiarFormulario();
        }

        // 3. El nuevo botón de Guardar que está dentro del formulario
        private void btnGuardarUsuario_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nombre = txtNombre.Text?.Trim();
                string correo = txtCorreo.Text?.Trim();
                string password = txtPassword.Password;

                // Obtener el ID del rol desde el ComboBox (usando la propiedad Tag)
                ComboBoxItem rolSeleccionado = (ComboBoxItem)cmbRol.SelectedItem;
                int idRol = Convert.ToInt32(rolSeleccionado.Tag);

                Usuario nuevoUsuario = new Usuario()
                {
                    Nombre = nombre,
                    Correo = correo,
                    Password = password,
                    IdRol = idRol
                };

                Usuario usuarioAutenticado = SesionGlobal.UsuarioActual;
                UsuarioBLL usuarioLogica = new UsuarioBLL();

                usuarioLogica.CrearUsuario(nuevoUsuario, usuarioAutenticado);

                MessageBox.Show("Usuario creado con éxito", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Cerrar modal y limpiar
                ModalCrearUsuario.Visibility = Visibility.Collapsed;
                LimpiarFormulario();

                // AQUÍ DEBERÍAS RECARGAR TU TABLA
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LimpiarFormulario()
        {
            txtNombre.Clear();
            txtCorreo.Clear();
            txtPassword.Clear();
            cmbRol.SelectedIndex = 0; // Vuelve al primer elemento (Vendedor)
        }
    }
}
