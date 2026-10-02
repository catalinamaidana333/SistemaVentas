using SistemaVentas.DAL;
using SistemaVentas.Entities;
using System;
using System.Linq;

namespace SistemaVentas.BLL
{
    public class VentaBLL
    {
        private readonly VentaDAL _ventaDAL;
        private readonly CN_Producto _productoBLL;

        public VentaBLL()
        {
            _ventaDAL = new VentaDAL();
            _productoBLL = new CN_Producto();
        }

        public int RegistrarVenta(Venta venta)
        {
            if (venta == null)
                throw new ArgumentNullException(nameof(venta));
            if (venta.IdCajaUsuario <= 0)
                throw new ArgumentException("Debe indicar una sesión de caja abierta para registrar la venta.", nameof(venta));
            if (venta.MetodoPago != "Efectivo" && venta.MetodoPago != "Mercado Pago")
                throw new ArgumentException("El método de pago no es válido.", nameof(venta));
            if (venta.Detalles == null || venta.Detalles.Count == 0)
                throw new ArgumentException("La venta debe contener al menos un producto.", nameof(venta));

            var productos = _productoBLL.Listar().Where(producto => producto.Activo).ToDictionary(producto => producto.IdProducto);
            foreach (DetalleVenta detalle in venta.Detalles)
            {
                if (detalle.Cantidad <= 0)
                    throw new ArgumentException("La cantidad de cada producto debe ser mayor que cero.", nameof(venta));
                if (!productos.TryGetValue(detalle.IdProducto, out Producto producto))
                    throw new ArgumentException($"El producto {detalle.IdProducto} no existe o no está activo.", nameof(venta));

               
                detalle.PrecioUnitario = producto.PrecioVenta;
                detalle.Subtotal = detalle.Cantidad * producto.PrecioVenta;
            }

            venta.Total = venta.Detalles.Sum(detalle => detalle.Subtotal);
            return _ventaDAL.Insertar(venta);
        }
    }
}
