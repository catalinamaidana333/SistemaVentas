/* =============================================================================
   Migración: Módulo de Compras
   Base de datos : SistemaVentas
   Fecha         : 2026-10-21
   Objetivo      : Soporte de compras con factura, observaciones y origen del
                   efectivo, más índice de consulta por sesión de caja.

   IMPORTANTE: script idempotente. Puede ejecutarse varias veces sin error.
   -----------------------------------------------------------------------------
   1) dbo.Compra.numero_factura    varchar(50)  NULL  -> Nº de factura del proveedor
   2) dbo.Compra.observaciones      varchar(255) NULL  -> Notas de la compra
   3) dbo.Compra.origen_efectivo   varchar(20)  NULL  -> 'Caja' | 'Externa' | NULL
                                                               (NULL = Mercado Pago)
   4) IX_Compra_id_caja_usuario    índice sobre dbo.Compra(id_caja_usuario)

   Nota: dbo.Compra.id_caja_usuario ya es NULLable en el script base
   (scriptSistVentas2309.sql, línea 152), por lo que no se modifica aquí.
   ============================================================================= */

SET NOCOUNT ON;
GO

/* -----------------------------------------------------------------------------
   1-3) Columnas nuevas en dbo.Compra
   -------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Compra') AND name = 'numero_factura')
    ALTER TABLE dbo.Compra ADD numero_factura varchar(50) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Compra') AND name = 'observaciones')
    ALTER TABLE dbo.Compra ADD observaciones varchar(255) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Compra') AND name = 'origen_efectivo')
    ALTER TABLE dbo.Compra ADD origen_efectivo varchar(20) NULL;
GO

/* -----------------------------------------------------------------------------
   4) Índice por sesión de caja (compras en efectivo por caja)
   -------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Compra') AND name = 'IX_Compra_id_caja_usuario')
    CREATE INDEX IX_Compra_id_caja_usuario ON dbo.Compra(id_caja_usuario);
GO

/* -----------------------------------------------------------------------------
   Verificación
   -------------------------------------------------------------------------- */
SELECT c.name AS columna,
       TYPE_NAME(c.user_type_id) AS tipo,
       c.max_length AS longitud_bytes,
       c.is_nullable AS admite_nulls
FROM sys.columns c
WHERE c.object_id = OBJECT_ID('dbo.Compra')
  AND c.name IN ('id_caja_usuario', 'numero_factura', 'observaciones', 'origen_efectivo')
ORDER BY c.column_id;

SELECT i.name AS indice,
       i.type_desc AS tipo_indice
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('dbo.Compra')
  AND i.name = 'IX_Compra_id_caja_usuario';
GO

PRINT 'Migracion modulo de compras aplicada correctamente.';
GO
