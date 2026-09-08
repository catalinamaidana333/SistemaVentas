using SistemaVentas.BLL;
using SistemaVentas.Entities;
using SistemaVentas.GUI.Contexto;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
            int idRol = SesionGlobal.UsuarioActual.IdRol;

            // 1. Vendedor (Rol 2): No tiene permisos para ver la gestión de usuarios
            if (idRol == 2)
            {
                MessageBox.Show("No tienes permisos para acceder a este módulo.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Visibility = Visibility.Collapsed; // Oculta todo el control
                return;
            }

            // 2. Supervisor (Rol 3): Acceso de solo lectura (no puede crear ni editar)
            if (idRol == 3)
            {
                // Ocultar botón de crear usuario
                btnCrearUsuario.Visibility = Visibility.Collapsed;

                // Ocultar la columna de acciones (botón de editar)
                if (colAcciones != null)
                {
                    colAcciones.Visibility = Visibility.Collapsed;
                }

                // Deshabilitar la edición directa en la grilla por seguridad
                if (dgListaUsuarios != null)
                {
                    dgListaUsuarios.IsReadOnly = true;
                }
            }

            // 3. Gerente (Rol 1): Pasa de largo, mantiene visibilidad de la columna y botón de crear.
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
    }
}