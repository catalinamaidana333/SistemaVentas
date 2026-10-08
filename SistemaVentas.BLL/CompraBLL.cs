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
            if (compra.IdUsuario <= 0 || compra.IdCajaUsuario <= 0 || compra.IdProveedor <= 0)
                throw new ArgumentException("La compra requiere usuario, sesión de caja y proveedor válidos.", nameof(compra));
            if (compra.MetodoPago != "Efectivo" && compra.MetodoPago != "Mercado Pago")
                throw new ArgumentException("El método de pago no es válido.", nameof(compra));
            if (compra.Detalles == null || compra.Detalles.Count == 0)
                throw new ArgumentException("La compra debe contener al menos un producto.", nameof(compra));

            foreach (DetalleCompra detalle in compra.Detalles)
            {
                if (detalle.IdProducto <= 0 || detalle.Cantidad <= 0 || detalle.CostoUnitario < 0)
                    throw new ArgumentException("Las líneas de compra requieren producto, cantidad y costo válidos.", nameof(compra));

                detalle.Subtotal = detalle.Cantidad * detalle.CostoUnitario;
            }

            compra.Total = compra.Detalles.Sum(detalle => detalle.Subtotal);
            return _compraDAL.Insertar(compra);
        }
    }
}
