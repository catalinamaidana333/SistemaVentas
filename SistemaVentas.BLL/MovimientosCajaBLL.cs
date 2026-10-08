using SistemaVentas.DAL;
using SistemaVentas.Entities;
using System;

namespace SistemaVentas.BLL
{
    public class MovimientosCajaBLL
    {
        private readonly MovimientosCajaDAL _movimientosCajaDAL;
        private readonly CajaUsuarioDAL _cajaUsuarioDAL;

        public MovimientosCajaBLL()
        {
            _movimientosCajaDAL = new MovimientosCajaDAL();
            _cajaUsuarioDAL = new CajaUsuarioDAL();
        }

        public bool RegistrarMovimientoManual(MovimientosCaja mov)
        {
            ArgumentNullException.ThrowIfNull(mov);

            if (mov.IdCaja <= 0)
                throw new ArgumentException("Debe seleccionar una caja válida.", nameof(mov));

            if (mov.IdUsuario <= 0)
                throw new ArgumentException("El usuario que registra el movimiento no es válido.", nameof(mov));

            if (mov.Monto <= 0)
                throw new ArgumentException("El monto debe ser mayor que cero.", nameof(mov));

            if (string.IsNullOrWhiteSpace(mov.Motivo))
                throw new ArgumentException("El motivo es obligatorio.", nameof(mov));

            if (!string.Equals(mov.Tipo, "INGRESO", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(mov.Tipo, "EGRESO", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("El tipo de movimiento debe ser INGRESO o EGRESO.", nameof(mov));

            mov.Tipo = mov.Tipo.Trim().ToUpperInvariant();
            mov.Motivo = mov.Motivo.Trim();

            CajaUsuario sesionAbierta = _cajaUsuarioDAL.ObtenerSesionAbiertaPorCaja(mov.IdCaja);
            mov.IdCajaUsuario = sesionAbierta?.IdCajaUsuario;

            if (mov.FechaHora == default)
                mov.FechaHora = DateTime.Now;

            if (!_movimientosCajaDAL.InsertarMovimiento(mov))
                throw new InvalidOperationException("No se pudo registrar el movimiento de caja.");

            return true;
        }
    }
}
