# Manual de Usuario

## Sistema de Gestión para Minimercado y Kiosco

**Versión del documento:** 1.0  
**Aplicación:** Sistema de Ventas (WPF, .NET 10, SQL Server)  
**Fecha:** Noviembre de 2026

---

## Índice

1. [Introducción y objetivos](#1-introducción-y-objetivos)
2. [Acceso y autenticación](#2-acceso-y-autenticación)
3. [Pantalla principal e interfaz](#3-pantalla-principal-e-interfaz)
4. [Módulo de Ventas y Facturación (POS)](#4-módulo-de-ventas-y-facturación-pos)
5. [Gestión de Catálogo e Inventario](#5-gestión-de-catálogo-e-inventario)
6. [Módulo de Compras y Proveedores](#6-módulo-de-compras-y-proveedores)
7. [Caja y Arqueo](#7-caja-y-arqueo)
8. [Reportes y Estadísticas](#8-reportes-y-estadísticas)
9. [Copias de Seguridad (Backups)](#9-copias-de-seguridad-backups)
10. [Solución de problemas frecuentes](#10-solución-de-problemas-frecuentes)
11. [Atajos de teclado](#11-atajos-de-teclado)
12. [Glosario](#12-glosario)
13. [Alcance y limitaciones de esta versión](#13-alcance-y-limitaciones-de-esta-versión)

> **Nota importante:** este manual describe la versión actual del sistema. El comprobante que emite la aplicación es un **ticket interno de venta en PDF**, no una factura fiscal. La versión actual no se conecta con AFIP ni emite Factura A, Factura B, Notas de Crédito o Notas de Débito.

---

## 1. Introducción y objetivos

### 1.1 Propósito del sistema

El Sistema de Ventas es una aplicación de escritorio para administrar las operaciones diarias de un minimercado o kiosco. Permite:

- Registrar ventas desde un punto de venta (POS).
- Controlar el stock de productos de forma automática.
- Registrar compras a proveedores y actualizar costos.
- Administrar la apertura, los movimientos y el cierre de caja.
- Gestionar usuarios y permisos según el rol.
- Consultar reportes de ventas, stock, actividad de usuarios y arqueo.
- Generar copias de seguridad de la base de datos.

### 1.2 Alcance

El sistema está dirigido a:

- **Gerente:** administración general, usuarios, productos, compras, reportes y backups.
- **Supervisor:** control de productos, stock y reportes operativos.
- **Vendedor:** operación del punto de venta, caja, compras y arqueo diario.

### 1.3 Requisitos del sistema

**Hardware**

- Computadora con Windows 10 u 11.
- Teclado y mouse.
- Conexión a la instancia de SQL Server donde se encuentra la base de datos `SistemaVentas`.
- Lector de PDF para abrir los tickets generados.
- Impresora (opcional): el sistema genera el ticket en PDF y luego puede imprimirse desde el visor de PDF.

**Software**

- .NET 10 (SDK para desarrollo o runtime para ejecución).
- SQL Server Express, Developer o superior.
- Base de datos `SistemaVentas` creada con los scripts del proyecto.
- Cadena de conexión configurada en `SistemaVentas.GUI/App.config`.
- Windows con permisos para leer y escribir en la carpeta temporal del usuario.

> **Nota:** la versión actual no utiliza pistola lectora de códigos de barras dentro del POS. La búsqueda de productos se realiza por nombre desde el catálogo.

---

## 2. Acceso y autenticación

### 2.1 Inicio de sesión

1. Abra la aplicación. Se mostrará la ventana **Iniciar Sesión**.
2. Ingrese el **correo electrónico** registrado para el usuario.
3. Ingrese la **contraseña**.
4. Presione **INICIAR SESIÓN**.

El sistema valida las credenciales contra la base de datos. Si el usuario está inactivo, mostrará un mensaje indicando que debe contactar al administrador.

Si el usuario tenía una **sesión de caja abierta** al momento de cerrar la aplicación o si el sistema se interrumpió, la sesión se recupera automáticamente al iniciar sesión. Esto permite continuar operando sin reabrir la caja.

### 2.2 Roles y permisos

Los roles se identifican en la base de datos con los siguientes IDs:

| ID | Rol |
|----|-----|
| 1 | Gerente |
| 2 | Vendedor |
| 3 | Supervisor |

Acceso a los módulos desde el menú lateral:

| Módulo | Gerente | Vendedor | Supervisor |
|--------|:-------:|:--------:|:----------:|
| Punto de Venta | Sí | Sí | Sí |
| Productos | Sí | No | Sí |
| Usuarios | Sí | No | No |
| Compras | Sí | Sí | No |
| Cajas | El botón existe, pero la vista no está enlazada | El botón existe, pero la vista no está enlazada | El botón existe, pero la vista no está enlazada |
| Reportes | Rentabilidad, Auditoría y Rendimiento | Arqueo Diario | Stock Crítico, Ventas Canceladas y Ventas por Vendedor |
| Backups | Sí | No | No |

> **Importante:** aunque el módulo **Punto de Venta** se muestra a todos los roles, la apertura de caja está reservada al rol **Vendedor**. Por lo tanto, un Gerente o un Supervisor no puede abrir una caja nueva ni registrar una venta desde su propia sesión.

### 2.3 Cierre de sesión

1. En el menú lateral, presione **Cerrar Sesión**.
2. El sistema elimina la sesión del usuario y vuelve a la pantalla de inicio de sesión.

La aplicación no incluye bloqueo de pantalla. Para proteger el sistema, cierre la sesión o bloquee la sesión de Windows.

### 2.4 Restablecimiento de contraseña

La versión actual no envía correos de recuperación. El enlace **¿Olvidaste tu contraseña?** de la pantalla de inicio de sesión no está habilitado.

Para restablecer una contraseña:

1. Inicie sesión como **Gerente**.
2. Ingrese a **Usuarios**.
3. Presione **Editar** en el usuario correspondiente.
4. Escriba una nueva contraseña en el campo **Contraseña**.
5. Presione **Guardar Cambios**.

La contraseña debe tener entre 8 y 128 caracteres.

### 2.5 Gestión de usuarios

El módulo **Usuarios** solo está disponible para el rol **Gerente**.

La pantalla permite:

1. Buscar usuarios por nombre, apellido, DNI o correo.
2. Filtrar por rol y por estado (activo o inactivo).
3. Crear un usuario con **Nuevo Usuario**.
4. Editar los datos de un usuario con **Editar**.
5. Activar o desactivar una cuenta desde el formulario de edición.

Al crear o editar un usuario, el sistema valida:

| Campo | Regla |
|---|---|
| Nombre de usuario | Entre 3 y 50 caracteres; solo minúsculas y números; no puede repetirse |
| Nombre y apellido | Entre 3 y 100 caracteres; solo letras y espacios |
| DNI | Entre 7 y 8 dígitos; no puede repetirse |
| Fecha de nacimiento | Edad entre 18 y 90 años; no puede ser futura |
| Dirección | Entre 5 y 200 caracteres |
| Correo electrónico | Formato válido; no puede repetirse |
| Contraseña | Entre 8 y 128 caracteres; obligatoria al crear; opcional al editar |
| Rol | Gerente, Vendedor o Supervisor |

El Gerente no puede desactivar su propia cuenta. Un usuario inactivo no puede iniciar sesión.

<!--
============================================================
PENDIENTE / IMPLEMENTABLE EN APROXIMADAMENTE 1 DÍA
- Cambio de contraseña por parte del propio usuario desde su perfil.
- Bloqueo de pantalla con reautenticación sin cerrar la sesión.
La recuperación de contraseña por correo electrónico requiere infraestructura
de envío de correos y no se considera una tarea de un día.
============================================================
-->

---

## 3. Pantalla principal e interfaz

### 3.1 Elementos de la pantalla

Al iniciar sesión se abre la ventana principal, compuesta por:

- **Menú lateral izquierdo:** accesos a los módulos habilitados según el rol.
- **Título del sistema:** `MINIMARKET`.
- **Nombre del usuario:** se muestra como `Hola, <nombre>`.
- **Botón Cerrar Sesión:** cierra la sesión actual.
- **Área de contenido:** muestra el módulo seleccionado.

### 3.2 Navegación

1. Presione un módulo del menú lateral.
2. El módulo se carga en el área de contenido, sin abrir una ventana nueva.
3. Para cambiar de módulo, presione otro acceso del menú.

Los módulos no habilitados para el rol no se muestran. En el caso de **Backups**, el botón se muestra deshabilitado para los roles que no son Gerente. El botón **Cajas** se muestra a todos los roles, pero no está enlazado a una vista en esta versión.

### 3.3 Filtros de sucursal, almacén o emisor

La versión actual está pensada para un único minimercado y no incluye filtros por sucursal, almacén ni emisor. Tampoco incluye un tablero con indicadores del día.

<!--
============================================================
PENDIENTE / IMPLEMENTABLE EN APROXIMADAMENTE 1 DÍA
- Dashboard inicial con accesos rápidos e indicadores del día.
La gestión de múltiples sucursales, almacenes o emisores no está implementada
y no se considera una tarea de un día.
============================================================
-->

---

## 4. Módulo de Ventas y Facturación (POS)

El módulo **Punto de Venta** permite registrar ventas, cobrar y generar el ticket interno.

### 4.1 Apertura de caja

Antes de vender, debe existir una sesión de caja abierta.

1. Ingrese a **Punto de Venta**.
2. En la barra superior, presione **📂 ABRIR CAJA**.
3. Seleccione la caja activa en el campo **Caja**.
4. Ingrese el **monto contado** inicial.
5. Presione **Aceptar**.

Reglas de apertura:

- Solo los usuarios con rol **Vendedor** pueden abrir caja.
- Un usuario no puede tener más de una sesión de caja abierta.
- Una caja física no puede ser utilizada por dos usuarios al mismo tiempo.
- El monto inicial debe ser mayor o igual a cero.
- Si el monto ingresado no coincide con el saldo esperado de la caja, el sistema muestra la diferencia y solicita confirmación para continuar.

Cuando la caja está abierta, el botón cambia a **📁 CERRAR CAJA**.

### 4.2 Carga de productos

1. En el campo de búsqueda del catálogo, escriba el **nombre** del producto.
2. Seleccione el producto en la lista.
3. Presione **+ AGREGAR** o **AGREGAR SELECCIONADO**.

El sistema valida el stock antes de agregar el producto. Si la cantidad solicitada supera el stock disponible, muestra un mensaje y no agrega la línea.

El catálogo del POS muestra únicamente productos:

- Activos.
- Con stock mayor a cero.

Los productos agotados dejan de aparecer en el catálogo hasta que se reponga el stock mediante una compra.

> **Nota:** la búsqueda es por nombre. La versión actual no permite buscar por código de barras ni utilizar una pistola lectora dentro del POS.

### 4.3 Carrito de venta

El carrito muestra las siguientes columnas:

| Columna | Descripción |
|---------|-------------|
| `#` | ID del producto |
| `ITEM` | Nombre del producto |
| `QUANTITY` | Cantidad |
| `PRICE` | Precio unitario |
| `Cant.` | Controles `-` y `+` para modificar la cantidad |
| `AMOUNT` | Subtotal de la línea |

Para modificar cantidades:

- Use los botones **+** y **-** de la fila.
- Seleccione una fila y use las teclas **+** y **-**.
- Seleccione una fila y presione un número del **1** al **9** para fijar esa cantidad exacta. Si el número supera el stock, la cantidad se ajusta al stock disponible.

Para quitar una línea:

1. Seleccione la fila.
2. Presione **DELETE**.

Para vaciar el carrito:

- Presione **LIMPIAR CARRITO** o **↪ CANCEL**.

> **Nota:** los botones **▲** y **▼** y el campo **Notes...** no forman parte del flujo operativo actual del POS.

### 4.4 Medios de pago

El selector **Método de pago** permite elegir:

- **Efectivo**
- **Mercado Pago**

La versión actual no incluye tarjeta de débito, tarjeta de crédito, transferencia bancaria ni billeteras virtuales distintas de Mercado Pago.

**Cobro en efectivo**

1. Seleccione **Efectivo**.
2. Presione **PROCESS SALE**.
3. En la ventana **Cobro en Efectivo**, ingrese el monto abonado por el cliente.
4. El sistema calcula el vuelto en tiempo real.
5. Presione **CONFIRMAR PAGO** o la tecla **Enter**.

El monto abonado debe ser igual o mayor al total de la venta. Si es menor, el sistema muestra **Dinero Insuficiente** y solicita corregir el importe. Presione **Esc** para cancelar el cobro.

**Mercado Pago**

1. Seleccione **Mercado Pago**.
2. Presione **PROCESS SALE**.
3. El sistema registra la venta por el total, sin cálculo de vuelto.

### 4.5 Confirmación de la venta y actualización de stock

Al presionar **PROCESS SALE**, el sistema:

1. Verifica que exista una caja abierta.
2. Verifica que el carrito no esté vacío.
3. Verifica que haya un método de pago seleccionado.
4. Registra la venta y su detalle en la base de datos.
5. Descuenta el stock de forma atómica.
6. Genera automáticamente el ticket en PDF.
7. Limpia el carrito y recarga el catálogo para reflejar el nuevo stock.

Si otro usuario vendió las últimas unidades mientras se realizaba la operación, el sistema muestra un mensaje de **Stock insuficiente** y no registra la venta.

### 4.6 Ticket de venta y reimpresión

El ticket se genera en formato PDF con el nombre comercial **AzulyOro Market** e incluye:

- Número de venta.
- Fecha y hora.
- Nombre del cajero.
- Método de pago.
- Detalle de productos: cantidad, producto e importe.
- Total.
- Monto abonado y vuelto (solo para pagos en efectivo).
- Leyenda de agradecimiento.

Ubicación del archivo:

```text
%TEMP%\SistemaVentas\Tickets
```

Por ejemplo:

```text
C:\Users\<usuario>\AppData\Local\Temp\SistemaVentas\Tickets
```

Para abrir el último ticket generado:

1. Presione **📄 RECEIPT**.
2. El sistema abre el PDF con el visor predeterminado de Windows.

Solo se puede reimprimir el último ticket generado mientras la pantalla de **Punto de Venta** permanezca abierta. Si aún no se registró una venta, el sistema muestra **Primero debe registrarse una venta**.

Si la venta se registra correctamente pero el ticket no puede generarse, la venta queda guardada y el sistema informa el error. En ese caso, verifique el espacio en disco y los permisos de la carpeta temporal.

### 4.7 Alcance del comprobante

El comprobante generado es un **ticket interno de venta**. No es un comprobante fiscal y no reemplaza a una factura.

La versión actual no incluye:

- Selección de tipo de comprobante (Factura A, Factura B, Notas de Crédito o Notas de Débito).
- Asociación de la venta a un cliente con CUIT o DNI.
- Descuentos manuales o por porcentaje.
- Pago dividido entre varios medios.
- Anulación de ventas o devoluciones.
- Facturación electrónica ni integración con AFIP.

<!--
============================================================
PENDIENTE / IMPLEMENTABLE EN APROXIMADAMENTE 1 DÍA
- Búsqueda por código de barras en el POS.
- Descuento manual por línea o por venta, siempre que se agregue el campo
  correspondiente a la tabla Venta y al procedimiento usp_RegistrarVenta.
- Anulación de venta con devolución de stock, agregando un estado a la tabla
  Venta y una operación de compensación.
- Módulo básico de clientes con alta, edición y búsqueda por DNI o CUIT.
La facturación electrónica AFIP, las notas de crédito o débito y la cuenta
corriente de clientes no se consideran tareas de un día y no se incluyen.
============================================================
-->

---

## 5. Gestión de Catálogo e Inventario

El módulo **Productos** está disponible para **Gerente** y **Supervisor**.

### 5.1 Alta de producto

1. Ingrese a **Productos**.
2. Complete el formulario izquierdo:

| Campo | Descripción |
|-------|-------------|
| Código de Barras | Se genera automáticamente y no es editable |
| Nombre del Producto | Nombre visible en el catálogo |
| Precio Costo | Costo unitario del producto |
| Precio Venta | Precio de venta al público |
| Stock Actual | Cantidad disponible |
| Categoría | Categoría a la que pertenece el producto |

3. Presione **Guardar Producto**.

Reglas de validación:

- El nombre es obligatorio.
- Debe seleccionar una categoría.
- El precio de venta debe ser mayor al precio de costo.
- Los precios y el stock deben ser valores numéricos válidos.
- El código de barras se genera automáticamente y debe ser único.

### 5.2 Edición de producto

1. En la tabla de productos, presione **Editar** en la fila correspondiente.
2. Modifique los campos permitidos.
3. Presione **Actualizar Producto**.

Al editar, el **Precio Costo** queda bloqueado. El costo se actualiza automáticamente al registrar compras.

> **Nota:** la tabla de productos no muestra un botón de eliminación. La versión actual permite crear y editar productos, pero no dar de baja productos desde la interfaz.

### 5.3 Stock y disponibilidad

El stock se actualiza de la siguiente manera:

- **Venta:** descuenta el stock de forma atómica al confirmar la venta.
- **Compra:** incrementa el stock al registrar la compra.
- **Catálogo del POS:** muestra solo productos activos con stock mayor a cero.

La versión actual no incluye ajustes manuales de inventario ni transferencias entre sucursales o almacenes.

<!--
============================================================
PENDIENTE / IMPLEMENTABLE EN APROXIMADAMENTE 1 DÍA
- Ajuste manual de stock con motivo, usuario y fecha, registrando el movimiento
  en una tabla de auditoría de inventario.
- Campo Stock Mínimo en el formulario de producto. Actualmente la columna
  stock_minimo existe en la base de datos, pero la interfaz no permite editarla.
Las transferencias entre sucursales no aplican a la operación actual de un
único minimercado y no se consideran una tarea de un día.
============================================================
-->

### 5.4 Alertas de stock bajo

El reporte **Stock Crítico** (disponible para Supervisor) muestra los productos activos cuyo stock actual es menor o igual al stock mínimo configurado en la base de datos.

> **Importante:** como la interfaz actual no permite editar el **Stock Mínimo**, los productos nuevos se crean con stock mínimo `5` (valor que aplica la capa de datos cuando no se informa otro). Para cambiar ese valor, el administrador debe actualizarlo directamente en la base de datos o esperar a que se habilite el campo en el formulario.

### 5.5 Categorías

Las categorías se cargan desde la tabla `Categoria` de la base de datos. La versión actual no incluye un módulo para crear, editar o eliminar categorías desde la interfaz.

<!--
============================================================
PENDIENTE / IMPLEMENTABLE EN APROXIMADAMENTE 1 DÍA
- ABM básico de categorías: alta, edición, baja lógica y listado.
============================================================
-->

### 5.6 Listado de productos

La tabla de productos muestra:

- Código de barras.
- Producto.
- Categoría.
- Precio de costo.
- Precio de venta.
- Stock.
- Acción **Editar**.

---

## 6. Módulo de Compras y Proveedores

El módulo **Compras** está disponible para **Gerente** y **Vendedor**. Permite registrar el ingreso de mercadería y actualizar el stock y los costos.

### 6.1 Registro de una compra

1. Ingrese a **Compras**.
2. Seleccione o busque un proveedor en el campo **Proveedor**.
3. Ingrese el **Número de factura** del proveedor (opcional).
4. Seleccione un producto del catálogo.
5. Ingrese la **Cantidad**.
6. Verifique o modifique el **Costo unitario**. El sistema propone el costo actual del producto.
7. Presione **+ AGREGAR**.
8. Repita los pasos para agregar más productos.
9. Complete las **Observaciones** si es necesario.
10. Seleccione el método de pago.
11. Presione **PROCESAR**.

La compra se registra y el stock de los productos se incrementa automáticamente.

### 6.2 Edición de líneas de compra

En la tabla de compras puede:

- Modificar la cantidad en la columna **QTY IN**.
- Modificar el costo unitario en la columna **UNIT COST**.
- Usar los botones **+** y **-** de cada fila.
- Seleccionar una fila y usar las teclas **+**, **-** o un número del **1** al **9**.
- Presionar **ELIMINAR** para quitar la línea seleccionada.
- Usar **▲** y **▼** para cambiar el orden de las líneas.

La cantidad y el costo unitario deben ser mayores a cero.

### 6.3 Alta rápida de proveedor

1. Presione el botón **+** junto al selector de proveedor.
2. Complete el formulario **Alta rápida de proveedor**:

| Campo | Descripción |
|-------|-------------|
| Razón Social | Nombre o razón social del proveedor |
| CUIT | Debe tener 11 dígitos y dígito verificador válido |
| Teléfono | Teléfono de contacto |
| Contacto | Nombre de la persona de contacto |

3. Presione **Guardar**.

El CUIT se valida y no puede repetirse entre proveedores. El proveedor queda activo y disponible para futuras compras.

### 6.4 Medios de pago y origen del efectivo

Métodos disponibles:

- **Mercado Pago** (seleccionado por defecto).
- **Efectivo**.

Si elige **Efectivo**, debe indicar el origen:

- **Caja / Mostrador:** requiere que el usuario tenga una sesión de caja abierta. El pago se descuenta de la caja.
- **Fondos externos:** no requiere caja abierta y no afecta el arqueo de la caja.

Si elige **Mercado Pago**, no se solicita origen de efectivo ni sesión de caja.

> **Importante:** la apertura de caja está reservada al rol Vendedor. Un Gerente que necesite registrar una compra en efectivo deberá usar **Fondos externos** o **Mercado Pago**, salvo que exista una sesión de caja abierta asociada a su usuario.

### 6.5 Alerta de margen

Después de registrar una compra, el sistema puede mostrar una alerta si el nuevo costo reduce el margen de ganancia por debajo del **20%**.

La ventana de alerta muestra:

- Producto.
- Costo anterior y costo nuevo.
- Precio de venta actual.
- Margen anterior y margen nuevo.
- Precio de venta sugerido.

Puede presionar **Aplicar** para actualizar el precio de venta del producto o cerrar la ventana para mantener el precio actual.

### 6.6 Limitaciones del módulo

La versión actual no incluye:

- Historial de compras.
- Anulación de compras.
- Directorio completo de proveedores con listado, edición y baja.
- Órdenes de compra.
- Cuenta corriente con proveedores.

<!--
============================================================
PENDIENTE / IMPLEMENTABLE EN APROXIMADAMENTE 1 DÍA
- Directorio de proveedores con listado, búsqueda, edición y baja lógica.
- Historial de compras con filtro por proveedor y rango de fechas.
El estado de cuenta corriente de clientes y proveedores no se considera una
tarea de un día y no se incluye.
============================================================
-->

---

## 7. Caja y Arqueo

### 7.1 Apertura de caja

La apertura se realiza desde el POS, como se describió en la sección 4.1.

Resumen de reglas:

- Solo el rol **Vendedor** puede abrir caja.
- Un usuario solo puede tener una caja abierta.
- Una caja física no puede ser usada por dos usuarios a la vez.
- El monto inicial no puede ser negativo.
- Si el monto no coincide con el saldo esperado, el sistema advierte la diferencia y permite continuar.

### 7.2 Cierre y arqueo de caja

1. En el POS, presione **📁 CERRAR CAJA**.
2. Ingrese el **dinero físico actual** contado en la caja.
3. Presione **Confirmar Arqueo**.

El sistema calcula:

```text
Monto esperado = Monto inicial
               + Ventas en efectivo
               - Compras en efectivo desde caja
               + Ingresos manuales
               - Egresos manuales
```

Luego calcula:

```text
Diferencia = Monto declarado - Monto esperado
```

Interpretación:

| Resultado | Significado |
|-----------|-------------|
| Diferencia = 0 | La caja cuadra correctamente |
| Diferencia > 0 | Sobró dinero en caja |
| Diferencia < 0 | Faltó dinero en caja |

Al finalizar, el sistema muestra un resumen con el monto esperado, el monto declarado y la diferencia. La sesión de caja queda cerrada y el botón vuelve a **📂 ABRIR CAJA**.

> **Nota:** las ventas por Mercado Pago no se suman al efectivo esperado en caja, porque no representan dinero físico en la registradora.

### 7.3 Movimientos manuales de caja

Desde el POS puede registrar ingresos y egresos de efectivo:

1. Presione **💰 Ingreso (Efectivo)** o **💸 Egreso (Efectivo)**.
2. Seleccione la caja.
3. Ingrese el monto.
4. Escriba el motivo (obligatorio).
5. Presione **Guardar**.

Los movimientos manuales afectan el monto esperado del arqueo. Si el usuario tiene una sesión de caja abierta, la caja queda fijada a esa sesión, salvo que el usuario sea Gerente.

### 7.4 Reporte de Arqueo Diario

El **Vendedor** puede consultar su arqueo desde **Reportes**. El reporte muestra:

- Fecha de apertura y, si corresponde, fecha de cierre.
- Monto inicial de caja.
- Total de ventas en efectivo.
- Total de ventas por Mercado Pago.
- Total de compras en efectivo.
- Monto esperado en caja según el sistema.
- Monto declarado por el vendedor.
- Diferencia.
- Estado de la caja.

> **Limitación conocida:** en la versión actual el reporte de Arqueo Diario es una versión simplificada. Consulta la última sesión de caja del usuario, no una sesión específica, y calcula el monto esperado solo con el monto inicial y las ventas en efectivo. No incluye las compras en efectivo desde caja ni los movimientos manuales de ingreso/egreso. Además, el total de ventas por Mercado Pago, el monto declarado, la diferencia y el estado pueden no mostrarse correctamente. Para el arqueo definitivo, use el cálculo que muestra la ventana **CERRAR CAJA**, que sí aplica la fórmula completa descrita en la sección 7.2.

Si el reporte indica **No se encontró información de caja para esta sesión**, verifique que el usuario tenga al menos una sesión de caja registrada.

### 7.5 Administración de cajas

El menú lateral incluye el botón **Cajas**, pero la vista de administración de cajas no está enlazada en esta versión. Por lo tanto, no es posible crear, editar o dar de baja cajas desde el menú.

<!--
============================================================
PENDIENTE / IMPLEMENTABLE EN APROXIMADAMENTE 1 DÍA
- Enlazar la vista CajaView al botón Cajas del menú principal.
- Impresión del cierre Z/X en PDF reutilizando QuestPDF, con el mismo formato
  del ticket de venta.
============================================================
-->

---

## 8. Reportes y Estadísticas

Los reportes se abren desde el menú **Reportes**. El contenido disponible depende del rol.

### 8.1 Reportes del Gerente

**Rentabilidad**

Muestra:

- Total de ventas.
- Total de compras o costos.
- Ganancia neta.

> **Limitación conocida:** el reporte de rentabilidad no está operativo en esta versión. Puede mostrar `$0.00` aunque existan ventas, porque la consulta actual no obtiene correctamente los costos desde el detalle de venta. Las ventas y el stock sí se registran bien. No utilice este reporte para calcular ganancias hasta que el administrador corrija la consulta.

**Auditoría y actividad de usuarios**

Muestra una tabla con:

- Nombre completo.
- Correo.
- Rol.
- Ventas totales.
- Monto vendido.
- Fecha de la última venta.

**Rendimiento mensual general**

Muestra una tabla con:

- Año.
- Mes.
- Total de ventas.
- Monto recaudado.
- Ticket promedio.

### 8.2 Reportes del Supervisor

**Stock crítico o bajo**

Muestra los productos activos con stock actual menor o igual al stock mínimo.

**Ventas canceladas y devoluciones del día**

Muestra las ventas registradas con total menor o igual a cero. Como la versión actual no permite anular ventas ni registrar devoluciones, este reporte normalmente aparecerá vacío.

> **Nota:** la columna **Método de Pago** puede aparecer vacía porque la consulta actual no devuelve ese dato.

**Ventas totales por vendedor**

Muestra:

- Vendedor.
- Correo.
- Cantidad de ventas.
- Total recaudado.
- Fecha de la última venta.

### 8.3 Reporte del Vendedor

**Arqueo diario**

Se describe en la sección 7.4.

### 8.4 Limitaciones de los reportes

La versión actual no incluye:

- Filtros por rango de fechas en la interfaz.
- Reporte de productos más vendidos.
- Reporte de márgenes por producto.
- Exportación de reportes a Excel o PDF.
- Reportes por sucursal.

<!--
============================================================
PENDIENTE / IMPLEMENTABLE EN APROXIMADAMENTE 1 DÍA
- Filtros de fecha desde/hasta en los reportes. Las consultas de Gerente y
  Supervisor ya aceptan parámetros de fecha en la capa de datos.
- Reporte de productos más vendidos agrupando DetalleVenta por producto.
- Reporte de margen por producto usando precio de venta y costo.
- Exportación de la grilla a CSV o PDF.
Los reportes por sucursal no aplican a la operación actual de un único local.
============================================================
-->

---

## 9. Copias de Seguridad (Backups)

El módulo **Gestión de Backups** está disponible exclusivamente para el rol **Gerente**.

### 9.1 Generar una copia de seguridad

1. Ingrese a **Gestión de Backups**.
2. Presione **Generar Backup**.
3. Seleccione la ubicación y el nombre del archivo `.bak`.
4. Presione **Guardar**.

El sistema genera la copia de seguridad de SQL Server y muestra la ruta exacta del archivo. Al finalizar, abre automáticamente la carpeta contenedora en el Explorador de Windows.

### 9.2 Historial de backups

La tabla **Historial de Backups** muestra los backups generados durante la sesión actual:

- ID.
- Nombre del archivo.
- Fecha y hora.
- Estado.

> **Importante:** el historial es solo visual y se mantiene en memoria. No se guarda en la base de datos ni se conserva después de cerrar la aplicación. El archivo `.bak` sí queda guardado en la ubicación elegida.

### 9.3 Recomendaciones

- Realice una copia de seguridad al finalizar cada jornada.
- Guarde los archivos en un disco externo o en una carpeta de respaldo.
- Verifique que la cuenta de servicio de SQL Server tenga permisos de escritura en la carpeta de destino.
- No reemplace la base de datos en producción sin la asistencia del administrador.

---

## 10. Solución de problemas frecuentes

### 10.1 No puedo iniciar sesión

| Posible causa | Solución |
|---------------|----------|
| Correo o contraseña incorrectos | Verifique los datos e intente nuevamente |
| Usuario inactivo | Solicite al Gerente que lo dé de alta desde **Usuarios** |
| Error de conexión a la base de datos | Verifique que SQL Server esté iniciado y que la cadena de conexión sea correcta |
| Contraseña olvidada | Solicite al Gerente que restablezca la contraseña desde **Usuarios > Editar** |

### 10.2 Error de conexión o caída del servidor

Si aparece un mensaje de error al cargar productos, ventas, compras o reportes:

1. Verifique que el servicio de SQL Server esté iniciado.
2. Verifique que la computadora tenga acceso a la instancia configurada.
3. Revise la cadena de conexión `ConexionBD` en `SistemaVentas.GUI/App.config`.
4. Verifique que la base de datos `SistemaVentas` exista.
5. Si el problema persiste, contacte al administrador del sistema.

### 10.3 No puedo registrar una venta

| Mensaje | Causa y solución |
|---------|------------------|
| **Debe abrir un turno de caja antes de procesar una venta** | Presione **ABRIR CAJA** y complete la apertura |
| **El carrito está vacío** | Agregue al menos un producto |
| **Seleccione un producto del catálogo** | Seleccione un producto de la lista antes de agregar |
| **Stock insuficiente** | Reduzca la cantidad o reponga stock mediante una compra |
| **Por favor, seleccione un método de pago válido** | Seleccione Efectivo o Mercado Pago |
| **El monto abonado es menor al total de la venta** | Ingrese un monto igual o mayor al total |

> **Importante:** si su usuario es Gerente o Supervisor, no podrá abrir caja. La operación de venta debe realizarla un usuario con rol Vendedor.

### 10.4 El ticket no se genera o no se abre

1. Verifique que la venta se haya registrado correctamente.
2. Verifique que la carpeta temporal tenga espacio disponible y permisos de escritura.
3. Verifique que haya un visor de PDF instalado.
4. Presione **📄 RECEIPT** para volver a abrir el último ticket.
5. Si el problema persiste, revise la ruta:

```text
%TEMP%\SistemaVentas\Tickets
```

### 10.5 No puedo registrar una compra

| Mensaje | Causa y solución |
|---------|------------------|
| **Seleccione un proveedor o registre uno nuevo** | Elija un proveedor de la lista o créelo con el botón **+** |
| **El proveedor seleccionado no está registrado en la base de datos** | Registre el proveedor desde el botón **+** |
| **Agregue al menos un producto antes de procesar la compra** | Cargue al menos una línea |
| **Ingrese una cantidad válida mayor a 0** | Corrija la cantidad |
| **Ingrese un costo unitario válido mayor a 0** | Corrija el costo |
| **Debe abrir un turno de caja antes de registrar una compra en efectivo desde la caja** | Abra caja o seleccione **Fondos externos** |

### 10.6 No puedo guardar un producto

| Mensaje | Causa y solución |
|---------|------------------|
| **Debe seleccionar una categoría** | Seleccione una categoría existente |
| **El precio de venta debe ser mayor al precio de costo** | Corrija los precios |
| **Revise los campos numéricos** | Ingrese valores válidos en precio y stock |
| **No se pudo generar el código de barras** | Verifique la conexión con la base de datos e intente nuevamente |

### 10.7 El reporte de stock crítico está vacío

El reporte muestra productos con `stock_actual <= stock_minimo`. Los productos nuevos se crean con stock mínimo `5`, pero si los datos se cargaron directamente en la base de datos con stock mínimo `0`, el reporte puede aparecer vacío aunque haya productos con poco stock.

Solución: configure el stock mínimo del producto en la base de datos o solicite la habilitación del campo en el formulario de productos.

### 10.8 El reporte de rentabilidad muestra $0.00

Es una limitación conocida de esta versión. La consulta del reporte no obtiene correctamente los costos desde el detalle de venta, por lo que el total de costos y la ganancia pueden quedar en cero. Las ventas y el stock se registran correctamente. No utilice este reporte para decisiones de rentabilidad hasta que el administrador corrija la consulta.

### 10.9 El reporte de ventas canceladas está vacío

Es el comportamiento esperado en la operación normal. La versión actual no permite anular ventas ni registrar devoluciones, por lo que no deberían existir ventas con total menor o igual a cero.

### 10.10 El arqueo diario no muestra información

1. Verifique que el usuario tenga al menos una sesión de caja registrada.
2. Si la caja está abierta, use la ventana **CERRAR CAJA** para obtener el arqueo definitivo.
3. Si la caja ya está cerrada y el reporte muestra datos incompletos, es una limitación conocida de esta versión; contacte al administrador.

### 10.11 No puedo generar un backup

- Verifique que su usuario tenga rol **Gerente**.
- Verifique que SQL Server tenga permisos para escribir en la carpeta seleccionada.
- Verifique que la ruta no esté bloqueada por permisos de Windows.
- Intente guardar el archivo en una carpeta local, por ejemplo `C:\Backups`.

### 10.12 No puedo acceder a un módulo

Los módulos se habilitan según el rol. Si un módulo no aparece en el menú, su usuario no tiene permisos para utilizarlo. Solicite al Gerente que revise su rol en **Usuarios**.

---

## 11. Atajos de teclado

### 11.1 Punto de Venta

| Tecla | Acción |
|-------|--------|
| **+** | Aumenta en 1 la cantidad de la fila seleccionada |
| **-** | Disminuye en 1 la cantidad de la fila seleccionada |
| **1** a **9** | Fija la cantidad exacta de la fila seleccionada |
| **Enter** | Confirma el cobro en efectivo (en la ventana de cobro) |
| **Esc** | Cancela el cobro en efectivo (en la ventana de cobro) |

> Los atajos de cantidad funcionan cuando la tabla del carrito tiene una fila seleccionada.

### 11.2 Compras

| Tecla | Acción |
|-------|--------|
| **+** | Aumenta en 1 la cantidad de la fila seleccionada |
| **-** | Disminuye en 1 la cantidad de la fila seleccionada |
| **1** a **9** | Fija la cantidad exacta de la fila seleccionada |

> Los atajos no se aplican mientras se está editando un campo de texto de la grilla.

---

## 12. Glosario

| Término | Definición |
|---------|------------|
| **POS** | Punto de venta. Pantalla donde se cargan productos y se cobra al cliente |
| **Caja** | Caja física o punto de cobro registrado en el sistema |
| **Sesión de caja** | Turno de trabajo de un usuario sobre una caja, desde la apertura hasta el cierre |
| **Arqueo** | Conteo y control del dinero físico de la caja contra el monto esperado por el sistema |
| **Monto esperado** | Dinero que el sistema calcula que debería haber en la caja |
| **Diferencia** | Resultado de restar el monto esperado al monto declarado |
| **Stock actual** | Cantidad disponible de un producto |
| **Stock mínimo** | Cantidad a partir de la cual se considera que el producto está en nivel crítico |
| **Margen** | Diferencia porcentual entre el precio de venta y el costo |
| **Ticket** | Comprobante interno de venta generado en PDF |
| **Backup** | Copia de seguridad de la base de datos en un archivo `.bak` |
| **BCrypt** | Algoritmo utilizado para almacenar las contraseñas de forma segura |

---

## 13. Alcance y limitaciones de esta versión

Esta versión del sistema incluye:

- Inicio de sesión con correo y contraseña.
- Roles Gerente, Vendedor y Supervisor.
- Punto de venta con carrito, validación de stock, cobro en efectivo y Mercado Pago.
- Generación de ticket de venta en PDF.
- Apertura, cierre y arqueo de caja.
- Movimientos manuales de ingreso y egreso de efectivo.
- Alta y edición de productos con código de barras automático.
- Registro de compras con actualización de stock y alertas de margen.
- Alta rápida de proveedores.
- Gestión de usuarios con validaciones, alta, edición, baja lógica y cambio de contraseña.
- Reportes de auditoría, rendimiento mensual, stock crítico, ventas canceladas, ventas por vendedor y arqueo diario.
- Módulo de rentabilidad disponible, con una limitación conocida en el cálculo de costos.
- Copias de seguridad de la base de datos para el rol Gerente.

Esta versión **no incluye**:

- Facturación fiscal electrónica ni integración con AFIP.
- Factura A, Factura B, Notas de Crédito o Notas de Débito.
- Módulo de clientes, cuenta corriente o historial de compras por cliente.
- Búsqueda por código de barras en el POS.
- Descuentos manuales, promociones o listas de precios.
- Pago dividido entre varios medios.
- Tarjetas de débito o crédito, transferencias bancarias o billeteras virtuales distintas de Mercado Pago.
- Anulación de ventas, devoluciones o notas de crédito.
- Ajustes manuales de inventario.
- Transferencias entre sucursales o almacenes.
- Múltiples sucursales, almacenes o emisores.
- Filtros por fecha en los reportes.
- Exportación de reportes a Excel o PDF.
- Reporte de productos más vendidos o margen por producto.
- Restauración de backups desde la interfaz.
- Historial persistente de backups.
- Administración de cajas desde el menú.
- Bloqueo de pantalla.
- Recuperación de contraseña por correo electrónico.

---

**Fin del manual.**
