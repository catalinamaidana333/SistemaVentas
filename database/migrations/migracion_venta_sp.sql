/* =============================================================================
   Migracion: Registro transaccional de ventas mediante procedimiento almacenado
   Base de datos : SistemaVentas
   Objetivo      : Centralizar en SQL Server el alta de la venta, su detalle y el
                   descuento de stock, garantizando atomicidad y concurrencia.

   Componentes:
   1) dbo.DetalleVentaTipo  -> tipo table-valued para enviar el carrito completo.
   2) dbo.usp_RegistrarVenta -> registra Venta + DetalleVenta y descuenta stock.

   IMPORTANTE: script idempotente. Puede ejecutarse varias veces sin error.
   El descuento de stock se hace con un UPDATE condicionado (stock_actual >=
   cantidad pedida), por lo que dos cajas concurrentes no pueden dejar el stock
   en negativo. Si el stock no alcanza se lanza el error 50001.
   ============================================================================= */

SET NOCOUNT ON;
GO

/* -----------------------------------------------------------------------------
   1) Tipo table-valued con el detalle del carrito
   -------------------------------------------------------------------------- */
IF TYPE_ID(N'dbo.DetalleVentaTipo') IS NULL
BEGIN
    CREATE TYPE dbo.DetalleVentaTipo AS TABLE
    (
        id_producto                INT           NOT NULL,
        cantidad                   INT           NOT NULL,
        precio_unitario_historico  DECIMAL(18,2) NOT NULL,
        subtotal                   DECIMAL(18,2) NOT NULL
    );
END;
GO

/* -----------------------------------------------------------------------------
   2) Procedimiento almacenado de registro de venta
   -------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_RegistrarVenta
    @id_caja_usuario INT,
    @metodo_pago     VARCHAR(50),
    @total           DECIMAL(18,2),
    @monto_abonado   DECIMAL(18,2),
    @vuelto          DECIMAL(18,2),
    @detalle         dbo.DetalleVentaTipo READONLY,
    @id_venta        INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @id_caja_usuario IS NULL OR @id_caja_usuario <= 0
        THROW 50002, N'La venta debe pertenecer a una sesion de caja valida.', 1;

    IF @metodo_pago IS NULL OR LTRIM(RTRIM(@metodo_pago)) = ''
        THROW 50003, N'El metodo de pago es obligatorio.', 1;

    IF NOT EXISTS (SELECT 1 FROM @detalle)
        THROW 50004, N'La venta debe contener al menos un producto.', 1;

    IF EXISTS (SELECT 1 FROM @detalle WHERE cantidad <= 0)
        THROW 50005, N'La cantidad de cada producto debe ser mayor que cero.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Venta
            (id_caja_usuario, fecha_hora, metodo_pago, total, monto_abonado, vuelto)
        VALUES
            (@id_caja_usuario, GETDATE(), @metodo_pago, @total, @monto_abonado, @vuelto);

        SET @id_venta = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO dbo.DetalleVenta
            (id_venta, id_producto, cantidad, precio_unitario_historico, subtotal)
        SELECT
            @id_venta,
            id_producto,
            cantidad,
            precio_unitario_historico,
            subtotal
        FROM @detalle;

        /* Descuento atomico y set-based. Se agrupa por producto para soportar
           lineas repetidas y se actualiza solo si hay stock suficiente. */
        DECLARE @filasActualizadas INT;

        ;WITH Pedido AS
        (
            SELECT id_producto, SUM(cantidad) AS cantidad
            FROM @detalle
            GROUP BY id_producto
        )
        UPDATE p
        SET p.stock_actual = p.stock_actual - pd.cantidad
        FROM dbo.Producto AS p
        INNER JOIN Pedido AS pd ON pd.id_producto = p.id_producto
        WHERE p.stock_actual >= pd.cantidad;

        SET @filasActualizadas = @@ROWCOUNT;

        DECLARE @productosSolicitados INT =
        (
            SELECT COUNT(DISTINCT id_producto)
            FROM @detalle
        );

        IF @filasActualizadas <> @productosSolicitados
        BEGIN
            DECLARE @productoFaltante NVARCHAR(250);

            SELECT TOP (1) @productoFaltante =
                CASE
                    WHEN p.id_producto IS NULL
                        THEN CONCAT(N'ID ', pd.id_producto, N' inexistente')
                    ELSE CONCAT(p.nombre,
                                N' (disponible: ', p.stock_actual,
                                N', solicitado: ', pd.cantidad, N')')
                END
            FROM
            (
                SELECT id_producto, SUM(cantidad) AS cantidad
                FROM @detalle
                GROUP BY id_producto
            ) AS pd
            LEFT JOIN dbo.Producto AS p ON p.id_producto = pd.id_producto
            WHERE p.id_producto IS NULL OR p.stock_actual < pd.cantidad
            ORDER BY pd.id_producto;

            IF @productoFaltante IS NULL
                SET @productoFaltante = N'desconocido';

            DECLARE @mensaje NVARCHAR(400) = CONCAT(
                N'Stock insuficiente para concretar la venta. ',
                N'Otro cajero pudo haber vendido las ultimas unidades. Producto: ',
                @productoFaltante,
                N'.');

            THROW 50001, @mensaje, 1;
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO

/* -----------------------------------------------------------------------------
   Verificacion
   -------------------------------------------------------------------------- */
SELECT
    t.name AS tipo,
    t.is_table_type AS es_table_type
FROM sys.types AS t
WHERE t.name = N'DetalleVentaTipo';

SELECT
    o.name AS procedimiento,
    o.type_desc AS tipo_objeto
FROM sys.objects AS o
WHERE o.object_id = OBJECT_ID(N'dbo.usp_RegistrarVenta');
GO

PRINT N'Migracion del procedimiento de ventas aplicada correctamente.';
GO

