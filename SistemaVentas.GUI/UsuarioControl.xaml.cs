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

            // 1. Vendedor (3): No debería poder ver este UserControl
            if (idRol == 3)
            {
                MessageBox.Show("No tienes permisos para acceder a este módulo.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Visibility = Visibility.Collapsed; // Oculta todo el control
                return;
            }

            // 2. Supervisor (2): Solo lectura (ve vendedores, no puede crear ni editar)
            if (idRol == 2)
            {
                btnCrearUsuario.Visibility = Visibility.Collapsed;

                // Si usas edición directa en celdas, deshabilitamos la grilla
                if (dgListaUsuarios != null)
                {
                    dgListaUsuarios.IsReadOnly = true;
                }

                // Nota: Si tenés una columna con un botón "Editar" por cada fila en el XAML,
                // deberás manejar su visibilidad desde el XAML usando DataTriggers o en el evento AutoGeneratingColumn.
            }

            // 3. Gerente (1): Pasa de largo, tiene acceso a todo.
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
    }
}