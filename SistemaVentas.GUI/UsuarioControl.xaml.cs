using SistemaVentas.BLL;
using SistemaVentas.Entities;
using SistemaVentas.GUI.Contexto;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SistemaVentas.GUI
{
    public partial class UsuarioControl : UserControl
    {
        private UsuarioBLL _usuarioLogica;

        public UsuarioControl()
        {
            InitializeComponent();
            _usuarioLogica = new UsuarioBLL();

            // Enganchamos el evento Loaded para que al abrir la vista cargue todo
            this.Loaded += UsuarioControl_Loaded;
        }

        private void UsuarioControl_Loaded(object sender, RoutedEventArgs e)
        {
            ConfigurarAccesos();
            CargarUsuarios();
        }

        private void ConfigurarAccesos()
        {
            int idRolActual = SesionGlobal.UsuarioActual.IdRol;

            // 1. Preguntamos a la BLL si puede entrar
            if (!_usuarioLogica.PuedeAccederPantallaUsuarios(idRolActual))
            {
                MessageBox.Show("No tienes permisos para acceder a este módulo.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Visibility = Visibility.Collapsed;
                return;
            }

            // 2. Preguntamos a la BLL si puede editar/crear
            if (!_usuarioLogica.PuedeCrearOEditarUsuarios(idRolActual))
            {
                btnCrearUsuario.Visibility = Visibility.Collapsed;

                if (colAcciones != null)
                    colAcciones.Visibility = Visibility.Collapsed;

                if (dgListaUsuarios != null)
                    dgListaUsuarios.IsReadOnly = true;
            }
        }

        private void CargarUsuarios()
        {
            try
            {
                // La BLL se encarga de saber qué usuarios devolver según el rol
                List<Usuario> listaUsuarios = _usuarioLogica.ObtenerUsuariosParaVista(SesionGlobal.UsuarioActual.IdRol);

                // Asignamos la lista a la tabla (Asegurate que tu DataGrid se llame dgUsuarios)
                if (dgListaUsuarios != null)
                {
                    dgListaUsuarios.ItemsSource = listaUsuarios;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la lista de usuarios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCrearUsuario_Click(object sender, RoutedEventArgs e)
        {
            ModalCrearUsuario.Visibility = Visibility.Visible;
        }

        private void btnCerrarModal_Click(object sender, RoutedEventArgs e)
        {
            ModalCrearUsuario.Visibility = Visibility.Collapsed;
            LimpiarFormulario();
        }

        private void btnGuardarUsuario_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nombre = txtNombre.Text?.Trim();
                string correo = txtCorreo.Text?.Trim();
                string password = txtPassword.Password;

                ComboBoxItem rolSeleccionado = (ComboBoxItem)cmbRol.SelectedItem;
                int idRol = Convert.ToInt32(rolSeleccionado.Tag);

                Usuario nuevoUsuario = new Usuario()
                {
                    Nombre = nombre,
                    Correo = correo,
                    Password = password,
                    IdRol = idRol
                };

                _usuarioLogica.CrearUsuario(nuevoUsuario, SesionGlobal.UsuarioActual);

                MessageBox.Show("Usuario creado con éxito", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                ModalCrearUsuario.Visibility = Visibility.Collapsed;
                LimpiarFormulario();

                // AQUÍ RECARGAMOS LA TABLA
                CargarUsuarios();
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
            cmbRol.SelectedIndex = 0;
        }

        // Variable para almacenar el usuario que seleccionamos en la grilla
        private Usuario _usuarioSeleccionado;

        // Método que se ejecuta al hacer clic en "Editar" en cualquier fila
        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            // Obtenemos el botón que fue clickeado
            Button btn = sender as Button;

            // Extraemos la entidad Usuario que está vinculada a esa fila
            _usuarioSeleccionado = btn.DataContext as Usuario;

            if (_usuarioSeleccionado != null)
            {
                // Precargamos los TextBox
                txtEditNombre.Text = _usuarioSeleccionado.Nombre;
                txtEditCorreo.Text = _usuarioSeleccionado.Correo;

                // Precargamos el ComboBox de Roles buscando el Tag que coincida
                foreach (ComboBoxItem item in cmbEditRol.Items)
                {
                    if (Convert.ToInt32(item.Tag) == _usuarioSeleccionado.IdRol)
                    {
                        cmbEditRol.SelectedItem = item;
                        break;
                    }
                }

                // Mostramos el modal de edición
                ModalEditarUsuario.Visibility = Visibility.Visible;
            }
        }

        private void btnGuardarEdicion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Validar que tengamos un usuario seleccionado
                if (_usuarioSeleccionado == null) return;

                // 2. Extraer el rol del ComboBox
                ComboBoxItem rolSeleccionado = (ComboBoxItem)cmbEditRol.SelectedItem;
                int idRolNuevo = Convert.ToInt32(rolSeleccionado.Tag);

                // 3. Actualizar la entidad con los datos del formulario
                _usuarioSeleccionado.Nombre = txtEditNombre.Text.Trim();
                _usuarioSeleccionado.Correo = txtEditCorreo.Text.Trim();
                _usuarioSeleccionado.IdRol = idRolNuevo;

                // 4. Mandar a la BLL
                // (Ajusta el nombre del método según cómo lo hayas llamado en UsuarioBLL)
                _usuarioLogica.ActualizarUsuario(_usuarioSeleccionado);

                MessageBox.Show("Usuario actualizado con éxito", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // 5. Ocultar modal y recargar grilla
                ModalEditarUsuario.Visibility = Visibility.Collapsed;
                CargarUsuarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDarDeBaja_Click(object sender, RoutedEventArgs e)
        {
            // Validar seguridad (Preguntar primero)
            var respuesta = MessageBox.Show($"¿Estás seguro de que deseas dar de baja al usuario '{_usuarioSeleccionado.Nombre}'?",
                                            "Confirmar Baja",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Warning);

            if (respuesta == MessageBoxResult.Yes)
            {
                try
                {
                    // Mandar solo el ID a la BLL para el borrado lógico
                    // (Ajusta el nombre del método según cómo lo hayas llamado en UsuarioBLL)
                    _usuarioLogica.DarDeBajaUsuario(_usuarioSeleccionado.IdUsuario);

                    MessageBox.Show("Usuario dado de baja exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Ocultar modal y recargar grilla
                    ModalEditarUsuario.Visibility = Visibility.Collapsed;
                    CargarUsuarios();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al dar de baja: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCerrarModalEdicion_Click(object sender, RoutedEventArgs e)
        {
            // Simplemente ocultamos el modal de edición
            ModalEditarUsuario.Visibility = Visibility.Collapsed;
        }

        private void txtNombre_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!ValidadorGUI.EsNombreValido(txtNombre.Text, out string error))
            {
                // ACÁ: Usamos el rojo que definiste en App.xaml
                txtNombre.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeError");

                // TIP VISUAL: Podés engrosar el borde para que el error se note más
                txtNombre.BorderThickness = new Thickness(2);

                txtNombre.ToolTip = error;
            }
            else
            {
                txtNombre.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeNormalAzul");

                // Volvemos el grosor a la normalidad
                txtNombre.BorderThickness = new Thickness(1);

                txtNombre.ToolTip = null;
            }
        }

        private void txtCorreo_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!ValidadorGUI.EsCorreoValido(txtCorreo.Text, out string error))
            {
                txtCorreo.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeError");
                txtCorreo.BorderThickness = new Thickness(2);
                txtCorreo.ToolTip = error;
            }
            else
            {
                txtCorreo.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeNormalAzul");
                txtCorreo.BorderThickness = new Thickness(1);
                txtCorreo.ToolTip = null;
            }
        }

        private void txtPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            // Ojo aquí: usamos .Password en lugar de .Text
            if (!ValidadorGUI.EsPasswordValido(txtPassword.Password, out string error))
            {
                txtPassword.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeError");
                txtPassword.BorderThickness = new Thickness(2);
                txtPassword.ToolTip = error;
            }
            else
            {
                txtPassword.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeNormalAzul");
                txtPassword.BorderThickness = new Thickness(1);
                txtPassword.ToolTip = null;
            }
        }
    }
}