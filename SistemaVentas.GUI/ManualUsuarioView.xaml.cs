using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SistemaVentas.GUI
{
    public partial class ManualUsuarioView : UserControl
    {
        private const string ManualResourceUri =
            "pack://application:,,,/SistemaVentas.GUI;component/Docs/MANUAL_DE_USUARIO.md";

        public ManualUsuarioView()
        {
            InitializeComponent();
            CargarManual();
        }

        private void CargarManual()
        {
            try
            {
                var recurso = Application.GetResourceStream(
                    new Uri(ManualResourceUri, UriKind.Absolute));

                if (recurso == null)
                {
                    Viewer.Markdown =
                        "# Manual no disponible\n\nNo se encontró el recurso del manual.";
                    return;
                }

                using var lector = new StreamReader(recurso.Stream, Encoding.UTF8);
                Viewer.Markdown = lector.ReadToEnd();
            }
            catch (Exception ex)
            {
                Viewer.Markdown =
                    $"# Error al cargar el manual\n\n{ex.Message}";
            }
        }
    }
}
