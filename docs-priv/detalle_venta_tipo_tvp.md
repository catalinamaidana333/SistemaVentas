# `dbo.DetalleVentaTipo`: tipo table-valued para registrar ventas

## ¿Qué es?

Es un **tipo table-valued (TVP)** de SQL Server: una estructura con el mismo formato que las líneas del carrito.

```sql
CREATE TYPE dbo.DetalleVentaTipo AS TABLE
(
    id_producto                INT           NOT NULL,
    cantidad                   INT           NOT NULL,
    precio_unitario_historico  DECIMAL(18,2) NOT NULL,
    subtotal                   DECIMAL(18,2) NOT NULL
);
```

Se usa como parámetro de `dbo.usp_RegistrarVenta` para enviar **todo el detalle de la venta en una sola llamada**, en lugar de mandar una fila por vez.

## ¿Por qué lo definimos?

Porque una venta no es una línea: es una cabecera (`Venta`) más un conjunto de líneas (`DetalleVenta`). El TVP permite que el procedimiento almacenado reciba ese conjunto completo, inserte todas las líneas y descuente el stock dentro de **una única transacción en el servidor**.

En el código, la DAL arma un `DataTable` con el carrito y lo asigna al parámetro `@detalle` con `SqlDbType.Structured`. El SP lo recibe como una tabla de solo lectura.

## ¿En qué se diferencia de las alternativas?

| Alternativa | Diferencia principal |
|---|---|
| Un parámetro por cada línea | No escala: la cantidad de productos es variable y obligaría a llamar al SP N veces. |
| SP que registra una sola línea | La transacción termina viviendo en C# (`BeginTransaction`), no en el servidor. |
| Pasar JSON o XML | Menos tipado, requiere parsear y validar dentro del SP, y no se valida el esquema al compilar. |
| Tabla temporal | Requiere crear, cargar y limpiar la tabla; más pasos y más manejo de errores. |

El TVP mantiene el tipado fuerte, evita parseo manual y permite trabajar de forma **set-based**.

## ¿En qué ayuda?

- **Atomicidad real:** venta, detalle y stock se insertan/actualizan en una sola transacción del servidor.
- **Menos round-trips:** una sola llamada a la base para toda la venta.
- **Tipado:** SQL Server conoce las columnas y sus tipos; no hay que interpretar texto.
- **Seguridad:** se sigue usando parametrización, sin SQL dinámico.
- **Concurrencia:** el SP puede agrupar por producto y hacer un único `UPDATE` condicionado (`stock_actual >= cantidad`), reduciendo el riesgo de deadlocks frente a actualizar fila por fila en distinto orden.
- **Reutilización:** cualquier otro proceso que necesite registrar una venta puede invocar el mismo SP y el mismo tipo.

## ¿En qué dificulta?

- **Dependencia de esquema:** el tipo debe existir en la base antes de ejecutar el SP; por eso la migración lo crea primero.
- **Mantenimiento:** si cambia el detalle (por ejemplo, se agrega un descuento por línea), hay que modificar el tipo y la migración.
- **No se puede alterar fácilmente:** no existe `CREATE OR ALTER TYPE`; para cambiarlo suele haber que recrearlo y revisar dependencias.
- **Más código en la DAL:** hay que construir un `DataTable` con nombres y tipos exactos.
- **Menos evidente al depurar:** desde C# no se ve el contenido como una lista simple; hay que inspeccionar el `DataTable`.

## ¿Para qué se usa normalmente?

Los TVP son el mecanismo estándar de SQL Server para **enviar colecciones de filas a un procedimiento almacenado o función**. Se usan en cargas por lote, importaciones, registros de cabecera-detalle, integraciones y cualquier caso donde haya que procesar un conjunto variable de datos de forma transaccional.

## ¿Qué pasaría si el SP no usara el TVP?

Si el SP recibiera una sola línea por llamada:

- La DAL tendría que abrir una transacción en C# y ejecutar el SP una vez por producto.
- La atomicidad dependería de que el código cliente haga bien el `Commit`/`Rollback`.
- Habría más viajes a la base y más posibilidad de bloqueos parciales.
- El control de stock se repartiría entre C# y SQL, en lugar de quedar centralizado.

Si en cambio se pasaran los datos como JSON/XML:

- El SP tendría que parsear y validar manualmente.
- Se perdería parte de la validación de tipos que da el esquema.
- El código sería más difícil de leer y de mantener.

## Conclusión

`DetalleVentaTipo` es la pieza que permite que `usp_RegistrarVenta` reciba el carrito completo, valide el stock de forma atómica y registre la venta en una sola operación del lado de SQL Server. A cambio de un poco más de infraestructura (crear el tipo y armar el `DataTable`), se gana transaccionalidad, rendimiento y una separación más clara entre la aplicación y la base de datos.
