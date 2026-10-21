using SistemaVentas.DAL;
using SistemaVentas.Entities;
using System;
using System.Linq;

namespace SistemaVentas.BLL
{
    public class CompraBLL
    {
        private readonly CompraDAL _compraDAL;

        public CompraBLL()
        {
            _compraDAL = new CompraDAL();
        }

        public int RegistrarCompra(Compra compra)
        {
            if (compra == null)
                throw new ArgumentNullException(nameof(compra));

            // 1. Validaciones básicas obligatorias (quitamos la obligación de la caja aquí)
            if (compra.IdUsuario <= 0 || compra.IdProveedor <= 0)
                throw new ArgumentException("La compra requiere usuario y proveedor válidos.", nameof(compra));

            if (compra.MetodoPago != "Efectivo" && compra.MetodoPago != "Mercado Pago")
                throw new ArgumentException("El método de pago no es válido.", nameof(compra));

            // 2. Validaciones de la Caja y Origen de fondos
            if (compra.IdCajaUsuario.HasValue && compra.IdCajaUsuario.Value <= 0)
                throw new ArgumentException("La sesión de caja indicada no es válida.", nameof(compra));

            // NUEVA REGLA: Si la plata sale de la caja, debe haber caja asignada.
            if (compra.OrigenEfectivo == "Caja" && !compra.IdCajaUsuario.HasValue)
                throw new ArgumentException("Si se paga con efectivo de la caja, debe haber una sesión abierta.", nameof(compra));

            // 3. Validaciones de los detalles de compra
            if (compra.Detalles == null || compra.Detalles.Count == 0)
                throw new ArgumentException("La compra debe contener al menos un producto.", nameof(compra));

            foreach (DetalleCompra detalle in compra.Detalles)
            {
                if (detalle.IdProducto <= 0 || detalle.Cantidad <= 0 || detalle.CostoUnitario < 0)
                    throw new ArgumentException("Las líneas de compra requieren producto, cantidad y costo válidos.", nameof(compra));
            }

            compra.Total = compra.Detalles.Sum(detalle => detalle.Subtotal);
            return _compraDAL.Insertar(compra);
        }
    }
}
