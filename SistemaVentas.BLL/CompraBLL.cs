using SistemaVentas.DAL;
using SistemaVentas.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaVentas.BLL
{
    public class CompraBLL
    {
        private readonly CompraDAL _compraDAL;
        private readonly CN_Proveedor _proveedorBLL;
        private readonly CN_Producto _productoBLL;

        public CompraBLL()
        {
            _compraDAL = new CompraDAL();
            _proveedorBLL = new CN_Proveedor();
            _productoBLL = new CN_Producto();
        }

        public int RegistrarCompra(Compra compra, out List<AlertaMargen> alertas)
        {
            alertas = new List<AlertaMargen>();

            if (compra == null)
                throw new ArgumentNullException(nameof(compra));

            // 1. Validaciones básicas obligatorias (quitamos la obligación de la caja aquí)
            if (compra.IdUsuario <= 0 || compra.IdProveedor <= 0)
                throw new ArgumentException("La compra requiere usuario y proveedor válidos.", nameof(compra));

            Proveedor? proveedor = _proveedorBLL.ObtenerPorId(compra.IdProveedor);
            if (proveedor == null || !proveedor.Activo)
                throw new ArgumentException("El proveedor no existe o no está activo.", nameof(compra));

            if (compra.MetodoPago != "Efectivo" && compra.MetodoPago != "Mercado Pago")
                throw new ArgumentException("El método de pago no es válido.", nameof(compra));

            if (compra.IdCajaUsuario.HasValue && compra.IdCajaUsuario.Value <= 0)
                throw new ArgumentException("La sesión de caja indicada no es válida.", nameof(compra));

            if (compra.MetodoPago == "Efectivo")
            {
                if (compra.OrigenEfectivo == "Caja" && !compra.IdCajaUsuario.HasValue)
                    throw new ArgumentException("Si se paga con efectivo de la caja, debe indicar una sesión de caja válida.", nameof(compra));

                if (compra.OrigenEfectivo == "Externa" && compra.IdCajaUsuario.HasValue)
                    throw new ArgumentException("El efectivo externo no puede estar asociado a una sesión de caja.", nameof(compra));

                if (compra.OrigenEfectivo != "Caja" && compra.OrigenEfectivo != "Externa")
                    throw new ArgumentException("El origen del efectivo debe ser Caja o Externa.", nameof(compra));
            }
            else if (compra.OrigenEfectivo != null || compra.IdCajaUsuario.HasValue)
            {
                throw new ArgumentException("Las compras con Mercado Pago no pueden tener origen de efectivo ni sesión de caja.", nameof(compra));
            }

            if (compra.Detalles == null || compra.Detalles.Count == 0)
                throw new ArgumentException("La compra debe contener al menos un producto.", nameof(compra));

            var productos = _productoBLL.Listar().ToDictionary(producto => producto.IdProducto);
            foreach (DetalleCompra detalle in compra.Detalles)
            {
                if (detalle.IdProducto <= 0 || detalle.Cantidad <= 0 || detalle.CostoUnitario <= 0)
                    throw new ArgumentException("Las líneas de compra requieren producto, cantidad y costo válidos.", nameof(compra));

                if (!productos.ContainsKey(detalle.IdProducto))
                    throw new ArgumentException($"El producto {detalle.IdProducto} no existe.", nameof(compra));
            }

            decimal totalCalculado = compra.Detalles.Sum(detalle => detalle.Subtotal);
            if (compra.Total != totalCalculado)
                throw new ArgumentException("El total de la compra debe coincidir con la suma de sus detalles.", nameof(compra));

            foreach (DetalleCompra detalle in compra.Detalles)
            {
                Producto producto = productos[detalle.IdProducto];
                if (detalle.CostoUnitario <= producto.PrecioCosto)
                    continue;

                if (producto.PrecioVenta <= 0)
                    throw new ArgumentException($"El producto {producto.Nombre} no tiene un precio de venta válido para calcular su margen.", nameof(compra));

                decimal margenAnterior = ((producto.PrecioVenta - producto.PrecioCosto) / producto.PrecioVenta) * 100;
                decimal margenNuevo = ((producto.PrecioVenta - detalle.CostoUnitario) / producto.PrecioVenta) * 100;

                if (margenNuevo < margenAnterior && margenNuevo < 20m)
                {
                    alertas.Add(new AlertaMargen
                    {
                        IdProducto = producto.IdProducto,
                        NombreProducto = producto.Nombre,
                        CostoAnterior = producto.PrecioCosto,
                        CostoNuevo = detalle.CostoUnitario,
                        PrecioVenta = producto.PrecioVenta,
                        MargenAnterior = margenAnterior,
                        MargenNuevo = margenNuevo,
                        PrecioVentaSugerido = detalle.CostoUnitario / (1 - 0.20m)
                    });
                }
            }

            return _compraDAL.Insertar(compra);
        }
    }
}
