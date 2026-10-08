using SistemaVentas.DAL;
using SistemaVentas.Entities;
using System;

namespace SistemaVentas.BLL
{
    /// <summary>
    /// Clase de lógica de negocio para la gestión de las sesiones de caja de los usuarios.
    /// Implementa validaciones de apertura, concurrencia y los cálculos críticos de cierre.
    /// </summary>
    public class CajaUsuarioBLL
    {
        private readonly CajaUsuarioDAL _cajaUsuarioDAL;
        // Asumimos la existencia de estos DALs para calcular los totales.
        // Si no existen exactamente así, deberás crearlos o adaptar los nombres.
        private readonly VentaDAL _ventaDAL;
        private readonly CompraDAL _compraDAL;
        private readonly MovimientosCajaDAL _movimientosCajaDAL;

        public CajaUsuarioBLL()
        {
            _cajaUsuarioDAL = new CajaUsuarioDAL();
            _ventaDAL = new VentaDAL();
            _compraDAL = new CompraDAL();
            _movimientosCajaDAL = new MovimientosCajaDAL();
        }

        public decimal? ObtenerSaldoEsperadoApertura(int idCaja)
        {
            if (idCaja <= 0)
                throw new ArgumentException("Identificador de caja inválido.", nameof(idCaja));

            return _cajaUsuarioDAL.ObtenerSaldoEsperadoApertura(idCaja);
        }

        /// <summary>
        /// Valida las reglas de negocio e intenta abrir una nueva sesión de caja.
        /// </summary>
        public int AbrirCaja(CajaUsuario cajaUsuario, int idRolUsuario)
        {
            // Regla 1: Solo el Rol 2 (Vendedor) puede abrir caja.
            if (idRolUsuario != 2)
            {
                throw new InvalidOperationException("Solo los usuarios con rol de Vendedor pueden realizar la apertura de caja.");
            }

            // Regla 2: El usuario no debe tener ya una caja abierta.
            CajaUsuario sesionExistente = _cajaUsuarioDAL.ObtenerSesionAbiertaPorUsuario(cajaUsuario.IdUsuario);
            if (sesionExistente != null)
            {
                throw new InvalidOperationException("El usuario ya tiene un turno de caja abierto. Debe cerrarlo antes de iniciar uno nuevo.");
            }

            // Regla 3: La caja física seleccionada no debe estar siendo usada por otro usuario.
            bool cajaFisicaEnUso = _cajaUsuarioDAL.VerificarCajaFisicaEnUso(cajaUsuario.IdCaja);
            if (cajaFisicaEnUso)
            {
                throw new InvalidOperationException("La caja física seleccionada ya está siendo utilizada por otro usuario en este momento.");
            }

            // Regla 4: El monto inicial no puede ser negativo (puede ser 0).
            if (cajaUsuario.MontoInicial < 0)
            {
                throw new ArgumentException("El monto de apertura no puede ser negativo.");
            }

            // Si pasa todas las validaciones, procedemos a insertar en la base de datos.
            return _cajaUsuarioDAL.AbrirCaja(cajaUsuario);
        }

        /// <summary>
        /// Recupera una sesión abierta en caso de que el sistema se haya cerrado abruptamente.
        /// </summary>
        public CajaUsuario RecuperarSesionAbierta(int idUsuario)
        {
            return _cajaUsuarioDAL.ObtenerSesionAbiertaPorUsuario(idUsuario);
        }

        /// <summary>
        /// Realiza el cálculo crítico de arqueo y cierra la sesión de caja.
        /// </summary>
        public void CerrarCaja(CajaUsuario cajaCierre)
        {
            // 1. Validar que la caja a cerrar realmente exista y esté abierta.
            if (cajaCierre.IdCajaUsuario <= 0)
            {
                throw new ArgumentException("Identificador de sesión de caja inválido.");
            }

            CajaUsuario sesionAbierta = _cajaUsuarioDAL.ObtenerSesionAbiertaPorId(cajaCierre.IdCajaUsuario);
            if (sesionAbierta == null)
            {
                throw new InvalidOperationException("La sesión de caja no existe o ya está cerrada.");
            }

            cajaCierre.MontoInicial = sesionAbierta.MontoInicial;

            // 2. Obtener los totales desde la Base de Datos filtrando por ESTA sesión de caja (IdCajaUsuario).
            // NOTA: Deberás implementar estos métodos en VentaDAL y CompraDAL si aún no existen.
            decimal totalVentasEfectivo = _ventaDAL.ObtenerTotalVentasEfectivoPorSesion(cajaCierre.IdCajaUsuario);
            decimal totalVentasMp = _ventaDAL.ObtenerTotalVentasMercadoPagoPorSesion(cajaCierre.IdCajaUsuario);
            decimal totalGastosEfectivo = _compraDAL.ObtenerTotalComprasEfectivoPorSesion(cajaCierre.IdCajaUsuario);
            decimal saldoMovimientosManuales = _movimientosCajaDAL.ObtenerSaldoNetoPorSesion(cajaCierre.IdCajaUsuario);

            // 3. Poblar la entidad con los subtotales para guardarlos en el historial
            cajaCierre.MontoTotalVentasEfectivo = totalVentasEfectivo;
            cajaCierre.MontoTotalVentasMp = totalVentasMp;
            cajaCierre.MontoTotalGastosEfec = totalGastosEfectivo;

            // 4. EL CÁLCULO CRÍTICO: Monto Sistema
            // Fórmula: Monto Inicial + Ventas en Efectivo - Gastos/Compras en Efectivo
            // Omitimos MercadoPago del MontoSistema porque ese dinero no está físicamente en la caja registradora.
            decimal montoSistema = cajaCierre.MontoInicial + totalVentasEfectivo - totalGastosEfectivo + saldoMovimientosManuales;
            cajaCierre.MontoSistema = montoSistema;

            // 5. Calcular la Diferencia
            // Si el cajero declara (MontoCierre) $100 y el sistema esperaba $120, la diferencia es -$20 (Faltante).
            // Si el cajero declara $130 y el sistema esperaba $120, la diferencia es +$10 (Sobrante).
            cajaCierre.Diferencia = cajaCierre.MontoCierre - montoSistema;

            // 6. Enviar a la base de datos para efectuar el cierre
            _cajaUsuarioDAL.CerrarCaja(cajaCierre);
        }
    }
}