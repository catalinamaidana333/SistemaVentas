/*
   Migración: CUIT único para proveedores
   Agrega una columna calculada que normaliza guiones y espacios y un índice
   único. No elimina ni modifica datos existentes.
*/
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.Proveedor', N'U') IS NULL
BEGIN
	;THROW 51000, N'No existe la tabla dbo.Proveedor.', 1;
END;
GO

IF EXISTS
(
	SELECT 1
	FROM dbo.Proveedor
	GROUP BY REPLACE(REPLACE(cuit, '-', ''), ' ', '')
	HAVING COUNT(*) > 1
)
BEGIN
	;THROW 51001, N'Hay proveedores con CUIT duplicado. Corrija esos registros antes de aplicar esta migración.', 1;
END;
GO

IF COL_LENGTH(N'dbo.Proveedor', N'cuit_normalizado') IS NULL
BEGIN
	ALTER TABLE dbo.Proveedor
	ADD cuit_normalizado AS (REPLACE(REPLACE(cuit, '-', ''), ' ', '')) PERSISTED;
END;
GO

IF NOT EXISTS
(
	SELECT 1
	FROM sys.indexes
	WHERE object_id = OBJECT_ID(N'dbo.Proveedor')
	  AND name = N'UX_Proveedor_cuit_normalizado'
)
BEGIN
	CREATE UNIQUE INDEX UX_Proveedor_cuit_normalizado
		ON dbo.Proveedor(cuit_normalizado);
END;
GO

PRINT N'Migración de CUIT único para proveedores aplicada correctamente.';
GO
