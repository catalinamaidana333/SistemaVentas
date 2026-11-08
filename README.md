# Sistema de Gestión para Minimercado y Kiosco

![.NET](https://img.shields.io/badge/.NET-10-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-blue?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)

Aplicación de escritorio desarrollada para la cátedra **Taller de Programación 2**. El sistema administra las operaciones diarias de un minimercado o kiosco: ventas, compras, inventario, usuarios, caja, reportes y copias de seguridad.

El proyecto está implementado en **C# con .NET 10 y WPF**, utiliza **SQL Server** como base de datos y sigue una arquitectura estricta en capas.

## Arquitectura

```text
SistemaVentas.GUI (WPF)
        |
        v
SistemaVentas.BLL (reglas de negocio)
        |
        v
SistemaVentas.DAL (acceso a datos / ADO.NET)
        |
        v
SQL Server

SistemaVentas.Entities (modelos compartidos por las tres capas)
```

Reglas principales:

- `SistemaVentas.GUI` solo referencia `SistemaVentas.BLL` y `SistemaVentas.Entities`.
- `SistemaVentas.BLL` referencia `SistemaVentas.DAL` y `SistemaVentas.Entities`.
- `SistemaVentas.DAL` referencia únicamente `SistemaVentas.Entities`.
- La capa de presentación no contiene consultas SQL ni referencias directas a `SqlClient`.
- `SistemaVentas.Entities` es una capa transversal con las entidades del dominio.

## Tecnologías utilizadas

- **Lenguaje:** C#
- **Framework:** .NET 10
  - GUI: `net10.0-windows`
  - BLL, DAL y Entities: `net10.0`
- **Interfaz gráfica:** WPF (Windows Presentation Foundation)
- **Base de datos:** SQL Server
- **Acceso a datos:** ADO.NET (`Microsoft.Data.SqlClient` y `System.Data.SqlClient`)
- **Seguridad:** BCrypt.Net-Next 4.2.0
- **Generación de tickets PDF:** QuestPDF 2026.9.1
- **Configuración:** System.Configuration.ConfigurationManager 10.0.11
- **Solución:** `SistemaVentas.slnx`

## Estructura del repositorio

```text
SistemaVentas/
├── SistemaVentas.GUI/                 # Capa de presentación (WPF)
│   ├── Login.xaml                     # Inicio de sesión
│   ├── MainWindow.xaml                # Ventana principal y menú por rol
│   ├── Ventas.xaml                    # Punto de venta (POS)
│   ├── Compra.xaml                    # Registro de compras
│   ├── ProductoControl.xaml           # ABM de productos
│   ├── UsuarioControl.xaml            # ABM de usuarios
│   ├── CajaView.xaml                  # Administración de cajas
│   ├── BackUpControl.xaml             # Copias de seguridad
│   ├── Dialogs/                       # Diálogos de caja, cobro, proveedores, etc.
│   ├── Reportes/
│   │   ├── Gerente/                   # Rentabilidad, auditoría, rendimiento
│   │   ├── Supervisor/                # Stock crítico, ventas canceladas, ventas por vendedor
│   │   └── Vendedor/                  # Arqueo diario
│   ├── Contexto/SesionGlobal.cs       # Sesión y usuario autenticado
│   └── Converters/                    # Converters de WPF
├── SistemaVentas.BLL/                 # Capa de lógica de negocio
│   ├── UsuarioBLL.cs
│   ├── VentaBLL.cs
│   ├── CompraBLL.cs
│   ├── CN_Producto.cs
│   ├── CN_Proveedor.cs
│   ├── CN_Categoria.cs
│   ├── CN_Caja.cs
│   ├── CajaUsuarioBLL.cs
│   ├── MovimientosCajaBLL.cs
│   ├── BackupBLL.cs
│   ├── Reportes/                      # Servicios de reportes
│   └── Tickets/TicketService.cs       # Generación de tickets PDF
├── SistemaVentas.DAL/                 # Capa de acceso a datos
│   ├── UsuarioDAL.cs
│   ├── VentaDAL.cs
│   ├── CompraDAL.cs
│   ├── CD_Producto.cs
│   ├── CD_Proveedor.cs
│   ├── CD_Categoria.cs
│   ├── CD_Caja.cs
│   ├── CajaUsuarioDAL.cs
│   ├── MovimientosCajaDAL.cs
│   └── Reportes/                      # Repositorios de reportes
├── SistemaVentas.Entities/            # Entidades y DTOs
│   ├── Usuario.cs
│   ├── Rol.cs
│   ├── Producto.cs
│   ├── Categoria.cs
│   ├── Proveedor.cs
│   ├── Venta.cs
│   ├── DetalleVenta.cs
│   ├── Compra.cs
│   ├── DetalleCompra.cs
│   ├── Caja.cs
│   ├── CajaUsuario.cs
│   ├── MovimientosCaja.cs
│   ├── Reportes/
│   └── Tickets/
├── database/
│   ├── scriptSistVentas2309.sql       # Creación de la base y el esquema
│   ├── script.sql                     # Script de tablas
│   └── migrations/
│       ├── migracion_compras.sql
│       └── migracion_cuit_unico_proveedor.sql
├── .github/
├── SistemaVentas.slnx
└── README.md
```

## Funcionalidades

### Autenticación y seguridad

- Inicio de sesión mediante correo y contraseña.
- Contraseñas almacenadas con hash **BCrypt**.
- Validación de credenciales en la capa BLL.
- Sesión global con el usuario autenticado (`SesionGlobal`).
- Recuperación automática de una sesión de caja abierta al iniciar sesión.
- Cierre de sesión desde el menú principal.

### Punto de venta (POS)

- Catálogo de productos disponibles para la venta.
- Carrito de venta con cálculo automático de subtotales y total.
- Validación de stock antes de agregar o modificar cantidades.
- Modificación de cantidades con botones y atajos de teclado (`+`, `-`, números).
- Métodos de pago: **Efectivo**, **Mercado Pago** y **Transferencia**.
- Diálogo de cobro en efectivo con cálculo de vuelto.
- Registro transaccional de la venta y actualización de stock.
- Generación de ticket en PDF con **QuestPDF**.
- Reimpresión del último ticket generado.

### Caja

- Apertura de turno de caja con monto inicial.
- Cierre de caja con monto declarado y cálculo de diferencia.
- Registro de ingresos y egresos manuales.
- Arqueo diario con detalle de ventas en efectivo, ventas por Mercado Pago, compras en efectivo, monto esperado y diferencia.

> La vista `CajaView` existe como base para la administración de cajas, pero actualmente no está enlazada al menú principal. La apertura y el cierre de caja se realizan desde el POS.

### Compras

- Registro de compras con selección de proveedor.
- Alta de proveedores desde un diálogo.
- Carga de número de factura y observaciones.
- Métodos de pago: **Efectivo** y **Mercado Pago**.
- Origen del efectivo: **Caja** o **Externa**.
- Actualización de stock al registrar la compra.
- Alertas de margen cuando el costo de compra afecta la rentabilidad.

### Productos

- Alta, edición y eliminación de productos.
- Asociación a categorías.
- Generación automática de código de barras.
- Registro de precio de costo, precio de venta, stock actual y stock mínimo.
- Control de estado activo/inactivo.

### Usuarios

- Alta, edición y consulta de usuarios.
- Filtros por rol y estado.
- Activación y baja de usuarios.
- Cambio de contraseña con hash BCrypt.
- Validaciones de datos en la capa BLL.
- Control de acceso según el rol del usuario autenticado.

### Reportes

**Gerente**

- Rentabilidad: total de ventas, total de compras y ganancia neta.
- Auditoría y actividad de usuarios.
- Rendimiento mensual general.

**Supervisor**

- Stock crítico o bajo.
- Ventas canceladas y devoluciones del día.
- Ventas totales por vendedor.

**Vendedor**

- Arqueo diario de su sesión de caja.

### Backups

- Generación de copias de seguridad `.bak` de SQL Server.
- Selección de la ubicación de guardado.
- Acceso restringido exclusivamente al rol **Gerente**.
- Apertura automática de la carpeta contenedora al finalizar.

## Roles y permisos

Los roles se identifican en la base de datos con los siguientes IDs:

| ID | Rol |
|----|-----|
| 1 | Gerente |
| 2 | Vendedor |
| 3 | Supervisor |

Acceso a los módulos:

| Módulo | Gerente | Vendedor | Supervisor |
|--------|:-------:|:--------:|:----------:|
| Punto de Venta | Sí | Sí | Sí |
| Productos | Sí | No | Sí |
| Usuarios | Sí | No | No |
| Compras | Sí | Sí | No |
| Reportes | Rentabilidad, Auditoría y Rendimiento | Arqueo Diario | Stock Crítico, Ventas Canceladas y Ventas por Vendedor |
| Backups | Sí | No | No |

## Modelo de datos

La base de datos `SistemaVentas` está compuesta por las siguientes tablas:

- `Caja`
- `CajaUsuario`
- `Categoria`
- `Compra`
- `DetalleCompra`
- `DetalleVenta`
- `MovimientosCaja`
- `Producto`
- `Proveedor`
- `Rol`
- `Usuario`
- `Venta`

## Requisitos previos

- Windows 10 u 11.
- .NET 10 SDK.
- SQL Server (Express, Developer o superior).
- Visual Studio con soporte para .NET 10, JetBrains Rider o Visual Studio Code.
- Acceso a una instancia de SQL Server con autenticación de Windows o credenciales configuradas.

## Instalación y ejecución

1. Clonar el repositorio:

   ```bash
   git clone <URL_DEL_REPOSITORIO>
   ```

2. Crear la base de datos y el esquema ejecutando `database/scriptSistVentas2309.sql` en SQL Server.

   Si la base `SistemaVentas` ya existe, se puede utilizar `database/script.sql` para crear las tablas.

3. Aplicar las migraciones de `database/migrations/`:

   ```text
   migracion_compras.sql
   migracion_cuit_unico_proveedor.sql
   ```

4. Configurar la cadena de conexión en `SistemaVentas.GUI/App.config`:

   ```xml
   <connectionStrings>
     <add name="ConexionBD"
          connectionString="Server=.;Database=SistemaVentas;Integrated Security=True;TrustServerCertificate=True;"
          providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```

   Ajustar `Server` según la instancia local de SQL Server.

5. Restaurar paquetes y compilar:

   ```bash
   dotnet restore SistemaVentas.slnx
   dotnet build SistemaVentas.slnx
   ```

6. Ejecutar la aplicación:

   ```bash
   dotnet run --project SistemaVentas.GUI/SistemaVentas.GUI.csproj
   ```

   También se puede abrir `SistemaVentas.slnx` en Visual Studio, establecer `SistemaVentas.GUI` como proyecto de inicio y presionar `F5`.

## Datos iniciales

Los scripts incluidos crean la estructura de la base de datos, pero no cargan roles ni usuarios semilla. Para poder iniciar sesión es necesario:

- Registrar los roles con los IDs `1` (Gerente), `2` (Vendedor) y `3` (Supervisor).
- Crear al menos un usuario activo con una contraseña almacenada como hash BCrypt.

## Diseño de interfaz

La interfaz está construida con WPF y utiliza una paleta oscura denominada **Deep Twilight**:

- Azul marino profundo para fondos y paneles.
- Dorado para títulos, bordes y elementos de acción.
- Estilos globales centralizados en `App.xaml` para botones, inputs, tablas y tipografía.
- Uso de `StaticResource` y `DynamicResource` para mantener consistencia visual.

## Estado del proyecto

- No se incluyen proyectos de tests automatizados.
- La vista de administración de cajas (`CajaView`) aún no está integrada al menú principal.
- La gestión operativa de caja se realiza desde el módulo de Punto de Venta.
