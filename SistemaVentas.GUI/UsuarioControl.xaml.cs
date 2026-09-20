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

        private bool _controlsInitialized = false;

        public UsuarioControl()
        {
            InitializeComponent();
            _usuarioLogica = new UsuarioBLL();

            // Enganchamos el evento Loaded para que al abrir la vista cargue todo
            this.Loaded += UsuarioControl_Loaded;
        }

        private void UsuarioControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Marcar que los controles están listos
            _controlsInitialized = true;

            ConfigurarAccesos();
            CargarUsuarios();
        }

        private void ConfigurarAccesos()
        {
            int idRolActual = SesionGlobal.UsuarioActual.IdRol;

            
            // Esto va a afectar al Supervisor, que sí puede entrar, pero no editar.
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
                // Validamos que haya seleccionado una fecha antes de continuar
                if (!dpFechaNacimiento.SelectedDate.HasValue)
                {
                    MessageBox.Show("Por favor, seleccione una fecha de nacimiento.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ComboBoxItem rolSeleccionado = (ComboBoxItem)cmbRol.SelectedItem;

                // Armamos el objeto con TODOS los campos de la interfaz
                Usuario nuevoUsuario = new Usuario()
                {
                    NombreUsuario = txtNombreUsuario.Text?.Trim(),
                    Nombree = txtNombree.Text?.Trim(), // Ojo acá con el 'Nombree'
                    Apellido = txtApellido.Text?.Trim(),
                    DNI = txtDNI.Text?.Trim(),
                    FechaNacimiento = dpFechaNacimiento.SelectedDate.Value,
                    Direccion = txtDireccion.Text?.Trim(),
                    Correo = txtCorreo.Text?.Trim(),
                    Password = txtPassword.Password,
                    IdRol = Convert.ToInt32(rolSeleccionado.Tag)
                };

                

                // LLAMAMOS A LA VALIDACIÓN NUEVA DE LA BLL
                if (!_usuarioLogica.ValidarDatosNuevoUsuario(nuevoUsuario, out string error))
                {
                    MessageBox.Show(error, "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Si hay error, cortamos acá
                }

                // Si pasó las validaciones, lo guardamos
                _usuarioLogica.CrearUsuario(nuevoUsuario, SesionGlobal.UsuarioActual);

                ModalCrearUsuario.Visibility = Visibility.Collapsed;
                LimpiarFormulario();
                CargarUsuarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LimpiarFormulario()
        {
            txtNombreUsuario.Clear();
            txtNombree.Clear();
            txtApellido.Clear();
            txtDNI.Clear();
            dpFechaNacimiento.SelectedDate = null; // Así se limpia el DatePicker
            txtDireccion.Clear();
            txtCorreo.Clear();
            txtPassword.Clear();
            cmbRol.SelectedIndex = 0;
        }

        // Variable para almacenar el usuario que seleccionamos en la grilla
        private Usuario _usuarioSeleccionado;

        // Método que se ejecuta al hacer clic en "Editar" en cualquier fila
        // Método que se ejecuta al hacer clic en "Editar" en cualquier fila
        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            _usuarioSeleccionado = btn.DataContext as Usuario;

            if (_usuarioSeleccionado != null)
            {
                // Precargamos TODOS los TextBox y el DatePicker
                txtEditNombreUsuario.Text = _usuarioSeleccionado.NombreUsuario;
                txtEditNombree.Text = _usuarioSeleccionado.Nombree;
                txtEditApellido.Text = _usuarioSeleccionado.Apellido;
                txtEditDNI.Text = _usuarioSeleccionado.DNI;
                dpEditFechaNacimiento.SelectedDate = _usuarioSeleccionado.FechaNacimiento;
                txtEditDireccion.Text = _usuarioSeleccionado.Direccion;
                txtEditCorreo.Text = _usuarioSeleccionado.Correo;

                foreach (ComboBoxItem item in cmbEditRol.Items)
                {
                    if (Convert.ToInt32(item.Tag) == _usuarioSeleccionado.IdRol)
                    {
                        cmbEditRol.SelectedItem = item;
                        break;
                    }
                }

                // --- NUEVA LÓGICA PARA EL BOTÓN DE ESTADO ---
                if (_usuarioSeleccionado.Estado == true)
                {
                    btnCambiarEstado.Content = "Dar de Baja";
                    btnCambiarEstado.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dc3545")); // Rojo
                }
                else
                {
                    btnCambiarEstado.Content = "Dar de Alta";
                    btnCambiarEstado.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28a745")); // Verde
                }

                ModalEditarUsuario.Visibility = Visibility.Visible;
            }
        }

        private void btnGuardarEdicion_Click(object sender, RoutedEventArgs e)
{
    try
    {
        if (_usuarioSeleccionado == null) return;
        
        // Validar fecha en edición también
        if (!dpEditFechaNacimiento.SelectedDate.HasValue)
        {
            MessageBox.Show("La fecha de nacimiento no puede estar vacía.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ComboBoxItem rolSeleccionado = (ComboBoxItem)cmbEditRol.SelectedItem;

        // Actualizamos TODOS los campos
        _usuarioSeleccionado.NombreUsuario = txtEditNombreUsuario.Text.Trim();
        _usuarioSeleccionado.Nombree = txtEditNombree.Text.Trim();
        _usuarioSeleccionado.Apellido = txtEditApellido.Text.Trim();
        _usuarioSeleccionado.DNI = txtEditDNI.Text.Trim();
        _usuarioSeleccionado.FechaNacimiento = dpEditFechaNacimiento.SelectedDate.Value;
        _usuarioSeleccionado.Direccion = txtEditDireccion.Text.Trim();
        _usuarioSeleccionado.Correo = txtEditCorreo.Text.Trim();
        _usuarioSeleccionado.IdRol = Convert.ToInt32(rolSeleccionado.Tag);

        if (!_usuarioLogica.ValidarDatosNuevoUsuario(_usuarioSeleccionado, out string error))
                {
                    // Si falta un dato o la fecha está mal, mostramos el cartel de advertencia amarillo y cortamos
                    MessageBox.Show(error, "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Si pasa la validación, recién ahí mandamos a actualizar
                _usuarioLogica.ActualizarUsuario(_usuarioSeleccionado);

               
        MessageBox.Show("Usuario actualizado con éxito", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

        ModalEditarUsuario.Visibility = Visibility.Collapsed;
        CargarUsuarios();
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error al actualizar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

        
        private void btnCambiarEstado_Click(object sender, RoutedEventArgs e)
        {
            if (_usuarioSeleccionado == null) return;

            // Determinamos qué texto mostrar en las alertas según el estado
            string accion = _usuarioSeleccionado.Estado ? "dar de baja" : "dar de alta";

            var respuesta = MessageBox.Show($"¿Estás seguro de que deseas {accion} al usuario '{_usuarioSeleccionado.Nombree}'?",
                                            $"Confirmar {accion.ToUpper()}",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Warning);

            if (respuesta == MessageBoxResult.Yes)
            {
                try
                {
                    // Verificamos el estado y llamamos a la BLL correspondiente
                    if (_usuarioSeleccionado.Estado == true)
                    {
                        _usuarioLogica.DarDeBajaUsuario(_usuarioSeleccionado.IdUsuario);
                        MessageBox.Show("Usuario dado de baja exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        _usuarioLogica.DarDeAltaUsuario(_usuarioSeleccionado.IdUsuario);
                        MessageBox.Show("Usuario dado de alta exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    // Ocultar modal y recargar grilla
                    ModalEditarUsuario.Visibility = Visibility.Collapsed;
                    CargarUsuarios();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al {accion}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (!ValidadorGUI.EsNombreValido(txtNombree.Text, out string error))
            {
                // ACÁ: Usamos el rojo que definiste en App.xaml
                txtNombree.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeError");

                // TIP VISUAL: Podés engrosar el borde para que el error se note más
                txtNombree.BorderThickness = new Thickness(2);

                txtNombree.ToolTip = error;
            }
            else
            {
                txtNombree.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeNormalAzul");

                // Volvemos el grosor a la normalidad
                txtNombree.BorderThickness = new Thickness(1);

                txtNombree.ToolTip = null;
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

        private void txtDNI_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!ValidadorGUI.EsDNIValido(txtDNI.Text, out string error))
            {
                txtDNI.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeError");
                txtDNI.BorderThickness = new Thickness(2);
                txtDNI.ToolTip = error;
            }
            else
            {
                txtDNI.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeNormalAzul");
                txtDNI.BorderThickness = new Thickness(1);
                txtDNI.ToolTip = null;
            }
        }

        private void dpFechaNacimiento_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!ValidadorGUI.EsFechaNacimientoValida(dpFechaNacimiento.SelectedDate, out string error))
            {
                dpFechaNacimiento.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeError");
                dpFechaNacimiento.BorderThickness = new Thickness(2);
                dpFechaNacimiento.ToolTip = error;
            }
            else
            {
                dpFechaNacimiento.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeNormalAzul");
                dpFechaNacimiento.BorderThickness = new Thickness(1);
                dpFechaNacimiento.ToolTip = null;
            }
        }

        private void txtNombreUsuario_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!ValidadorGUI.EsNombreUsuarioValido(txtNombreUsuario.Text, out string error))
            {
                txtNombreUsuario.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeError");
                txtNombreUsuario.BorderThickness = new Thickness(2);
                txtNombreUsuario.ToolTip = error;
            }
            else
            {
                txtNombreUsuario.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeNormalAzul");
                txtNombreUsuario.BorderThickness = new Thickness(1);
                txtNombreUsuario.ToolTip = null;
            }
        }

        private void txtEditDNI_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!ValidadorGUI.EsDNIValido(txtEditDNI.Text, out string error))
            {
                txtEditDNI.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeError");
                txtEditDNI.BorderThickness = new Thickness(2);
                txtEditDNI.ToolTip = error;
            }
            else
            {
                txtEditDNI.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeNormalAzul");
                txtEditDNI.BorderThickness = new Thickness(1);
                txtEditDNI.ToolTip = null;
            }
        }

        private void dpEditFechaNacimiento_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!ValidadorGUI.EsFechaNacimientoValida(dpEditFechaNacimiento.SelectedDate, out string error))
            {
                dpEditFechaNacimiento.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeError");
                dpEditFechaNacimiento.BorderThickness = new Thickness(2);
                dpEditFechaNacimiento.ToolTip = error;
            }
            else
            {
                dpEditFechaNacimiento.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeNormalAzul");
                dpEditFechaNacimiento.BorderThickness = new Thickness(1);
                dpEditFechaNacimiento.ToolTip = null;
            }
        }

        private void txtEditNombreUsuario_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!ValidadorGUI.EsNombreUsuarioValido(txtEditNombreUsuario.Text, out string error))
            {
                txtEditNombreUsuario.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeError");
                txtEditNombreUsuario.BorderThickness = new Thickness(2);
                txtEditNombreUsuario.ToolTip = error;
            }
            else
            {
                txtEditNombreUsuario.BorderBrush = (Brush)Application.Current.FindResource("ColorBordeNormalAzul");
                txtEditNombreUsuario.BorderThickness = new Thickness(1);
                txtEditNombreUsuario.ToolTip = null;
            }
        }
        
        /// <summary>
        /// Manejador para cambios en el filtro de rol
        /// </summary>
        private void cmbFiltroRol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }

        /// <summary>
        /// Manejador para cambios en el filtro de estado
        /// </summary>
        private void cmbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }

        /// <summary>
        /// Aplica los filtros seleccionados a la grilla de usuarios
        /// Solo se ejecuta después de que todos los controles han sido inicializados
        /// </summary>
        private void AplicarFiltros()
        {
            try
            {
                // Evitar ejecutarse durante la inicialización del control
                if (!_controlsInitialized)
                {
                    return;
                }

                // Validar que los controles existan
                if (cmbFiltroEstado == null || cmbFiltroRol == null)
                {
                    return;
                }

                // Obtener valores seleccionados de los filtros
                int? idRolSeleccionado = null;
                bool? estadoSeleccionado = null;

                // Leer filtro de rol
                if (cmbFiltroRol.SelectedItem is ComboBoxItem itemRol)
                {
                    string tagRol = itemRol.Tag?.ToString();
                    if (!string.IsNullOrEmpty(tagRol) && tagRol != "0")
                    {
                        idRolSeleccionado = int.Parse(tagRol);
                    }
                }

                // Leer filtro de estado
                if (cmbFiltroEstado.SelectedItem is ComboBoxItem itemEstado)
                {
                    string tagEstado = itemEstado.Tag?.ToString();
                    if (tagEstado == "1") // Solo Activos
                    {
                        estadoSeleccionado = true;
                    }
                    else if (tagEstado == "2") // Solo Inactivos
                    {
                        estadoSeleccionado = false;
                    }
                    // Si tagEstado es "0", significa ambos, así que estadoSeleccionado permanece null
                }

                // Obtener usuarios filtrados desde BLL
                List<Usuario> usuariosFiltrados = _usuarioLogica.ObtenerUsuariosFiltrados(idRolSeleccionado, estadoSeleccionado);

                // Asignar la lista filtrada al DataGrid
                if (dgListaUsuarios != null)
                {
                    dgListaUsuarios.ItemsSource = usuariosFiltrados;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al aplicar filtros: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}